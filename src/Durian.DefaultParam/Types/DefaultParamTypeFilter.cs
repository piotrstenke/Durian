// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Durian.Analysis.Data;
using Durian.Analysis.Filters;
using Durian.Analysis.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using static Durian.Analysis.DefaultParam.DefaultParamAnalyzer;
using static Durian.Analysis.DefaultParam.Types.DefaultParamTypeAnalyzer;

namespace Durian.Analysis.DefaultParam.Types
{
	/// <summary>
	/// Filtrates and validates <see cref="TypeDeclarationSyntax"/>es collected by a <see cref="DefaultParamSyntaxReceiver"/>.
	/// </summary>
	public sealed class DefaultParamTypeFilter : DefaultParamFilter<DefaultParamTypeContext>
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="DefaultParamTypeFilter"/> class.
		/// </summary>
		public DefaultParamTypeFilter()
		{
		}

		/// <inheritdoc/>
		public override bool ValidateAndCreate(in DefaultParamTypeContext context, [NotNullWhen(true)] out IMemberData? data)
		{
			if (AnalyzeAgainstPartial(context.Symbol, context.CancellationToken) &&
				AnalyzeAgainstProhibitedAttributes(context.Symbol, context.TargetCompilation, out AttributeData[]? attributes) &&
				AnalyzeContainingTypes(context.Symbol, context.TargetCompilation, out ITypeData[]? containingTypes) &&
				AnalyzeTypeParameters(context.Symbol, in context.GetTypeParameters())
			)
			{
				INamedTypeSymbol[] symbols = DefaultParamUtilities.TypeDatasToTypeSymbols(containingTypes);
				string targetNamespace = GetTargetNamespace(context.Symbol, context.TargetCompilation, attributes, symbols);

				if (AnalyzeCollidingMembers(
					context.Symbol,
					in context.GetTypeParameters(),
					context.TargetCompilation,
					targetNamespace,
					out HashSet<int>? applyNewModifiers,
					attributes,
					symbols,
					context.CancellationToken
				))
				{
					bool inherit = ShouldInheritInsteadOfCopying(context.Symbol, context.TargetCompilation, attributes, symbols);

					data = new DefaultParamTypeData(
						context.Node,
						context.TargetCompilation,
						context.Symbol,
						context.SemanticModel,
						in context.GetTypeParameters(),
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
		public override bool ValidateAndCreate(in DefaultParamTypeContext context, [NotNullWhen(true)] out IMemberData? data, IDiagnosticReceiver diagnosticReceiver)
		{
			bool isValid = AnalyzeAgainstProhibitedAttributes(context.Symbol, context.TargetCompilation, out AttributeData[]? attributes, diagnosticReceiver);
			isValid &= AnalyzeAgainstPartial(context.Symbol, diagnosticReceiver, context.CancellationToken);
			isValid &= AnalyzeContainingTypes(context.Symbol, context.TargetCompilation, out ITypeData[]? containingTypes, diagnosticReceiver);
			isValid &= AnalyzeTypeParameters(context.Symbol, in context.GetTypeParameters(), diagnosticReceiver);

			if (isValid)
			{
				INamedTypeSymbol[] symbols = DefaultParamUtilities.TypeDatasToTypeSymbols(containingTypes);
				string targetNamespace = GetTargetNamespace(context.Symbol, context.TargetCompilation, attributes, symbols);

				if (AnalyzeCollidingMembers(
					context.Symbol,
					in context.GetTypeParameters(),
					context.TargetCompilation,
					targetNamespace,
					out HashSet<int>? applyNewModifiers,
					diagnosticReceiver,
					attributes,
					symbols,
					context.CancellationToken
				))
				{
					bool inherit = ShouldInheritInsteadOfCopying(context.Symbol, context.TargetCompilation, diagnosticReceiver, attributes, symbols);

					data = new DefaultParamTypeData(
						context.Node,
						context.TargetCompilation,
						context.Symbol,
						context.SemanticModel,
						in context.GetTypeParameters(),
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
				ShouldInheritInsteadOfCopying(context.Symbol, context.TargetCompilation, diagnosticReceiver);
			}

			data = null;
			return false;
		}

		/// <inheritdoc/>
		protected override IEnumerable<CSharpSyntaxNode>? GetCandidateNodes(IDurianSyntaxReceiver syntaxReceiver)
		{
			if (syntaxReceiver is not DefaultParamSyntaxReceiver sr)
			{
				return base.GetCandidateNodes(syntaxReceiver);
			}

			if (sr.CandidateTypes.Count == 0)
			{
				return default;
			}

			return sr.CandidateTypes;
		}

		/// <inheritdoc/>
		protected override TypeParameterListSyntax? GetTypeParameterList(CSharpSyntaxNode node)
		{
			return (node as TypeDeclarationSyntax)?.TypeParameterList;
		}

		/// <inheritdoc/>
		protected override bool TryCreateContext(in SyntaxValidationContext validationContext, in TypeParameterContainer typeParameters, [NotNullWhen(true)] out DefaultParamTypeContext context)
		{
			if (validationContext.TargetCompilation is not DefaultParamCompilationData compilation ||
				validationContext.Node is not TypeDeclarationSyntax node ||
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
