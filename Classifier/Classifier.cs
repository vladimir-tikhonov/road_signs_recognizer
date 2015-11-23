using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Classifier
{
    public class Classifier
    {
        public int NumberOfClusters { get; set; }
        public List<Sign> ElementList { get;set; }
        public List<Cluster> ClusterList { get; set; }
        public double Accuracy { get; set; }

        public Classifier(double accuracy, int n, List<Sign> elements)
        {
            ElementList = elements;
            NumberOfClusters = n;
            Accuracy = accuracy;
            ClusterList = new List<Cluster>();
            SetInitialClusterCenters();
        }

        public void Partition()
        {
            foreach(Sign sign in ElementList)
            {
                double min = double.MaxValue;
                Cluster cluster = ClusterList[0];
                foreach (Cluster s in ClusterList)
                {
                    double dist = s.GetDistanceToCenter(sign);
                    if (dist <= min)
                    {
                        min = dist;
                        cluster = s;
                    }
                }
                if (sign.ClusterNumber != 0)
                {
                    Cluster oldCluster = this.ClusterList.Find(
                       delegate(Cluster cl)
                       {
                           return cl.Number == sign.ClusterNumber;
                       }
                    );
                    oldCluster.ElementList.Remove(sign);
                }
                sign.ClusterNumber = cluster.Number;
                cluster.ElementList.Add(sign);
            }
        }

        public List<Sign> TeachClassifier()
        {
            bool needToRepeat = true;
            int count = 0;
            while (needToRepeat)
            {
                Partition();
                foreach (Cluster cluster in ClusterList)
                {
                    Sign newCenterElement = cluster.FindCenterElement();
                    double dist = Sign.GetDistance(cluster.Center, newCenterElement);
                    if (dist >= this.Accuracy)
                    {
                        newCenterElement.ClusterNumber = cluster.Number;
                        cluster.Center = newCenterElement;
                        count++;
                    }
                }
                if (count == 0)
                {
                    needToRepeat = false;
                }
                count = 0;
            }
            List<Sign> result = new List<Sign>();
            foreach (Cluster cl in this.ClusterList)
            {
                result.Add(cl.Center);
            }
            return result;
        }

        public int ClassifyElement(Sign sign)
        {
            double min = double.MaxValue;
            Cluster cluster = ClusterList[0];
            foreach (Cluster s in ClusterList)
            {
                double dist = s.GetDistanceToCenter(sign);
                if (dist <= min)
                {
                    min = dist;
                    cluster = s;
                }
            }
            return cluster.Number;
        }

        public void SetInitialClusterCenters()
        {
            //for (int i = 0; i < NumberOfClusters; i++)
            //{
            //    Random rnd = new Random();
            //    int num = rnd.Next(0, ElementList.Count - 1);
            //    ClusterList.Add(new Cluster(ElementList[num]), i+1);
            //}
            ClusterList.Add(new Cluster(ElementList[0], 1));
            ClusterList.Add(new Cluster(ElementList[8], 2));
            ClusterList.Add(new Cluster(ElementList[15], 3));
            ClusterList.Add(new Cluster(ElementList[17], 4));
            ClusterList.Add(new Cluster(ElementList[23], 5));
        }
    }
}
