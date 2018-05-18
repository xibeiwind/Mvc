using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
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

                IMethodSymbol conventionMethod = null;
                var expectedStatusCodes = GetDeclaredStatusCodes(analyzerContext, method)
                    ?? GetStatusCodesFromConvention(analyzerContext, method, out conventionMethod);
                if (expectedStatusCodes == null || expectedStatusCodes.Count == 0)
                {
                    return;
                }

                var (declaredReturnType, isTaskOfT) = AnalyzerUtils.UnwrapReturnType(analyzerContext, method);

                var actualStatusCodes = new HashSet<int>();
                foreach (var returnStatement in methodSyntax.DescendantNodes().OfType<ReturnStatementSyntax>())
                {
                    var returnType = context.SemanticModel.GetTypeInfo(returnStatement.Expression, context.CancellationToken).Type;
                    if ((returnType == method.ReturnType || returnType == declaredReturnType) && !expectedStatusCodes.Contains(0))
                    {
                        // Verify there's a ProducesDefaultResponseAttribute.
                        var additionalLocations = conventionMethod == null ? Enumerable.Empty<Location>() : new[] { conventionMethod.Locations[0] };
                        context.ReportDiagnostic(Diagnostic.Create(
                            DiagnosticDescriptors.MVC7006_ApiActionIsMissingProducesDefaultResponseAttribute,
                            returnStatement.GetLocation(),
                            additionalLocations,
                            returnType));
                    }
                    
                    if (!analyzerContext.IActionResult.IsAssignableFrom(returnType))
                    {
                        // Returning an object. Verify we have a DefaultResponse

                    }

                    var statusCodeAttribute = returnType.GetAttributeData(analyzerContext.StatusCodeAttribute, inherit: true);
                    if (statusCodeAttribute == null ||
                        statusCodeAttribute.ConstructorArguments.Length == 0 ||
                        statusCodeAttribute.ConstructorArguments[0].Kind != TypedConstantKind.Primitive)
                    {
                        continue;
                    }

                    var statusCode = (int)statusCodeAttribute.ConstructorArguments[0].Value;
                    if (!expectedStatusCodes.Contains(statusCode))
                    {
                        var dictionary = ImmutableDictionary.Create<string, string>()
                            .Add("StatusCode", statusCode.ToString());
                        var additionalLocations = conventionMethod == null ? Enumerable.Empty<Location>() : new[] { conventionMethod.Locations[0] };
                        context.ReportDiagnostic(Diagnostic.Create(
                            DiagnosticDescriptors.MVC7004_ApiActionIsMissingMetadata,
                            returnStatement.GetLocation(),
                            additionalLocations,
                            dictionary,
                            statusCode));
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

        private List<int> GetStatusCodesFromConvention(ApiControllerAnalyzerContext analyzerContext, IMethodSymbol method, out IMethodSymbol conventionMethod)
        {
            var attribute = method.ContainingType.GetAttributeData(analyzerContext.ApiControllerAttribute);
            Debug.Assert(attribute != null);
            conventionMethod = null;
            var conventionProperty = attribute.NamedArguments.First(f => f.Key == "ConventionType").Value;
            if (conventionProperty.Kind != TypedConstantKind.Type || conventionProperty.Value == null)
            {
                return null;
            }

            var conventionType = (ITypeSymbol)conventionProperty.Value;
            conventionMethod = conventionType.GetMembers(method.Name)
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
            var attributes = method.GetAttributeDataItems(analyzerContext.IApiResponseMetadataProvider);
            var statusCodes = (List<int>)null;
            foreach (var attribute in attributes)
            {
                if (attribute.AttributeClass == analyzerContext.ProducesDefaultResponseAttribute)
                {
                    statusCodes = statusCodes ?? new List<int>();
                    statusCodes.Add(0);
                    continue;
                }

                var parameters = attribute.AttributeConstructor.Parameters;
                var index = -1;
                for (var i = 0; i < parameters.Length; i++)
                {
                    var parameter = parameters[i];
                    if (string.Equals(parameter.Name, "StatusCode", StringComparison.OrdinalIgnoreCase) && (parameter.Type.SpecialType & SpecialType.System_Int32) == SpecialType.System_Int32)
                    {
                        index = i;
                        break;
                    }
                }

                var statusCode = index != -1 && attribute.ConstructorArguments[index].Kind == TypedConstantKind.Primitive && attribute.ConstructorArguments[index].Value != null ?
                    (int)attribute.ConstructorArguments[index].Value :
                    0;

                if (statusCode != 0)
                {
                    statusCodes = statusCodes ?? new List<int>();
                    statusCodes.Add(statusCode);
                }
            }

            return statusCodes;
        }
    }
}
