// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Durian.Generator;
using Durian.Info;
using Microsoft.CodeAnalysis;

namespace Durian.Analysis.DefaultParam
{
    /// <summary>
    /// Contains <see cref="DiagnosticDescriptor"/>s of all the <see cref="Diagnostic"/>s that can be reported by the <see cref="DefaultParamGenerator"/> or one of the analyzers.
    /// </summary>
    public static class DefaultParamDiagnostics
    {
        /// <summary>
        /// Provides a diagnostic message indicating that a containing type of a member marked with the <c>Durian.DefaultParamAttribute</c> must be <see langword="partial"/>.
        /// </summary>
        public static readonly DiagnosticDescriptor DUR0101_ContainingTypeMustBePartial = new(
            id: "DUR0101",
            title: "Containing type of a member with the DefaultParamAttribute must be partial",
            messageFormat: "'{0}': Containing type of a member with the DefaultParamAttribute must be partial",
            category: "Durian.DefaultParam",
            defaultSeverity: DiagnosticSeverity.Error,
            helpLinkUri: DocsPath + "/DUR0101.md",
            isEnabledByDefault: true
        );

        /// <summary>
        /// Provides a diagnostic message indicating that a method with the <c>Durian.DefaultParamAttribute</c> cannot be <see langword="partial"/> or <see langword="extern"/>.
        /// </summary>
        public static readonly DiagnosticDescriptor DUR0102_MethodCannotBePartialOrExtern = new(
            id: "DUR0102",
            title: "Method with the DefaultParamAttribute cannot be partial or extern",
            messageFormat: "'{0}': Method with the DefaultParamAttribute cannot be partial or extern",
            category: "Durian.DefaultParam",
            defaultSeverity: DiagnosticSeverity.Error,
            helpLinkUri: DocsPath + "/DUR0102.md",
            isEnabledByDefault: true
        );

        /// <summary>
        /// Provides a diagnostic message indicating that a method with the <c>Durian.DefaultParamAttribute</c> is not valid on this type of method.
        /// </summary>
        public static readonly DiagnosticDescriptor DUR0103_DefaultParamIsNotValidOnThisTypeOfMethod = new(
            id: "DUR0103",
            title: "DefaultParamAttribute is not valid on this type of method",
            messageFormat: "'{0}': DefaultParamAttribute is not valid on this type of method",
            category: "Durian.DefaultParam",
            defaultSeverity: DiagnosticSeverity.Error,
            helpLinkUri: DocsPath + "/DUR0103.md",
            isEnabledByDefault: true
        );

        /// <summary>
        /// Provides a diagnostic message indicating that a method with the <c>Durian.DefaultParamAttribute</c> cannot be applied to members with the <see cref="System.CodeDom.Compiler.GeneratedCodeAttribute"/> or <see cref="DurianGeneratedAttribute"/>.
        /// </summary>
        public static readonly DiagnosticDescriptor DUR0104_DefaultParamCannotBeAppliedWhenGenerationAttributesArePresent = new(
            id: "DUR0104",
            title: "DefaultParamAttribute cannot be applied to members with the GeneratedCodeAttribute or DurianGeneratedAttribute",
            messageFormat: "'{0}': DefaultParamAttribute cannot be applied to members with the GeneratedCodeAttribute or DurianGeneratedAttribute",
            category: "Durian.DefaultParam",
            defaultSeverity: DiagnosticSeverity.Error,
            helpLinkUri: DocsPath + "/DUR0104.md",
            isEnabledByDefault: true
        );

        /// <summary>
        /// Provides a diagnostic message indicating that a method with the <c>Durian.DefaultParamAttribute</c> must be placed on the right-most type parameter or right to the left-most DefaultParam type parameter.
        /// </summary>
        public static readonly DiagnosticDescriptor DUR0105_DefaultParamMustBeLast = new(
            id: "DUR0105",
            title: "DefaultParamAttribute must be placed on the right-most type parameter or right to the left-most DefaultParam type parameter",
            messageFormat: "'{0}': DefaultParamAttribute must be placed on the right-most type parameter or right to the left-most DefaultParam type parameter",
            category: "Durian.DefaultParam",
            defaultSeverity: DiagnosticSeverity.Error,
            helpLinkUri: DocsPath + "/DUR0105.md",
            isEnabledByDefault: true
        );

