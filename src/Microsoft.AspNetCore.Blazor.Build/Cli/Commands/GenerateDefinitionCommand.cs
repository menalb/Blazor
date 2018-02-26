// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Blazor.Razor;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Razor;
using Microsoft.Extensions.CommandLineUtils;

namespace Microsoft.AspNetCore.Blazor.Build.Cli.Commands
{
    internal static class GenerateDefinitionCommand
    {
        public static void Command(CommandLineApplication command)
        {
            var sourceFilePaths = command.Option(
                "--source",
                "Blazor component source file path.",
                CommandOptionType.MultipleValue);
            var outputFilePaths = command.Option(
                "--output",
                "Blazor component declaration output file path.",
                CommandOptionType.MultipleValue);
            var referencePaths = command.Option(
                "--reference",
                "Reference assembly for the application, used to discovery components",
                CommandOptionType.MultipleValue);
            var baseDirectory = command.Option(
                "--base-directory",
                "Root directory of the project.",
                CommandOptionType.SingleValue);
            var baseNamespace = command.Option(
                "--namespace",
                "The base namespace for the generated C# classes.",
                CommandOptionType.SingleValue);
            var verboseFlag = command.Option(
                "--verbose",
                "Indicates that verbose console output should written",
                CommandOptionType.NoValue);

            command.OnExecute(() =>
            {
                if (!VerifyOptions(sourceFilePaths.Values, outputFilePaths.Values, referencePaths.Values, baseDirectory.Value(), baseNamespace.Value()))
                {
                    return 1;
                }

                var items = new GenerateWorkItem[sourceFilePaths.Values.Count];
                for (var i = 0; i < sourceFilePaths.Values.Count; i++)
                {
                    items[i] = new GenerateWorkItem(sourceFilePaths.Values[i], outputFilePaths.Values[i]);
                }


                var configuration = BlazorExtensionInitializer.DefaultConfiguration;
                var engine = RazorProjectEngine.Create(configuration, RazorProjectFileSystem.Create(baseDirectory.Value()), b =>
                {
                    BlazorExtensionInitializer.Register(b);

                    // We don't yet have a first-class way to pass a base-namespace into Razor, doing it ourselves instead.
                    var classifier = b.Features.OfType<ComponentDocumentClassifierPass>().Single();
                    classifier.BaseNamespace = baseNamespace.Value();

                    // Tag Helper infrastructure, repurposed to support components.
                    b.Features.Add(new CompilationTagHelperFeature());
                    b.Features.Add(new DefaultMetadataReferenceFeature()
                    {
                        // Ignore files that don't exist. This can happen due to bad SDK authoring, but if the
                        // assembly doesn't have anything important it won't matter.
                        References = referencePaths.Values
                            .Where(r => File.Exists(r))
                            .Select(r => MetadataReference.CreateFromFile(r))
                            .ToArray(),
                    });
                });

                var diagnostics = new List<RazorDiagnostic>();
                for (var i = 0; i < items.Length; i++)
                {
                    var item = items[i];
                    var projectItem = engine.FileSystem.GetItem(item.SourceFilePath);

                    var codeDocument = engine.Process(projectItem);

                    File.WriteAllText(item.OutputFilePath, codeDocument.GetCSharpDocument().GeneratedCode);
                    diagnostics.AddRange(codeDocument.GetCSharpDocument().Diagnostics);
                }

                foreach (var diagnostic in diagnostics)
                {
                    Console.WriteLine(diagnostic.ToString());
                }

                var hasError = diagnostics.Any(item => item.Severity == RazorDiagnosticSeverity.Error);
                return hasError ? 1 : 0;
            });
        }

        private static bool VerifyOptions(IList<string> sourceFilePaths, IList<string> outputFilePaths, IList<string> references, string baseDirectory, string @namespace)
        {
            var hasErrors = false;
            if (sourceFilePaths.Count == 0)
            {
                hasErrors = true;
                Console.WriteLine("ERROR: no value specified for '--source'.");
            }

            if (outputFilePaths.Count == 0)
            {
                hasErrors = true;
                Console.WriteLine("ERROR: no value specified for '--reference'.");
            }

            if (references.Count == 0)
            {
                hasErrors = true;
                Console.WriteLine("ERROR: no value specified for '--output'.");
            }

            if (sourceFilePaths.Count != outputFilePaths.Count)
            {
                hasErrors = true;
                Console.WriteLine("ERROR: '--source' and '--output' must have the same number of values.");
            }

            if (baseDirectory == null)
            {
                hasErrors = true;
                Console.WriteLine("ERROR: no value specified for '--base-directory'.");
            }

            if (@namespace == null)
            {
                hasErrors = true;
                Console.WriteLine("ERROR: no value specified for '--namespace'.");
            }

            return !hasErrors;
        }

        private struct GenerateWorkItem
        {
            public GenerateWorkItem(string sourceFilePath, string outputFilePath)
            {
                if (sourceFilePath == null)
                {
                    throw new ArgumentNullException(nameof(sourceFilePath));
                }

                if (outputFilePath == null)
                {
                    throw new ArgumentNullException(nameof(outputFilePath));
                }

                SourceFilePath = sourceFilePath;
                OutputFilePath = outputFilePath;
            }

            public string SourceFilePath { get; }

            public string OutputFilePath { get; }
        }
    }
}
