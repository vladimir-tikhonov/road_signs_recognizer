using System.Collections.Generic;

namespace Classifier
{
    public class PrebuildSignsClassifier
    {
        readonly List<Sign> _data = new List<Sign>();
        Classifier _classifier;

        public List<Sign> Teach()
        {
            _data.Add(new Sign(50, 15, 30, 1, 5));
            _data.Add(new Sign(50, 20, 40, 2, 3));
            _data.Add(new Sign(50, 13, 20, 3, 5));
            _data.Add(new Sign(50, 23, 25, 9, 1));

            _data.Add(new Sign(100, 70, 15, 1, 1));
            _data.Add(new Sign(100, 20, 70, 1, 1));
            _data.Add(new Sign(100, 20, 50, 1, 1));
            _data.Add(new Sign(100, 30, 40, 1, 1));
            _data.Add(new Sign(100, 40, 50, 1, 1));
            _data.Add(new Sign(100, 70, 1, 1, 20));
            _data.Add(new Sign(100, 60, 1, 1, 30));
            _data.Add(new Sign(100, 60, 10, 1, 20));

            _data.Add(new Sign(100, 1, 30, 1, 60));
            _data.Add(new Sign(100, 1, 40, 1, 50));
            _data.Add(new Sign(100, 1, 20, 1, 60));
            _data.Add(new Sign(100, 1, 27, 1, 57));

            _data.Add(new Sign(150, 1, 29, 1, 60));
            _data.Add(new Sign(150, 20, 1, 1, 60));
            _data.Add(new Sign(150, 1, 40, 1, 50));
            _data.Add(new Sign(150, 70, 20, 1, 1));
            _data.Add(new Sign(150, 5, 5, 80, 1));

            _data.Add(new Sign(50, 20, 1, 400, 1));
            _data.Add(new Sign(50, 21, 1, 450, 1));
            _data.Add(new Sign(100, 40, 1, 500, 1));
            _data.Add(new Sign(100, 38, 1, 450, 1));
            _data.Add(new Sign(100, 1, 2, 700, 1));

            _classifier = new Classifier(0.004, 5, _data);
            return _classifier.TeachClassifier();
        }

        public int FindClass(Sign sign)
        {
            return _classifier.ClassifyElement(sign);
        }
    }
}
