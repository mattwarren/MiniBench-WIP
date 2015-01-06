using System;
using System.Collections.Generic;
using System.Reflection;

namespace MiniBench.Core
{
    /// <summary>
    /// Probes an assembly for benchmarks.
    /// </summary>
    public class BenchmarkProbe : MarshalByRefObject
    {
        //public BenchmarkTarget[] Probe(string assemblyName)
        //{
        //    var assembly = FindAssembly(assemblyName);
        //    if (assembly == null)
        //    {
        //        // TODO: Ick.
        //        throw new Exception("Could not find assembly");
        //    }

        //    return Probe(assembly);
        //}

        public BenchmarkTarget[] Probe(Assembly assembly)
        {
            List<BenchmarkTarget> targets = new List<BenchmarkTarget>();
            foreach (var type in assembly.GetTypes())
            {
                if (!type.IsClass || !type.IsPublic || type.IsAbstract ||
                    type.GetConstructor(Type.EmptyTypes) == null || type.IsGenericType)
                {
                    continue;
                }
                foreach (var method in type.GetMethods())
                {
                    if (method.IsPublic &&
                        method.IsDefined(typeof(BenchmarkAttribute), false) &&
                        method.GetParameters().Length == 0)
                    {
                        targets.Add(new BenchmarkTarget(type, method));
                    }
                }
            }
            return targets.ToArray();
        }

        //private Assembly FindAssembly(string name)
        //{
        //    foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        //    {
        //        if (assembly.GetName().Name == name)
        //        {
        //            return assembly;
        //        }
        //    }
        //    return null;
        //}
    }
}
