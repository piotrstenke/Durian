using System;
using Durian.Info;
using Microsoft.CodeAnalysis;

namespace Durian.Analysis.FriendClass
{
	/// <summary>
	/// Contains <see cref="DiagnosticDescriptor"/>s of all the <see cref="Diagnostic"/>s that can be reported by either <see cref="FriendClassAccessAnalyzer"/> or <see cref="FriendClassDeclarationAnalyzer"/>.
	/// </summary>
	public static class FriendClassDiagnostics
	{
		/// <summary>
		/// Provides a diagnostic message indicating that <see cref="Type"/> specified by the target <c>Durian.FriendClassAttribute</c> is outside of the current assembly.
		/// </summary>
		public static readonly DiagnosticDescriptor DUR0301_TargetTypeIsOutsideOfAssembly = new(
			id: "DUR0301",
			title: "Target type is outside of the current assembly",
			messageFormat: "'{0}': Target type is outside of the current assembly",
			category: "Durian.FriendClass",
			defaultSeverity: DiagnosticSeverity.Error,
			helpLinkUri: DocsPath + "/DUR0301.md",
			isEnabledByDefault: true
		);

		/// <summary>
		/// Provides a diagnostic message indicating that an <see langword="internal"/> member of a certain <see cref="Type"/> cannot be accessed, because the current <see cref="Type"/> is not a friend of that <see cref="Type"/>.
		/// </summary>
		public static readonly DiagnosticDescriptor DUR0302_MemberCannotBeAccessedOutsideOfFriendClass = new(
			id: "DUR0302",
			title: "Member cannot be accessed outside of friend types",
			messageFormat: "'{0}': Member '{1}' cannot be accessed outside of friend types",
			category: "Durian.FriendClass",
			defaultSeverity: DiagnosticSeverity.Error,
			helpLinkUri: DocsPath + "/DUR0302.md",
			isEnabledByDefault: true
		);

		/// <summary>
		/// Provides a diagnostic message indicating that the <c>Durian.Configuration.FriendClassConfigurationAttribute</c> should not be used on types with no <c>Durian.FriendClassAttribute</c> applied.
		/// </summary>
		public static readonly DiagnosticDescriptor DUR0303_DoNotUseFriendClassConfigurationAttributeOnTypesWithNoFriends = new(
			id: "DUR0303",
			title: "Do not use FriendClassConfigurationAttribute on types with no friend specified",
			messageFormat: "'{0}': Do not use FriendClassConfigurationAttribute on types with no friend specified",
			category: "Durian.FriendClass",
			defaultSeverity: DiagnosticSeverity.Warning,
			helpLinkUri: DocsPath + "/DUR0303.md",
			isEnabledByDefault: true
		);

		/// <summary>
		/// Provides a diagnostic message indicating that the type specified by a <c>Durian.FriendClassAttribute</c> cannot access the target type.
		/// </summary>
		public static readonly DiagnosticDescriptor DUR0304_ValueOfFriendClassCannotAccessTargetType = new(
			id: "DUR0304",
			title: "Type specified by a FriendClassAttribute cannot access the target type",
			messageFormat: "'{0}': Type '{1}' cannot access the target type",
			category: "Durian.FriendClass",
			defaultSeverity: DiagnosticSeverity.Warning,
			helpLinkUri: DocsPath + "/DUR0304.md",
			isEnabledByDefault: true
		);

		/// <summary>
		/// Provides a diagnostic message indicating that the type with one or more <c>Durian.FriendClassAttribute</c>s does not declare any <see langword="internal"/> members.
		/// </summary>
		public static readonly DiagnosticDescriptor DUR0305_TypeDoesNotDeclareInternalMembers = new(
			id: "DUR0305",
			title: "Target type does not declare any 'internal' members",
			messageFormat: "'{0}': FriendClassAttribute is unnecessary as the target type does not declare any 'internal' members",
			category: "Durian.FriendClass",
			defaultSeverity: DiagnosticSeverity.Warning,
			helpLinkUri: DocsPath + "/DUR0305.md",
			isEnabledByDefault: true
		);

