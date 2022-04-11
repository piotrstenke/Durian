// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Durian.Analysis.Cache;
using Durian.Analysis.Data;
using Durian.Analysis.Filters;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Durian.Analysis.CopyFrom.CopyFromAnalyzer;

namespace Durian.Analysis.CopyFrom.Methods
{
	/// <summary>
	/// Filtrates and validates <see cref="MethodDeclarationSyntax"/>es collected by a <see cref="CopyFromSyntaxReceiver"/>.
	/// </summary>
	public sealed class CopyFromMethodFilter : CachedSyntaxValidator<ICopyFromMember, CopyFromMethodContext>
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="CopyFromMethodFilter"/> class.
		/// </summary>
		public CopyFromMethodFilter()
		{
		}

		/// <inheritdoc/>
		public override bool ValidateAndCreate(in CopyFromMethodContext context, out IMemberData? data, IDiagnosticReceiver diagnosticReceiver)
		{
			CopyFromMethodContext newContext = context;

			bool isValid = AnalyzeMethodWithoutPattern(
				ref newContext,
				out ImmutableArray<AttributeData> attributes,
				out IMethodSymbol? targetMethod,
				diagnosticReceiver
			);

			bool hasTarget = targetMethod is not null;
			List<PatternData> patterns = new(attributes.Length);

			HashSet<string> set = new();

			foreach (AttributeData attr in attributes)
			{
				if (SymbolEqualityComparer.Default.Equals(attr.AttributeClass, newContext.Compilation.PatternAttribute) &&
					AnalyzePattern(newContext.Symbol, attr, set, hasTarget, out string? pattern, out string? replacement, diagnosticReceiver))
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
				newContext.Node!,
				newContext.Compilation,
				newContext.Symbol,
				newContext.SemanticModel,
				targetMethod!,
				patterns.ToArray(),
				attributes: attributes
			);

			return true;
		}

		/// <inheritdoc/>
		public override bool ValidateAndCreate(in CopyFromMethodContext context, out IMemberData? data)
		{
			CopyFromMethodContext newContext = context;

			bool isValid = AnalyzeMethodWithoutPattern(
				ref newContext,
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
					if (SymbolEqualityComparer.Default.Equals(attr.AttributeClass, newContext.Compilation.PatternAttribute) &&
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
				newContext.Node!,
				newContext.Compilation,
				newContext.Symbol,
				newContext.SemanticModel,
				targetMethod!,
				patterns?.ToArray(),
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

			if (sr.CandidateMethods.Count == 0)
			{
				return default;
			}

			return sr.CandidateMethods;
		}

		/// <inheritdoc/>
		protected override bool TryCreateContext(in SyntaxValidationContext validationContext, [NotNullWhen(true)] out CopyFromMethodContext context)
		{
			if (validationContext.TargetCompilation is not CopyFromCompilationData compilation ||
				validationContext.Symbol is not IMethodSymbol symbol ||
				validationContext.Node is not MethodDeclarationSyntax node)
			{
				context = default;
				return false;
			}

			context = new(compilation, validationContext.SemanticModel, symbol, node, validationContext.CancellationToken);
			return true;
		}
	}
}
