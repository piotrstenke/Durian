﻿// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System.Threading;
using Durian.Analysis.Data;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Durian.Analysis.Filters
{
	/// <summary>
	/// Defines data that is passed to the <see cref="ISyntaxValidator{T}.ValidateAndCreate(in T, out IMemberData?)"/> method.
	/// </summary>
	public readonly struct SyntaxValidationContext : ISyntaxValidationContext
	{
		/// <inheritdoc/>
		public CancellationToken CancellationToken { get; }

		/// <inheritdoc/>
		public CSharpSyntaxNode Node { get; }

		/// <inheritdoc/>
		public SemanticModel SemanticModel { get; }

		/// <inheritdoc/>
		public ISymbol Symbol { get; }

		/// <inheritdoc/>
		public ICompilationData TargetCompilation { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="SyntaxValidationContext"/> structure.
		/// </summary>
		/// <param name="compilation">Parent <see cref="ICompilationData"/> of the target <paramref name="node"/>.</param>
		/// <param name="semanticModel"><see cref="Microsoft.CodeAnalysis.SemanticModel"/> of the <paramref name="node"/>.</param>
		/// <param name="symbol"><see cref="ISymbol"/> that is represented by the <paramref name="node"/>.</param>
		/// <param name="node"><see cref="CSharpSyntaxNode"/> to validate.</param>
		/// <param name="cancellationToken"><see cref="System.Threading.CancellationToken"/> that specifies if the operation should be canceled.</param>
		public SyntaxValidationContext(
			ICompilationData compilation,
			SemanticModel semanticModel,
			ISymbol symbol,
			CSharpSyntaxNode node,
			CancellationToken cancellationToken = default
		)
		{
			TargetCompilation = compilation;
			SemanticModel = semanticModel;
			Node = node;
			Symbol = symbol;
			CancellationToken = cancellationToken;
		}

		/// <summary>
		/// Converts the current <see cref="SyntaxValidationContext"/> to a <see cref="ValidationDataContext"/>.
		/// </summary>
		public ValidationDataContext ToValidationContext()
		{
			return new(Node, TargetCompilation, CancellationToken);
		}
	}
}