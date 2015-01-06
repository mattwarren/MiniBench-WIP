using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MiniBench.Core;

namespace MiniBench.Demo
{
    public class SampleBenchmark
    {
        [Benchmark]
        public void SomeMethod()
        {
            DateTime.Now.ToString();
        }
    }
}
