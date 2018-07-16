// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Simplification;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Microsoft.AspNetCore.Mvc.Analyzers.ApiResponseMetadata
{
    internal abstract class ApiResponseMetadataCodeFixStrategy
    {
        public abstract Task ExecuteAsync(ApiResponseMetadataCodeFixStrategyContext context);

        protected static NameSyntax SimplifiedTypeName(string typeName)
        {
            return ParseName(typeName).WithAdditionalAnnotations(Simplifier.Annotation);
        }


        protected static MethodDeclarationSyntax CreateNewConventionMethod(ApiResponseMetadataCodeFixStrategyContext context, out IList<AttributeSyntax> methodAttributes)
        {
            var semanticModel = context.SemanticModel;
            var statusCodes = new HashSet<int>();
            methodAttributes = new List<AttributeSyntax>();

            foreach (var metadata in context.DeclaredApiResponseMetadata)
            {
                statusCodes.Add(metadata.StatusCode);

                if (metadata.IsImplicit)
                {
                    // Attribute is implicitly defined (and does not appear in source)
                    continue;
                }

                if (metadata.AttributeSource != context.Method)
                {
                    // Attribute isn't defined on a method.
                    continue;
                }

                var attributeSyntax = (AttributeSyntax)metadata.Attribute.ApplicationSyntaxReference.GetSyntax(context.CancellationToken);
                methodAttributes.Add(attributeSyntax);
            }

            var producesResponseTypeName = SimplifiedTypeName(SymbolNames.ProducesResponseTypeAttribute);
            foreach (var metadata in context.UndocumentedMetadata)
            {
                var statusCode = metadata.IsDefaultResponse ? 200 : metadata.StatusCode;
                statusCodes.Add(statusCode);
            }

            var conventionMethodAttributes = new SyntaxList<AttributeListSyntax>();
            foreach (var statusCode in statusCodes.OrderBy(s => s))
            {
                var producesResponseTypeAttribute = CreateProducesResponseTypeAttribute(statusCode);
                conventionMethodAttributes = conventionMethodAttributes.Add(AttributeList().AddAttributes(producesResponseTypeAttribute));
            }

            var nameMatchBehaviorAttribute = CreateNameMatchAttribute(SymbolNames.ApiConventionNameMatchBehavior_Prefix);
            conventionMethodAttributes = conventionMethodAttributes.Add(AttributeList().AddAttributes(nameMatchBehaviorAttribute));

            var voidType = PredefinedType(Token(SyntaxKind.VoidKeyword));
            var methodName = GetConventionMethodName(context.Method.Name);

            var conventionParamterNames = new List<string>();
            var conventionParameterList = ParameterList();
            foreach (var parameter in context.Method.Parameters)
            {
                var parameterName = GetConventionParameterName(parameter.Name);
                var parameterType = PredefinedType(Token(SyntaxKind.ObjectKeyword));

                conventionParamterNames.Add(parameterName);

                var parameterNameMatchBehaviorAttribute = CreateNameMatchAttribute(SymbolNames.ApiConventionNameMatchBehavior_Suffix);
                var parameterTypeMatchBehaviorAttribute = CreateTypeMatchAttribute(SymbolNames.ApiConventionTypeMatchBehavior_Any);

                var conventionParameter = Parameter(Identifier(parameterName))
                    .WithType(parameterType.WithAdditionalAnnotations(Simplifier.Annotation))
                    .AddAttributeLists(AttributeList().AddAttributes(parameterNameMatchBehaviorAttribute, parameterTypeMatchBehaviorAttribute));

                conventionParameterList = conventionParameterList.AddParameters(conventionParameter);
            }

            var builder = new StringBuilder();
            builder.AppendLine("/// <summary>");
            var parameterCount = context.Method.Parameters.Length;
            if (parameterCount == 0)
            {
                var text = $"An API convention that matches all methods that start with the term '{methodName}' containing no parameters.";
                builder.AppendLine(text);
            }
            else
            {
                var text = $"An API convention that matches all methods that start with the term '{methodName}' containing exactly {parameterCount} parameter(s)." +
                    Environment.NewLine +
                    "/// Parameters must match the following requirements:";

                builder.Append("/// ").AppendLine(text);
                builder.AppendLine("/// <list type=\"number\">");

                for (var i = 0; i < conventionParamterNames.Count; i++)
                {
                    builder.AppendLine($"/// <item>Parameter at position '{(i + 1)}' has suffix '{conventionParamterNames[i]}'.</item>");
                }

                builder.AppendLine("/// </list>");
            }
            builder.AppendLine("/// </summary>");
            var comments = ParseLeadingTrivia(builder.ToString());

            var method = MethodDeclaration(voidType, methodName)
               .WithAttributeLists(conventionMethodAttributes)
               .AddModifiers(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.StaticKeyword))
               .WithBody(Block())
               .WithParameterList(conventionParameterList);

            method = method.WithLeadingTrivia(
                method.GetLeadingTrivia().AddRange(comments));

            return method;
        }

        protected static AttributeSyntax CreateProducesResponseTypeAttribute(ActualApiResponseMetadata metadata)
        {
            var statusCode = metadata.IsDefaultResponse ? 200 : metadata.StatusCode;
            return CreateProducesResponseTypeAttribute(statusCode);
        }

        private static AttributeSyntax CreateProducesResponseTypeAttribute(int statusCode)
        {
            return Attribute(
                SimplifiedTypeName(SymbolNames.ProducesResponseTypeAttribute),
                AttributeArgumentList().AddArguments(
                    AttributeArgument(
                        LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(statusCode)))));
        }

        protected static AttributeSyntax CreateNameMatchAttribute(string nameMatchBehavior)
        {
            var attribute = Attribute(
                SimplifiedTypeName(SymbolNames.ApiConventionNameMatchAttribute),
                AttributeArgumentList().AddArguments(
                    AttributeArgument(
                        MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            SimplifiedTypeName(SymbolNames.ApiConventionNameMatchBehavior),
                            IdentifierName(nameMatchBehavior)))));
            return attribute;
        }

        protected static AttributeSyntax CreateTypeMatchAttribute(string typeMatchBehavior)
        {
            var attribute = Attribute(
                SimplifiedTypeName(SymbolNames.ApiConventionTypeMatchAttribute),
                AttributeArgumentList().AddArguments(
                    AttributeArgument(
                        MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            SimplifiedTypeName(SymbolNames.ApiConventionTypeMatchBehavior),
                            IdentifierName(typeMatchBehavior)))));
            return attribute;
        }

        protected internal static string GetConventionMethodName(string methodName)
        {
            // PostItem -> Post

            if (methodName.Length < 2)
            {
                return methodName;
            }

            for (var i = 1; i < methodName.Length; i++)
            {
                if (char.IsUpper(methodName[i]) && char.IsLower(methodName[i - 1]))
                {
                    return methodName.Substring(0, i);
                }
            }

            return methodName;
        }

        protected internal static string GetConventionParameterName(string parameterName)
        {
            // userName -> name

            if (parameterName.Length < 2)
            {
                return parameterName;
            }

            for (var i = parameterName.Length - 2; i > 0; i--)
            {
                if (char.IsUpper(parameterName[i]) && char.IsLower(parameterName[i - 1]))
                {
                    return char.ToLower(parameterName[i]) + parameterName.Substring(i + 1);
                }
            }

            return parameterName;
        }
    }
}
