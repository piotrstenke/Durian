// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System.Threading;
using Durian.Analysis.Data;
using Durian.Analysis.Extensions;
using Durian.Analysis.Filters;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.Analysis.CopyFrom.Types
{
	/// <summary>
	/// <see cref="ISyntaxValidationContext"/> used during analysis of methods marked with the <c>Durian.CopyFromTypeAttribute</c>.
	/// </summary>
	public readonly struct CopyFromTypeContext : ISyntaxValidationContext
	{
		/// <inheritdoc/>
		public CancellationToken CancellationToken { get; }

		/// <inheritdoc cref="ISyntaxValidationContext.TargetCompilation"/>
		public CopyFromCompilationData Compilation { get; }

		/// <inheritdoc cref="ISyntaxValidationContext.Node"/>
		public TypeDeclarationSyntax? Node { get; }

		/// <inheritdoc/>
		public SemanticModel SemanticModel { get; }

		/// <inheritdoc cref="ISyntaxValidationContext.Symbol"/>
		public INamedTypeSymbol Symbol { get; }

		ICompilationData ISyntaxValidationContext.TargetCompilation => Compilation;

		CSharpSyntaxNode ISyntaxValidationContext.Node => Node!;

		ISymbol ISyntaxValidationContext.Symbol => Symbol;

		/// <summary>
		/// Initializes a new instance of the <see cref="CopyFromTypeContext"/> structure.
		/// </summary>
		/// <param name="compilation">Parent <see cref="CopyFromCompilationData"/> of the target <paramref name="node"/>.</param>
		/// <param name="semanticModel"><see cref="Microsoft.CodeAnalysis.SemanticModel"/> of the <paramref name="node"/>.</param>
		/// <param name="symbol"><see cref="INamedTypeSymbol"/> that is represented by the <paramref name="node"/>.</param>
		/// <param name="node"><see cref="TypeDeclarationSyntax"/> to validate.</param>
		/// <param name="cancellationToken"><see cref="System.Threading.CancellationToken"/> that specifies if the operation should be canceled.</param>
		public CopyFromTypeContext(
			CopyFromCompilationData compilation,
			SemanticModel semanticModel,
			INamedTypeSymbol symbol,
			TypeDeclarationSyntax? node = default,
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
		/// Attempts to return a <see cref="CopyFromTypeContext"/> with the <see cref="Node"/> property set using the <see cref="SymbolExtensions.TryGetSyntax{T}(ISymbol, out T, CancellationToken)"/> extension method.
		/// </summary>
		/// <param name="context">Returned <see cref="CopyFromTypeContext"/>.</param>
		public bool TryInitNode(out CopyFromTypeContext context)
		{
			if (Node is not null)
			{
				context = this;
				return true;
			}

			if (Symbol.TryGetSyntax(out TypeDeclarationSyntax? node, CancellationToken))
			{
				context = new(Compilation, SemanticModel, Symbol, node, CancellationToken);
				return true;
			}

			context = default;
			return false;
		}
	}
}
