using System.Linq;
using System.Threading.Tasks;
using Durian.Analysis.CodeFixes;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.Analysis.CopyFrom.CodeFixes
{
	/// <summary>
	/// Code fix that moves a <c>Durian.PatternAttribute</c> to the same partial declaration where <c>Durian.CopyFromTypeAttribute</c> is located.
	/// </summary>
	[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(MovePatternAttributeCodeFix))]
	public sealed class MovePatternAttributeCodeFix : DurianCodeFix<AttributeSyntax>
	{
		/// <inheritdoc/>
		public override string Id => $"{Title} [{nameof(CopyFrom)}]";

		/// <inheritdoc/>
		public override string Title => $"Move {PatternAttributeProvider.TypeName} to declaration with a {CopyFromTypeAttributeProvider.TypeName}";

		/// <summary>
		/// Initializes a new instance of the <see cref="MovePatternAttributeCodeFix"/> class.
		/// </summary>
		public MovePatternAttributeCodeFix()
		{
		}

		/// <inheritdoc/>
		protected override Task<Document> ExecuteAsync(CodeFixExecutionContext<AttributeSyntax> context)
		{
			if (context.Node.Parent is not AttributeListSyntax attrList || attrList.Parent is not TypeDeclarationSyntax decl)
			{
				return Result();
			}

			if (context.SemanticModel.GetDeclaredSymbol(decl, context.CancellationToken) is not INamedTypeSymbol type)
			{
				return Result();
			}

			if (context.Compilation.GetTypeByMetadataName(CopyFromTypeAttributeProvider.FullName) is not INamedTypeSymbol attributeSymbol)
			{
				return Result();
			}

			TypeDeclarationSyntax? target = type.DeclaringSyntaxReferences
				.Select(r => r.GetSyntaxAsync(context.CancellationToken).Result)
				.OfType<TypeDeclarationSyntax>()
				.FirstOrDefault(s => s.AttributeLists
					.SelectMany(attr => attr.Attributes)
					.Select(attr =>
					{
						SymbolInfo info = context.SemanticModel.GetSymbolInfo(attr, context.CancellationToken);
						return info.Symbol?.ContainingType;
					})
					.Any(attr => attr is not null && SymbolEqualityComparer.Default.Equals(attr, attributeSymbol))
				);

			if (target is null)
			{
				return Result();
			}

			AttributeListSyntax newList = SyntaxFactory.AttributeList(SyntaxFactory.SingletonSeparatedList(context.Node));

			TypeDeclarationSyntax newTarget = target.WithAttributeLists(target.AttributeLists.Add(newList));
			context.RegisterChange(target, newTarget);

			SeparatedSyntaxList<AttributeSyntax> list;

			if (attrList.Attributes.Count == 1)
			{
				list = default;
			}
			else
			{
				list = attrList.Attributes.Remove(context.Node);
			}

			TypeDeclarationSyntax newDecl = decl.WithAttributeLists(decl.AttributeLists.Replace(attrList, attrList.WithAttributes(list)));

			context.RegisterChange(decl, newDecl);

			return Result();

			Task<Document> Result()
			{
				return Task.FromResult(context.Document);
			}
		}

		/// <inheritdoc/>
		protected override DiagnosticDescriptor[] GetSupportedDiagnostics()
		{
			return new[]
			{
				CopyFromDiagnostics.DUR0219_PatternOnDifferentDeclaration
			};
		}
	}
}
