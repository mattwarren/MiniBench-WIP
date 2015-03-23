using MiniBench.Core;
using System;
//using System.Runtime.CompilerServices;

namespace MiniBench.Demo
{
    // From http://hg.openjdk.java.net/code-tools/jmh/file/e0f24b563ae3/jmh-samples/src/main/java/org/openjdk/jmh/samples/JMHSample_08_DeadCode.java
    class SampleDeadCodeElimination
    {
        private double x = Math.PI;
        private Blackhole blackhole = new Blackhole();

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
            Math.Sqrt(x);
        }

        [Benchmark]
        public void measureBlackholeConsume()
        {
            // This is wrong: result is not used, and the entire computation is optimized out.
            blackhole.Consume(Math.Sqrt(x));
        }

        [Benchmark]
        public void measureBlackholeConsumeAggressiveInlining()
        {
            // This is wrong: result is not used, and the entire computation is optimized out.
            blackhole.ConsumeAggressiveInlining(Math.Sqrt(x));
        }

        [Benchmark]
        public void measureBlackholeConsumeJavaMethod()
        {
            // This is wrong: result is not used, and the entire computation is optimized out.
            blackhole.ConsumeJavaMethod(Math.Sqrt(x));
        }

        [Benchmark]
        public double measureRight()
        {
            // This is correct: the result is being used.
            return Math.Sqrt(x);
        }

        //[Benchmark]
        //public int NoInliningMethodCall()
        //{
        //    return NoInliningMethod();
        //}

        //[Benchmark]
        //public int AggressiveInliningMethodCall()
        //{
        //    return AggressiveInliningMethod();
        //}

        //[MethodImpl(MethodImplOptions.NoInlining)]
        //private int NoInliningMethod()
        //{
        //    return 1;
        //}

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        //private int AggressiveInliningMethod()
        //{
        //    return 1;
        //}
    }
}
