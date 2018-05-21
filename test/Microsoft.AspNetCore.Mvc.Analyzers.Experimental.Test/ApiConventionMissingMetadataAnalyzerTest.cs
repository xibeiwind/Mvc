// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Analyzer.Testing;
using Microsoft.AspNetCore.Mvc.Analyzers.Infrastructure;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.Analyzers.Experimental
{
    public class ApiConventionMissingMetadataAnalyzerTest
    {
        private static DiagnosticDescriptor DiagnosticDescriptor = DiagnosticDescriptors.MVC7004_ApiActionIsMissingMetadata;

        private MvcDiagnosticAnalyzerRunner Executor { get; } = new MvcDiagnosticAnalyzerRunner(new ApiConventionMissingResponseTypeAnalyzer());

        private CodeFixProvider CodeFixProvider { get; } = new ApiConventionMissingResponseTypeCodeFixProvider();

        [Fact]
        public async Task DiagnosticsAreReturned_IfActionReturnsStatusCodeThatIsNotInConvention()
        {
            // Arrange
            var testSource = ReadTestSource();
            var expectedLocation = testSource.DefaultMarkerLocation;

            // Act
            var result = await Executor.GetDiagnosticsAsync(testSource.Source);

            // Assert
            Assert.Collection(
                result,
                diagnostic =>
                {
                    Assert.Equal(DiagnosticDescriptor.Id, diagnostic.Id);
                    Assert.Same(DiagnosticDescriptor, diagnostic.Descriptor);
                    AnalyzerAssert.DiagnosticLocation(expectedLocation, diagnostic.Location);
                });
        }

        [Fact]
        public async Task CodeFixesAreProvided_IfActionReturnsStatusCodeThatIsNotInConvention()
        {
            // Arrange
            var testSource = ReadTestSource(nameof(DiagnosticsAreReturned_IfActionReturnsStatusCodeThatIsNotInConvention));
            var analyzerSource = ReadAnalyzerSource();
            var project = DiagnosticProject.Create(GetType().Assembly, new[] { testSource.Source });

            // Act
            var diagnostics = await Executor.GetDiagnosticsAsync(testSource.Source);
            var result = await CodeFixRunner.Default.ApplyCodeFixAsync(CodeFixProvider, project.Documents.Single(), diagnostics.Single());
            Assert.Equal(analyzerSource, result);
        }

        private TestSource ReadTestSource([CallerMemberName] string testMethod = "") =>
            MvcTestSource.Read(GetType().Name, testMethod);

        private string ReadAnalyzerSource([CallerMemberName] string testMethod = "")
        {
            var testSource = ReadTestSource(testMethod);
            var diagnosticFileName = testMethod.Replace("CodeFixesAreProvided_", "DiagnosticsAreReturned_");
            return testSource.Source.Replace(testMethod, diagnosticFileName);
        }
    }
}
