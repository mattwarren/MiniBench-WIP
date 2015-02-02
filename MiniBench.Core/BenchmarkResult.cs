// Copyright 2014 The Noda Time Authors. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;

namespace MiniBench.Core
{
    public sealed class BenchmarkResult : MarshalByRefObject
    {
        private readonly IBenchmarkTarget target;
        public IBenchmarkTarget Target { get { return target; } }

        private readonly long iterations;
        public long Iterations { get { return iterations; } }

        private readonly TimeSpan elapsedTime;
        public TimeSpan ElapsedTime { get { return elapsedTime; } }

        private readonly string failure;
        public string Failure { get { return failure; } }

        private BenchmarkResult(IBenchmarkTarget target, long iterations, TimeSpan elapsedTime, string failure)
        {
            this.target = target;
            this.iterations = iterations;
            this.elapsedTime = elapsedTime;
            this.failure = failure;
        }

        internal static BenchmarkResult ForFailure(IBenchmarkTarget target, string failure)
        {
            return new BenchmarkResult(target, 0, TimeSpan.Zero, failure);
        }

        internal static BenchmarkResult ForSuccess(IBenchmarkTarget target, long iterations, TimeSpan elapsedTime)
        {
            return new BenchmarkResult(target, iterations, elapsedTime, null);
        }

        public override string ToString()
        {
            return failure != null
                ? string.Format("{0}: Failed: {1}", target, failure)
                : string.Format("{0}: {1:N0} iterations in {2:N2}ms", target, iterations, elapsedTime.TotalMilliseconds);
        }
    }
}
