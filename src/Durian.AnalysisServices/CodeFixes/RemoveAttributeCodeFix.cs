// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

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

		/// <inheritdoc/>
		public override async Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			CodeFixData<T> data = await CodeFixData<T>.FromAsync(context).ConfigureAwait(false);

			if (!data.Success)
			{
				return;
			}

			SemanticModel? semanticModel = await data.Document.GetSemanticModelAsync(data.CancellationToken).ConfigureAwait(false);

			if (semanticModel is null)
			{
				return;
			}

			INamedTypeSymbol[] attributes = GetAttributeSymbols((CSharpCompilation)semanticModel.Compilation, context.CancellationToken).ToArray();

			if (attributes.Length == 0)
			{
				return;
			}

			CodeAction? action = await GetCodeActionAsync(data);

			if (action is null)
			{
				return;
			}

			context.RegisterCodeFix(action, context.Diagnostics);
		}

		/// <inheritdoc/>
		protected override async Task<Document> ExecuteAsync(CodeFixExecutionContext<T> context)
		{
			INamedTypeSymbol[] attributes = GetAttributeSymbols((CSharpCompilation)context.Compilation, context.CancellationToken).ToArray();

			if (attributes.Length == 0)
			{
				return context.Document;
			}

			return await RemoveAttributeCodeFix<T>.RemoveAttributesAsync(context.Document, context.SemanticModel, attributes, context.Root, context.Node, context.CancellationToken).ConfigureAwait(false);
		}

		/// <inheritdoc/>
		protected override Task<CodeAction?> GetCodeActionAsync(CodeFixData<T> data)
		{
			if (!data.Success || !data.HasSemanticModel)
			{
				return Task.FromResult<CodeAction?>(null);
			}

			INamedTypeSymbol[] attributes = GetAttributeSymbols((CSharpCompilation)data.SemanticModel.Compilation, data.CancellationToken).ToArray();

			if (attributes.Length == 0)
			{
				return Task.FromResult<CodeAction?>(null);
			}

			Func<CancellationToken, Task<Document>> function;

			Document document = data.Document;
			CompilationUnitSyntax root = data.Root;
			Diagnostic diagnostic = data.Diagnostic;
			SemanticModel semanticModel = data.SemanticModel;

			if (data.HasNode)
			{
				T node = data.Node;

				function = cancellationToken => RemoveAttributesAsync(document, semanticModel, attributes, root, node, cancellationToken);
			}
			else
			{
				AttributeListSyntax? attr = root.FindNode(diagnostic.Location.SourceSpan)?.FirstAncestorOrSelf<AttributeListSyntax>();

				if (attr is null)
				{
					return Task.FromResult<CodeAction?>(null);
				}

				function = cancellationToken => RemoveAttributesAsync(document, semanticModel, attributes, root, attr, cancellationToken);
			}

			return Task.FromResult<CodeAction?>(CodeAction.Create(Title, function, Id));
		}

		private static Task<Document> RemoveAttributesAsync(Document document, SemanticModel semanticModel, INamedTypeSymbol[] attributes, CompilationUnitSyntax root, T node, CancellationToken cancellationToken)
		{
			SyntaxList<AttributeListSyntax> attributeLists = SyntaxFactory.List(node.AttributeLists
				.Where(attrList => attrList.Attributes
					.Select(attr => semanticModel.GetSymbolInfo(attr, cancellationToken).Symbol)
					.Any(attr => attr is not null && !attributes.Any(a => SymbolEqualityComparer.Default.Equals(attr.ContainingSymbol, a)))));

			if (attributeLists.Any())
			{
				MemberDeclarationSyntax newNode = node.WithAttributeLists(attributeLists);
				SyntaxNode newRoot = root.ReplaceNode(node, newNode);

				return Task.FromResult(document.WithSyntaxRoot(newRoot));
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

				return Task.FromResult(document.WithSyntaxRoot(newRoot));
			}
		}

		private static Task<Document> RemoveAttributesAsync(Document document, SemanticModel semanticModel, INamedTypeSymbol[] attributes, CompilationUnitSyntax root, AttributeListSyntax attrList, CancellationToken cancellationToken)
		{
			SeparatedSyntaxList<AttributeSyntax> list = SyntaxFactory.SeparatedList(attrList.Attributes
				.Select(attr => (attr, semanticModel.GetSymbolInfo(attr, cancellationToken).Symbol))
				.Where(attrData => attrData.Symbol is not null && !attributes.Any(attr => SymbolEqualityComparer.Default.Equals(attrData.Symbol.ContainingSymbol, attr)))
				.Select(attrData => attrData.attr));

			if (list.Any())
			{
				AttributeListSyntax newAttrList = attrList.WithAttributes(list);
				SyntaxNode newRoot = root.ReplaceNode(attrList, newAttrList);
				return Task.FromResult(document.WithSyntaxRoot(newRoot));
			}
			else
			{
				SyntaxNode? newRoot = root.RemoveNode(attrList, SyntaxRemoveOptions.KeepEndOfLine | SyntaxRemoveOptions.KeepDirectives);

				if (newRoot is null)
				{
					return Task.FromResult(document);
				}

				return Task.FromResult(document.WithSyntaxRoot(newRoot));
			}
		}
	}
}
