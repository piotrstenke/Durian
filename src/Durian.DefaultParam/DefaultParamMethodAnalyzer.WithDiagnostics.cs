﻿using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using Durian.Generator.Data;
using Durian.Generator.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.Generator.DefaultParam
{
	public partial class DefaultParamMethodAnalyzer
	{
		/// <summary>
		/// Contains static methods that analyze methods with type parameters marked using the <see cref="DefaultParamAttribute"/> and report <see cref="Diagnostic"/>s for the invalid onces.
		/// </summary>
		public static new class WithDiagnostics
		{
			/// <summary>
			/// Fully analyzes the specified <paramref name="symbol"/>.
			/// </summary>
			/// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> that is used to report <see cref="Diagnostic"/>s.</param>
			/// <param name="symbol"><see cref="IMethodSymbol"/> to analyze.</param>
			/// <param name="compilation">Current <see cref="DefaultParamCompilationData"/>.</param>
			/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
			/// <returns><see langword="true"/> if the <paramref name="symbol"/> is valid, otherwise <see langword="false"/>.</returns>
			public static bool Analyze(IDiagnosticReceiver diagnosticReceiver, IMethodSymbol symbol, DefaultParamCompilationData compilation, CancellationToken cancellationToken = default)
			{
				TypeParameterContainer typeParameters = TypeParameterContainer.CreateFrom(symbol, compilation, cancellationToken);

				if (typeParameters.HasDefaultParams)
				{
					if (!AnalyzeAgaintsLocalFunction(diagnosticReceiver, symbol))
					{
						return false;
					}
				}
				else if (!symbol.IsOverride)
				{
					return false;
				}

				return AnalyzeCore(diagnosticReceiver, symbol, compilation, ref typeParameters, cancellationToken);
			}

			/// <summary>
			/// Analyzes, if the <paramref name="symbol"/> has valid <paramref name="typeParameters"/> when compared to the <paramref name="symbol"/>'s base method.
			/// </summary>
			/// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> that is used to report <see cref="Diagnostic"/>s.</param>
			/// <param name="symbol"><see cref="IMethodSymbol"/> to analyze.</param>
			/// <param name="typeParameters"><see cref="TypeParameterContainer"/> that contains type parameters to be analyzed.</param>
			/// <param name="compilation">Current <see cref="DefaultParamCompilationData"/>.</param>
			/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
			/// <returns><see langword="true"/> if the <paramref name="symbol"/> is valid, otherwise <see langword="false"/>.</returns>
			public static bool AnalyzeBaseMethodAndTypeParameters(
				IDiagnosticReceiver diagnosticReceiver,
				IMethodSymbol symbol,
				ref TypeParameterContainer typeParameters,
				DefaultParamCompilationData compilation,
				CancellationToken cancellationToken = default
			)
			{
				if (!symbol.IsOverride)
				{
					return AnalyzeTypeParameters(diagnosticReceiver, in typeParameters);
				}

				IMethodSymbol? baseMethod = symbol.OverriddenMethod;

				if (baseMethod is null)
				{
					return false;
				}

				if (IsDefaultParamGenerated(baseMethod, compilation))
				{
					diagnosticReceiver.ReportDiagnostic(DefaultParamDiagnostics.DUR0107_DoNotOverrideGeneratedMethods, symbol);
					return false;
				}

				TypeParameterContainer baseTypeParameters = GetBaseMethodTypeParameters(baseMethod, compilation, cancellationToken);

				if (DefaultParamAnalyzer.AnalyzeTypeParameters(in baseTypeParameters) && AnalyzeBaseMethodParameters(diagnosticReceiver, in typeParameters, in baseTypeParameters))
				{
					typeParameters = TypeParameterContainer.Combine(in typeParameters, in baseTypeParameters);
					return true;
				}

				return false;
			}

			/// <summary>
			/// Analyzes, if the <paramref name="symbol"/> is a local function.
			/// </summary>
			/// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> that is used to report <see cref="Diagnostic"/>s.</param>
			/// <param name="symbol"><see cref="IMethodSymbol"/> to analyze.</param>
			/// <returns><see langword="true"/> if the <paramref name="symbol"/> is valid (is not local function), otherwise <see langword="false"/>.</returns>
			public static bool AnalyzeAgaintsLocalFunction(IDiagnosticReceiver diagnosticReceiver, IMethodSymbol symbol)
			{
				if (symbol.MethodKind == MethodKind.LocalFunction)
				{
					ReportDiagnosticForLocalFunction(diagnosticReceiver, symbol);
					return false;
				}

				return true;
			}

			/// <summary>
			/// Analyzes, if the <paramref name="symbol"/> or either <see langword="partial"/> or <see langword="extern"/>.
			/// </summary>
			/// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> that is used to report <see cref="Diagnostic"/>s.</param>
			/// <param name="symbol"><see cref="IMethodSymbol"/> to analyze.</param>
			/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
			/// <returns><see langword="true"/> if the <paramref name="symbol"/> is valid (is not <see langword="partial"/> or <see langword="extern"/>), otherwise <see langword="false"/>.</returns>
			public static bool AnalyzeAgaintsPartialOrExtern(IDiagnosticReceiver diagnosticReceiver, IMethodSymbol symbol, CancellationToken cancellationToken = default)
			{
				if (symbol?.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax(cancellationToken) is not MethodDeclarationSyntax declaration)
				{
					return false;
				}

				return AnalyzeAgaintsPartialOrExtern(diagnosticReceiver, symbol, declaration);
			}

			/// <summary>
			/// Analyzes, if the <paramref name="symbol"/> or either <see langword="partial"/> or <see langword="extern"/>.
			/// </summary>
			/// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> that is used to report <see cref="Diagnostic"/>s.</param>
			/// <param name="symbol"><see cref="IMethodSymbol"/> to analyze.</param>
			/// <param name="declaration">Main <see cref="MethodDeclarationSyntax"/> of the <paramref name="symbol"/>.</param>
			/// <returns><see langword="true"/> if the <paramref name="symbol"/> is valid (is not <see langword="partial"/> or <see langword="extern"/>), otherwise <see langword="false"/>.</returns>
			public static bool AnalyzeAgaintsPartialOrExtern(IDiagnosticReceiver diagnosticReceiver, IMethodSymbol symbol, MethodDeclarationSyntax declaration)
			{
				if (symbol.IsExtern || symbol.IsPartial(declaration))
				{
					diagnosticReceiver.ReportDiagnostic(DefaultParamDiagnostics.DUR0102_MethodCannotBePartialOrExtern, symbol);
					return false;
				}

				return true;
			}

			/// <summary>
			/// Analyzes, if the signature of the <paramref name="symbol"/> is valid.
			/// </summary>
			/// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> that is used to report <see cref="Diagnostic"/>s.</param>
			/// <param name="symbol"><see cref="IMethodSymbol"/> to analyze the signature of.</param>
			/// <param name="typeParameters"><see cref="TypeParameterContainer"/> containing type parameters of the <paramref name="symbol"/>.</param>
			/// <param name="compilation">Current <see cref="DefaultParamCompilationData"/>.</param>
			/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
			/// <returns><see langword="true"/> if the signature of <paramref name="symbol"/> is valid, otherwise <see langword="false"/>.</returns>
			public static bool AnalyzeMethodSignature(IDiagnosticReceiver diagnosticReceiver, IMethodSymbol symbol, in TypeParameterContainer typeParameters, DefaultParamCompilationData compilation, CancellationToken cancellationToken = default)
			{
				return AnalyzeMethodSignature(diagnosticReceiver, symbol, in typeParameters, compilation, out _, cancellationToken);
			}

			/// <summary>
			/// Analyzes, if the signature of the <paramref name="symbol"/> is valid. If so, returns a <see cref="HashSet{T}"/> of indexes of type parameters with the <see cref="DefaultParamAttribute"/> applied for whom the <see langword="new"/> modifier should be applied.
			/// </summary>
			/// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> that is used to report <see cref="Diagnostic"/>s.</param>
			/// <param name="symbol"><see cref="IMethodSymbol"/> to analyze the signature of.</param>
			/// <param name="typeParameters"><see cref="TypeParameterContainer"/> containing type parameters of the <paramref name="symbol"/>.</param>
			/// <param name="compilation">Current <see cref="DefaultParamCompilationData"/>.</param>
			/// <param name="applyNew"><see langword="abstract"/><see cref="HashSet{T}"/> of indexes of type parameters with the <see cref="DefaultParamAttribute"/> applied for whom the <see langword="new"/> modifier should be applied. -or- <see langword="null"/> if the <paramref name="symbol"/> is not valid.</param>
			/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
			/// <returns><see langword="true"/> if the signature of <paramref name="symbol"/> is valid, otherwise <see langword="false"/>.</returns>
			public static bool AnalyzeMethodSignature(
				IDiagnosticReceiver diagnosticReceiver,
				IMethodSymbol symbol,
				in TypeParameterContainer typeParameters,
				DefaultParamCompilationData compilation,
				out HashSet<int>? applyNew,
				CancellationToken cancellationToken = default
			)
			{
				IParameterSymbol[] symbolParameters = symbol.Parameters.ToArray();
				CollidingMethod[] collidingMethods = GetCollidingMethods(symbol, compilation, symbolParameters.Length, typeParameters.Length, typeParameters.NumNonDefaultParam);

				if (collidingMethods.Length == 0)
				{
					applyNew = null;
					return true;
				}

				return AnalyzeCollidingMethods(
					diagnosticReceiver,
					symbol,
					in typeParameters,
					collidingMethods,
					symbolParameters,
					compilation.Configuration,
					cancellationToken,
					out applyNew
				);
			}

			/// <summary>
			/// Reports <see cref="Diagnostic"/>s for the specified <paramref name="localFunctionSymbol"/>.
			/// </summary>
			/// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> that is used to report <see cref="Diagnostic"/>s.</param>
			/// <param name="localFunctionSymbol"><see cref="IMethodSymbol"/> of a local function to report the <see cref="Diagnostic"/>s for.</param>
			public static void ReportDiagnosticForLocalFunction(IDiagnosticReceiver diagnosticReceiver, IMethodSymbol localFunctionSymbol)
			{
				diagnosticReceiver.ReportDiagnostic(DefaultParamDiagnostics.DUR0103_DefaultParamIsNotValidOnLocalFunctionsOrLambdas, localFunctionSymbol);
			}

			// These two method shouldn't be accessible from method analyzer

			//public static bool DefaultAnalyze(IDiagnosticReceiver diagnosticReceiver, ISymbol symbol, DefaultParamCompilationData compilation, CancellationToken cancellationToken = default)
			//{
			//	return DefaultParamAnalyzer.WithDiagnostics.DefaultAnalyze(diagnosticReceiver, symbol, compilation, cancellationToken);
			//}

			//public static bool DefaultAnalyze(IDiagnosticReceiver diagnosticReceiver, ISymbol symbol, DefaultParamCompilationData compilation, in TypeParameterContainer typeParameters, CancellationToken cancellationToken = default)
			//{
			//	return DefaultParamAnalyzer.WithDiagnostics.DefaultAnalyze(diagnosticReceiver, symbol, compilation, in typeParameters, cancellationToken);
			//}

			/// <inheritdoc cref="DefaultParamAnalyzer.WithDiagnostics.AnalyzeAgaintsProhibitedAttributes(IDiagnosticReceiver, ISymbol, DefaultParamCompilationData)"/>
			public static bool AnalyzeAgaintsProhibitedAttributes(IDiagnosticReceiver diagnosticReceiver, IMethodSymbol symbol, DefaultParamCompilationData compialation)
			{
				return DefaultParamAnalyzer.WithDiagnostics.AnalyzeAgaintsProhibitedAttributes(diagnosticReceiver, symbol, compialation);
			}

			/// <inheritdoc cref="DefaultParamAnalyzer.WithDiagnostics.AnalyzeAgaintsProhibitedAttributes(IDiagnosticReceiver, ISymbol, DefaultParamCompilationData, out AttributeData[])"/>
			public static bool AnalyzeAgaintsProhibitedAttributes(IDiagnosticReceiver diagnosticReceiver, IMethodSymbol symbol, DefaultParamCompilationData compilation, [NotNullWhen(true)] out AttributeData[]? attributes)
			{
				return DefaultParamAnalyzer.WithDiagnostics.AnalyzeAgaintsProhibitedAttributes(diagnosticReceiver, symbol, compilation, out attributes);
			}

			/// <inheritdoc cref="DefaultParamAnalyzer.WithDiagnostics.AnalyzeContainingTypes(IDiagnosticReceiver, ISymbol, CancellationToken)"/>
			public static bool AnalyzeContainingTypes(IDiagnosticReceiver diagnosticReceiver, IMethodSymbol symbol, CancellationToken cancellationToken = default)
			{
				return DefaultParamAnalyzer.WithDiagnostics.AnalyzeContainingTypes(diagnosticReceiver, symbol, cancellationToken);
			}

			/// <inheritdoc cref="DefaultParamAnalyzer.WithDiagnostics.AnalyzeContainingTypes(IDiagnosticReceiver, ISymbol, DefaultParamCompilationData, out ITypeData[])"/>
			public static bool AnalyzeContainingTypes(IDiagnosticReceiver diagnosticReceiver, IMethodSymbol symbol, DefaultParamCompilationData compilation, [NotNullWhen(true)] out ITypeData[]? containingTypes)
			{
				return DefaultParamAnalyzer.WithDiagnostics.AnalyzeContainingTypes(diagnosticReceiver, symbol, compilation, out containingTypes);
			}

			/// <inheritdoc cref="DefaultParamAnalyzer.WithDiagnostics.AnalyzeTypeParameters(IDiagnosticReceiver, in TypeParameterContainer)"/>
			public static bool AnalyzeTypeParameters(IDiagnosticReceiver diagnosticReceiver, in TypeParameterContainer typeParameters)
			{
				return DefaultParamAnalyzer.WithDiagnostics.AnalyzeTypeParameters(diagnosticReceiver, in typeParameters);
			}

			private static bool AnalyzeCore(IDiagnosticReceiver diagnosticReceiver, IMethodSymbol symbol, DefaultParamCompilationData compilation, ref TypeParameterContainer typeParameters, CancellationToken cancellationToken)
			{
				bool isValid = AnalyzeAgaintsPartialOrExtern(diagnosticReceiver, symbol, cancellationToken);
				isValid &= AnalyzeAgaintsProhibitedAttributes(diagnosticReceiver, symbol, compilation);
				isValid &= AnalyzeContainingTypes(diagnosticReceiver, symbol, cancellationToken);
				isValid &= AnalyzeBaseMethodAndTypeParameters(diagnosticReceiver, symbol, ref typeParameters, compilation, cancellationToken);

				if (!isValid)
				{
					return false;
				}

				return AnalyzeMethodSignature(diagnosticReceiver, symbol, in typeParameters, compilation, cancellationToken);
			}

			private static bool AnalyzeCollidingMethods(
				IDiagnosticReceiver diagnosticReceiver,
				IMethodSymbol symbol,
				in TypeParameterContainer typeParameters,
				CollidingMethod[] collidingMethods,
				IParameterSymbol[] symbolParameters,
				DefaultParamConfiguration configuration,
				CancellationToken cancellationToken,
				out HashSet<int>? applyNew
			)
			{
				HashSet<int> diagnosed = new();
				HashSet<int> applyNewLocal = new();
				int numMethods = collidingMethods.Length;
				bool isValid = true;

				ParameterGeneration[][] generations = GetParameterGenerations(in typeParameters, symbolParameters);

				for (int i = 0; i < numMethods; i++)
				{
					ref readonly CollidingMethod currentMethod = ref collidingMethods[i];
					int targetIndex = currentMethod.TypeParameters.Length - typeParameters.NumNonDefaultParam;

					if (diagnosed.Contains(targetIndex))
					{
						continue;
					}

					ParameterGeneration[] targetParameters = generations[targetIndex];

					if (!AnalyzeCollidingMethodParameters(
						diagnosticReceiver,
						symbol,
						in currentMethod,
						in typeParameters,
						targetParameters,
						targetIndex,
						diagnosed,
						applyNewLocal,
						configuration,
						ref isValid,
						cancellationToken)
					)
					{
						break;
					}
				}

				applyNew = GetApplyNewOrNull(applyNewLocal);
				return isValid;
			}

			private static bool AnalyzeCollidingMethodParameters(
				IDiagnosticReceiver diagnosticReceiver,
				IMethodSymbol symbol,
				in CollidingMethod collidingMethod,
				in TypeParameterContainer typeParameters,
				ParameterGeneration[] targetGeneration,
				int targetIndex,
				HashSet<int> diagnosed,
				HashSet<int> applyNew,
				DefaultParamConfiguration configuration,
				ref bool isValid,
				CancellationToken cancellationToken
			)
			{
				if (!HasCollidingParameters(targetGeneration, in collidingMethod))
				{
					return true;
				}

				if (!configuration.ApplyNewModifierWhenPossible || SymbolEqualityComparer.Default.Equals(collidingMethod.Symbol.ContainingType, symbol.ContainingType))
				{
					if (HasNewModifier(symbol, collidingMethod.Symbol, cancellationToken))
					{
						applyNew.Add(targetIndex);
						return true;
					}

					string signature = GetMethodSignatureString(symbol.Name, in typeParameters, targetIndex, targetGeneration);
					diagnosticReceiver.ReportDiagnostic(DefaultParamDiagnostics.DUR0118_MethodWithSignatureAlreadyExists, symbol, signature);
					diagnosed.Add(targetIndex);
					isValid = false;

					if (diagnosed.Count == typeParameters.NumDefaultParam)
					{
						return false;
					}
				}
				else if (isValid)
				{
					applyNew.Add(targetIndex);
				}

				return true;
			}

			private static bool AnalyzeBaseMethodParameters(IDiagnosticReceiver diagnosticReceiver, in TypeParameterContainer typeParameters, in TypeParameterContainer baseTypeParameters)
			{
				int length = baseTypeParameters.Length;
				bool isValid = true;

				int firstIndex = GetFirstDefaultParamIndex(in typeParameters, in baseTypeParameters);

				for (int i = firstIndex; i < length; i++)
				{
					ref readonly TypeParameterData baseData = ref baseTypeParameters[i];
					ref readonly TypeParameterData thisData = ref typeParameters[i];

					if (!AnalyzeParameterInBaseMethod(diagnosticReceiver, in thisData, in baseData))
					{
						isValid = false;
					}
				}

				return isValid;
			}

			private static int GetFirstDefaultParamIndex(in TypeParameterContainer typeParameters, in TypeParameterContainer baseTypeParameters)
			{
				if (typeParameters.FirstDefaultParamIndex == -1)
				{
					return baseTypeParameters.FirstDefaultParamIndex;
				}

				if (baseTypeParameters.FirstDefaultParamIndex == -1)
				{
					return typeParameters.FirstDefaultParamIndex;
				}

				if (typeParameters.FirstDefaultParamIndex < baseTypeParameters.FirstDefaultParamIndex)
				{
					return typeParameters.FirstDefaultParamIndex;
				}

				return baseTypeParameters.FirstDefaultParamIndex;
			}

			private static bool AnalyzeParameterInBaseMethod(IDiagnosticReceiver diagnosticReceiver, in TypeParameterData thisData, in TypeParameterData baseData)
			{
				if (baseData.IsDefaultParam)
				{
					if (!thisData.IsDefaultParam)
					{
						diagnosticReceiver.ReportDiagnostic(DefaultParamDiagnostics.DUR0110_OverriddenDefaultParamAttribuetShouldBeAddedForClarity, thisData.Symbol);

						// This diagnostic is only a warning, so the symbol is still valid.
						return true;
					}

					if (!SymbolEqualityComparer.Default.Equals(thisData.TargetType, baseData.TargetType))
					{
						diagnosticReceiver.ReportDiagnostic(DefaultParamDiagnostics.DUR0108_ValueOfOverriddenMethodMustBeTheSameAsBase, thisData.Location, thisData.Symbol);
						return false;
					}
				}
				else if (thisData.IsDefaultParam)
				{
					diagnosticReceiver.ReportDiagnostic(DefaultParamDiagnostics.DUR0109_DoNotAddDefaultParamAttributeOnOverridenParameters, thisData.Location, thisData.Symbol);
					return false;
				}

				return true;
			}
		}
	}
}
