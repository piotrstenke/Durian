// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Durian.Analysis.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using static Durian.Analysis.CopyFrom.CopyFromDiagnostics;

namespace Durian.Analysis.CopyFrom
{
	public partial class CopyFromAnalyzer
	{
		internal static bool AnalyzePattern(
			ISymbol symbol,
			AttributeData patternAttribute,
			HashSet<string> patterns,
			bool hasTarget,
			[NotNullWhen(true)] out string? pattern,
			[NotNullWhen(true)] out string? replacement,
			IDiagnosticReceiver diagnosticReceiver
		)
		{
			Location? location = null;
			bool isValid = true;

			if (!hasTarget)
			{
				location = patternAttribute.GetLocation();
				diagnosticReceiver.ReportDiagnostic(DUR0215_RedundantPatternAttribute, location, symbol);
				isValid = false;
			}

			if (HasValidRegexPattern(patternAttribute, out pattern, out replacement))
			{
				if (!patterns.Add(pattern))
				{
					diagnosticReceiver.ReportDiagnostic(DUR0216_EquivalentPatternAttribute, location ?? patternAttribute.GetLocation(), symbol);
					isValid = false;
				}
			}
			else
			{
				diagnosticReceiver.ReportDiagnostic(DUR0214_InvalidPatternAttributeSpecified, location ?? patternAttribute.GetLocation(), symbol);
				isValid = false;
			}

			return isValid;
		}

		internal static bool HasValidRegexPattern(
			AttributeData attribute,
			[NotNullWhen(true)] out string? pattern,
			[NotNullWhen(true)] out string? replacement
		)
		{
			ImmutableArray<TypedConstant> arguments = attribute.ConstructorArguments;

			if (arguments.Length < 2 || arguments[0].Value is not string p || string.IsNullOrEmpty(p) || arguments[1].Value is not string r)
			{
				pattern = default;
				replacement = default;
				return false;
			}

			pattern = p;
			replacement = r;
			return true;
		}

		internal static void ReportEquivalentPattern(IDiagnosticReceiver diagnosticReceiver, Location? location, ISymbol symbol)
		{
			diagnosticReceiver.ReportDiagnostic(DUR0216_EquivalentPatternAttribute, location, symbol);
		}

		private static void AnalyzePattern(
			ISymbol symbol,
			CopyFromCompilationData compilation,
			ImmutableArray<AttributeData> attributes,
			bool hasTarget,
			IDiagnosticReceiver diagnosticReceiver
		)
		{
			AttributeData[] patternAttributes = GetAttributes(attributes, compilation.PatternAttribute!);

			if (patternAttributes.Length == 0)
			{
				return;
			}

			HashSet<string> set = new();

			foreach (AttributeData pattern in patternAttributes)
			{
				AnalyzePattern(symbol, pattern, set, hasTarget, out _, out _, diagnosticReceiver);
			}
		}

		private static bool AnalyzePattern(
			ISymbol symbol,
			CopyFromCompilationData compilation,
			AttributeSyntax attrSyntax,
			IDiagnosticReceiver diagnosticReceiver
		)
		{
			AttributeData? currentData = null;
			HashSet<string> patterns = new();
			bool hasCopyFrom = false;

			foreach (AttributeData attribute in symbol.GetAttributes())
			{
				if (SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, compilation.PatternAttribute))
				{
					if (currentData is null && attribute.ApplicationSyntaxReference?.Span == attrSyntax.Span)
					{
						currentData = attribute;
					}
					else if (attribute.ConstructorArguments[0].Value is string pattern && !string.IsNullOrEmpty(pattern))
					{
						patterns.Add(pattern);
					}
				}
				else if (
					!hasCopyFrom &&
					(SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, compilation.CopyFromMethodAttribute) ||
					SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, compilation.CopyFromTypeAttribute)))
				{
					hasCopyFrom = true;
				}
			}

			if (currentData is null)
			{
				return false;
			}

			return AnalyzePattern(symbol, currentData, patterns, hasCopyFrom, out _, out _, diagnosticReceiver);
		}

		private static void AnalyzePatternAttribute(SyntaxNodeAnalysisContext context, CopyFromCompilationData compilation, AttributeSyntax attr, CSharpSyntaxNode member)
		{
			if (context.SemanticModel.GetDeclaredSymbol(member) is ISymbol symbol)
			{
				DiagnosticReceiver.Contextual<SyntaxNodeAnalysisContext> diagnosticReceiver = DiagnosticReceiver.Factory.SyntaxNode(context);
				AnalyzePattern(symbol, compilation, attr, diagnosticReceiver);
			}
		}
	}
}
