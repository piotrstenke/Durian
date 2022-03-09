// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Durian.Analysis.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Collections.Generic;
using System.Linq;

using static Durian.Analysis.DurianDiagnostics;

namespace Durian.Analysis
{
	/// <summary>
	/// Analyzer that checks if the <see cref="PartialNameAttribute"/> is used properly.
	/// </summary>
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public sealed class PartialNameAnalyzer : DurianAnalyzer
	{
		/// <inheritdoc/>
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(
			DUR0006_PartialNameAttributeNotOnPartial,
			DUR0009_DuplicatePartialPart
		);

		/// <summary>
		/// Initializes a new instance of the <see cref="PartialNameAnalyzer"/> class.
		/// </summary>
		public PartialNameAnalyzer()
		{
		}

		/// <inheritdoc/>
		public override void Register(IDurianAnalysisContext context)
		{
			context.RegisterCompilationStartAction(context =>
			{
				INamedTypeSymbol? partialNameAttribute = context.Compilation.GetTypeByMetadataName($"{nameof(Durian)}.{nameof(PartialNameAttribute)}");

				if(partialNameAttribute is null)
				{
					return;
				}

				context.RegisterSymbolAction(context => Analyze(context, partialNameAttribute), SymbolKind.NamedType);
			});
		}

		private static void Analyze(SymbolAnalysisContext context, INamedTypeSymbol partialNameAttribute)
		{
			if(context.Symbol is not INamedTypeSymbol type || type.TypeKind is not TypeKind.Class and not TypeKind.Struct and not TypeKind.Interface)
			{
				return;
			}

			HashSet<string> names = new();

			AttributeData[] attributes = context.Symbol.GetAttributes(partialNameAttribute).ToArray();

			if(attributes.Length == 0)
			{
				return;
			}

			if(!type.IsPartial())
			{
				foreach (AttributeData attribute in attributes)
				{
					context.ReportDiagnostic(Diagnostic.Create(DUR0006_PartialNameAttributeNotOnPartial, attribute.GetLocation(), type));
				}
			}

			foreach (AttributeData attribute in attributes)
			{
				if(attribute.TryGetConstructorArgumentValue(0, out string? value) && value is not null && !names.Add(value))
				{
					context.ReportDiagnostic(Diagnostic.Create(DUR0009_DuplicatePartialPart, attribute.GetLocation(), type));
				}
			}
		}
	}
}