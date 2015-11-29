using System;
using System.Collections.Generic;

namespace Lib
{
    // http://habrahabr.ru/post/198268/
    public class Perceptron
    {
        private const int InputSize = 300;
        private const int HiddenLayerSize = InputSize;
        private const int OutputLayerSize = 5;
        private const double ErrorTreshold = 0.05;
        private const double LearningSpeed = 0.1;

        // Веса связей между входным и скрытым слоем
        private double[][] _v;
        // Веса связей между скрытым и выходным слоем
        private double[][] _w;
        // Входной вектор
        private int[] _x;
        // Смещение для нейронов скрытого слоя
        private double[] _v0;
        // Входной сигнал нейронов скрытого слоя
        private double[] _zIn;
        // Входной сигнал нейронов скрытого слоя после функции активации
        private double[] _z;
        // Смещение для нейронов выходного слоя слоя
        private double[] _w0;
        // Входной сигнал нейронов выходного слоя
        private double[] _yIn;
        // Входной сигнал нейронов выходного слоя после функции активации
        private double[] _y;

        public void Reset()
        {
            _v = new double[InputSize][];
            _w = new double[HiddenLayerSize][];

            _x = new int[InputSize];
            _v0 = new double[HiddenLayerSize];
            _z = new double[HiddenLayerSize];
            _zIn = new double[HiddenLayerSize];

            _w0 = new double[OutputLayerSize];
            _y = new double[OutputLayerSize];
            _yIn = new double[OutputLayerSize];

            var random = new Random();
            var beta = 0.7*Math.Pow(HiddenLayerSize, 1.0/InputSize);

            // Вычисление начального смещения нейронов скрытого и выходного слоёв
            for (var i = 0; i < _v0.Length; i++)
            {
                _v0[i] = random.NextDouble()*2*beta - beta;
            }

            for (var i = 0; i < _w0.Length; i++)
            {
                _w0[i] = random.NextDouble() * 2 * beta - beta;
            }
            // ---------------------------------------------------

            // Вычисление и корректировка нального значение весов связей между входным и скрытым слоем
            for (var i = 0; i < _v.Length; i++)
            {
                _v[i] = new double[HiddenLayerSize];
            }

            foreach (var row in _v)
            {
                for (var j = 0; j < row.Length; j++)
                {
                    row[j] = random.NextDouble() * 0.5 - 0.5;
                }
            }
            var normesV = new double[HiddenLayerSize];
            for (var j = 0; j < HiddenLayerSize; j++)
            {
                var sum = 0.0;
                for (var i = 0; i < InputSize; i++)
                {
                    sum += _v[i][j] * _v[i][j];
                }
                normesV[j] = Math.Sqrt(sum);
            }

            for (var i = 0; i < InputSize; i++)
            {
                for (var j = 0; j < HiddenLayerSize; j++)
                {
                    _v[i][j] = beta * _v[i][j] / normesV[j];
                }
            }
            // ---------------------------------------------------
            // Вычисление и корректировка нального значение весов связей между скрытым и выходным слоем
            for (var i = 0; i < _w.Length; i++)
            {
                _w[i] = new double[OutputLayerSize];
            }

            foreach (var row in _w)
            {
                for (var j = 0; j < row.Length; j++)
                {
                    row[j] = random.NextDouble() * 0.5 - 0.5;
                }
            }

            var normesW = new double[OutputLayerSize];
            for (var j = 0; j < OutputLayerSize; j++)
            {
                var sum = 0.0;
                for (var i = 0; i < HiddenLayerSize; i++)
                {
                    sum += _w[i][j] * _w[i][j];
                }
                normesW[j] = Math.Sqrt(sum);
            }

            for (var i = 0; i < HiddenLayerSize; i++)
            {
                for (var j = 0; j < OutputLayerSize; j++)
                {
                    _w[i][j] = beta * _w[i][j] / normesW[j];
                }
            }
            // ---------------------------------------------------
        }

