using System;
using System.Collections.Generic;

namespace MiniBench.Core.Profiling
{
    public class Profiler
    {
        private readonly Dictionary<IInternalProfiler, AggregatedProfilerResult []> profilers =
            new Dictionary<IInternalProfiler, AggregatedProfilerResult []>
            {
                { new GCProfiler(), null }
            };

        public void BeforeIteration()
        {
            foreach (KeyValuePair<IInternalProfiler, AggregatedProfilerResult []> profiler in profilers)
            {
                profiler.Key.BeforeIteration();
            }
        }

        public void AfterIteration()
        {
            try
            {
                IInternalProfiler [] keysCopy = new IInternalProfiler[profilers.Keys.Count];
                profilers.Keys.CopyTo(keysCopy, 0);
                foreach (IInternalProfiler profiler in keysCopy)
                {
                    IList<ProfilerResult> results = profiler.AfterIteration();
                    if (profilers[profiler] == null && results.Count > 0)
                    {
                        AggregatedProfilerResult[] aggregatedResult = new AggregatedProfilerResult[results.Count];
                        for (int i = 0; i < results.Count; i++)
                        {
                            aggregatedResult[i] = new AggregatedProfilerResult
                            (
                                results[i].Name,
                                results[i].Units,
                                results[i].AggregationMode
                            );
                            aggregatedResult[i].RawResults.Add(results[i].Value);
                        }
                        profilers[profiler] = aggregatedResult;
                    }
                    else
                    {
                        for (int i = 0; i < results.Count; i++)
                        {
                            profilers[profiler][i].RawResults.Add(results[i].Value);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // TODO where does the Exception bubble up to if we don't have a try-catch here??
                Console.WriteLine(ex.ToString());
                Console.WriteLine(ex.StackTrace);
            }
        }

        public void PrintIterationResults()
        {
            try
            {
                foreach (KeyValuePair<IInternalProfiler, AggregatedProfilerResult[]> profiler in profilers)
                {
                    for (int i = 0; i < profiler.Value.Length; i++)
                    {
                        AggregatedProfilerResult result = profiler.Value[i];
                        Console.WriteLine("Result {0,30} {1:N0} {2} ({3})",
                                          result.Name, result.RawResults[result.RawResults.Count - 1], result.Units, result.Mode);
                    }
                }
            }
            catch (Exception ex)
            {
                // TODO where does the Exception bubble up to if we don't have a try-catch here??
                Console.WriteLine(ex.ToString());
                Console.WriteLine(ex.StackTrace);
            }
        }

        public void PrintOverallResults()
        {
            try
            {
                foreach (KeyValuePair<IInternalProfiler, AggregatedProfilerResult[]> profiler in profilers)
                {
                    for (int i = 0; i < profiler.Value.Length; i++)
                    {
                        AggregatedProfilerResult result = profiler.Value[i];
                        Console.WriteLine("Aggregated Result {0,30} {1:N0} {2} ({3})",
                                          result.Name, result.AggregatedValue, result.Units, result.Mode);
                    }
                }
            }
            catch (Exception ex)
            {
                // TODO where does the Exception bubble up to if we don't have a try-catch here??
                Console.WriteLine(ex.ToString());
                Console.WriteLine(ex.StackTrace);
            }
        }
    }
}
