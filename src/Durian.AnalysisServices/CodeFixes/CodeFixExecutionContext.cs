﻿using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.Analysis.CodeFixes;

/// <summary>
/// Contains data needed to execute a code fix.
/// </summary>
/// <typeparam name="T">Type of <see cref="SyntaxNode"/> this <see cref="CodeFixExecutionContext{T}"/> can store.</typeparam>
public sealed class CodeFixExecutionContext<T> where T : SyntaxNode
{
	private readonly SyntaxAnnotation _annotation;

	private SemanticModel? _semanticModel;

	/// <summary>
	/// <see cref="System.Threading.CancellationToken"/> that specifies if the operation should be canceled.
	/// </summary>
	public CancellationToken CancellationToken { get; }

	/// <summary>
	/// Current <see cref="Microsoft.CodeAnalysis.Compilation"/>.
	/// </summary>
	public CSharpCompilation Compilation { get; private set; }

	/// <summary>
	/// <see cref="Microsoft.CodeAnalysis.Diagnostic"/> that the code fix is being proposed for.
	/// </summary>
	public Diagnostic Diagnostic { get; }

	/// <summary>
	/// <see cref="Microsoft.CodeAnalysis.Document"/> where the <see cref="Diagnostic"/> is to be found.
	/// </summary>
	public Document Document { get; private set; }

	/// <summary>
	/// <see cref="SyntaxNode"/> this context represents.
	/// </summary>
	public T Node { get; private set; }

	/// <summary>
	/// Root node of the analyzed tree.
	/// </summary>
	public CompilationUnitSyntax Root { get; private set; }

	/// <summary>
	/// <see cref="Microsoft.CodeAnalysis.SemanticModel"/> of the <see cref="Root"/> node.
	/// </summary>
	public SemanticModel SemanticModel => _semanticModel ??= Compilation.GetSemanticModel(Root.SyntaxTree);

	private CodeFixExecutionContext(
		Diagnostic diagnostic,
		Document document,
		CompilationUnitSyntax root,
		T node,
		CSharpCompilation compilation,
		CancellationToken cancellationToken
	)
	{
		_annotation = new();
		Diagnostic = diagnostic;
		CancellationToken = cancellationToken;
		Root = root.ReplaceNode(node, node.WithAdditionalAnnotations(_annotation));
		Compilation = compilation.ReplaceSyntaxTree(root.SyntaxTree, Root.SyntaxTree);
		Document = document.WithSyntaxRoot(Root);
		Node = Root.GetAnnotatedNodes(_annotation).OfType<T>().First();
		_semanticModel = null;
	}

	/// <summary>
	/// Creates a new <see cref="CodeFixExecutionContext{T}"/> from the specified data.
	/// </summary>
	/// <param name="diagnostic"><see cref="Microsoft.CodeAnalysis.Diagnostic"/> that the code fix is being proposed for.</param>
	/// <param name="document"><see cref="Microsoft.CodeAnalysis.Document"/> where the <see cref="Diagnostic"/> is to be found.</param>
	/// <param name="root">Root node of the analyzed tree.</param>
	/// <param name="node"><see cref="SyntaxNode"/> this context represents.</param>
	/// <param name="compilation">Current <see cref="CSharpCompilation"/>.</param>
	/// <param name="cancellationToken"><see cref="System.Threading.CancellationToken"/> that specifies if the operation should be canceled.</param>
	public static CodeFixExecutionContext<T> From(
		Diagnostic diagnostic,
		Document document,
		CompilationUnitSyntax root,
		T node,
		CSharpCompilation compilation,
		CancellationToken cancellationToken = default
	)
	{
		return new CodeFixExecutionContext<T>(diagnostic, document, root, node, compilation, cancellationToken);
	}

	/// <summary>
	/// Creates a new <see cref="CodeFixExecutionContext{T}"/> from the specified data.
	/// </summary>
	/// <param name="diagnostic"><see cref="Microsoft.CodeAnalysis.Diagnostic"/> that the code fix is being proposed for.</param>
	/// <param name="document"><see cref="Microsoft.CodeAnalysis.Document"/> where the <see cref="Diagnostic"/> is to be found.</param>
	/// <param name="root">Root node of the analyzed tree.</param>
	/// <param name="node"><see cref="SyntaxNode"/> this context represents.</param>
	/// <param name="semanticModel"><see cref="SemanticModel"/> of the target <paramref name="node"/>.</param>
	/// <param name="cancellationToken"><see cref="System.Threading.CancellationToken"/> that specifies if the operation should be canceled.</param>
	public static CodeFixExecutionContext<T> From(
		Diagnostic diagnostic,
		Document document,
		CompilationUnitSyntax root,
		T node,
		SemanticModel semanticModel,
		CancellationToken cancellationToken = default
	)
	{
		return new CodeFixExecutionContext<T>(
			diagnostic,
			document,
			root,
			node,
			(CSharpCompilation)semanticModel.Compilation,
			cancellationToken);
	}

