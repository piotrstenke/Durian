// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Durian.Analysis.Data;
using Durian.Analysis.Filtration;
using Durian.Analysis.SymbolContainers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Durian.Analysis.DefaultParam.DefaultParamAnalyzer;
using static Durian.Analysis.DefaultParam.Delegates.DefaultParamDelegateAnalyzer;

namespace Durian.Analysis.DefaultParam.Delegates
{
	/// <summary>
	/// Filtrates and validates <see cref="DelegateDeclarationSyntax"/>es collected by a <see cref="DefaultParamSyntaxReceiver"/>.
	/// </summary>
	public sealed class DefaultParamDelegateFilter : DefaultParamFilter<DefaultParamDelegateContext>
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="DefaultParamDelegateFilter"/> class.
		/// </summary>
		public DefaultParamDelegateFilter()
		{
		}

		/// <inheritdoc/>
		public override bool ValidateAndCreate(in DefaultParamDelegateContext context, out IMemberData? data)
		{
			if (AnalyzeAgainstProhibitedAttributes(context.Symbol, context.TargetCompilation, out AttributeData[]? attributes) &&
				AnalyzeContainingTypes(context.Symbol, context.TargetCompilation, out ITypeData[]? containingTypes) &&
				AnalyzeTypeParameters(context.Symbol, in context.GetTypeParameters()))
			{
				ImmutableArray<INamedTypeSymbol> symbols = DefaultParamUtilities.TypeDatasToTypeSymbols(containingTypes);
				string targetNamespace = GetTargetNamespace(context.Symbol, context.TargetCompilation, attributes, symbols);

				if (AnalyzeCollidingMembers(context.Symbol, in context.GetTypeParameters(), context.TargetCompilation, targetNamespace, out HashSet<int>? newModifiers, attributes, symbols, context.CancellationToken))
				{
					data = new DefaultParamDelegateData(
						context.Node,
						context.TargetCompilation,
						new DefaultParamDelegateData.Properties()
						{
							SemanticModel = context.SemanticModel,
							Symbol = context.Symbol,
							TargetNamespace = targetNamespace,
							NewModifierIndices = newModifiers,
							ContainingTypes = containingTypes.ToWritableContainer(context.TargetCompilation),
							TypeParameters = context.GetTypeParameters(),
							Attributes = attributes.ToImmutableArray()
						});

					return true;
				}
			}

			data = null;
			return false;
		}

		/// <inheritdoc/>
		public override bool ValidateAndCreate(in DefaultParamDelegateContext context, out IMemberData? data, IDiagnosticReceiver diagnosticReceiver)
		{
			bool isValid = AnalyzeAgainstProhibitedAttributes(context.Symbol, context.TargetCompilation, out AttributeData[]? attributes, diagnosticReceiver);
			isValid &= AnalyzeContainingTypes(context.Symbol, context.TargetCompilation, out IWritableSymbolContainer<INamedTypeSymbol, ITypeData>? containingTypes, diagnosticReceiver);
			isValid &= AnalyzeTypeParameters(context.Symbol, in context.GetTypeParameters(), diagnosticReceiver);

			if (isValid)
			{
				ImmutableArray<INamedTypeSymbol> symbols = containingTypes!.GetSymbols().CastArray<INamedTypeSymbol>();
				string targetNamespace = GetTargetNamespace(context.Symbol, context.TargetCompilation, attributes, symbols);

				if (AnalyzeCollidingMembers(context.Symbol, in context.GetTypeParameters(), context.TargetCompilation, targetNamespace, out HashSet<int>? applyNewModifiers, diagnosticReceiver, attributes, symbols, context.CancellationToken))
				{
					data = new DefaultParamDelegateData(
						context.Node,
						context.TargetCompilation,
						new DefaultParamDelegateData.Properties()
						{
							SemanticModel = context.SemanticModel,
							Symbol = context.Symbol,
							TargetNamespace = targetNamespace,
							NewModifierIndices = applyNewModifiers,
							ContainingTypes = new(containingTypes),
							TypeParameters = context.GetTypeParameters(),
							Attributes = attributes!.ToImmutableArray()
						});

					return true;
				}
			}

			data = null;
			return false;
		}

		/// <inheritdoc/>
		protected override IEnumerable<SyntaxNode>? GetCandidateNodes(IDurianSyntaxReceiver syntaxReceiver)
		{
			if (syntaxReceiver is not DefaultParamSyntaxReceiver sr)
			{
				return base.GetCandidateNodes(syntaxReceiver);
			}

			if (sr.CandidateDelegates.Count == 0)
			{
				return default;
			}

			return sr.CandidateDelegates;
		}

		/// <inheritdoc/>
		protected override TypeParameterListSyntax? GetTypeParameterList(SyntaxNode node)
		{
			return (node as DelegateDeclarationSyntax)?.TypeParameterList;
		}

		/// <inheritdoc/>
		protected override bool TryCreateContext(in SyntaxValidationContext validationContext, in TypeParameterContainer typeParameters, [NotNullWhen(true)] out DefaultParamDelegateContext context)
		{
			if (validationContext.TargetCompilation is not DefaultParamCompilationData compilation ||
				validationContext.Node is not DelegateDeclarationSyntax node ||
				validationContext.Symbol is not INamedTypeSymbol symbol
			)
			{
				context = default;
				return false;
			}

			context = new(compilation, validationContext.SemanticModel, symbol, in typeParameters, node, validationContext.CancellationToken);
			return true;
		}
	}
}