        /// <summary>
        /// Provides a diagnostic message indicating that the value of DefaultParamAttribute does not satisfy the type constraint.
        /// </summary>
        public static readonly DiagnosticDescriptor DUR0106_TargetTypeDoesNotSatisfyConstraint = new(
            id: "DUR0106",
            title: "Value of DefaultParamAttribute does not satisfy the type constraint",
            messageFormat: "'{0}': Type '{1}' does not satisfy the type constraint",
            category: "Durian.DefaultParam",
            defaultSeverity: DiagnosticSeverity.Error,
            helpLinkUri: DocsPath + "/DUR0106.md",
            isEnabledByDefault: true
        );

        /// <summary>
        /// Provides a diagnostic message indicating that the user should not override methods generated using the <c>Durian.DefaultParamAttribute</c>.
        /// </summary>
        public static readonly DiagnosticDescriptor DUR0107_DoNotOverrideGeneratedMethods = new(
            id: "DUR0107",
            title: "Do not override methods generated using the DefaultParamAttribute",
            messageFormat: "'{0}': Do not override methods generated using the DefaultParamAttribute",
            category: "Durian.DefaultParam",
            defaultSeverity: DiagnosticSeverity.Error,
            helpLinkUri: DocsPath + "/DUR0107.md",
            isEnabledByDefault: true
        );

        /// <summary>
        /// Provides a diagnostic message indicating that value of <c>Durian.DefaultParamAttribute</c> of overriding method must match the base method.
        /// </summary>
        public static readonly DiagnosticDescriptor DUR0108_ValueOfOverriddenMethodMustBeTheSameAsBase = new(
            id: "DUR0108",
            title: "Value of DefaultParamAttribute of overriding method must match the base method",
            messageFormat: "'{0}': Value of DefaultParamAttribute of overriding method must match the base method",
            category: "Durian.DefaultParam",
            defaultSeverity: DiagnosticSeverity.Error,
            helpLinkUri: DocsPath + "/DUR0108.md",
            isEnabledByDefault: true
        );

        /// <summary>
        /// Provides a diagnostic message indicating that the user should not add the <c>Durian.DefaultParamAttribute</c> on overridden type parameters that are not DefaultParam.
        /// </summary>
        public static readonly DiagnosticDescriptor DUR0109_DoNotAddDefaultParamAttributeOnOverridenParameters = new(
            id: "DUR0109",
            title: "Do not add the DefaultParamAttribute on overridden type parameters that are not DefaultParam",
            messageFormat: "'{0}': Do not add the DefaultParamAttribute on overridden type parameters that are not DefaultParam",
            category: "Durian.DefaultParam",
            defaultSeverity: DiagnosticSeverity.Error,
            helpLinkUri: DocsPath + "/DUR0109.md",
            isEnabledByDefault: true
        );

        /// <summary>
        /// Provides a diagnostic message indicating that the <c>Durian.DefaultParamAttribute</c> of overridden type parameter should be added for clarity.
        /// </summary>
        public static readonly DiagnosticDescriptor DUR0110_OverriddenDefaultParamAttribuetShouldBeAddedForClarity = new(
            id: "DUR0110",
            title: "DefaultParamAttribute of overridden type parameter should be added for clarity",
            messageFormat: "'{0}': DefaultParamAttribute of overridden type parameter should be added for clarity",
            category: "Durian.DefaultParam",
            defaultSeverity: DiagnosticSeverity.Warning,
            helpLinkUri: DocsPath + "/DUR0110.md",
            isEnabledByDefault: true
        );

        /// <summary>
        /// Provides a diagnostic message indicating that the <c>Durian.Configuration.DefaultParamConfigurationAttribute</c>  is not valid on members without the <c>Durian.DefaultParamAttribute</c>.
        /// </summary>
        public static readonly DiagnosticDescriptor DUR0111_DefaultParamConfigurationAttributeCannotBeAppliedToMembersWithoutDefaultParamAttribute = new(
            id: "DUR0111",
            title: "DefaultParamConfigurationAttribute is not valid on members without the DefaultParamAttribute",
            messageFormat: "'{0}': DefaultParamConfigurationAttribute is not valid on members without the DefaultParamAttribute",
            category: "Durian.DefaultParam",
            defaultSeverity: DiagnosticSeverity.Error,
            helpLinkUri: DocsPath + "/DUR0111.md",
            isEnabledByDefault: true
        );

