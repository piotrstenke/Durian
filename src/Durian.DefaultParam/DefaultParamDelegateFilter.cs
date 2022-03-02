// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Durian.Analysis.Data;
using Durian.Analysis.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using static Durian.Analysis.DefaultParam.DefaultParamAnalyzer;
using static Durian.Analysis.DefaultParam.DefaultParamDelegateAnalyzer;

namespace Durian.Analysis.DefaultParam
{
    /// <summary>
    /// Filtrates and validates <see cref="DelegateDeclarationSyntax"/>es collected by a <see cref="DefaultParamSyntaxReceiver"/>.
    /// </summary>
    public sealed class DefaultParamDelegateFilter : DefaultParamFilter<DelegateDeclarationSyntax, INamedTypeSymbol, DefaultParamDelegateData>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultParamDelegateFilter"/> class.
        /// </summary>
        /// <param name="generator"><see cref="DefaultParamGenerator"/> that is the target of this filter.</param>
        /// <exception cref="ArgumentNullException"><paramref name="generator"/> is <see langword="null"/>.</exception>
        public DefaultParamDelegateFilter(DefaultParamGenerator generator) : base(generator)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultParamDelegateFilter"/> class.
        /// </summary>
        /// <param name="generator"><see cref="DefaultParamGenerator"/> that is the target of this filter.</param>
        /// <param name="hintNameProvider"><see cref="IHintNameProvider"/> that is used to create a hint name for the generated source.</param>
        /// <exception cref="ArgumentNullException"><paramref name="generator"/> is <see langword="null"/>. -or- <paramref name="hintNameProvider"/> is <see langword="null"/>.</exception>
        public DefaultParamDelegateFilter(DefaultParamGenerator generator, IHintNameProvider hintNameProvider) : base(generator, hintNameProvider)
        {
        }

        /// <inheritdoc/>
        public override bool ValidateAndCreate(
            DelegateDeclarationSyntax node,
            DefaultParamCompilationData compilation,
            SemanticModel semanticModel,
            INamedTypeSymbol symbol,
            in TypeParameterContainer typeParameters,
            [NotNullWhen(true)] out DefaultParamDelegateData? data,
            CancellationToken cancellationToken = default
        )
        {
            if (AnalyzeAgainstProhibitedAttributes(symbol, compilation, out AttributeData[]? attributes) &&
                AnalyzeContainingTypes(symbol, compilation, out ITypeData[]? containingTypes) &&
                AnalyzeTypeParameters(symbol, in typeParameters))
            {
                INamedTypeSymbol[] symbols = DefaultParamUtilities.TypeDatasToTypeSymbols(containingTypes);
                string targetNamespace = GetTargetNamespace(symbol, compilation, attributes, symbols);

                if (AnalyzeCollidingMembers(symbol, in typeParameters, compilation, targetNamespace, out HashSet<int>? newModifiers, attributes, symbols, cancellationToken))
                {
                    data = new DefaultParamDelegateData(
                        node,
                        compilation,
                        symbol,
                        semanticModel,
                        typeParameters,
                        targetNamespace,
                        newModifiers,
                        containingTypes,
                        null,
                        attributes
                    );

                    return true;
                }
            }

            data = null;
            return false;
        }

        /// <inheritdoc/>
        public override bool ValidateAndCreate(
            DelegateDeclarationSyntax node,
            DefaultParamCompilationData compilation,
            SemanticModel semanticModel,
            INamedTypeSymbol symbol,
            in TypeParameterContainer typeParameters,
            [NotNullWhen(true)] out DefaultParamDelegateData? data,
            IDiagnosticReceiver diagnosticReceiver,
            CancellationToken cancellationToken = default
        )
        {
            bool isValid = AnalyzeAgainstProhibitedAttributes(symbol, compilation, out AttributeData[]? attributes, diagnosticReceiver);
            isValid &= AnalyzeContainingTypes(symbol, compilation, out ITypeData[]? containingTypes, diagnosticReceiver);
            isValid &= AnalyzeTypeParameters(symbol, in typeParameters, diagnosticReceiver);

            if (isValid)
            {
                INamedTypeSymbol[] symbols = DefaultParamUtilities.TypeDatasToTypeSymbols(containingTypes!);
                string targetNamespace = GetTargetNamespace(symbol, compilation, attributes!, symbols);

                if (AnalyzeCollidingMembers(symbol, in typeParameters, compilation, targetNamespace, out HashSet<int>? applyNewModifiers, diagnosticReceiver, attributes!, symbols, cancellationToken))
                {
                    data = new DefaultParamDelegateData(
                        node,
                        compilation,
                        symbol,
                        semanticModel,
                        typeParameters,
                        targetNamespace,
                        applyNewModifiers,
                        containingTypes,
                        null,
                        attributes
                    );

                    return true;
                }
            }

            data = null;
            return false;
        }

        /// <inheritdoc/>
        protected override IEnumerable<DelegateDeclarationSyntax>? GetCandidateNodes(DefaultParamSyntaxReceiver syntaxReceiver)
        {
            return syntaxReceiver.CandidateDelegates;
        }

        /// <inheritdoc/>
        protected override TypeParameterListSyntax? GetTypeParameterList(DelegateDeclarationSyntax node)
        {
            return node.TypeParameterList;
        }
    }
}