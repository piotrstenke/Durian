﻿// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Durian.Info;
using Microsoft.CodeAnalysis;

namespace Durian.Analysis
{
    /// <summary>
    /// Contains <see cref="DiagnosticDescriptor"/> s of the most common Durian errors.
    /// </summary>
    public static class DurianDiagnostics
    {
        /// <summary>
        /// Provides diagnostic message indicating that the target project must reference the
        /// <c>Durian.Core</c> package.
        /// </summary>
        [WithoutLocation]
        public static readonly DiagnosticDescriptor DUR0001_ProjectMustReferenceDurianCore = new(
            id: "DUR0001",
            title: "Projects with any Durian analyzer must reference the Durian.Core package",
            messageFormat: "Projects with any Durian analyzer must reference the Durian.Core package",
            category: "Durian",
            defaultSeverity: DiagnosticSeverity.Error,
            helpLinkUri: DocsPath + "/DUR0001.md",
            isEnabledByDefault: true
        );

        /// <summary>
        /// Provides diagnostic message indicating that the target type cannot be accessed, because
        /// its module is not imported.
        /// </summary>
        public static readonly DiagnosticDescriptor DUR0002_ModuleOfTypeIsNotImported = new(
            id: "DUR0002",
            title: "Type cannot be accessed, because its module is not imported",
            messageFormat: "Type '{0}' cannot be accessed, because the '{1}' module is not imported",
            category: "Durian",
            defaultSeverity: DiagnosticSeverity.Error,
            helpLinkUri: DocsPath + "/DUR0002.md",
            isEnabledByDefault: true
        );

        /// <summary>
        /// Provides diagnostic message indicating that the user should not use types from the
        /// Durian.Generator namespace.
        /// </summary>
        public static readonly DiagnosticDescriptor DUR0003_DoNotUseTypeFromDurianGeneratorNamespace = new(
            id: "DUR0003",
            title: "Do not use types from the Durian.Generator namespace",
            messageFormat: "Do not use types from the Durian.Generator namespace",
            category: "Durian",
            defaultSeverity: DiagnosticSeverity.Error,
            helpLinkUri: DocsPath + "/DUR0003.md",
            isEnabledByDefault: true
        );

        /// <summary>
        /// Provides diagnostic message indicating that Durian modules can be used only in C#.
        /// </summary>
        [WithoutLocation]
        public static readonly DiagnosticDescriptor DUR0004_DurianModulesAreValidOnlyInCSharp = new(
            id: "DUR0004",
            title: "Durian modules can be used only in C#",
            messageFormat: "Durian modules can be used only in C#",
            category: "Durian",
            defaultSeverity: DiagnosticSeverity.Error,
            helpLinkUri: DocsPath + "/DUR0004.md",
            isEnabledByDefault: true
        );

        /// <summary>
        /// Provides diagnostic message indicating that Durian modules can be used only in CSharp.
        /// </summary>
        public static readonly DiagnosticDescriptor DUR0005_DoNotAddTypesToGeneratorNamespace = new(
            id: "DUR0005",
            title: "Do not add custom types to the Durian.Generator namespace",
            messageFormat: "Do not add custom types to the Durian.Generator namespace",
            category: "Durian",
            defaultSeverity: DiagnosticSeverity.Error,
            helpLinkUri: DocsPath + "/DUR0005.md",
            isEnabledByDefault: true
        );

        /// <summary>
        /// Provides diagnostic message indicating that a Durian package containing analyzers should not be referenced if the main Durian package is present.
        /// </summary>
        [WithoutLocation]
        public static readonly DiagnosticDescriptor DUR0007_DoNotReferencePackageIfManagerIsPresent = new(
            id: "DUR0007",
            title: "Do not reference Durian analyzer package if the main Durian package is already included",
            messageFormat: "Do not reference the '{0}' package if the main Durian package is already included",
            category: "Durian",
            defaultSeverity: DiagnosticSeverity.Error,
            helpLinkUri: DocsPath + "/DUR0007.md",
            isEnabledByDefault: true
        );

        /// <summary>
        /// Provides diagnostic message indicating that the project references multiple Durian analyzers, and should reference the main Durian package instead.
        /// </summary>
        [WithoutLocation]
        public static readonly DiagnosticDescriptor DUR0008_MultipleAnalyzers = new(
            id: "DUR0008",
            title: "Separate analyzer packages detected, reference the main Durian package instead for better performance",
            messageFormat: "Separate analyzer packages detected, reference the main Durian package instead for better performance",
            category: "Durian",
            defaultSeverity: DiagnosticSeverity.Warning,
            helpLinkUri: DocsPath + "/DUR0008.md",
            isEnabledByDefault: true
        );

        /// <summary>
        /// Documentation directory of the <c>Core</c> module.
        /// </summary>
        public static string DocsPath => GlobalInfo.Repository + "/tree/master/docs/Core";
    }
}