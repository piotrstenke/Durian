using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
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
	/// <summary>
	/// Removes the <see cref="GeneratedCodeAttribute"/> from the declaration.
	/// </summary>
	[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RemoveDurianGeneratedAttributeCodeFix))]
	[Shared]
	public sealed class RemoveDurianGeneratedAttributeCodeFix : CodeFixProvider
	{
		/// <summary>
		/// Title of this code fix that is displayed to the user.
		/// </summary>
		public static string Title => "Remove 'DurianGenerated' attribute";

		/// <inheritdoc/>
		public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(
			DefaultParamDiagnostics.Descriptors.DefaultParamAttributeCannotBeAppliedToMembersWithAttribute.Id
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

			if (root is null)
			{
				return;
			}

			MemberDeclarationSyntax? declaration = root.FindNode(diagnostic.Location.SourceSpan)?.FirstAncestorOrSelf<MemberDeclarationSyntax>();
			Document document = context.Document;
			Func<CancellationToken, Task<Document>> function;

			if (declaration is null)
			{
				AttributeListSyntax? attr = root.FindNode(diagnostic.Location.SourceSpan)?.FirstAncestorOrSelf<AttributeListSyntax>();

				if (attr is null)
				{
					return;
				}

				function = token => RemoveDurianGeneratedAttributeAsync(document, root, attr, token);
			}
			else
			{
				function = token => RemoveDurianGeneratedAttributeAsync(document, root, declaration, token);
			}

			context.RegisterCodeFix(
				CodeAction.Create(
					title: Title,
					createChangedDocument: function,
					equivalenceKey: Title),
				diagnostic
			);
		}

		private static async Task<Document> RemoveDurianGeneratedAttributeAsync(Document document, SyntaxNode root, MemberDeclarationSyntax node, CancellationToken cancellationToken)
		{
			SemanticModel? semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
			INamedTypeSymbol? durianGenerated = GetAttribute(semanticModel);

			if (semanticModel is null || durianGenerated is null)
			{
				return document;
			}

			IEnumerable<AttributeListSyntax> attributeLists = node.AttributeLists
				.Where(list => list.Attributes
					.Select(attr => semanticModel.GetSymbolInfo(attr, cancellationToken).Symbol)
					.Any(attr => attr is not null && !SymbolEqualityComparer.Default.Equals(attr.ContainingSymbol, durianGenerated)));

			MemberDeclarationSyntax newNode = node.WithAttributeLists(SyntaxFactory.List(attributeLists));
			SyntaxNode newRoot = root.ReplaceNode(node, newNode);

			return document.WithSyntaxRoot(newRoot);
		}

		private static async Task<Document> RemoveDurianGeneratedAttributeAsync(Document document, SyntaxNode root, AttributeListSyntax attrList, CancellationToken cancellationToken)
		{
			SemanticModel? semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
			INamedTypeSymbol? durianGenerated = GetAttribute(semanticModel);

			if (semanticModel is null || durianGenerated is null)
			{
				return document;
			}

			IEnumerable<AttributeSyntax> attributes = attrList.Attributes
				.Select(attr => (attr, semanticModel.GetSymbolInfo(attr, cancellationToken).Symbol))
				.Where(attrData => attrData.Symbol is not null && !SymbolEqualityComparer.Default.Equals(attrData.Symbol.ContainingSymbol, durianGenerated))
				.Select(attrData => attrData.attr);

			SeparatedSyntaxList<AttributeSyntax> list = SyntaxFactory.SeparatedList(attributes);

			if (list.Any())
			{
				AttributeListSyntax newAttrList = attrList.WithAttributes(list);
				SyntaxNode newRoot = root.ReplaceNode(attrList, newAttrList);
				return document.WithSyntaxRoot(newRoot);
			}
			else
			{
				SyntaxNode? newRoot = root.RemoveNode(attrList, SyntaxRemoveOptions.KeepEndOfLine | SyntaxRemoveOptions.KeepDirectives);

				if (newRoot is null)
				{
					return document;
				}

				return document.WithSyntaxRoot(newRoot);
			}
		}

		private static INamedTypeSymbol? GetAttribute(SemanticModel? semanticModel)
		{
			return semanticModel?.Compilation.GetTypeByMetadataName($"{DurianStrings.GeneratorAttributesNamespace}.DurianGeneratedAttribute");
		}
	}
}
