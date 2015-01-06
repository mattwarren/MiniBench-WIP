MiniBench.Core
----

This is the core assembly which 3rd party code refers to in order to specify benchmarks.
It has two purposes:

- Provide the attributes required for 3rd party code
- Provide just enough functionality to live in the same AppDomain as the 3rd party code and run the tests.
  ("Driver" code will load this and the 3rd party code into an AppDomain, then control testing.)

It must not:

- Specify an architecture
- Have any other dependencies
