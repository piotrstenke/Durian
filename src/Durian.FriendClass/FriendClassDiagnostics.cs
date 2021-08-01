// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using Microsoft.CodeAnalysis;
using Durian.Info;
using Durian.Configuration;

namespace Durian.Analysis.FriendClass
{
	/// <summary>
	/// Contains <see cref="DiagnosticDescriptor"/>s of all the <see cref="Diagnostic"/>s that can be reported by the <see cref="FriendClassAnalyzer"/>.
	/// </summary>
	public static class FriendClassDiagnostics
	{
		/// <summary>
		/// Provides diagnostic message indicating that <see cref="Type"/> specified by the target <see cref="FriendClassAttribute"/> is outside of the current assembly.
		/// </summary>
		public static readonly DiagnosticDescriptor DUR0301_TargetTypeIsOutsideOfAssembly = new(
			id: "DUR0301",
			title: "Target type is outside of the current assembly",
			messageFormat: "'{0}': Target type is outside of the current assembly, thus FriendClass will have no effect",
			category: "Durian.FriendClass",
			defaultSeverity: DiagnosticSeverity.Warning,
			helpLinkUri: DocsPath + "/DUR0301.md",
			isEnabledByDefault: true
		);

		/// <summary>
		/// Provides diagnostic message indicating that an <see langword="internal"/> member of a certain <see cref="Type"/> cannot be accessed, because the current <see cref="Type"/> is not a friend of that <see cref="Type"/>.
		/// </summary>
		public static readonly DiagnosticDescriptor DUR0302_MemberCannotBeAccessedOutsideOfFriendClass = new(
			id: "DUR0302",
			title: "Member cannot be accessed outside of friend types",
			messageFormat: "'{0}': Member '{1}' of '{2}' cannot be accessed outside of friend types",
			category: "Durian.FriendClass",
			defaultSeverity: DiagnosticSeverity.Error,
			helpLinkUri: DocsPath + "/DUR0302.md",
			isEnabledByDefault: true
		);

		/// <summary>
		/// Provides diagnostic message indicating that the <see cref="FriendClassConfigurationAttribute"/> should not be used on types with no <see cref="FriendClassAttribute"/> applied.
		/// </summary>
		public static readonly DiagnosticDescriptor DUR0303_DoNotUseFriendClassConfigurationAttributeOnTypesWithNoFriends = new(
			id: "DUR0303",
			title: "Do not use FriendClassConfigurationAttribute on types with no friends specified",
			messageFormat: "'{0}': Do not use FriendClassConfigurationAttribute on types with no friends specified",
			category: "Durian.FriendClass",
			defaultSeverity: DiagnosticSeverity.Warning,
			helpLinkUri: DocsPath + "/DUR0303.md",
			isEnabledByDefault: true
		);

		/// <summary>
		/// Provides diagnostic message indicating that the <see cref="FriendClassConfigurationAttribute.ApplyToType"/> property should not be set to <see langword="true"/> on types that are not <see langword="internal"/>.
		/// </summary>
		public static readonly DiagnosticDescriptor DUR0304_DoNotUseApplyToTypeOnNonInternalTypes = new(
			id: "DUR0304",
			title: "Do not set ApplyToType to true on types that are not 'internal'",
			messageFormat: "'{0}': Do not set ApplyToType to true on types that are not 'internal'",
			category: "Durian.FriendClass",
			defaultSeverity: DiagnosticSeverity.Warning,
			helpLinkUri: DocsPath + "/DUR0304.md",
			isEnabledByDefault: true
		);

		/// <summary>
		/// Provides diagnostic message indicating that the type specified by a <see cref="FriendClassAttribute"/> cannot access the target type.
		/// </summary>
		public static readonly DiagnosticDescriptor DUR0305_ValueOfFriendClassCannotAccessTargetType = new(
			id: "DUR0305",
			title: "Type specified by a FriendClassAttribute cannot access the target type",
			messageFormat: "'{0}': Type '{1}' cannot access the target type",
			category: "Durian.FriendClass",
			defaultSeverity: DiagnosticSeverity.Warning,
			helpLinkUri: DocsPath + "/DUR0305.md",
			isEnabledByDefault: true
		);

		/// <summary>
		/// Provides diagnostic message indicating that the type with one or more <see cref="FriendClassAttribute"/>s does not declare any <see langword="internal"/> members.
		/// </summary>
		public static readonly DiagnosticDescriptor DUR0306_TypeDoesNotDeclareInternalMembers = new(
			id: "DUR0306",
			title: "Target type does not declare any 'internal' members",
			messageFormat: "'{0}': FriendClassAttribute is unnecessary as the target type does not declare any 'internal' members",
			category: "Durian.FriendClass",
			defaultSeverity: DiagnosticSeverity.Warning,
			helpLinkUri: DocsPath + "/DUR0306.md",
			isEnabledByDefault: true
		);

		/// <summary>
		/// Provides diagnostic message indicating that a value of <see cref="FriendClassAttribute"/> was specified multiple times on the same type.
		/// </summary>
		public static readonly DiagnosticDescriptor DUR0307_FriendTypeSpecifiedByMultipleAttributes = new(
			id: "DUR0307",
			title: "Friend type is specified multiple times by two different FriendClassAttributes",
			messageFormat: "'{0}': Friend type '{1}' is specified multiple times by two different FriendClassAttributes",
			category: "Durian.FriendClass",
			defaultSeverity: DiagnosticSeverity.Error,
			helpLinkUri: DocsPath + "/DUR0307.md",
			isEnabledByDefault: true
		);

		/// <summary>
		/// Provides diagnostic message indicating that an <see langword="internal"/> member of a certain <see cref="Type"/> cannot be accessed,
		/// because the current <see cref="Type"/> is a sub class of a the <see cref="Type"/>'s friend type, but <see cref="FriendClassAttribute.AllowInherit"/> is set to <see langword="false"/>.
		/// </summary>
		public static readonly DiagnosticDescriptor DUR0308_MemberCannotBeAccessedBySubClass = new(
			id: "DUR0308",
			title: "Member cannot be accessed by a friend type's sub class",
			messageFormat: "'{0}': Member '{1}' of '{2}' cannot be accessed by a friend type's sub class",
			category: "Durian.FriendClass",
			defaultSeverity: DiagnosticSeverity.Error,
			helpLinkUri: DocsPath + "/DUR0308.md",
			isEnabledByDefault: true
		);

		/// <summary>
		/// Provides diagnostic message indicating that an <see langword="internal"/> <see cref="Type"/> cannot be accessed by a <see cref="Type"/> that is not a friend <see cref="Type"/>.
		/// </summary>
		public static readonly DiagnosticDescriptor DUR0309_TypeCannotBeAccessedByNonFriendType = new(
			id: "DUR0309",
			title: "Type cannot be accessed by a non-friend type",
			messageFormat: "'{0}': Type '{1}' cannot be accessed by a non-friend type",
			category: "Durian.FriendClass",
			defaultSeverity: DiagnosticSeverity.Error,
			helpLinkUri: DocsPath + "/DUR0309.md",
			isEnabledByDefault: true
		);

		/// <summary>
		/// Documentation directory of the <c>FriendClass</c> module.
		/// </summary>
		public static string DocsPath => GlobalInfo.Repository + "/tree/master/docs/FriendClass";
	}
}
