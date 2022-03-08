// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;

namespace Durian.Analysis.Data
{
    /// <summary>
    /// Basic implementation of the <see cref="ICompilationData"/> interface.
    /// </summary>
    public class CompilationData : ICompilationData
    {
        /// <inheritdoc/>
        public CSharpCompilation Compilation { get; private set; }

        /// <inheritdoc/>
        public virtual bool HasErrors { get; protected set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CompilationData"/> class.
        /// </summary>
        /// <param name="compilation">Current <see cref="CSharpCompilation"/>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="compilation"/> is <see langword="null"/>.</exception>
        public CompilationData(CSharpCompilation compilation)
        {
            if (compilation is null)
            {
                throw new ArgumentNullException(nameof(compilation));
            }

            Compilation = compilation;
            Reset();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CompilationData"/> class.
        /// </summary>
        /// <param name="compilation">Current <see cref="CSharpCompilation"/>.</param>
        /// <param name="reset">Determines whether to call <see cref="Reset"/>().</param>
        /// <exception cref="ArgumentNullException"><paramref name="compilation"/> is <see langword="null"/>.</exception>
        protected CompilationData(CSharpCompilation compilation, bool reset)
        {
            if(compilation is null)
            {
                throw new ArgumentNullException(nameof(compilation));
            }

            Compilation = compilation;

            if(reset)
            {
                Reset();
            }
        }

        /// <summary>
        /// Resets all collected <see cref="ISymbol"/>s.
        /// </summary>
        public virtual void Reset()
        {
            // Do nothing by default.
        }

        /// <inheritdoc/>
        public void UpdateCompilation(CSharpSyntaxTree? tree)
        {
            if (tree is not null)
            {
                CSharpCompilation compilation = Compilation;
                Compilation = Compilation.AddSyntaxTrees(tree);
                OnUpdate(compilation);
            }
        }

        /// <inheritdoc/>
        public void UpdateCompilation(CSharpSyntaxTree? original, CSharpSyntaxTree? updated)
        {
            if (original is null || updated is null)
            {
                return;
            }

            CSharpCompilation compilation = Compilation;
            Compilation = Compilation.ReplaceSyntaxTree(original, updated);
            OnUpdate(compilation);
        }

        /// <inheritdoc/>
        public void UpdateCompilation(IEnumerable<CSharpSyntaxTree>? trees)
        {
            if (trees is not null)
            {
                CSharpCompilation compilation = Compilation;
                Compilation = Compilation.AddSyntaxTrees(trees);
                OnUpdate(compilation);
            }
        }

        /// <inheritdoc/>
        public void UpdateCompilation(MetadataReference? reference)
        {
            if (reference is not null)
            {
                CSharpCompilation compilation = Compilation;
                Compilation = Compilation.AddReferences(reference);
                OnUpdate(compilation);
            }
        }

        /// <inheritdoc/>
        public void UpdateCompilation(MetadataReference? original, MetadataReference? updated)
        {
            if (original is null || updated is null)
            {
                return;
            }

            CSharpCompilation compilation = Compilation;
            Compilation = Compilation.ReplaceReference(original, updated);
            OnUpdate(compilation);
        }

        /// <inheritdoc/>
        public void UpdateCompilation(IEnumerable<MetadataReference>? references)
        {
            if (references is not null)
            {
                CSharpCompilation compilation = Compilation;
                Compilation = Compilation.AddReferences(references);
                OnUpdate(compilation);
            }
        }

        /// <summary>
        /// Returns a <see cref="INamedTypeSymbol"/> by the specified <paramref name="metadataName"/> and sets <see cref="HasErrors"/> to <see langword="true"/> if the <see cref="INamedTypeSymbol"/> could not be found.
        /// </summary>
        /// <param name="metadataName">Metadata name of <see cref="INamedTypeSymbol"/> to include.</param>
        protected INamedTypeSymbol? IncludeType(string metadataName)
        {
            if (Compilation.GetTypeByMetadataName(metadataName) is not INamedTypeSymbol t)
            {
                HasErrors = true;
                return default;
            }

            return t;
        }

        /// <summary>
        /// Methods executed when the <see cref="Compilation"/> has changed using one of the <c>Update...</c> methods.
        /// </summary>
        /// <param name="oldCompilation"><see cref="CSharpCompilation"/> that was updated.</param>
        protected virtual void OnUpdate(CSharpCompilation oldCompilation)
        {
            Reset();
        }
    }
}