        /// <summary>
        /// Provides a diagnostic message indicating that the <c>Durian.Configuration.DefaultParamConfigurationAttribute.TypeConvention</c> should not be used on members other than types.
        /// </summary>
        public static readonly DiagnosticDescriptor DUR0112_TypeConvetionShouldNotBeUsedOnMembersOtherThanTypes = new(
            id: "DUR0112",
            title: "TypeConvention property should not be used on members other than types",
            messageFormat: "'{0}': TypeConvention property should not be used on members other than types",
            category: "Durian.DefaultParam",
            defaultSeverity: DiagnosticSeverity.Warning,
            helpLinkUri: DocsPath + "/DUR0112.md",
            isEnabledByDefault: true
        );

        /// <summary>
        /// Provides a diagnostic message indicating that the <c>Durian.Configuration.DefaultParamConfigurationAttribute.MethodConvention</c> should not be used on members other than methods.
        /// </summary>
        public static readonly DiagnosticDescriptor DUR0113_MethodConventionShouldNotBeUsedOnMembersOtherThanMethods = new(
            id: "DUR0113",
            title: "MethodConvention property should not be used on members other than methods",
            messageFormat: "'{0}': MethodConvention property should not be used on members other than methods",
            category: "Durian.DefaultParam",
            defaultSeverity: DiagnosticSeverity.Warning,
            helpLinkUri: DocsPath + "/DUR0113.md",
            isEnabledByDefault: true
        );

        /// <summary>
        /// Provides a diagnostic message indicating that a method generated using the <c>Durian.DefaultParamAttribute</c> already exists.
        /// </summary>
        public static readonly DiagnosticDescriptor DUR0114_MethodWithSignatureAlreadyExists = new(
            id: "DUR0114",
            title: "Method with generated signature already exists",
            messageFormat: "'{0}': Method with generated signature '{1}' already exists",
            category: "Durian.DefaultParam",
            defaultSeverity: DiagnosticSeverity.Error,
            helpLinkUri: DocsPath + "/DUR0114.md",
            isEnabledByDefault: true
        );

        /// <summary>
        /// Provides a diagnostic message indicating that a method with the <c>Durian.Configuration.DefaultParamConfigurationAttribute</c> is not valid on this type method.
        /// </summary>
        public static readonly DiagnosticDescriptor DUR0115_DefaultParamConfigurationIsNotValidOnThisTypeOfMethod = new(
            id: "DUR0115",
            title: "DefaultParamConfigurationAttribute is not valid on this type of method",
            messageFormat: "'{0}': DefaultParamConfigurationAttribute is not valid on this type of method",
            category: "Durian.DefaultParam",
            defaultSeverity: DiagnosticSeverity.Error,
            helpLinkUri: DocsPath + "/DUR0115.md",
            isEnabledByDefault: true
        );

        /// <summary>
        /// Provides a diagnostic message indicating that a member with name the same as the one generated using the <c>Durian.DefaultParamAttribute</c> already exists.
        /// </summary>
        public static readonly DiagnosticDescriptor DUR0116_MemberWithNameAlreadyExists = new(
            id: "DUR0116",
            title: "Member with generated name already exists",
            messageFormat: "'{0}': Member with generated name '{1}' already exists",
            category: "Durian.DefaultParam",
            defaultSeverity: DiagnosticSeverity.Error,
            helpLinkUri: DocsPath + "/DUR0116.md",
            isEnabledByDefault: true
        );

        /// <summary>
        /// Provides a diagnostic message indicating that <see cref="TypeConvention.Inherit"/> cannot be used on a struct or a <see langword="sealed"/> type.
        /// </summary>
        public static readonly DiagnosticDescriptor DUR0117_InheritTypeConventionCannotBeUsedOnStructOrSealedType = new(
            id: "DUR0117",
            title: "DPTypeConvention.Inherit cannot be used on a struct or a sealed type",
            messageFormat: "'{0}': DPTypeConvention.Inherit cannot be used on a struct or sealed type",
            category: "Durian.DefaultParam",
            defaultSeverity: DiagnosticSeverity.Error,
            helpLinkUri: DocsPath + "/DUR0117.md",
            isEnabledByDefault: true
        );

