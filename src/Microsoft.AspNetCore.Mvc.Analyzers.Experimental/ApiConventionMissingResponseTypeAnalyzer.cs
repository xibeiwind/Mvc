using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Microsoft.AspNetCore.Mvc.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ApiConventionMissingResponseTypeAnalyzer : ApiControllerAnalyzerBase
    {
        public ApiConventionMissingResponseTypeAnalyzer()
            : base(DiagnosticDescriptors.MVC7004_ApiActionIsMissingMetadata)
        {
        }

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
            DiagnosticDescriptors.MVC7004_ApiActionIsMissingMetadata,
            DiagnosticDescriptors.MVC7005_ApiActionIsMissingResponse,
            DiagnosticDescriptors.MVC7006_ApiActionIsMissingProducesAttribute);

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

                var (declaredReturnType, isTaskOfT) = AnalyzerUtils.UnwrapReturnType(analyzerContext, method);

                var declaredResponseMetadata = GetResponseMetadata(analyzerContext, method, declaredReturnType);
                var actualResponseMetadata = new List<(ITypeSymbol type, int statusCode)>()
                {
                    // 400 Bad Model State errors by default for APIControllers
                    (null, 400),
                };

                foreach (var returnStatement in methodSyntax.DescendantNodes().OfType<ReturnStatementSyntax>())
                {
                    (ITypeSymbol type, int statusCode) actual;
                    var returnType = context.SemanticModel.GetTypeInfo(returnStatement.Expression, analyzerContext.Context.CancellationToken).Type;
                    if (returnType == declaredReturnType)
                    {
                        actual = (declaredReturnType, statusCode: 200);
                        actualResponseMetadata.Add(actual);
                        if (!declaredResponseMetadata.Contains(actual))
                        {
                            var dictionary = ImmutableDictionary.Create<string, string>()
                                .Add("ProducedType", declaredReturnType.Name);
                            context.ReportDiagnostic(Diagnostic.Create(
                                DiagnosticDescriptors.MVC7006_ApiActionIsMissingProducesAttribute,
                                returnStatement.GetLocation(),
                                dictionary,
                                returnType));
                        }

                        continue;
                    }

                    var statusCodeAttribute = returnType.GetAttributeData(analyzerContext.StatusCodeAttribute, inherit: true);
                    if (statusCodeAttribute == null ||
                        statusCodeAttribute.ConstructorArguments.Length == 0 ||
                        statusCodeAttribute.ConstructorArguments[0].Kind != TypedConstantKind.Primitive)
                    {
                        continue;
                    }

                    var statusCode = (int)statusCodeAttribute.ConstructorArguments[0].Value;
                    actual = (null, statusCode);
                    actualResponseMetadata.Add(actual);

                    if (!declaredResponseMetadata.Any(d => d.statusCode == actual.statusCode))
                    {
                        var dictionary = ImmutableDictionary.Create<string, string>()
                            .Add("StatusCode", statusCode.ToString());
                        var additionalLocations = new[] { methodSyntax.GetLocation() };
                        context.ReportDiagnostic(Diagnostic.Create(
                            DiagnosticDescriptors.MVC7004_ApiActionIsMissingMetadata,
                            returnStatement.GetLocation(),
                            additionalLocations,
                            dictionary,
                            statusCode));
                    }
                }

                foreach (var declared in declaredResponseMetadata)
                {
                    if (!actualResponseMetadata.Any(d => d.statusCode == declared.statusCode))
                    {
                        context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.MVC7005_ApiActionIsMissingResponse, methodSyntax.Identifier.GetLocation(), declared.statusCode));
                    }
                }

            }, SyntaxKind.MethodDeclaration);
        }

        private List<(ITypeSymbol type, int statusCode)> GetResponseMetadata(ApiControllerAnalyzerContext analyzerContext, IMethodSymbol method, ITypeSymbol declaredReturnType)
        {
            var metadata = GetResponseMetadataFromMethod(analyzerContext, method, declaredReturnType);
            if (metadata != null)
            {
                return metadata;
            }

            var conventionMethod = GetConventionMethod(analyzerContext, method);
            return conventionMethod != null ? GetResponseMetadataFromMethod(analyzerContext, conventionMethod, declaredReturnType) : null;
        }

        private IMethodSymbol GetConventionMethod(ApiControllerAnalyzerContext analyzerContext, IMethodSymbol method)
        {
            var attribute = method.ContainingType.GetAttributeData(analyzerContext.ApiControllerAttribute);
            Debug.Assert(attribute != null);

            ITypeSymbol conventionType;
            if (attribute.ConstructorArguments.Length == 0)
            {
                conventionType = analyzerContext.DefaultApiConventions;
            }
            else
            {
                if (attribute.ConstructorArguments[0].Kind != TypedConstantKind.Type || attribute.ConstructorArguments[0].Value == null)
                {
                    return null;
                }
                conventionType = (ITypeSymbol)attribute.ConstructorArguments[0].Value;
            }

            var conventionMethod = conventionType.GetMembers()
                .OfType<IMethodSymbol>()
                .FirstOrDefault(methodInConvention =>
                {
                    if (!method.Name.StartsWith(methodInConvention.Name, StringComparison.Ordinal))
                    {
                        return false;
                    }

                    if (methodInConvention.Parameters.Length != method.Parameters.Length)
                    {
                        return false;
                    }

                    for (var i = 0; i < method.Parameters.Length; i++)
                    {
                        var parameter = method.Parameters[i];
                        var parameterInConvention = methodInConvention.Parameters[i];

                        if (parameterInConvention.Type.TypeKind == TypeKind.TypeParameter)
                        {
                            continue;
                        }
                        else if (!IsNameMatch(parameter.Name, parameterInConvention.Name))
                        {
                            return false;
                        }
                    }

                    return true;
                });

            return conventionMethod;
        }

        private List<(ITypeSymbol, int)> GetResponseMetadataFromMethod(ApiControllerAnalyzerContext analyzerContext, IMethodSymbol method, ITypeSymbol declaredReturnType)
        {
            var attributes = method.GetAttributeDataItems(analyzerContext.IApiResponseMetadataProvider);
            var responseMetadata = (List<(ITypeSymbol, int)>)null;
            foreach (var attribute in attributes)
            {
                responseMetadata = responseMetadata ?? new List<(ITypeSymbol, int)>();
                var metadata = ReadMetadata(attribute);
                responseMetadata.Add(metadata);
            }

            return responseMetadata;

            (ITypeSymbol, int) ReadMetadata(AttributeData attribute)
            {
                var type = (ITypeSymbol)null;
                var statusCode = 200;

                if (attribute.AttributeClass == analyzerContext.ProducesDefaultResponseAttribute)
                {
                    type = declaredReturnType;
                }

                var parameters = attribute.AttributeConstructor.Parameters;
                for (var i = 0; i < parameters.Length; i++)
                {
                    var parameter = parameters[i];
                    if (string.Equals(parameter.Name, "StatusCode", StringComparison.OrdinalIgnoreCase) && (parameter.Type.SpecialType & SpecialType.System_Int32) == SpecialType.System_Int32)
                    {
                        var argument = attribute.ConstructorArguments[i];
                        if (argument.Kind == TypedConstantKind.Primitive && argument.Value is int value)
                        {
                            statusCode = value;
                        }
                    }
                    else if (string.Equals(parameter.Name, "type") || parameter.Type == analyzerContext.SystemType)
                    {
                        var argument = attribute.ConstructorArguments[i];
                        if (argument.Kind == TypedConstantKind.Type && argument.Value is ITypeSymbol value)
                        {
                            type = value;
                        }
                    }
                }

                return (type, statusCode);
            }
        }


        private static bool IsNameMatch(string name, string conventionName)
        {
            // Leading underscores could be used to allow multiple parameter names with the same suffix e.g. GetPersonAddress(int personId, int addressId)
            // A common convention that allows targeting these category of methods would look like Get(int id, int _id)
            conventionName = conventionName.Trim('_');

            // name = id, conventionName = id
            if (string.Equals(name, conventionName, StringComparison.Ordinal))
            {
                return true;
            }

            if (name.Length <= conventionName.Length)
            {
                return false;
            }

            // name = personId, conventionName = id
            var index = name.Length - conventionName.Length - 1;
            if (!char.IsLower(name[index]))
            {
                return false;
            }

            index++;
            if (name[index] != char.ToUpper(conventionName[0]))
            {
                return false;
            }

            index++;
            return string.Compare(name, index, conventionName, 1, conventionName.Length - 1, StringComparison.Ordinal) == 0;
        }
    }
}
