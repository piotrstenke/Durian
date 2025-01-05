using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using static Durian.Analysis.CopyFrom.CopyFromDiagnostics;

namespace Durian.Analysis.CopyFrom;

public partial class CopyFromAnalyzer
{
	internal static bool AnalyzePattern(
		ISymbol symbol,
		AttributeData patternAttribute,
		HashSet<string> patterns,
		bool hasAnyTarget,
		bool hasTargetOnSameDeclaration,
		[NotNullWhen(true)] out string? pattern,
		[NotNullWhen(true)] out string? replacement,
		out int order,
		IDiagnosticReceiver diagnosticReceiver
	)
	{
		Location? location = null;
		bool isValid = true;

		if (hasAnyTarget)
		{
			if (!hasTargetOnSameDeclaration)
			{
				location = patternAttribute.GetLocation();
				diagnosticReceiver.ReportDiagnostic(DUR0219_PatternOnDifferentDeclaration, location, symbol);
				isValid = false;
			}
		}
		else
		{
			location = patternAttribute.GetLocation();
			diagnosticReceiver.ReportDiagnostic(DUR0215_RedundantPatternAttribute, location, symbol);
			isValid = false;
		}

		if (HasValidRegexPattern(patternAttribute, out pattern, out replacement, out order))
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

	internal static Location? GetParentLocation(AttributeData attribute)
	{
		return attribute.ApplicationSyntaxReference?.GetSyntax()?.Parent?.Parent?.GetLocation();
	}

	internal static bool HasValidRegexPattern(
		AttributeData attribute,
		[NotNullWhen(true)] out string? pattern,
		[NotNullWhen(true)] out string? replacement,
		out int order
	)
	{
		ImmutableArray<TypedConstant> arguments = attribute.ConstructorArguments;

		if (arguments.Length < 2 || arguments[0].Value is not string p || string.IsNullOrEmpty(p) || arguments[1].Value is not string r)
		{
			pattern = default;
			replacement = default;
			order = default;
			return false;
		}

		pattern = p;
		replacement = r;
		order = attribute.GetNamedArgumentValue<int>(PatternAttributeProvider.Order);

		return true;
	}

	internal static void ReportEquivalentPattern(IDiagnosticReceiver diagnosticReceiver, Location? location, ISymbol symbol)
	{
		diagnosticReceiver.ReportDiagnostic(DUR0216_EquivalentPatternAttribute, location, symbol);
	}

	internal static PatternData[]? SortByOrder(List<(int order, PatternData pattern)> patterns)
	{
		if (patterns.Count == 0)
		{
			return default;
		}

		PatternData[] array = new PatternData[patterns.Count];

		patterns.Sort((a, b) =>
		{
			if (a.order == 0 || a.order < b.order)
			{
				return -1;
			}
			else if (a.order == b.order)
			{
				return 0;
			}

			return 1;
		});

		for (int i = 0; i < patterns.Count; i++)
		{
			array[i] = patterns[i].pattern;
		}

		return array;
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
			Location? location = GetParentLocation(pattern);
			bool hasTargetOnCurrentDeclaration = location is not null && attributes.Any(attr => IsCopyFromAttribute(attr, compilation) && GetParentLocation(attr) == location);

			AnalyzePattern(symbol, pattern, set, hasTarget, hasTargetOnCurrentDeclaration, out _, out _, out _, diagnosticReceiver);
		}
	}

	private static bool AnalyzePattern(
		ISymbol symbol,
		CopyFromCompilationData compilation,
		AttributeSyntax attrSyntax,
		IDiagnosticReceiver diagnosticReceiver
	)
	{
		Location currentLocation = attrSyntax.GetLocation();
		Location? declarationLocation = attrSyntax.Parent?.Parent?.GetLocation();

		AttributeData? currentData = null;
		HashSet<string> patterns = new();
		bool hasAnyTarget = false;
		bool hasTargetOnCurrentDeclaration = false;

		foreach (AttributeData attribute in symbol.GetAttributes())
		{
			if (SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, compilation.PatternAttribute))
			{
				if (currentData is null && attribute.GetLocation() == currentLocation)
				{
					currentData = attribute;
				}
				else if (attribute.ConstructorArguments[0].Value is string pattern && !string.IsNullOrEmpty(pattern))
				{
					patterns.Add(pattern);
				}

				continue;
			}

			if (!hasTargetOnCurrentDeclaration && IsCopyFromAttribute(attribute, compilation))
			{
				hasAnyTarget = true;

				if (declarationLocation is not null && GetParentLocation(attribute) == declarationLocation)
				{
					hasTargetOnCurrentDeclaration = true;
				}
			}
		}

		if (currentData is null)
		{
			return false;
		}

		return AnalyzePattern(symbol, currentData, patterns, hasAnyTarget, hasTargetOnCurrentDeclaration, out _, out _, out _, diagnosticReceiver);
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
