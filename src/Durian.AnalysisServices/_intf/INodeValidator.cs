// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Durian.Analysis.Data;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace Durian.Analysis
{
	/// <summary>
	/// Provides methods for validating a <see cref="CSharpSyntaxNode"/>.
	/// </summary>
	/// <typeparam name="T">Type of <see cref="IMemberData"/> this <see cref="INodeValidator{T}"/> can return.</typeparam>
	public interface INodeValidator<T> : IValidationDataProvider where T : IMemberData
	{
		/// <inheritdoc cref="INodeValidatorWithDiagnostics{T}.ValidateAndCreate(CSharpSyntaxNode, ICompilationData, SemanticModel, ISymbol, out T, IDiagnosticReceiver, CancellationToken)"/>
		bool ValidateAndCreate(
			CSharpSyntaxNode node,
			ICompilationData compilation,
			[NotNullWhen(true)] out T? data,
			CancellationToken cancellationToken = default
		);

		/// <inheritdoc cref="INodeValidatorWithDiagnostics{T}.ValidateAndCreate(CSharpSyntaxNode, ICompilationData, SemanticModel, ISymbol, out T, IDiagnosticReceiver, CancellationToken)"/>
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
