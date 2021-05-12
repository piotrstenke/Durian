using Durian.Generator;
using Microsoft.CodeAnalysis;

namespace Durian.DefaultParam
{
	/// <summary>
	/// Contains <see cref="DiagnosticDescriptor"/>s of all the <see cref="Diagnostic"/>s that can be reported by the <see cref="DefaultParamGenerator"/> or one of the analyzers.
	/// </summary>
	public static class DefaultParamDiagnostics
	{
		/// <summary>
		/// Documentation directory of the <c>DefaultParam</c> module.
		/// </summary>
		public static string DocsPath => DurianInfo.Repository + @"\docs\DefaultParam";

		/// <summary>
		/// Provides diagnostic message indicating that a containing type of a member with the <see cref="DefaultParamAttribute"/> must be <see langword="partial"/>.
		/// </summary>
		public static readonly DiagnosticDescriptor DUR0101_ContainingTypeMustBePartial = new(
			id: "DUR0101",
			title: "Containing type of a member with the DefaultParam attribute must be partial",
			messageFormat: "'{0}': Containing type of a member with the DefaultParam attribute must be partial",
			category: "Durian.DefaultParam",
			defaultSeverity: DiagnosticSeverity.Error,
			helpLinkUri: DocsPath + @"\DUR0101.md",
			isEnabledByDefault: true
		);

		/// <summary>
		/// Provides diagnostic message indicating that a method with the <see cref="DefaultParamAttribute"/> cannot be <see langword="partial"/> or <see langword="extern"/>.
		/// </summary>
		public static readonly DiagnosticDescriptor DUR0102_MethodCannotBePartialOrExtern = new(
			id: "DUR0102",
			title: "Method with the DefaultParam attribute cannot be partial or extern",
			messageFormat: "'{0}': Method with the DefaultParam attribute cannot be partial or extern",
			category: "Durian.DefaultParam",
			defaultSeverity: DiagnosticSeverity.Error,
			helpLinkUri: DocsPath + @"\DUR0102.md",
			isEnabledByDefault: true
		);

		/// <summary>
		/// Provides diagnostic message indicating that a method with the <see cref="DefaultParamAttribute"/> is not valid on local functions or anonymous methods.
		/// </summary>
		public static readonly DiagnosticDescriptor DUR0103_DefaultParamIsNotValidOnLocalFunctionsOrLambdas = new(
			id: "DUR0103",
			title: "DefaultParamAttribute is not valid on local functions or lambdas",
			messageFormat: "'{0}': DefaultParamAttribute is not valid on local functions or lambdas",
			category: "Durian.DefaultParam",
			defaultSeverity: DiagnosticSeverity.Error,
			helpLinkUri: DocsPath + @"\DUR0103.md",
			isEnabledByDefault: true
		);

		/// <summary>
		/// Provides diagnostic message indicating that a method with the <see cref="DefaultParamAttribute"/> cannot be applied to members with the <see cref="System.CodeDom.Compiler.GeneratedCodeAttribute"/> or <see cref="DurianGeneratedAttribute"/>.
		/// </summary>
		public static readonly DiagnosticDescriptor DUR0104_DefaultParamCannotBeAppliedWhenGenerationAttributesArePresent = new(
			id: "DUR0104",
			title: "DefaultParamAttribute cannot be applied to members with the GeneratedCodeAttribute or DurianGeneratedAttribute",
			messageFormat: "'{0}': DefaultParamAttribute cannot be applied to members with the GeneratedCodeAttribute or DurianGeneratedAttribute",
			category: "Durian.DefaultParam",
			defaultSeverity: DiagnosticSeverity.Error,
			helpLinkUri: DocsPath + @"\DUR0104.md",
			isEnabledByDefault: true
		);

		/// <summary>
		/// Provides diagnostic message indicating that a method with the <see cref="DefaultParamAttribute"/> must be placed on the right-most type parameter.
		/// </summary>
		public static readonly DiagnosticDescriptor DUR0105_DefaultParamMustBeLast = new(
			id: "DUR0105",
			title: "DefaultParamAttribute must be placed on the right-most type parameter",
			messageFormat: "'{0}': DefaultParamAttribute must be placed on the right-most type parameter",
			category: "Durian.DefaultParam",
			defaultSeverity: DiagnosticSeverity.Error,
			helpLinkUri: DocsPath + @"\DUR0105.md",
			isEnabledByDefault: true
		);

		/// <summary>
		/// Provides diagnostic message indicating that the value of DefaultParamAttribute does not satisfy the type constraint.
		/// </summary>
		public static readonly DiagnosticDescriptor DUR0106_TargetTypeDoesNotSatisfyConstraint = new(
			id: "DUR0106",
			title: "Value of DefaultParamAttribute does not satisfy the type constraint",
			messageFormat: "'{0}': Type '{1}' does not satisfy the type constraint",
			category: "Durian.DefaultParam",
			defaultSeverity: DiagnosticSeverity.Error,
			helpLinkUri: DocsPath + @"\DUR0106.md",
			isEnabledByDefault: true
		);

		/// <summary>
		/// Provides diagnostic message indicating that the user should not override methods generated using the <see cref="DefaultParamAttribute"/>.
		/// </summary>
		public static readonly DiagnosticDescriptor DUR0107_DoNotOverrideGeneratedMethods = new(
			id: "DUR0107",
			title: "Do not override methods generated using DefaultParamAttribute",
			messageFormat: "'{0}': Do not override methods generated using DefaultParamAttribute",
			category: "Durian.DefaultParam",
			defaultSeverity: DiagnosticSeverity.Error,
			helpLinkUri: DocsPath + @"\DUR0107.md",
			isEnabledByDefault: true
		);

