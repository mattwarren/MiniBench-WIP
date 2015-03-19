using System;

namespace MiniBench.Core
{
    public class OptionsBuilder
    {
        private Type benchmarkType;
        private String benchmarkRegex;

        private bool useType = false;
        private int runs;
        private int warmupRuns;

        public OptionsBuilder Include(Type benchmarkType)
        {
            this.benchmarkType = benchmarkType;
            useType = true;
            return this;
        }

        public OptionsBuilder Include(String benchmarkRegex)
        {
            this.benchmarkRegex = benchmarkRegex;
            useType = false;
            return this;
        }

        public OptionsBuilder WarmupRuns(int warmupRuns)
        {
            this.warmupRuns = warmupRuns;
            return this;
        }

        public OptionsBuilder Runs(int runs)
        {
            this.runs = runs;
            return this;
        }

        public Options Build()
        {
            if (useType)
                return new Options(benchmarkType, warmupRuns, runs);

            return new Options(benchmarkRegex, warmupRuns, runs);
        }
    }
}
