using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniBench
{
    class BenchmarkTemplate
    {
        internal static string namespaceReplaceText = "##NAMESPACE-NAME##";
        internal static string classReplaceText = "##CLASS-NAME##";
        internal static string methodReplaceText = "##METHOD-NAME##";
        internal static string methodParametersReplaceText = "##METHOD-PARAMETERS##";
        internal static string generatedClassReplaceText = "##GENERAGED-CLASS-NAME##";

        internal static string benchmarkHarnessTemplate =
@"using MiniBench.Core;
using ##NAMESPACE-NAME##;
using System;
using System.Diagnostics;
using System.Collections.ObjectModel;

namespace MiniBench.Benchmarks
{
    public class ##GENERAGED-CLASS-NAME## : MarshalByRefObject, IBenchmarkTarget
    {
        private readonly string @namespace = ""##NAMESPACE-NAME##"";
        public string Namespace { get { return @namespace; } }

        private readonly string @type = ""##CLASS-NAME##"";
        public string Type { get { return @type; } }

        private readonly string @method = ""##METHOD-NAME##"";
        public string Method { get { return @method; } }

        private readonly ReadOnlyCollection<string> categories;
        public ReadOnlyCollection<string> Categories { get { return categories; } }

        public BenchmarkResult RunTest(TimeSpan warmupTime, TimeSpan targetTime)
        {
            try
            {
                Console.WriteLine(""Running benchmark: ##CLASS-NAME##.##METHOD-NAME##"");
                ##CLASS-NAME## benchmarkClass = GetBenchmarkClass();

                // Make sure the method is JIT-compiled.
                // TODO: Blackhole.Comsume(benchmarkClass.##METHOD-NAME##(##METHOD-PARAMETERS##));
                benchmarkClass.##METHOD-NAME##(##METHOD-PARAMETERS##);

                long ticks = (long)(Stopwatch.Frequency * warmupTime.TotalSeconds);
                GC.Collect();
                GC.WaitForPendingFinalizers();

                Stopwatch stopwatch = Stopwatch.StartNew();
                long warmupIterations = 0;
                while (stopwatch.ElapsedTicks < ticks)
                {
                    // TODO: Blackhole.Comsume(benchmarkClass.##METHOD-NAME##(##METHOD-PARAMETERS##));
                    benchmarkClass.##METHOD-NAME##(##METHOD-PARAMETERS##);
                    warmupIterations++;
                }
                stopwatch.Stop();
                Console.WriteLine(""Warmup {0:N0} iterations in {1}ms"", warmupIterations, (long)stopwatch.ElapsedMilliseconds);

                double ratio = targetTime.TotalSeconds / stopwatch.Elapsed.TotalSeconds;
                long iterations = (long)(warmupIterations * ratio);
                GC.Collect();
                GC.WaitForPendingFinalizers();

                stopwatch.Reset();
                stopwatch.Start();
                for (long iteration = 0; iteration < iterations; iteration++)
                {
                    // TODO: Blackhole.Comsume(benchmarkClass.##METHOD-NAME##(##METHOD-PARAMETERS##));
                    benchmarkClass.##METHOD-NAME##(##METHOD-PARAMETERS##);
                }
                stopwatch.Stop();

                Console.WriteLine(""Benchmark COMPLETE"");
                return BenchmarkResult.ForSuccess(this, iterations, stopwatch.Elapsed);
            }
            catch (Exception e)
            {
                // TODO: Stack trace?
                return BenchmarkResult.ForFailure(this, e.ToString());
            }
        }

        private ##CLASS-NAME## benchmarkClass = null;
        private ##CLASS-NAME## GetBenchmarkClass()
        {
            if (benchmarkClass == null)
                benchmarkClass = new ##CLASS-NAME##();
            return benchmarkClass;
        }
    }
}";

        internal static string launcherReplaceText = "##RUNNER-NAME##";
        internal static string benchmarkLauncherTemplate =
@"using System;
using MiniBench.Core;

namespace MiniBench.Benchmarks
{
    class Program
    {
        static void Main(string[] args)
        {
            // This is just temporary, will eventually spin-up an App-Domain and all that good stuff
            //BenchmarkResult result = ##RUNNER-NAME##;
            //Console.WriteLine(""Result: ""  + result);
        }
    }
}";
    }
}
