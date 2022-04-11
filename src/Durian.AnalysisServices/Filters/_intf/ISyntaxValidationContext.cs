// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Durian.Analysis.Data;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Threading;

namespace Durian.Analysis.Filters
{
	/// <summary>
	/// Defines data that is passed to the <see cref="ISyntaxValidator{T}.ValidateAndCreate(in T, out IMemberData?)"/> method.
	/// </summary>
	public interface ISyntaxValidationContext
	{
		/// <summary>
		/// <see cref="System.Threading.CancellationToken"/> that specifies if the operation should be canceled.
		/// </summary>
		CancellationToken CancellationToken { get; }

		/// <summary>
		/// Parent <see cref="ICompilationData"/> of the target <see cref="Node"/>.
		/// </summary>
		ICompilationData TargetCompilation { get; }

		/// <summary>
		/// <see cref="CSharpSyntaxNode"/> to validate.
		/// </summary>
		CSharpSyntaxNode Node { get; }

		/// <summary>
		/// <see cref="Microsoft.CodeAnalysis.SemanticModel"/> of the <see cref="Node"/>.
		/// </summary>
		SemanticModel SemanticModel { get; }

		/// <summary>
		/// <see cref="ISymbol"/> that is represented by the <see cref="Node"/>.
		/// </summary>
		ISymbol Symbol { get; }
	}
}
