using System;
using System.Collections.Generic;
using System.Reflection;

namespace MiniBench.Core
{

    internal class CommandLineArgs
    {
        public bool ShouldExit { get; private set; }
        public bool ListBenchmarks { get; private set; }
        public bool ListProfilers { get; private set; }
        public string BenchmarksToRun { get; private set; }

        private readonly OptionSet optionSet;
        private bool help;
        
        public CommandLineArgs(string[] args)
        {
            optionSet = SetupOptions();
            ShouldExit = false;
            ParseCommandLineArgs(args);
        }

        private OptionSet SetupOptions()
        {
            // "value" parameters with a required value (append `=' to the option name) or an optional value (append `:' to the option name).
            return new OptionSet()
                .Add("?|help|h", "Prints out the options.", option => help = option != null)
                .Add("l|list", "List matching benchmarks and exit.", option => ListBenchmarks = option != null)
                .Add("lprof|listProf", "List the available Profilers and exit.", option => ListProfilers = option != null);
        }

        private void ParseCommandLineArgs(string[] args)
        {
            try
            {
                List<string> extraArgs = optionSet.Parse(args);

                if (help)
                {
                    string usageMessage =
                        "Usage: " + Assembly.GetCallingAssembly().GetName().Name + " [regexp*] " +
                        Environment.NewLine + "With optional regex to match the Benchmarks to run" +
                        Environment.NewLine + "If no regex is specified, all the Benchmarks within the file are run" +
                        Environment.NewLine + Environment.NewLine + "Options:";
                    showHelp(usageMessage);
                    ShouldExit = true;
                    return;
                }

                // This is how we can get "Default" parameters
                if (extraArgs.Count > 0)
                {
                    BenchmarksToRun = string.Join(" ", extraArgs.ToArray());
                    Console.WriteLine("Benchmarks To Run: \"{0}\"", BenchmarksToRun);
                }
            }
            catch (OptionException optionException)
            {
                showHelp(string.Format("Error {0} - usage is:", optionException.Message));
                ShouldExit = true;
            }
        }

        private void showHelp(string message)
        {
            Console.Error.WriteLine(message);
            optionSet.WriteOptionDescriptions(Console.Error);
        }
    }
}
