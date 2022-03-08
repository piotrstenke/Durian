// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Durian.TestServices
{
    /// <summary>
    /// A <see cref="IGeneratorTestResult"/> that represents a single <see cref="CSharpSyntaxTree"/> retrieved from a tested <see cref="ISourceGenerator"/>.
    /// </summary>
    public readonly struct SingletonGeneratorTestResult : IGeneratorTestResult
    {
        private readonly GeneratorRunResult _runResult;

        private readonly GeneratedSourceResult _sourceResult;

        /// <summary>
        /// A collection of <see cref="Diagnostic"/>s that were reported during the generator pass.
        /// </summary>
        public ImmutableArray<Diagnostic> Diagnostics => _runResult.Diagnostics;

        /// <inheritdoc/>
        public Exception? Exception => _runResult.Exception;

        /// <inheritdoc/>
        public ISourceGenerator Generator => _runResult.Generator;

        /// <summary>
        /// An identifier provided by the generator that identifies the added <see cref="SourceText"/>
        /// </summary>
        public string? HintName => _sourceResult.HintName;

        /// <inheritdoc/>
        public CSharpCompilation InputCompilation { get; }

        /// <summary>
        /// Determines whether the <see cref="ISourceGenerator"/> actually generated any <see cref="CSharpSyntaxTree"/>.
        /// </summary>
        [MemberNotNullWhen(true, nameof(SourceText), nameof(SyntaxTree), nameof(HintName))]
        public bool IsGenerated { get; }

        /// <inheritdoc/>
        public CSharpCompilation OutputCompilation { get; }

        /// <summary>
        /// The <see cref="Microsoft.CodeAnalysis.Text.SourceText"/> that was added by the generator.
        /// </summary>
        public SourceText? SourceText => _sourceResult.SourceText;

        /// <summary>
        /// A <see cref="CSharpSyntaxTree"/> that was generated during the generator pass.
        /// </summary>
        public CSharpSyntaxTree? SyntaxTree => _sourceResult.SyntaxTree as CSharpSyntaxTree;

        private SingletonGeneratorTestResult(
            GeneratorRunResult runResult,
            GeneratedSourceResult sourceResult,
            CSharpCompilation inputCompilation,
            CSharpCompilation outputCompilation,
            bool isGenerated
        )
        {
            _runResult = runResult;
            _sourceResult = sourceResult;
            InputCompilation = inputCompilation;
            OutputCompilation = outputCompilation;
            IsGenerated = isGenerated;
        }

        /// <inheritdoc cref="Create(CSharpGeneratorDriver, CSharpCompilation, CSharpCompilation, int)"/>
        public static SingletonGeneratorTestResult Create(CSharpGeneratorDriver generatorDriver, CSharpCompilation inputCompilation, CSharpCompilation outputCompilation)
        {
            return Create(generatorDriver, inputCompilation, outputCompilation, 0);
        }

        /// <summary>
        /// Creates a new <see cref="SingletonGeneratorTestResult"/> from the specified <paramref name="generatorDriver"/>.
        /// </summary>
        /// <param name="generatorDriver"><see cref="CSharpGeneratorDriver"/> that was used to run the generator test.</param>
        /// <param name="inputCompilation"><see cref="CSharpCompilation"/> that was passed as input to the <paramref name="generatorDriver"/>.</param>
        /// <param name="outputCompilation"><see cref="CSharpCompilation"/> that was returned by the <paramref name="generatorDriver"/>.</param>
        /// <param name="sourceIndex">Index of generated syntax tree to include in the result.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="generatorDriver"/> is <see langword="null"/>. -or-
        /// <paramref name="inputCompilation"/> is <see langword="null"/>. -or-
        /// <paramref name="outputCompilation"/> is <see langword="null"/>.
        /// </exception>
        public static SingletonGeneratorTestResult Create(
            CSharpGeneratorDriver generatorDriver,
            CSharpCompilation inputCompilation,
            CSharpCompilation outputCompilation,
            int sourceIndex
        )
        {
            if (generatorDriver is null)
            {
                throw new ArgumentNullException(nameof(generatorDriver));
            }

            if (inputCompilation is null)
            {
                throw new ArgumentNullException(nameof(inputCompilation));
            }

            if (outputCompilation is null)
            {
                throw new ArgumentNullException(nameof(outputCompilation));
            }

            GeneratorRunResult runResult = generatorDriver.GetRunResult().Results[0];
            bool isGenerated = runResult.Exception is null && runResult.GeneratedSources.Length > sourceIndex;

            if (isGenerated)
            {
                GeneratedSourceResult sourceResult = runResult.GeneratedSources[sourceIndex];

                return new SingletonGeneratorTestResult(runResult, sourceResult, inputCompilation, outputCompilation, true);
            }

            return new SingletonGeneratorTestResult(runResult, default, inputCompilation, outputCompilation, false);
        }

        /// <summary>
        /// Checks if the <paramref name="expected"/> <see cref="CSharpSyntaxTree"/> is equivalent to the <see cref="CSharpSyntaxTree"/> created by the <see cref="ISourceGenerator"/>.
        /// </summary>
        /// <param name="expected">A <see cref="CSharpSyntaxTree"/> that was expected to be generated.</param>
        public bool Compare(CSharpSyntaxTree? expected)
        {
            if (expected is null)
            {
                return false;
            }

            return SyntaxTree?.IsEquivalentTo(expected) ?? false;
        }

        /// <summary>
        /// Checks if the <see cref="CSharpSyntaxTree"/> created from the <paramref name="expected"/> source is equivalent to the <see cref="CSharpSyntaxTree"/> created by the <see cref="ISourceGenerator"/>.
        /// </summary>
        /// <param name="expected">A <see cref="string"/> that represents a <see cref="CSharpSyntaxTree"/> that was expected to be generated.</param>
        public bool Compare(string? expected)
        {
            if (expected is null)
            {
                return false;
            }

            if (CSharpSyntaxTree.ParseText(expected, encoding: Encoding.UTF8) is not CSharpSyntaxTree tree)
            {
                return false;
            }

            return SyntaxTree?.IsEquivalentTo(tree) ?? false;
        }

        bool IGeneratorTestResult.Compare(GeneratorDriverRunResult result)
        {
            if (result is null || result.GeneratedTrees.IsDefaultOrEmpty)
            {
                return false;
            }

            if (result.GeneratedTrees[0] is not CSharpSyntaxTree tree)
            {
                return false;
            }

            return SyntaxTree?.IsEquivalentTo(tree) ?? false;
        }
    }
}