// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Durian.Configuration;
using Durian.Info;
using Microsoft.CodeAnalysis;

namespace Durian.Analysis.GenericSpecialization
{
	/// <summary>
	/// Contains <see cref="DiagnosticDescriptor"/>s of all the <see cref="Diagnostic"/>s that can be reported by the <see cref="GenericSpecializationGenerator"/> or one of the analyzers.
	/// </summary>
	public static class GenSpecDiagnostics
	{
		/// <summary>
		/// Provides diagnostic message indicating that non-generic types cannot use the <see cref="AllowSpecializationAttribute"/>.
		/// </summary>
		public static readonly DiagnosticDescriptor DUR0201_NonGenericTypesCannotUseTheAllowSpecializationAttribute = new(
			id: "DUR0201",
			title: "Non-generic types cannot use the AllowSpecializationAttribute",
			messageFormat: "'{0}': Non-generic types cannot use the AllowSpecializationAttribute",
			category: "Durian.GenericSpecialization",
			defaultSeverity: DiagnosticSeverity.Error,
			helpLinkUri: DocsPath + "/DUR0201.md",
			isEnabledByDefault: true
		);

		/// <summary>
		/// Provides diagnostic message indicating that the target class must be marked with the <see cref="AllowSpecializationAttribute"/>.
		/// </summary>
		public static readonly DiagnosticDescriptor DUR0202_TargetClassMustBeMarkedWithAllowSpecializationAttribute = new(
			id: "DUR0202",
			title: "Target class must be marked with the AllowSpecializationAttribute",
			messageFormat: "'{0}': Target class '{1}' must be marked with the AllowSpecializationAttribute",
			category: "Durian.GenericSpecialization",
			defaultSeverity: DiagnosticSeverity.Error,
			helpLinkUri: DocsPath + "/DUR0202.md",
			isEnabledByDefault: true
		);

		/// <summary>
		/// Provides diagnostic message indicating that the name specified using either <see cref="GenericSpecializationConfigurationAttribute.TemplateName"/> or <see cref="GenericSpecializationConfigurationAttribute.InterfaceName"/> is not a valid identifier.
		/// </summary>
		public static readonly DiagnosticDescriptor DUR0203_SpecifiedNameIsNotAValidIdentifier = new(
			id: "DUR0203",
			title: "Specified name is not a valid identifier",
			messageFormat: "'{0}': Name '{1}' is not a valid identifier",
			category: "Durian.GenericSpecialization",
			defaultSeverity: DiagnosticSeverity.Error,
			helpLinkUri: DocsPath + "/DUR0203.md",
			isEnabledByDefault: true
		);

		/// <summary>
		/// Provides diagnostic message indicating that the user should not specify the <see cref="GenericSpecializationConfigurationAttribute"/> on members that do not contain any generic child classes or are not generic themselves.
		/// </summary>
		public static readonly DiagnosticDescriptor DUR0204_DoNotSpecifyConfigurationAttributeOnMemberWithNoSpecializations = new(
			id: "DUR0204",
			title: "Do not specify the GenericSpecializationConfigurationAttribute on members that do not contain any generic child classes or are not generic themselves",
			messageFormat: "'{0}': Do not specify the GenericSpecializationConfigurationAttribute on members that do not contain any generic child classes or are not generic themselves",
			category: "Durian.GenericSpecialization",
			defaultSeverity: DiagnosticSeverity.Warning,
			helpLinkUri: DocsPath + "/DUR0204.md",
			isEnabledByDefault: true
		);

		/// <summary>
		/// Provides diagnostic message indicating that a generic specialization must inherit the default implementation class.
		/// </summary>
		public static readonly DiagnosticDescriptor DUR0205_SpecializationMustInheritMainImplementation = new(
			id: "DUR0205",
			title: "Generic specialization must inherit the default implementation class",
			messageFormat: "'{0}': Generic specialization must inherit the default implementation class '{1}'",
			category: "Durian.GenericSpecialization",
			defaultSeverity: DiagnosticSeverity.Error,
			helpLinkUri: DocsPath + "/DUR0205.md",
			isEnabledByDefault: true
		);

