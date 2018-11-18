using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLib
{
    public class Distribute
    {
        public static double GetSigma(List<double> dl)
        {
            if (dl.Count == 0) return 0;
            var Average = dl.Average();
            double sd = 0;
            foreach (var d in dl)
            {
                sd += (d - Average) * (d - Average);
            }
            sd /= dl.Count;
            return Math.Sqrt(sd);
        }

        public static double GetDistance(double value, List<double> dl)
        {
            if (dl.Count == 0) return 0;
            var sigma = GetSigma(dl);
            if (sigma == 0) return 0;
            return (value - dl.Average()) / sigma;
        }

        public static List<DataPoint> GetDistribution(List<double> dataList, int countNumber)
        {
            var sigma = GetSigma(dataList);
            if (dataList == null || dataList.Count == 0) return null;
            if (sigma == 0) return null;
            List<StatisticsArea> dl = new List<StatisticsArea>();

            var min = dataList.Min();
            var max = dataList.Max();
            min = Math.Min(min, dataList.Average() - (3.5 * sigma));
            //if (min < 0) min = 0;
            max = Math.Max(max, dataList.Average() + (3.5 * sigma));

            var step = (1d / Convert.ToDouble(countNumber)) * sigma;
            for (var x = min; x <= max + step; x = x + step)
            {
                var statistic = new StatisticsArea()
                {
                    MinBorder = x,
                    Count = 0
                };
                if (dl.Count > 0)
                    dl[dl.Count - 1].MaxBorder = x;
                dl.Add(statistic);
            }

            foreach (var data in dataList)
            {
                var statistic =
                    dl.FirstOrDefault(v => v.MinBorder < data && v.MaxBorder >= data);
                if (statistic != null)
                    statistic.Count++;
            }
            var cl = new List<DataPoint>();
            var sum = dl.Sum(v => v.Count);
            dl.ForEach(v =>
            {
                cl.Add(new DataPoint()
                {
                    XValue = (v.MinBorder + v.MaxBorder) / 2 / sigma,
                    YValue = v.Count / Convert.ToDouble(sum) * 5
                });
            });
            return cl.OrderBy(v => v.XValue).ToList();
        }

        public static List<DataPoint> GetNormalDistribute(double u=0, double s=1,double sigmaArea=3.5,double step=0.1)
        {
            List<DataPoint> dl = new List<DataPoint>();
            for (var x = u - (sigmaArea * s); x < u + (sigmaArea * s); x = x + step * s)
            {
                dl.Add(new DataPoint()
                {
                    XValue = x,
                    YValue = Math.Pow(Math.E, (-((x - u) * (x - u)) / 2 / s / s)) / Math.Sqrt(2 * Math.PI) / s
                });
            }
            
            return dl;
        }

    }

    public class DataPoint
    {
        public double XValue { get; set; }
        public double YValue { get; set; }
    }
    public class StatisticsArea
    {
        public double MinBorder { get; set; }
        public double MaxBorder { get; set; }
        public int Count { get; set; }
    }
}
