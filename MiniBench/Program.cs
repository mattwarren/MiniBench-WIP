using MiniBench.Core;
using System;
using System.IO;
using System.Reflection;
using System.Security.Policy;

namespace MiniBench
{
    /// <summary>
    /// Command-line interface for MiniBench
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            string fileToBenchmark = args[1];
            string extension = Path.GetExtension(fileToBenchmark);

            AppDomain domain = AppDomain.CreateDomain("MiniBench runner", new Evidence(), Environment.CurrentDirectory, Environment.CurrentDirectory, shadowCopyFiles: false);
            var loader = CreateInstance<AssemblyLoader>(domain);

            Assembly loadedAssembly = null;
            if (extension == ".dll")
            {
                AssemblyName assembly = AssemblyName.GetAssemblyName(fileToBenchmark);
                Console.WriteLine("Loading Benchmark Assembly from disk: {0}\n", assembly.FullName);
                loadedAssembly = loader.Load(assembly.FullName); // fileToBenchmark);
            }
            else if (extension == ".cs")
            {
                var peStream = new MemoryStream();
                var pdbMemoryStream = new MemoryStream();
                Console.WriteLine("Compiling Benchmark code into an Assembly: {0}\n", fileToBenchmark);
                var generator = new CodeGenerator(peStream, pdbMemoryStream);
                generator.GenerateCode(File.ReadAllText(fileToBenchmark));
                loadedAssembly = loader.Load(rawAssembly: peStream.GetBuffer(), rawSymbolStore: pdbMemoryStream.GetBuffer());
            }

            var probe = CreateInstance<BenchmarkProbe>(domain);
            BenchmarkTarget[] targets = probe.Probe(loadedAssembly);

            foreach (var target in targets)
            {
                var result = target.RunTest(TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(10));
                Console.WriteLine(result);
            }
        }

        private static T CreateInstance<T>(AppDomain domain)
        {
            Type type = typeof(T);
            return (T)domain.CreateInstanceAndUnwrap(type.Assembly.FullName, type.FullName);
        }
    }
}
