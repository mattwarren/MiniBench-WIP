using System;
using System.Collections.Generic;

namespace MiniBench.Core.Profiling
{
    public class GCProfiler : IInternalProfiler
    {
        private int beforeGen0, beforeGen1, beforeGen2;
        private long memoryBefore;

        public string SummaryText()
        {
            return
                "GCProfiler - Calculates the GC Collection Counts for Generations 0, 1 and 2. " + 
                "Also calculates the memory usage (per iteration) and the peak during the entire run";
        }

        public void BeforeIteration()
        {
            beforeGen0 = GC.CollectionCount(0);
            beforeGen1 = GC.CollectionCount(1);
            beforeGen2 = GC.CollectionCount(2);
            memoryBefore = GC.GetTotalMemory(forceFullCollection: false);
        }

        public IList<ProfilerResult> AfterIteration()
        {
            var gen0 = GC.CollectionCount(0) - beforeGen0;
            var gen1 = GC.CollectionCount(1) - beforeGen1;
            var gen2 = GC.CollectionCount(2) - beforeGen2;
            var memoryAfter = GC.GetTotalMemory(forceFullCollection: false);

            return new []
                {
                    new ProfilerResult("GC.Gen0", gen0, "counts", AggregationMode.Sum),
                    new ProfilerResult("GC.Gen1", gen1, "counts", AggregationMode.Sum),
                    new ProfilerResult("GC.Gen2", gen2, "counts", AggregationMode.Sum),
                    new ProfilerResult("Memory.Usage", memoryAfter - memoryBefore, "bytes", AggregationMode.Max),
                    new ProfilerResult("Memory.Peak", memoryAfter, "bytes", AggregationMode.Max),
                };
        }
    }
}
