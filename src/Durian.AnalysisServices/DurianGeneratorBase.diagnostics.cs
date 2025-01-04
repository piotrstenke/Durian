using Durian.Info;
using Microsoft.CodeAnalysis;

namespace Durian.Analysis
{
	public abstract partial class DurianGeneratorBase
	{
		#region Diagnostics copied from Durian.Core.Analyzer

		private static readonly DiagnosticDescriptor DUR0001_ProjectMustReferenceDurianCore = new(
			id: "DUR0001",
			title: "Projects with any Durian analyzer must reference the Durian.Core package",
			messageFormat: "Projects with any Durian analyzer must reference the Durian.Core package",
			category: "Durian",
			defaultSeverity: DiagnosticSeverity.Error,
			helpLinkUri: GlobalInfo.Repository + "/tree/master/docs/Core/DUR0001.md",
			isEnabledByDefault: true
		);

		private static readonly DiagnosticDescriptor DUR0004_DurianModulesAreValidOnlyInCSharp = new(
			id: "DUR0004",
			title: "Durian modules can be used only in C#",
			messageFormat: "Durian modules can be used only in C#",
			category: "Durian",
			defaultSeverity: DiagnosticSeverity.Error,
			helpLinkUri: GlobalInfo.Repository + "/tree/master/docs/Core/DUR0004.md",
			isEnabledByDefault: true
		);

		#endregion Diagnostics copied from Durian.Core.Analyzer
	}
}
