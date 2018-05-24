// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Microsoft.AspNetCore.Mvc.Analyzers
{
    public class ApiControllerAnalyzerContext
    {
#pragma warning disable RS1012 // Start action has no registered actions.
        public ApiControllerAnalyzerContext(CompilationStartAnalysisContext context)
#pragma warning restore RS1012 // Start action has no registered actions.
        {
            Context = context;
            ApiControllerAttribute = context.Compilation.GetTypeByMetadataName(TypeNames.ApiControllerAttribute);
        }

        public CompilationStartAnalysisContext Context { get; }

        public INamedTypeSymbol ApiControllerAttribute { get; }

        private INamedTypeSymbol _routeAttribute;
        public INamedTypeSymbol RouteAttribute => GetType(TypeNames.IRouteTemplateProvider, ref _routeAttribute);

        private INamedTypeSymbol _actionResultOfT;
        public INamedTypeSymbol ActionResultOfT => GetType(TypeNames.ActionResultOfT, ref _actionResultOfT);

        private INamedTypeSymbol _systemThreadingTask;
        public INamedTypeSymbol SystemThreadingTask => GetType(TypeNames.Task, ref _systemThreadingTask);

        private INamedTypeSymbol _systemThreadingTaskOfT;
        public INamedTypeSymbol SystemThreadingTaskOfT => GetType(TypeNames.TaskOfT, ref _systemThreadingTaskOfT);

        private INamedTypeSymbol _objectResult;
        public INamedTypeSymbol ObjectResult => GetType(TypeNames.ObjectResult, ref _objectResult);

        private INamedTypeSymbol _iActionResult;
        public INamedTypeSymbol IActionResult => GetType(TypeNames.IActionResult, ref _iActionResult);

        private INamedTypeSymbol _modelState;
        public INamedTypeSymbol ModelStateDictionary => GetType(TypeNames.ModelStateDictionary, ref _modelState);

        private INamedTypeSymbol _nonActionAttribute;
        public INamedTypeSymbol NonActionAttribute => GetType(TypeNames.NonActionAttribute, ref _nonActionAttribute);

        private INamedTypeSymbol _iApiResponseMetadataProvider;
        public INamedTypeSymbol IApiResponseMetadataProvider => GetType(TypeNames.IApiResponseMetadataProvider, ref _iApiResponseMetadataProvider);

        private INamedTypeSymbol _statusCodeAttribute;
        public INamedTypeSymbol StatusCodeAttribute => GetType(TypeNames.StatusCodeAttribute, ref _statusCodeAttribute);

        private INamedTypeSymbol _producesDefaultResponseAttribute;
        public INamedTypeSymbol ProducesDefaultResponseAttribute => GetType(TypeNames.ProducesDefaultResponseAttribute, ref _producesDefaultResponseAttribute);

        private INamedTypeSymbol _defaultApiConventions;
        public INamedTypeSymbol DefaultApiConventions => GetType(TypeNames.DefaultApiConventions, ref _defaultApiConventions);

        private INamedTypeSymbol _systemType;
        public INamedTypeSymbol SystemType => GetType(TypeNames.SystemType, ref _systemType);

        private INamedTypeSymbol GetType(string name, ref INamedTypeSymbol cache)
        {
            cache = cache ?? Context.Compilation.GetTypeByMetadataName(name);
            if (cache == null)
            {
                throw new ArgumentException($"Type {name} could not be found.");
            }

            return cache;
        }

        public bool IsApiAction(IMethodSymbol method)
        {
            return
                method.ContainingType.HasAttribute(ApiControllerAttribute, inherit: true) &&
                method.DeclaredAccessibility == Accessibility.Public &&
                method.MethodKind == MethodKind.Ordinary &&
                !method.IsGenericMethod &&
                !method.IsAbstract &&
                !method.IsStatic &&
                !method.HasAttribute(NonActionAttribute);
        }
    }
}
