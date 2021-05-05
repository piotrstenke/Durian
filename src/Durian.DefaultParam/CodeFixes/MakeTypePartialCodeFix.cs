using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.DefaultParam.CodeFixes
{
	[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(MakeTypePartialCodeFix))]
	[Shared]
	public sealed class MakeTypePartialCodeFix : CodeFixProvider
	{
		public static string Title => "Make type partial";

		public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(
			DefaultParamDiagnostics.Descriptors.ParentTypeOfMemberWithDefaultParamAttributeMustBePartial.Id
		);

		public override FixAllProvider? GetFixAllProvider()
		{
			return WellKnownFixAllProviders.BatchFixer;
		}

		public override async Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			SyntaxNode? root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

			if (root is null)
			{
				return;
			}

			Diagnostic diagnostic = context.Diagnostics[0];
			TypeDeclarationSyntax declaration = root.FindToken(diagnostic.Location.SourceSpan.Start).Parent!.AncestorsAndSelf().OfType<TypeDeclarationSyntax>().First();

			context.RegisterCodeFix(CodeAction.Create(
				title: Title,
				createChangedDocument: token => MakePartialAsync(context.Document, root, declaration, token),
				equivalenceKey: Title),
				diagnostic
			);
		}

		private static Task<Document> MakePartialAsync(Document document, SyntaxNode root, TypeDeclarationSyntax declaration, CancellationToken cancellationToken)
		{
			return Task.Run(() =>
			{
				TypeDeclarationSyntax newDeclaration = declaration.WithModifiers(SyntaxFactory.TokenList(declaration.Modifiers.Add(SyntaxFactory.Token(SyntaxKind.PartialKeyword))));
				SyntaxNode newRoot = root.ReplaceNode(declaration, newDeclaration);

				return document.WithSyntaxRoot(newRoot);
			},
			cancellationToken);
		}
	}
}
