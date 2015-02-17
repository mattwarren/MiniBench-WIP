namespace MiniBench
{
    class BenchmarkTemplate
    {
        private static string namespaceReplaceText = "##NAMESPACE-NAME##";
        private static string classReplaceText = "##CLASS-NAME##";
        private static string methodReplaceText = "##METHOD-NAME##";
        private static string methodParametersReplaceText = "##METHOD-PARAMETERS##";
        private static string generatedClassReplaceText = "##GENERAGED-CLASS-NAME##";

        private static string benchmarkHarnessTemplate =
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
                Console.WriteLine(""Warmup:    {0,12:N0} iterations in {1,10:N2}ms"", warmupIterations, (long)stopwatch.ElapsedMilliseconds);

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
                Console.WriteLine(""Benchmark: {0,12:N0} iterations in {1,10:N2}ms"", iterations, (long)stopwatch.ElapsedMilliseconds);

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

        private static string benchmarkLauncherTemplate =
@"using System;
using MiniBench.Core;
//using ##NAMESPACE-NAME##;

namespace MiniBench.Benchmarks
{
    class Program
    {
        static void Main(string[] args)
        {
            // TODO put in here any attributes that control the benchmark parameters
            Options opt = new OptionsBuilder()
                    //.Include(typeof(##CLASS-NAME##))
                    .Include(typeof(##NAMESPACE-NAME##.##CLASS-NAME##))
                    .Build();
            new Runner(opt).Run();
        }
    }
}";

        internal static string ProcessCodeTemplates(string namespaceName, string className, string methodName, string generatedClassName)
        {
            // TODO at some point, we might need a less-hacky templating mechanism?!
            var generatedBenchmark = benchmarkHarnessTemplate
                                .Replace(namespaceReplaceText, namespaceName)
                                .Replace(classReplaceText, className)
                                .Replace(methodReplaceText, methodName)
                                .Replace(methodParametersReplaceText, "")
                                .Replace(generatedClassReplaceText, generatedClassName);
            return generatedBenchmark;
        }

        internal static string ProcessLauncherTemplate(string namespaceName, string className)
        {
            var benchmarkLauncher = benchmarkLauncherTemplate
                                .Replace(namespaceReplaceText, namespaceName)
                                .Replace(classReplaceText, className);
            return benchmarkLauncher;
        }
    }
}
