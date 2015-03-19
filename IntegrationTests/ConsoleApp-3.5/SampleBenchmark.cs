using System;
using System.Reflection;
using MiniBench.Core;
using Xunit;

namespace ConsoleApp_3_5
{
    public class SampleBenchmark_3_5
    {
        // This has to be static for the test to work!! 
        // The Benchmark Runner new's up a new instance of this class!!
        private static int _demoTestRunCount;

        public static void Main(string[] args)
        {
        }

        [Fact]
        public void BasicTest()
        {
            // From http://stackoverflow.com/questions/8517159/how-to-detect-at-runtime-that-net-version-4-5-currently-running-your-code
            // and http://stackoverflow.com/questions/2310701/determine-framework-clr-version-of-assembly/18623516#18623516
            Console.WriteLine("Environment Version;" + Environment.Version);
            //Console.WriteLine("Assembly Image Runtime Version: " + Assembly.GetExecutingAssembly().ImageRuntimeVersion);
            //Console.WriteLine("Version 1: " + typeof (int).Assembly.GetName().Version);
            //Console.WriteLine("Version 2: " + System.Runtime.InteropServices.RuntimeEnvironment.GetSystemVersion());
            //Console.WriteLine("Version 3: " + typeof (int).Assembly.ImageRuntimeVersion);

            //Console.WriteLine(ConfigurationManager.GetSection("startup"));

            _demoTestRunCount = 0;
            Options opt = new OptionsBuilder()
                    .Include(typeof(SampleBenchmark_3_5))
                    .WarmupRuns(0)
                    .Runs(1)
                    .Build();
            new Runner(opt).Run();

            Assert.True(_demoTestRunCount > 0, "Expected the Benchmark method to be run at least once: " + _demoTestRunCount);
        }

        [Benchmark]
        public double UnitTestBenchmark()
        {
            _demoTestRunCount++;
            return Math.Sqrt(123.456);
        }
    }
}