		/// <summary>
		/// Provides a diagnostic message indicating that a value of <c>Durian.FriendClassAttribute</c> was specified multiple times on the same type.
		/// </summary>
		public static readonly DiagnosticDescriptor DUR0306_FriendTypeSpecifiedByMultipleAttributes = new(
			id: "DUR0306",
			title: "Friend type is specified multiple times by two different FriendClassAttributes",
			messageFormat: "'{0}': Friend type '{1}' is specified multiple times by two different FriendClassAttributes",
			category: "Durian.FriendClass",
			defaultSeverity: DiagnosticSeverity.Error,
			helpLinkUri: DocsPath + "/DUR0306.md",
			isEnabledByDefault: true
		);

		/// <summary>
		/// Provides a diagnostic message indicating that an <see langword="internal"/> member cannot be accessed by the <see cref="Type"/>'s child classes, unless the <c>Durian.Configuration.FriendClassConfigurationAttribute.AllowChildren</c> property is set to <see langword="true"/>.
		/// </summary>
		public static readonly DiagnosticDescriptor DUR0307_MemberCannotBeAccessedByChildClass = new(
			id: "DUR0307",
			title: "Member cannot be accessed by a child type",
			messageFormat: "'{0}': Member '{1}' cannot be accessed by a child type",
			category: "Durian.FriendClass",
			defaultSeverity: DiagnosticSeverity.Error,
			helpLinkUri: DocsPath + "/DUR0307.md",
			isEnabledByDefault: true
		);

		/// <summary>
		/// Provides a diagnostic message indicating that a friend type is not valid or is <see langword="null"/>.
		/// </summary>
		public static readonly DiagnosticDescriptor DUR0308_TypeIsNotValid = new(
			id: "DUR0308",
			title: "Type is not a valid friend type",
			messageFormat: "'{0}': Type '{1}' is not a valid friend type",
			category: "Durian.FriendClass",
			defaultSeverity: DiagnosticSeverity.Error,
			helpLinkUri: DocsPath + "/DUR0308.md",
			isEnabledByDefault: true
		);

		/// <summary>
		/// Provides a diagnostic message indicating that a type cannot be a friend of itself.
		/// </summary>
		public static readonly DiagnosticDescriptor DUR0309_TypeCannotBeFriendOfItself = new(
			id: "DUR0309",
			title: "Type cannot be a friend of itself",
			messageFormat: "'{0}': Type cannot be a friend of itself",
			category: "Durian.FriendClass",
			defaultSeverity: DiagnosticSeverity.Error,
			helpLinkUri: DocsPath + "/DUR0309.md",
			isEnabledByDefault: true
		);

		/// <summary>
		/// Provides a diagnostic message indicating that an <see langword="internal"/> member cannot be accessed by child classes of a friend type, unless the <c>Durian.FriendClassAttribute.AllowFriendChildren</c> property is set to <see langword="true"/>.
		/// </summary>
		public static readonly DiagnosticDescriptor DUR0310_MemberCannotBeAccessedByChildClassOfFriend = new(
			id: "DUR0310",
			title: "Member cannot be accessed by friend type's child type",
			messageFormat: "'{0}': Member '{1}' cannot be accessed outside by friend type's child class",
			category: "Durian.FriendClass",
			defaultSeverity: DiagnosticSeverity.Error,
			helpLinkUri: DocsPath + "/DUR0310.md",
			isEnabledByDefault: true
		);

