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
    /// Filtrates and validates <see cref="TypeDeclarationSyntax"/>es collected by a <see cref="CopyFromSyntaxReceiver"/>.
    /// </summary>
    public sealed class CopyFromTypeFilter : CachedSyntaxFilterValidator<CopyFromCompilationData, CopyFromSyntaxReceiver, TypeDeclarationSyntax, INamedTypeSymbol, CopyFromTypeData>.WithDiagnostics, ICopyFromFilter
    {
        /// <summary>
        /// <see cref="CopyFromGenerator"/> that created this filter.
        /// </summary>
        public new CopyFromGenerator Generator => (base.Generator as CopyFromGenerator)!;

        /// <summary>
        /// Initializes a new instance of the <see cref="CopyFromTypeFilter"/> class.
        /// </summary>
        /// <param name="generator"><see cref="CopyFromGenerator"/> that is the target of this filter.</param>
        /// <exception cref="ArgumentNullException"><paramref name="generator"/> is <see langword="null"/>.</exception>
        public CopyFromTypeFilter(CopyFromGenerator generator) : base(generator)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CopyFromTypeFilter"/> class.
        /// </summary>
        /// <param name="generator"><see cref="CopyFromGenerator"/> that is the target of this filter.</param>
        /// <param name="hintNameProvider"><see cref="IHintNameProvider"/> that is used to create a hint name for the generated source.</param>
        /// <exception cref="ArgumentNullException"><paramref name="generator"/> is <see langword="null"/>. -or- <paramref name="hintNameProvider"/> is <see langword="null"/>.</exception>
        public CopyFromTypeFilter(CopyFromGenerator generator, IHintNameProvider hintNameProvider) : base(generator, hintNameProvider)
        {
        }

        /// <inheritdoc/>
        public override bool ValidateAndCreate(
            TypeDeclarationSyntax node,
            CopyFromCompilationData compilation,
            SemanticModel semanticModel,
            INamedTypeSymbol symbol,
            [NotNullWhen(true)] out CopyFromTypeData? data,
            CancellationToken cancellationToken = default
        )
        {
            bool isValid = AnalyzeWithoutPattern(
                symbol,
                compilation,
                semanticModel,
                out ImmutableArray<AttributeData> attributes,
                out List<INamedTypeSymbol>? targetTypes
            );

            bool hasTarget = targetTypes?.Count > 0;
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

            data = new CopyFromTypeData(
                node,
                compilation,
                symbol,
                semanticModel,
                targetTypes!.ToArray(),
                patterns?.ToArray(),
                attributes: attributes
            );

            return true;
        }

        /// <inheritdoc/>
        public override bool ValidateAndCreate(
            TypeDeclarationSyntax node,
            CopyFromCompilationData compilation,
            SemanticModel semanticModel,
            INamedTypeSymbol symbol,
            [NotNullWhen(true)] out CopyFromTypeData? data,
            IDiagnosticReceiver diagnosticReceiver,
            CancellationToken cancellationToken = default
        )
        {
            bool isValid = AnalyzeWithoutPattern(
                symbol,
                compilation,
                semanticModel,
                out ImmutableArray<AttributeData> attributes,
                out List<INamedTypeSymbol>? targetTypes,
                diagnosticReceiver
            );

            bool hasTarget = targetTypes?.Count > 0;
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

            data = new CopyFromTypeData(
                node,
                compilation,
                symbol,
                semanticModel,
                targetTypes!.ToArray(),
                patterns.ToArray(),
                attributes: attributes
            );

            return true;
        }

        /// <inheritdoc/>
        protected override IEnumerable<TypeDeclarationSyntax>? GetCandidateNodes(CopyFromSyntaxReceiver syntaxReceiver)
        {
            return syntaxReceiver.CandidateTypes;
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
            return ((ICachedGeneratorSyntaxFilter<CopyFromTypeData>)this).Filtrate(context.CastContext<CopyFromTypeData>());
        }

        IEnumerator<IMemberData> ICachedGeneratorSyntaxFilter<ICopyFromMember>.GetEnumerator(in CachedGeneratorExecutionContext<ICopyFromMember> context)
        {
            return ((ICachedGeneratorSyntaxFilter<CopyFromTypeData>)this).GetEnumerator(context.CastContext<CopyFromTypeData>());
        }
    }
}