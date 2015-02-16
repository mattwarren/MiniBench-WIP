using System;
using System.Collections.Generic;
using System.Xml;

namespace MiniBench
{
    internal class ProjectFileParser
    {
        internal IList<Tuple<String, String>> GetReferences(string csprojPath)
        {
            // From http://stackoverflow.com/questions/4649989/reading-a-csproj-file-in-c-sharp/4650090#4650090
            XmlDocument xmldoc = new XmlDocument();
            xmldoc.Load(csprojPath);

            XmlNamespaceManager mgr = new XmlNamespaceManager(xmldoc.NameTable);
            mgr.AddNamespace("x", "http://schemas.microsoft.com/developer/msbuild/2003");

            var references = new List<Tuple<String, String>>();
            Console.WriteLine("References from " + csprojPath);
            // Why doesn't "//x:ItemGroup/Reference" work, to find Reference Nodes under ItemGroup nodes?
            foreach (XmlNode item in xmldoc.SelectNodes("//x:Reference", mgr))
            {
                var includeAttrribute = item.Attributes["Include"];
                if (includeAttrribute == null || item.ChildNodes.Count == 0)
                    continue;

                foreach (XmlNode childNode in item.ChildNodes)
                {
                    Console.WriteLine("\t" + includeAttrribute.Value + " " + childNode.InnerText);
                    references.Add(Tuple.Create(includeAttrribute.Value, childNode.InnerText));
                }
            }
            return references;
        }

        internal IList<String> GetSourceFiles(string csprojPath)
        {
            // From http://stackoverflow.com/questions/4649989/reading-a-csproj-file-in-c-sharp/4650090#4650090
            XmlDocument xmldoc = new XmlDocument();
            xmldoc.Load(csprojPath);

            XmlNamespaceManager mgr = new XmlNamespaceManager(xmldoc.NameTable);
            mgr.AddNamespace("x", "http://schemas.microsoft.com/developer/msbuild/2003");

            Console.WriteLine("Source Files from " + csprojPath);
            var sourceFiles = new List<String>();
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
    }
}
