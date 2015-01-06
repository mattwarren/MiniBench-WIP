using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace MiniBench.Core
{
    public class AssemblyLoader : MarshalByRefObject
    {
        public void Load(string name)
        {
            Assembly.Load(name);
        }
    }
}
