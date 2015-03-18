using System.Collections.Generic;

namespace MiniBench.Core.Profiling
{
    /// <summary>
    /// We have this because we DON'T want to use LINQ (we want to target .NET 2.0 and upwards)
    /// </summary>
    internal static class ListExtensions
    {
        internal static double Sum(List<double> list)
        {
            double sum = 0;
            for (int i = 0; i < list.Count; i++)
            {
                sum += list[i];
            }
            return sum;
        }

        internal static double Average(List<double> list)
        {
            double sum = 0;
            for (int i = 0; i < list.Count; i++)
            {
                sum += list[i];
            }
            return sum / list.Count;
        }

        internal static double Max(List<double> list)
        {
            double max = double.MinValue;
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i] > max)
                    max = list[i];
            }
            return max;
        }
    }
}