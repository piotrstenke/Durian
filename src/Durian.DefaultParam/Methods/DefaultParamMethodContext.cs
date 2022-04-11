// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System.Threading;
using Durian.Analysis.Data;
using Durian.Analysis.Filters;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.Analysis.DefaultParam.Methods
{
	/// <summary>
	/// <see cref="ISyntaxValidationContext"/> used during analysis of methods marked with the <c>Durian.DefaultParamAttribute</c>.
	/// </summary>
	public readonly struct DefaultParamMethodContext : ISyntaxValidationContext
	{
		internal readonly TypeParameterContainer _typeParameters;

		/// <inheritdoc/>
		public CancellationToken CancellationToken { get; }

		/// <inheritdoc cref="ISyntaxValidationContext.TargetCompilation"/>
		public DefaultParamCompilationData TargetCompilation { get; }

		/// <inheritdoc cref="ISyntaxValidationContext.Node"/>
		public MethodDeclarationSyntax Node { get; }

		/// <inheritdoc/>
		public SemanticModel SemanticModel { get; }

		/// <inheritdoc cref="ISyntaxValidationContext.Symbol"/>
		public IMethodSymbol Symbol { get; }

		ICompilationData ISyntaxValidationContext.TargetCompilation => TargetCompilation;

		CSharpSyntaxNode ISyntaxValidationContext.Node => Node!;

		ISymbol ISyntaxValidationContext.Symbol => Symbol;

		/// <summary>
		/// Initializes a new instance of the <see cref="DefaultParamMethodContext"/> structure.
		/// </summary>
		/// <param name="compilation">Parent <see cref="DefaultParamCompilationData"/> of the target <paramref name="node"/>.</param>
		/// <param name="semanticModel"><see cref="Microsoft.CodeAnalysis.SemanticModel"/> of the <paramref name="node"/>.</param>
		/// <param name="symbol"><see cref="IMethodSymbol"/> that is represented by the <paramref name="node"/>.</param>
		/// <param name="typeParameters"><see cref="TypeParameterContainer"/> that contains the node's type parameters.</param>
		/// <param name="node"><see cref="MethodDeclarationSyntax"/> to validate.</param>
		/// <param name="cancellationToken"><see cref="System.Threading.CancellationToken"/> that specifies if the operation should be canceled.</param>
		public DefaultParamMethodContext(
			DefaultParamCompilationData compilation,
			SemanticModel semanticModel,
			IMethodSymbol symbol,
			in TypeParameterContainer typeParameters,
			MethodDeclarationSyntax node,
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
