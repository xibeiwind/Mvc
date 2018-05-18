using Microsoft.CodeAnalysis;

namespace Microsoft.AspNetCore.Mvc.Analyzers
{
    public static class AnalyzerUtils
    {
        public static (ITypeSymbol returnType, bool isTaskOActionResult) UnwrapReturnType(ApiControllerAnalyzerContext analyzerContext, IMethodSymbol method)
        {
            var returnType = method.ReturnType;
            //if (returnType is INamedTypeSymbol namedReturnType && namedReturnType.ConstructedFrom != null && analyzerContext.SystemThreadingTaskOfT.IsAssignableFrom(namedReturnType.ConstructedFrom))
            //{
            //    // Unwrap Task<T>.
            //    returnType = namedReturnType.TypeArguments[0];
            //}

            //if (returnType is INamedTypeSymbol namedReturnType && namedReturnType.ConstructedFrom != null && analyzerContext.SystemThreadingTaskOfT.IsAssignableFrom(namedReturnType.ConstructedFrom))
            //{

            //}

                return (returnType, returnType == method.ReturnType);

            ITypeSymbol UnwrapType(ITypeSymbol symbolToUnwrap, INamedTypeSymbol wrappingType)
            {
                if (returnType is INamedTypeSymbol namedReturnType 
                    && namedReturnType.ConstructedFrom != null 
                    && wrappingType.IsAssignableFrom(namedReturnType.ConstructedFrom))
                {
                    return namedReturnType.TypeArguments[0];
                }
            }
        }
    }
}
