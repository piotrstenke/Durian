// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Durian.Analysis.Data;
using Durian.Analysis.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Durian.TestServices
{
    /// <summary>
    /// A <see cref="ICompilationData"/> that is unit-test friendly and can be used without worries of encountering an exception.
    /// </summary>
    public sealed class TestableCompilationData : ICompilationData
    {
        private CSharpCompilation? _currentCompilation;

        private CSharpCompilation? _originalCompilation;

        /// <summary>
        /// A <see cref="CSharpCompilation"/> that is affected by the <see cref="UpdateCompilation(CSharpSyntaxTree)"/> method or its overloads.
        /// </summary>
        public CSharpCompilation CurrentCompilation => _currentCompilation!;

        /// <inheritdoc/>
        public bool HasErrors { get; set; }

        /// <summary>
        /// Original <see cref="CSharpCompilation"/> that is not affected by the <see cref="UpdateCompilation(CSharpSyntaxTree)"/> method or its overloads.
        /// </summary>
        /// <exception cref="ArgumentNullException">Value cannot be <see langword="null"/>.</exception>
        public CSharpCompilation OriginalCompilation
        {
            get => _originalCompilation!;
            set
            {
                if (value is null)
                {
                    throw new ArgumentNullException(nameof(OriginalCompilation));
                }

                if (_originalCompilation != value)
                {
                    _originalCompilation = value;
                    _currentCompilation = value;
                }
            }
        }

        CSharpCompilation ICompilationData.Compilation => _currentCompilation!;

        private TestableCompilationData(CSharpCompilation? compilation)
        {
            _originalCompilation = compilation;
            _currentCompilation = compilation;
        }

        /// <summary>
        /// Creates a new <see cref="TestableCompilationData"/>.
        /// </summary>
        /// <param name="includeDefaultAssemblies">Determines whether to include all the default assemblies returned by the <see cref="RoslynUtilities.GetBaseReferences(bool)"/> method.</param>
        public static TestableCompilationData Create(bool includeDefaultAssemblies = true)
        {
            return new TestableCompilationData(includeDefaultAssemblies ? RoslynUtilities.CreateBaseCompilation() : null);
        }

        /// <summary>
        /// Creates a new <see cref="TestableCompilationData"/>.
        /// </summary>
        /// <param name="sources">A collection of <see cref="string"/>s that will be parsed as <see cref="CSharpSyntaxTree"/>s and added to the newly-created <see cref="TestableCompilationData"/>.</param>
        /// <param name="assemblies">A collection of <see cref="Assembly"/> instances to be referenced by the newly-created <see cref="TestableCompilationData"/>.</param>
        public static TestableCompilationData Create(IEnumerable<string>? sources, IEnumerable<Assembly>? assemblies)
        {
            return new TestableCompilationData(RoslynUtilities.CreateCompilationWithAssemblies(sources, assemblies?.ToArray()));
        }

        /// <summary>
        /// Creates a new <see cref="TestableCompilationData"/>.
        /// </summary>
        /// <param name="sources">A collection of <see cref="string"/>s that will be parsed as <see cref="CSharpSyntaxTree"/>s and added to the newly-created <see cref="TestableCompilationData"/>.</param>
        public static TestableCompilationData Create(IEnumerable<string>? sources)
        {
            return new TestableCompilationData(RoslynUtilities.CreateCompilationWithAssemblies(sources));
        }

        /// <summary>
        /// Creates a new <see cref="TestableCompilationData"/>.
        /// </summary>
        /// <param name="source">A <see cref="string"/> that will be parsed as a <see cref="CSharpSyntaxTree"/> and added to the newly-created <see cref="TestableCompilationData"/>.</param>
        public static TestableCompilationData Create(string? source)
        {
            return new TestableCompilationData(RoslynUtilities.CreateCompilationWithAssemblies(source));
        }

        /// <summary>
        /// Creates a new <see cref="TestableCompilationData"/>.
        /// </summary>
        /// <param name="source">A <see cref="string"/> that will be parsed as a <see cref="CSharpSyntaxTree"/> and added to the newly-created <see cref="TestableCompilationData"/>.</param>
        /// <param name="assemblies">A collection of <see cref="Assembly"/> instances to be referenced by the newly-created <see cref="TestableCompilationData"/>.</param>
        public static TestableCompilationData Create(string? source, IEnumerable<Assembly>? assemblies)
        {
            return new TestableCompilationData(RoslynUtilities.CreateCompilationWithAssemblies(source, assemblies?.ToArray()));
        }

        /// <summary>
        /// Creates a new <see cref="TestableCompilationData"/>.
        /// </summary>
        /// <param name="compilation">A <see cref="CSharpCompilation"/> to be used as the base compilation of the newly-created <see cref="TestableCompilationData"/>.</param>
        public static TestableCompilationData Create(CSharpCompilation? compilation)
        {
            return new TestableCompilationData(compilation);
        }

        /// <summary>
        /// Creates a new <see cref="IMemberData"/> from the specified <paramref name="source"/>.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="index">Index at which the <see cref="IMemberData"/> should be returned. Can be thought of as a number of <see cref="CSharpSyntaxNode"/> of type <typeparamref name="TNode"/> to skip before creating a valid <see cref="IMemberData"/>.</param>
        /// <typeparam name="TNode">Type of <see cref="CSharpSyntaxNode"/> to find and convert to a <see cref="IMemberData"/>.</typeparam>
        /// <returns>
        /// A new <see cref="IMemberData"/> created from a <see cref="CSharpSyntaxTree"/> of type <typeparamref name="TNode"/> found at the specified index in the parsed <see cref="CSharpSyntaxTree"/> -or-
        /// <see langword="null"/> if no such <see cref="CSharpSyntaxNode"/> exists.
        /// </returns>
        public IMemberData? GetMemberData<TNode>(string? source, int index = 0) where TNode : MemberDeclarationSyntax
        {
            return GetNode<TNode>(source, index)?.GetMemberData(this);
        }

        /// <summary>
        /// Parses a <see cref="CSharpSyntaxTree"/> from the specified <paramref name="source"/> and returns the <see cref="CSharpSyntaxNode"/> of type <typeparamref name="TNode"/> at the specified index in that tree.
        /// </summary>
        /// <param name="source">A <see cref="string"/> to be parsed and converted to a <see cref="CSharpSyntaxTree"/>.</param>
        /// <param name="index">Index at which the <see cref="CSharpSyntaxNode"/> should be returned. Can be thought of as a number of <see cref="CSharpSyntaxNode"/> of type <typeparamref name="TNode"/> to skip.</param>
        /// <typeparam name="TNode">Type of <see cref="CSharpSyntaxNode"/> to find and return.</typeparam>
        /// <returns>
        /// The <see cref="CSharpSyntaxNode"/> of type <typeparamref name="TNode"/> found at the specified index in the parsed <see cref="CSharpSyntaxTree"/> -or-
        /// <see langword="null"/> if no such <see cref="CSharpSyntaxNode"/> exists.
        /// </returns>
        public TNode? GetNode<TNode>(string? source, int index = 0) where TNode : CSharpSyntaxNode
        {
            if (source is null)
            {
                return null;
            }

            return GetNode<TNode>(CSharpSyntaxTree.ParseText(source, encoding: Encoding.UTF8) as CSharpSyntaxTree, index);
        }

        /// <summary>
        /// Returns the <see cref="CSharpSyntaxNode"/> of type <typeparamref name="TNode"/> at the specified index in the <paramref name="syntaxTree"/>.
        /// </summary>
        /// <param name="syntaxTree">A <see cref="CSharpSyntaxTree"/> to get the <see cref="CSharpSyntaxNode"/> of type <typeparamref name="TNode"/> from.</param>
        /// <param name="index">Index at which the <see cref="CSharpSyntaxNode"/> should be returned. Can be thought of as a number of <see cref="CSharpSyntaxNode"/> of type <typeparamref name="TNode"/> to skip.</param>
        /// <typeparam name="TNode">Type of <see cref="CSharpSyntaxNode"/> to find and return.</typeparam>
        /// <returns>
        /// The <see cref="CSharpSyntaxNode"/> of type <typeparamref name="TNode"/> found at the specified index in the specified <paramref name="syntaxTree"/> -or-
        /// <see langword="null"/> if no such <see cref="CSharpSyntaxNode"/> exists.
        /// </returns>
        public TNode? GetNode<TNode>(CSharpSyntaxTree? syntaxTree, int index = 0) where TNode : CSharpSyntaxNode
        {
            TNode? node = RoslynUtilities.ParseNode<TNode>(syntaxTree, index);

            if (node is null)
            {
                return null;
            }

            UpdateCompilation(syntaxTree);
            return node;
        }

        /// <summary>
        /// Retrieves a <see cref="ISymbol"/> of type <typeparamref name="TSymbol"/> from the specified <paramref name="node"/>.
        /// </summary>
        /// <param name="node"><see cref="CSharpSyntaxNode"/> to retrieve the <see cref="ISymbol"/> from.</param>
        /// <typeparam name="TSymbol">Type of <see cref="ISymbol"/> to retrieve from the specified <paramref name="node"/>.</typeparam>
        /// <returns>
        /// An <see cref="ISymbol"/> of type <typeparamref name="TSymbol"/> retrieved from the target <paramref name="node"/> -or-
        /// <see langword="null"/> if no such <see cref="CSharpSyntaxNode"/> exists.
        /// </returns>
        public TSymbol? GetSymbol<TSymbol>(CSharpSyntaxNode? node) where TSymbol : class, ISymbol
        {
            if (node is null)
            {
                return null;
            }

            if (_currentCompilation is null)
            {
                return null;
            }

            SemanticModel semanticModel = _currentCompilation.GetSemanticModel(node.SyntaxTree);
            ISymbol? symbol = semanticModel.GetDeclaredSymbol(node);

            if (symbol is not null)
            {
                return symbol as TSymbol;
            }

            symbol = semanticModel.GetSymbolInfo(node).Symbol;

            if (symbol is not null)
            {
                return symbol as TSymbol;
            }

            return semanticModel.GetTypeInfo(node).Type as TSymbol;
        }

        /// <summary>
        /// Retrieves a <see cref="ISymbol"/> of type <typeparamref name="TSymbol"/> from the specified <paramref name="syntaxTree"/>.
        /// </summary>
        /// <param name="syntaxTree"><see cref="CSharpSyntaxTree"/> to retrieve the <see cref="ISymbol"/> from.</param>
        /// <param name="index">Index at which the <see cref="ISymbol"/> should be returned. Can be thought of as a number of <see cref="CSharpSyntaxNode"/> of type <typeparamref name="TNode"/> to skip before retrieving a valid <see cref="ISymbol"/>.</param>
        /// <typeparam name="TSymbol">Type of <see cref="ISymbol"/> to retrieve from the specified <paramref name="syntaxTree"/>.</typeparam>
        /// <typeparam name="TNode">Type of <see cref="CSharpSyntaxNode"/> to find and convert to a <see cref="ISymbol"/> of type <typeparamref name="TSymbol"/>.</typeparam>
        /// <returns>
        /// An <see cref="ISymbol"/> of type <typeparamref name="TSymbol"/> retrieved from the target <paramref name="syntaxTree"/> -or-
        /// <see langword="null"/> if no such <see cref="CSharpSyntaxNode"/> exists.
        /// </returns>
        public TSymbol? GetSymbol<TSymbol, TNode>(CSharpSyntaxTree? syntaxTree, int index = 0) where TSymbol : class, ISymbol where TNode : CSharpSyntaxNode
        {
            return GetSymbol<TSymbol>(GetNode<TNode>(syntaxTree, index));
        }

        /// <summary>
        /// Retrieves a <see cref="ISymbol"/> of type <typeparamref name="TSymbol"/> from the specified <paramref name="source"/>.
        /// </summary>
        /// <param name="source">A <see cref="string"/> to be parsed and converted to a <see cref="CSharpSyntaxTree"/>.</param>
        /// <param name="index">Index at which the <see cref="ISymbol"/> should be returned. Can be thought of as a number of <see cref="CSharpSyntaxNode"/> of type <typeparamref name="TNode"/> to skip before retrieving a valid <see cref="ISymbol"/>.</param>
        /// <typeparam name="TSymbol">Type of <see cref="ISymbol"/> to retrieve from the specified <paramref name="source"/>.</typeparam>
        /// <typeparam name="TNode">Type of <see cref="CSharpSyntaxNode"/> to find and convert to a <see cref="ISymbol"/> of type <typeparamref name="TSymbol"/>.</typeparam>
        /// <returns>
        /// An <see cref="ISymbol"/> of type <typeparamref name="TSymbol"/> retrieved from a <see cref="CSharpSyntaxTree"/> of type <typeparamref name="TNode"/> found at the specified index in the parsed <see cref="CSharpSyntaxTree"/> -or-
        /// <see langword="null"/> if no such <see cref="CSharpSyntaxNode"/> exists.
        /// </returns>
        public TSymbol? GetSymbol<TSymbol, TNode>(string? source, int index = 0) where TSymbol : class, ISymbol where TNode : CSharpSyntaxNode
        {
            return GetSymbol<TSymbol>(GetNode<TNode>(source, index));
        }

        /// <summary>
        /// Resets the <see cref="TestableCompilationData"/>, so that the <see cref="CurrentCompilation"/> now references the <see cref="OriginalCompilation"/>.
        /// </summary>
        public void Reset()
        {
            _currentCompilation = _originalCompilation;
        }

        /// <summary>
        /// Parses the <paramref name="source"/> <see cref="string"/> and adds the created <see cref="CSharpSyntaxTree"/> to the <see cref="Compilation"/>.
        /// </summary>
        /// <param name="source"><see cref="string"/> to parse and add.</param>
        public void UpdateCompilation(string? source)
        {
            if (source is null)
            {
                return;
            }

            UpdateCompilation(CSharpSyntaxTree.ParseText(source, encoding: Encoding.UTF8) as CSharpSyntaxTree);
        }

        /// <summary>
        /// Adds the <paramref name="tree"/> to the <see cref="Compilation"/>.
        /// </summary>
        /// <param name="tree"><see cref="CSharpSyntaxTree"/> to add.</param>
        public void UpdateCompilation(CSharpSyntaxTree? tree)
        {
            if (tree is null)
            {
                return;
            }

            _currentCompilation = _currentCompilation?.AddSyntaxTrees(tree);
        }

        /// <summary>
        /// Replaces the <paramref name="original"/> <see cref="SyntaxTree"/> with the <paramref name="updated"/> one.
        /// </summary>
        /// <param name="original"><see cref="CSharpSyntaxTree"/> to replace.</param>
        /// <param name="updated"><see cref="CSharpSyntaxTree"/> to replace the <paramref name="original"/> by.</param>
        public void UpdateCompilation(CSharpSyntaxTree? original, CSharpSyntaxTree? updated)
        {
            if (original is null || updated is null)
            {
                return;
            }

            _currentCompilation = _currentCompilation?.ReplaceSyntaxTree(original, updated);
        }

        /// <summary>
        /// Adds the following <paramref name="trees"/> to the <see cref="Compilation"/>.
        /// </summary>
        /// <param name="trees"><see cref="CSharpSyntaxTree"/>s to add.</param>
        public void UpdateCompilation(IEnumerable<CSharpSyntaxTree>? trees)
        {
            if (trees is null)
            {
                return;
            }

            _currentCompilation = _currentCompilation?.AddSyntaxTrees(trees);
        }

        /// <summary>
        /// Adds the <paramref name="reference"/> to the <see cref="Compilation"/>.
        /// </summary>
        /// <param name="reference"><see cref="MetadataReference"/> to add.</param>
        public void UpdateCompilation(MetadataReference? reference)
        {
            if (reference is null)
            {
                return;
            }

            _currentCompilation = _currentCompilation?.AddReferences(reference);
        }

        /// <summary>
        /// Replaces the <paramref name="original"/> <see cref="MetadataReference"/> with the <paramref name="updated"/> one.
        /// </summary>
        /// <param name="original"><see cref="MetadataReference"/> to replace.</param>
        /// <param name="updated"><see cref="MetadataReference"/> to replace the <paramref name="original"/> by.</param>
        public void UpdateCompilation(MetadataReference? original, MetadataReference? updated)
        {
            if (original is null || updated is null)
            {
                return;
            }

            _currentCompilation = _currentCompilation?.ReplaceReference(original, updated);
        }

        /// <summary>
        /// Adds the following <paramref name="references"/> to the <see cref="Compilation"/>.
        /// </summary>
        /// <param name="references"><see cref="MetadataReference"/>s to add.</param>
        public void UpdateCompilation(IEnumerable<MetadataReference>? references)
        {
            if (references is null)
            {
                return;
            }

            _currentCompilation = _currentCompilation?.AddReferences(references);
        }
    }
}