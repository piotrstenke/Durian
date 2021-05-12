namespace Durian.Generator
{
	/// <summary>
	/// Factory class of <see cref="ModuleIdentity"/> for all available Durian modules.
	/// </summary>
	public static class ModuleRepository
	{
		/// <summary>
		/// Returns a <see cref="ModuleIdentity"/> for the <c>Durian.Core</c> module.
		/// </summary>
		public static ModuleIdentity Core => new(
			name: "Durian.Core",
			version: "1.0.0",
			module: DurianModule.Core,
			type: ModuleType.Library,
			id: default
		);

		/// <summary>
		/// Returns a <see cref="ModuleIdentity"/> for the <c>Durian.AnalysisCore</c> module.
		/// </summary>
		public static ModuleIdentity AnalysisCore => new(
			name: "Durian.AnalysisCore",
			version: "1.0.0",
			module: DurianModule.AnalysisCore,
			type: ModuleType.Library,
			id: 00,
			docPath: @"docs\AnalysisCore",
			diagnostics: new[]
			{
				new DiagnosticData("Member name is reserved for internal purposes", 01, "DUR0001.md", true, true),
				new DiagnosticData("Member with name already exists", 02, "DUR0002.md", true, true),
				new DiagnosticData("Member names cannot be the same as their enclosing type", 03, "DUR0003.md", true, true),
				new DiagnosticData("Member name could not be resolved", 04, "DUR0004.md", true, true),
				new DiagnosticData("Name refers to multiple members", 05, "DUR0005.md", true, true),
				new DiagnosticData("Members from outside of the current assembly cannot be accessed", 06, "DUR0006.md", true, true),
				new DiagnosticData("Member cannot refer to itself", 07, "DUR0007.md", true, true),
				new DiagnosticData("Target member contains errors, and cannot be referenced", 08, "DUR0008.md", true, true),
				new DiagnosticData("Method with the specified signature already exists", 09, "DUR0009.md", true, true),
				new DiagnosticData("Projects with any Durian analyzer must reference the Durian.Core package", 10, "DUR0010.md", true, false),
			}
		);

		/// <summary>
		/// Returns a <see cref="ModuleIdentity"/> for the <c>Durian.RoslynTestServices</c> module.
		/// </summary>
		public static ModuleIdentity RoslynTestServices => new(
			name: "Durian.RoslynTestServices",
			version: "1.0.0",
			module: DurianModule.RoslynTestServices,
			type: ModuleType.Library,
			id: default
		);

		/// <summary>
		/// Returns a <see cref="ModuleIdentity"/> for the <c>Durian.DefaultParam</c> module.
		/// </summary>
		public static ModuleIdentity DefaultParam => new(
			name: "Durian.DefaultParam",
			version: "1.0.0",
			module: DurianModule.DefaultParam,
			type: ModuleType.SyntaxBasedGenerator | ModuleType.StaticGenerator | ModuleType.Analyzer,
			id: 01,
			docPath: @"docs\DefaultParam",
			staticTrees: new[]
			{
				new StaticTreeIdentity("DefaultParamAttribute", "Durian", StaticTreeType.Attribute),
				new StaticTreeIdentity("DefaultParamConfigurationAttribute", "Durian.Configuration", StaticTreeType.Attribute),
				new StaticTreeIdentity("DPMethodGenConvention", "Durian.Configuration", StaticTreeType.Enum),
				new StaticTreeIdentity("DPTypeGenConvention", "Durian.Configuration", StaticTreeType.Enum)
			},
			diagnostics: new[]
			{
				new DiagnosticData("Containing type of a member with the DefaultParam attribute must be partial", 01, "DUR0101.md", true, true),
				new DiagnosticData("Method with the DefaultParam attribute cannot be partial or extern", 02, "DUR0102.md", true, true),
				new DiagnosticData("DefaultParamAttribute is not valid on local functions or lambdas", 03, "DUR0103.md", true, true),
				new DiagnosticData("DefaultParamAttribute cannot be applied to members with the GeneratedCodeAttribute or DurianGeneratedAttribute", 04, "DUR0104.md", true, true),
				new DiagnosticData("DefaultParamAttribute must be placed on the right-most type parameter", 05, "DUR0105.md", true, true),
				new DiagnosticData("Value of DefaultParamAttribute does not satisfy the type constraint", 06, "DUR0106.md", true, true),
				new DiagnosticData("Do not override methods generated using DefaultParamAttribute", 07, "DUR0107.md", true, true),
				new DiagnosticData("Value of DefaultParamAttribute of overriding method must match the base method", 08, "DUR0108.md", true, true),
				new DiagnosticData("Do not add the DefaultParamAttribute on overridden type parameters", 09, "DUR0109.md", true, true),
				new DiagnosticData("DefaultParamAttribute of overridden type parameter should be added for clarity", 10, "DUR0110.md", false, true),
				new DiagnosticData("DefaultParamConfigurationAttribute is not valid on members without the DefaultParamAttribute", 11, "DUR0111.md", true, true),
				new DiagnosticData("TypeConvention property should not be used on members other than types", 12, "DUR0112.md", false, true),
				new DiagnosticData("MethodConvention property should not be used on members other than methods", 13, "DUR0113.md", false, true),
				new DiagnosticData("ApplyNewModifierWhenPossible property should not be used on members directly", 14, "DUR0114.md", false, true),

				// Diagnostics from AnalysisCore
				new DiagnosticData("Method with the specified signature already exists", 09, "DUR0009.md", true, true, AnalysisCore),
				new DiagnosticData("Projects with any Durian analyzer must reference the Durian.Core package", 10, "DUR0010.md", true, false, AnalysisCore),
			}
		);
	}
}
