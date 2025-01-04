using Durian.Info;
using Microsoft.CodeAnalysis;

namespace Durian.Analysis.InterfaceTargets;

/// <summary>
/// Contains <see cref="DiagnosticDescriptor"/>s of all the <see cref="Diagnostic"/>s that can be reported by the <see cref="InterfaceTargetsAnalyzer"/>.
/// </summary>
public static class InterfaceTargetsDiagnostics
{
	/// <summary>
	/// Provides a diagnostic message indicating that the target interface cannot be implemented by members of specified kind.
	/// </summary>
#pragma warning disable IDE1006 // Naming Styles
	public static readonly DiagnosticDescriptor DUR0401_InterfaceCannotBeImplementedByMembersOfThisKind = new(
		id: "DUR0401",
		title: "Interface is not valid on members of this kind",
		messageFormat: "'{0}': Interface '{1}' cannot be implemented by {2}",
		category: "Durian.InterfaceTargets",
		defaultSeverity: DiagnosticSeverity.Error,
		isEnabledByDefault: true,
		helpLinkUri: DocsPath + "/DUR0401.md"
	);

	/// <summary>
	/// Provides a diagnostic message indicating that the target interface cannot be a base of another interface.
	/// </summary>
	public static readonly DiagnosticDescriptor DUR0402_InterfaceCannotBeBaseOfAnotherInterface = new(
		id: "DUR0402",
		title: "Interface cannot be a base of another interface",
		messageFormat: "'{0}': Interface '{1}' cannot be a base of another interface",
		category: "Durian.InterfaceTargets",
		defaultSeverity: DiagnosticSeverity.Error,
		isEnabledByDefault: true,
		helpLinkUri: DocsPath + "/DUR0402.md"
	);

	/// <summary>
	/// Provides a diagnostic message indicating that the target interface is not accessible directly in code.
	/// </summary>
	public static readonly DiagnosticDescriptor DUR0403_InterfaceIsNotDirectlyAccessible = new(
		id: "DUR0403",
		title: "Interface is accessible only through reflection",
		messageFormat: "'{0}': Interface '{1}' is accessible only through reflection",
		category: "Durian.InterfaceTargets",
		defaultSeverity: DiagnosticSeverity.Error,
		isEnabledByDefault: true,
		helpLinkUri: DocsPath + "/DUR0403.md"
	);

	/// <summary>
	/// Provides a diagnostic message indicating that the target interface is applied with invalid constraints.
	/// </summary>
	public static readonly DiagnosticDescriptor DUR0404_InvalidConstraint = new(
		id: "DUR0404",
		title: "Interface will never match target constraint",
		messageFormat: "'{0}': Interface '{1}' will never match a '{2}' constraint",
		category: "Durian.InterfaceTargets",
		defaultSeverity: DiagnosticSeverity.Warning,
		isEnabledByDefault: true,
		helpLinkUri: DocsPath + "/DUR0404.md"
	);
#pragma warning restore IDE1006 // Naming Styles

	/// <summary>
	/// Documentation directory of the <c>InterfaceTargets</c> module.
	/// </summary>
	public static string DocsPath => GlobalInfo.Repository + "/tree/master/docs/InterfaceTargets";
}
