using System;

namespace MiniBench.Core
{
    public class OptionsBuilder
    {
        private Type benchmarkType;
        private String benchmarkRegex;

        private bool useType = false;

        public OptionsBuilder()
        {
        }

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

        public Options Build()
        {
            if (useType)
                return new Options(benchmarkType);

            return new Options(benchmarkRegex);
        }
    }
}
