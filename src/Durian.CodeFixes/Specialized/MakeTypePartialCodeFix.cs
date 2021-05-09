using System.Composition;
using System.Globalization;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.CodeFixes.Specialized
{
	/// <summary>
	/// A code fix that applies the <see langword="partial"/> keyword to a type.
	/// </summary>
	[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(MakeTypePartialCodeFix))]
	[Shared]
	public class MakeTypePartialCodeFix : ApplyModifierCodeFix<TypeDeclarationSyntax>
	{
		/// <inheritdoc/>
		public override string Title => "Make type partial";

		/// <inheritdoc/>
		public sealed override SyntaxKind[] ModifiersToApply => new[] { SyntaxKind.PartialKeyword };

		/// <summary>
		/// Creates a new instance of the <see cref="MakeTypePartialCodeFix"/> class.
		/// </summary>
		public MakeTypePartialCodeFix()
		{
		}

		/// <inheritdoc/>
		protected override bool ValidateDiagnostic(Diagnostic diagnostic)
		{
			string message = diagnostic.GetMessage(CultureInfo.InvariantCulture);
			return message.Contains("partial");
		}

		/// <inheritdoc/>
		protected override DiagnosticDescriptor[] GetSupportedDiagnostics()
		{
			return new DiagnosticDescriptor[]
			{
				DurianDescriptors.MemberWithAttributeMustHaveModifier,
				DurianDescriptors.ParentTypeOfMemberWithAttributeMustBePartial,
			};
		}
	}
}
