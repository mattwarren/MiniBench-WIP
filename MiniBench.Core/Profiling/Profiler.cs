using System;
using System.Collections.Generic;

namespace MiniBench.Core.Profiling
{
    public class Profiler
    {
        internal readonly Dictionary<IInternalProfiler, AggregatedProfilerResult []> Profilers =
            new Dictionary<IInternalProfiler, AggregatedProfilerResult []>
            {
                { new GCProfiler(), null }
            };

        public void BeforeIteration()
        {
            foreach (KeyValuePair<IInternalProfiler, AggregatedProfilerResult []> profiler in Profilers)
            {
                profiler.Key.BeforeIteration();
            }
        }

        public void AfterIteration()
        {
            try
            {

                IInternalProfiler [] keysCopy = new IInternalProfiler[Profilers.Keys.Count];
                Profilers.Keys.CopyTo(keysCopy, 0);
                foreach (IInternalProfiler profiler in keysCopy)
                {
                    IList<ProfilerResult> results = profiler.AfterIteration();
                    if (Profilers[profiler] == null && results.Count > 0)
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
                        Profilers[profiler] = aggregatedResult;
                    }
                    else
                    {
                        for (int i = 0; i < results.Count; i++)
                        {
                            Profilers[profiler][i].RawResults.Add(results[i].Value);
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
                foreach (KeyValuePair<IInternalProfiler, AggregatedProfilerResult[]> profiler in Profilers)
                {
                    Array.ForEach(profiler.Value, result =>
                        {
                            Console.WriteLine("Result {0,36}: {1:N0} {2} ({3})", result.Name,
                                              result.RawResults[result.RawResults.Count - 1], result.Units, result.Mode);

                        });
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
                foreach (KeyValuePair<IInternalProfiler, AggregatedProfilerResult[]> profiler in Profilers)
                {
                    Array.ForEach(profiler.Value, result =>
                        {
                            Console.WriteLine("Aggregated Result {0,25}: {1:N0} {2} ({3})",
                                              result.Name, result.AggregatedValue, result.Units, result.Mode);
                        });
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
