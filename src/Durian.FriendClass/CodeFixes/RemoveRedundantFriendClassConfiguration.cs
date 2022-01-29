// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Durian.Analysis.CodeFixes;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Durian.Analysis.FriendClass.FriendClassDiagnostics;

namespace Durian.Analysis.FriendClass.CodeFixes
{
	/// <summary>
	/// Code fox for the <see cref="DUR0303_DoNotUseFriendClassConfigurationAttributeOnTypesWithNoFriends"/>
	/// and <see cref="DUR0313_ConfigurationIsRedundant"/> diagnostics.
	/// </summary>
	[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RemoveRedundantFriendClassConfiguration))]
	public class RemoveRedundantFriendClassConfiguration : RemoveNodeCodeFix<AttributeSyntax>
	{
		/// <inheritdoc/>
		public override string Id => $"{Title} [{nameof(FriendClass)}]";

		/// <inheritdoc/>
		public override string Title => $"Remove redundant {FriendClassConfigurationAttributeProvider.TypeName}";

		/// <summary>
		/// Initializes a new instance of the <see cref="RemoveRedundantFriendClassConfiguration"/> class.
		/// </summary>
		public RemoveRedundantFriendClassConfiguration()
		{
		}

		/// <inheritdoc/>
		protected override DiagnosticDescriptor[] GetSupportedDiagnostics()
		{
			return new[]
			{
				DUR0303_DoNotUseFriendClassConfigurationAttributeOnTypesWithNoFriends,
				DUR0313_ConfigurationIsRedundant
			};
		}
	}
}