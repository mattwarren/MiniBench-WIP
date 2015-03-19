using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp;

namespace MiniBench
{
    internal class ProjectSettings
    {
        public string RootFolder { get; private set; }

        public string ProjectPath { get; private set; }

        public string OutputFileName { get; private set; }

        public string OutputFileExtension { get; private set; }

        public LanguageVersion TargetFrameworkVersion { get; private set; }

        public IEnumerable<string> SourceFiles { get; private set; }

        public IEnumerable<Tuple<string, string>> References { get; private set; }
        
        public ProjectSettings(string projectPath, string rootFolder, string outputFileName, 
                               string outputFileExtension, LanguageVersion targetFrameworkVersion,
                               IEnumerable<String> sourceFiles, IEnumerable<Tuple<String, String>> references)
        {
            ProjectPath = projectPath;
            RootFolder = rootFolder;
            OutputFileName = outputFileName;
            OutputFileExtension = outputFileExtension;
            TargetFrameworkVersion = targetFrameworkVersion;
            References = references;
            SourceFiles = sourceFiles;
        }
    }
}