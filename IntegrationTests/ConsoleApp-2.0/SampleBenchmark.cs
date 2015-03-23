using System;
using System.Reflection;
using MiniBench.Core;
using Xunit;

namespace ConsoleApp_2_0
{
    public class SampleBenchmark_2_0
    {
        // This has to be static for the test to work!! 
        // The Benchmark Runner new's up a new instance of this class!!
        private static int _demoTestRunCount;

        public static void Main(string[] args)
        {
            PrintRuntimeInfo();
            Console.WriteLine("Press <ENTER> to exit");
            Console.ReadLine();
        }

        private static void PrintRuntimeInfo()
        {
            Console.WriteLine("Environment Version: " + Environment.Version);
            Console.WriteLine("Assembly Runtime Version: " + Assembly.GetExecutingAssembly().ImageRuntimeVersion);
        }

        [Fact]
        public void BasicTest()
        {
            _demoTestRunCount = 0;
            Options opt = new OptionsBuilder()
                    .Include(typeof(SampleBenchmark_2_0))
                    .WarmupRuns(0)
                    .Runs(1)
                    .Build();
            new Runner(opt).Run();

            Assert.True(_demoTestRunCount > 0, "Expected the Benchmark method to be run at least once: " + _demoTestRunCount);

            PrintRuntimeInfo();

            Assembly executingAssembly = Assembly.GetExecutingAssembly();
            Assert.True(executingAssembly.ImageRuntimeVersion.StartsWith("v2.0."),
                        "Expected .NET Runtime Version to be v2.0.X.X, but was " + executingAssembly.ImageRuntimeVersion);
        }

        [Benchmark]
        public double UnitTestBenchmark()
        {
            _demoTestRunCount++;
            return Math.Sqrt(123.456);
        }
    }
}