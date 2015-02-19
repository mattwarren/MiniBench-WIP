using MiniBench.Core;
using System.Diagnostics;

namespace MiniBench.Demo
{
    // Inspired by 
    // http://shipilev.net/blog/2014/nanotrusting-nanotime
    // http://blogs.msdn.com/b/ericlippert/archive/2010/04/08/precision-and-accuracy-of-datetime.aspx
    // 
    // We seem to get results that are consistent with his, yay!! i.e. latency ~16ns granularity ~390ns
    // From http://shipilev.net/blog/2014/nanotrusting-nanotime/#_one_off_measurements:
    //
    // Those were *nix-ish platforms. Let’s try Windows. This is a very typical Windows data point:
    //   Java(TM) SE Runtime Environment, 1.7.0_51-b13
    //   Java HotSpot(TM) 64-Bit Server VM, 24.51-b03
    //   Windows 7, 6.1, amd64
    //
    //     Running with 1 threads and [-client]:
    //        granularity_nanotime:     371,419 +- 1,541 ns
    //            latency_nanotime:      14,415 +- 0,389 ns
    //
    //     Running with 1 threads and [-server]:
    //        granularity_nanotime:     371,237 +- 1,239 ns
    //            latency_nanotime:      14,326 +- 0,308 ns

    class SampleStopwatchTimestamp
    {
        [Benchmark]
        public void baseline()
        {
            // do nothing, this is a baseline
            return;
        }

        [Benchmark]
        public long Latency()
        {
            // See http://shipilev.net/blog/2014/nanotrusting-nanotime/#_latency
            return Stopwatch.GetTimestamp();
        }

        [Benchmark]
        public long Granularity()
        {
            // See http://shipilev.net/blog/2014/nanotrusting-nanotime/#_granularity
            long current, lastValue  = Stopwatch.GetTimestamp();
            do
            {
                current = Stopwatch.GetTimestamp();
            } while (current == lastValue);
            lastValue = current;
            return current;
        }
    }
}
