using System;
using System.Reflection;
using System.Security.Policy;

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
                    typeof(IBenchmarkTarget).IsAssignableFrom(type) == false || 
                    type.Name.StartsWith(options.BenchmarkPrefix) == false)
                {
                    continue;
                }

                //For example: "Generated_Runner_MiniBench_Demo_SampleBenchmark_DemoTest" comes from DemoTest() in MiniBench.Demo.SampleBenchmark.cs
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

                AppDomain domain = AppDomain.CreateDomain("MiniBench runner", new Evidence(), Environment.CurrentDirectory, Environment.CurrentDirectory, false);
                //BenchmarkResult loader = CreateInstance<IBenchmarkTarget>(domain);
                BenchmarkResult result = obj.RunTest(TimeSpan.FromMilliseconds(1000), TimeSpan.FromMilliseconds(0100));
                Console.WriteLine(result + "\n");
            }
        }

        private static T CreateInstance<T>(AppDomain domain)
        {
            Type type = typeof(T);
            return (T)domain.CreateInstanceAndUnwrap(type.Assembly.FullName, type.FullName);
        }
    }
}
