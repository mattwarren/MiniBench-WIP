namespace MiniBench
{
    class BenchmarkTemplate
    {
        private static string namespaceReplaceText = "##NAMESPACE-NAME##";
        private static string classReplaceText = "##CLASS-NAME##";
        private static string methodReplaceText = "##METHOD-NAME##";
        //private static string methodParametersReplaceText = "##METHOD-PARAMETERS##";
        private static string benchmarkMethodCallReplaceText = "##BENCHMARK-METHOD-CALL##";
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

        private readonly Blackhole blackhole = new Blackhole();

        public ##GENERAGED-CLASS-NAME##()
        {
            // TODO Eventually we need to get this from the Benchmark itself, for the time being just use a placeholder
            categories = new ReadOnlyCollection<string>(new String [] { ""Testing"" } );
        }

        public BenchmarkResult RunTest(TimeSpan warmupTime, TimeSpan targetTime)
        {
            try
            {
                Console.WriteLine(""Running benchmark: {0}.{1}"", @type, @method);
                ##CLASS-NAME## benchmarkClass = GetBenchmarkClass();

                // Make sure the method is JIT-compiled.
                ##BENCHMARK-METHOD-CALL##;

                long ticks = (long)(Stopwatch.Frequency * warmupTime.TotalSeconds);
                GC.Collect();
                GC.WaitForPendingFinalizers();

                //System.Diagnostics.Debugger.Launch();

                Stopwatch stopwatch = new Stopwatch();
                long warmupIterations = 0;
                stopwatch.Reset();
                stopwatch.Start();
                warmupIterations = 0;
                while (stopwatch.ElapsedTicks < ticks)
                {
                    ##BENCHMARK-METHOD-CALL##;
                    warmupIterations++;
                }
                stopwatch.Stop();
                Console.WriteLine(""Warmup:    {0,12:N0} iterations in {1,10:N3} ms, {2,6:N3} ns/op"", 
                                    warmupIterations, stopwatch.Elapsed.TotalMilliseconds, Utils.TicksToNanoseconds(stopwatch) / warmupIterations);

                double ratio = targetTime.TotalSeconds / stopwatch.Elapsed.TotalSeconds;
                long iterations = (long)(warmupIterations * ratio);
                GC.Collect();
                GC.WaitForPendingFinalizers();

                int batches = 4;
                for (int batch = 0; batch < batches; batch++)
                {
                    stopwatch.Reset();
                    stopwatch.Start();
                    for (long iteration = 0; iteration < iterations; iteration++)
                    {
                        ##BENCHMARK-METHOD-CALL##;
                    }
                    stopwatch.Stop();

                    Console.WriteLine(""Benchmark: {0,12:N0} iterations in {1,10:N3} ms, {2,6:N3} ns/op"", 
                                        iterations, stopwatch.Elapsed.TotalMilliseconds, Utils.TicksToNanoseconds(stopwatch) / iterations);
                }

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
@"//using System;
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

        internal static string ProcessCodeTemplates(string namespaceName, string className, string methodName, 
                                                    string generatedClassName, bool generateBlackhole = true)
        {
            // TODO at some point, we might need a less-hacky templating mechanism?!
            string benchmarkMethodCall;
            if (generateBlackhole)
                benchmarkMethodCall = string.Format("blackhole.Consume(benchmarkClass.{0}())", methodName);
            else
                benchmarkMethodCall = string.Format("benchmarkClass.{0}()", methodName);

            var generatedBenchmark = benchmarkHarnessTemplate
                                .Replace(namespaceReplaceText, namespaceName)
                                .Replace(classReplaceText, className)
                                .Replace(methodReplaceText, methodName)
                                //.Replace(methodParametersReplaceText, "")
                                .Replace(benchmarkMethodCallReplaceText, benchmarkMethodCall)
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
