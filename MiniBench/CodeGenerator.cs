using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MiniBench.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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
            allSyntaxTrees.AddRange(GenerateRunners(allMethods, namespaceName, className, outputDirectory));
            allSyntaxTrees.Add(GenerateLauncher(outputDirectory));

            CompileAndEmitCode(allSyntaxTrees, emitToDisk: true);
        }

        private List<SyntaxTree> GenerateRunners(IList<MethodDeclarationSyntax> methods, string namespaceName, string className, string outputDirectory)
        {
            var filePrefix = "Generated_Runner";
            foreach (var existingGeneratedFile in Directory.EnumerateFiles(outputDirectory, filePrefix + "*"))
            {
                File.Delete(existingGeneratedFile);
            }

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
                var generatedBenchmark = BenchmarkTemplate.ProcessTemplates(namespaceName, className, methodName, generatedClassName);
                var generatedRunnerTree = CSharpSyntaxTree.ParseText(generatedBenchmark, options: parseOptions);
                generatedRunners.Add(generatedRunnerTree);
                var fileName = string.Format(generatedClassName + ".cs");
                File.WriteAllText(Path.Combine(outputDirectory, fileName), generatedRunnerTree.GetRoot().ToFullString());
                Console.WriteLine("Wrote generated file: " + fileName);
            }
            return generatedRunners;
        }

        private SyntaxTree GenerateLauncher(string outputDirectory)
        {
            //var launcherCode = string.Format("new {0}().RunTest(TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(10))", generatedClassName);
            var launcherCode = "";
            var generatedLauncher = BenchmarkTemplate.benchmarkLauncherTemplate
                                .Replace(BenchmarkTemplate.launcherReplaceText, launcherCode);
            var generatedLauncherTree = CSharpSyntaxTree.ParseText(generatedLauncher, options: parseOptions);
            var fileName = "Generated_Launcher.cs";
            File.WriteAllText(Path.Combine(outputDirectory, fileName), generatedLauncherTree.GetRoot().ToFullString());
            Console.WriteLine("Wrote generated file: " + fileName);

            return generatedLauncherTree;
        }

        private List<SyntaxTree> GenerateEmbeddedCode()
        {
            var embeddedCodeFiles = new[] { "BenchmarkAttribute.cs", "BenchmarkResult.cs", "CategoryAttribute.cs", "IBenchmarkTarget.cs" };
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
                                            optimizationLevel: OptimizationLevel.Release);
            var generatedCodeName = "Benchmark";
            var compilation = CSharpCompilation.Create(generatedCodeName, allSyntaxTrees, GetRequiredReferences(), compilationOptions);
            var result = compilation.Emit(peStream: peStream, pdbStream: pdbStream, cancellationToken: CancellationToken.None);
            Console.WriteLine("Emit in-memory Success: {0}\n  {1}", result.Success, string.Join("\n  ", result.Diagnostics));

            if (emitToDisk)
            {
                // Write them to disk for debugging
                Console.WriteLine("Current directory: " + Environment.CurrentDirectory);
                var emitToDiskResult = compilation.Emit(outputPath: generatedCodeName + ".exe",
                                                        pdbPath: generatedCodeName + ".pdb",
                                                        xmlDocumentationPath: generatedCodeName + ".xml");
                Console.WriteLine("Emit to disk   Success: {0}", emitToDiskResult.Success);
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
                    MetadataReference.CreateFromAssembly(typeof(System.Diagnostics.Stopwatch).Assembly)
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
