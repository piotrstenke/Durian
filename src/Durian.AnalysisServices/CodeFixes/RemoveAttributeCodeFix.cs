using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.Generator.CodeFixes
{
	/// <summary>
	/// A code fix that removes specific attributes.
	/// </summary>
	/// <typeparam name="T">Type of <see cref="CSharpSyntaxNode"/> this <see cref="DurianCodeFix{T}"/> can handle.</typeparam>
	public abstract class RemoveAttributeCodeFix<T> : DurianCodeFix<T> where T : MemberDeclarationSyntax
	{
		/// <summary>
		/// Creates a new instance of the <see cref="RemoveAttributeCodeFix{T}"/> class.
		/// </summary>
		protected RemoveAttributeCodeFix()
		{
		}

		/// <summary>
		/// Returns a collection of <see cref="INamedTypeSymbol"/>s that represent the attributes to remove.
		/// </summary>
		/// <param name="compilation">Current <see cref="CSharpCompilation"/>.</param>
		/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
		public abstract IEnumerable<INamedTypeSymbol> GetAttributeSymbols(CSharpCompilation compilation, CancellationToken cancellationToken = default);

#pragma warning disable IDE0019 // Use pattern matching
#pragma warning disable RCS1221 // Use pattern matching instead of combination of 'as' operator and null check.
		/// <inheritdoc/>
		public override async Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			Diagnostic? diagnostic = context.Diagnostics.FirstOrDefault();

			if (diagnostic is null)
			{
				return;
			}

			CSharpSyntaxNode? root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false) as CSharpSyntaxNode;

			if (root is null)
			{
				return;
			}

			T? node = root.FindNode(diagnostic.Location.SourceSpan)?.FirstAncestorOrSelf<T>();
			Document document = context.Document;
			CodeAction? action = GetCodeAction(document, diagnostic, root, node!, context.CancellationToken);

			if (action is null)
			{
				return;
			}

			context.RegisterCodeFix(action, diagnostic);
		}
#pragma warning restore RCS1221 // Use pattern matching instead of combination of 'as' operator and null check.
#pragma warning restore IDE0019 // Use pattern matching

		/// <inheritdoc/>
		protected override CodeAction? GetCodeAction(Document document, Diagnostic diagnostic, CSharpSyntaxNode root, T node, CancellationToken cancellationToken)
		{
			Func<CancellationToken, Task<Document>> function;

			if (node is null)
			{
				AttributeListSyntax? attr = root.FindNode(diagnostic.Location.SourceSpan)?.FirstAncestorOrSelf<AttributeListSyntax>();

				if (attr is null)
				{
					return null;
				}

				function = token => RemoveAttributesAsync(document, root, attr, token);
			}
			else
			{
				function = token => RemoveAttributesAsync(document, root, node, token);
			}

			return CodeAction.Create(Title, function, Title);
		}

		/// <inheritdoc/>
		protected override Task<Document> Execute(Document document, CSharpSyntaxNode root, T node, CancellationToken cancellationToken)
		{
			return RemoveAttributesAsync(document, root, node, cancellationToken);
		}

		private async Task<Document> RemoveAttributesAsync(Document document, CSharpSyntaxNode root, T node, CancellationToken cancellationToken)
		{
			SemanticModel? semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);

			if (semanticModel is null)
			{
				return document;
			}

			INamedTypeSymbol[] attributes = GetAttributeSymbols((CSharpCompilation)semanticModel.Compilation, cancellationToken).ToArray();

			if (attributes.Length == 0)
			{
				return document;
			}

			SyntaxList<AttributeListSyntax> attributeLists = SyntaxFactory.List(node.AttributeLists
				.Where(attrList => attrList.Attributes
					.Select(attr => semanticModel.GetSymbolInfo(attr, cancellationToken).Symbol)
					.Any(attr => attr is not null && !attributes.Any(a => SymbolEqualityComparer.Default.Equals(attr.ContainingSymbol, a)))));

			if (attributeLists.Any())
			{
				MemberDeclarationSyntax newNode = node.WithAttributeLists(attributeLists);
				SyntaxNode newRoot = root.ReplaceNode(node, newNode);

				return document.WithSyntaxRoot(newRoot);
			}
			else
			{
				MemberDeclarationSyntax newNode = node;

				foreach (AttributeListSyntax list in node.AttributeLists)
				{
					MemberDeclarationSyntax? n = newNode.RemoveNode(list, SyntaxRemoveOptions.KeepEndOfLine | SyntaxRemoveOptions.KeepDirectives);

					if (n is null)
					{
						continue;
					}

					newNode = n;
				}

				SyntaxNode newRoot = root.ReplaceNode(node, newNode);

				return document.WithSyntaxRoot(newRoot);
			}
		}

		private async Task<Document> RemoveAttributesAsync(Document document, CSharpSyntaxNode root, AttributeListSyntax attrList, CancellationToken cancellationToken)
		{
			SemanticModel? semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);

			if (semanticModel is null)
			{
				return document;
			}

			INamedTypeSymbol[] attributes = GetAttributeSymbols((CSharpCompilation)semanticModel.Compilation, cancellationToken).ToArray();

			if (attributes.Length == 0)
			{
				return document;
			}

			SeparatedSyntaxList<AttributeSyntax> list = SyntaxFactory.SeparatedList(attrList.Attributes
				.Select(attr => (attr, semanticModel.GetSymbolInfo(attr, cancellationToken).Symbol))
				.Where(attrData => attrData.Symbol is not null && !attributes.Any(attr => SymbolEqualityComparer.Default.Equals(attrData.Symbol.ContainingSymbol, attr)))
				.Select(attrData => attrData.attr));

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
	}
}
