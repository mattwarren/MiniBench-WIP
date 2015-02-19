using MiniBench.Core;
using System;

namespace MiniBench.Demo
{
    // From http://hg.openjdk.java.net/code-tools/jmh/file/e0f24b563ae3/jmh-samples/src/main/java/org/openjdk/jmh/samples/JMHSample_08_DeadCode.java
    class SampleDeadCodeElimination
    {
        private double x = Math.PI;

        [Benchmark]
        public void baseline()
        {
            // do nothing, this is a baseline
            return;
        }

        [Benchmark]
        public void measureWrong()
        {
            // This is wrong: result is not used, and the entire computation is optimized out.
            Math.Log(x);
        }

        [Benchmark]
        public double measureRight()
        {
            // This is correct: the result is being used.
            return Math.Log(x);
        }
    }
}
