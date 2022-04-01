// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Durian.Analysis.Data;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Threading;

namespace Durian.Analysis.Filters
{
	/// <summary>
	/// /<see cref="ISyntaxValidatorContext"/> that also provides a <see cref="IDiagnosticReceiver"/> to report appropriate <see cref="Diagnostic"/>s.
	/// </summary>
	public readonly struct SyntaxValidatorContextWithDiagnostics : ISyntaxValidatorContextWithDiagnostics
	{
		/// <inheritdoc/>
		public CancellationToken CancellationToken { get; }

		/// <inheritdoc/>
		public ICompilationData Compilation { get; }

		/// <inheritdoc/>
		public IDiagnosticReceiver DiagnosticReceiver { get; }

		/// <inheritdoc/>
		public CSharpSyntaxNode Node { get; }

		/// <inheritdoc/>
		public SemanticModel SemanticModel { get; }

		/// <inheritdoc/>
		public ISymbol Symbol { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="SyntaxValidatorContext"/> structure.
		/// </summary>
		/// <param name="compilation">Parent <see cref="ICompilationData"/> of the target <paramref name="node"/>.</param>
		/// <param name="semanticModel"><see cref="Microsoft.CodeAnalysis.SemanticModel"/> of the <paramref name="node"/>.</param>
		/// <param name="node"><see cref="CSharpSyntaxNode"/> to validate.</param>
		/// <param name="symbol"><see cref="ISymbol"/> that is represented by the <paramref name="node"/>.</param>
		/// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> that is used to report <see cref="Diagnostic"/>s.</param>
		/// <param name="cancellationToken"><see cref="System.Threading.CancellationToken"/> that specifies if the operation should be canceled.</param>
		public SyntaxValidatorContextWithDiagnostics(
			ICompilationData compilation,
			SemanticModel semanticModel,
			CSharpSyntaxNode node,
			ISymbol symbol,
			IDiagnosticReceiver diagnosticReceiver,
			CancellationToken cancellationToken = default
		)
		{
			Compilation = compilation;
			SemanticModel = semanticModel;
			Node = node;
			Symbol = symbol;
			DiagnosticReceiver = diagnosticReceiver;
			CancellationToken = cancellationToken;
		}
	}
}
