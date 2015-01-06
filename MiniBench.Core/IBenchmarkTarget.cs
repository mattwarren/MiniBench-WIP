using System;
using System.Collections.ObjectModel;

namespace MiniBench.Core
{
    public interface IBenchmarkTarget
    {
        string Namespace { get; }

        string Type { get; }

        string Method { get; }

        ReadOnlyCollection<string> Categories { get; }

        BenchmarkResult RunTest(TimeSpan warmupTime, TimeSpan targetTime);
    }
}
