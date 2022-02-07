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
	/// Code fox for the <see cref="DUR0306_FriendTypeSpecifiedByMultipleAttributes"/> diagnostic.
	/// </summary>
	[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RemoveDuplicateFriendClassCodeFix))]
	public sealed class RemoveDuplicateFriendClassCodeFix : RemoveNodeCodeFix<AttributeSyntax>
	{
		/// <inheritdoc/>
		public override string Id => $"{Title} [{nameof(FriendClass)}]";

		/// <inheritdoc/>
		public override string Title => $"Remove duplicate {FriendClassAttributeProvider.TypeName}";

		/// <summary>
		/// Initializes a new instance of the <see cref="RemoveDuplicateFriendClassCodeFix"/> class.
		/// </summary>
		public RemoveDuplicateFriendClassCodeFix()
		{
		}

		/// <inheritdoc/>
		protected override DiagnosticDescriptor[] GetSupportedDiagnostics()
		{
			return new[]
			{
				DUR0306_FriendTypeSpecifiedByMultipleAttributes
			};
		}
	}
}
