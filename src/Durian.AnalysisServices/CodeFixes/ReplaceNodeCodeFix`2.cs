using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.Analysis.CodeFixes;

/// <summary>
/// A code fix that replaces a specific <see cref="SyntaxNode"/> with a new one.
/// </summary>
/// <typeparam name="TOld">Type of <see cref="SyntaxNode"/> to replace.</typeparam>
/// <typeparam name="TNew">Type of <see cref="SyntaxNode"/> to replace old <see cref="SyntaxNode"/> with.</typeparam>
public abstract class ReplaceNodeCodeFix<TOld, TNew> : DurianCodeFix<TOld> where TOld : SyntaxNode where TNew : SyntaxNode
{
	/// <summary>
	/// Creates a new instance of the <see cref="ReplaceNodeCodeFix{T}"/> class.
	/// </summary>
	protected ReplaceNodeCodeFix()
	{
	}

	/// <inheritdoc/>
	protected sealed override Task<Document> ExecuteAsync(CodeFixExecutionContext<TOld> context)
	{
		TNew newNode = GetNewNode(context.Node, context.Compilation, context.SemanticModel, out INamespaceSymbol[]? requiredNamespaces);
		CompilationUnitSyntax newRoot = context.Root.ReplaceNode(context.Node, newNode);

		if (requiredNamespaces?.Length > 0)
		{
			newRoot = newRoot.AddUsings(requiredNamespaces);
		}

		return Task.FromResult(context.Document.WithSyntaxRoot(newRoot));
	}

	/// <inheritdoc cref="ReplaceNodeCodeFix{T}.GetNewNode(T, CSharpCompilation, SemanticModel, out INamespaceSymbol[])"/>
	protected abstract TNew GetNewNode(TOld currentNode, CSharpCompilation compilation, SemanticModel semanticModel, out INamespaceSymbol[]? requiredNamespaces);
}
