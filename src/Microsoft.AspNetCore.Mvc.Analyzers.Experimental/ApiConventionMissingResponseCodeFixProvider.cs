// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Composition;
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
    public class ApiConventionMissingResponseTypeCodeFixProvider : CodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create(DiagnosticDescriptors.MVC7004_ApiActionIsMissingMetadata.Id);

        public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var diagnostic = context.Diagnostics[0];
            var statusCode = diagnostic.Properties["StatusCode"];
            var title = $"Add [ProducesResponse({statusCode})]";
            var rootNode = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            context.RegisterCodeFix(
                CodeAction.Create(
                    title,
                    createChangedDocument: CreateChangedDocumentAsync,
                    equivalenceKey: title),
                context.Diagnostics);

            async Task<Document> CreateChangedDocumentAsync(CancellationToken cancellationToken)
            {
                var editor = await DocumentEditor.CreateAsync(context.Document, cancellationToken).ConfigureAwait(false);
                var returnStatement = (ReturnStatementSyntax)rootNode.FindNode(context.Span);
                var methodDeclaration = returnStatement.FirstAncestorOrSelf<MethodDeclarationSyntax>();

                var compilation = editor.SemanticModel.Compilation;
                var producesResponseTypeAttribute = compilation.GetTypeByMetadataName(TypeNames.ProducesResponseTypeAttribute);
                var attributeName = producesResponseTypeAttribute.ToMinimalDisplayString(editor.SemanticModel, methodDeclaration.SpanStart);
                if (attributeName.EndsWith("Attribute", StringComparison.Ordinal))
                {
                    attributeName = attributeName.Substring(0, attributeName.Length - "Attribute".Length);
                }

                var attribute = SyntaxFactory.Attribute(
                    SyntaxFactory.ParseName(attributeName),
                    SyntaxFactory.AttributeArgumentList().AddArguments(
                        SyntaxFactory.AttributeArgument(
                            SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(int.Parse(statusCode))))));

                editor.AddAttribute(methodDeclaration, attribute);
                return editor.GetChangedDocument();
            }
        }
    }
}