        /// <summary>
        /// Provides a diagnostic message indicating that <see cref="TypeConvention.Copy"/> or <see cref="TypeConvention.Default"/> should be applied for the struct/sealed type.
        /// </summary>
        public static readonly DiagnosticDescriptor DUR0118_ApplyCopyTypeConventionOnStructOrSealedTypeOrTypeWithNoPublicCtor = new(
            id: "DUR0118",
            title: "DPTypeConvention.Copy or DPTypeConvention.Default should be applied for clarity",
            messageFormat: "'{0}': Apply DPTypeConvention.Copy or DPTypeConvention.Default for clarity",
            description: "DPTypeConvention.Inherit is applied to the enclosing scope, but it is ignored for structs and classes marked as static or sealed with no accessible constructors. Explicitly apply DPTypeConvention.Copy or DPTypeConvention.Default to avoid confusion.",
            category: "Durian.DefaultParam",
            defaultSeverity: DiagnosticSeverity.Warning,
            helpLinkUri: DocsPath + "/DUR0118.md",
            isEnabledByDefault: true
        );

        /// <summary>
        /// Provides a diagnostic message indicating that value of <c>Durian.DefaultParamAttribute</c> cannot be less accessible than the target member.
        /// </summary>
        public static readonly DiagnosticDescriptor DUR0119_DefaultParamValueCannotBeLessAccessibleThanTargetMember = new(
            id: "DUR0119",
            title: "DefaultParam value cannot be less accessible than the target member",
            messageFormat: "'{0}': DefaultParam value '{1}' cannot be less accessible than the target member",
            category: "Durian.DefaultParam",
            defaultSeverity: DiagnosticSeverity.Error,
            helpLinkUri: DocsPath + "/DUR0119.md",
            isEnabledByDefault: true
        );

        /// <summary>
        /// Provides a diagnostic message indicating that the target type is invalid <c>Durian.DefaultParamAttribute</c> value when there is a type parameter constrained to the target type parameter.
        /// </summary>
        public static readonly DiagnosticDescriptor DUR0120_TypeCannotBeUsedWithConstraint = new(
            id: "DUR0120",
            title: "Type is invalid DefaultParam value when there is a type parameter constrained to this type parameter",
            messageFormat: "'{0}': Type '{1}' is not valid DefaultParam value when there is a type parameter constrained to this type parameter",
            category: "Durian.DefaultParam",
            defaultSeverity: DiagnosticSeverity.Error,
            helpLinkUri: DocsPath + "/DUR0120.md",
            isEnabledByDefault: true
        );

        /// <summary>
        /// Provides a diagnostic message indicating that the target type is invalid <c>Durian.DefaultParamAttribute</c> value.
        /// </summary>
        public static readonly DiagnosticDescriptor DUR0121_TypeIsNotValidDefaultParamValue = new(
            id: "DUR0121",
            title: "Type is invalid DefaultParam value",
            messageFormat: "'{0}': Type '{1}' is not valid DefaultParam value",
            category: "Durian.DefaultParam",
            defaultSeverity: DiagnosticSeverity.Error,
            helpLinkUri: DocsPath + "/DUR0121.md",
            isEnabledByDefault: true
        );

        /// <summary>
        /// Provides a diagnostic message indicating that a <c>Durian.DefaultParamAttribute</c> cannot be used on a <see langword="partial"/> type.
        /// </summary>
        public static readonly DiagnosticDescriptor DUR0122_DoNotUseDefaultParamOnPartialType = new(
            id: "DUR0122",
            title: "DefaultParamAttribute cannot be used on a partial type",
            messageFormat: "'{0}': DefaultParamAttribute cannot be used on a partial type",
            category: "Durian.DefaultParam",
            defaultSeverity: DiagnosticSeverity.Error,
            helpLinkUri: DocsPath + "/DUR0122.md",
            isEnabledByDefault: true
        );

        /// <summary>
        /// Provides a diagnostic message indicating that the <see cref="TypeConvention.Inherit"/> cannot be used on a type without accessible constructor.
        /// </summary>
        public static readonly DiagnosticDescriptor DUR0123_InheritTypeConventionCannotBeUsedOnTypeWithNoAccessibleConstructor = new(
            id: "DUR0123",
            title: "TypeConvention.Inherit cannot be used on a type without accessible constructor",
            messageFormat: "'{0}': TypeConvention.Inherit cannot be used on a type without accessible constructor",
            category: "Durian.DefaultParam",
            defaultSeverity: DiagnosticSeverity.Error,
            helpLinkUri: DocsPath + "/DUR0123.md",
            isEnabledByDefault: true
        );

