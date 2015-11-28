using System;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;


namespace Lib
{
    /// <summary>
    /// Реализация алгоритма билинейной интерполяции
    /// </summary>
    public class BilinearInterpolation
    {

        /// <summary>
        /// Изменение размера изображения
        /// </summary>
        /// <param name="image">Исходное изображение размер которого нужно изменить</param>
        /// <param name="width">Ширина результирующего изображения в пикселях</param>
        /// <param name="height">Высота результирующего изображения в пикселях</param>
        /// <returns>Исходное изображение нового размера</returns>
        public static Bitmap Resize(Bitmap image, int width, int height)
        {
            var fmt = image.PixelFormat;

            if (fmt != PixelFormat.Format24bppRgb && fmt != PixelFormat.Format32bppArgb &&
                fmt != PixelFormat.Format32bppRgb && fmt != PixelFormat.Format32bppPArgb)
            {
                throw new ArgumentException("Incorrect image format");
            }

            if (width <= 0 || height <= 0)
            {
                throw new ArgumentException("Incorect size");
            }

            var sourceData = image.LockBits(new Rectangle(0, 0, image.Width, image.Height),
                ImageLockMode.WriteOnly, image.PixelFormat);

            var bytesPerPixel = Image.GetPixelFormatSize(fmt) / 8;
            var resultData = new byte[bytesPerPixel * width * height];
            var widthInBytes = width * bytesPerPixel;

            unsafe
            {
                var ptrFirstPixel = (byte*)sourceData.Scan0;

                Parallel.For(0, height, y =>
                {
                    var tmp = (double)y / (height - 1) * (sourceData.Height - 1);

                    //Координата Y проецируемого пикселя исходного изображения
                    var coordY = ((int)tmp < 0) ? 0 : (((int)tmp >= sourceData.Height - 1) ? sourceData.Height - 2 : (int)tmp);

                    var diffY = tmp - coordY;

                    for (var x = 0; x < widthInBytes; x += bytesPerPixel)
                    {
                        tmp = (double)(x / bytesPerPixel) / (width - 1) * (sourceData.Width - 1);
                        var coordX = ((int)tmp < 0) ? 0 : (((int)tmp >= sourceData.Width - 1) ? sourceData.Width - 2 : (int)tmp);
                        var diffX = tmp - coordX;

                        //Коэффициенты, учитывающие погрешность вычислений
                        var с1 = (1 - diffX) * (1 - diffY);
                        var с2 = diffX * (1 - diffY);
                        var с3 = diffX * diffY;
                        var с4 = (1 - diffX) * diffY;

                        //Пиксели исходного изображения, которые учитываются для
                        //вычисления значения пикселя изображения нового размера
                        var p1 = ptrFirstPixel + coordY * sourceData.Stride + coordX * bytesPerPixel;
                        var p2 = ptrFirstPixel + coordY * sourceData.Stride + (coordX + 1) * bytesPerPixel;
                        var p3 = ptrFirstPixel + (coordY + 1) * sourceData.Stride + (coordX + 1) * bytesPerPixel;
                        var p4 = ptrFirstPixel + (coordY + 1) * sourceData.Stride + coordX * bytesPerPixel;

                        resultData[y * widthInBytes + x] = (byte)(*p1 * с1 + *p2 * с2 + *p3 * с3 + *p4 * с4); //B
                        resultData[y * widthInBytes + x + 1] = (byte)(*(p1 + 1) * с1 + *(p2 + 1) * с2 + *(p3 + 1) * с3 + *(p4 + 1) * с4); //G
                        resultData[y * widthInBytes + x + 2] = (byte)(*(p1 + 2) * с1 + *(p2 + 2) * с2 + *(p3 + 2) * с3 + *(p4 + 2) * с4); //R

                        if (fmt != PixelFormat.Format24bppRgb)
                        {
                            resultData[y * widthInBytes + x + 3] = 255; //A
                        }
                    }
                });
            }

            Bitmap newImage = new Bitmap(width, height, sourceData.PixelFormat);
            var newImageData = newImage.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, newImage.PixelFormat);
            Marshal.Copy(resultData, 0, newImageData.Scan0, resultData.Length);

            image.UnlockBits(sourceData);
            newImage.UnlockBits(newImageData);

            return newImage;
        }
    }
}
