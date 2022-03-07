// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Durian.Analysis.Cache;
using Durian.Analysis.Data;
using Durian.Analysis.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using static Durian.Analysis.CopyFrom.CopyFromAnalyzer;

namespace Durian.Analysis.CopyFrom
{
    /// <summary>
    /// Filtrates and validates <see cref="MethodDeclarationSyntax"/>es collected by a <see cref="CopyFromSyntaxReceiver"/>.
    /// </summary>
    public sealed class CopyFromMethodFilter : CachedSyntaxFilterValidator<CopyFromCompilationData, CopyFromSyntaxReceiver, MethodDeclarationSyntax, IMethodSymbol, CopyFromMethodData>.WithDiagnostics, ICopyFromFilter
    {
        /// <summary>
        /// <see cref="CopyFromGenerator"/> that created this filter.
        /// </summary>
        public new CopyFromGenerator Generator => (base.Generator as CopyFromGenerator)!;

        /// <summary>
        /// Initializes a new instance of the <see cref="CopyFromMethodFilter"/> class.
        /// </summary>
        /// <param name="generator"><see cref="CopyFromGenerator"/> that is the target of this filter.</param>
        /// <exception cref="ArgumentNullException"><paramref name="generator"/> is <see langword="null"/>.</exception>
        public CopyFromMethodFilter(CopyFromGenerator generator) : base(generator)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CopyFromMethodFilter"/> class.
        /// </summary>
        /// <param name="generator"><see cref="CopyFromGenerator"/> that is the target of this filter.</param>
        /// <param name="hintNameProvider"><see cref="IHintNameProvider"/> that is used to create a hint name for the generated source.</param>
        /// <exception cref="ArgumentNullException"><paramref name="generator"/> is <see langword="null"/>. -or- <paramref name="hintNameProvider"/> is <see langword="null"/>.</exception>
        public CopyFromMethodFilter(CopyFromGenerator generator, IHintNameProvider hintNameProvider) : base(generator, hintNameProvider)
        {
        }

        /// <inheritdoc/>
        public override bool ValidateAndCreate(
            MethodDeclarationSyntax node,
            CopyFromCompilationData compilation,
            SemanticModel semanticModel,
            IMethodSymbol symbol,
            [NotNullWhen(true)] out CopyFromMethodData? data,
            CancellationToken cancellationToken = default
        )
        {
            bool isValid = AnalyzeWithoutPattern(
                symbol,
                compilation,
                semanticModel,
                node,
                out ImmutableArray<AttributeData> attributes,
                out IMethodSymbol? targetMethod
            );

            bool hasTarget = targetMethod is not null;
            List<PatternData>? patterns = null;

            if (hasTarget)
            {
                patterns = new(attributes.Length);
                HashSet<string> set = new();

                foreach (AttributeData attr in attributes)
                {
                    if (SymbolEqualityComparer.Default.Equals(attr.AttributeClass, compilation.PatternAttribute!) &&
                        HasValidRegexPattern(attr, out string? pattern, out string? replacement) &&
                        set.Add(pattern))
                    {
                        patterns.Add(new PatternData(pattern, replacement));
                    }
                }
            }

            if (!isValid)
            {
                data = default;
                return false;
            }

            data = new CopyFromMethodData(
                node,
                compilation,
                symbol,
                semanticModel,
                targetMethod!,
                patterns?.ToArray(),
                attributes: attributes
            );

            return true;
        }

        /// <inheritdoc/>
        public override bool ValidateAndCreate(
            MethodDeclarationSyntax node,
            CopyFromCompilationData compilation,
            SemanticModel semanticModel,
            IMethodSymbol symbol,
            [NotNullWhen(true)] out CopyFromMethodData? data,
            IDiagnosticReceiver diagnosticReceiver,
            CancellationToken cancellationToken = default
        )
        {
            bool isValid = AnalyzeWithoutPattern(
                symbol,
                compilation,
                semanticModel,
                node,
                out ImmutableArray<AttributeData> attributes,
                out IMethodSymbol? targetMethod,
                diagnosticReceiver
            );

            bool hasTarget = targetMethod is not null;
            List<PatternData> patterns = new(attributes.Length);

            HashSet<string> set = new();

            foreach (AttributeData attr in attributes)
            {
                if (SymbolEqualityComparer.Default.Equals(attr.AttributeClass, compilation.PatternAttribute!) &&
                    AnalyzePattern(symbol, attr, set, hasTarget, out string? pattern, out string? replacement, diagnosticReceiver))
                {
                    patterns.Add(new PatternData(pattern, replacement));
                }
            }

            if (!isValid)
            {
                data = default;
                return false;
            }

            data = new CopyFromMethodData(
                node,
                compilation,
                symbol,
                semanticModel,
                targetMethod!,
                patterns.ToArray(),
                attributes: attributes
            );

            return true;
        }

        /// <inheritdoc/>
        protected override IEnumerable<MethodDeclarationSyntax>? GetCandidateNodes(CopyFromSyntaxReceiver syntaxReceiver)
        {
            return syntaxReceiver.CandidateMethods;
        }

        /// <inheritdoc/>
        protected override CopyFromCompilationData? CreateCompilation(in GeneratorExecutionContext context)
        {
            if (context.Compilation is not CSharpCompilation compilation)
            {
                return null;
            }

            return new CopyFromCompilationData(compilation);
        }

        IEnumerable<IMemberData> ICachedGeneratorSyntaxFilter<ICopyFromMember>.Filtrate(in CachedGeneratorExecutionContext<ICopyFromMember> context)
        {
            return ((ICachedGeneratorSyntaxFilter<CopyFromMethodData>)this).Filtrate(context.CastContext<CopyFromMethodData>());
        }

        IEnumerator<IMemberData> ICachedGeneratorSyntaxFilter<ICopyFromMember>.GetEnumerator(in CachedGeneratorExecutionContext<ICopyFromMember> context)
        {
            return ((ICachedGeneratorSyntaxFilter<CopyFromMethodData>)this).GetEnumerator(context.CastContext<CopyFromMethodData>());
        }
    }
}