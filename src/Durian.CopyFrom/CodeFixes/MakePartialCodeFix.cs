// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Durian.Analysis.CodeFixes;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.Analysis.CopyFrom.CodeFixes
{
	/// <summary>
	/// Code fox for the <see cref="CopyFromDiagnostics.DUR0201_ContainingTypeMustBePartial"/> and <see cref="CopyFromDiagnostics.DUR0202_MemberMustBePartial"/> diagnostics.
	/// </summary>
	[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(MakePartialCodeFix))]
	public sealed class MakePartialCodeFix : ApplyModifierCodeFix<TypeDeclarationSyntax>
	{
		/// <inheritdoc/>
		public override string Id => $"{Title} [{nameof(CopyFrom)}]";

		/// <inheritdoc/>
		public override SyntaxKind[] ModifiersToApply => new[] { SyntaxKind.PartialKeyword };

		/// <inheritdoc/>
		public override string Title => "Make member partial";

		/// <summary>
		/// Creates a new instance of the <see cref="MakePartialCodeFix"/> class.
		/// </summary>
		public MakePartialCodeFix()
		{
		}

		/// <inheritdoc/>
		protected override DiagnosticDescriptor[] GetSupportedDiagnostics()
		{
			return new DiagnosticDescriptor[]
			{
				CopyFromDiagnostics.DUR0201_ContainingTypeMustBePartial,
				CopyFromDiagnostics.DUR0202_MemberMustBePartial
			};
		}
	}
}
