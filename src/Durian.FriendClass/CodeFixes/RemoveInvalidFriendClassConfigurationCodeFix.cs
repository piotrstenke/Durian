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
	/// Code fox for diagnostics indicating that value of <c>Durian.Configuration.FriendClassConfigurationAttribute</c> is not valid.
	/// </summary>
	[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RemoveInvalidFriendClassConfigurationCodeFix))]
	public sealed class RemoveInvalidFriendClassConfigurationCodeFix : RemoveNodeCodeFix<AttributeArgumentSyntax>
	{
		/// <inheritdoc/>
		public override string Id => $"{Title} [{nameof(FriendClass)}]";

		/// <inheritdoc/>
		public override string Title => $"Remove invalid {FriendClassConfigurationAttributeProvider.TypeName}";

		/// <summary>
		/// Initializes a new instance of the <see cref="RemoveInvalidFriendClassConfigurationCodeFix"/> class.
		/// </summary>
		public RemoveInvalidFriendClassConfigurationCodeFix()
		{
		}

		/// <inheritdoc/>
		protected override DiagnosticDescriptor[] GetSupportedDiagnostics()
		{
			return new[]
			{
				DUR0311_DoNotAllowChildrenOnSealedType,
				DUR0315_DoNotAllowInheritedOnTypeWithoutBaseType,
				DUR0316_BaseTypeHasNoInternalInstanceMembers
			};
		}
	}
}
