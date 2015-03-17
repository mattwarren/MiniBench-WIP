using System;
using System.Collections.Generic;

namespace MiniBench.Core.Profiling
{
    internal class AggregatedProfilerResult
    {
        internal string Name { get; private set; }
        
        internal string Units { get; private set; }
        internal AggregationMode Mode { get; private set; }
        internal List<double> RawResults { get; private set; }

        public AggregatedProfilerResult(string name, string units, AggregationMode mode)
        {
            Name = name;
            Units = units;
            Mode = mode;
            RawResults = new List<double>();
        }

        internal double AggregatedValue
        {
            get
            {
                switch (Mode)
                {
                    case AggregationMode.Sum:
                        return ListExtensions.Sum(RawResults);
                    case AggregationMode.Average:
                        return ListExtensions.Average(RawResults);
                    case AggregationMode.Max:
                        return ListExtensions.Max(RawResults);
                    default:
                        throw new InvalidOperationException("Unexpected AggregationMode: " + Mode);
                }
            }
        }
    }
}