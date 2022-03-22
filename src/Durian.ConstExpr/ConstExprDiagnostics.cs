// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Durian.Info;
using Microsoft.CodeAnalysis;

namespace Durian.ConstExpr
{
	/// <summary>
	/// Contains <see cref="DiagnosticDescriptor"/>s of all the <see cref="Diagnostic"/>s that can be reported by the <see cref="ConstExprGenerator"/> or one of the analyzers.
	/// </summary>
	public static class ConstExprDiagnostics
	{
		/// <summary>
		/// Provides a diagnostic message indicating that a containing type of a member marked with the <c>Durian.ConstExprSourceAttribute</c> must be <see langword="partial"/>.
		/// </summary>
		public static readonly DiagnosticDescriptor DUR0501_ContainingTypeMustBePartial = new(
			id: "DUR0501",
			title: "Containing type of a member with the ConstExprSourceAttribute must be partial",
			messageFormat: "'{0}': Containing type of a member with the ConstExprSourceAttribute must be partial",
			category: "Durian.ConstExpr",
			defaultSeverity: DiagnosticSeverity.Error,
			helpLinkUri: DocsPath + "/DUR0501.md",
			isEnabledByDefault: true
		);

		/// <summary>
		/// Provides a diagnostic message indicating that a member marked with the <c>Durian.ConstExprSourceAttribute</c> must be <see langword="partial"/>.
		/// </summary>
		public static readonly DiagnosticDescriptor DUR0502_MemberMustBePartial = new(
			id: "DUR0502",
			title: "Member marked with the ConstExprSourceAttribute must be partial",
			messageFormat: "'{0}': Member marked with the ConstExprSourceAttribute must be partial",
			category: "Durian.ConstExpr",
			defaultSeverity: DiagnosticSeverity.Error,
			helpLinkUri: DocsPath + "/DUR0502.md",
			isEnabledByDefault: true
		);

		/// <summary>
		/// Provides a diagnostic message indicating that the value of <c>Durian.ConstExprSourceAttribute.Source</c> property cannot be resolved.
		/// </summary>
		public static readonly DiagnosticDescriptor DUR0503_MemberCannotBeResolved = new(
			id: "DUR0503",
			title: "Target member cannot be resolved",
			messageFormat: "'{0}': Member '{1}' cannot be resolved",
			category: "Durian.ConstExpr",
			defaultSeverity: DiagnosticSeverity.Error,
			helpLinkUri: DocsPath + "/DUR0503.md",
			isEnabledByDefault: true
		);

		/// <summary>
		/// Provides a diagnostic message indicating that a member resolved from the value of <c>Durian.ConstExprSourceAttribute.Source</c> property is not compatible with the current member kind.
		/// </summary>
		public static readonly DiagnosticDescriptor DUR0504_WrongTargetMemberKind = new(
			id: "DUR0504",
			title: "Target member is not compatible",
			messageFormat: "'{0}': Member '{1}' is not compatible with the current member",
			category: "Durian.ConstExpr",
			defaultSeverity: DiagnosticSeverity.Error,
			helpLinkUri: DocsPath + "/DUR0504.md",
			isEnabledByDefault: true
		);

		/// <summary>
		/// Provides a diagnostic message indicating that the target implementation is not accessible.
		/// </summary>
		public static readonly DiagnosticDescriptor DUR0505_ImplementationNotAccessible = new(
			id: "DUR0505",
			title: "Implementation of the target member is not accessible",
			messageFormat: "'{0}': Implementation of member '{1}' is not accessible",
			category: "Durian.ConstExpr",
			defaultSeverity: DiagnosticSeverity.Error,
			helpLinkUri: DocsPath + "/DUR0505.md",
			isEnabledByDefault: true
		);