		/// <summary>
		/// Provides diagnostic message indicating that a generic specialization must implement the specialization interface.
		/// </summary>
		public static readonly DiagnosticDescriptor DUR0206_SpecializationMustImplementInterface = new(
			id: "DUR0206",
			title: "Generic specialization must implement the specialization interface",
			messageFormat: "'{0}': Generic specialization must implement the specialization interface '{1}'",
			category: "Durian.GenericSpecialization",
			defaultSeverity: DiagnosticSeverity.Error,
			helpLinkUri: DocsPath + "/DUR0206.md",
			isEnabledByDefault: true
		);

		/// <summary>
		/// Provides diagnostic message indicating that the specialization class cannot be abstract or static.
		/// </summary>
		public static readonly DiagnosticDescriptor DUR0207_SpecializationCannotBeAbstractOrStatic = new(
			id: "DUR0207",
			title: "Class marked with the GenericSpecializationAttribute cannot be abstract or static",
			messageFormat: "'{0}': Class marked with the GenericSpecializationAttribute cannot be abstract or static",
			category: "Durian.GenericSpecialization",
			defaultSeverity: DiagnosticSeverity.Error,
			helpLinkUri: DocsPath + "/DUR0207.md",
			isEnabledByDefault: true
		);

		/// <summary>
		/// Provides diagnostic message indicating that the user must provide a default implementation for the target generic class.
		/// </summary>
		public static readonly DiagnosticDescriptor DUR0208_ProvideDefaultImplementation = new(
			id: "DUR0208",
			title: "Generic class lacks default implementation",
			messageFormat: "'{0}': Generic class lacks default implementation",
			category: "Durian.GenericSpecialization",
			defaultSeverity: DiagnosticSeverity.Error,
			helpLinkUri: DocsPath + "/DUR0208.md",
			isEnabledByDefault: true
		);

		/// <summary>
		/// Provides diagnostic message indicating that the target generic class must be partial.
		/// </summary>
		public static readonly DiagnosticDescriptor DUR0209_TargetGenericClassMustBePartial = new(
			id: "DUR0209",
			title: "Class marked with the AllowSpecializationAttribute must be partial",
			messageFormat: "'{0}': Class marked with the AllowSpecializationAttribute must be partial",
			category: "Durian.GenericSpecialization",
			defaultSeverity: DiagnosticSeverity.Error,
			helpLinkUri: DocsPath + "/DUR0209.md",
			isEnabledByDefault: true
		);

		/// <summary>
		/// Provides diagnostic message indicating that a containing type of a member marked with the <see cref="AllowSpecializationAttribute"/> must be <see langword="partial"/>.
		/// </summary>
		public static readonly DiagnosticDescriptor DUR0210_ContainingTypesMustBePartial = new(
			id: "DUR0210",
			title: "Containing type of a member marked with the AllowSpecializationAttribute must be partial",
			messageFormat: "'{0}': Containing type of a member marked with the AllowSpecializationAttribute must be partial",
			category: "Durian.GenericSpecialization",
			defaultSeverity: DiagnosticSeverity.Error,
			helpLinkUri: DocsPath + "/DUR0210.md",
			isEnabledByDefault: true
		);

		/// <summary>
		/// Provides diagnostic message indicating that the target generic class cannot be abstract or static.
		/// </summary>
		public static readonly DiagnosticDescriptor DUR0211_TargetGenericClassCannotBeAbstractOrStatic = new(
			id: "DUR0211",
			title: "Class marked with the AllowSpecializationAttribute cannot be abstract or static",
			messageFormat: "'{0}': Class marked with the AllowSpecializationAttribute cannot be abstract or static",
			category: "Durian.GenericSpecialization",
			defaultSeverity: DiagnosticSeverity.Error,
			helpLinkUri: DocsPath + "/DUR0211.md",
			isEnabledByDefault: true
		);

		/// <summary>
		/// Provides diagnostic message indicating that the user should not specialize a generic class without the <see cref="AllowSpecializationAttribute"/>.
		/// </summary>
		public static readonly DiagnosticDescriptor DUR0212_DoNotSpecializeClassWithoutAllowSpecializationAttribute = new(
			id: "DUR0212",
			title: "Do not provide a specialization for a generic class that is not marked with the AllowSpecializationAttribute",
			messageFormat: "'{0}': Do not provide a specialization for generic class that is not marked with the AllowSpecializationAttribute",
			category: "Durian.GenericSpecialization",
			defaultSeverity: DiagnosticSeverity.Error,
			helpLinkUri: DocsPath + "/DUR0212.md",
			isEnabledByDefault: true
		);

