using Microsoft.CodeAnalysis;

namespace Microsoft.AspNetCore.Mvc.Analyzers
{
    public static class AnalyzerUtils
    {
        public static (ITypeSymbol returnType, bool isTaskOActionResult) UnwrapReturnType(ApiControllerAnalyzerContext analyzerContext, IMethodSymbol method)
        {
            var returnType = method.ReturnType;
            returnType = UnwrapType(returnType, analyzerContext.SystemThreadingTaskOfT);
            var isTaskOfActionResult = returnType != method.ReturnType;

            returnType = UnwrapType(returnType, analyzerContext.ActionResultOfT);

            return (returnType, isTaskOfActionResult);

            ITypeSymbol UnwrapType(ITypeSymbol symbolToUnwrap, INamedTypeSymbol wrappingType)
            {
                if (returnType is INamedTypeSymbol namedReturnType 
                    && namedReturnType.ConstructedFrom != null 
                    && wrappingType.IsAssignableFrom(namedReturnType.ConstructedFrom))
                {
                    return namedReturnType.TypeArguments[0];
                }

                return symbolToUnwrap;
            }
        }
    }
}