		/// <summary>
		/// Provides a diagnostic message indicating that two <c>Durian.ConstExprSourceAttribute</c>s are equivalent.
		/// </summary>
		public static readonly DiagnosticDescriptor DUR0506_EquivalentTarget = new(
			id: "DUR0506",
			title: "Equivalent ConstExprSourceAttribute already specified",
			messageFormat: "'{0}': Equivalent ConstExprSourceAttribute already specified",
			category: "Durian.ConstExpr",
			defaultSeverity: DiagnosticSeverity.Warning,
			helpLinkUri: DocsPath + "/DUR0506.md",
			isEnabledByDefault: true
		);

		/// <summary>
		/// Provides a diagnostic message indicating that a member with name the same as <c>Durian.ConstExprSourceAttribute.Name</c> already exists.
		/// </summary>
		public static readonly DiagnosticDescriptor DUR0507_MemberAlreadyExists = new(
			id: "DUR0507",
			title: "Member with target name already exists",
			messageFormat: "'{0}': Member with name '{1}' already exists",
			category: "Durian.ConstExpr",
			defaultSeverity: DiagnosticSeverity.Error,
			helpLinkUri: DocsPath + "/DUR0507.md",
			isEnabledByDefault: true
		);

		/// <summary>
		/// Provides a diagnostic message indicating that a <c>Durian.ConstExprAttribute</c> was specified on unsupported method kind.
		/// </summary>
		public static readonly DiagnosticDescriptor DUR0508_UnsupportedMethodKind = new(
			id: "DUR0508",
			title: "Durian.ConstExprAttribute cannot be specified on a method of this kind",
			messageFormat: "'{0}': Durian.ConstExprAttribute cannot be specified on a method of this kind",
			category: "Durian.ConstExpr",
			defaultSeverity: DiagnosticSeverity.Error,
			helpLinkUri: DocsPath + "/DUR0508.md",
			isEnabledByDefault: true
		);

		/// <summary>
		/// Provides a diagnostic message indicating that a the value of <c>Durian.ConstExprSourceAttribute.Name</c> is not valid.
		/// </summary>
		public static readonly DiagnosticDescriptor DUR0509_InvalidName = new(
			id: "DUR0509",
			title: "Value is not a valid name",
			messageFormat: "'{0}': Value '{1}' is not a valid name",
			category: "Durian.ConstExpr",
			defaultSeverity: DiagnosticSeverity.Error,
			helpLinkUri: DocsPath + "/DUR0509.md",
			isEnabledByDefault: true
		);

		/// <summary>
		/// Provides a diagnostic message indicating the number of arguments specified in the <c>Durian.ConstExprSourceAttribute</c> does not match the actual number of arguments in the target method.
		/// </summary>
		public static readonly DiagnosticDescriptor DUR0510_WrongNumberOfArguments = new(
			id: "DUR0510",
			title: "Wrong number of arguments",
			messageFormat: "'{0}': Wrong number of arguments: {1}, expected: {2}",
			category: "Durian.ConstExpr",
			defaultSeverity: DiagnosticSeverity.Error,
			helpLinkUri: DocsPath + "/DUR0510.md",
			isEnabledByDefault: true
		);

		/// <summary>
		/// Provides a diagnostic message indicating the an argument of <c>Durian.ConstExprSourceAttribute</c> has wrong type.
		/// </summary>
		public static readonly DiagnosticDescriptor DUR0511_ArgumentTypeMismatch = new(
			id: "DUR0511",
			title: "Wrong argument type",
			messageFormat: "'{0}': Wrong argument type at position: {1}, expected: {2}",
			category: "Durian.ConstExpr",
			defaultSeverity: DiagnosticSeverity.Error,
			helpLinkUri: DocsPath + "/DUR0511.md",
			isEnabledByDefault: true
		);

		/// <summary>
		/// Documentation directory of the <c>DefaultParam</c> module.
		/// </summary>
		public static string DocsPath => GlobalInfo.Repository + "/tree/master/docs/ConstExpr";
	}
}
