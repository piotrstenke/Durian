// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Durian.Info;
using Microsoft.CodeAnalysis;

namespace Durian.Analysis.InterfaceTargets
{
	/// <summary>
	/// Contains <see cref="DiagnosticDescriptor"/>s of all the <see cref="Diagnostic"/>s that can be reported by the <see cref="InterfaceTargetsAnalyzer"/>.
	/// </summary>
	public static class InterfaceTargetsDiagnostics
	{
		/// <summary>
		/// Provides diagnostic message indicating that the target interface cannot be implemented by members of specified kind.
		/// </summary>
		public static readonly DiagnosticDescriptor DUR0401_InterfaceCannotBeImplementedByMembersOfThisKind = new(
			id: "DUR0401",
			title: "Interface is not valid on members of this kind",
			messageFormat: "'{0}': Interface '{1}' cannot be implemented by {2}",
			category: "Durian.FriendClass",
			defaultSeverity: DiagnosticSeverity.Error,
			helpLinkUri: DocsPath + "/DUR0401.md",
			isEnabledByDefault: true
		);

		/// <summary>
		/// Provides diagnostic message indicating that the target interface cannot be a base of another interface.
		/// </summary>
		public static readonly DiagnosticDescriptor DUR0402_InterfaceCannotBeBaseOfAnotherInterface = new(
			id: "DUR0402",
			title: "Interface cannot be a base of another interface",
			messageFormat: "'{0}': Interface '{1}' cannot be a base of another interface",
			category: "Durian.FriendClass",
			defaultSeverity: DiagnosticSeverity.Error,
			helpLinkUri: DocsPath + "/DUR0402.md",
			isEnabledByDefault: true
		);

		/// <summary>
		/// Provides diagnostic message indicating that the target interface is not accessible directly in code.
		/// </summary>
		public static readonly DiagnosticDescriptor DUR0403_InterfaceIsNotDirectlyAccessible = new(
			id: "DUR0403",
			title: "Interface is accessible only through reflection",
			messageFormat: "'{0}': Interface '{1}' is accessible only through reflection",
			category: "Durian.FriendClass",
			defaultSeverity: DiagnosticSeverity.Error,
			helpLinkUri: DocsPath + "/DUR0403.md",
			isEnabledByDefault: true
		);

		/// <summary>
		/// Documentation directory of the <c>InterfaceTargets</c> module.
		/// </summary>
		public static string DocsPath => GlobalInfo.Repository + "/tree/master/docs/InterfaceTargets";
	}
}