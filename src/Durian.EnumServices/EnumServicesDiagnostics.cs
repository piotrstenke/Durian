// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using Durian.Info;
using Microsoft.CodeAnalysis;

namespace Durian.Analysis.EnumServices
{
	/// <summary>
	/// Contains <see cref="DiagnosticDescriptor"/>s of all the <see cref="Diagnostic"/>s that can be reported by either <see cref="EnumServicesAnalyzer"/> or <see cref="EnumServicesGenerator"/>.
	/// </summary>
	public static class EnumServicesDiagnostics
	{
		/// <summary>
		/// Provides diagnostic message indicating that the <see cref="Durian.EnumServices"/> or <see cref="GeneratedTypeAccess"/> value of <see cref="EnumServicesAttribute"/> is not valid.
		/// </summary>
		public static readonly DiagnosticDescriptor DUR0201_InvalidEnumValue = new(
			id: "DUR0201",
			title: "Invalid enum value",
			messageFormat: "'{0}': Invalid enum value",
			category: "Durian.EnumServices",
			defaultSeverity: DiagnosticSeverity.Error,
			helpLinkUri: DocsPath + "/DUR0201.md",
			isEnabledByDefault: true
		);

		/// <summary>
		/// Provides diagnostic message indicating that the value of <see cref="EnumServicesAttribute.Prefix"/>, <see cref="EnumServicesAttribute.ClassName"/> or <see cref="EnumServicesAttribute.Namespace"/> is not valid.
		/// </summary>
		public static readonly DiagnosticDescriptor DUR0202_InvalidName = new(
			id: "DUR0202",
			title: "Invalid prefix or identifier",
			messageFormat: "'{0}': Invalid prefix or identifier",
			category: "Durian.EnumServices",
			defaultSeverity: DiagnosticSeverity.Error,
			helpLinkUri: DocsPath + "/DUR0202.md",
			isEnabledByDefault: true
		);

		/// <summary>
		/// Provides diagnostic message indicating that the class specified in <see cref="EnumServicesAttribute.ClassName"/> must be static and partial.
		/// </summary>
		public static readonly DiagnosticDescriptor DUR0203_NotStaticOrPartial = new(
			id: "DUR0203",
			title: "Target class must be static and partial",
			messageFormat: "'{0}': Target class must be static and partial",
			category: "Durian.EnumServices",
			defaultSeverity: DiagnosticSeverity.Error,
			helpLinkUri: DocsPath + "/DUR0203.md",
			isEnabledByDefault: true
		);

		/// <summary>
		/// Provides diagnostic message indicating that the value of <see cref="EnumServicesAttribute.Accessibility"/> is different than the actual accessibility.
		/// </summary>
		public static readonly DiagnosticDescriptor DUR0204_EnumAccessbilityLessThanGeneratedExtension = new(
			id: "DUR0204",
			title: "Target enum cannot be less accessible than its generated extension methods",
			messageFormat: "'{0}': Target enum cannot be less accessible than its generated extension methods",
			category: "Durian.EnumServices",
			defaultSeverity: DiagnosticSeverity.Error,
			helpLinkUri: DocsPath + "/DUR0204.md",
			isEnabledByDefault: true
		);

		/// <summary>
		/// Provides diagnostic message indicating that <see cref="EnumServicesAttribute"/> should not be
		/// applied on types with no inner enums.
		/// </summary>
		public static readonly DiagnosticDescriptor DUR0205_GeneratedAcessibilityGreatedThanExisting = new(
			id: "DUR0205",
			title: "Accessibility of generated extension methods cannot be greater than that of their containing type",
			messageFormat: "'{0}': Accessibility of generated extension methods cannot be greater than that of their containing type",
			category: "Durian.EnumServices",
			defaultSeverity: DiagnosticSeverity.Error,
			helpLinkUri: DocsPath + "/DUR0205.md",
			isEnabledByDefault: true
		);

		/// <summary>
		/// Provides diagnostic message indicating that flag-related methods
		/// cannot be generated for enum without the <see cref="FlagsAttribute"/> applied.
		/// </summary>
		public static readonly DiagnosticDescriptor DUR0206_FlagsOnlyMethodsOnNonFlagEnum = new(
			id: "DUR0206",
			title: "Flags-only methods cannot be generated for non-flags enum",
			messageFormat: "'{0}': Flags-only methods cannot be generated for non-flags enum",
			category: "Durian.EnumServices",
			defaultSeverity: DiagnosticSeverity.Warning,
			helpLinkUri: DocsPath + "/DUR0206.md",
			isEnabledByDefault: true
		);

		/// <summary>
		/// Provides diagnostic message indicating that <see cref="EnumServicesAttribute"/> should not be
		/// applied on types with no inner enums.
		/// </summary>
		public static readonly DiagnosticDescriptor DUR0207_NoEnumInScope = new(
			id: "DUR0207",
			title: "Don't use EnumServices on types without inner enums",
			messageFormat: "'{0}': Don't use EnumServices on types without inner enums",
			category: "Durian.EnumServices",
			defaultSeverity: DiagnosticSeverity.Warning,
			helpLinkUri: DocsPath + "/DUR0207.md",
			isEnabledByDefault: true
		);

		/// <summary>
		/// Provides diagnostic message indicating that <see cref="EnumServicesAttribute"/> should not be
		/// applied on types with no inner enums.
		/// </summary>
		public static readonly DiagnosticDescriptor DUR0208_MemberAlreadyExists = new(
			id: "DUR0208",
			title: "Extension method already exists, so it won't be generated",
			messageFormat: "'{0}': Extension method '{1}' already exists, so it won't be generated",
			category: "Durian.EnumServices",
			defaultSeverity: DiagnosticSeverity.Warning,
			helpLinkUri: DocsPath + "/DUR0208.md",
			isEnabledByDefault: true
		);

		/// <summary>
		/// Documentation directory of the <c>EnumServices</c> module.
		/// </summary>
		public static string DocsPath => GlobalInfo.Repository + "/tree/master/docs/EnumServices";
	}
}
