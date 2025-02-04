﻿using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Durian.Analysis.Cache;
using Durian.Analysis.Data;
using Durian.Analysis.Filtering;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Durian.Analysis.CopyFrom.CopyFromAnalyzer;

namespace Durian.Analysis.CopyFrom.Methods;

/// <summary>
/// Filters and validates <see cref="MethodDeclarationSyntax"/>es collected by a <see cref="CopyFromSyntaxReceiver"/>.
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
	public override bool TryGetContext(in PreValidationContext validationContext, [NotNullWhen(true)] out CopyFromMethodContext context)
	{
		SemanticModel semanticModel = validationContext.TargetCompilation.Compilation.GetSemanticModel(validationContext.Node.SyntaxTree);

		ISymbol? symbol;

		if (validationContext.Node is LambdaExpressionSyntax lambda)
		{
			symbol = semanticModel.GetSymbolInfo(lambda, validationContext.CancellationToken).Symbol;
		}
		else
		{
			symbol = semanticModel.GetDeclaredSymbol(validationContext.Node, validationContext.CancellationToken);
		}

		if (symbol is null)
		{
			context = default;
			return false;
		}

		return TryCreateContext(validationContext.ToSyntaxContext(semanticModel, symbol), out context);
	}

	/// <inheritdoc/>
	public override bool ValidateAndCreate(in CopyFromMethodContext context, out IMemberData? data, IDiagnosticReceiver diagnosticReceiver)
	{
		CopyFromMethodContext newContext = context;

		bool isValid = AnalyzeMethodWithoutPattern(
			ref newContext,
			out ImmutableArray<AttributeData> attributes,
			out List<IMethodSymbol>? dependencies,
			out TargetMethodData? targetMethod,
			diagnosticReceiver
		);

		bool hasTarget = targetMethod is not null;

		HashSet<string> set = new();
		List<(int order, PatternData pattern)> notSortedPatterns = new();

		foreach (AttributeData attr in attributes)
		{
			if (SymbolEqualityComparer.Default.Equals(attr.AttributeClass, newContext.Compilation.PatternAttribute) &&
				AnalyzePattern(newContext.Symbol, attr, set, hasTarget, hasTarget, out string? pattern, out string? replacement, out int order, diagnosticReceiver))
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

		data = new CopyFromMethodData(
			newContext.AsMethod!,
			newContext.Compilation,
			targetMethod!,
			new CopyFromMethodData.Properties
			{
				Patterns = patterns,
				Dependencies = dependencies?.ToArray(),
				Attributes = attributes,
				SemanticModel = newContext.SemanticModel,
				Symbol = newContext.Symbol
			}
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
			out List<IMethodSymbol>? dependencies,
			out TargetMethodData? targetMethod
		);

		bool hasTarget = targetMethod is not null;
		PatternData[]? patterns = null;

		if (hasTarget)
		{
			List<(int order, PatternData pattern)> notSortedPatterns = new();
			HashSet<string> set = new();

			foreach (AttributeData attr in attributes)
			{
				if (SymbolEqualityComparer.Default.Equals(attr.AttributeClass, newContext.Compilation.PatternAttribute) &&
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

		data = new CopyFromMethodData(
			newContext.AsMethod!,
			newContext.Compilation,
			targetMethod!,
			new CopyFromMethodData.Properties
			{
				Patterns = patterns,
				Dependencies = dependencies?.ToArray(),
				Attributes = attributes,
				SemanticModel = newContext.SemanticModel,
				Symbol = newContext.Symbol
			}
		);

		return true;
	}

	/// <inheritdoc/>
	protected override IEnumerable<SyntaxNode>? GetCandidateNodes(IDurianSyntaxReceiver syntaxReceiver)
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
			validationContext.Symbol is not IMethodSymbol symbol
		)
		{
			context = default;
			return false;
		}

		context = new(compilation, validationContext.SemanticModel, symbol, validationContext.Node, validationContext.CancellationToken);
		return true;
	}
}
