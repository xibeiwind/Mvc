// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.CodeAnalysis;

namespace Microsoft.AspNetCore.Mvc.Analyzers
{
    public static class DiagnosticDescriptors
    {
        public static readonly DiagnosticDescriptor MVC7000_ApiActionsMustBeAttributeRouted =
            new DiagnosticDescriptor(
                "MVC7000",
                "Actions on types annotated with ApiControllerAttribute must be attribute routed.",
                "Actions on types annotated with ApiControllerAttribute must be attribute routed.",
                "Usage",
                DiagnosticSeverity.Warning,
                isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor MVC7001_ApiActionsHaveBadModelStateFilter =
            new DiagnosticDescriptor(
                "MVC7001",
                "Actions on types annotated with ApiControllerAttribute do not require explicit ModelState validity check.",
                "Actions on types annotated with ApiControllerAttribute do not require explicit ModelState validity check.",
                "Usage",
                DiagnosticSeverity.Warning,
                isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor MVC7002_ApiActionsShouldReturnActionResultOf =
            new DiagnosticDescriptor(
                "MVC7002",
                "Actions on types annotated with ApiControllerAttribute should return ActionResult<T>.",
                "Actions on types annotated with ApiControllerAttribute should return ActionResult<T>.",
                "Usage",
                DiagnosticSeverity.Warning,
                isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor MVC7003_ActionsMustNotBeAsyncVoid =
            new DiagnosticDescriptor(
                "MVC7003",
                "Controller actions must not have async void signature.",
                "Controller actions must not have async void signature.",
                "Usage",
                DiagnosticSeverity.Warning,
                isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor MVC7004_ApiActionIsMissingMetadata =
            new DiagnosticDescriptor(
                "MVC7004",
                "API action returns an action result with status code {0} without an ApiExplorer thing for it.",
                "API action returns an action result with status code {0} without an ApiExplorer thing for it.",
                "Usage",
                DiagnosticSeverity.Warning,
                isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor MVC7005_ApiActionIsMissingResponse =
            new DiagnosticDescriptor(
                "MVC7005",
                "API action claims to return an action result with status code '{0}' but no result was found that matches this constraint.",
                "API action claims to return an action result with status code '{0}' but no result was found that matches this constraint.",
                "Usage",
                DiagnosticSeverity.Info,
                isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor MVC7006_ApiActionIsMissingProducesAttribute =
            new DiagnosticDescriptor(
                "MVC7006",
                "API action returns value '{0}' but does not specify a ProducesAttribute.",
                "API action returns value '{0}' but does not specify a ProducesAttribute.",
                "Usage",
                DiagnosticSeverity.Warning,
                isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor MVC7007_ApiActionIsMissingDefaultResponse =
            new DiagnosticDescriptor(
                "MVC7005",
                "API action claims to return default response but no return type '{0}' was found.",
                "API action claims to return an action result with status code '{0}' but no result was found that matches this constraint.",
                "Usage",
                DiagnosticSeverity.Info,
                isEnabledByDefault: true);
    }
}
