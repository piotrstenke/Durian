using Durian.Generator.CodeFixes;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.Generator.DefaultParam.CodeFixes
{
	/// <summary>
	/// Code fox for the <see cref="DefaultParamDiagnostics.DUR0101_ContainingTypeMustBePartial"/> diagnostic.
	/// </summary>
	[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(DUR0101_MakeContainingTypePartialCodeFix))]
	public sealed class DUR0101_MakeContainingTypePartialCodeFix : ApplyModifierCodeFix<TypeDeclarationSyntax>
	{
		/// <inheritdoc/>
		public override string Title => "Make type partial";

		/// <inheritdoc/>
		public override string Id => Title + " [DefaultParam]";

		/// <inheritdoc/>
		public override SyntaxKind[] ModifiersToApply => new[] { SyntaxKind.PartialKeyword };

		/// <summary>
		/// Creates a new instance of the <see cref="DUR0101_MakeContainingTypePartialCodeFix"/> class.
		/// </summary>
		public DUR0101_MakeContainingTypePartialCodeFix()
		{
		}

		/// <inheritdoc/>
		protected override DiagnosticDescriptor[] GetSupportedDiagnostics()
		{
			return new DiagnosticDescriptor[] { DefaultParamDiagnostics.DUR0101_ContainingTypeMustBePartial };
		}
	}
}
