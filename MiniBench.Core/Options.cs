using System;

namespace MiniBench.Core
{
    public class Options
    {
        //Feature 'automatically implemented properties' is not available in C# 2.  Please use language version 3 or greater.
        private readonly Type benchmarkType;
        public Type BenchmarkType { get { return benchmarkType; } }

        private readonly string benchmarkPrefix;
        public string BenchmarkPrefix { get { return benchmarkPrefix; } }

        private static readonly string GeneratedPrefix = "Generated_Runner";

        public Options(Type benchmarkType)
        {
            this.benchmarkType = benchmarkType;
            this.benchmarkPrefix = string.Format("{0}_{1}_{2}",
                                            GeneratedPrefix,
                                            benchmarkType.Namespace.Replace('.', '_'),
                                            benchmarkType.Name);
        }
    }
}