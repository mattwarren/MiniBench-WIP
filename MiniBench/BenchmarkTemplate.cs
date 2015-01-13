
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

        private static string benchmarkLauncherTemplate =
@"using System;
using MiniBench.Core;
using System.Reflection;

namespace MiniBench.Benchmarks
{
    class Program
    {
        static void Main(string[] args)
        {
            // TODO spin-up a new App-Domain and execute the benchmark in that
            Assembly assembly = Assembly.GetExecutingAssembly();
            foreach (Type type in assembly.GetTypes())
            {
                if (!type.IsClass || !type.IsPublic || type.IsAbstract ||
                    typeof(IBenchmarkTarget).IsAssignableFrom(type) == false)
                {
                    continue;
                }

                Console.WriteLine(""Found: "" + type.Name);
                IBenchmarkTarget obj = assembly.CreateInstance(type.FullName) as IBenchmarkTarget;
                if (obj == null)
                {
                    Console.WriteLine(""Unable to create type: "" + type.Name);
                    continue;
                }
                BenchmarkResult result = obj.RunTest(TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(10));
                Console.WriteLine(result + ""\n"");
            }
        }
    }
}";

        internal static string ProcessCodeTemplates(string namespaceName, string className, string methodName, string generatedClassName)
        {
            // TODO at some point, we might need a less-hacky templating mechanism?!
            var generatedBenchmark = BenchmarkTemplate.benchmarkHarnessTemplate
                                .Replace(BenchmarkTemplate.namespaceReplaceText, namespaceName)
                                .Replace(BenchmarkTemplate.classReplaceText, className)
                                .Replace(BenchmarkTemplate.methodReplaceText, methodName)
                                .Replace(BenchmarkTemplate.methodParametersReplaceText, "")
                                .Replace(BenchmarkTemplate.generatedClassReplaceText, generatedClassName);
            return generatedBenchmark;
        }

        internal static string ProcessLauncherTemplate()
        {
            return benchmarkLauncherTemplate;
        }
    }
}
