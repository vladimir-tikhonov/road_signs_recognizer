using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Classifier
{
    public class TryClassifier
    {
        List<Sign> data = new List<Sign>();
        Classifier classifier;

        public List<Sign> Main()
        {
            data.Add(new Sign(10, 15, 30, 1, 5));
            data.Add(new Sign(10, 20, 40, 2, 3));
            data.Add(new Sign(10, 13, 20, 3, 5));
            data.Add(new Sign(10, 23, 25, 9, 1));

            data.Add(new Sign(20, 70, 15, 1, 1));
            data.Add(new Sign(20, 20, 70, 1, 1));
            data.Add(new Sign(20, 20, 50, 1, 1));
            data.Add(new Sign(20, 30, 40, 1, 1));
            data.Add(new Sign(20, 40, 50, 1, 1));
            data.Add(new Sign(20, 70, 1, 1, 20));
            data.Add(new Sign(20, 60, 1, 1, 30));
            data.Add(new Sign(20, 60, 10, 1, 20));

            data.Add(new Sign(20, 1, 30, 1, 60));
            data.Add(new Sign(20, 1, 40, 1, 50));
            data.Add(new Sign(20, 1, 20, 1, 60));
            data.Add(new Sign(20, 1, 27, 1, 57));

            data.Add(new Sign(30, 1, 29, 1, 60));
            data.Add(new Sign(30, 20, 1, 1, 60));
            data.Add(new Sign(30, 1, 40, 1, 50));
            data.Add(new Sign(30, 70, 20, 1, 1));
            data.Add(new Sign(30, 5, 5, 80, 1));

            data.Add(new Sign(10, 20, 1, 40, 1));
            data.Add(new Sign(10, 21, 1, 45, 1));
            data.Add(new Sign(20, 40, 1, 50, 1));
            data.Add(new Sign(20, 38, 1, 45, 1));
            data.Add(new Sign(20, 1, 2, 70, 1));

            this.classifier = new Classifier(0.004, 5, data);
            return classifier.TeachClassifier();
        }

        public int FindClass(Sign sign)
        {
            return classifier.ClassifyElement(sign);
        }
    }
}
