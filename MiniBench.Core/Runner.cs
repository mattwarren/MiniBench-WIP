using System;
using System.Reflection;

namespace MiniBench.Core
{
    public class Runner
    {
        private readonly Options options;

        private Runner()
        {
        }

        public Runner(Options options)
        {
            this.options = options;
        }

        public void Run()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            foreach (Type type in assembly.GetTypes())
            {
                if (!type.IsClass || !type.IsPublic || type.IsAbstract ||
                    typeof(IBenchmarkTarget).IsAssignableFrom(type) == false)
                {
                    continue;
                }

                //"Generated_Runner_MiniBench_Demo_SampleBenchmark_DemoTest" comes from DemoTest() in MiniBench.Demo.SampleBenchmark.cs
                Console.WriteLine("Expected:  " + options.BenchmarkPrefix);
                Console.WriteLine("Found:     " + type.Name);
                //Console.WriteLine("FullName:  " + type.FullName);
                //Console.WriteLine("Namespace: " + type.Namespace);
                IBenchmarkTarget obj = assembly.CreateInstance(type.FullName) as IBenchmarkTarget;
                if (obj == null)
                {
                    Console.WriteLine("Unable to create type: " + type.Name);
                    continue;
                }

                if (type.Name.StartsWith(options.BenchmarkPrefix))
                {
                    BenchmarkResult result = obj.RunTest(TimeSpan.FromMilliseconds(5), TimeSpan.FromMilliseconds(5));
                    Console.WriteLine(result + "\n");
                }
            }
        }
    }
}
