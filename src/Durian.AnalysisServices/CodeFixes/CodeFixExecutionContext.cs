using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.Generator.CodeFixes
{
	/// <summary>
	/// Contains data needed to execute a code fix.
	/// </summary>
	/// <typeparam name="T">Type of <see cref="CSharpSyntaxNode"/> this <see cref="CodeFixExecutionContext{T}"/> can store.</typeparam>
	public struct CodeFixExecutionContext<T> where T : CSharpSyntaxNode
	{
		private SemanticModel? _semanticModel;

		/// <summary>
		/// <see cref="System.Threading.CancellationToken"/> that specifies if the operation should be canceled.
		/// </summary>
		public CancellationToken CancellationToken { get; }

		/// <summary>
		/// <see cref="Microsoft.CodeAnalysis.Document"/> where the <see cref="Diagnostic"/> is to be found.
		/// </summary>
		public Document Document { get; private set; }

		/// <summary>
		/// <see cref="Microsoft.CodeAnalysis.Diagnostic"/> that the code fix is being proposed for.
		/// </summary>
		public Diagnostic Diagnostic { get; }

		/// <summary>
		/// Root node of the analyzed tree.
		/// </summary>
		public CompilationUnitSyntax Root { get; private set; }

		/// <summary>
		/// <see cref="CSharpSyntaxNode"/> this context represents.
		/// </summary>
		public T Node { get; }

		private CodeFixExecutionContext(Diagnostic diagnostic, Document document, SemanticModel? semanticModel, CompilationUnitSyntax root, T node, CancellationToken cancellationToken)
		{
			Diagnostic = diagnostic;
			Document = document;
			Root = root;
			CancellationToken = cancellationToken;
			Node = node;
			_semanticModel = semanticModel;
		}

		/// <summary>
		/// Registers a change performed by the code fix.
		/// </summary>
		/// <param name="original">Original <see cref="CSharpSyntaxNode"/> that is begin replaced by the <paramref name="updated"/> node.</param>
		/// <param name="updated"><see cref="CSharpSyntaxNode"/> to replace the <paramref name="original"/> node with.</param>
		public void RegisterChange(CSharpSyntaxNode original, CSharpSyntaxNode updated)
		{
			Root = Root.ReplaceNode(original, updated);
		}

		/// <summary>
		/// Registers a change performed by the code fix and updates the root of the <see cref="Document"/>.
		/// </summary>
		/// <param name="original">Original <see cref="CSharpSyntaxNode"/> that is begin replaced by the <paramref name="updated"/> node.</param>
		/// <param name="updated"><see cref="CSharpSyntaxNode"/> to replace the <paramref name="original"/> node with.</param>
		public void RegisterChangeAndUpdateDocument(CSharpSyntaxNode original, CSharpSyntaxNode updated)
		{
			RegisterChange(original, updated);
			UpdateDocument();
		}

		/// <summary>
		/// Sets the <see cref="Root"/> to a new value.
		/// </summary>
		/// <param name="root">Value to set as <see cref="Root"/>.</param>
		public void SetRoot(CompilationUnitSyntax root)
		{
			Root = root;
		}

		/// <summary>
		/// Updates the root of the <see cref="Document"/>.
		/// </summary>
		public void UpdateDocument()
		{
			Document = Document.WithSyntaxRoot(Root);
		}

		/// <summary>
		/// Converts this context to a context with the specified <paramref name="node"/>.
		/// </summary>
		/// <typeparam name="TNode">Type of node.</typeparam>
		/// <param name="node">New target node of the context.</param>
		public CodeFixExecutionContext<TNode> WithNode<TNode>(TNode node) where TNode : CSharpSyntaxNode
		{
			return new CodeFixExecutionContext<TNode>(Diagnostic, Document, _semanticModel, Root, node, CancellationToken);
		}

		/// <summary>
		/// Gets the <see cref="SemanticModel"/> of the target <see cref="Node"/>.
		/// </summary>
		/// <param name="cancellationToken"><see cref="System.Threading.CancellationToken"/> that specifies if the operation should be canceled.</param>
		public async Task<SemanticModel?> GetSemanticModelAsync(CancellationToken cancellationToken = default)
		{
			if (_semanticModel is not null)
			{
				return _semanticModel;
			}

			return _semanticModel = await Document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
		}

		/// <summary>
		/// Creates a new <see cref="CodeFixExecutionContext{T}"/> from the specified data.
		/// </summary>
		/// <param name="diagnostic"><see cref="Microsoft.CodeAnalysis.Diagnostic"/> that the code fix is being proposed for.</param>
		/// <param name="document"><see cref="Microsoft.CodeAnalysis.Document"/> where the <see cref="Diagnostic"/> is to be found.</param>
		/// <param name="semanticModel"><see cref="SemanticModel"/> of the target <paramref name="node"/>.</param>
		/// <param name="root">Root node of the analyzed tree.</param>
		/// <param name="node"><see cref="CSharpSyntaxNode"/> this context represents.</param>
		/// <param name="cancellationToken"><see cref="System.Threading.CancellationToken"/> that specifies if the operation should be canceled.</param>
		public static CodeFixExecutionContext<T> From(Diagnostic diagnostic, Document document, SemanticModel? semanticModel, CompilationUnitSyntax root, T node, CancellationToken cancellationToken = default)
		{
			return new CodeFixExecutionContext<T>(diagnostic, document, semanticModel, root, node, cancellationToken);
		}

		/// <inheritdoc cref="From(in CodeFixData{T}, CancellationToken)"/>
		public static CodeFixExecutionContext<T> From(in CodeFixData<T> data)
		{
			return From(in data, data.CancellationToken);
		}

		/// <summary>
		/// Creates a new <see cref="CodeFixExecutionContext{T}"/> from the specified <paramref name="data"/>.
		/// </summary>
		/// <param name="data"><see cref="CodeFixData{T}"/> to create the <see cref="CodeFixExecutionContext{T}"/> from.</param>
		/// <param name="cancellationToken"><see cref="System.Threading.CancellationToken"/> that specifies if the operation should be canceled.</param>
		/// <exception cref="InvalidOperationException">The <see cref="CodeFixData{T}.Success"/> property of <paramref name="data"/> returned <see langword="false"/>. -or- The <see cref="CodeFixData{T}.HasNode"/> property of <paramref name="data"/> returned <see langword="false"/>.</exception>
		public static CodeFixExecutionContext<T> From(in CodeFixData<T> data, CancellationToken cancellationToken)
		{
			if (!data.Success)
			{
				throw new InvalidOperationException($"The {nameof(CodeFixData<T>.Success)} property of '{nameof(data)}' returned false!");
			}

			if (!data.HasNode)
			{
				throw new InvalidOperationException($"The {nameof(CodeFixData<T>.HasNode)} property of '{nameof(data)}' returned false!");
			}

			return new CodeFixExecutionContext<T>(data.Diagnostic, data.Document, data.SemanticModel, data.Root, data.Node, cancellationToken);
		}
	}
}
