using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MiniBench.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;

namespace MiniBench
{
    internal class CodeGenerator
    {
        private readonly MemoryStream peStream;
        private readonly MemoryStream pdbStream;

        private readonly CSharpParseOptions parseOptions = 
            new CSharpParseOptions(
                    kind: SourceCodeKind.Regular,
                    languageVersion: LanguageVersion.CSharp2);

        private readonly Encoding defaultEncoding = Encoding.UTF8;

        internal CodeGenerator(MemoryStream peStream, MemoryStream pdbStream)
        {
            this.peStream = peStream;
            this.pdbStream = pdbStream;
        }

        internal void GenerateCode(string code)
        {
            var benchmarkTree = CSharpSyntaxTree.ParseText(code, options: parseOptions);

            // TODO error cheecking, in case the file doesn't have a Namespace, Class or any valid Methods!
            var @namespace = NodesOfType<NamespaceDeclarationSyntax>(benchmarkTree).FirstOrDefault();
            var namespaceName = @namespace.Name.ToString();
            var @class = NodesOfType<ClassDeclarationSyntax>(benchmarkTree).FirstOrDefault();
            var className = @class.Identifier.ToString();
            var allMethods = NodesOfType<MethodDeclarationSyntax>(benchmarkTree);

            var outputDirectory = Environment.CurrentDirectory;
            var allSyntaxTrees = new List<SyntaxTree>(GenerateEmbeddedCode());
            allSyntaxTrees.Add(benchmarkTree);

            var generatedRunners = GenerateRunners(allMethods, namespaceName, className, outputDirectory);
            allSyntaxTrees.AddRange(generatedRunners);

            var generatedLauncher = GenerateLauncher(outputDirectory);
            allSyntaxTrees.Add(generatedLauncher);

            CompileAndEmitCode(allSyntaxTrees, emitToDisk: true);
        }

        private List<SyntaxTree> GenerateRunners(IList<MethodDeclarationSyntax> methods, string namespaceName, string className, string outputDirectory)
        {
            var filePrefix = "Generated_Runner";
            var fileDeletionTimer = Stopwatch.StartNew();
            foreach (var existingGeneratedFile in Directory.EnumerateFiles(outputDirectory, filePrefix + "*"))
            {
                File.Delete(existingGeneratedFile);
            }
            fileDeletionTimer.Stop();
            Console.WriteLine("Took {0} ({1,5:N0}ms) - to delete existing files from disk", fileDeletionTimer.Elapsed, fileDeletionTimer.ElapsedMilliseconds);

            var modifiers = methods.Select(m => m.Modifiers).ToList();
            var benchmarkAttribute = typeof(BenchmarkAttribute).Name.Replace("Attribute", "");
            var validMethods = methods.Where(m => m.Modifiers.Any(mod => mod.CSharpKind() == SyntaxKind.PublicKeyword))
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
                var generatedBenchmark = BenchmarkTemplate.ProcessCodeTemplates(namespaceName, className, methodName, generatedClassName);
                var generatedRunnerTree = CSharpSyntaxTree.ParseText(generatedBenchmark, options: parseOptions, path: outputFileName, encoding: defaultEncoding);
                generatedRunners.Add(generatedRunnerTree);
                codeGenTimer.Stop();
                Console.WriteLine("Took {0} ({1,5:N0}ms) - to generate CSharp Syntx Tree", codeGenTimer.Elapsed, codeGenTimer.ElapsedMilliseconds);

                var fileWriteTimer = Stopwatch.StartNew();
                File.WriteAllText(outputFileName, generatedRunnerTree.GetRoot().ToFullString(), encoding: defaultEncoding);
                fileWriteTimer.Stop();
                Console.WriteLine("Generated file: " + fileName);
                Console.WriteLine("Took {0} ({1,5:N0}ms) - to write file to disk", fileWriteTimer.Elapsed, fileWriteTimer.ElapsedMilliseconds);
            }
            return generatedRunners;
        }

        private SyntaxTree GenerateLauncher(string outputDirectory)
        {
            var generatedLauncher = BenchmarkTemplate.ProcessLauncherTemplate();
            var fileName = "Generated_Launcher.cs";
            var outputFileName = Path.Combine(outputDirectory, fileName);
            var generatedLauncherTree = CSharpSyntaxTree.ParseText(generatedLauncher, options: parseOptions, path: outputFileName, encoding: defaultEncoding);
            File.WriteAllText(outputFileName, generatedLauncherTree.GetRoot().ToFullString(), encoding: defaultEncoding);
            Console.WriteLine("Generated file: " + fileName);

            return generatedLauncherTree;
        }

        private List<SyntaxTree> GenerateEmbeddedCode()
        {
            // TODO Maybe make this list automatic, i.e. search for all Embedded Resources in the "MiniBench.Core" namespace?
            var embeddedCodeFiles = new[] 
                {
                    "BenchmarkAttribute.cs", "CategoryAttribute.cs", 
                    "IBenchmarkTarget.cs", "BenchmarkResult.cs", 
                    "Options.cs", "OptionsBuilder.cs", "Runner.cs",
                };
            var embeddedCodeTrees = new List<SyntaxTree>();
            foreach (var codeFile in embeddedCodeFiles)
            {
                var codeText = GetEmbeddedResource("MiniBench.Core." + codeFile);
                var codeTree = CSharpSyntaxTree.ParseText(codeText, options: parseOptions);
                embeddedCodeTrees.Add(codeTree);
            }

            return embeddedCodeTrees;
        }

