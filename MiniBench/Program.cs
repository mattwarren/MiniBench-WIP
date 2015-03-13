using System;
using System.IO;


namespace MiniBench
{
    /// <summary>
    /// Command-line interface for MiniBench
    /// </summary>
    class Program
    {
        static int Main(string[] args)
        {
            var expectedExtension = ".csproj";
            if (args.Length == 0)
            {
                Console.WriteLine("No Command line arguments were provided, you must specify a {0} file", expectedExtension);
                return -1;
            }

            string projectFileName = args[0];
            string extension = Path.GetExtension(projectFileName);
            if (extension != expectedExtension)
            {
                Console.WriteLine("You must specify a {0} file: {1}", expectedExtension, projectFileName);
                return -1;
            }

            Console.WriteLine("Compiling Benchmark code into an self-contained Benchmark.exe using:\n\t{0}\n", projectFileName);
            var projectParser = new ProjectFileParser();
            var projectSettings = projectParser.ParseProjectFile(projectFileName);

            var generator = new CodeGenerator(projectSettings);
            generator.GenerateCode();

            return 0;
        }
    }
}
