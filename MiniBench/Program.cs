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

            var projectParser = new ProjectFileParser();
            var sourceFiles = projectParser.GetSourceFiles(projectFileName);
            var references = projectParser.GetReferences(projectFileName);

            Console.WriteLine("Compiling Benchmark code into an self-contained Benchmark.exe: {0}\n", projectFileName);
            var generator = new CodeGenerator(Path.GetDirectoryName(projectFileName), sourceFiles, references);
            generator.GenerateCode();

            return 0;
        }
    }
}