        public double[] Classify(int[] input)
        {
            _x = input;
            // Шаг 4: обработка входных сигналов на скрытом слое
            for (var i = 0; i < _zIn.Length; i++)
            {
                _zIn[i] = _v0[i];
            }
            for (var i = 0; i < _x.Length; i++)
            {
                for (var j = 0; j < HiddenLayerSize; j++)
                {
                    _zIn[j] += _x[i] * _v[i][j];
                }
            }
            for (var i = 0; i < _zIn.Length; i++)
            {
                _z[i] = CalcActivation(_zIn[i]);
            }
            // ---------------------------------------------------
            // Шаг 5: обработка входных сигналов на выходном слое
            for (var i = 0; i < _yIn.Length; i++)
            {
                _yIn[i] = _w0[i];
            }

            for (var j = 0; j < _z.Length; j++)
            {
                for (var k = 0; k < OutputLayerSize; k++)
                {
                    _yIn[k] += _z[j] * _w[j][k];
                }
            }

            for (var i = 0; i < _y.Length; i++)
            {
                _y[i] = CalcActivation(_yIn[i]);
            }
            // ---------------------------------------------------

            return _y;
        }

        public void Teach(Dictionary<int[], double[]> samples)
        {
            double maxError;
            do
            {
                maxError = 0;
                foreach (var sample in samples)
                {
                    Classify(sample.Key);
                    var sigmaOutput = new double[OutputLayerSize];

                    // Шаг 6: высление ошибки на выходном слое и корректировка смещения выходного слоя
                    for (var i = 0; i < OutputLayerSize; i++)
                    {
                        sigmaOutput[i] = (sample.Value[i] - _y[i]) * CalcActivationDerivative(_yIn[i]);
                    }
                    for (var i = 0; i < _w0.Length; i++)
                    {
                        _w0[i] += LearningSpeed * sigmaOutput[i];
                    }
                    // ---------------------------------------------------

                    for (var i = 0; i < _y.Length; i++)
                    {
                        maxError = Math.Max(maxError, Math.Abs(sample.Value[i] - _y[i]));
                    }

                    // Шаг 7: высление ошибки на скрытом слое и корректировка смещения скрытого слоя
                    var sigmaHiddenIn = new double[HiddenLayerSize];
                    var sigmaHidden = new double[HiddenLayerSize];
                    for (var j = 0; j < sigmaHiddenIn.Length; j++)
                    {
                        for (var k = 0; k < sigmaOutput.Length; k++)
                        {
                            sigmaHiddenIn[j] += sigmaOutput[k] * _w[j][k];
                        }
                    }

                    for (var i = 0; i < sigmaHiddenIn.Length; i++)
                    {
                        sigmaHidden[i] = sigmaHiddenIn[i] * CalcActivationDerivative(_zIn[i]);
                    }

                    for (var i = 0; i < _v0.Length; i++)
                    {
                        _v0[i] += LearningSpeed * sigmaHidden[i];
                    }
                    // ---------------------------------------------------

                    // Шаг 8: корректировка весов скрытого и выходного слоя
                    for (var j = 0; j < _w.Length; j++)
                    {
                        for (var k = 0; k < OutputLayerSize; k++)
                        {
                            _w[j][k] += LearningSpeed * sigmaOutput[k] * _z[j];
                        }
                    }

                    for (var i = 0; i < InputSize; i++)
                    {
                        for (var j = 0; j < HiddenLayerSize; j++)
                        {
                            _v[i][j] += LearningSpeed * sigmaHidden[j] * _x[i];
                        }
                    }
                    // ---------------------------------------------------
                }
            } while (maxError > ErrorTreshold);
        }

        private double CalcActivation(double x)
        {
            return 1 / (1 + Math.Exp(-x));
        }

        private double CalcActivationDerivative(double x)
        {
            return CalcActivation(x) * (1 - CalcActivation(x));
        }
    }
}
