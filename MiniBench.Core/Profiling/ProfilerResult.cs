namespace MiniBench.Core.Profiling
{
    public class ProfilerResult
    {
        public string Name { get; private set; }
        public double Value { get; private set; }
        public string Units { get; private set; }
        public AggregationMode AggregationMode { get; private set; }

        public ProfilerResult(string name, double value, string units, AggregationMode aggregationType)
        {
            Name = name;
            Value = value;
            Units = units;
            AggregationMode = aggregationType;
        }
    }
}