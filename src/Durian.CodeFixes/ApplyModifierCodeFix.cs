using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.CodeFixes
{
	/// <summary>
	/// A code fix that applies a specified modifier or modifiers.
	/// </summary>
	/// <typeparam name="T">Type of <see cref="CSharpSyntaxNode"/> this <see cref="DurianCodeFix{T}"/> can handle.</typeparam>
	public abstract class ApplyModifierCodeFix<T> : DurianCodeFix<T> where T : MemberDeclarationSyntax
	{
		/// <summary>
		/// An array of <see cref="SyntaxKind"/>s representing the modifiers to be applied.
		/// </summary>
		public abstract SyntaxKind[] ModifiersToApply { get; }

		/// <summary>
		/// Creates a new instance of the <see cref="ApplyModifierCodeFix{T}"/> class.
		/// </summary>
		protected ApplyModifierCodeFix()
		{
		}

		/// <inheritdoc/>
		protected override DiagnosticDescriptor[] GetSupportedDiagnostics()
		{
			return new DiagnosticDescriptor[] { DurianDescriptors.MemberWithAttributeMustHaveModifier };
		}

		/// <inheritdoc/>
		protected override Task<Document> Execute(Document document, CSharpSyntaxNode root, T node, CancellationToken cancellationToken)
		{
			MemberDeclarationSyntax newNode = node.AddModifiers(ModifiersToApply.Select(m => SyntaxFactory.Token(m)).ToArray());
			SyntaxNode newRoot = root.ReplaceNode(node, newNode);

			return Task.FromResult(document.WithSyntaxRoot(newRoot));
		}
	}
}
