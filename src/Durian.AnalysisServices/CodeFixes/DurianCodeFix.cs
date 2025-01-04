using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.Analysis.CodeFixes
{
	/// <summary>
	/// Base class for all Durian code fixes that provides methods for straight-up registration and execution of <see cref="CodeAction"/>s.
	/// </summary>
	/// <typeparam name="T">Type of <see cref="SyntaxNode"/> this <see cref="DurianCodeFix{T}"/> can handle.</typeparam>
	public abstract class DurianCodeFix<T> : DurianCodeFixBase where T : SyntaxNode
	{
		/// <summary>
		/// Creates a new instance of the <see cref="DurianCodeFix{T}"/> class.
		/// </summary>
		protected DurianCodeFix()
		{
		}

		/// <inheritdoc/>
		public override Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			return RegisterCodeFixesAsync(context, false);
		}

		/// <summary>
		/// Actually executes the <see cref="CodeAction"/>.
		/// </summary>
		/// <param name="context"><see cref="CodeFixExecutionContext{T}"/> that contains all the data needed to properly perform a code fix.</param>
		protected abstract Task<Document> ExecuteAsync(CodeFixExecutionContext<T> context);

		/// <summary>
		/// Returns a <see cref="CodeAction"/> to be executed.
		/// </summary>
		/// <param name="data">Represents data that is used when creating a <see cref="CodeAction"/> for the code fix.</param>
		protected virtual async Task<CodeAction?> GetCodeActionAsync(CodeFixData<T> data)
		{
			if (!data.Success || !data.HasNode)
			{
				return null;
			}

			Document document = data.Document;
			T node = data.Node;
			CompilationUnitSyntax root = data.Root;
			Diagnostic diagnostic = data.Diagnostic;
			SemanticModel? semanticModel = data.SemanticModel ?? await document.GetSemanticModelAsync(data.CancellationToken).ConfigureAwait(false);

			if (semanticModel is null)
			{
				return null;
			}

			return CodeAction.Create(Title, cancellationToken => ExecuteAsync(CodeFixExecutionContext<T>.From(
				diagnostic,
				document,
				root,
				node,
				semanticModel,
				cancellationToken)), Id);
		}

		/// <summary>
		/// Computes one or more fixes for the specified Microsoft.CodeAnalysis.CodeFixes.CodeFixContext.
		/// </summary>
		/// <param name="context">A Microsoft.CodeAnalysis.CodeFixes.CodeFixContext containing context information about the diagnostics to fix. The context must only contain diagnostics with a <see cref="Diagnostic.Id"/> included in the <see cref="CodeFixProvider.FixableDiagnosticIds"/> for the current provider.</param>
		/// <param name="includeSemanticModel">Determines whether to include the <see cref="SemanticModel"/> when creating this <see cref="CodeFixData{T}"/>.</param>
		protected async Task RegisterCodeFixesAsync(CodeFixContext context, bool includeSemanticModel)
		{
			CodeFixData<T> data = await CodeFixData<T>.FromAsync(context, includeSemanticModel).ConfigureAwait(false);
			CodeAction? action = await GetCodeActionAsync(data).ConfigureAwait(false);

			if (action is null)
			{
				return;
			}

			context.RegisterCodeFix(action, data.Diagnostic!);
		}
	}
}
