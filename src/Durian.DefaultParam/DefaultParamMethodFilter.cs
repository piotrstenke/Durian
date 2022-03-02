// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Durian.Analysis.Data;
using Durian.Analysis.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using static Durian.Analysis.DefaultParam.DefaultParamAnalyzer;
using static Durian.Analysis.DefaultParam.DefaultParamMethodAnalyzer;

namespace Durian.Analysis.DefaultParam
{
    /// <summary>
    /// Filtrates and validates <see cref="MethodDeclarationSyntax"/>es collected by a <see cref="DefaultParamSyntaxReceiver"/>.
    /// </summary>
    public sealed class DefaultParamMethodFilter : DefaultParamFilter<MethodDeclarationSyntax, IMethodSymbol, DefaultParamMethodData>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultParamMethodFilter"/> class.
        /// </summary>
        /// <param name="generator"><see cref="DefaultParamGenerator"/> that is the target of this filter.</param>
        /// <exception cref="ArgumentNullException"><paramref name="generator"/> is <see langword="null"/>.</exception>
        public DefaultParamMethodFilter(DefaultParamGenerator generator) : base(generator)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultParamMethodFilter"/> class.
        /// </summary>
        /// <param name="generator"><see cref="DefaultParamGenerator"/> that is the target of this filter.</param>
        /// <param name="hintNameProvider"><see cref="IHintNameProvider"/> that is used to create a hint name for the generated source.</param>
        /// <exception cref="ArgumentNullException"><paramref name="generator"/> is <see langword="null"/>. -or- <paramref name="hintNameProvider"/> is <see langword="null"/>.</exception>
        public DefaultParamMethodFilter(DefaultParamGenerator generator, IHintNameProvider hintNameProvider) : base(generator, hintNameProvider)
        {
        }

        /// <inheritdoc/>
        public override bool ValidateAndCreate(
            MethodDeclarationSyntax node,
            DefaultParamCompilationData compilation,
            SemanticModel semanticModel,
            IMethodSymbol symbol,
            in TypeParameterContainer typeParameters,
            [NotNullWhen(true)] out DefaultParamMethodData? data,
            IDiagnosticReceiver diagnosticReceiver,
            CancellationToken cancellationToken = default
        )
        {
            if (!ShouldBeAnalyzed(symbol, compilation, in typeParameters, out TypeParameterContainer combinedTypeParameters, diagnosticReceiver, cancellationToken))
            {
                data = null;
                return false;
            }

            bool isValid = AnalyzeAgainstInvalidMethodType(symbol, diagnosticReceiver);
            isValid &= AnalyzeAgainstPartialOrExtern(symbol, node, diagnosticReceiver);
            isValid &= AnalyzeAgainstProhibitedAttributes(symbol, compilation, out AttributeData[]? attributes, diagnosticReceiver);
            isValid &= AnalyzeContainingTypes(symbol, compilation, out ITypeData[]? containingTypes, diagnosticReceiver);

            if (isValid)
            {
                TypeParameterContainer combinedParameters = typeParameters;

                if (AnalyzeBaseMethodAndTypeParameters(symbol, ref combinedParameters, in combinedTypeParameters, diagnosticReceiver))
                {
                    INamedTypeSymbol[] symbols = DefaultParamUtilities.TypeDatasToTypeSymbols(containingTypes!);

                    if (AnalyzeMethodSignature(symbol, in combinedParameters, compilation, out HashSet<int>? newModifiers, diagnosticReceiver, attributes!, symbols, cancellationToken))
                    {
                        bool call = ShouldCallInsteadOfCopying(symbol, compilation, attributes!, symbols);
                        string targetNamespace = GetTargetNamespace(symbol, compilation, attributes!, symbols);

                        data = new(
                            node,
                            compilation,
                            symbol,
                            semanticModel,
                            in combinedParameters,
                            call,
                            targetNamespace,
                            newModifiers,
                            containingTypes,
                            null,
                            attributes
                        );

                        return true;
                    }
                }
            }

            data = null;
            return false;
        }

        /// <inheritdoc/>
        public override bool ValidateAndCreate(
            MethodDeclarationSyntax node,
            DefaultParamCompilationData compilation,
            SemanticModel semanticModel,
            IMethodSymbol symbol,
            in TypeParameterContainer typeParameters,
            [NotNullWhen(true)] out DefaultParamMethodData? data,
            CancellationToken cancellationToken = default
        )
        {
            if (ShouldBeAnalyzed(symbol, compilation, in typeParameters, out TypeParameterContainer combinedTypeParameters, cancellationToken) &&
                AnalyzeAgainstInvalidMethodType(symbol) &&
                AnalyzeAgainstPartialOrExtern(symbol, node) &&
                AnalyzeAgainstProhibitedAttributes(symbol, compilation, out AttributeData[]? attributes) &&
                AnalyzeContainingTypes(symbol, compilation, out ITypeData[]? containingTypes)
            )
            {
                TypeParameterContainer combinedParameters = typeParameters;

                if (AnalyzeBaseMethodAndTypeParameters(symbol, ref combinedParameters, in combinedTypeParameters))
                {
                    INamedTypeSymbol[] symbols = DefaultParamUtilities.TypeDatasToTypeSymbols(containingTypes);

                    if (AnalyzeMethodSignature(symbol, in combinedParameters, compilation, out HashSet<int>? newModifiers, attributes, symbols, cancellationToken))
                    {
                        bool call = ShouldCallInsteadOfCopying(symbol, compilation, attributes!, symbols);
                        string targetNamespace = GetTargetNamespace(symbol, compilation, attributes, symbols);

                        data = new(
                            node,
                            compilation,
                            symbol,
                            semanticModel,
                            in combinedParameters,
                            call,
                            targetNamespace,
                            newModifiers,
                            containingTypes,
                            null,
                            attributes
                        );

                        return true;
                    }
                }
            }

            data = null;
            return false;
        }

        /// <inheritdoc/>
        protected override IEnumerable<MethodDeclarationSyntax>? GetCandidateNodes(DefaultParamSyntaxReceiver syntaxReceiver)
        {
            return syntaxReceiver.CandidateMethods;
        }

        /// <inheritdoc/>
        protected override TypeParameterListSyntax? GetTypeParameterList(MethodDeclarationSyntax node)
        {
            return node.TypeParameterList;
        }

        /// <inheritdoc/>
        protected override bool TypeParametersAreValid(in TypeParameterContainer typeParameters, MethodDeclarationSyntax node)
        {
            return typeParameters.HasDefaultParams || node.Modifiers.Any(m => m.IsKind(SyntaxKind.OverrideKeyword));
        }
    }
}