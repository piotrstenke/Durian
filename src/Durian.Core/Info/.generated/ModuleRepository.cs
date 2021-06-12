//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the GenerateModuleRepository project.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Durian.Info
{
	/// <summary>
	/// Factory class of <see cref="ModuleIdentity"/> for all available Durian modules.
	/// </summary>
	public static class ModuleRepository
	{
		/// <summary>
		/// Creates a new instance of <see cref="ModuleIdentity"/> for the <see cref="DurianModule.Core"/> module.
		/// </summary>
		public static ModuleIdentity Core => new(
			module: DurianModule.Core,
			id: 00,
			packages: new PackageIdentity[]
			{
				PackageRepository.Core,
				PackageRepository.CoreAnalyzer,
			},
			docPath: @"tree\master\docs\Core",
			diagnostics: new DiagnosticData[]
			{
				new DiagnosticData(
					title: "Projects with any Durian analyzer must reference the Durian.Core package",
					id: 01,
					docsPath: @"tree\master\docs\Core\DUR0001.md",
					fatal: true,
					hasLocation: true
				),

				new DiagnosticData(
					title: "Type cannot be accessed, because its module is not imported",
					id: 02,
					docsPath: @"tree\master\docs\Core\DUR0002.md",
					fatal: true,
					hasLocation: true
				),

				new DiagnosticData(
					title: "Do not use types from the Durian.Generator namespace",
					id: 03,
					docsPath: @"tree\master\docs\Core\DUR0003.md",
					fatal: true,
					hasLocation: true
				),

				new DiagnosticData(
					title: "Durian modules can be used only in C#",
					id: 04,
					docsPath: @"tree\master\docs\Core\DUR0004.md",
					fatal: true,
					hasLocation: true
				),

				new DiagnosticData(
					title: "Do not add custom types to the Durian.Generator namespace",
					id: 05,
					docsPath: @"tree\master\docs\Core\DUR0005.md",
					fatal: true,
					hasLocation: true
				),

				new DiagnosticData(
					title: "Target project must use C# 9 or newer",
					id: 06,
					docsPath: @"tree\master\docs\Core\DUR0006.md",
					fatal: true,
					hasLocation: true
				),
			},
			types: new TypeIdentity[]
			{
				new TypeIdentity(
					name: "DurianGeneratedAttribute",
					@namespace: "Durian.Generator",
					modules: new DurianModule[]
					{
						DurianModule.Core,
					}
				),
				new TypeIdentity(
					name: "EnableModuleAttribute",
					@namespace: "Durian.Generator",
					modules: new DurianModule[]
					{
						DurianModule.Core,
					}
				),
				new TypeIdentity(
					name: "IncludeTypesAttribute",
					@namespace: "Durian.Generator",
					modules: new DurianModule[]
					{
						DurianModule.Core,
					}
				),
				new TypeIdentity(
					name: "DiagnosticFilesAttribute",
					@namespace: "Durian.Generator",
					modules: new DurianModule[]
					{
						DurianModule.Core,
					}
				),
				new TypeIdentity(
					name: "PackageDefinitionAttribute",
					@namespace: "Durian.Generator",
					modules: new DurianModule[]
					{
						DurianModule.Core,
					}
				),
				new TypeIdentity(
					name: "MovedFromAttribute",
					@namespace: "Durian.Generator",
					modules: new DurianModule[]
					{
						DurianModule.Core,
					}
				),
			}
		);

		/// <summary>
		/// Creates a new instance of <see cref="ModuleIdentity"/> for the <see cref="DurianModule.DefaultParam"/> module.
		/// </summary>
		public static ModuleIdentity DefaultParam => new(
			module: DurianModule.DefaultParam,
			id: 01,
			packages: new PackageIdentity[]
			{
				PackageRepository.DefaultParam,
			},
			docPath: @"tree\master\docs\DefaultParam",
			diagnostics: new DiagnosticData[]
			{
				new DiagnosticData(
					title: "Containing type of a member with the DefaultParamAttribute must be partial",
					id: 01,
					docsPath: @"tree\master\docs\DefaultParam\DUR0101.md",
					fatal: true,
					hasLocation: true
				),

				new DiagnosticData(
					title: "Method with the DefaultParamAttribute cannot be partial or extern",
					id: 02,
					docsPath: @"tree\master\docs\DefaultParam\DUR0102.md",
					fatal: true,
					hasLocation: true
				),

				new DiagnosticData(
					title: "DefaultParamAttribute is not valid on this type of method",
					id: 03,
					docsPath: @"tree\master\docs\DefaultParam\DUR0103.md",
					fatal: true,
					hasLocation: true
				),

				new DiagnosticData(
					title: "DefaultParamAttribute cannot be applied to members with the GeneratedCodeAttribute or DurianGeneratedAttribute",
					id: 04,
					docsPath: @"tree\master\docs\DefaultParam\DUR0104.md",
					fatal: true,
					hasLocation: true
				),

				new DiagnosticData(
					title: "DefaultParamAttribute must be placed on the right-most type parameter or right to the left-most DefaultParam type parameter",
					id: 05,
					docsPath: @"tree\master\docs\DefaultParam\DUR0105.md",
					fatal: true,
					hasLocation: true
				),

				new DiagnosticData(
					title: "Value of DefaultParamAttribute does not satisfy the type constraint",
					id: 06,
					docsPath: @"tree\master\docs\DefaultParam\DUR0106.md",
					fatal: true,
					hasLocation: true
				),

				new DiagnosticData(
					title: "Do not override methods generated using the DefaultParamAttribute",
					id: 07,
					docsPath: @"tree\master\docs\DefaultParam\DUR0107.md",
					fatal: true,
					hasLocation: true
				),

				new DiagnosticData(
					title: "Value of DefaultParamAttribute of overriding method must match the base method",
					id: 08,
					docsPath: @"tree\master\docs\DefaultParam\DUR0108.md",
					fatal: true,
					hasLocation: true
				),

				new DiagnosticData(
					title: "Do not add the DefaultParamAttribute on overridden type parameters that are not DefaultParam",
					id: 09,
					docsPath: @"tree\master\docs\DefaultParam\DUR0109.md",
					fatal: true,
					hasLocation: true
				),

				new DiagnosticData(
					title: "DefaultParamAttribute of overridden type parameter should be added for clarity",
					id: 10,
					docsPath: @"tree\master\docs\DefaultParam\DUR0110.md",
					fatal: false,
					hasLocation: true
				),

				new DiagnosticData(
					title: "DefaultParamConfigurationAttribute is not valid on members without the DefaultParamAttribute",
					id: 11,
					docsPath: @"tree\master\docs\DefaultParam\DUR0111.md",
					fatal: true,
					hasLocation: true
				),

				new DiagnosticData(
					title: "TypeConvention property should not be used on members other than types",
					id: 12,
					docsPath: @"tree\master\docs\DefaultParam\DUR0112.md",
					fatal: false,
					hasLocation: true
				),

				new DiagnosticData(
					title: "MethodConvention property should not be used on members other than methods",
					id: 13,
					docsPath: @"tree\master\docs\DefaultParam\DUR0113.md",
					fatal: false,
					hasLocation: true
				),

				new DiagnosticData(
					title: "Method with generated signature already exists",
					id: 14,
					docsPath: @"tree\master\docs\DefaultParam\DUR0114.md",
					fatal: true,
					hasLocation: true
				),

				new DiagnosticData(
					title: "DefaultParamConfigurationAttribute is not valid on this type of method",
					id: 15,
					docsPath: @"tree\master\docs\DefaultParam\DUR0115.md",
					fatal: true,
					hasLocation: true
				),

				new DiagnosticData(
					title: "Member with generated name already exists",
					id: 16,
					docsPath: @"tree\master\docs\DefaultParam\DUR0116.md",
					fatal: true,
					hasLocation: true
				),

				new DiagnosticData(
					title: "DPTypeConvention.Inherit cannot be used on a struct or a sealed type",
					id: 17,
					docsPath: @"tree\master\docs\DefaultParam\DUR0117.md",
					fatal: true,
					hasLocation: true
				),

				new DiagnosticData(
					title: "DPTypeConvention.Copy or DPTypeConvention.Default should be applied for clarity",
					id: 18,
					docsPath: @"tree\master\docs\DefaultParam\DUR0118.md",
					fatal: false,
					hasLocation: true
				),

				new DiagnosticData(
					title: "DefaultParam value cannot be less accessible than the target member",
					id: 19,
					docsPath: @"tree\master\docs\DefaultParam\DUR0119.md",
					fatal: true,
					hasLocation: true
				),

				new DiagnosticData(
					title: "Type is invalid DefaultParam value when there is a type parameter constrained to this type parameter",
					id: 20,
					docsPath: @"tree\master\docs\DefaultParam\DUR0120.md",
					fatal: true,
					hasLocation: true
				),

				new DiagnosticData(
					title: "Type is invalid DefaultParam value",
					id: 21,
					docsPath: @"tree\master\docs\DefaultParam\DUR0121.md",
					fatal: true,
					hasLocation: true
				),

				new DiagnosticData(
					title: "DefaultParamAttribute cannot be used on a partial type",
					id: 22,
					docsPath: @"tree\master\docs\DefaultParam\DUR0122.md",
					fatal: true,
					hasLocation: true
				),

				new DiagnosticData(
					title: "TypeConvention.Inherit cannot be used on a type without accessible constructor",
					id: 23,
					docsPath: @"tree\master\docs\DefaultParam\DUR0123.md",
					fatal: true,
					hasLocation: true
				),

				new DiagnosticData(
					title: "ApplyNewModifierWhenPossible should not be used when target is not a child type",
					id: 24,
					docsPath: @"tree\master\docs\DefaultParam\DUR0124.md",
					fatal: false,
					hasLocation: true
				),

				new DiagnosticData(
					title: "DefaultParamScopedConfigurationAttribute should not be used on types with no DefaultParam members",
					id: 25,
					docsPath: @"tree\master\docs\DefaultParam\DUR0125.md",
					fatal: false,
					hasLocation: true
				),

				new DiagnosticData(
					title: "Members with the DefaultParamAttribute cannot be nested within other DefaultParam members",
					id: 26,
					docsPath: @"tree\master\docs\DefaultParam\DUR0126.md",
					fatal: true,
					hasLocation: true
				),

				new DiagnosticData(
					title: "Target namespace is not a valid identifier",
					id: 27,
					docsPath: @"tree\master\docs\DefaultParam\DUR0127.md",
					fatal: false,
					hasLocation: true
				),

				new DiagnosticData(
					title: "Do not specify target namespace for a nested member",
					id: 28,
					docsPath: @"tree\master\docs\DefaultParam\DUR0128.md",
					fatal: false,
					hasLocation: true
				),

				new DiagnosticData(
					title: "Target namespace already contains member with the specified name",
					id: 29,
					docsPath: @"tree\master\docs\DefaultParam\DUR0129.md",
					fatal: true,
					hasLocation: true
				),
			},
			types: new TypeIdentity[]
			{
				new TypeIdentity(
					name: "DefaultParamAttribute",
					@namespace: "Durian",
					modules: new DurianModule[]
					{
						DurianModule.DefaultParam,
					}
				),
				new TypeIdentity(
					name: "DefaultParamConfigurationAttribute",
					@namespace: "Durian.Configuration",
					modules: new DurianModule[]
					{
						DurianModule.DefaultParam,
					}
				),
				new TypeIdentity(
					name: "DefaultParamScopedConfigurationAttribute",
					@namespace: "Durian.Configuration",
					modules: new DurianModule[]
					{
						DurianModule.DefaultParam,
					}
				),
				new TypeIdentity(
					name: "DPMethodConvention",
					@namespace: "Durian.Configuration",
					modules: new DurianModule[]
					{
						DurianModule.DefaultParam,
					}
				),
				new TypeIdentity(
					name: "DPTypeConvention",
					@namespace: "Durian.Configuration",
					modules: new DurianModule[]
					{
						DurianModule.DefaultParam,
					}
				),
			}
		);
	}
}
