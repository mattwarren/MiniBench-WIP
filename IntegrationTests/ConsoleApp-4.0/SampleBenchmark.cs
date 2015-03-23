using System;
using System.Linq;
using System.Reflection;
using System.Runtime.Versioning;
using MiniBench.Core;
using Xunit;

namespace ConsoleApp_4_0
{
    public class SampleBenchmark_4_0
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

            // From http://stackoverflow.com/questions/2310701/determine-framework-clr-version-of-assembly/18623516#18623516
            // Only works in .NET 4.0 and upwards though (not 2.0, 3.0 & 3.5)
            object[] list = Assembly.GetExecutingAssembly().GetCustomAttributes(true);
            var a = (TargetFrameworkAttribute)list.FirstOrDefault(p => p is TargetFrameworkAttribute);
            // TODO Seems like MiniBench needs to put these back in when we re-write the file?
            if (a != null)
            {
                Console.WriteLine("Target Framework: " + a.FrameworkDisplayName);
            }
        }

        [Fact]
        public void BasicTest()
        {
            _demoTestRunCount = 0;
            Options opt = new OptionsBuilder()
                    .Include(typeof(SampleBenchmark_4_0))
                    .WarmupRuns(0)
                    .Runs(1)
                    .Build();
            new Runner(opt).Run();

            Assert.True(_demoTestRunCount > 0, "Expected the Benchmark method to be run at least once: " + _demoTestRunCount);

            PrintRuntimeInfo();

            Assembly executingAssembly = Assembly.GetExecutingAssembly();
            Assert.True(executingAssembly.ImageRuntimeVersion.StartsWith("v4.0."),
                        "Expected .NET Runtime Version to be v4.0.X.X, but was " + executingAssembly.ImageRuntimeVersion);
        }

        [Benchmark]
        public double UnitTestBenchmark()
        {
            _demoTestRunCount++;
            return Math.Sqrt(123.456);
        }
    }
}