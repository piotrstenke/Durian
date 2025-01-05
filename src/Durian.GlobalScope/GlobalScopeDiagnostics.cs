using System;
using Durian.Info;
using Microsoft.CodeAnalysis;

namespace Durian.Analysis.GlobalScope;

/// <summary>
/// Contains <see cref="DiagnosticDescriptor"/>s of all the <see cref="Diagnostic"/>s that can be reported by either <see cref="GlobalScopeDeclarationAnalyzer"/>.
/// </summary>
public static class GlobalScopeDiagnostics
{
#pragma warning disable IDE1006 // Naming Styles
	/// <summary>
	/// Provides a diagnostic message indicating that <see cref="Type"/> marked with the <c>Durian.GlobalScopeAttribute</c> is not a static class.
	/// </summary>
	public static readonly DiagnosticDescriptor DUR0501_TypeIsNotStaticClass = new(
		id: "DUR0501",
		title: "Type marked with the GlobalScopeAttribute must be static",
		messageFormat: "'{0}': Type marked with the GlobalScopeAttribute must be static",
		category: "Durian.GlobalScope",
		defaultSeverity: DiagnosticSeverity.Error,
		isEnabledByDefault: true,
		helpLinkUri: DocsPath + "/DUR0501.md"
	);

	/// <summary>
	/// Provides a diagnostic message indicating that <see cref="Type"/> marked with the <c>Durian.GlobalScopeAttribute</c> cannot be nested within other type.
	/// </summary>
	public static readonly DiagnosticDescriptor DUR0502_TypeIsNotTopLevel = new(
		id: "DUR0502",
		title: "Type marked with the GlobalScopeAttribute cannot be a nested type",
		messageFormat: "'{0}': Type marked with the GlobalScopeAttribute cannot be a nested type",
		category: "Durian.GlobalScope",
		defaultSeverity: DiagnosticSeverity.Error,
		isEnabledByDefault: true,
		helpLinkUri: DocsPath + "/DUR0502.md"
	);

	///// <summary>
	///// Provides a diagnostic message indicating that <see cref="Type"/> marked with the <c>Durian.GlobalScopeAttribute</c> cannot be accessed.
	///// </summary>
	//public static readonly DiagnosticDescriptor DUR0503_MemberNotAccessible = new(
	//	id: "DUR0503",
	//	title: "Top level member cannot be accessed without importing the parent namespace",
	//	messageFormat: "'{0}': Top level member cannot be accessed without importing the parent namespace",
	//	category: "Durian.GlobalScope",
	//	defaultSeverity: DiagnosticSeverity.Error,
	//	isEnabledByDefault: true,
	//	helpLinkUri: DocsPath + "/DUR0503.md"
	//);

#pragma warning restore IDE1006 // Naming Styles

	/// <summary>
	/// Documentation directory of the <c>GlobalScope</c> module.
	/// </summary>
	public static string DocsPath => GlobalInfo.Repository + "/tree/master/docs/GlobalScope";
}
