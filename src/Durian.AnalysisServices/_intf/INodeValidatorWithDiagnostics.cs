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
    /// Provides methods for validating a <see cref="CSharpSyntaxNode"/> with an option to report appropriate diagnostics.
    /// </summary>
    /// <typeparam name="T">Type of <see cref="IMemberData"/> this <see cref="INodeValidator{T}"/> can return.</typeparam>
    public interface INodeValidatorWithDiagnostics<T> : INodeValidator<T> where T : IMemberData
    {
        /// <inheritdoc cref="ValidateAndCreate(CSharpSyntaxNode, ICompilationData, SemanticModel, ISymbol, out T?, IDiagnosticReceiver, CancellationToken)"/>
        bool ValidateAndCreate(
            CSharpSyntaxNode node,
            ICompilationData compilation,
            [NotNullWhen(true)] out T? data,
            IDiagnosticReceiver diagnosticReceiver,
            CancellationToken cancellationToken = default
        );

        /// <summary>
        /// Validates the specified <paramref name="node"/> and returns a new instance of <see cref="IMemberData"/> if the validation was a success.
        /// </summary>
        /// <param name="node"><see cref="CSharpSyntaxNode"/> to validate.</param>
        /// <param name="compilation">Parent <see cref="ICompilationData"/> of the target <paramref name="node"/>.</param>
        /// <param name="semanticModel"><see cref="SemanticModel"/> of the <paramref name="node"/>.</param>
        /// <param name="symbol"><see cref="ISymbol"/> that is represented by the <paramref name="node"/>.</param>
        /// <param name="data"><see cref="IMemberData"/> that is returned if the validation succeeds.</param>
        /// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> that is used to report <see cref="Diagnostic"/>s.</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
        /// <returns><see langword="true"/> if the validation was a success, <see langword="false"/> otherwise.</returns>
        bool ValidateAndCreate(
            CSharpSyntaxNode node,
            ICompilationData compilation,
            SemanticModel semanticModel,
            ISymbol symbol,
            [NotNullWhen(true)] out T? data,
            IDiagnosticReceiver diagnosticReceiver,
            CancellationToken cancellationToken = default
        );
    }
}