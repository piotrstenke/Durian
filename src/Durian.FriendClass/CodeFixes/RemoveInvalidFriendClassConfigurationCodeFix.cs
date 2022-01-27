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
	/// Code fox for the <see cref="DUR0311_DoNotAllowChildrenOnSealedType"/> diagnostic.
	/// </summary>
	[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RemoveInvalidFriendClassConfigurationCodeFix))]
	public class RemoveInvalidFriendClassConfigurationCodeFix : RemoveNodeCodeFix<AttributeArgumentSyntax>
	{
		/// <inheritdoc/>
		public override string Id => Title + " [FriendClass]";

		/// <inheritdoc/>
		public override string Title => "Remove invalid FriendClassConfigurationAttribute";

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
				DUR0311_DoNotAllowChildrenOnSealedType
			};
		}
	}
}