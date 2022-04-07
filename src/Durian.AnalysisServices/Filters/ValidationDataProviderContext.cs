// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Durian.Analysis.Data;
using Microsoft.CodeAnalysis.CSharp;
using System.Threading;

namespace Durian.Analysis.Filters
{
	/// <summary>
	/// Data that is passed to the <see cref="IValidationDataProvider{T}.TryGetValidationData(in ValidationDataProviderContext, out T)"/> method.
	/// </summary>
	public readonly struct ValidationDataProviderContext
	{
		/// <summary>
		/// <see cref="CSharpSyntaxNode"/> to get the data of.
		/// </summary>
		public CSharpSyntaxNode Node { get; }

		/// <summary>
		/// Parent <see cref="ICompilationData"/> of the target <see cref="Node"/>.
		/// </summary>
		public ICompilationData TargetCompilation { get; }

		/// <summary>
		/// <see cref="System.Threading.CancellationToken"/> that specifies if the operation should be canceled.
		/// </summary>
		public CancellationToken CancellationToken { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="ValidationDataProviderContext"/> structure.
		/// </summary>
		/// <param name="node"><see cref="CSharpSyntaxNode"/> to get the data of.</param>
		/// <param name="compilation">Parent <see cref="ICompilationData"/> of the target <paramref name="node"/>.</param>
		/// <param name="cancellationToken"><see cref="System.Threading.CancellationToken"/> that specifies if the operation should be canceled.</param>
		public ValidationDataProviderContext(CSharpSyntaxNode node, ICompilationData compilation, CancellationToken cancellationToken = default)
		{
			Node = node;
			TargetCompilation = compilation;
			CancellationToken = cancellationToken;
		}
	}
}
