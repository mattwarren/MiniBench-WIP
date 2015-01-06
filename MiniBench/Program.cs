using System;
using System.Security.Policy;
using MiniBench.Core;
using System.Reflection;
using System.IO;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp;
using System.Threading;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MiniBench
{
    /// <summary>
    /// Command-line interface for MiniBench
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            string fileToBenchmark = args[1];
            string extension = Path.GetExtension(fileToBenchmark);

            AppDomain domain = AppDomain.CreateDomain("MiniBench runner", new Evidence(), Environment.CurrentDirectory, Environment.CurrentDirectory, shadowCopyFiles: false);
            var loader = CreateInstance<AssemblyLoader>(domain);

            Assembly loadedAssembly = null;
            if (extension == ".dll")
            {
                AssemblyName assembly = AssemblyName.GetAssemblyName(fileToBenchmark);
                Console.WriteLine("Loading Benchmark Assembly from disk: {0}\n", assembly.FullName);
                loadedAssembly = loader.Load(assembly.FullName); // fileToBenchmark);
            }
            else if (extension == ".cs")
            {
                var peStream = new MemoryStream();
                var pdbMemoryStream = new MemoryStream();
                Console.WriteLine("Compiling Benchmark code into an Assembly: {0}\n", fileToBenchmark);
                CompileCode(File.ReadAllText(fileToBenchmark), peStream, pdbMemoryStream);
                loadedAssembly = loader.Load(rawAssembly: peStream.GetBuffer(), rawSymbolStore: pdbMemoryStream.GetBuffer());
            }

            var probe = CreateInstance<BenchmarkProbe>(domain);
            BenchmarkTarget[] targets = probe.Probe(loadedAssembly);

            foreach (var target in targets)
            {
                var result = target.RunTest(TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(10));
                Console.WriteLine(result);
            }
        }

        static void CompileCode(string script, MemoryStream peStream, MemoryStream pdbStream)
        {
            var parseOptions = new CSharpParseOptions(kind: SourceCodeKind.Regular, 
                                                      languageVersion: LanguageVersion.CSharp2);
            var benchmarkTree = CSharpSyntaxTree.ParseText(script, options: parseOptions);

            // TODO get Namespace, Class and Method names dynamicially from the Benchmark code!
            var namespaceName = "MiniBench.Demo";
            var className = "SampleBenchmark"; 
            var methodName = "DateTimeNow";
            // Can't have '.' or '-' in class names (which is where this gets used)
            var generatedClassName = string.Format("Generated_Runner_{0}_{1}_{2}", 
                                        namespaceName.Replace('.', '_'), 
                                        className, 
                                        methodName); 
            var generatedBenchmark = BenchmarkTemplate.benchmarkHarnessTemplate
                                .Replace(BenchmarkTemplate.namespaceReplaceText, namespaceName)
                                .Replace(BenchmarkTemplate.classReplaceText, className)
                                .Replace(BenchmarkTemplate.methodReplaceText, methodName)
                                .Replace(BenchmarkTemplate.methodParametersReplaceText, "")
                                .Replace(BenchmarkTemplate.generatedClassReplaceText, generatedClassName);
            //Console.WriteLine("Generated benchmark runner:\n{0}\n", benchmark);
            var generatedRunnerTree = CSharpSyntaxTree.ParseText(generatedBenchmark, options: parseOptions);
            var fileName = string.Format(generatedClassName + ".cs");
            File.WriteAllText(fileName, generatedRunnerTree.GetRoot().ToFullString());

            var embeddedCodeFiles = new[] { "BenchmarkAttribute.cs", "BenchmarkResult.cs", "CategoryAttribute.cs", "IBenchmarkTarget.cs" };
            var embeddedCodeTrees = new List<SyntaxTree>();
            foreach (var codeFile in embeddedCodeFiles)
            {
                var codeText = GetEmbeddedResource("MiniBench.Core." + codeFile);
                var codeTree = CSharpSyntaxTree.ParseText(codeText, options: parseOptions);
                embeddedCodeTrees.Add(codeTree);
            }

            var launcherCode = string.Format("new {0}().RunTest(TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(10))", generatedClassName);
            var generatedLauncher = BenchmarkTemplate.benchmarkLauncherTemplate
                                .Replace(BenchmarkTemplate.launcherReplaceText, launcherCode);
            var generatedLauncherTree = CSharpSyntaxTree.ParseText(generatedLauncher, options: parseOptions);
            File.WriteAllText("GeneratedLauncher.cs", generatedLauncherTree.GetRoot().ToFullString());

            //Console.WriteLine("BEFORE:\n{0}\n", benchmarkTree.GetRoot().ToFullString());
            //File.WriteAllText("Input.cs", benchmarkTree.GetRoot().ToFullString());
            //var modifiedTree = MethodRemoval.ProcessTree(benchmarkTree);
            //Console.WriteLine("AFTER:\n{0}\n", modifiedTree.GetRoot().ToFullString());
            //File.WriteAllText("Generated.cs", modifiedTree.GetRoot().ToFullString());

            // TODO Maybe re-write this using the Fluent-API, as shown here 
            // http://roslyn.codeplex.com/discussions/541557

            // Breaking change in the latest Roslyn API, see http://roslyn.codeplex.com/discussions/572265
            //var references = new List<MetadataReference> { new MetadataFileReference(typeof(String).Assembly.Location) };
            var references = new List<MetadataReference> 
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

                MetadataReference.CreateFromAssembly(typeof(System.Diagnostics.Stopwatch).Assembly)

                // We don't reference this, we embeded it directly in the dll we emit, so everything is self-contained
                //MetadataReference.CreateFromAssembly(typeof(MiniBench.Core.BenchmarkAttribute).Assembly),
            };
            
            var compilationOptions = new CSharpCompilationOptions(
                                            //outputKind: OutputKind.DynamicallyLinkedLibrary,
                                            outputKind: OutputKind.ConsoleApplication,
                                            optimizationLevel: OptimizationLevel.Release);
            //var compilation = CSharpCompilation.Create("Testing", new[] { modifiedTree }, references, compilationOptions);
            var allSyntaxTrees = new List<SyntaxTree>(embeddedCodeTrees);
            allSyntaxTrees.Add(benchmarkTree);
            allSyntaxTrees.Add(generatedRunnerTree);
            allSyntaxTrees.Add(generatedLauncherTree);
            //var syntaxTrees = new[] { benchmarkTree, generatedRunnerTree, generatedLauncherTree };
            var generatedCodeName = "Benchmark";
            var compilation = CSharpCompilation.Create(generatedCodeName, allSyntaxTrees, references, compilationOptions);
            //var assembly = compilation.Assembly;

            // Write them to disk for debugging
            Console.WriteLine("Current directory: " + Environment.CurrentDirectory);
            var emitToDiskResult = compilation.Emit(outputPath: generatedCodeName + ".exe", // ".dll", 
                                                    pdbPath: generatedCodeName + ".pdb", 
                                                    xmlDocumentationPath: generatedCodeName + ".xml");
            Console.WriteLine("Emit to disk   Success: {0}", emitToDiskResult.Success);

            var result = compilation.Emit(peStream: peStream, pdbStream: pdbStream, cancellationToken: CancellationToken.None);
            Console.WriteLine("Emit in-memory Success: {0}\n  {1}", result.Success, string.Join("\n  ", result.Diagnostics));
            Console.WriteLine("\npeStream position = {0:N0} (length = {1:N0}), pdbStream position = {2:N0} (length = {3:N0})\n",
                                peStream.Position, peStream.Length, pdbStream.Position, pdbStream.Length);
        }

        private static T CreateInstance<T>(AppDomain domain)
        {
            Type type = typeof(T);
            return (T) domain.CreateInstanceAndUnwrap(type.Assembly.FullName, type.FullName);
        }

        private static string GetEmbeddedResource(string resourceFullPath)
        {
            var assembly = Assembly.GetExecutingAssembly();
            //var resourceName = "MyCompany.MyProduct.MyFile.txt";

            using (Stream stream = assembly.GetManifestResourceStream(resourceFullPath))
            using (StreamReader reader = new StreamReader(stream))
            {
                string result = reader.ReadToEnd();
                return result;
            }
        }
    }

    //public class MethodRemoval : CSharpSyntaxRewriter
    //{
    //    public static SyntaxTree ProcessTree(SyntaxTree tree)
    //    {
    //        var rewriter = new MethodRemoval();
    //        var result = rewriter.Visit(tree.GetRoot());
    //        return result.SyntaxTree;
    //    }

    //    public override SyntaxNode VisitMethodDeclaration(MethodDeclarationSyntax node)
    //    {
    //        if (node.Identifier.ValueText.Contains("DateTimeUtcNow"))
    //            return null;
    //     return base.VisitMethodDeclaration(node);
    //   }
    //}
}