		/// <summary>
		/// Provides a diagnostic message indicating that the <c>Durian.FriendClassAttribute.AllowChildren</c> property should not be used on structs or sealed/static classes.
		/// </summary>
		public static readonly DiagnosticDescriptor DUR0311_DoNotAllowChildrenOnSealedType = new(
			id: "DUR0311",
			title: "Do not use FriendClassConfigurationAttribute.AllowChildren on a sealed type",
			messageFormat: "'{0}': Do not use FriendClassConfigurationAttribute.AllowChildren on a sealed type",
			category: "Durian.FriendClass",
			defaultSeverity: DiagnosticSeverity.Warning,
			helpLinkUri: DocsPath + "/DUR0311.md",
			isEnabledByDefault: true
		);

		/// <summary>
		/// Provides a diagnostic message indicating that an inner type does not need to be specified as a friend type, because it is a friend implicitly.
		/// </summary>
		public static readonly DiagnosticDescriptor DUR0312_InnerTypeIsImplicitFriend = new(
			id: "DUR0312",
			title: "Inner types don't need to be specified as friends explicitly",
			messageFormat: "'{0}': Inner types don't need to be specified as friends explicitly",
			category: "Durian.FriendClass",
			defaultSeverity: DiagnosticSeverity.Warning,
			helpLinkUri: DocsPath + "/DUR0312.md",
			isEnabledByDefault: true
		);

		/// <summary>
		/// Provides a diagnostic message indicating that a <c>Durian.Configuration.FriendClassConfigurationAttribute</c> is redundant, because all its values are set to default.
		/// </summary>
		public static readonly DiagnosticDescriptor DUR0313_ConfigurationIsRedundant = new(
			id: "DUR0313",
			title: "FriendClassConfigurationAttribute is redundant",
			messageFormat: "'{0}': FriendClassConfigurationAttribute is redundant",
			category: "Durian.FriendClass",
			defaultSeverity: DiagnosticSeverity.Warning,
			helpLinkUri: DocsPath + "/DUR0313.md",
			isEnabledByDefault: true
		);

		/// <summary>
		/// Provides a diagnostic message indicating that inherited static members should not be accessed by non-friend types.
		/// </summary>
		public static readonly DiagnosticDescriptor DUR0314_DoNotAccessInheritedStaticMembers = new(
			id: "DUR0314",
			title: "Inherited static members are not protected against access from non-friend types",
			messageFormat: "'{0}': Inherited static members are not protected against access from non-friend types",
			category: "Durian.FriendClass",
			defaultSeverity: DiagnosticSeverity.Warning,
			helpLinkUri: DocsPath + "/DUR0314.md",
			isEnabledByDefault: true
		);

		/// <summary>
		/// Provides a diagnostic message indicating that the <c>Durian.Configuration.FriendClassConfigurationAttribute.IncludeInherited</c> property should not be used on types that have no base type.
		/// </summary>
		public static readonly DiagnosticDescriptor DUR0315_DoNotAllowInheritedOnTypeWithoutBaseType = new(
			id: "DUR0315",
			title: "Do not use FriendClassConfigurationAttribute.IncludeInherited on a type without parent type",
			messageFormat: "'{0}': Do not use FriendClassConfigurationAttribute.IncludeInherited on a type without parent type",
			category: "Durian.FriendClass",
			defaultSeverity: DiagnosticSeverity.Error,
			helpLinkUri: DocsPath + "/DUR0315.md",
			isEnabledByDefault: true
		);

		/// <summary>
		/// Provides a diagnostic message indicating that the no internal instance members were inherited.
		/// </summary>
		public static readonly DiagnosticDescriptor DUR0316_BaseTypeHasNoInternalInstanceMembers = new(
			id: "DUR0316",
			title: "Base type does not provide internal instance members",
			messageFormat: "'{0}': Base type does not provide internal instance members",
			category: "Durian.FriendClass",
			defaultSeverity: DiagnosticSeverity.Warning,
			helpLinkUri: DocsPath + "/DUR0316.md",
			isEnabledByDefault: true
		);

		/// <summary>
		/// Documentation directory of the <c>FriendClass</c> module.
		/// </summary>
		public static string DocsPath => GlobalInfo.Repository + "/tree/master/docs/FriendClass";
	}
}
