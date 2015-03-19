using System;

namespace MiniBench.Core
{
    public class Options
    {
        private readonly Type benchmarkType;
        public Type BenchmarkType { get { return benchmarkType; } }

        private readonly string benchmarkPrefix;
        public string BenchmarkPrefix { get { return benchmarkPrefix; } }

        private readonly string benchmarkRegex;
        public string BenchmarkRegex { get { return benchmarkRegex; } }

        public int WarmupRuns { get; private set; }
        public int Runs { get; private set; }

        private TimeSpan warmupTime = TimeSpan.FromSeconds(10);
        public TimeSpan WarmupTime { get { return warmupTime; } }

        private TimeSpan targetTime = TimeSpan.FromSeconds(10);
        public TimeSpan TargetTime { get { return warmupTime; } }

        private static readonly string GeneratedPrefix = "Generated_Runner";

        public Options(Type benchmarkType, int warmupRuns, int runs)
        {
            this.benchmarkType = benchmarkType;
            this.benchmarkPrefix = string.Format("{0}_{1}_{2}",
                                            GeneratedPrefix,
                                            benchmarkType.Namespace.Replace('.', '_'),
                                            benchmarkType.Name);
            WarmupRuns = warmupRuns;
            Runs = runs;
        }

        internal Options(String benchmarkRegex, int warmupRuns, int runs)
        {
            this.benchmarkRegex = benchmarkRegex;
            WarmupRuns = warmupRuns;
            Runs = runs;
        }
    }
}