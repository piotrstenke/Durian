using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;

namespace Durian.CodeFixes
{
	/// <summary>
	/// Base class for all Durian code fixes.
	/// </summary>
	/// <typeparam name="T">Type of <see cref="CSharpSyntaxNode"/> this <see cref="DurianCodeFix{T}"/> can handle.</typeparam>
	public abstract class DurianCodeFix<T> : CodeFixProvider where T : CSharpSyntaxNode
	{
		/// <inheritdoc/>
		public sealed override ImmutableArray<string> FixableDiagnosticIds
		{
			get
			{
				return ImmutableArray.Create(GetSupportedDiagnostics().Select(d => d.Id).ToArray());
			}
		}

		/// <summary>
		/// Title of this <see cref="DurianCodeFix{T}"/>.
		/// </summary>
		public abstract string Title { get; }

		/// <summary>
		/// Creates a new instance of the <see cref="DurianCodeFix{T}"/> class.
		/// </summary>
		protected DurianCodeFix()
		{
		}

		/// <inheritdoc/>
		public override FixAllProvider? GetFixAllProvider()
		{
			return WellKnownFixAllProviders.BatchFixer;
		}

		/// <inheritdoc/>
		public override async Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			Diagnostic? diagnostic = context.Diagnostics.FirstOrDefault();

			if (diagnostic is null || !ValidateDiagnostic(diagnostic))
			{
				return;
			}

			CSharpSyntaxNode? root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false) as CSharpSyntaxNode;
			T? node = root?.FindNode(diagnostic.Location.SourceSpan)?.FirstAncestorOrSelf<T>();

			if (node is null)
			{
				return;
			}

			Document document = context.Document;
			CodeAction? action = GetCodeAction(document, diagnostic, root!, node, context.CancellationToken);

			if (action is null)
			{
				return;
			}

			context.RegisterCodeFix(action, diagnostic);
		}

		/// <summary>
		/// Determines, whether the specified <paramref name="diagnostic"/> is valid for this <see cref="DurianCodeFix{T}"/>.
		/// </summary>
		/// <param name="diagnostic"><see cref="Diagnostic"/> to validate.</param>
		protected virtual bool ValidateDiagnostic(Diagnostic diagnostic)
		{
			return true;
		}

		/// <summary>
		/// Returns a <see cref="CodeAction"/> to be executed.
		/// </summary>
		/// <param name="document">Current <see cref="Document"/>.</param>
		/// <param name="diagnostic">Current <see cref="Diagnostic"/>.</param>
		/// <param name="root">Root <see cref="SyntaxNode"/> of the <paramref name="document"/>.</param>
		/// <param name="node">Target <see cref="CSharpSyntaxNode"/>.</param>
		/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
		protected virtual CodeAction? GetCodeAction(Document document, Diagnostic diagnostic, CSharpSyntaxNode root, T node, CancellationToken cancellationToken)
		{
			return CodeAction.Create(Title, cancellationToken => Execute(document, root, node, cancellationToken), Title);
		}

		/// <summary>
		/// Actually executes the <see cref="CodeAction"/>.
		/// </summary>
		/// <param name="document">Current <see cref="Document"/>.</param>
		/// <param name="root">Root <see cref="SyntaxNode"/> of the <paramref name="document"/>.</param>
		/// <param name="node">Target <see cref="CSharpSyntaxNode"/>.</param>
		/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
		protected abstract Task<Document> Execute(Document document, CSharpSyntaxNode root, T node, CancellationToken cancellationToken);

		/// <summary>
		/// Returns the <see cref="DiagnosticDescriptor"/>s supported by this <see cref="DurianCodeFix{T}"/>.
		/// </summary>
		protected abstract DiagnosticDescriptor[] GetSupportedDiagnostics();
	}
}
