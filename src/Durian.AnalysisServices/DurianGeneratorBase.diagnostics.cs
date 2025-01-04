using Durian.Info;
using Microsoft.CodeAnalysis;

namespace Durian.Analysis;

partial class DurianGeneratorBase
{
	#region Diagnostics copied from Durian.Core.Analyzer

#pragma warning disable IDE1006 // Naming Styles
	private static readonly DiagnosticDescriptor DUR0001_ProjectMustReferenceDurianCore = new(
#pragma warning disable RS2008 // Enable analyzer release tracking
		id: "DUR0001",
#pragma warning restore RS2008 // Enable analyzer release tracking
		title: "Projects with any Durian analyzer must reference the Durian.Core package",
		messageFormat: "Projects with any Durian analyzer must reference the Durian.Core package",
		category: "Durian",
		defaultSeverity: DiagnosticSeverity.Error,
		isEnabledByDefault: true,
		helpLinkUri: GlobalInfo.Repository + "/tree/master/docs/Core/DUR0001.md");

	private static readonly DiagnosticDescriptor DUR0004_DurianModulesAreValidOnlyInCSharp = new(
#pragma warning disable RS2008 // Enable analyzer release tracking
		id: "DUR0004",
#pragma warning restore RS2008 // Enable analyzer release tracking
		title: "Durian modules can be used only in C#",
		messageFormat: "Durian modules can be used only in C#",
		category: "Durian",
		defaultSeverity: DiagnosticSeverity.Error,
		isEnabledByDefault: true,
		helpLinkUri: GlobalInfo.Repository + "/tree/master/docs/Core/DUR0004.md");
#pragma warning restore IDE1006 // Naming Styles

	#endregion Diagnostics copied from Durian.Core.Analyzer
}