        private void CompileAndEmitCode(List<SyntaxTree> allSyntaxTrees, bool emitToDisk)
        {
            // TODO Maybe re-write this using the Fluent-API, as shown here  http://roslyn.codeplex.com/discussions/541557
            var compilationOptions = new CSharpCompilationOptions(
                                            outputKind: OutputKind.ConsoleApplication,
                                            mainTypeName: "MiniBench.Benchmarks.Program",
                                            optimizationLevel: OptimizationLevel.Release);
            // TODO decide if this is a good idea or not?? We are re-writing over the top of the file that VS has just built!
            // But it does mean that you can use the Test Runner in VS to run the Integration Tests :-)
            var generatedCodeName = "MiniBench.Demo"; // "Benchmark";

            // One call here will be sloooowww (probably Create() or Emit()), because it causes a load/JIT of certain parts of Roslyn
            // see https://roslyn.codeplex.com/discussions/573503 for a full explanation (JITting of Roslyn dll's is the main cause)

            var compilationTimer = Stopwatch.StartNew();
            var compilation = CSharpCompilation.Create(generatedCodeName, allSyntaxTrees, GetRequiredReferences(), compilationOptions);
            compilationTimer.Stop();
            Console.WriteLine("Took {0} ({1,5:N0}ms) - to create the CSharpCompilation", compilationTimer.Elapsed, compilationTimer.ElapsedMilliseconds);

            //var codeEmitInMemoryTimer = Stopwatch.StartNew();
            //var emitInMemoryResult = compilation.Emit(peStream: peStream, pdbStream: pdbStream, cancellationToken: CancellationToken.None);
            //codeEmitInMemoryTimer.Stop();
            //// Don't print diagnostics here, let it be done when we emit to disk, i.e. the real thing
            ////Console.WriteLine("Emit in-memory Success: {0}\n  {1}", emitInMemoryResult.Success, string.Join("\n  ", emitInMemoryResult.Diagnostics));
            //Console.WriteLine("Emit in-memory Success: {0}", emitInMemoryResult.Success);
            //Console.WriteLine("Took {0} ({1,5:N0}ms) - to emit all generated code IN-MEMORY", codeEmitInMemoryTimer.Elapsed, codeEmitInMemoryTimer.ElapsedMilliseconds);

            if (emitToDisk)
            {
                Console.WriteLine("\nCurrent directory: " + Environment.CurrentDirectory);
                var codeEmitToDiskTimer = Stopwatch.StartNew();
                var emitToDiskResult = compilation.Emit(outputPath: generatedCodeName + ".exe",
                                                        pdbPath: generatedCodeName + ".pdb",
                                                        xmlDocumentationPath: generatedCodeName + ".xml");
                codeEmitToDiskTimer.Stop();
                Console.WriteLine("Emit to DISK Success: {0}\n  {1}", emitToDiskResult.Success, string.Join("\n  ", emitToDiskResult.Diagnostics));
                Console.WriteLine("Took {0} ({1,5:N0}ms) - to emit generated code to DISK", codeEmitToDiskTimer.Elapsed, codeEmitToDiskTimer.ElapsedMilliseconds);
            }
        }

        private static IList<MetadataReference> GetRequiredReferences()
        {
            return new List<MetadataReference>
                { 
                    // TODO need to get a reference to mscorlib v 2.0 here, 
                    // can't use "typeof(String).Assembly" as that's v 4.0 (because MiniBench itself is 4.0)
                    //  warning CS1701: Assuming assembly reference 
                    //'mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089' used by 'MiniBench.Core' matches identity 
                    //'mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089' of 'mscorlib', you may need to supply runtime policy
                    // see http://source.roslyn.codeplex.com/#Roslyn.Compilers.CSharp.Symbol.UnitTests/Compilation/ReferenceManagerTests.cs
                    // and http://source.roslyn.codeplex.com/#Roslyn.Test.Utilities/TestBase.cs,d7b20a77e5912080
                    // and http://source.roslyn.codeplex.com/#Roslyn.Compilers.CSharp.Symbol.UnitTests/Compilation/ReferenceManagerTests.cs,a90d68f18e804c1a,references

                    //MetadataReference.CreateFromAssembly(typeof(String).Assembly),
                    //MetadataReference.CreateFromAssembly(Assembly.Load("mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089")),
                    MetadataReference.CreateFromAssembly(Assembly.LoadFile(@"C:\Windows\Microsoft.NET\Framework64\v2.0.50727\mscorlib.dll")),
                    //MetadataReference.CreateFromAssembly(Assembly.LoadFrom(@"C:\Windows\Microsoft.NET\Framework64\v2.0.50727\mscorlib.dll")),
                    //MetadataReference.CreateFromAssembly(Assembly.LoadFrom(@"C:\Windows\Microsoft.NET\Framework\v2.0.50727\mscorlib.dll")),

                    // In here we include any other parts of the .NET framework that we need
                    MetadataReference.CreateFromAssembly(typeof(System.Diagnostics.Stopwatch).Assembly),

                    // TODO in here, we need to detect that the SampleBenchmark is using Xunit and include it
                    // Need a general way, for instance the benchmark can include any 3rd party references it wants! 
                    // Maybe supply the .csproj file on the command-line and read the dependencies from that?!?!
                    MetadataReference.CreateFromFile(@"C:\Users\warma11\Downloads\__GitHub__\MiniBench-WIP\MiniBench.Demo\..\packages\xunit.1.9.2\lib\net20\xunit.dll")
                };
        }

        private static string GetEmbeddedResource(string resourceFullPath)
        {
            var assembly = Assembly.GetExecutingAssembly();
            using (Stream stream = assembly.GetManifestResourceStream(resourceFullPath))
            using (StreamReader reader = new StreamReader(stream))
            {
                string result = reader.ReadToEnd();
                return result;
            }
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
