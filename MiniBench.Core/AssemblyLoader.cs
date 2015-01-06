using System;
using System.Reflection;

namespace MiniBench.Core
{
    public class AssemblyLoader : MarshalByRefObject
    {
        public Assembly Load(string name)
        {
            return Assembly.Load(name);
        }

        public Assembly Load(byte[] rawAssembly, byte[] rawSymbolStore)
        {
            return Assembly.Load(rawAssembly: rawAssembly, rawSymbolStore: rawSymbolStore);
        }
    }
}