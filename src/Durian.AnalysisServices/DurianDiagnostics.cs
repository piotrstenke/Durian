using Durian.Info;
using Microsoft.CodeAnalysis;
using Durian.Generator;

namespace Durian.Generator
{
	/// <summary>
	/// Contains <see cref="DiagnosticDescriptor"/>s of the most common Durian errors.
	/// </summary>
	public static class DurianDiagnostics
	{
		/// <summary>
		/// Documentation directory of the <c>AnalysisCore</c> module.
		/// </summary>
		public static string DocsPath => ModuleIdentity.GetModule(DurianModule.AnalysisServices).Documentation;

		/// <summary>
		/// Provides diagnostic message indicating that the target project must reference the <c>Durian.Core</c> package.
		/// </summary>
		[WithoutLocation]
		public static readonly DiagnosticDescriptor DUR0001_ProjectMustReferenceDurianCore = new(
			id: "DUR0001",
			title: "Projects with any Durian analyzer must reference the Durian.Core package",
			messageFormat: "Projects with any Durian analyzer must reference the Durian.Core package",
			category: "Durian",
			defaultSeverity: DiagnosticSeverity.Error,
			helpLinkUri: DocsPath + @"\DUR0001.md",
			isEnabledByDefault: true
		);

		/// <summary>
		/// Provides diagnostic message indicating that the target type cannot be accessed, because its module is not imported
		/// </summary>
		[WithoutLocation]
		public static readonly DiagnosticDescriptor DUR0002_ModuleOfTypeIsNotImported = new(
			id: "DUR0002",
			title: "Type cannot be accessed, because its module is not imported",
			messageFormat: "Type '{0}' cannot be accessed, because the '{1}' module is not imported",
			category: "Durian",
			defaultSeverity: DiagnosticSeverity.Warning,
			helpLinkUri: DocsPath + @"\DUR0002.md",
			isEnabledByDefault: true
		);
	}
}
