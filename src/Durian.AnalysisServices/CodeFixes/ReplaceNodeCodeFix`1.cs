// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System.Threading.Tasks;
using Durian.Analysis.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.Analysis.CodeFixes
{
	/// <summary>
	/// A code fix that replaces a specific <see cref="CSharpSyntaxNode"/> with a new one.
	/// </summary>
	/// <typeparam name="T">Type of <see cref="CSharpSyntaxNode"/> to replace.</typeparam>
	public abstract class ReplaceNodeCodeFix<T> : DurianCodeFix<T> where T : CSharpSyntaxNode
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
			SyntaxNode newNode = GetNewNode(context.Node, context.Compilation, context.SemanticModel, out INamespaceSymbol[]? requiredNamespaces);
			CompilationUnitSyntax newRoot = context.Root.ReplaceNode(context.Node, newNode);

			if(requiredNamespaces?.Length > 0)
			{
				newRoot = newRoot.AddUsings(requiredNamespaces);
			}

			return Task.FromResult(context.Document.WithSyntaxRoot(newRoot));
		}

		/// <summary>
		/// Returns the <see cref="SyntaxNode"/> to replace the <paramref name="currentNode"/> with.
		/// </summary>
		/// <param name="currentNode"><see cref="SyntaxNode"/> to be replaced.</param>
		/// <param name="compilation">Current <see cref="CSharpCompilation"/>.</param>
		/// <param name="semanticModel">Current <see cref="SemanticModel"/>.</param>
		/// <param name="requiredNamespaces">Collection of symbols of <see langword="namespace"/>s that should be added to the current <see cref="SyntaxTree"/>'s using directives.</param>
		protected abstract SyntaxNode GetNewNode(T currentNode, CSharpCompilation compilation, SemanticModel semanticModel, out INamespaceSymbol[]? requiredNamespaces);
	}
}