		/// <summary>
		/// Provides diagnostic message indicating that the user should not specialize a generic class that is not part of the current assembly.
		/// </summary>
		public static readonly DiagnosticDescriptor DUR0213_DoNotSpecializeClassOutsideOfTheCurrentAssembly = new(
			id: "DUR0213",
			title: "Do not provide a specialization for a generic class that is not part of the current assembly",
			messageFormat: "'{0}': Do not provide a specialization for a generic class that is not part of the current assembly",
			category: "Durian.GenericSpecialization",
			defaultSeverity: DiagnosticSeverity.Error,
			helpLinkUri: DocsPath + "/DUR0213.md",
			isEnabledByDefault: true
		);

		/// <summary>
		/// Provides diagnostic message indicating that the user should not specialize a generic class from the System namespace or any of its child namespaces.
		/// </summary>
		public static readonly DiagnosticDescriptor DUR0214_DoNotSpecializeSystemClass = new(
			id: "DUR0214",
			title: "Do not provide a specialization for a generic class from the System namespace or any of its child namespaces",
			messageFormat: "'{0}': Do not provide a specialization for a generic class from the System namespace or any of its child namespaces",
			category: "Durian.GenericSpecialization",
			defaultSeverity: DiagnosticSeverity.Error,
			helpLinkUri: DocsPath + "/DUR0214.md",
			isEnabledByDefault: true
		);

		/// <summary>
		/// Provides diagnostic message indicating that the target type specified in the <see cref="GenericSpecializationAttribute"/> must be an unbound generic class.
		/// </summary>
		public static readonly DiagnosticDescriptor DUR0215_TargetTypeMustBeUnboundGenericType = new(
			id: "DUR0215",
			title: "Type specified in the GenericSpecializationAttribute must be an unbound generic class",
			messageFormat: "'{0}': Type specified in the GenericSpecializationAttribute must be an unbound generic class",
			category: "Durian.GenericSpecialization",
			defaultSeverity: DiagnosticSeverity.Error,
			helpLinkUri: DocsPath + "/DUR0215.md",
			isEnabledByDefault: true
		);

		/// <summary>
		/// Provides diagnostic message indicating that the target type is specified more than once by multiple <see cref="GenericSpecializationAttribute"/>s.
		/// </summary>
		public static readonly DiagnosticDescriptor DUR0216_DuplicateGenericSpecializationAttribute = new(
			id: "DUR0216",
			title: "Duplicate GenericSpecializationAttribute type",
			messageFormat: "'{0}': Type '{1}' is specified in multiple GenericSpecializationAttributes",
			category: "Durian.GenericSpecialization",
			defaultSeverity: DiagnosticSeverity.Warning,
			helpLinkUri: DocsPath + "/DUR0216.md",
			isEnabledByDefault: true
		);

		/// <summary>
		/// Provides diagnostic message indicating that the user should not declare members in a class marked with the <see cref="AllowSpecializationAttribute"/>.
		/// </summary>
		public static readonly DiagnosticDescriptor DUR0217_DoNotAddMembersToTargetClass = new(
			id: "DUR0217",
			title: "Do not declare members that are not specializations in a class marked with the AllowSpecializationAttribute",
			messageFormat: "'{0}': Do not declare members that are not specializations in class marked with the AllowSpecializationAttribute",
			description: "Do not declare members that are not specializations in a class marked with the AllowSpecializationAttribute. Move them to the default implementation class instead.",
			category: "Durian.GenericSpecialization",
			defaultSeverity: DiagnosticSeverity.Error,
			helpLinkUri: DocsPath + "/DUR0217.md",
			isEnabledByDefault: true
		);

		/// <summary>
		/// Provides diagnostic message indicating that the user should not specify base types and implemented interfaces for a class marked with the <see cref="AllowSpecializationAttribute"/>.
		/// </summary>
		public static readonly DiagnosticDescriptor DUR0218_DoNotSpecifyBaseTypesOrInterfacesForTargetClass = new(
			id: "DUR0218",
			title: "Do not specify base types or implemented interfaces for a class marked with the AllowSpecializationAttribute",
			messageFormat: "'{0}': Do not specify base types or implemented interfaces for class marked with the AllowSpecializationAttribute; move them to default implementation class instead",
			description: "Do not specify base types or implemented interfaces for a class marked with the AllowSpecializationAttribute. Move them to the default implementation class instead.",
			category: "Durian.GenericSpecialization",
			defaultSeverity: DiagnosticSeverity.Error,
			helpLinkUri: DocsPath + "/DUR0218.md",
			isEnabledByDefault: true
		);

