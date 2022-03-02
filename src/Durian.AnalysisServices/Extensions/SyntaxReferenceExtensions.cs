// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Microsoft.CodeAnalysis;
using System;

namespace Durian.Analysis.Extensions
{
    /// <summary>
    /// Contains various extension methods for <see cref="SyntaxReference"/> classes.
    /// </summary>
    public static class SyntaxReferenceExtensions
    {
        /// <summary>
        /// Checks if the specified <paramref name="reference"/> points to the given <paramref name="node"/>.
        /// </summary>
        /// <param name="reference"><see cref="SyntaxReference"/> to check if points to the given <paramref name="node"/>.</param>
        /// <param name="node"><see cref="SyntaxNode"/> to check.</param>
        /// <exception cref="ArgumentNullException"><paramref name="reference"/> is <see langword="null"/>. -or- <paramref name="node"/> is <see langword="null"/>.</exception>
        public static bool HasReference(this SyntaxReference reference, SyntaxNode node)
        {
            if (reference is null)
            {
                throw new ArgumentNullException(nameof(reference));
            }

            if (node is null)
            {
                throw new ArgumentNullException(nameof(node));
            }

            return reference.SyntaxTree == node.SyntaxTree && reference.Span.Contains(node.Span);
        }

        /// <summary>
        /// Checks if the specified <paramref name="reference"/> points to the given <paramref name="symbol"/>.
        /// </summary>
        /// <param name="reference"><see cref="SyntaxReference"/> to check if points to the given <paramref name="symbol"/>.</param>
        /// <param name="symbol"><see cref="ISymbol"/> to check.</param>
        /// <exception cref="ArgumentNullException"><paramref name="reference"/> is <see langword="null"/>. -or- <paramref name="symbol"/> is <see langword="null"/>.</exception>
        public static bool HasReference(this SyntaxReference reference, ISymbol symbol)
        {
            if (reference is null)
            {
                throw new ArgumentNullException(nameof(reference));
            }

            if (symbol is null)
            {
                throw new ArgumentNullException(nameof(symbol));
            }

            foreach (SyntaxReference r in symbol.DeclaringSyntaxReferences)
            {
                if (r == reference || (reference.SyntaxTree == r.SyntaxTree && reference.Span.Contains(r.Span)))
                {
                    return true;
                }
            }

            return false;
        }
    }
}