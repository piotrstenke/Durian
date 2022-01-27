// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Durian.Analysis.Data;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Durian.Analysis
{
	/// <summary>
	/// Provides methods for validating a <see cref="CSharpSyntaxNode"/>.
	/// </summary>
	/// <typeparam name="T">Type of <see cref="IMemberData"/> this <see cref="INodeValidator{T}"/> can return.</typeparam>
	public interface INodeValidator<T> where T : IMemberData
	{
		/// <summary>
		/// Checks whether a <see cref="SemanticModel"/> and a <see cref="ISymbol"/> can be created from the given <paramref name="node"/>. If so, returns them.
		/// If so, returns them.
		/// </summary>
		/// <param name="node"><see cref="CSharpSyntaxNode"/> to get the data of.</param>
		/// <param name="compilation">Parent <see cref="ICompilationData"/> of the target <paramref name="node"/>.</param>
		/// <param name="semanticModel"><see cref="SemanticModel"/> of the <paramref name="node"/>.</param>
		/// <param name="symbol"><see cref="ISymbol"/> created for the <paramref name="node"/>.</param>
		/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
		/// <returns><see langword="true"/> if a <see cref="SemanticModel"/> and a <see cref="ISymbol"/> that be created from the given <paramref name="node"/>, <see langword="false"/> otherwise.</returns>
		bool GetValidationData(
			CSharpSyntaxNode node,
			ICompilationData compilation,
			[NotNullWhen(true)] out SemanticModel? semanticModel,
			[NotNullWhen(true)] out ISymbol? symbol,
			CancellationToken cancellationToken = default
		);

		/// <inheritdoc cref="INodeValidatorWithDiagnostics{T}.ValidateAndCreateWithDiagnostics(IDiagnosticReceiver, CSharpSyntaxNode, ICompilationData, SemanticModel, ISymbol, out T, CancellationToken)"/>
		bool ValidateAndCreate(
			CSharpSyntaxNode node,
			ICompilationData compilation,
			[NotNullWhen(true)] out T? data,
			CancellationToken cancellationToken = default
		);

		/// <inheritdoc cref="INodeValidatorWithDiagnostics{T}.ValidateAndCreateWithDiagnostics(IDiagnosticReceiver, CSharpSyntaxNode, ICompilationData, SemanticModel, ISymbol, out T, CancellationToken)"/>
		bool ValidateAndCreate(
			CSharpSyntaxNode node,
			ICompilationData compilation,
			SemanticModel semanticModel,
			ISymbol symbol,
			[NotNullWhen(true)] out T? data,
			CancellationToken cancellationToken = default
		);
	}
}