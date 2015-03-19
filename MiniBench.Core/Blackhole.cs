using System;
using System.Diagnostics;
//using System.Runtime.CompilerServices;
using System.Runtime.CompilerServices;
using System.Threading;

namespace MiniBench.Core
{
    // Currently this is pretty much a copy of the JMH (Java) version, see 
    // http://hg.openjdk.java.net/code-tools/jmh/file/e0f24b563ae3/jmh-core/src/main/java/org/openjdk/jmh/infra/Blackhole.java
    public class Blackhole
    {
        //public volatile double d1;
        public double d1;
        public double d2;

        // This is delibrately null, it should throw a NullReferenceException if accessed!
        public volatile Blackhole nullBait = null;

        public Blackhole()
        {
            Random r = new Random((int)Stopwatch.GetTimestamp());

            d1 = r.NextDouble(); 
            d2 = d1 + Ulp(d1);

            if (d1 == d2)
            {
                //throw new IllegalStateException("double tombstones are equal");
                throw new ApplicationException("double tombstones are equal");
            }
        }

        public void Consume(Object @object)
        {
            // TODO Complete this for Object
        }

        public void Consume(ValueType @valueType)
        {
            // TODO Complete this for ValueType
        }


        // TODO work out how much of this we really need to defeat the .NET JITter (as opposed to HotSpot)
        //[MethodImpl(MethodImplOptions.NoInlining)]
        //[MethodImpl(MethodImplOptions.AggressiveInlining)] // .NET 4.0 and up only
        public void ConsumeJavaMethod(double d)
        {
            double d1 = this.d1; // volatile read
            //double d1 = Thread.VolatileRead(ref this.d1);
            //double d1 = Volatile.Read(ref this.d1);
            Thread.MemoryBarrier();

            // this uses '&' so that both operands are executed
            // if it used '&&' instead that the RHS one could be short-circuited 
            if (d == d1 & d == d2) 
            {
                // SHOULD NEVER HAPPEN
                nullBait.d1 = d; // implicit null pointer exception
            }
        }

        // Don't force the JITter one way or the other, let it decide and see what happens (inlined or not)
        [MethodImpl(MethodImplOptions.NoInlining)]
        public void Consume(double d)
        {
            this.d1 += d;
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)] // .NET 4.0 and up only
        public void ConsumeAggressiveInlining(double d)
        {
            this.d1 += d;
        }

        private double Ulp(double value)
        {
            // From http://stackoverflow.com/questions/9485943/calculate-the-unit-in-the-last-place-ulp-for-doubles/9487719#9487719
            long bits = BitConverter.DoubleToInt64Bits(value);
            double nextValue = BitConverter.Int64BitsToDouble(bits + 1);
            return nextValue - value;
        }
    }
}
