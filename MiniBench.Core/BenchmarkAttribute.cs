// Copyright 2014 The MiniBench Authors. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;

namespace MiniBench.Core
{
    /// <summary>
    /// Indicates that the method (or property) is a benchmark which can be executed.
    /// It must be parameterless; any return value is ignored.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class BenchmarkAttribute : Attribute
    {
    }
}
