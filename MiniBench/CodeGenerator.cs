using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace MiniBench
{
    internal class CodeGenerator
    {
        private readonly ProjectSettings projectSettings;

        private readonly CSharpParseOptions parseOptions;

        private readonly Encoding defaultEncoding = Encoding.UTF8;

        private readonly String filePrefix = "Generated_Runner";
        private readonly String launcherFileName = "Generated_Launcher.cs";
        private readonly String benchmarkAttribute = "Benchmark";

        internal CodeGenerator(ProjectSettings projectSettings)
        {
            this.projectSettings = projectSettings;

            // We don't want to target .NET 2.0 only LANGUAGE features, otherwise we can't use all the nice compiler-only stuff
            // like var, auto-properties, names arguments, etc. The main thing is that when we build the Benchmark .exe/.dll, 
            // we need to TARGET the correct Runtime Framework Version, which is either .NET 2.0 or 4.0.
            parseOptions = new CSharpParseOptions(kind: SourceCodeKind.Regular, languageVersion: LanguageVersion.CSharp4);
        }

        internal void GenerateCode()
        {
            var outputDirectory = Environment.CurrentDirectory;
            var generatedCodeDirectory = Path.Combine(outputDirectory, "GeneratedCode");
            Directory.CreateDirectory(generatedCodeDirectory);
            var fileDeletionTimer = Stopwatch.StartNew();
            foreach (var existingGeneratedFile in Directory.EnumerateFiles(generatedCodeDirectory, filePrefix + "*"))
            {
                File.Delete(existingGeneratedFile);
            }
            fileDeletionTimer.Stop();
            Console.WriteLine("\nTook {0} ({1,7:N2}ms) - to delete existing files from disk\n", fileDeletionTimer.Elapsed, fileDeletionTimer.ElapsedMilliseconds);

            var allSyntaxTrees = new List<SyntaxTree>(GenerateEmbeddedCode());
            foreach (var file in projectSettings.SourceFiles.Where(f => f.StartsWith("Properties\\") == false))
            {
                Console.WriteLine("Processing file: " + file);
                var code = File.ReadAllText(Path.Combine(projectSettings.RootFolder, file));
                var benchmarkTree = CSharpSyntaxTree.ParseText(code, options: parseOptions);

                // TODO error checking, in case the file doesn't have a Namespace, Class or any valid Methods!
                var @namespace = NodesOfType<NamespaceDeclarationSyntax>(benchmarkTree).FirstOrDefault();
                var namespaceName = @namespace.Name.ToString();
                // TODO we're not robust to having multiple classes in 1 file, we need to find the class that contains the [Benchmark] methods!!
                var @class = NodesOfType<ClassDeclarationSyntax>(benchmarkTree).FirstOrDefault();
                var className = @class.Identifier.ToString();
                var allMethods = NodesOfType<MethodDeclarationSyntax>(benchmarkTree);

                allSyntaxTrees.Add(benchmarkTree);

                var generatedRunners = GenerateRunners(allMethods, namespaceName, className, generatedCodeDirectory);
                allSyntaxTrees.AddRange(generatedRunners);
            }

            var generatedLauncher = GenerateLauncher(generatedCodeDirectory);
            allSyntaxTrees.Add(generatedLauncher);

            CompileAndEmitCode(allSyntaxTrees);
        }

        private IEnumerable<SyntaxTree> GenerateRunners(IEnumerable<MethodDeclarationSyntax> methods, string namespaceName, string className, string outputDirectory)
        {
            var methodWithAttributes = methods.Select(m => new
                {
                    Name = m.Identifier.ToString(),
                    ReturnType = m.ReturnType,
                    Blackhole = ShouldGenerateBlackhole(m.ReturnType),
                    Attributes = String.Join(", ", m.AttributeLists.SelectMany(atrl => atrl.Attributes.Select(atr => atr.Name.ToString())))
                });
            Console.WriteLine(
                String.Join("\n", methodWithAttributes.Select(m => 
                    string.Format("{0,25} - {1} - (Blackhole {2,5}), Attributes = {3}",
                        m.Name, m.ReturnType.ToString().PadRight(10), m.Blackhole, m.Attributes))));

            var validMethods = methods.Where(m => m.Modifiers.Any(mod => mod.IsKind(SyntaxKind.PublicKeyword)))
                                      .Where(m => m.AttributeLists.SelectMany(atrl => atrl.Attributes)
                                                                  .Any(atr => atr.Name.ToString() == benchmarkAttribute))
                                      .ToList();
            var generatedRunners = new List<SyntaxTree>(validMethods.Count);
            foreach (var method in validMethods)
            {
                var methodName = method.Identifier.ToString();
                // Can't have '.' or '-' in class names (which is where this gets used)
                var generatedClassName = string.Format("{0}_{1}_{2}_{3}",
                                            filePrefix,
                                            namespaceName.Replace('.', '_'),
                                            className,
                                            methodName);
                var fileName = string.Format(generatedClassName + ".cs");
                var outputFileName = Path.Combine(outputDirectory, fileName);

                var codeGenTimer = Stopwatch.StartNew();
                var generateBlackhole = ShouldGenerateBlackhole(method.ReturnType);
                var generatedBenchmark = BenchmarkTemplate.ProcessCodeTemplates(namespaceName, className, methodName, generatedClassName, generateBlackhole);
                var generatedRunnerTree = CSharpSyntaxTree.ParseText(generatedBenchmark, options: parseOptions, path: outputFileName, encoding: defaultEncoding);
                generatedRunners.Add(generatedRunnerTree);
                codeGenTimer.Stop();
                Console.WriteLine("Took {0} ({1,7:N2}ms) - to generate CSharp Syntax Tree", codeGenTimer.Elapsed, codeGenTimer.ElapsedMilliseconds);

                var fileWriteTimer = Stopwatch.StartNew();
                File.WriteAllText(outputFileName, generatedRunnerTree.GetRoot().ToFullString(), encoding: defaultEncoding);
                fileWriteTimer.Stop();
                Console.WriteLine("Took {0} ({1,7:N2}ms) - to write file to disk", fileWriteTimer.Elapsed, fileWriteTimer.ElapsedMilliseconds);
                Console.WriteLine("Generated file: {0}\n", fileName);
            }
            return generatedRunners;
        }

        private SyntaxTree GenerateLauncher(string outputDirectory)
        {
            var generatedLauncher = BenchmarkTemplate.ProcessLauncherTemplate();
            var outputFileName = Path.Combine(outputDirectory, launcherFileName);
            var codeGenTimer = Stopwatch.StartNew();
            var generatedLauncherTree = CSharpSyntaxTree.ParseText(generatedLauncher, options: parseOptions, path: outputFileName, encoding: defaultEncoding);
            codeGenTimer.Stop();
            Console.WriteLine("Took {0} ({1,7:N2}ms) - to generate CSharp Syntax Tree", codeGenTimer.Elapsed, codeGenTimer.ElapsedMilliseconds);

            var fileWriteTimer = Stopwatch.StartNew();
            File.WriteAllText(outputFileName, generatedLauncherTree.GetRoot().ToFullString(), encoding: defaultEncoding);
            fileWriteTimer.Stop();
            Console.WriteLine("Took {0} ({1,7:N2}ms) - to write file to disk", fileWriteTimer.Elapsed, fileWriteTimer.ElapsedMilliseconds);
            Console.WriteLine("Generated file: " + launcherFileName);

            return generatedLauncherTree;
        }

        private IEnumerable<SyntaxTree> GenerateEmbeddedCode()
        {
            var embeddedCodeTrees = new List<SyntaxTree>();
            var assembly = Assembly.GetExecutingAssembly();
            foreach (var codeFile in assembly.GetManifestResourceNames())
            {
                if (codeFile.StartsWith("MiniBench.Core.") == false &&
                    codeFile.StartsWith("MiniBench.Profiling.") == false)
                    continue;

                using (Stream stream = assembly.GetManifestResourceStream(codeFile))
                using (var reader = new StreamReader(stream))
                {
                    string result = reader.ReadToEnd();
                    // By adding a "virtual" path we can match up the errors/warnings (in the VS Output Window) with the embedded resource .cs file
                    var codeTree = CSharpSyntaxTree.ParseText(result, options: parseOptions, path: codeFile, encoding: defaultEncoding);
                    embeddedCodeTrees.Add(codeTree);
                }
            }

            return embeddedCodeTrees;
        }

        private void CompileAndEmitCode(IEnumerable<SyntaxTree> allSyntaxTrees)
        {
            // As we're adding out own Main Function, we always Compile as OutputKind.ConsoleApplication, regardless of the actual extension (dll/exe)
            var compilationOptions = new CSharpCompilationOptions(
                                            outputKind: OutputKind.ConsoleApplication,
                                            mainTypeName: "MiniBench.Benchmarks.Program",
                                            optimizationLevel: OptimizationLevel.Release);

            // One call here will be sloooowww (probably Create() or Emit()), because it causes a load/JIT of certain parts of Roslyn
            // see https://roslyn.codeplex.com/discussions/573503 for a full explanation (JITting of Roslyn dll's is the main cause)

            var compilationTimer = Stopwatch.StartNew();
            var compilation = CSharpCompilation.Create(projectSettings.OutputFileName, allSyntaxTrees, GetRequiredReferences(), compilationOptions);
            compilationTimer.Stop();
            Console.WriteLine("\nTook {0} ({1,7:N2}ms) - to create the CSharpCompilation", compilationTimer.Elapsed, compilationTimer.ElapsedMilliseconds);
            Console.WriteLine("\nCurrent directory: " + Environment.CurrentDirectory);

            // TODO fix this IOException (happens if the file is still being used whilst we are trying to "re-write" it)
            //  Unhandled Exception: System.IO.IOException: The process cannot access the file '....MiniBench.Demo.dll' because it is being used by another process.

            // TODO we should probably emit to a .temp file, than only if it's successful copy that over the top of the existing file (and delete the .temp file)
            // that way, if something goes wrong the original binaries are left in-tact and we never emit invalid files

            var codeEmitToDiskTimer = Stopwatch.StartNew();
            var emitToDiskResult = compilation.Emit(outputPath: projectSettings.OutputFileName + projectSettings.OutputFileExtension,
                                                    pdbPath: projectSettings.OutputFileName + ".pdb",
                                                    xmlDocPath: projectSettings.OutputFileName + ".xml");
            codeEmitToDiskTimer.Stop();
            Console.WriteLine("Took {0} ({1,7:N2}ms) - to emit generated code to DISK", codeEmitToDiskTimer.Elapsed, codeEmitToDiskTimer.ElapsedMilliseconds);
            Console.WriteLine("Emit to DISK Success: {0}", emitToDiskResult.Success);
            if (emitToDiskResult.Diagnostics.Length > 0)
            {
                Console.WriteLine("\nCompilation Warnings:\n\t{0}\n", string.Join("\n\t", emitToDiskResult.Diagnostics));
            }
        }

        private bool ShouldGenerateBlackhole(TypeSyntax returnType)
        {
            // If the method returns void, double, etc, then the type will be "PredefinedTypeSyntax"
            var predefinedTypeSyntax = returnType as PredefinedTypeSyntax;
            if (predefinedTypeSyntax != null && predefinedTypeSyntax.Keyword.IsKind(SyntaxKind.VoidKeyword) == false)
                return true;

            // If the method returns DateTime, String, etc, then the type will be "IdentifierNameSyntax"
            var identifierNameSyntax = returnType as IdentifierNameSyntax;
            if (identifierNameSyntax != null && identifierNameSyntax.IsKind(SyntaxKind.VoidKeyword) == false)
                return true;

            // If we don't know, return false?
            return false;
        }

        private IEnumerable<MetadataReference> GetRequiredReferences()
        {
            var standardReferences = new List<MetadataReference>(16);
            if (projectSettings.TargetFrameworkVersion == LanguageVersion.CSharp2 ||
                projectSettings.TargetFrameworkVersion == LanguageVersion.CSharp3)
            {
                // We have to read the dll's from disk as a Stream and create a MetadataReference from that.
                // If we use MetadataReference.CreateFromAssembly(..) the .NET 4.0 versions are used instead.
                var runtimeDlls = new[]
                    {
                        // TODO C:\Windows\Microsoft.NET\Framework64 or just \Framework ?!?
                        @"C:\Windows\Microsoft.NET\Framework\v2.0.50727\mscorlib.dll",
                        @"C:\Windows\Microsoft.NET\Framework\v2.0.50727\System.dll"
                    };

                foreach (var runtimeDll in runtimeDlls)
                {
                    using (var fileStream = File.OpenRead(runtimeDll))
                    {
                        standardReferences.Add(MetadataReference.CreateFromStream(fileStream, filePath: runtimeDll));
                    }
                }
            }
            else
            {
                // As MiniBench.exe runs as a .NET 4.0 (or 4.5) process (due to the Roslyn dependancy)
                // We can just get the .NET 4.0 runtimes components in the normal way
                // Using typeof(..) means it get's the best match for us, for instance from the GAC

                // This pulls in mscorlib.dll 
                standardReferences.Add(MetadataReference.CreateFromAssembly(typeof(String).Assembly));
                // This pulls in System.dll
                standardReferences.Add(MetadataReference.CreateFromAssembly(typeof(Stopwatch).Assembly));
                // This pulls in System.Core, i.e. the stuff you need for LINQ
                standardReferences.Add(MetadataReference.CreateFromAssembly(typeof(System.Linq.Enumerable).Assembly));
            }

            // Now add the references we need from the .csproj file
            foreach (var reference in projectSettings.References)
            {
                // TODO we need to handle references that don't have a "HintPath", i.e. things like:
                // <Reference Include="System" />
                // <Reference Include="System.Data" />
                // <Reference Include="System.Xml" />
                // TODO At the moment we only deal with ones like this:
                // <Reference Include="xunit">
                //     <HintPath>..\packages\xunit.1.9.2\lib\net20\xunit.dll</HintPath>
                // </Reference>
                standardReferences.Add(MetadataReference.CreateFromFile(Path.GetFullPath(Path.Combine(projectSettings.RootFolder, reference.Item2))));
            }

            Console.WriteLine("\nAdding References:\n\t" + String.Join("\n\t", standardReferences.Select(r => r.Display)));

            return standardReferences;
        }

        private static IList<T> NodesOfType<T>(SyntaxTree tree)
        {
            return tree.GetRoot()
                    .DescendantNodes()
                    .OfType<T>()
                    .ToList();
        }
    }
}
