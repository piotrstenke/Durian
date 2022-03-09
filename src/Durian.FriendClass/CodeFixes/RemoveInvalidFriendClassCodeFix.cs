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
	/// Code fix for diagnostics indicating that value of <c>Durian.FriendClassAttribute</c> is not valid.
	/// </summary>
	[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RemoveInvalidFriendClassCodeFix))]
	public sealed class RemoveInvalidFriendClassCodeFix : RemoveNodeCodeFix<AttributeSyntax>
	{
		/// <inheritdoc/>
		public override string Id => $"{Title} [{nameof(FriendClass)}]";

		/// <inheritdoc/>
		public override string Title => $"Remove invalid {FriendClassAttributeProvider.TypeName}";

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