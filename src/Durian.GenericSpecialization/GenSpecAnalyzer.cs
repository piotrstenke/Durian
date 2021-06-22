// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

//using Microsoft.CodeAnalysis;
//using Microsoft.CodeAnalysis.CSharp;
//using System.Collections.Immutable;
//using System.Diagnostics.CodeAnalysis;
//using System.Threading;
//using static Durian.Analysis.GenericSpecialization.GenSpecDiagnostics;
//using Durian.Analysis.Extensions;
//using System.Linq;
//using System.Collections.Generic;

//#if !MAIN_PACKAGE

//using Microsoft.CodeAnalysis.Diagnostics;

//#endif

//namespace Durian.Analysis.GenericSpecialization
//{
//	/// <summary>
//	/// Analyzes classes marked by the <see cref="AllowSpecializationAttribute"/>.
//	/// </summary>
//#if !MAIN_PACKAGE

//	[DiagnosticAnalyzer(LanguageNames.CSharp)]
//#endif
//#pragma warning disable RS1001 // Missing diagnostic analyzer attribute.

//	public sealed partial class AllowSpecializationAnalyzer : DurianAnalyzer<GenSpecCompilationData>
//#pragma warning restore RS1001 // Missing diagnostic analyzer attribute.
//	{
//		/// <inheritdoc/>
//		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(
//			DUR0201_NonGenericTypesCannotUseTheAllowSpecializationAttribute,
//			DUR0209_TargetGenericClassMustBePartial,
//			DUR0210_ContainingTypesMustBePartial,
//			DUR0211_TargetGenericClassCannotBeAbstractOrStatic
//		);

//		/// <summary>
//		/// Initializes a new instance of the <see cref="AllowSpecializationAnalyzer"/> class.
//		/// </summary>
//		public AllowSpecializationAnalyzer()
//		{
//		}

//		/// <inheritdoc/>
//		public override void Register(IDurianAnalysisContext context, GenSpecCompilationData compilation)
//		{
//			context.RegisterSymbolAction(context => AnalyzeSymbol(context, compilation), SymbolKind.NamedType);
//		}

//		/// <inheritdoc/>
//		protected override GenSpecCompilationData CreateCompilation(CSharpCompilation compilation)
//		{
//			return new GenSpecCompilationData(compilation);
//		}

//		private static void AnalyzeSymbol(SymbolAnalysisContext context, GenSpecCompilationData compilation)
//		{
//			if (context.Symbol is not INamedTypeSymbol t || t.TypeKind != TypeKind.Class)
//			{
//				return;
//			}

//			ContextualDiagnosticReceiver<SymbolAnalysisContext> diagnosticReceiver = DiagnosticReceiverFactory.Symbol(context);
//			WithDiagnostics.Analyze(diagnosticReceiver, (INamedTypeSymbol)context.Symbol, compilation, context.CancellationToken);
//		}

//		private static bool HasPartialKeyword(INamedTypeSymbol symbol, CancellationToken cancellationToken)
//		{
//			return symbol.GetModifiers(cancellationToken).Any(m => m.IsKind(SyntaxKind.PartialKeyword));
//		}
//	}

//	public partial class AllowSpecializationAnalyzer
//	{
//		/// <summary>
//		/// Contains <see langword="static"/> methods that perform the most basic GenSpec-related analysis and report <see cref="Diagnostic"/>s if the analyzed <see cref="INamedTypeSymbol"/> is not valid.
//		/// </summary>
//		public static class WithDiagnostics
//		{
//			/// <summary>
//			/// Performs analysis of the <paramref name="symbol"/> and reports <see cref="Diagnostic"/>s if the <paramref name="symbol"/> is not valid.
//			/// </summary>
//			/// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> that is used to report <see cref="Diagnostic"/>s.</param>
//			/// <param name="symbol"><see cref="ISymbol"/> to analyze.</param>
//			/// <param name="compilation">Current <see cref="GenSpecCompilationData"/>.</param>
//			/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
//			/// <returns><see langword="true"/> if the <paramref name="symbol"/> is valid, otherwise <see langword="false"/>.</returns>
//			public static bool Analyze(IDiagnosticReceiver diagnosticReceiver, INamedTypeSymbol symbol, GenSpecCompilationData compilation, CancellationToken cancellationToken = default)
//			{
//				if (symbol.TypeKind != TypeKind.Class || symbol.TypeParameters.Length == 0)
//				{
//					return false;
//				}

//				INamedTypeSymbol[] containingTypes = symbol.GetContainingTypeSymbols().ToArray();
//				IEnumerable<AttributeData> attributes = symbol.GetAttributes();

//				bool isValid = HasAllowSpecializationAttribute(symbol, attributes, compilation);
//				isValid &= AnalyzeModifiers(diagnosticReceiver, symbol, cancellationToken);
//				isValid &= AnalyzeContainingTypes(diagnosticReceiver, symbol, cancellationToken);

//				return isValid;
//			}

//			public static bool AnalyzeAttributes(IDiagnosticReceiver diagnosticReceiver, INamedTypeSymbol symbol, IEnumerable<AttributeData> attributes, INamedTypeSymbol[] containingTypes)
//			{
//				foreach (AttributeData attribute in collection)
//				{
//				}
//			}

//			public static bool AnalyzeContainingTypes(IDiagnosticReceiver diagnosticReceiver, INamedTypeSymbol[] containingTypes, CancellationToken cancellationToken = default)
//			{
//				bool isValid = true;

//				if (containingTypes.Length > 0)
//				{
//					foreach (INamedTypeSymbol type in containingTypes)
//					{
//						if (!HasPartialKeyword(type, cancellationToken))
//						{
//							diagnosticReceiver.ReportDiagnostic(DUR0210_ContainingTypesMustBePartial, type);
//							isValid = false;
//						}
//					}
//				}

//				return isValid;
//			}

//			public static bool AnalyzeModifiers(IDiagnosticReceiver diagnosticReceiver, INamedTypeSymbol symbol, CancellationToken cancellationToken = default)
//			{
//				bool hasPartial = false;
//				bool hasInvalidModifiers = false;

//				foreach (SyntaxToken modifier in symbol.GetModifiers(cancellationToken))
//				{
//					if (modifier.IsKind(SyntaxKind.StaticKeyword) || modifier.IsKind(SyntaxKind.AbstractKeyword))
//					{
//						diagnosticReceiver.ReportDiagnostic(DUR0211_TargetGenericClassCannotBeAbstractOrStatic);
//						hasInvalidModifiers = true;
//					}
//					else if (modifier.IsKind(SyntaxKind.PartialKeyword))
//					{
//						hasPartial = true;
//					}
//				}

//				if (!hasPartial)
//				{
//					diagnosticReceiver.ReportDiagnostic(DUR0209_TargetGenericClassMustBePartial);
//				}

//				return hasPartial && !hasInvalidModifiers;
//			}

//			public static bool HasAllowSpecializationAttribute(INamedTypeSymbol symbol, IEnumerable<AttributeData> attributes, GenSpecCompilationData compilation)
//			{
//				foreach (AttributeData attr in attributes)
//				{
//					if (SymbolEqualityComparer.Default.Equals(attr.AttributeClass, compilation.AllowSpecializationAttribute))
//					{
//						return true;
//					}
//				}

//				return false;
//			}
//		}
//	}
//}
