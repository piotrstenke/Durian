using System.Threading.Tasks;
using Durian.Configuration;
using Durian.Generator.CodeFixes;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.Generator.DefaultParam.CodeFixes
{
	/// <summary>
	/// Code fox for the <see cref="DefaultParamDiagnostics.DUR0118_ApplyCopyTypeConventionOnStructOrSealedTypeOrTypeWithNoPublicCtor"/> diagnostic.
	/// </summary>
	[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(DUR0118_ApplyCopyConvention))]
	public sealed class DUR0118_ApplyCopyConvention : DurianCodeFixBase
	{
		/// <inheritdoc/>
		public override string Title => "Apply DPTypeConvention.Copy";

		/// <inheritdoc/>
		public override string Id => Title + " [DefaultParam]";

		/// <summary>
		/// Creates a new instance of the <see cref="DUR0118_ApplyCopyConvention"/> class.
		/// </summary>
		public DUR0118_ApplyCopyConvention()
		{
		}

		/// <inheritdoc/>
		protected override DiagnosticDescriptor[] GetSupportedDiagnostics()
		{
			return new DiagnosticDescriptor[] { DefaultParamDiagnostics.DUR0118_ApplyCopyTypeConventionOnStructOrSealedTypeOrTypeWithNoPublicCtor };
		}

		/// <inheritdoc/>
		public override async Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			CodeFixData<TypeDeclarationSyntax> data = await CodeFixData<TypeDeclarationSyntax>.FromAsync(context, true).ConfigureAwait(false);

			if (!data.Success || !data.HasNode || !data.HasSemanticModel)
			{
				return;
			}

			INamedTypeSymbol? attribute = data.SemanticModel.Compilation.GetTypeByMetadataName(typeof(DefaultParamConfigurationAttribute).ToString());

			if (attribute is null)
			{
				return;
			}

			CodeAction? action = GetCodeAction(in data, attribute);

			if (action is null)
			{
				return;
			}

			context.RegisterCodeFix(action, data.Diagnostic);
		}

		private CodeAction GetCodeAction(in CodeFixData<TypeDeclarationSyntax> data, INamedTypeSymbol attribute)
		{
			Document document = data.Document!;
			TypeDeclarationSyntax node = data.Node!;
			CompilationUnitSyntax root = data.Root!;
			Diagnostic diagnostic = data.Diagnostic!;
			SemanticModel semanticModel = data.SemanticModel!;

			return CodeAction.Create(Title, cancenllationToken => ExecuteAsync(CodeFixExecutionContext<TypeDeclarationSyntax>.From(diagnostic, document, semanticModel, root, node!, cancenllationToken), attribute), Id);
		}

		private static async Task<Document> ExecuteAsync(CodeFixExecutionContext<TypeDeclarationSyntax> context, INamedTypeSymbol attribute)
		{
			SemanticModel? semanticModel = await context.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false);

			NameSyntax name;

			if (CodeFixUtility.HasUsingDirective(semanticModel!, context.Root.Usings, attribute, context.CancellationToken))
			{
				name = SyntaxFactory.IdentifierName(attribute.Name);
			}
			else
			{
				name = SyntaxFactory.ParseName(attribute.ToString());
			}

			TypeDeclarationSyntax type = context.Node.AddAttributeLists(SyntaxFactory.AttributeList(SyntaxFactory.SingletonSeparatedList(
				SyntaxFactory.Attribute(name,
					SyntaxFactory.AttributeArgumentList(SyntaxFactory.SingletonSeparatedList(
						SyntaxFactory.AttributeArgument(
							SyntaxFactory.MemberAccessExpression(
								SyntaxKind.SimpleMemberAccessExpression,
								SyntaxFactory.IdentifierName(nameof(DPTypeConvention)),
								SyntaxFactory.IdentifierName(nameof(DPTypeConvention.Copy))))
						.WithNameEquals(
							SyntaxFactory.NameEquals(
								SyntaxFactory.IdentifierName(nameof(DefaultParamConfigurationAttribute.TypeConvention)),
								SyntaxFactory.Token(SyntaxKind.EqualsToken).WithTrailingTrivia(SyntaxFactory.Space)))))))));

			context.RegisterChangeAndUpdateDocument(context.Node, type);
			return context.Document;
		}
	}
}
