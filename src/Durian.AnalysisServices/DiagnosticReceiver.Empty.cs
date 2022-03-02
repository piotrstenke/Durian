// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Durian.Analysis
{
    public sealed partial class DiagnosticReceiver
    {
        /// <summary>
        /// A <see cref="IDirectDiagnosticReceiver"/> that does not actually report any diagnostics.
        /// </summary>
        public sealed class Empty : INodeDiagnosticReceiver
        {
            /// <inheritdoc/>
            public int Count => 0;

            /// <summary>
            /// Initializes a new instance of the <see cref="Empty"/> class.
            /// </summary>
            public Empty()
            {
            }

            /// <inheritdoc/>
            public void Clear()
            {
                // Do nothing.
            }

            /// <inheritdoc/>
            public void Push()
            {
                // Do nothing.
            }

            /// <inheritdoc/>
            public void ReportDiagnostic(DiagnosticDescriptor descriptor, Location? location, params object?[]? messageArgs)
            {
                // Do nothing
            }

            /// <inheritdoc/>
            public void ReportDiagnostic(Diagnostic diagnostic)
            {
                // Do nothing
            }

            /// <inheritdoc/>
            public void SetTargetNode(CSharpSyntaxNode node, string hintName)
            {
                // Do nothing.
            }
        }
    }
}