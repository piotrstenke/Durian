// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Durian.Analysis.CodeFixes
{
	/// <summary>
	/// A code fix that replaces a specific <see cref="CSharpSyntaxNode"/> with a new one.
	/// </summary>
	/// <typeparam name="T">Type of <see cref="CSharpSyntaxNode"/> to replace.</typeparam>
	/// <typeparam name="U">Type of <see cref="CSharpSyntaxNode"/> to replace old <see cref="CSharpSyntaxNode"/> with.</typeparam>
	public abstract class ReplaceNodeCodeFix<T, U> : DurianCodeFix<T> where T : CSharpSyntaxNode where U : CSharpSyntaxNode
	{
		/// <summary>
		/// Creates a new instance of the <see cref="ReplaceNodeCodeFix{T}"/> class.
		/// </summary>
		protected ReplaceNodeCodeFix()
		{
		}

		/// <inheritdoc/>
		protected sealed override Task<Document> ExecuteAsync(CodeFixExecutionContext<T> context)
		{
			U newNode = GetNewNode(context.Node, context.SemanticModel);
			SyntaxNode newRoot = context.Root.ReplaceNode(context.Node, newNode);

			return Task.FromResult(context.Document.WithSyntaxRoot(newRoot));
		}

		/// <inheritdoc cref="ReplaceNodeCodeFix{T}.GetNewNode(T, SemanticModel)"/>
		protected abstract U GetNewNode(T currentNode, SemanticModel semanticModel);
	}
}
