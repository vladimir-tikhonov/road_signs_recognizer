using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Classifier
{
    public class Sign
    {
        public Sign(int signForm, double redColor, double whiteColor, double yellowColor, double blueColor, int clusterNumber = 0)
        {
            SignForm = signForm;
            RedColor = redColor;
            WhiteColor = whiteColor;
            YellowColor = yellowColor;
            BlueColor = blueColor;
            ClusterNumber = clusterNumber;
        }

        public int SignForm { get; set; }
        public double RedColor { get; set; }
        public double WhiteColor { get; set; }
        public double YellowColor { get; set; }
        public double BlueColor { get; set; }
        public int ClusterNumber { get; set; }

        public static double GetDistance(Sign el1, Sign el2)
        {
            int sf2 = (el2.SignForm - el1.SignForm) * (el2.SignForm - el1.SignForm);
            double rc2 = Math.Pow(el2.RedColor - el1.RedColor, 2);
            double bc2 = Math.Pow(el2.BlueColor - el1.BlueColor, 2);
            double whc2 = Math.Pow(el2.WhiteColor - el1.WhiteColor, 2);
            double yc2 = Math.Pow(el2.YellowColor - el1.YellowColor, 2);

            return Math.Sqrt(sf2 + rc2 + bc2 + whc2 + yc2);
        }
    }
}