        /// <summary>
        /// Provides a diagnostic message indicating that the <c>Durian.Configuration.DefaultParamConfigurationAttribute.ApplyNewModifierWhenPossible</c> should not be used when target is not a child type.
        /// </summary>
        public static readonly DiagnosticDescriptor DUR0124_ApplyNewModifierShouldNotBeUsedWhenIsNotChildOfType = new(
            id: "DUR0124",
            title: "ApplyNewModifierWhenPossible should not be used when target is not a child type",
            messageFormat: "'{0}': ApplyNewModifierWhenPossible should not be used when target is not a child type",
            category: "Durian.DefaultParam",
            defaultSeverity: DiagnosticSeverity.Warning,
            helpLinkUri: DocsPath + "/DUR0124.md",
            isEnabledByDefault: true
        );

        /// <summary>
        /// Provides a diagnostic message indicating that the <c>Durian.Configuration.DefaultParamScopedConfigurationAttribute</c> should not be used on types with no DefaultParam members
        /// </summary>
        public static readonly DiagnosticDescriptor DUR0125_ScopedConfigurationShouldNotBePlacedOnATypeWithoutDefaultParamMembers = new(
            id: "DUR0125",
            title: "DefaultParamScopedConfigurationAttribute should not be used on types with no DefaultParam members",
            messageFormat: "'{0}': DefaultParamScopedConfigurationAttribute should not be used on types with no DefaultParam members",
            category: "Durian.DefaultParam",
            defaultSeverity: DiagnosticSeverity.Warning,
            helpLinkUri: DocsPath + "/DUR0125.md",
            isEnabledByDefault: true
        );

        /// <summary>
        /// Provides a diagnostic message indicating that members with the <c>Durian.DefaultParamAttribute</c> cannot be nested within other DefaultParam members.
        /// </summary>
        public static readonly DiagnosticDescriptor DUR0126_DefaultParamMembersCannotBeNested = new(
            id: "DUR0126",
            title: "Members with the DefaultParamAttribute cannot be nested within other DefaultParam members",
            messageFormat: "'{0}': Members with the DefaultParamAttribute cannot be nested within other DefaultParam members",
            category: "Durian.DefaultParam",
            defaultSeverity: DiagnosticSeverity.Error,
            helpLinkUri: DocsPath + "/DUR0126.md",
            isEnabledByDefault: true
        );

        /// <summary>
        /// Provides a diagnostic message indicating that the specified <c>Durian.Configuration.DefaultParamConfigurationAttribute.TargetNamespace</c> or <c>Durian.Configuration.DefaultParamConfigurationAttribute.TargetNamespace</c> is not a valid identifier.
        /// </summary>
        public static readonly DiagnosticDescriptor DUR0127_InvalidTargetNamespace = new(
            id: "DUR0127",
            title: "Target namespace is not a valid identifier",
            messageFormat: "'{0}': Target namespace '{1}' is not a valid identifier; parent namespace will be used instead",
            category: "Durian.DefaultParam",
            defaultSeverity: DiagnosticSeverity.Warning,
            helpLinkUri: DocsPath + "/DUR0127.md",
            isEnabledByDefault: true
        );

        /// <summary>
        /// Provides a diagnostic message indicating that the user should not specify target namespace for a nested member.
        /// </summary>
        public static readonly DiagnosticDescriptor DUR0128_DoNotSpecifyTargetNamespaceForNestedMembers = new(
            id: "DUR0128",
            title: "Do not specify target namespace for a nested member",
            messageFormat: "'{0}': Do not specify target namespace for a nested member",
            category: "Durian.DefaultParam",
            defaultSeverity: DiagnosticSeverity.Warning,
            helpLinkUri: DocsPath + "/DUR0128.md",
            isEnabledByDefault: true
        );

        /// <summary>
        /// Provides a diagnostic message indicating that the namespace specified in the <c>Durian.Configuration.DefaultParamConfigurationAttribute</c> or <c>Durian.Configuration.DefaultParamScopedConfigurationAttribute</c> already contains member with the generated name.
        /// </summary>
        public static readonly DiagnosticDescriptor DUR0129_TargetNamespaceAlreadyContainsMemberWithName = new(
            id: "DUR0129",
            title: "Target namespace already contains member with the generated name",
            messageFormat: "'{0}': Namespace '{1}' already contains member with name '{2}'",
            category: "Durian.DefaultParam",
            defaultSeverity: DiagnosticSeverity.Error,
            helpLinkUri: DocsPath + "/DUR0129.md",
            isEnabledByDefault: true
        );

        /// <summary>
        /// Documentation directory of the <c>DefaultParam</c> module.
        /// </summary>
        public static string DocsPath => GlobalInfo.Repository + "/tree/master/docs/DefaultParam";
    }
}