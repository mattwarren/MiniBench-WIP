## Definitions

### Scale of benchmarks:

- kilo: > 1000 s
- seconds: 1…1000 s
- **milli**: 1…1000 ms, 
- **micro**: 1…1000 us, single webapp request
- **nano**: 1…1000 ns, single operations
- picoseconds: 1…1000 ps, pipelining

Aim is to make a framework that can **accurately** measure **milli**, **micro** and **nano** benchmarks. 

But in reality the main use-cases are probably **milli/micro** benchmarks, so these must work above all else. (*Based on my experience so far, you need to benchmarking something like `Math.Sqrt(..)` to get down to the **nano** level, but I could be wrong?*)

## Problem(s) we're trying to solve

###Too many people get benchmarking wrong
Just search on Stack Overflow, you see a lot of common mistakes:
 
- Using `DateTime` instead of `StopWatch`
- Running a DEBUG build, or under the Debugger
- Not allowing a warm-up period (for the JITter, caching etc)
- Not running enough iterations to get a meaningful result
- Not removing (as much as possible) the overhead of the Garbage Collection

In [this 1 post alone](http://stackoverflow.com/questions/10617681/unexpected-poor-performance-of-delegates-in-c-sharp) several users make benchmarking mistakes and are corrected by others.

Even this [highly up-voted SO answer](http://stackoverflow.com/questions/1047218/benchmarking-small-code-samples-in-c-can-this-implementation-be-improved/1048708#1048708) has problems, for instance it doesn't [prevent dead-code elimination](http://mattwarren.org/2014/09/19/the-art-of-benchmarking/).

###No common/standard way of presenting benchmark results
what to write here????

## Things to consider

#### Defeating the JITter
Need to worry about the effect of the JITter performing:
- inlining
- dead-code elimination
- loop-unrolling (although we may have to do this ourselves if the overhead of the loop dominates the code we are trying to benchmark, this is definitely for **nano** though)

#### Action/Func v. Having the code in-line
.NET [doesn't inline delegate calls](http://www.philosophicalgeek.com/2014/07/25/using-windbg-to-answer-implementation-questions-for-yourself-can-a-delegate-invocation-be-inlined/), like it does with regular function calls, this is an overhead which may or may not be acceptable (will need to measure). The only way I can see to get round this is to use some form of dynamic code-gen, maybe via Roslyn? i.e. given this code

``` csharp
[Benchmark]
public void Test()
{
    return Math.Sqrt(123.456);
}
```

we generate, emit and execute something like this:

``` csharp
static double ProfileDirect(ResultInfo result, int iterations)
{
	// clean up
	GC.Collect();
	GC.WaitForPendingFinalizers();
	GC.Collect();

	// warm up
	var temp = Math.Sqrt(123.456);

	var watch = new Stopwatch();
	watch.Start();
	for (int i = 0; i < iterations; i++)
	{
		temp += Math.Sqrt(123.456);
	}
	watch.Stop();	
    result.Elapsed = watch.ElapsedTicks;
    result.Iterations = iterations;

    // prevent dead-code elimination
	return temp;
}

```

#### Getting statistics and measurements correct
We should be profiling in *batches* and recording a total time for each batch. Then we should record an overall time where the results of the batches are combined and we get a average/median/std-dev/variance etc of the batch timings. 

This is so we can get more detailed measurements **without** having the overhead of `Stopwatch` on every iteration of the benchmark.

#### Integration with existing solutions
- Want to make this C.I friendly. A good use case is libraries like [Jil](https://github.com/kevin-montrose/Jil), [NodaTime](https://code.google.com/p/noda-time/source) (amongst others), that in order to prevent regressions, have micro-benchmarks that run against every build. Maybe we could submit a pull-requests against those libraries, to make it easier for them to use minibench.
- Need to find out what (if any) analytic libraries/packages it is worth integrating with **OR** is it just okay to output a csv/txt file?
- Should be really easy to copy/paste benchmarks on site like Stack Overflow, so good console app support as well.

## Existing .NET solutions

Either we can use ideas from these or see if the authors would like to be involved in Minibench

1. [Lambda Micro benchmarking](https://github.com/biboudis/LambdaMicrobenchmarking) used in [Clash of the Lambdas](http://biboudis.github.io/clashofthelambdas/)
2. [Etimo.Benchmarks](http://etimo.se/blog/etimo-benchmarks-lightweight-net-benchmark-tool/)
3. [Noda.Time Benchmarks](https://code.google.com/p/noda-time/source/browse/#hg%2Fsrc%2FNodaTime.Benchmarks)

## Useful references

1. As always Eric Lippert is worth reading!
 - http://tech.pro/blog/1293/c-performance-benchmark-mistakes-part-one
    - **Mistake #1:** Choosing a bad metric.
    - **Mistake #2:** Over-focusing on subsystem performance at the expense of end-to-end performance.
    - **Mistake #3:** Running your benchmark in the debugger.
    - **Mistake #4:** Benchmarking the debug build instead of the release build.
 - http://tech.pro/tutorial/1295/c-performance-benchmark-mistakes-part-two
    - **Mistake #5:** Using a clock instead of a stopwatch.
 - http://tech.pro/tutorial/1317/c-performance-benchmark-mistakes-part-three
    - **Mistake #6:** Treat the first run as nothing special when measuring average performance.
    - **Mistake #7:** Assuming that runtime characteristics in one environment tell you what behavior will be in a different environment.
 - http://tech.pro/tutorial/1433/performance-benchmark-mistakes-part-four
    - **Mistake #8:** Forget to take garbage collection effects into account.
1. [Microbenchmarks in Java and C#](http://www.itu.dk/people/sestoft/papers/benchmarking.pdf) - some real research done on the subject!
1. [MeasureIt](http://measureitdotnet.codeplex.com/) created by [Vance Morrison](http://blogs.msdn.com/b/vancem/archive/2009/02/06/measureit-update-tool-for-doing-microbenchmarks.aspx) an Architect on the .NET Runtime Team (specializing in performance issues with the runtime or managed code in general).

