using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Microsoft.AspNetCore.Mvc.Analyzers.Experimental
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ApiConventionMissingResponseTypeAnalyzer : ApiControllerAnalyzerBase
    {
        public static readonly string ReturnTypeKey = "ReturnType";

        public ApiConventionMissingResponseTypeAnalyzer()
            : base(DiagnosticDescriptors.MVC7004_ApiActionIsMissingMetadata)
        {
        }

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(DiagnosticDescriptors.MVC7004_ApiActionIsMissingMetadata, DiagnosticDescriptors.MVC7005_ApiActionIsMissingResponse);

        protected override void InitializeWorker(ApiControllerAnalyzerContext analyzerContext)
        {
            analyzerContext.Context.RegisterSyntaxNodeAction(context =>
            {
                var methodSyntax = (MethodDeclarationSyntax)context.Node;
                if (methodSyntax.Body == null)
                {
                    // Ignore expression bodied methods.
                }

                var method = context.SemanticModel.GetDeclaredSymbol(methodSyntax, context.CancellationToken);
                if (!analyzerContext.IsApiAction(method))
                {
                    return;
                }

                if (method.ReturnsVoid || method.ReturnType.Kind != SymbolKind.NamedType)
                {
                    return;
                }

                var expectedStatusCodes = GetDeclaredStatusCodes(analyzerContext, method)
                    ?? GetStatusCodesFromConvention(analyzerContext, method);
                if (expectedStatusCodes?.Count == 0)
                {
                    return;
                }

                var actualStatusCodes = new HashSet<int>();
                foreach (var returnStatement in methodSyntax.DescendantNodes().OfType<ReturnStatementSyntax>())
                {
                    var returnType = context.SemanticModel.GetTypeInfo(returnStatement.Expression, context.CancellationToken);
                    var statusCodeAttribute = returnType.Type.GetAttributeData(analyzerContext.StatusCodeAttribute, inherit: true);
                    if (statusCodeAttribute == null || statusCodeAttribute.ConstructorArguments.Length == 0 || statusCodeAttribute.ConstructorArguments[0].Kind != TypedConstantKind.Primitive)
                    {
                        continue;
                    }

                    var statusCode = (int)statusCodeAttribute.ConstructorArguments[0].Value;
                    if (!expectedStatusCodes.Contains(statusCode))
                    {
                        context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.MVC7004_ApiActionIsMissingMetadata, returnStatement.GetLocation(), statusCode));
                    }

                    actualStatusCodes.Add(statusCode);
                }

                var unusedStatusCodes = expectedStatusCodes.Except(actualStatusCodes).ToList();
                for (var i = 0; i < unusedStatusCodes.Count; i++)
                {
                    context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.MVC7005_ApiActionIsMissingResponse, methodSyntax.GetLocation(), unusedStatusCodes[i]));
                }

            }, SyntaxKind.MethodDeclaration);
        }

        private List<int> GetStatusCodesFromConvention(ApiControllerAnalyzerContext analyzerContext, IMethodSymbol method)
        {
            var attribute = method.ContainingType.GetAttributes().First(a => a.AttributeClass == analyzerContext.ApiControllerAttribute);
            var conventionProperty = attribute.NamedArguments.First(f => f.Key == "ConventionType").Value;
            if (conventionProperty.Kind != TypedConstantKind.Type || conventionProperty.Value == null)
            {
                return null;
            }

            var conventionType = (ITypeSymbol)conventionProperty.Value;
            var conventionMethod = conventionType.GetMembers(method.Name)
                .OfType<IMethodSymbol>()
                .FirstOrDefault(methodInConvention =>
                {
                    if (methodInConvention.Parameters.Length != method.Parameters.Length)
                    {
                        return false;
                    }

                    for (var i = 0; i < method.Parameters.Length; i++)
                    {
                        var parameter = method.Parameters[i];
                        var parameterInConvention = methodInConvention.Parameters[i];

                        if (parameter.Type != parameterInConvention.Type ||
                            parameter.Name != parameterInConvention.Name)
                        {
                            return false;
                        }
                    }

                    return true;
                });

            return GetDeclaredStatusCodes(analyzerContext, conventionMethod);
        }

        private List<int> GetDeclaredStatusCodes(ApiControllerAnalyzerContext analyzerContext, IMethodSymbol method)
        {
            var statusCodes = (List<int>)null;
            var attributes = method.GetAttributes();
            foreach (var attribute in attributes)
            {
                if (analyzerContext.IApiResponseMetadataProvider.IsAssignableFrom(attribute.AttributeClass))
                {
                    var propertyValue = attribute.NamedArguments.First(argument => argument.Key == "StatusCode").Value;
                    var statusCode = propertyValue.Kind == TypedConstantKind.Primitive && propertyValue.Value != null ? (int)propertyValue.Value : 0;

                    if (statusCode != 0)
                    {
                        statusCodes = statusCodes ?? new List<int>();
                        statusCodes.Add(statusCode);
                    }
                }
            }

            return statusCodes;
        }
    }
}
