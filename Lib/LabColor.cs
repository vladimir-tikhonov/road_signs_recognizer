using System;
using System.Windows.Media;

namespace Lib
{
    public class LabColor
    {
        public LabColor(XyzColor xyzSource)
        {
            BuildFromXyz(xyzSource.X, xyzSource.Y, xyzSource.Z);
        }

        public double L { get; private set; }
        public double A { get; private set; }
        public double B { get; private set; }

        public static LabColor FromRgb(byte r, byte g, byte b)
        {
            return new LabColor(new XyzColor(Color.FromRgb(r, g, b)));
        }

        public override string ToString()
        {
            return $"L: {L}, A: {A}, B: {B}";
        }

        private void BuildFromXyz(double x, double y, double z)
        {
            L = 116.0*Fxyz(y/1.0) - 16;
            A = 500.0*(Fxyz(x/0.9505) - Fxyz(y/1.0));
            B = 200.0*(Fxyz(y/1.0) - Fxyz(z/1.0890));
        }

        private static double Fxyz(double t)
        {
            return ((t > 0.008856) ? Math.Pow(t, (1.0 / 3.0)) : (7.787 * t + 16.0 / 116.0));
        }
    }
}
