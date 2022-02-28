// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using Durian.Analysis.Cache;
using Durian.Analysis.Data;
using Durian.Analysis.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Durian.Analysis.DefaultParam.DefaultParamAnalyzer;
using static Durian.Analysis.DefaultParam.DefaultParamTypeAnalyzer;

namespace Durian.Analysis.DefaultParam
{
	/// <summary>
	/// Filtrates and validates <see cref="TypeDeclarationSyntax"/>es collected by a <see cref="DefaultParamSyntaxReceiver"/>.
	/// </summary>
	public sealed class DefaultParamTypeFilter : DefaultParamFilter<DefaultParamTypeData, TypeDeclarationSyntax, INamedTypeSymbol>
    {
		/// <summary>
		/// Initializes a new instance of the <see cref="DefaultParamTypeFilter"/> class.
		/// </summary>
		/// <param name="generator"><see cref="DefaultParamGenerator"/> that is the target of this filter.</param>
		/// <exception cref="ArgumentNullException"><paramref name="generator"/> is <see langword="null"/>.</exception>
		public DefaultParamTypeFilter(DefaultParamGenerator generator) : base(generator)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DefaultParamTypeFilter"/> class.
		/// </summary>
		/// <param name="generator"><see cref="DefaultParamGenerator"/> that is the target of this filter.</param>
		/// <param name="hintNameProvider"><see cref="IHintNameProvider"/> that is used to create a hint name for the generated source.</param>
		/// <exception cref="ArgumentNullException"><paramref name="generator"/> is <see langword="null"/>. -or- <paramref name="hintNameProvider"/> is <see langword="null"/>.</exception>
		public DefaultParamTypeFilter(DefaultParamGenerator generator, IHintNameProvider hintNameProvider) : base(generator, hintNameProvider)
		{
		}

		/// <inheritdoc/>
		protected override IEnumerable<TypeDeclarationSyntax>? GetCandidateNodes(DefaultParamSyntaxReceiver syntaxReceiver)
        {
			return syntaxReceiver.CandidateTypes;
        }

        /// <inheritdoc/>
        protected override TypeParameterListSyntax? GetTypeParameterList(TypeDeclarationSyntax node)
        {
			return node.TypeParameterList;
        }

        /// <inheritdoc/>
        public override bool ValidateAndCreate(
            TypeDeclarationSyntax node,
			DefaultParamCompilationData compilation,
			SemanticModel semanticModel,
            INamedTypeSymbol symbol,
			in TypeParameterContainer typeParameters,
			[NotNullWhen(true)] out DefaultParamTypeData? data,
            CancellationToken cancellationToken = default
		)
        {
			if (AnalyzeAgainstPartial(symbol, cancellationToken) &&
				AnalyzeAgainstProhibitedAttributes(symbol, compilation, out AttributeData[]? attributes) &&
				AnalyzeContainingTypes(symbol, compilation, out ITypeData[]? containingTypes) &&
				AnalyzeTypeParameters(symbol, in typeParameters)
			)
			{
				INamedTypeSymbol[] symbols = DefaultParamUtilities.TypeDatasToTypeSymbols(containingTypes);
				string targetNamespace = GetTargetNamespace(symbol, compilation, attributes, symbols);

				if (AnalyzeCollidingMembers(symbol, in typeParameters, compilation, targetNamespace, out HashSet<int>? applyNewModifiers, attributes, symbols, cancellationToken))
				{
					bool inherit = ShouldInheritInsteadOfCopying(symbol, compilation, attributes, symbols);

					data = new DefaultParamTypeData(
						node,
						compilation,
						symbol,
						semanticModel,
						in typeParameters,
						inherit,
						targetNamespace,
						applyNewModifiers,
						null,
						null,
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
			TypeDeclarationSyntax node,
			DefaultParamCompilationData compilation,
			SemanticModel semanticModel,
			INamedTypeSymbol symbol,
			in TypeParameterContainer typeParameters,
			[NotNullWhen(true)] out DefaultParamTypeData? data,
			IDiagnosticReceiver diagnosticReceiver,
			CancellationToken cancellationToken = default
		)
        {
			bool isValid = AnalyzeAgainstProhibitedAttributes(symbol, compilation, out AttributeData[]? attributes, diagnosticReceiver);
			isValid &= AnalyzeAgainstPartial(symbol, diagnosticReceiver, cancellationToken);
			isValid &= AnalyzeContainingTypes(symbol, compilation, out ITypeData[]? containingTypes, diagnosticReceiver);
			isValid &= AnalyzeTypeParameters(symbol, in typeParameters, diagnosticReceiver);

			if (isValid)
			{
				INamedTypeSymbol[] symbols = DefaultParamUtilities.TypeDatasToTypeSymbols(containingTypes);
				string targetNamespace = GetTargetNamespace(symbol, compilation, attributes!, symbols);

				if (AnalyzeCollidingMembers(symbol, in typeParameters, compilation, targetNamespace, out HashSet<int>? applyNewModifiers, diagnosticReceiver, attributes!, symbols, cancellationToken))
				{
					bool inherit = ShouldInheritInsteadOfCopying(symbol, compilation, diagnosticReceiver, attributes!, symbols);

					data = new DefaultParamTypeData(
						node,
						compilation,
						symbol,
						semanticModel,
						typeParameters,
						inherit,
						targetNamespace,
						applyNewModifiers,
						null,
						null,
						containingTypes,
						null,
						attributes
					);

					return true;
				}
			}
			else
			{
				ShouldInheritInsteadOfCopying(symbol, compilation, diagnosticReceiver);
			}

			data = null;
			return false;
		}
    }
}