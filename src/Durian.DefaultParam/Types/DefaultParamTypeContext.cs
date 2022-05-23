// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System.Threading;
using Durian.Analysis.Data;
using Durian.Analysis.Filters;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.Analysis.DefaultParam.Types
{
	/// <summary>
	/// <see cref="ISyntaxValidationContext"/> used during analysis of types marked with the <c>Durian.DefaultParamAttribute</c>.
	/// </summary>
	public readonly struct DefaultParamTypeContext : ISyntaxValidationContext
	{
		internal readonly TypeParameterContainer _typeParameters;

		/// <inheritdoc/>
		public CancellationToken CancellationToken { get; }

		/// <inheritdoc cref="ISyntaxValidationContext.Node"/>
		public TypeDeclarationSyntax Node { get; }

		/// <inheritdoc/>
		public SemanticModel SemanticModel { get; }

		/// <inheritdoc cref="ISyntaxValidationContext.Symbol"/>
		public INamedTypeSymbol Symbol { get; }

		/// <inheritdoc cref="ISyntaxValidationContext.TargetCompilation"/>
		public DefaultParamCompilationData TargetCompilation { get; }

		CSharpSyntaxNode ISyntaxValidationContext.Node => Node!;
		ISymbol ISyntaxValidationContext.Symbol => Symbol;
		ICompilationData ISyntaxValidationContext.TargetCompilation => TargetCompilation;

		/// <summary>
		/// Initializes a new instance of the <see cref="DefaultParamTypeContext"/> structure.
		/// </summary>
		/// <param name="compilation">Parent <see cref="DefaultParamCompilationData"/> of the target <paramref name="node"/>.</param>
		/// <param name="semanticModel"><see cref="Microsoft.CodeAnalysis.SemanticModel"/> of the <paramref name="node"/>.</param>
		/// <param name="symbol"><see cref="INamedTypeSymbol"/> that is represented by the <paramref name="node"/>.</param>
		/// <param name="typeParameters"><see cref="TypeParameterContainer"/> that contains the node's type parameters.</param>
		/// <param name="node"><see cref="TypeDeclarationSyntax"/> to validate.</param>
		/// <param name="cancellationToken"><see cref="System.Threading.CancellationToken"/> that specifies if the operation should be canceled.</param>
		public DefaultParamTypeContext(
			DefaultParamCompilationData compilation,
			SemanticModel semanticModel,
			INamedTypeSymbol symbol,
			in TypeParameterContainer typeParameters,
			TypeDeclarationSyntax node,
			CancellationToken cancellationToken = default
		)
		{
			TargetCompilation = compilation;
			SemanticModel = semanticModel;
			Node = node;
			Symbol = symbol;
			CancellationToken = cancellationToken;
			_typeParameters = typeParameters;
		}
	}
}
