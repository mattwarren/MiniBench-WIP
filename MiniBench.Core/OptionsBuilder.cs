using System;

namespace MiniBench.Core
{
    public class OptionsBuilder
    {
        private Type benchmarkType;

        public OptionsBuilder()
        {
        }

        public OptionsBuilder Include(Type benchmarkType)
        {
            this.benchmarkType = benchmarkType;
            return this;
        }

        public Options Build()
        {
            return new Options(benchmarkType);
        }
    }
}
