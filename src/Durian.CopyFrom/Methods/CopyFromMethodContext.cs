﻿using System.Threading;
using Durian.Analysis.Data;
using Durian.Analysis.Filtering;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.Analysis.CopyFrom.Methods;

/// <summary>
/// <see cref="ISyntaxValidationContext"/> used during analysis of methods marked with the <c>Durian.CopyFromMethodAttribute</c>.
/// </summary>
public readonly struct CopyFromMethodContext : ISyntaxValidationContext
{
	/// <inheritdoc/>
	public CancellationToken CancellationToken { get; }

	/// <inheritdoc cref="ISyntaxValidationContext.TargetCompilation"/>
	public CopyFromCompilationData Compilation { get; }

	/// <summary>
	/// <see cref="Node"/> as a <see cref="MethodDeclarationSyntax"/>.
	/// </summary>
	public MethodDeclarationSyntax? AsMethod => Node as MethodDeclarationSyntax;

	/// <inheritdoc cref="ISyntaxValidationContext.Node"/>
	public SyntaxNode? Node { get; }

	/// <inheritdoc/>
	public SemanticModel SemanticModel { get; }

	/// <inheritdoc cref="ISyntaxValidationContext.Symbol"/>
	public IMethodSymbol Symbol { get; }

	SyntaxNode ISyntaxValidationContext.Node => Node!;
	ISymbol ISyntaxValidationContext.Symbol => Symbol;
	ICompilationData ISyntaxValidationContext.TargetCompilation => Compilation;

	/// <summary>
	/// Initializes a new instance of the <see cref="CopyFromMethodContext"/> structure.
	/// </summary>
	/// <param name="compilation">Parent <see cref="CopyFromCompilationData"/> of the target <paramref name="node"/>.</param>
	/// <param name="semanticModel"><see cref="Microsoft.CodeAnalysis.SemanticModel"/> of the <paramref name="node"/>.</param>
	/// <param name="symbol"><see cref="IMethodSymbol"/> that is represented by the <paramref name="node"/>.</param>
	/// <param name="node"><see cref="SyntaxNode"/> to validate.</param>
	/// <param name="cancellationToken"><see cref="System.Threading.CancellationToken"/> that specifies if the operation should be canceled.</param>
	public CopyFromMethodContext(
		CopyFromCompilationData compilation,
		SemanticModel semanticModel,
		IMethodSymbol symbol,
		SyntaxNode? node = default,
		CancellationToken cancellationToken = default
	)
	{
		Compilation = compilation;
		SemanticModel = semanticModel;
		Node = node;
		Symbol = symbol;
		CancellationToken = cancellationToken;
	}

	/// <summary>
	/// Attempts to return a <see cref="CopyFromMethodContext"/> with the <see cref="Node"/> property set using the <see cref="SymbolExtensions.TryGetSyntax{T}(ISymbol, out T, CancellationToken)"/> extension method.
	/// </summary>
	/// <param name="context">Returned <see cref="CopyFromMethodContext"/>.</param>
	public bool TryInitNode(out CopyFromMethodContext context)
	{
		if (Node is not null)
		{
			context = this;
			return true;
		}

		if (Symbol.TryGetSyntax(out SyntaxNode? node, CancellationToken))
		{
			context = new(Compilation, SemanticModel, Symbol, node, CancellationToken);
			return true;
		}

		context = default;
		return false;
	}
}
