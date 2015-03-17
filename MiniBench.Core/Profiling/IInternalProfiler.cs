using System.Collections.Generic;

namespace MiniBench.Core.Profiling
{
    /// <summary>
    /// This interface is for information that can be obtained via or from the .NET runtime itself
    /// The callbacks run before and after each individual Benchmark iteration and return a result for that run
    /// All the individual results are then collacted (per Benchmark) and displayed at the end
    /// </summary>
    public interface IInternalProfiler
    {
        string SummaryText();

        void BeforeIteration();

        IList<ProfilerResult> AfterIteration();
    }
}
