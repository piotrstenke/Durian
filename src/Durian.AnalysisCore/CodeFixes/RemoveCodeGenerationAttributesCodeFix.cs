using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Composition;
using System.Globalization;
using System.Linq;
using System.Threading;
using Durian.Generator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.CodeFixes
{
	/// <summary>
	/// A code fix that removes either <see cref="GeneratedCodeAttribute"/> or <see cref="DurianGeneratedAttribute"/>.
	/// </summary>
	[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RemoveCodeGenerationAttributesCodeFix))]
	[Shared]
	public abstract class RemoveCodeGenerationAttributesCodeFix : RemoveAttributeCodeFix<MemberDeclarationSyntax>
	{
		private string _title;

		/// <inheritdoc/>
		public override string Title => _title;

		/// <summary>
		/// Creates a new instance of the <see cref="RemoveCodeGenerationAttributesCodeFix"/> class.
		/// </summary>
		protected RemoveCodeGenerationAttributesCodeFix()
		{
			_title = "Remove DurianGeneratedAttribute";
		}

		/// <inheritdoc/>
		public override IEnumerable<INamedTypeSymbol> GetAttributeSymbols(CSharpCompilation compilation, CancellationToken cancellationToken = default)
		{
			INamedTypeSymbol?[] symbols = new[]
			{
				compilation.GetTypeByMetadataName(typeof(DurianGeneratedAttribute).ToString()),
				compilation.GetTypeByMetadataName(typeof(GeneratedCodeAttribute).ToString()),
			};

#pragma warning disable CS8619 // Nullability of reference types in value doesn't match target type.
			return symbols.Where(s => s is not null);
#pragma warning restore CS8619 // Nullability of reference types in value doesn't match target type.
		}

		/// <inheritdoc/>
		protected override bool ValidateDiagnostic(Diagnostic diagnostic)
		{
			string message = diagnostic.GetMessage(CultureInfo.InvariantCulture);

			if (message.Contains("DurianGenerated"))
			{
				_title = "Remove DurianGeneratedAttribute";
			}
			else if (message.Contains("GeneratedCode"))
			{
				_title = "Remove GeneratedCodeAttribute";
			}
			else
			{
				return false;
			}

			return true;
		}
	}
}
