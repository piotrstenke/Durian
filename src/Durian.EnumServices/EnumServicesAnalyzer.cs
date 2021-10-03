// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System.Diagnostics.CodeAnalysis;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Durian.Analysis.Cache;

using Services = Durian.EnumServices;
using static Durian.Analysis.EnumServices.EnumServicesDiagnostics;
using System;

namespace Durian.Analysis.EnumServices
{
	/// <summary>
	/// Analyzes usage of the <see cref="EnumServicesAttribute"/>.
	/// </summary>
#if !MAIN_PACKAGE

	[DiagnosticAnalyzer(LanguageNames.CSharp)]
#endif
#pragma warning disable RS1001 // Missing diagnostic analyzer attribute.
	public partial class EnumServicesAnalyzer : DurianAnalyzer<EnumServicesCompilationData>, ICachedAnalyzerInfo<EnumServicesData>
#pragma warning restore RS1001 // Missing diagnostic analyzer attribute.
	{
		/// <inheritdoc/>
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(
			DUR0201_InvalidEnumValue,
			DUR0202_InvalidName,
			DUR0203_NotStaticOrPartial,
			DUR0204_EnumAccessbilityLessThanGeneratedExtension,
			DUR0205_GeneratedAcessibilityGreatedThanExisting,
			DUR0206_FlagsOnlyMethodsOnNonFlagEnum,
			DUR0207_NoEnumInScope,
			DUR0208_MemberAlreadyExists
		);

		/// <summary>
		/// Determines whether the specified <paramref name="services"/> value is valid.
		/// </summary>
		/// <param name="services">Value to check if is valid.</param>
		/// <returns></returns>
		public static bool IsValidServicesValue(Services services)
		{
			return services != 0 && (services & Services.All) != 0;
		}

		/// <inheritdoc/>
		public override void Register(IDurianAnalysisContext context, EnumServicesCompilationData compilation)
		{
			context.RegisterSymbolAction(context => AnalyzeSymbol(context, compilation), SymbolKind.NamedType);
			context.RegisterSymbolAction(context => )
		}

		/// <inheritdoc/>
		protected override EnumServicesCompilationData CreateCompilation(CSharpCompilation compilation, IDiagnosticReceiver diagnosticReceiver)
		{
			return new EnumServicesCompilationData(compilation);
		}

		private static void AnalyzeSymbol(SymbolAnalysisContext context, EnumServicesCompilationData compilation)
		{
			if ()
		}

		private static bool ShouldAnalyze(ISymbol symbol)
		{
			return symbol is INamedTypeSymbol t && t.TypeKind == TypeKind.Enum;
		}
	}
}
