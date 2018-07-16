// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Analyzer.Testing;
using Microsoft.AspNetCore.Mvc.Analyzers.Infrastructure;
using Microsoft.CodeAnalysis;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.Analyzers
{
    public class ApiConventionCodeFixIntegrationTest
    {
        private MvcDiagnosticAnalyzerRunner AnalyzerRunner { get; } = new MvcDiagnosticAnalyzerRunner(new ApiConventionAnalyzer());

        private ApiConventionCodeFixRunner CodeFixRunner { get; } = new ApiConventionCodeFixRunner();

        [Fact]
        public Task ExtractToConvention_AddsAttributesToExistingConventionMethod() => RunTest();

        [Fact]
        public Task ExtractToConvention_AddsNewConventionMethodToExistingConventionType() => RunTest();

        private async Task RunTest([CallerMemberName] string testMethod = "")
        {
            // Arrange
            var project = GetProject(testMethod);
            var controllerDocument = project.DocumentIds[0];
            var conventionDocument = project.DocumentIds[1];

            var expectedController = Read(testMethod, "Controller.Output");
            var expectedConvention = Read(testMethod, "Convention.Output");

            // Act
            var diagnostics = await AnalyzerRunner.GetDiagnosticsAsync(project);
            var updatedProject = await CodeFixRunner.ApplyCodeFixAsync(project, diagnostics);

            // Assert
            var actualController = 
        }

        private Project GetProject(string testMethod)
        {
            var controller = Read(testMethod, "Controller.Input");
            var convention = Read(testMethod, "Convention.Input");
            
            return DiagnosticProject.Create(GetType().Assembly, new[] { controller, convention });
        }

        private string Read(string testMethod, string fileName)
        {
            var testClassName = GetType().Name;
            var filePath = Path.Combine(MvcTestSource.ProjectDirectory, "TestFiles", testClassName, testMethod, fileName + ".cs");
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"TestFile {testMethod} could not be found at {filePath}.", filePath);
            }

            var fileContent = File.ReadAllText(filePath);
            return TestSource.Read(fileContent)
                .Source
                .Replace("_INPUT_", "_TEST_", StringComparison.Ordinal)
                .Replace("_OUTPUT_", "_TEST_", StringComparison.Ordinal);
        }

        private class ApiConventionCodeFixRunner : CodeFixRunner
        {
            public ApiConventionCodeFixRunner()
            {
            }

            public async Task<Project> ApplyCodeFixAsync(Project project, Diagnostic[] diagnostics)
            {
                var document = Assert.Single(project.Documents);
                var diagnostic = Assert.Single(diagnostics);

                var solution = project.Solution;
                var updatedSolution = await GetChangedSolutionAsync(
                    new ExtractToExistingApiConventionCodeFixProvider(),
                    document,
                    diagnostic);

                var updatedProject = updatedSolution.GetProject(project.Id);
                await EnsureCompilable(updatedProject);

                return updatedProject;
            }
        }
    }
}
