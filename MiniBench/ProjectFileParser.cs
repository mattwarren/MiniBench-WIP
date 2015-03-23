using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace MiniBench
{
    internal class ProjectFileParser
    {
        internal ProjectSettings ParseProjectFile(string csprojPath)
        {
            // From http://stackoverflow.com/questions/4649989/reading-a-csproj-file-in-c-sharp/4650090#4650090
            XmlDocument xmldoc = new XmlDocument();
            xmldoc.Load(csprojPath);

            XmlNamespaceManager mgr = new XmlNamespaceManager(xmldoc.NameTable);
            mgr.AddNamespace("x", "http://schemas.microsoft.com/developer/msbuild/2003");

            Console.WriteLine("Reading from " + csprojPath);

            return new ProjectSettings
            (
                csprojPath,
                Path.GetDirectoryName(csprojPath),
                GetOutputFileName(xmldoc, mgr),
                GetOutputFileExtension(xmldoc, mgr),
                GetTargetFrameworkVersion(xmldoc, mgr),
                GetSourceFiles(xmldoc, mgr),
                GetReferences(xmldoc, mgr)
            );
        }


        private String GetOutputFileName(XmlDocument xmldoc, XmlNamespaceManager mgr)
        {
            XmlNode item = xmldoc.SelectSingleNode("//x:AssemblyName", mgr);
            Console.WriteLine("AssemblyName: " + (item != null ? item.InnerText : "<NULL>"));
            if (item != null)
            {
                return item.InnerText;
            }

            return "Unknown";
        }

        private String GetOutputFileExtension(XmlDocument xmldoc, XmlNamespaceManager mgr)
        {
            var fileExtension = ".Unknown";
            XmlNode item = xmldoc.SelectSingleNode("//x:OutputType", mgr);
            if (item != null)
            {
                if (item.InnerText == "Exe" || item.InnerText == "WinExe")
                    fileExtension = ".exe";
                else if (item.InnerText == "Library")
                    fileExtension = ".dll";
            }

            Console.WriteLine("FileExtension: {0} -> {1}", (item != null ? item.InnerText : "<NULL>"), fileExtension);
            return fileExtension;
        }

        private LanguageVersion GetTargetFrameworkVersion(XmlDocument xmldoc, XmlNamespaceManager mgr)
        {
            // Default to CSharp2
            var languageVersion = LanguageVersion.CSharp2;
            XmlNode item = xmldoc.SelectSingleNode("//x:TargetFrameworkVersion", mgr);
            if (item != null)
            {
                if (item.InnerText.StartsWith("v1."))
                    languageVersion = LanguageVersion.CSharp1;
                else if (item.InnerText.StartsWith("v2."))
                    languageVersion = LanguageVersion.CSharp2;
                else if (item.InnerText.StartsWith("v3."))
                    languageVersion = LanguageVersion.CSharp3;
                else if (item.InnerText.StartsWith("v4."))
                    languageVersion = LanguageVersion.CSharp4;
                else if (item.InnerText.StartsWith("v5."))
                    languageVersion = LanguageVersion.CSharp5;
                else if (item.InnerText.StartsWith("v6."))
                    languageVersion = LanguageVersion.CSharp6;
            }

            Console.WriteLine("TargetFrameworkVersion: {0} -> {1}", (item != null ? item.InnerText : "<NULL>"), languageVersion);
            return languageVersion;
        }

        private IEnumerable<string> GetSourceFiles(XmlDocument xmldoc, XmlNamespaceManager mgr)
        {
            var sourceFiles = new List<String>();
            Console.WriteLine("Source Files:");
            foreach (XmlNode item in xmldoc.SelectNodes("//x:Compile", mgr))
            {
                var includeAttrribute = item.Attributes["Include"];
                if (includeAttrribute == null)
                    continue;

                Console.WriteLine("\t" + includeAttrribute.Value);
                sourceFiles.Add(includeAttrribute.Value);
            }
            return sourceFiles;
        }

        private IEnumerable<Tuple<string, string>> GetReferences(XmlDocument xmldoc, XmlNamespaceManager mgr)
        {
            var references = new List<Tuple<String, String>>();
            // Why doesn't "//x:ItemGroup/Reference" work, to find Reference Nodes under ItemGroup nodes?
            Console.WriteLine("References:");
            foreach (XmlNode item in xmldoc.SelectNodes("//x:Reference", mgr))
            {
                var includeAttrribute = item.Attributes["Include"];
                if (includeAttrribute == null || item.ChildNodes.Count == 0)
                    continue;

                XmlNode hintPath = includeAttrribute.SelectSingleNode("//x:HintPath", mgr);
                Console.WriteLine("\t{0} {1}", includeAttrribute.Value, hintPath != null ? hintPath.InnerText : "<null>");
                references.Add(Tuple.Create(includeAttrribute.Value, hintPath != null ? hintPath.InnerText : null));
            }
            return references;
        }
    }
}
