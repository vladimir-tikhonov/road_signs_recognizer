using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Classifier
{
    public class Cluster
    {
        public Cluster(Sign center, int number)
        {
            Center = center;
            ElementList = new List<Sign>();
            Number = number;
        }

        public int Number { get; set; }
        public Sign Center { get; set; }
        public List<Sign> ElementList { get; set; }

        public double GetDistanceToCenter(Sign element)
        {
            return Sign.GetDistance(Center, element);
        }

        public Sign FindCenterElement()
        {
            Sign centerElement = new Sign(0, 0, 0, 0, 0);
            foreach(Sign elem in ElementList)
            {
                centerElement.SignForm += elem.SignForm;
                centerElement.RedColor += elem.RedColor;
                centerElement.WhiteColor += elem.WhiteColor;
                centerElement.YellowColor += elem.YellowColor;
                centerElement.BlueColor += elem.BlueColor;
            }
            if (ElementList.Count > 0)
            {
                centerElement.SignForm /= ElementList.Count;
                centerElement.RedColor /= ElementList.Count;
                centerElement.WhiteColor /= ElementList.Count;
                centerElement.YellowColor /= ElementList.Count;
                centerElement.BlueColor /= ElementList.Count;
            } else {
                centerElement = Center;
            }
            return centerElement;
        }
    }
}
