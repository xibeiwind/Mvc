// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Composition;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Analyzers.ApiResponseMetadata;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;

namespace Microsoft.AspNetCore.Mvc.Analyzers
{
    [ExportCodeFixProvider(LanguageNames.CSharp)]
    [Shared]
    public class AddResponseTypeAttributeCodeFixProvider : CodeFixProvider
    {
        private static readonly ApiResponseMetadataCodeFixStrategy[] Strategies = new ApiResponseMetadataCodeFixStrategy[]
        {
            new AddProducesResponseTypeAttributesCodeFixStrategy(),
        };

        public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(
            DiagnosticDescriptors.MVC1004_ActionReturnsUndocumentedStatusCode.Id,
            DiagnosticDescriptors.MVC1005_ActionReturnsUndocumentedSuccessResult.Id);

        public sealed override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            if (context.Diagnostics.Length == 0)
            {
                return Task.CompletedTask;
            }

            var diagnostic = context.Diagnostics[0];
            if ((diagnostic.Descriptor.Id != DiagnosticDescriptors.MVC1004_ActionReturnsUndocumentedStatusCode.Id) &&
                (diagnostic.Descriptor.Id != DiagnosticDescriptors.MVC1005_ActionReturnsUndocumentedSuccessResult.Id))
            {
                return Task.CompletedTask;
            }

            if (diagnostic.AdditionalLocations.Count == 0 || !diagnostic.Properties.TryGetValue(ApiConventionAnalyzer.ApiConventionInSourceKey, out var conventionName))
            {
                // Additional location points to the syntax of an existing ApiConvention type that is in code.
                return Task.CompletedTask;
            }

            var conventionLocation = diagnostic.AdditionalLocations[0];
            var analyzerDocument = context.Document.Project.GetDocument(conventionLocation.SourceTree);

            var title = $"Add ProducesResponseType attributes to method.";

            var codeFix = new ApiResponseMetadataCodeAction(context.Document, diagnostic, Strategies, title)
            {
                AnalyzerDocument = analyzerDocument,
            };

            context.RegisterCodeFix(codeFix, diagnostic);
            return Task.CompletedTask;
        }
    }
}
