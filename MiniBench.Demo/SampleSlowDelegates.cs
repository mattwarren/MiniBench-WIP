using MiniBench.Core;
using System;
using System.Runtime.CompilerServices;

namespace MiniBench.Demo
{
    // From // From https://github.com/controlflow/resharper-heapview/blob/master/ReSharper.HeapView/Tests/Data/Daemon/SlowDelegates01.cs
    class SampleSlowDelegates
    {
        private delegate void Action();

        // lags only if T is a reference type (but not at x64)
        static void GenericTest<T>() { PassMeDelegate(() => typeof(T)); }   // <--
        static void GenericTest2<T>() { PassMeDelegate(Bar<T>.Foo); }       // <--

        delegate object SomeFunc();
        [MethodImpl(MethodImplOptions.NoInlining)]
        static void PassMeDelegate(SomeFunc action) { GC.KeepAlive(action); }

        // Original Results
        // ################
        // 22 => 00:00:00.0091278   9.13 ms
        // 23 => 00:00:00.2983961 298.40 ms
        // 24 => 00:00:00.0092931   9.29 ms
        // 25 => 00:00:00.2817894 281.79 ms

        [Benchmark]
        public void Case22()
        {
            // delegates, created from lambdas in a generic method
            Measure(GenericTest<int>);     // 22
        }

        [Benchmark]
        public void Case23()
        {
            // delegates, created from lambdas in a generic method
            Measure(GenericTest<string>);  // 23  <--
        }

        [Benchmark]
        public void Case24()
        {
            // delegates, created from non-generic static method group in a generic type,
            // parametrized with the containing generic method's type parameter
            Measure(GenericTest2<int>);     // 24
        }

        [Benchmark]
        public void Case25()
        {
            // delegates, created from non-generic static method group in a generic type,
            // parametrized with the containing generic method's type parameter
            Measure(GenericTest2<string>);  // 25 <--
        }

        private void Measure(Action action)
        {
            action();
        }
    }

    internal interface IBar
    {
        object BooVirt();
        object BooVirt<U>();
    }

    class Bar : IBar
    {
        public static object Foo() { return null; }
        public static object Foo<U>() { return null; }
        public object Boo() { return null; }
        public object Boo<U>() { return null; }
        public virtual object BooVirt() { return null; }
        public virtual object BooVirt<U>() { return null; }
        public static void Baz<U>(U a) { }
        public void Baz2<U>(U a) { }
    }

    internal interface IBar<T>
    {
        object BooVirt();
    }

    class Bar<T> : IBar<T>
    {
        public static object Foo() { return null; }
        public object Boo() { return null; }
        public virtual object BooVirt() { return null; }
    }
}
