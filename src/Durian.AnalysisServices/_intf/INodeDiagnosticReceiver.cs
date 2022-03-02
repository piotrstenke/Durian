// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;

namespace Durian.Analysis
{
    /// <summary>
    /// <see cref="IDiagnosticReceiver"/> that reports <see cref="Diagnostic"/>s for a specified <see cref="CSharpSyntaxNode"/>.
    /// </summary>
    public interface INodeDiagnosticReceiver : IDirectDiagnosticReceiver, IDiagnosticReceiverWithBuffer
    {
        /// <summary>
        /// Sets the <see cref="CSharpSyntaxNode"/> that the diagnostics will be reported for.
        /// </summary>
        /// <param name="node"><see cref="CSharpSyntaxNode"/> to set as the target.</param>
        /// <param name="hintName">Name that is used to identify the <paramref name="node"/>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="node"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="hintName"/> is <see langword="null"/> or empty.</exception>
        void SetTargetNode(CSharpSyntaxNode node, string hintName);
    }
}