		/// <summary>
		/// Provides diagnostic message indicating that the user should not use the <see cref="GenericSpecializationConfigurationAttribute.ForceInherit"/> property on a sealed class.
		/// </summary>
		public static readonly DiagnosticDescriptor DUR0219_CannotForceInheritSealedClass = new(
			id: "DUR0219",
			title: "Do not force inherit a sealed class",
			messageFormat: "'{0}': Do not force inherit a sealed class",
			category: "Durian.GenericSpecialization",
			defaultSeverity: DiagnosticSeverity.Warning,
			helpLinkUri: DocsPath + "/DUR0219.md",
			isEnabledByDefault: true
		);

		/// <summary>
		/// Provides diagnostic message indicating that the default generic implementation cannot be abstract or static.
		/// </summary>
		public static readonly DiagnosticDescriptor DUR0220_DefaultImplementationCannotBeAbstractOrStatic = new(
			id: "DUR0220",
			title: "Default generic implementation cannot be abstract or static",
			messageFormat: "'{0}': Default generic implementation cannot be abstract or static",
			category: "Durian.GenericSpecialization",
			defaultSeverity: DiagnosticSeverity.Error,
			helpLinkUri: DocsPath + "/DUR0220.md",
			isEnabledByDefault: true
		);

		/// <summary>
		/// Provides diagnostic message indicating that the default generic implementation cannot be generic.
		/// </summary>
		public static readonly DiagnosticDescriptor DUR0221_DefaultImplementationCannotBeGeneric = new(
			id: "DUR0221",
			title: "Default generic implementation cannot be generic",
			messageFormat: "'{0}': Default generic implementation cannot be generic",
			category: "Durian.GenericSpecialization",
			defaultSeverity: DiagnosticSeverity.Error,
			helpLinkUri: DocsPath + "/DUR0221.md",
			isEnabledByDefault: true
		);

		/// <summary>
		/// Provides diagnostic message indicating that the value of <see cref="GenericSpecializationConfigurationAttribute.TemplateName"/> or <see cref="GenericSpecializationConfigurationAttribute.InterfaceName"/> cannot be the same as the name of the containing class.
		/// </summary>
		public static readonly DiagnosticDescriptor DUR0222_TargetNameCannotBeTheSameAsContainingClass = new(
			id: "DUR0222",
			title: "Interface or template name cannot be the same as containing class",
			messageFormat: "'{0}': Interface or template name cannot be the same as containing class",
			category: "Durian.GenericSpecialization",
			defaultSeverity: DiagnosticSeverity.Error,
			helpLinkUri: DocsPath + "/DUR0222.md",
			isEnabledByDefault: true
		);

		/// <summary>
		/// Provides diagnostic message indicating that the default generic specialization must be a class.
		/// </summary>
		public static readonly DiagnosticDescriptor DUR0223_DefaultSpecializationMustBeClass = new(
			id: "DUR0223",
			title: "Generic specialization must be a class",
			messageFormat: "'{0}': Generic specialization must be a class",
			category: "Durian.GenericSpecialization",
			defaultSeverity: DiagnosticSeverity.Error,
			helpLinkUri: DocsPath + "/DUR0223.md",
			isEnabledByDefault: true
		);

		/// <summary>
		/// Provides diagnostic message indicating that a specialization class cannot inherit the type it is a specialization of.
		/// </summary>
		public static readonly DiagnosticDescriptor DUR0224_SpecializationCannotInheritTypeItIsSpecializing = new(
			id: "DUR0224",
			title: "Specialization class cannot inherit the type it is a specialization of",
			messageFormat: "'{0}': Specialization class cannot inherit the type it is a specialization of",
			category: "Durian.GenericSpecialization",
			defaultSeverity: DiagnosticSeverity.Error,
			helpLinkUri: DocsPath + "/DUR0224.md",
			isEnabledByDefault: true
		);

		/// <summary>
		/// Documentation directory of the <c>DefaultParam</c> module.
		/// </summary>
		public static string DocsPath => DurianInfo.Repository + "/tree/master/docs/GenericSpecialization";
	}
}
