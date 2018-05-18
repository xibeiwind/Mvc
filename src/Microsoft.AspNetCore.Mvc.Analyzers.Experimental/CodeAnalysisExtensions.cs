// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.CodeAnalysis;

namespace Microsoft.AspNetCore.Mvc.Analyzers
{
    internal static class CodeAnalysisExtensions
    {
        public static bool HasAttribute(this ITypeSymbol typeSymbol, INamedTypeSymbol attribute, bool inherit)
        {
            return typeSymbol.GetAttributeData(attribute, inherit) != null;
        }

        public static AttributeData GetAttributeData(this ITypeSymbol typeSymbol, INamedTypeSymbol attribute, bool inherit)
        {
            do
            {
                var attributeData = typeSymbol.GetAttributeData(attribute);
                if (attributeData != null)
                {
                    return attributeData;
                }

                typeSymbol = typeSymbol.BaseType;
            } while (inherit && typeSymbol != null);

            return null;
        }

        public static bool HasAttribute(this ISymbol symbol, INamedTypeSymbol attribute)
            => symbol.GetAttributeData(attribute) != null;

        public static IEnumerable<AttributeData> GetAttributeDataItems(this ISymbol symbol, INamedTypeSymbol attributeType)
        {
            foreach (var declaredAttribute in symbol.GetAttributes())
            {
                if (attributeType.IsAssignableFrom(declaredAttribute.AttributeClass))
                {
                    yield return declaredAttribute;
                }
            }
        }

        public static AttributeData GetAttributeData(this ISymbol symbol, INamedTypeSymbol attributeType)
        {
            foreach (var declaredAttribute in symbol.GetAttributes())
            {
                if (attributeType.IsAssignableFrom(declaredAttribute.AttributeClass))
                {
                    return declaredAttribute;
                }
            }

            return null;
        }

        public static bool IsAssignableFrom(this INamedTypeSymbol target, ITypeSymbol source)
        {
            Debug.Assert(source != null);
            Debug.Assert(target != null);

            if (source == target)
            {
                return true;
            }

            if (target.TypeKind == TypeKind.Interface)
            {
                foreach (var @interface in source.AllInterfaces)
                {
                    if (@interface == target)
                    {
                        return true;
                    }
                }

                return false;
            }

            do
            {
                if (source == target)
                {
                    return true;
                }

                source = source.BaseType;
            } while (source != null);

            return false;
        }
    }
}
