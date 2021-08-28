// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CodeFixes;
using Durian.Analysis.CodeFixes;
using System.Threading;
using static Durian.Analysis.FriendClass.FriendClassDiagnostics;

namespace Durian.Analysis.FriendClass.CodeFixes
{
	/// <summary>
	/// Code fox for the <see cref="DUR0306_FriendTypeSpecifiedByMultipleAttributes"/> diagnostic.
	/// </summary>
	[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RemoveDuplicateFriendClassCodeFix))]
	public class RemoveDuplicateFriendClassCodeFix : RemoveNodeCodeFix<AttributeSyntax>
	{
		/// <inheritdoc/>
		public override string Id => Title + " [FriendClass]";

		/// <inheritdoc/>
		public override string Title => "Remove duplicate FriendClassAttribute";

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
