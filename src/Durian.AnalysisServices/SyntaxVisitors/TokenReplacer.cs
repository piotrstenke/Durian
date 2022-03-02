// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Durian.Analysis.SyntaxVisitors
{
    /// <summary>
    /// Replaces <see cref="SyntaxToken"/>s of the specified type with the provided <see cref="Replacement"/>.
    /// </summary>
    public abstract class TokenReplacer : CSharpSyntaxRewriter
    {
        /// <summary>
        /// <see cref="SyntaxToken"/> that is the replacement.
        /// </summary>
        public SyntaxToken Replacement { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TokenReplacer"/> class.
        /// </summary>
        protected TokenReplacer()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TokenReplacer"/> class.
        /// </summary>
        /// <param name="replacement"><see cref="SyntaxToken"/> that is the replacement.</param>
        protected TokenReplacer(in SyntaxToken replacement)
        {
            Replacement = replacement;
        }
    }
}