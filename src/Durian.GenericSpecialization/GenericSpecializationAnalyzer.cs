// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using static Durian.Analysis.GenericSpecialization.GenSpecDiagnostics;
using System.Threading;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

#if !MAIN_PACKAGE

using Microsoft.CodeAnalysis.Diagnostics;

#endif

namespace Durian.Analysis.GenericSpecialization
{
	/// <summary>
	/// Analyzes classes marked by the <see cref="GenericSpecializationAttribute"/>.
	/// </summary>
#if !MAIN_PACKAGE

	[DiagnosticAnalyzer(LanguageNames.CSharp)]
#endif
#pragma warning disable RS1001 // Missing diagnostic analyzer attribute.

	public sealed partial class GenericSpecializationAnalyzer : DurianAnalyzer<GenSpecCompilationData>
#pragma warning restore RS1001 // Missing diagnostic analyzer attribute.
	{
		/// <summary>
		/// Analyzes classes marked by the <see cref="GenericSpecializationAttribute"/> and reports appropriate <see cref="Diagnostic"/>s.
		/// </summary>
		public static class WithDiagnostics
		{
			public static bool Analyze(
				IDiagnosticReceiver diagnosticReceiver,
				INamedTypeSymbol symbol,
				GenSpecCompilationData compilation,
				CancellationToken cancellationToken = default
			)
			{
			}
		}

		/// <inheritdoc/>
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(
			DUR0202_TargetClassMustBeMarkedWithAllowSpecializationAttribute,
			DUR0205_SpecializationMustInheritMainImplementation,
			DUR0206_SpecializationMustImplementInterface,
			DUR0207_SpecializationCannotBeAbstractOrStatic,
			DUR0209_TargetGenericClassMustBePartial,
			DUR0211_TargetGenericClassCannotBeAbstractOrStatic
		);

		/// <summary>
		/// Initializes a new instance of the <see cref="GenericSpecializationAnalyzer"/> class.
		/// </summary>
		public GenericSpecializationAnalyzer()
		{
		}

		/// <summary>
		/// Determines whether an <see cref="GenericSpecializationAttribute"/> is contained withing the specified collection of <paramref name="attributes"/>.
		/// </summary>
		/// <param name="attributes">A collection of <see cref="AttributeData"/> to analyze.</param>
		/// <param name="compilation">Current <see cref="GenSpecCompilationData"/>.</param>
		/// <param name="attribute"><see cref="AttributeData"/> that represents the <see cref="GenericSpecializationAttribute"/> that was found. Included only if the method return <see langword="true"/>.</param>
		public static bool HasGenericSpecializationAttribute(
			IEnumerable<AttributeData> attributes,
			GenSpecCompilationData compilation,
			[NotNullWhen(true)] out AttributeData? attribute
		)
		{
			foreach (AttributeData attr in attributes)
			{
				if (SymbolEqualityComparer.Default.Equals(attr.AttributeClass, compilation.GenericSpecializationAttribute))
				{
					attribute = attr;
					return true;
				}
			}

			attribute = null;
			return false;
		}

		/// <inheritdoc cref="HasGenericSpecializationAttribute(INamedTypeSymbol, GenSpecCompilationData, out AttributeData?, out AttributeData[])"/>
		public static bool HasGenericSpecializationAttribute(INamedTypeSymbol symbol, GenSpecCompilationData compilation)
		{
			IEnumerable<AttributeData> attributes = symbol.GetAttributes();
			return HasGenericSpecializationAttribute(attributes, compilation, out _);
		}

		/// <inheritdoc cref="HasGenericSpecializationAttribute(INamedTypeSymbol, GenSpecCompilationData, out AttributeData?, out AttributeData[])"/>
		public static bool HasGenericSpecializationAttribute(
			INamedTypeSymbol symbol,
			GenSpecCompilationData compilation,
			[NotNullWhen(true)] out AttributeData? attribute
		)
		{
			IEnumerable<AttributeData> attributes = symbol.GetAttributes();
			return HasGenericSpecializationAttribute(attributes, compilation, out attribute);
		}

		/// <summary>
		/// Determines whether the specified <paramref name="symbol"/> has an <see cref="AllowSpecializationAttribute"/> applied.
		/// </summary>
		/// <param name="symbol"><see cref="INamedTypeSymbol"/> to check if has an <see cref="AllowSpecializationAttribute"/> applied.</param>
		/// <param name="compilation">Current <see cref="GenSpecCompilationData"/>.</param>
		/// <param name="attribute"><see cref="AttributeData"/> that represents the <see cref="GenericSpecializationAttribute"/> that was found.</param>
		/// <param name="attributes">An array of <see cref="AttributeData"/>s of the specified <paramref name="symbol"/>. Included only if the method returns <see langword="true"/>.</param>
		public static bool HasGenericSpecializationAttribute(
			INamedTypeSymbol symbol,
			GenSpecCompilationData compilation,
			[NotNullWhen(true)] out AttributeData? attribute,
			[NotNullWhen(true)] out AttributeData[]? attributes
		)
		{
			IEnumerable<AttributeData> collection = symbol.GetAttributes();
			bool isValid = HasGenericSpecializationAttribute(collection, compilation, out attribute);
			attributes = isValid ? collection.ToArray() : null;
			return isValid;
		}

		public static bool ShouldAnalyze(
					ISymbol symbol,
					GenSpecCompilationData compilation,
					IEnumerable<AttributeData>? attributes = null
				)
		{
		}

		public static bool ShouldAnalyze(
					ISymbol symbol,
					GenSpecCompilationData compilation,
					out AttributeData[]? attributes
				)
		{
		}

		/// <inheritdoc/>
		public override void Register(IDurianAnalysisContext context, GenSpecCompilationData compilation)
		{
			context.RegisterSymbolAction(context => AnalyzeSymbol(context, compilation), SymbolKind.NamedType);
		}

		/// <inheritdoc/>
		protected override GenSpecCompilationData CreateCompilation(CSharpCompilation compilation)
		{
			return new GenSpecCompilationData(compilation);
		}

		private static void AnalyzeSymbol(SymbolAnalysisContext context, GenSpecCompilationData compilation)
		{
			if (context.Symbol is not INamedTypeSymbol t || t.TypeKind != TypeKind.Class)
			{
				return;
			}

			ContextualDiagnosticReceiver<SymbolAnalysisContext> diagnosticReceiver = DiagnosticReceiverFactory.Symbol(context);
			WithDiagnostics.Analyze(diagnosticReceiver, t, compilation, context.CancellationToken);
		}
	}
}
