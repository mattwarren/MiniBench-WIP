﻿using System;
using System.Reflection;
using System.Security.Policy;
using System.Text.RegularExpressions;
using MiniBench.Core.Profiling;

namespace MiniBench.Core
{
    public class Runner
    {
        private readonly Options options;

        private Runner()
        {
        }

        public Runner(Options options)
        {
            this.options = options;
        }

        public void Run()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            foreach (Type type in assembly.GetTypes())
            {
                if (!type.IsClass || !type.IsPublic || type.IsAbstract ||
                    typeof(IBenchmarkTarget).IsAssignableFrom(type) == false)
                {
                    continue;
                }
                    
                if (String.IsNullOrEmpty(options.BenchmarkPrefix) == false &&
                    type.Name.StartsWith(options.BenchmarkPrefix) == false)
                {
                    continue;
                }

                if (String.IsNullOrEmpty(options.BenchmarkRegex) == false &&
                    Regex.IsMatch(type.Name, options.BenchmarkRegex) == false)
                {
                    continue;
                }

                //For example: "Generated_Runner_MiniBench_Demo_SampleBenchmark_DemoTest" comes from DemoTest() in MiniBench.Demo.SampleBenchmark.cs
                //Console.WriteLine("Expected:  " + options.BenchmarkPrefix);
                //Console.WriteLine("Found:     " + type.Name);
                IBenchmarkTarget obj = assembly.CreateInstance(type.FullName) as IBenchmarkTarget;
                if (obj == null)
                {
                    Console.WriteLine("Unable to create type: " + type.Name);
                    continue;
                }

                Profiler profiler = new Profiler();

                // TODO review this list of App Domain gotchas and see if we will run into any of them https://github.com/fixie/fixie/issues/8
                AppDomain domain = AppDomain.CreateDomain("MiniBench runner", new Evidence(), Environment.CurrentDirectory, Environment.CurrentDirectory, false);
                // TODO complete this App Domain stuff, what Type do we want to load into the App Domain?
                //BenchmarkResult loader = CreateInstance<IBenchmarkTarget>(domain);
                BenchmarkResult result = obj.RunTest(TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(10), profiler);
                //Console.WriteLine(result);
                Console.WriteLine();

                profiler.PrintOverallResults();
            }
        }

        private static T CreateInstance<T>(AppDomain domain)
        {
            Type type = typeof(T);
            return (T)domain.CreateInstanceAndUnwrap(type.Assembly.FullName, type.FullName);
        }
    }
}