	/// <inheritdoc cref="FromAsync(CodeFixData{T}, CancellationToken)"/>
	public static Task<CodeFixExecutionContext<T>> FromAsync(CodeFixData<T> data)
	{
		return FromAsync(data, data.CancellationToken);
	}

	/// <summary>
	/// Creates a new <see cref="CodeFixExecutionContext{T}"/> from the specified <paramref name="data"/>.
	/// </summary>
	/// <param name="data"><see cref="CodeFixData{T}"/> to create the <see cref="CodeFixExecutionContext{T}"/> from.</param>
	/// <param name="cancellationToken"><see cref="System.Threading.CancellationToken"/> that specifies if the operation should be canceled.</param>
	/// <exception cref="InvalidOperationException">The <see cref="CodeFixData{T}.Success"/> property of <paramref name="data"/> returned <see langword="false"/>. -or- The <see cref="CodeFixData{T}.HasNode"/> property of <paramref name="data"/> returned <see langword="false"/>.</exception>
	public static async Task<CodeFixExecutionContext<T>> FromAsync(CodeFixData<T> data, CancellationToken cancellationToken)
	{
		if (!data.Success)
		{
			throw new InvalidOperationException($"The {nameof(CodeFixData<T>.Success)} property of '{nameof(data)}' returned false!");
		}

		if (!data.HasNode)
		{
			throw new InvalidOperationException($"The {nameof(CodeFixData<T>.HasNode)} property of '{nameof(data)}' returned false!");
		}

		if (data.HasSemanticModel)
		{
			return new CodeFixExecutionContext<T>(
				data.Diagnostic,
				data.Document,
				data.Root,
				data.Node,
				(CSharpCompilation)data.SemanticModel.Compilation,
				cancellationToken);
		}

		CSharpCompilation? compilation = (CSharpCompilation?)await data.Document.Project.GetCompilationAsync(cancellationToken).ConfigureAwait(false);

		return new CodeFixExecutionContext<T>(
			data.Diagnostic,
			data.Document,
			data.Root,
			data.Node,
			compilation!,
			cancellationToken);
	}

	/// <summary>
	/// Registers a change on the <see cref="Node"/> performed by the code fix.
	/// </summary>
	/// <param name="updated"><see cref="SyntaxNode"/> to replace the original <see cref="Node"/> with.</param>
	public void RegisterChange(T updated)
	{
		RegisterChange(Node, updated);
	}

	/// <summary>
	/// Registers a change performed by the code fix.
	/// </summary>
	/// <param name="original">Original <see cref="SyntaxNode"/> that is begin replaced by the <paramref name="updated"/> node.</param>
	/// <param name="updated"><see cref="SyntaxNode"/> to replace the <paramref name="original"/> node with.</param>
	public void RegisterChange(SyntaxNode original, SyntaxNode updated)
	{
		SyntaxTree old = Root.SyntaxTree;
		Root = Root.ReplaceNode(original, updated);
		Compilation = Compilation.ReplaceSyntaxTree(old, Root.SyntaxTree);
		Document = Document.WithSyntaxRoot(Root);
		Node = Root.GetAnnotatedNodes(_annotation).OfType<T>().First();

		if (_semanticModel is not null)
		{
			_semanticModel = null;
		}
	}

	/// <summary>
	/// Converts this context to a context with the specified <paramref name="node"/>.
	/// </summary>
	/// <typeparam name="TNode">Type of node.</typeparam>
	/// <param name="node">New target node of the context.</param>
	public CodeFixExecutionContext<TNode> WithNode<TNode>(TNode node) where TNode : SyntaxNode
	{
		return new CodeFixExecutionContext<TNode>(Diagnostic, Document, Root, node, Compilation, CancellationToken);
	}
}
