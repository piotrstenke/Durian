// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Durian.Analysis.Data;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Threading;

namespace Durian.Analysis.Filters
{
	/// <summary>
	/// Data that is passed to the <see cref="IValidationDataProvider.GetValidationData"/> method.
	/// </summary>
	public struct ValidationDataProviderContext
	{
		/// <summary>
		/// <see cref="CSharpSyntaxNode"/> to get the data of.
		/// </summary>
		public CSharpSyntaxNode Node { get; }

		/// <summary>
		/// Parent <see cref="ICompilationData"/> of the target <see cref="Node"/>.
		/// </summary>
		public ICompilationData Compilation { get; }

		/// <summary>
		/// <see cref="System.Threading.CancellationToken"/> that specifies if the operation should be canceled.
		/// </summary>
		public CancellationToken CancellationToken { get; }

		internal SemanticModel? SemanticModel { get; set; }

		internal ISymbol? Symbol { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="ValidationDataProviderContext"/> structure.
		/// </summary>
		/// <param name="node"><see cref="CSharpSyntaxNode"/> to get the data of.</param>
		/// <param name="compilation">Parent <see cref="ICompilationData"/> of the target <paramref name="node"/>.</param>
		/// <param name="cancellationToken"><see cref="System.Threading.CancellationToken"/> that specifies if the operation should be canceled.</param>
		public ValidationDataProviderContext(CSharpSyntaxNode node, ICompilationData compilation, CancellationToken cancellationToken = default)
		{
			Node = node;
			Compilation = compilation;
			CancellationToken = cancellationToken;
			SemanticModel = default;
			Symbol = default;
		}

		/// <summary>
		/// Sets the specified <paramref name="semanticModel"/> as the target <see cref="Microsoft.CodeAnalysis.SemanticModel"/>.
		/// </summary>
		/// <param name="semanticModel"><see cref="Microsoft.CodeAnalysis.SemanticModel"/> to set as the target <see cref="Microsoft.CodeAnalysis.SemanticModel"/>.</param>
		public void SetSemanticModel(SemanticModel semanticModel)
		{
			SemanticModel = semanticModel;
		}

		/// <summary>
		/// Sets the specified <paramref name="symbol"/> as the target <see cref="ISymbol"/>.
		/// </summary>
		/// <param name="symbol"><see cref="ISymbol"/> to set as the target <see cref="ISymbol"/>.</param>
		public void SetSymbol(ISymbol symbol)
		{
			Symbol = symbol;
		}
	}
}