		/// <summary>
		/// Provides diagnostic message indicating that value of <see cref="DefaultParamAttribute"/> of overriding method must match the base method.
		/// </summary>
		public static readonly DiagnosticDescriptor DUR0108_ValueOfOverriddenMethodMustBeTheSameAsBase = new(
			id: "DUR0108",
			title: "Value of DefaultParamAttribute of overriding method must match the base method",
			messageFormat: "'{0}': Value of DefaultParamAttribute of overriding method must match the base method",
			category: "Durian.DefaultParam",
			defaultSeverity: DiagnosticSeverity.Error,
			helpLinkUri: DocsPath + @"\DUR0108.md",
			isEnabledByDefault: true
		);

		/// <summary>
		/// Provides diagnostic message indicating that the user should not add the <see cref="DefaultParamAttribute"/> on overridden type parameters.
		/// </summary>
		public static readonly DiagnosticDescriptor DUR0109_DoNotAddDefaultParamAttributeOnOverridenParameters = new(
			id: "DUR0109",
			title: "Do not add the DefaultParamAttribute on overridden type parameters",
			messageFormat: "'{0}': Do not add the DefaultParamATtribute on overridden type parameters",
			category: "Durian.DefaultParam",
			defaultSeverity: DiagnosticSeverity.Error,
			helpLinkUri: DocsPath + @"\DUR0109.md",
			isEnabledByDefault: true
		);

		/// <summary>
		/// Provides diagnostic message indicating that the <see cref="DefaultParamAttribute"/> of overridden type parameter should be added for clarity.
		/// </summary>
		public static readonly DiagnosticDescriptor DUR0110_OverriddenDefaultParamAttribuetShouldBeAddedForClarity = new(
			id: "DUR0110",
			title: "DefaultParamAttribute of overridden type parameter should be added for clarity",
			messageFormat: "'{0}': DefaultParamAttribute of overridden type parameter should be added for clarity",
			category: "Durian.DefaultParam",
			defaultSeverity: DiagnosticSeverity.Warning,
			helpLinkUri: DocsPath + @"\DUR0110.md",
			isEnabledByDefault: true
		);

		/// <summary>
		/// Provides diagnostic message indicating that the <see cref="DefaultParamConfigurationAttribute"/>  is not valid on members without the <see cref="DefaultParamAttribute"/>.
		/// </summary>
		public static readonly DiagnosticDescriptor DUR0111_DefaultParamConfigurationAttributeCannotBeAppliedToMembersWithoutDefaultParamAttribute = new(
			id: "DUR0111",
			title: "DefaultParamConfigurationAttribute is not valid on members without the DefaultParamAttribute",
			messageFormat: "'{0}': DefaultParamConfigurationAttribute is not valid on members without the DefaultParamAttribute",
			category: "Durian.DefaultParam",
			defaultSeverity: DiagnosticSeverity.Error,
			helpLinkUri: DocsPath + @"\DUR0111.md",
			isEnabledByDefault: true
		);

		/// <summary>
		/// Provides diagnostic message indicating that the <see cref="DefaultParamConfigurationAttribute.TypeConventionProperty"/> should not be used on members other than types.
		/// </summary>
		public static readonly DiagnosticDescriptor DUR0112_TypeConvetionShouldNotBeUsedOnMethodsOrDelegates = new(
			id: "DUR0112",
			title: "TypeConvention property should not be used on members other than types",
			messageFormat: "'{0}': TypeConvention property should not be used on members other than types",
			category: "Durian.DefaultParam",
			defaultSeverity: DiagnosticSeverity.Warning,
			helpLinkUri: DocsPath + @"\DUR0112.md",
			isEnabledByDefault: true
		);

		/// <summary>
		/// Provides diagnostic message indicating that the <see cref="DefaultParamConfigurationAttribute.MethodConvetionProperty"/> should not be used on members other than methods.
		/// </summary>
		public static readonly DiagnosticDescriptor DUR0113_MethodConvetionShouldNotBeUsedOnMembersOtherThanMethods = new(
			id: "DUR0113",
			title: "MethodConvention property should not be used on members other than methods",
			messageFormat: "'{0}': MethodConvention property should not be used on members other than methods",
			category: "Durian.DefaultParam",
			defaultSeverity: DiagnosticSeverity.Warning,
			helpLinkUri: DocsPath + @"\DUR0113.md",
			isEnabledByDefault: true
		);

		/// <summary>
		/// Provides diagnostic message indicating that the <see cref="DefaultParamConfigurationAttribute.ApplyNewModifierWhenPossibleProperty"/> property should not be used on members directly.
		/// </summary>
		public static readonly DiagnosticDescriptor DUR0114_NewModifierPropertyShouldNotBeUsedOnMembers = new(
			id: "DUR0114",
			title: "ApplyNewModifierWhenPossible property should not be used on members directly",
			messageFormat: "'{0}': ApplyNewModifierWhenPossible property should not be used on members directly",
			category: "Durian.DefaultParam",
			defaultSeverity: DiagnosticSeverity.Warning,
			helpLinkUri: DocsPath + @"\DUR0114.md",
			isEnabledByDefault: true
		);
	}
}
