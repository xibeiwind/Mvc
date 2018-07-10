// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Analyzer.Testing;
using Microsoft.AspNetCore.Mvc.Analyzers.Infrastructure;
using Microsoft.CodeAnalysis;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.Analyzers
{
    public class ApiConvention_ExtractToConventionCodeFixProviderTest
    {
        private MvcDiagnosticAnalyzerRunner AnalyzerRunner { get; } = new MvcDiagnosticAnalyzerRunner(new ApiConventionAnalyzer());

        private ApiConvention_ExtractToConventionCodeFixRunner CodeFixRunner { get; } = new ApiConvention_ExtractToConventionCodeFixRunner();

        [Fact]
        public async Task ExtractToConvention_AddsAttributesToExistingConventionMethod()
        {
            var project = GetProject();
            var diagnostics = await AnalyzerRunner.GetDiagnosticsAsync(project);

            var actualCode = await CodeFixRunner.ApplyCodeFixAsync(project, diagnostics);

            Assert.NotNull(actualCode);
        }


        private Project GetProject([CallerMemberName] string testMethod = "")
        {
            var testSource = MvcTestSource.Read(GetType().Name, testMethod);
            return DiagnosticProject.Create(GetType().Assembly, new[] { testSource.Source });
        }

        private class ApiConvention_ExtractToConventionCodeFixRunner : CodeFixRunner
        {
            public ApiConvention_ExtractToConventionCodeFixRunner()
            {
            }

            public Task<string> ApplyCodeFixAsync(Project project, Diagnostic[] diagnostics)
            {
                var document = Assert.Single(project.Documents);
                var diagnostic = diagnostics[0];

                return ApplyCodeFixAsync(
                    new ApiConvention_ExtractToConventionCodeFixProvider(),
                    document,
                    diagnostic);
            }
                
        }
    }
}
