// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Durian.Analysis
{
    /// <summary>
    /// <see cref="ISyntaxReceiver"/> that provides access to all collected nodes.
    /// </summary>
    public interface IDurianSyntaxReceiver : ISyntaxReceiver, INodeProvider
    {
        /// <summary>
        /// Determines whether the <see cref="ISyntaxReceiver"/> is empty, i.e. it didn't collect any <see cref="CSharpSyntaxNode"/>s.
        /// </summary>
        bool IsEmpty();
    }
}