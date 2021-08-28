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
	/// Code fix for diagnostics indicating that value of <see cref="FriendClassAttribute"/> is redundant.
	/// </summary>
	[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RemoveRedundantFriendClassCodeFix))]
	public class RemoveRedundantFriendClassCodeFix : RemoveNodeCodeFix<AttributeSyntax>
	{
		/// <inheritdoc/>
		public override string Id => Title + " [FriendClass]";

		/// <inheritdoc/>
		public override string Title => "Remove redundant FriendClassAttribute";

		/// <summary>
		/// Initializes a new instance of the <see cref="RemoveRedundantFriendClassCodeFix"/> class.
		/// </summary>
		public RemoveRedundantFriendClassCodeFix()
		{
		}

		/// <inheritdoc/>
		protected override DiagnosticDescriptor[] GetSupportedDiagnostics()
		{
			return new[]
			{
				DUR0305_TypeDoesNotDeclareInternalMembers,
				DUR0312_InnerTypeIsImplicitFriend
			};
		}
	}
}
