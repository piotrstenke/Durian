// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Durian.Analysis.Data;
using Durian.Analysis.Filtration;
using Durian.Analysis.SymbolContainers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Durian.Analysis.DefaultParam.DefaultParamAnalyzer;
using static Durian.Analysis.DefaultParam.Methods.DefaultParamMethodAnalyzer;

namespace Durian.Analysis.DefaultParam.Methods
{
	/// <summary>
	/// Filtrates and validates <see cref="MethodDeclarationSyntax"/>es collected by a <see cref="DefaultParamSyntaxReceiver"/>.
	/// </summary>
	public sealed class DefaultParamMethodFilter : DefaultParamFilter<DefaultParamMethodContext>
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="DefaultParamMethodFilter"/> class.
		/// </summary>
		public DefaultParamMethodFilter()
		{
		}

		/// <inheritdoc/>
		public override bool ValidateAndCreate(in DefaultParamMethodContext context, [NotNullWhen(true)] out IMemberData? data, IDiagnosticReceiver diagnosticReceiver)
		{
			if (!ShouldBeAnalyzed(context.Symbol, context.TargetCompilation, in context.GetTypeParameters(), out TypeParameterContainer combinedTypeParameters, diagnosticReceiver, context.CancellationToken))
			{
				data = null;
				return false;
			}

			bool isValid = AnalyzeAgainstInvalidMethodType(context.Symbol, diagnosticReceiver);
			isValid &= AnalyzeAgainstPartialOrExtern(context.Symbol, context.Node, diagnosticReceiver);
			isValid &= AnalyzeAgainstProhibitedAttributes(context.Symbol, context.TargetCompilation, out AttributeData[]? attributes, diagnosticReceiver);
			isValid &= AnalyzeContainingTypes(context.Symbol, context.TargetCompilation, out IWritableSymbolContainer<INamedTypeSymbol, ITypeData>? containingTypes, diagnosticReceiver);

			if (isValid)
			{
				TypeParameterContainer combinedParameters = context.GetTypeParameters();

				if (AnalyzeBaseMethodAndTypeParameters(context.Symbol, ref combinedParameters, in combinedTypeParameters, diagnosticReceiver))
				{
					ImmutableArray<INamedTypeSymbol> symbols = containingTypes!.GetSymbols().CastArray<INamedTypeSymbol>();

					if (AnalyzeMethodSignature(context.Symbol, in combinedParameters, context.TargetCompilation, out HashSet<int>? newModifiers, diagnosticReceiver, attributes, symbols, context.CancellationToken))
					{
						bool call = ShouldCallInsteadOfCopying(context.Symbol, context.TargetCompilation, attributes, symbols);
						string targetNamespace = GetTargetNamespace(context.Symbol, context.TargetCompilation, attributes, symbols);

						data = new DefaultParamMethodData(
							context.Node,
							context.TargetCompilation,
							new DefaultParamMethodData.Properties()
							{
								Symbol = context.Symbol,
								SemanticModel = context.SemanticModel,
								NewModifierIndices = newModifiers,
								TypeParameters = combinedParameters,
								CallInsteadOfCopying = call,
								TargetNamespace = targetNamespace,
								ContainingTypes = new(containingTypes),
								Attributes = attributes!.ToImmutableArray()
							});

						return true;
					}
				}
			}

			data = null;
			return false;
		}

		/// <inheritdoc/>
		public override bool ValidateAndCreate(in DefaultParamMethodContext context, [NotNullWhen(true)] out IMemberData? data)
		{
			if (ShouldBeAnalyzed(context.Symbol, context.TargetCompilation, in context.GetTypeParameters(), out TypeParameterContainer combinedTypeParameters, context.CancellationToken) &&
				AnalyzeAgainstInvalidMethodType(context.Symbol) &&
				AnalyzeAgainstPartialOrExtern(context.Symbol, context.Node) &&
				AnalyzeAgainstProhibitedAttributes(context.Symbol, context.TargetCompilation, out AttributeData[]? attributes) &&
				AnalyzeContainingTypes(context.Symbol, context.TargetCompilation, out ImmutableArray<ITypeData> containingTypes)
			)
			{
				TypeParameterContainer combinedParameters = context.GetTypeParameters();

				if (AnalyzeBaseMethodAndTypeParameters(context.Symbol, ref combinedParameters, in combinedTypeParameters))
				{
					ImmutableArray<INamedTypeSymbol> symbols = DefaultParamUtilities.TypeDatasToTypeSymbols(containingTypes);

					if (AnalyzeMethodSignature(context.Symbol, in combinedParameters, context.TargetCompilation, out HashSet<int>? newModifiers, attributes, symbols, context.CancellationToken))
					{
						bool call = ShouldCallInsteadOfCopying(context.Symbol, context.TargetCompilation, attributes, symbols);
						string targetNamespace = GetTargetNamespace(context.Symbol, context.TargetCompilation, attributes, symbols);

						data = new DefaultParamMethodData(
							context.Node,
							context.TargetCompilation,
							new DefaultParamMethodData.Properties()
							{
								Symbol = context.Symbol,
								SemanticModel = context.SemanticModel,
								NewModifierIndices = newModifiers,
								TypeParameters = combinedParameters,
								CallInsteadOfCopying = call,
								TargetNamespace = targetNamespace,
								ContainingTypes = containingTypes!.ToWritableContainer(context.TargetCompilation),
								Attributes = attributes!.ToImmutableArray()
							});

						return true;
					}
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

			if (sr.CandidateMethods.Count == 0)
			{
				return default;
			}

			return sr.CandidateMethods;
		}

		/// <inheritdoc/>
		protected override TypeParameterListSyntax? GetTypeParameterList(SyntaxNode node)
		{
			return (node as MethodDeclarationSyntax)?.TypeParameterList;
		}

		/// <inheritdoc/>
		protected override bool TryCreateContext(in SyntaxValidationContext validationContext, in TypeParameterContainer typeParameters, [NotNullWhen(true)] out DefaultParamMethodContext context)
		{
			if (validationContext.TargetCompilation is not DefaultParamCompilationData compilation ||
				validationContext.Node is not MethodDeclarationSyntax node ||
				validationContext.Symbol is not IMethodSymbol symbol
			)
			{
				context = default;
				return false;
			}

			context = new(compilation, validationContext.SemanticModel, symbol, in typeParameters, node, validationContext.CancellationToken);
			return true;
		}

		/// <inheritdoc/>
		protected override bool TypeParametersAreValid(in TypeParameterContainer typeParameters, SyntaxNode node)
		{
			if (node is not MethodDeclarationSyntax method)
			{
				return false;
			}

			return typeParameters.HasDefaultParams || method.Modifiers.Any(m => m.IsKind(SyntaxKind.OverrideKeyword));
		}
	}
}
