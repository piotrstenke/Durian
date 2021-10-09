// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CodeFixes;
using Durian.Analysis.CodeFixes;
using static Durian.Analysis.FriendClass.FriendClassDiagnostics;

namespace Durian.Analysis.FriendClass.CodeFixes
{
	/// <summary>
	/// Code fix for diagnostics indicating that value of <c>Durian.FriendClassAttribute</c> is not valid.
	/// </summary>
	[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RemoveInvalidFriendClassCodeFix))]
	public class RemoveInvalidFriendClassCodeFix : RemoveNodeCodeFix<AttributeSyntax>
	{
		/// <inheritdoc/>
		public override string Id => Title + " [FriendClass]";

		/// <inheritdoc/>
		public override string Title => "Remove invalid FriendClassAttribute";

		/// <summary>
		/// Initializes a new instance of the <see cref="RemoveInvalidFriendClassCodeFix"/> class.
		/// </summary>
		public RemoveInvalidFriendClassCodeFix()
		{
		}

		/// <inheritdoc/>
		protected override DiagnosticDescriptor[] GetSupportedDiagnostics()
		{
			return new[]
			{
				DUR0301_TargetTypeIsOutsideOfAssembly,
				DUR0302_MemberCannotBeAccessedOutsideOfFriendClass,
				DUR0304_ValueOfFriendClassCannotAccessTargetType,
				DUR0308_TypeIsNotValid,
				DUR0309_TypeCannotBeFriendOfItself
			};
		}
	}
}
