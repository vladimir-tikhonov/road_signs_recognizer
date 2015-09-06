using System;
using System.Windows.Media;

namespace Lib
{
    public class XyzColor
    {
        public XyzColor(Color rgbSource)
        {
            BuildFromRgb(rgbSource.R, rgbSource.G, rgbSource.B);
        }

        public double X { get; private set; }
        public double Y { get; private set; }
        public double Z { get; private set; }

        private void BuildFromRgb(int red, int green, int blue)
        {
            var rLinear = red / 255.0;
            var gLinear = green / 255.0;
            var bLinear = blue / 255.0;

            var r = (rLinear > 0.04045) ? Math.Pow((rLinear + 0.055) / (
                1 + 0.055), 2.2) : (rLinear / 12.92);
            var g = (gLinear > 0.04045) ? Math.Pow((gLinear + 0.055) / (
                1 + 0.055), 2.2) : (gLinear / 12.92);
            var b = (bLinear > 0.04045) ? Math.Pow((bLinear + 0.055) / (
                1 + 0.055), 2.2) : (bLinear / 12.92);

            X = r*0.4124 + g*0.3576 + b*0.1805;
            Y = r*0.2126 + g*0.7152 + b*0.0722;
            Z = r*0.0193 + g*0.1192 + b*0.9505;
        }
    }
}
