// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;

namespace Microsoft.AspNetCore.Mvc.Analyzers
{
    [ExportCodeFixProvider(LanguageNames.CSharp)]
    [Shared]
    public class ApiConvention_ExtractToConventionCodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(
            DiagnosticDescriptors.MVC1004_ActionReturnsUndocumentedStatusCode.Id);

        public sealed override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            if (context.Diagnostics.Length == 0)
            {
                return Task.CompletedTask;
            }

            var diagnostic = context.Diagnostics[0];
            context.RegisterCodeFix(new MyCodeAction(context.Document, diagnostic), diagnostic);

            return Task.CompletedTask;
        }

        private class MyCodeAction : CodeAction
        {
            private readonly Document _document;
            private readonly Diagnostic _diagnostic;

            public MyCodeAction(Document document, Diagnostic diagnostic)
            {
                _document = document;
                _diagnostic = diagnostic;
            }

            public override string Title => "Extract to convention";

            protected override async Task<Document> GetChangedDocumentAsync(CancellationToken cancellationToken)
            {
                var documentEditor = await CalculateFixAsync(cancellationToken);
                return documentEditor?.GetChangedDocument();
            }

            private async Task<DocumentEditor> CalculateFixAsync(CancellationToken cancellationToken)
            {
                var root = await _document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
                var semanticModel = await _document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);

                var methodReturnStatement = (ReturnStatementSyntax)root.FindNode(_diagnostic.Location.SourceSpan);
                var methodDeclaration = methodReturnStatement.FirstAncestorOrSelf<MethodDeclarationSyntax>();
                var method = semanticModel.GetDeclaredSymbol(methodDeclaration, cancellationToken);

                var symbolCache = new ApiControllerSymbolCache(semanticModel.Compilation);
                var responseMetadata = SymbolApiResponseMetadataProvider.GetResponseMetadata(symbolCache, method);

                var diagnostics = semanticModel.GetMethodBodyDiagnostics(methodDeclaration.Span, cancellationToken)
                    .Where(d => d.Descriptor == DiagnosticDescriptors.MVC1004_ActionReturnsUndocumentedStatusCode)
                    .ToArray();

                var methodToClone = responseMetadata.Count != 0 ? responseMetadata[0].Convention : method;
                MethodDeclarationSyntax syntaxToUpdate = null;
                if (await ShouldUpdateExistingConventionMethod())
                {
                    return await UpdateSyntax(semanticModel, syntaxToUpdate, diagnostics, cancellationToken);
                }

                return null;

                async Task<bool> ShouldUpdateExistingConventionMethod()
                {
                    if (method == methodToClone)
                    {
                        return false;
                    }

                    if (methodToClone.DeclaringSyntaxReferences.Length != 1)
                    {
                        return false;
                    }

                    var syntaxReference = methodToClone.DeclaringSyntaxReferences[0];
                    syntaxToUpdate = (MethodDeclarationSyntax)await syntaxReference.GetSyntaxAsync(cancellationToken);
                    return syntaxToUpdate.GetLocation().IsInSource;
                }
            }

            private async Task<DocumentEditor> UpdateSyntax(SemanticModel semanticModel, MethodDeclarationSyntax syntaxToUpdate, Diagnostic[] diagnostics, CancellationToken cancellationToken)
            {
                var compilation = semanticModel.Compilation;
                var producesResponseTypeAttribute = compilation.GetTypeByMetadataName(SymbolNames.ProducesResponseTypeAttribute);
                var attributeName = producesResponseTypeAttribute.ToMinimalDisplayString(semanticModel, syntaxToUpdate.SpanStart);
                if (attributeName.EndsWith("Attribute", StringComparison.Ordinal))
                {
                    attributeName = attributeName.Substring(0, attributeName.Length - "Attribute".Length);
                }

                var editor = await DocumentEditor.CreateAsync(_document, cancellationToken).ConfigureAwait(false);

                foreach (var diagnostic in diagnostics)
                {
                    var statusCode = int.Parse(diagnostic.Properties[ApiConventionAnalyzer.StatusCode]);

                    var attribute = SyntaxFactory.Attribute(
                        SyntaxFactory.ParseName(attributeName),
                        SyntaxFactory.AttributeArgumentList().AddArguments(
                            SyntaxFactory.AttributeArgument(
                                SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(statusCode)))));

                    editor.AddAttribute(syntaxToUpdate, attribute);
                }

                return editor;
            }
        }
    }
}
