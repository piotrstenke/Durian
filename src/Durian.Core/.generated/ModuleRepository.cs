//------------------------------------------------------------------------------
// <auto-generated>
//	 This code was generated by the GenerateModuleRepository project.
//
//	 Changes to this file may cause incorrect behavior and will be lost if
//	 the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Durian.Info
{
	/// <summary>
	/// Factory class of <see cref="ModuleIdentity"/>s for all available Durian modules.
	/// </summary>
	public static class ModuleRepository
	{
		/// <summary>
		/// Returns a <see cref="ModuleIdentity"/> for the <see cref="DurianModule.Core"/> module.
		/// </summary>
		public static ModuleIdentity Core
		{
			get
			{
				if(!IdentityPool.Modules.TryGetValue("Core", out ModuleIdentity module))
				{
					module = new(
						module: DurianModule.Core,
						id: 00,
						packages: new DurianPackage[]
						{
							DurianPackage.Main,
							DurianPackage.CoreAnalyzer,
							DurianPackage.Core,
						},
						docsPath: "https://github.com/piotrstenke/Durian/tree/master/docs/Core",
						diagnostics: new DiagnosticData[]
						{
							new DiagnosticData(
								title: "Projects with any Durian analyzer must reference the Durian.Core package",
								id: 01,
								docsPath: "https://github.com/piotrstenke/Durian/tree/master/docs/Core/DUR0001.md",
								fatal: true,
								hasLocation: true
							),

							new DiagnosticData(
								title: "Type cannot be accessed, because its module is not imported",
								id: 02,
								docsPath: "https://github.com/piotrstenke/Durian/tree/master/docs/Core/DUR0002.md",
								fatal: true,
								hasLocation: true
							),

							new DiagnosticData(
								title: "Do not use types from the Durian.Generator namespace",
								id: 03,
								docsPath: "https://github.com/piotrstenke/Durian/tree/master/docs/Core/DUR0003.md",
								fatal: true,
								hasLocation: true
							),

							new DiagnosticData(
								title: "Durian modules can be used only in C#",
								id: 04,
								docsPath: "https://github.com/piotrstenke/Durian/tree/master/docs/Core/DUR0004.md",
								fatal: true,
								hasLocation: true
							),

							new DiagnosticData(
								title: "Do not add custom types to the Durian.Generator namespace",
								id: 05,
								docsPath: "https://github.com/piotrstenke/Durian/tree/master/docs/Core/DUR0005.md",
								fatal: true,
								hasLocation: true
							),

							new DiagnosticData(
								title: "PartialNameAttribute should be applied to a partial type",
								id: 06,
								docsPath: "https://github.com/piotrstenke/Durian/tree/master/docs/Core/DUR0006.md",
								fatal: false,
								hasLocation: true
							),

							new DiagnosticData(
								title: "Do not reference Durian analyzer package if the main Durian package is already included",
								id: 07,
								docsPath: "https://github.com/piotrstenke/Durian/tree/master/docs/Core/DUR0007.md",
								fatal: true,
								hasLocation: true
							),

							new DiagnosticData(
								title: "Separate analyzer packages detected, reference the main Durian package instead for better performance",
								id: 08,
								docsPath: "https://github.com/piotrstenke/Durian/tree/master/docs/Core/DUR0008.md",
								fatal: false,
								hasLocation: true
							),

							new DiagnosticData(
								title: "Type already has a PartialNameAttribute with same value",
								id: 09,
								docsPath: "https://github.com/piotrstenke/Durian/tree/master/docs/Core/DUR0009.md",
								fatal: false,
								hasLocation: true
							),
						},
						types: new TypeIdentity[]
						{
							TypeRepository.EnableModuleAttribute,
							TypeRepository.DurianGeneratedAttribute,
							TypeRepository.PartialNameAttribute,
							TypeRepository.UsingsAttribute,
						}
					);
				}

				return module;
			}
		}

		/// <summary>
		/// Returns a <see cref="ModuleIdentity"/> for the <see cref="DurianModule.Development"/> module.
		/// </summary>
		public static ModuleIdentity Development
		{
			get
			{
				if(!IdentityPool.Modules.TryGetValue("Development", out ModuleIdentity module))
				{
					module = new(
						module: DurianModule.Development,
						id: default,
						packages: new DurianPackage[]
						{
							DurianPackage.AnalysisServices,
							DurianPackage.TestServices,
						},
						docsPath: null,
						diagnostics: null,
						types: null
					);
				}

				return module;
			}
		}

		/// <summary>
		/// Returns a <see cref="ModuleIdentity"/> for the <see cref="DurianModule.DefaultParam"/> module.
		/// </summary>
		public static ModuleIdentity DefaultParam
		{
			get
			{
				if(!IdentityPool.Modules.TryGetValue("DefaultParam", out ModuleIdentity module))
				{
					module = new(
						module: DurianModule.DefaultParam,
						id: 01,
						packages: new DurianPackage[]
						{
							DurianPackage.DefaultParam,
						},
						docsPath: "https://github.com/piotrstenke/Durian/tree/master/docs/DefaultParam",
						diagnostics: new DiagnosticData[]
						{
							new DiagnosticData(
								title: "Containing type of a member with the DefaultParamAttribute must be partial",
								id: 01,
								docsPath: "https://github.com/piotrstenke/Durian/tree/master/docs/DefaultParam/DUR0101.md",
								fatal: true,
								hasLocation: true
							),

							new DiagnosticData(
								title: "Method with the DefaultParamAttribute cannot be partial or extern",
								id: 02,
								docsPath: "https://github.com/piotrstenke/Durian/tree/master/docs/DefaultParam/DUR0102.md",
								fatal: true,
								hasLocation: true
							),

							new DiagnosticData(
								title: "DefaultParamAttribute is not valid on this type of method",
								id: 03,
								docsPath: "https://github.com/piotrstenke/Durian/tree/master/docs/DefaultParam/DUR0103.md",
								fatal: true,
								hasLocation: true
							),

							new DiagnosticData(
								title: "DefaultParamAttribute cannot be applied to members with the GeneratedCodeAttribute or DurianGeneratedAttribute",
								id: 04,
								docsPath: "https://github.com/piotrstenke/Durian/tree/master/docs/DefaultParam/DUR0104.md",
								fatal: true,
								hasLocation: true
							),

							new DiagnosticData(
								title: "DefaultParamAttribute must be placed on the right-most type parameter or right to the left-most DefaultParam type parameter",
								id: 05,
								docsPath: "https://github.com/piotrstenke/Durian/tree/master/docs/DefaultParam/DUR0105.md",
								fatal: true,
								hasLocation: true
							),

							new DiagnosticData(
								title: "Value of DefaultParamAttribute does not satisfy the type constraint",
								id: 06,
								docsPath: "https://github.com/piotrstenke/Durian/tree/master/docs/DefaultParam/DUR0106.md",
								fatal: true,
								hasLocation: true
							),

							new DiagnosticData(
								title: "Do not override methods generated using the DefaultParamAttribute",
								id: 07,
								docsPath: "https://github.com/piotrstenke/Durian/tree/master/docs/DefaultParam/DUR0107.md",
								fatal: true,
								hasLocation: true
							),

							new DiagnosticData(
								title: "Value of DefaultParamAttribute of overriding method must match the base method",
								id: 08,
								docsPath: "https://github.com/piotrstenke/Durian/tree/master/docs/DefaultParam/DUR0108.md",
								fatal: true,
								hasLocation: true
							),

							new DiagnosticData(
								title: "Do not add the DefaultParamAttribute on overridden type parameters that are not DefaultParam",
								id: 09,
								docsPath: "https://github.com/piotrstenke/Durian/tree/master/docs/DefaultParam/DUR0109.md",
								fatal: true,
								hasLocation: true
							),

							new DiagnosticData(
								title: "DefaultParamAttribute of overridden type parameter should be added for clarity",
								id: 10,
								docsPath: "https://github.com/piotrstenke/Durian/tree/master/docs/DefaultParam/DUR0110.md",
								fatal: false,
								hasLocation: true
							),

							new DiagnosticData(
								title: "DefaultParamConfigurationAttribute is not valid on members without the DefaultParamAttribute",
								id: 11,
								docsPath: "https://github.com/piotrstenke/Durian/tree/master/docs/DefaultParam/DUR0111.md",
								fatal: true,
								hasLocation: true
							),

							new DiagnosticData(
								title: "TypeConvention property should not be used on members other than types",
								id: 12,
								docsPath: "https://github.com/piotrstenke/Durian/tree/master/docs/DefaultParam/DUR0112.md",
								fatal: false,
								hasLocation: true
							),

							new DiagnosticData(
								title: "MethodConvention property should not be used on members other than methods",
								id: 13,
								docsPath: "https://github.com/piotrstenke/Durian/tree/master/docs/DefaultParam/DUR0113.md",
								fatal: false,
								hasLocation: true
							),

							new DiagnosticData(
								title: "Method with generated signature already exists",
								id: 14,
								docsPath: "https://github.com/piotrstenke/Durian/tree/master/docs/DefaultParam/DUR0114.md",
								fatal: true,
								hasLocation: true
							),

							new DiagnosticData(
								title: "DefaultParamConfigurationAttribute is not valid on this type of method",
								id: 15,
								docsPath: "https://github.com/piotrstenke/Durian/tree/master/docs/DefaultParam/DUR0115.md",
								fatal: true,
								hasLocation: true
							),

							new DiagnosticData(
								title: "Member with generated name already exists",
								id: 16,
								docsPath: "https://github.com/piotrstenke/Durian/tree/master/docs/DefaultParam/DUR0116.md",
								fatal: true,
								hasLocation: true
							),

							new DiagnosticData(
								title: "DPTypeConvention.Inherit cannot be used on a struct or a sealed type",
								id: 17,
								docsPath: "https://github.com/piotrstenke/Durian/tree/master/docs/DefaultParam/DUR0117.md",
								fatal: true,
								hasLocation: true
							),

							new DiagnosticData(
								title: "DPTypeConvention.Copy or DPTypeConvention.Default should be applied for clarity",
								id: 18,
								docsPath: "https://github.com/piotrstenke/Durian/tree/master/docs/DefaultParam/DUR0118.md",
								fatal: false,
								hasLocation: true
							),

							new DiagnosticData(
								title: "DefaultParam value cannot be less accessible than the target member",
								id: 19,
								docsPath: "https://github.com/piotrstenke/Durian/tree/master/docs/DefaultParam/DUR0119.md",
								fatal: true,
								hasLocation: true
							),

							new DiagnosticData(
								title: "Type is invalid DefaultParam value when there is a type parameter constrained to this type parameter",
								id: 20,
								docsPath: "https://github.com/piotrstenke/Durian/tree/master/docs/DefaultParam/DUR0120.md",
								fatal: true,
								hasLocation: true
							),

							new DiagnosticData(
								title: "Type is invalid DefaultParam value",
								id: 21,
								docsPath: "https://github.com/piotrstenke/Durian/tree/master/docs/DefaultParam/DUR0121.md",
								fatal: true,
								hasLocation: true
							),

							new DiagnosticData(
								title: "DefaultParamAttribute cannot be used on a partial type",
								id: 22,
								docsPath: "https://github.com/piotrstenke/Durian/tree/master/docs/DefaultParam/DUR0122.md",
								fatal: true,
								hasLocation: true
							),

							new DiagnosticData(
								title: "TypeConvention.Inherit cannot be used on a type without accessible constructor",
								id: 23,
								docsPath: "https://github.com/piotrstenke/Durian/tree/master/docs/DefaultParam/DUR0123.md",
								fatal: true,
								hasLocation: true
							),

							new DiagnosticData(
								title: "ApplyNewModifierWhenPossible should not be used when target is not a child type",
								id: 24,
								docsPath: "https://github.com/piotrstenke/Durian/tree/master/docs/DefaultParam/DUR0124.md",
								fatal: false,
								hasLocation: true
							),

							new DiagnosticData(
								title: "DefaultParamScopedConfigurationAttribute should not be used on types with no DefaultParam members",
								id: 25,
								docsPath: "https://github.com/piotrstenke/Durian/tree/master/docs/DefaultParam/DUR0125.md",
								fatal: false,
								hasLocation: true
							),

							new DiagnosticData(
								title: "Members with the DefaultParamAttribute cannot be nested within other DefaultParam members",
								id: 26,
								docsPath: "https://github.com/piotrstenke/Durian/tree/master/docs/DefaultParam/DUR0126.md",
								fatal: true,
								hasLocation: true
							),

							new DiagnosticData(
								title: "Target namespace is not a valid identifier",
								id: 27,
								docsPath: "https://github.com/piotrstenke/Durian/tree/master/docs/DefaultParam/DUR0127.md",
								fatal: false,
								hasLocation: true
							),

							new DiagnosticData(
								title: "Do not specify target namespace for a nested member",
								id: 28,
								docsPath: "https://github.com/piotrstenke/Durian/tree/master/docs/DefaultParam/DUR0128.md",
								fatal: false,
								hasLocation: true
							),

							new DiagnosticData(
								title: "Target namespace already contains member with the generated name",
								id: 29,
								docsPath: "https://github.com/piotrstenke/Durian/tree/master/docs/DefaultParam/DUR0129.md",
								fatal: true,
								hasLocation: true
							),
						},
						types: new TypeIdentity[]
						{
							TypeRepository.DefaultParamAttribute,
							TypeRepository.DefaultParamConfigurationAttribute,
							TypeRepository.DefaultParamScopedConfigurationAttribute,
							TypeRepository.DPMethodConvention,
							TypeRepository.DPTypeConvention,
						}
					);
				}

				return module;
			}
		}

		/// <summary>
		/// Returns a <see cref="ModuleIdentity"/> for the <see cref="DurianModule.FriendClass"/> module.
		/// </summary>
		public static ModuleIdentity FriendClass
		{
			get
			{
				if(!IdentityPool.Modules.TryGetValue("FriendClass", out ModuleIdentity module))
				{
					module = new(
						module: DurianModule.FriendClass,
						id: 03,
						packages: new DurianPackage[]
						{
							DurianPackage.FriendClass,
						},
						docsPath: "https://github.com/piotrstenke/Durian/tree/master/docs/FriendClass",
						diagnostics: new DiagnosticData[]
						{
							new DiagnosticData(
								title: "Target type is outside of the current assembly",
								id: 01,
								docsPath: "https://github.com/piotrstenke/Durian/tree/master/docs/FriendClass/DUR0301.md",
								fatal: true,
								hasLocation: true
							),

							new DiagnosticData(
								title: "Member cannot be accessed outside of friend types",
								id: 02,
								docsPath: "https://github.com/piotrstenke/Durian/tree/master/docs/FriendClass/DUR0302.md",
								fatal: true,
								hasLocation: true
							),

							new DiagnosticData(
								title: "Do not use FriendClassConfigurationAttribute on types with no friend specified",
								id: 03,
								docsPath: "https://github.com/piotrstenke/Durian/tree/master/docs/FriendClass/DUR0303.md",
								fatal: false,
								hasLocation: true
							),

							new DiagnosticData(
								title: "Type specified by a FriendClassAttribute cannot access the target type",
								id: 04,
								docsPath: "https://github.com/piotrstenke/Durian/tree/master/docs/FriendClass/DUR0304.md",
								fatal: false,
								hasLocation: true
							),

							new DiagnosticData(
								title: "Target type does not declare any 'internal' members",
								id: 05,
								docsPath: "https://github.com/piotrstenke/Durian/tree/master/docs/FriendClass/DUR0305.md",
								fatal: false,
								hasLocation: true
							),

							new DiagnosticData(
								title: "Friend type is specified multiple times by two different FriendClassAttributes",
								id: 06,
								docsPath: "https://github.com/piotrstenke/Durian/tree/master/docs/FriendClass/DUR0306.md",
								fatal: true,
								hasLocation: true
							),

							new DiagnosticData(
								title: "Member cannot be accessed by a child type",
								id: 07,
								docsPath: "https://github.com/piotrstenke/Durian/tree/master/docs/FriendClass/DUR0307.md",
								fatal: true,
								hasLocation: true
							),

							new DiagnosticData(
								title: "Type is not a valid friend type",
								id: 08,
								docsPath: "https://github.com/piotrstenke/Durian/tree/master/docs/FriendClass/DUR0308.md",
								fatal: true,
								hasLocation: true
							),

							new DiagnosticData(
								title: "Type cannot be a friend of itself",
								id: 09,
								docsPath: "https://github.com/piotrstenke/Durian/tree/master/docs/FriendClass/DUR0309.md",
								fatal: true,
								hasLocation: true
							),

							new DiagnosticData(
								title: "Member cannot be accessed by friend type's child type",
								id: 10,
								docsPath: "https://github.com/piotrstenke/Durian/tree/master/docs/FriendClass/DUR0310.md",
								fatal: true,
								hasLocation: true
							),

							new DiagnosticData(
								title: "Do not use FriendClassConfigurationAttribute.AllowChildren on a sealed type",
								id: 11,
								docsPath: "https://github.com/piotrstenke/Durian/tree/master/docs/FriendClass/DUR0311.md",
								fatal: false,
								hasLocation: true
							),

							new DiagnosticData(
								title: "Inner types don't need to be specified as friends explicitly",
								id: 12,
								docsPath: "https://github.com/piotrstenke/Durian/tree/master/docs/FriendClass/DUR0312.md",
								fatal: false,
								hasLocation: true
							),

							new DiagnosticData(
								title: "FriendClassConfigurationAttribute is redundant",
								id: 13,
								docsPath: "https://github.com/piotrstenke/Durian/tree/master/docs/FriendClass/DUR0313.md",
								fatal: false,
								hasLocation: true
							),

							new DiagnosticData(
								title: "Inherited static members are not protected against access from non-friend types",
								id: 14,
								docsPath: "https://github.com/piotrstenke/Durian/tree/master/docs/FriendClass/DUR0314.md",
								fatal: false,
								hasLocation: true
							),

							new DiagnosticData(
								title: "Do not use FriendClassConfigurationAttribute.IncludeInherited on a type without parent type",
								id: 15,
								docsPath: "https://github.com/piotrstenke/Durian/tree/master/docs/FriendClass/DUR0315.md",
								fatal: true,
								hasLocation: true
							),

							new DiagnosticData(
								title: "Base type does not provide internal instance members",
								id: 16,
								docsPath: "https://github.com/piotrstenke/Durian/tree/master/docs/FriendClass/DUR0316.md",
								fatal: false,
								hasLocation: true
							),
						},
						types: new TypeIdentity[]
						{
							TypeRepository.FriendClassAttribute,
							TypeRepository.FriendClassConfigurationAttribute,
						}
					);
				}

				return module;
			}
		}

		/// <summary>
		/// Returns a <see cref="ModuleIdentity"/> for the <see cref="DurianModule.InterfaceTargets"/> module.
		/// </summary>
		public static ModuleIdentity InterfaceTargets
		{
			get
			{
				if(!IdentityPool.Modules.TryGetValue("InterfaceTargets", out ModuleIdentity module))
				{
					module = new(
						module: DurianModule.InterfaceTargets,
						id: 04,
						packages: new DurianPackage[]
						{
							DurianPackage.InterfaceTargets,
						},
						docsPath: "https://github.com/piotrstenke/Durian/tree/master/docs/InterfaceTargets",
						diagnostics: new DiagnosticData[]
						{
							new DiagnosticData(
								title: "Interface is not valid on members of this kind",
								id: 01,
								docsPath: "https://github.com/piotrstenke/Durian/tree/master/docs/InterfaceTargets/DUR0401.md",
								fatal: true,
								hasLocation: true
							),

							new DiagnosticData(
								title: "Interface cannot be a base of another interface",
								id: 02,
								docsPath: "https://github.com/piotrstenke/Durian/tree/master/docs/InterfaceTargets/DUR0402.md",
								fatal: true,
								hasLocation: true
							),

							new DiagnosticData(
								title: "Interface is accessible only through reflection",
								id: 03,
								docsPath: "https://github.com/piotrstenke/Durian/tree/master/docs/InterfaceTargets/DUR0403.md",
								fatal: true,
								hasLocation: true
							),
						},
						types: new TypeIdentity[]
						{
							TypeRepository.InterfaceTargets,
							TypeRepository.InterfaceTargetsAttribute,
						}
					);
				}

				return module;
			}
		}

		/// <summary>
		/// Returns a <see cref="ModuleIdentity"/> for the <see cref="DurianModule.CopyFrom"/> module.
		/// </summary>
		public static ModuleIdentity CopyFrom
		{
			get
			{
				if(!IdentityPool.Modules.TryGetValue("CopyFrom", out ModuleIdentity module))
				{
					module = new(
						module: DurianModule.CopyFrom,
						id: 02,
						packages: new DurianPackage[]
						{
							DurianPackage.CopyFrom,
						},
						docsPath: "https://github.com/piotrstenke/Durian/tree/master/docs/CopyFrom",
						diagnostics: new DiagnosticData[]
						{
							new DiagnosticData(
								title: "Containing type of a member with the CopyFromTypeAttribute or CopyFromMethodAttribute must be partial",
								id: 01,
								docsPath: "https://github.com/piotrstenke/Durian/tree/master/docs/CopyFrom/DUR0201.md",
								fatal: true,
								hasLocation: true
							),

							new DiagnosticData(
								title: "Member marked with the CopyFromTypeAttribute or CopyFromMethodAttribute must be partial",
								id: 02,
								docsPath: "https://github.com/piotrstenke/Durian/tree/master/docs/CopyFrom/DUR0202.md",
								fatal: true,
								hasLocation: true
							),

							new DiagnosticData(
								title: "Target member cannot be resolved",
								id: 03,
								docsPath: "https://github.com/piotrstenke/Durian/tree/master/docs/CopyFrom/DUR0203.md",
								fatal: true,
								hasLocation: true
							),

							new DiagnosticData(
								title: "Target member is not compatible",
								id: 04,
								docsPath: "https://github.com/piotrstenke/Durian/tree/master/docs/CopyFrom/DUR0204.md",
								fatal: true,
								hasLocation: true
							),

							new DiagnosticData(
								title: "Implementation of the target member is not accessible",
								id: 05,
								docsPath: "https://github.com/piotrstenke/Durian/tree/master/docs/CopyFrom/DUR0205.md",
								fatal: true,
								hasLocation: true
							),

							new DiagnosticData(
								title: "Equivalent CopyFromTypeAttribute already specified",
								id: 06,
								docsPath: "https://github.com/piotrstenke/Durian/tree/master/docs/CopyFrom/DUR0206.md",
								fatal: false,
								hasLocation: true
							),

							new DiagnosticData(
								title: "Member cannot copy from itself, parent, child or outer type",
								id: 07,
								docsPath: "https://github.com/piotrstenke/Durian/tree/master/docs/CopyFrom/DUR0207.md",
								fatal: true,
								hasLocation: true
							),

							new DiagnosticData(
								title: "Two or more members were resolved",
								id: 08,
								docsPath: "https://github.com/piotrstenke/Durian/tree/master/docs/CopyFrom/DUR0208.md",
								fatal: true,
								hasLocation: true
							),

							new DiagnosticData(
								title: "Cannot copy from a method without implementation",
								id: 09,
								docsPath: "https://github.com/piotrstenke/Durian/tree/master/docs/CopyFrom/DUR0209.md",
								fatal: true,
								hasLocation: true
							),

							new DiagnosticData(
								title: "CopyFromMethodAttribute is not valid on this kind of method",
								id: 10,
								docsPath: "https://github.com/piotrstenke/Durian/tree/master/docs/CopyFrom/DUR0210.md",
								fatal: true,
								hasLocation: true
							),

							new DiagnosticData(
								title: "Method marked with the CopyFromMethodAttribute already has a declaration",
								id: 11,
								docsPath: "https://github.com/piotrstenke/Durian/tree/master/docs/CopyFrom/DUR0211.md",
								fatal: true,
								hasLocation: true
							),

							new DiagnosticData(
								title: "Target member does not have a return type",
								id: 12,
								docsPath: "https://github.com/piotrstenke/Durian/tree/master/docs/CopyFrom/DUR0212.md",
								fatal: true,
								hasLocation: true
							),

							new DiagnosticData(
								title: "Target member cannot have a return type",
								id: 13,
								docsPath: "https://github.com/piotrstenke/Durian/tree/master/docs/CopyFrom/DUR0213.md",
								fatal: true,
								hasLocation: true
							),

							new DiagnosticData(
								title: "Invalid PatternAttribute specified",
								id: 14,
								docsPath: "https://github.com/piotrstenke/Durian/tree/master/docs/CopyFrom/DUR0214.md",
								fatal: false,
								hasLocation: true
							),

							new DiagnosticData(
								title: "PatternAttribute is redundant",
								id: 15,
								docsPath: "https://github.com/piotrstenke/Durian/tree/master/docs/CopyFrom/DUR0215.md",
								fatal: false,
								hasLocation: true
							),

							new DiagnosticData(
								title: "PatternAttribute with equivalent pattern already specified",
								id: 16,
								docsPath: "https://github.com/piotrstenke/Durian/tree/master/docs/CopyFrom/DUR0216.md",
								fatal: false,
								hasLocation: true
							),

							new DiagnosticData(
								title: "Type is not a valid type argument",
								id: 17,
								docsPath: "https://github.com/piotrstenke/Durian/tree/master/docs/CopyFrom/DUR0217.md",
								fatal: true,
								hasLocation: true
							),

							new DiagnosticData(
								title: "Partial part does not exist",
								id: 18,
								docsPath: "https://github.com/piotrstenke/Durian/tree/master/docs/CopyFrom/DUR0218.md",
								fatal: true,
								hasLocation: true
							),
						},
						types: new TypeIdentity[]
						{
							TypeRepository.CopyFromTypeAttribute,
							TypeRepository.CopyFromMethodAttribute,
							TypeRepository.PatternAttribute,
							TypeRepository.PartialNameAttribute,
							TypeRepository.UsingsAttribute,
						}
					);
				}

				return module;
			}
		}
	}
}
