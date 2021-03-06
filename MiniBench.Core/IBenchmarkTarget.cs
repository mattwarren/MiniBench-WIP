﻿using System.Collections.ObjectModel;
using MiniBench.Core.Profiling;

namespace MiniBench.Core
{
    public interface IBenchmarkTarget
    {
        string Namespace { get; }

        string Type { get; }

        string Method { get; }

        ReadOnlyCollection<string> Categories { get; }

        BenchmarkResult RunTest(Options options, Profiler profiler);
    }
}
