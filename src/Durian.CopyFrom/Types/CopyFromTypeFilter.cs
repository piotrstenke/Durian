// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Durian.Analysis.Cache;
using Durian.Analysis.Data;
using Durian.Analysis.Filters;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Durian.Analysis.CopyFrom.CopyFromAnalyzer;

namespace Durian.Analysis.CopyFrom.Types
{
	/// <summary>
	/// Filtrates and validates <see cref="TypeDeclarationSyntax"/>es collected by a <see cref="CopyFromSyntaxReceiver"/>.
	/// </summary>
	public sealed class CopyFromTypeFilter : CachedSyntaxValidator<ICopyFromMember, CopyFromTypeContext>
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="CopyFromTypeFilter"/> class.
		/// </summary>
		public CopyFromTypeFilter()
		{
		}

		/// <inheritdoc/>
		public override bool ValidateAndCreate(in CopyFromTypeContext context, out IMemberData? data)
		{
			if (context.Node is null)
			{
				data = default;
				return false;
			}

			bool isValid = AnalyzeTypeWithoutPattern(
				in context,
				out ImmutableArray<AttributeData> attributes,
				out List<INamedTypeSymbol>? dependencies,
				out List<TargetData>? targetTypes
			);

			bool hasTarget = targetTypes?.Count > 0;
			PatternData[]? patterns = null;

			if (hasTarget)
			{
				HashSet<string> set = new();

				List<(int order, PatternData pattern)> notSortedPatterns = new();

				foreach (AttributeData attr in attributes)
				{
					if (SymbolEqualityComparer.Default.Equals(attr.AttributeClass, context.Compilation.PatternAttribute) &&
						HasValidRegexPattern(attr, out string? pattern, out string? replacement, out int order) &&
						set.Add(pattern))
					{
						notSortedPatterns.Add((order, new PatternData(pattern, replacement)));
					}
				}

				patterns = SortByOrder(notSortedPatterns);
			}

			if (!isValid)
			{
				data = default;
				return false;
			}

			data = new CopyFromTypeData(
				context.Node!,
				context.Compilation,
				context.Symbol,
				context.SemanticModel,
				targetTypes!.ToArray(),
				dependencies?.ToArray(),
				patterns,
				attributes: attributes
			);

			return true;
		}

		/// <inheritdoc/>
		public override bool ValidateAndCreate(in CopyFromTypeContext context, out IMemberData? data, IDiagnosticReceiver diagnosticReceiver)
		{
			if (context.Node is null)
			{
				data = default;
				return false;
			}

			bool isValid = AnalyzeTypeWithoutPattern(
				in context,
				out ImmutableArray<AttributeData> attributes,
				out List<INamedTypeSymbol>? dependencies,
				out List<TargetData>? targetTypes,
				diagnosticReceiver
			);

			CopyFromCompilationData compilation = context.Compilation;

			bool hasTarget = targetTypes?.Count > 0;

			HashSet<string> set = new();
			List<(int order, PatternData pattern)> notSortedPatterns = new();

			foreach (AttributeData attr in attributes)
			{
				if (!SymbolEqualityComparer.Default.Equals(attr.AttributeClass, context.Compilation.PatternAttribute))
				{
					continue;
				}

				Location? location = GetParentLocation(attr);
				bool hasTargetOnCurrentDeclaration =
					location is not null &&
					attributes.Any(attr => IsCopyFromAttribute(attr, compilation) && GetParentLocation(attr) == location);

				if (AnalyzePattern(
					context.Symbol,
					attr,
					set,
					hasTarget,
					hasTargetOnCurrentDeclaration,
					out string? pattern,
					out string? replacement,
					out int order,
					diagnosticReceiver)
				)
				{
					notSortedPatterns.Add((order, new PatternData(pattern, replacement)));
				}
			}

			PatternData[]? patterns = SortByOrder(notSortedPatterns);

			if (!isValid)
			{
				data = default;
				return false;
			}

			data = new CopyFromTypeData(
				context.Node!,
				context.Compilation,
				context.Symbol,
				context.SemanticModel,
				targetTypes!.ToArray(),
				dependencies?.ToArray(),
				patterns,
				attributes: attributes
			);

			return true;
		}

		/// <inheritdoc/>
		protected override IEnumerable<CSharpSyntaxNode>? GetCandidateNodes(IDurianSyntaxReceiver syntaxReceiver)
		{
			if (syntaxReceiver is not CopyFromSyntaxReceiver sr)
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
		protected override bool TryCreateContext(in SyntaxValidationContext validationContext, [NotNullWhen(true)] out CopyFromTypeContext context)
		{
			if (validationContext.TargetCompilation is not CopyFromCompilationData compilation ||
				validationContext.Symbol is not INamedTypeSymbol symbol ||
				validationContext.Node is not TypeDeclarationSyntax node)
			{
				context = default;
				return false;
			}

			context = new(compilation, validationContext.SemanticModel, symbol, node, validationContext.CancellationToken);
			return true;
		}
	}
}
