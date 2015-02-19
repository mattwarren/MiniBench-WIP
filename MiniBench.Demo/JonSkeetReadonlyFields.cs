using MiniBench.Core;

namespace MiniBench.Demo
{
    // See http://codeblog.jonskeet.uk/2014/07/16/micro-optimization-the-surprising-inefficiency-of-readonly-fields/
    // Building this from the command line with /o+ /debug- and running (in a 64-bit CLR, but no RyuJIT) this takes about 20 seconds to run on my laptop. 

    // We can make it much faster with just one small change:
    // class Test
    // { 
    //     private Int256 value;
    //     // Code as before 
    // }

    // The same test now takes about 4 seconds – a 5-fold speed improvement, just by making a field non-readonly. 
    // If we look at the IL for the TotalValue property, the copying becomes obvious. Here it is when the field is readonly:

    // There is an optimization which is even faster – moving the totalling property into Int256. 
    // That way (with the non-readonly field, still) the total time is less than a second – twenty times faster than the original code!

    class JonSkeetReadonlyFields
    {
        TestSlow testSlow = new TestSlow();
        TestFast testFast = new TestFast();

        [Benchmark]
        public void baseline()
        {
            // do nothing, this is a baseline
            return;
        }

        [Benchmark]
        public long SlowVersion()
        {
            return testSlow.TotalValue;
        }

        [Benchmark]
        public void SlowVersionMeasureWrong()
        {
            // This is wrong: result is not used, and the entire computation is optimized out.
            var temp = testSlow.TotalValue;
        }

        [Benchmark]
        public long FastVersion()
        {
            return testFast.TotalValue;
        }

        [Benchmark]
        public void FastVersionMeasureWrong()
        {
            // This is wrong: result is not used, and the entire computation is optimized out.
            var temp = testFast.TotalValue;
        }

        [Benchmark]
        public long EvenFasterVersion()
        {
            return testFast.TotalValueEvenFaster;
        }

        [Benchmark]
        public void EvenFasterVersionMeasureWrong()
        {
            // This is wrong: result is not used, and the entire computation is optimized out.
            var temp = testFast.TotalValueEvenFaster;
        }
    }

    class TestSlow
    {
        private readonly Int256 value;

        public TestSlow()
        {
            value = new Int256(1L, 5L, 10L, 100L);
        }

        public long TotalValue
        {
            get
            {
                return value.Bits0 + value.Bits1 + value.Bits2 + value.Bits3;
            }
        }
    }

    class TestFast
    {
        // NO READONLY
        private Int256 value;

        public TestFast()
        {
            value = new Int256(1L, 5L, 10L, 100L);
        }

        public long TotalValue
        {
            get
            {
                return value.Bits0 + value.Bits1 + value.Bits2 + value.Bits3;
            }
        }

        public long TotalValueEvenFaster
        {
            get { return value.TotalValue; }
        }
    }

    public struct Int256 
    { 
        private readonly long bits0;
        private readonly long bits1;
        private readonly long bits2;
        private readonly long bits3;

        public Int256(long bits0, long bits1, long bits2, long bits3)
        {
            this.bits0 = bits0;
            this.bits1 = bits1;
            this.bits2 = bits2;
            this.bits3 = bits3;
        }

        public long Bits0 { get { return bits0; } }
        public long Bits1 { get { return bits1; } }
        public long Bits2 { get { return bits2; } }
        public long Bits3 { get { return bits3; } }

        public long TotalValue
        {
            get
            {
                return this.bits0 + this.bits1 + this.bits2 + this.bits3;
            }
        }
    }
}
