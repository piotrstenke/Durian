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
    /// Provides a method for retrieving data required for syntax validation, like <see cref="SemanticModel"/> or <see cref="ISymbol"/>.
    /// </summary>
    public interface IValidationDataProvider
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
    }
}