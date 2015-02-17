using MiniBench.Core;
using System;

namespace MiniBench.Demo
{
    // Fromhttp://hg.openjdk.java.net/code-tools/jmh/file/e0f24b563ae3/jmh-samples/src/main/java/org/openjdk/jmh/samples/JMHSample_10_ConstantFold.java
    public class SampleConstantFolding
    {
        // IDEs will say "Oh, you can convert this field to local variable". Don't. Trust. Them.
        private double x = Math.PI;

        // IDEs will probably also say "Look, it could be final". Don't. Trust. Them. Either.
        // TODO is const in c# the same as final ins Java?
        private const double wrongX = Math.PI;

        [Benchmark]
        public double baseline() 
        {
            // simply return the value, this is a baseline
            return Math.PI;
        }

        [Benchmark]
        public double measureWrong_1() 
        {
            // This is wrong: the source is predictable, and computation is foldable.
            return Math.Log(Math.PI);
        }

        [Benchmark]
        public double measureWrong_2() 
        {
            // This is wrong: the source is predictable, and computation is foldable.
            return Math.Log(wrongX);
        }

        [Benchmark]
        public double measureRight() 
        {
            // This is correct: the source is not predictable.
            return Math.Log(x);
        }
    }
}
