using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace Durian.Analysis.CodeFixes
{
	/// <summary>
	/// A code fix that removes a specific <see cref="SyntaxNode"/>.
	/// </summary>
	/// <typeparam name="T">Type of <see cref="SyntaxNode"/> to remove.</typeparam>
	public abstract class RemoveNodeCodeFix<T> : DurianCodeFix<T> where T : SyntaxNode
	{
		/// <summary>
		/// Creates a new instance of the <see cref="RemoveNodeCodeFix{T}"/> class.
		/// </summary>
		protected RemoveNodeCodeFix()
		{
		}

		/// <inheritdoc/>
		protected sealed override Task<Document> ExecuteAsync(CodeFixExecutionContext<T> context)
		{
			SyntaxNode? newRoot = context.Root.RemoveNode(context.Node, SyntaxRemoveOptions.KeepEndOfLine | SyntaxRemoveOptions.KeepDirectives);

			if (newRoot is null)
			{
				return Task.FromResult(context.Document);
			}

			return Task.FromResult(context.Document.WithSyntaxRoot(newRoot));
		}
	}
}
