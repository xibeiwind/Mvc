// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Analyzer.Testing;
using Microsoft.AspNetCore.Mvc.Analyzers.Infrastructure;
using Microsoft.CodeAnalysis;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.Analyzers.Experimental
{
    public class ApiConventionMissingResponseTypeAnalyzerTest
    {
        private static DiagnosticDescriptor DiagnosticDescriptor = DiagnosticDescriptors.MVC7005_ApiActionIsMissingResponse;

        private MvcDiagnosticAnalyzerRunner Executor { get; } = new MvcDiagnosticAnalyzerRunner(new ApiConventionMissingResponseTypeAnalyzer());

        [Fact]
        public async Task DiagnosticsAreReturned_ForDefaultConventions_IfGetActionDoesNotHaveA404()
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
        public async Task DiagnosticsAreReturned_IfActionMethodHasAttribute()
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

        private static TestSource ReadTestSource([CallerMemberName] string testMethod = "") =>
            MvcTestSource.Read(nameof(ApiConventionMissingResponseTypeAnalyzerTest), testMethod);
    }
}
