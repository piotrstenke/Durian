using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.DefaultParam.CodeFixes
{
	/// <summary>
	/// A code fix that applies the 'partial' modifier to a type.
	/// </summary>
	[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(MakeTypePartialCodeFix))]
	[Shared]
	public sealed class MakeTypePartialCodeFix : CodeFixProvider
	{
		/// <summary>
		/// Title of this code fix that is displayed to the user.
		/// </summary>
		public static string Title => "Make type partial";

		/// <inheritdoc/>
		public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(
			DefaultParamDiagnostics.Descriptors.ParentTypeOfMemberWithDefaultParamAttributeMustBePartial.Id
		);

		/// <inheritdoc/>
		public override FixAllProvider? GetFixAllProvider()
		{
			return WellKnownFixAllProviders.BatchFixer;
		}

		/// <inheritdoc/>
		public override async Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			Diagnostic? diagnostic = context.Diagnostics.FirstOrDefault();

			if (diagnostic is null)
			{
				return;
			}

			SyntaxNode? root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
			TypeDeclarationSyntax? declaration = root?.FindNode(diagnostic.Location.SourceSpan)?.FirstAncestorOrSelf<TypeDeclarationSyntax>();

			if (declaration is null)
			{
				return;
			}

			Document document = context.Document;

			context.RegisterCodeFix(
				CodeAction.Create(
					title: Title,
					createChangedDocument: _ => MakePartialAsync(document, root!, declaration),
					equivalenceKey: Title),
				diagnostic
			);
		}

		private static Task<Document> MakePartialAsync(Document document, SyntaxNode root, TypeDeclarationSyntax declaration)
		{
			TypeDeclarationSyntax newDeclaration = declaration.WithModifiers(SyntaxFactory.TokenList(declaration.Modifiers.Add(SyntaxFactory.Token(SyntaxKind.PartialKeyword))));
			SyntaxNode newRoot = root.ReplaceNode(declaration, newDeclaration);

			return Task.FromResult(document.WithSyntaxRoot(newRoot));
		}
	}
}
