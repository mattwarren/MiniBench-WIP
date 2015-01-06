MiniBench.Core
----

This is the core assembly which 3rd party code refers to in order to specify benchmarks.
It has two purposes:

- Provide the attributes required for 3rd party code
- Provide just enough functionality to live in the same AppDomain as the 3rd party code and run the tests.
  ("Driver" code (in MiniBench) will load this and the 3rd party code into an AppDomain, then control testing.)

It (MiniBench.Core) must not:

- Specify an architecture
- Have any other dependencies


.NET Framework Versions of each Project
----
MiniBench		- .NET 4.5.1
MiniBench.Core	- .NET 2.0
MiniBench.Demo	- .NET 4.0