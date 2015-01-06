using System;
using System.Security.Policy;
using MiniBench.Core;

namespace MiniBench
{
    /// <summary>
    /// Command-line interface for MiniBench
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            string assembly = args[0];
            AppDomain domain = AppDomain.CreateDomain("MiniBench runner", new Evidence(), Environment.CurrentDirectory, Environment.CurrentDirectory, false);
            var loader = CreateInstance<AssemblyLoader>(domain);
            loader.Load(assembly);
            var probe = CreateInstance<BenchmarkProbe>(domain);
            var targets = probe.Probe(assembly);
            foreach (var target in targets)
            {
                var result = target.RunTest(TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(10));
                Console.WriteLine(result);
            }
        }

        private static T CreateInstance<T>(AppDomain domain)
        {
            Type type = typeof(T);
            return (T) domain.CreateInstanceAndUnwrap(type.Assembly.FullName, type.FullName);
        }
    }
}
