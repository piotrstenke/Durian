// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using Durian.Analysis.Data;
using Durian.Analysis.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.Analysis.DefaultParam
{
#pragma warning disable RS1001 // Missing diagnostic analyzer attribute.

	public partial class DefaultParamMethodAnalyzer
#pragma warning restore RS1001 // Missing diagnostic analyzer attribute.
	{
		/// <summary>
		/// Contains static methods that analyze methods with type parameters marked using the <see cref="DefaultParamAttribute"/> and report <see cref="Diagnostic"/>s for the invalid ones.
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
			public static bool Analyze(
				IDiagnosticReceiver diagnosticReceiver,
				IMethodSymbol symbol,
				DefaultParamCompilationData compilation,
				CancellationToken cancellationToken = default
			)
			{
				TypeParameterContainer typeParameters = TypeParameterContainer.CreateFrom(symbol, compilation, cancellationToken);

				if (!typeParameters.HasDefaultParams && !symbol.IsOverride)
				{
					return false;
				}

				return AnalyzeCore(diagnosticReceiver, symbol, compilation, ref typeParameters, cancellationToken);
			}

			/// <summary>
			/// Analyzes if the <paramref name="symbol"/> is of an invalid type.
			/// </summary>
			/// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> that is used to report <see cref="Diagnostic"/>s.</param>
			/// <param name="symbol"><see cref="IMethodSymbol"/> to analyze.</param>
			/// <returns><see langword="true"/> if the <paramref name="symbol"/> is valid, otherwise <see langword="false"/>.</returns>
			public static bool AnalyzeAgainstInvalidMethodType(IDiagnosticReceiver diagnosticReceiver, IMethodSymbol symbol)
			{
				if (symbol.MethodKind == MethodKind.LocalFunction ||
					symbol.MethodKind == MethodKind.AnonymousFunction ||
					symbol.MethodKind == MethodKind.ExplicitInterfaceImplementation ||
					(symbol.ContainingType is INamedTypeSymbol t && t.TypeKind == TypeKind.Interface)
				)
				{
					ReportDiagnosticForInvalidMethodType(diagnosticReceiver, symbol);
					return false;
				}

				return true;
			}

			/// <summary>
			/// Analyzes if the <paramref name="symbol"/> or either <see langword="partial"/> or <see langword="extern"/>.
			/// </summary>
			/// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> that is used to report <see cref="Diagnostic"/>s.</param>
			/// <param name="symbol"><see cref="IMethodSymbol"/> to analyze.</param>
			/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
			/// <returns><see langword="true"/> if the <paramref name="symbol"/> is valid (is not <see langword="partial"/> or <see langword="extern"/>), otherwise <see langword="false"/>.</returns>
			public static bool AnalyzeAgainstPartialOrExtern(
				IDiagnosticReceiver diagnosticReceiver,
				IMethodSymbol symbol,
				CancellationToken cancellationToken = default
			)
			{
				if (symbol?.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax(cancellationToken) is not MethodDeclarationSyntax declaration)
				{
					return false;
				}

				return AnalyzeAgainstPartialOrExtern(diagnosticReceiver, symbol, declaration);
			}

			/// <summary>
			/// Analyzes if the <paramref name="symbol"/> or either <see langword="partial"/> or <see langword="extern"/>.
			/// </summary>
			/// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> that is used to report <see cref="Diagnostic"/>s.</param>
			/// <param name="symbol"><see cref="IMethodSymbol"/> to analyze.</param>
			/// <param name="declaration">Main <see cref="MethodDeclarationSyntax"/> of the <paramref name="symbol"/>.</param>
			/// <returns><see langword="true"/> if the <paramref name="symbol"/> is valid (is not <see langword="partial"/> or <see langword="extern"/>), otherwise <see langword="false"/>.</returns>
			public static bool AnalyzeAgainstPartialOrExtern(
				IDiagnosticReceiver diagnosticReceiver,
				IMethodSymbol symbol,
				MethodDeclarationSyntax declaration
			)
			{
				if (symbol.IsExtern || symbol.IsPartial(declaration))
				{
					diagnosticReceiver.ReportDiagnostic(DefaultParamDiagnostics.DUR0102_MethodCannotBePartialOrExtern, symbol);
					return false;
				}

				return true;
			}

			/// <inheritdoc cref="DefaultParamAnalyzer.WithDiagnostics.AnalyzeAgainstProhibitedAttributes(IDiagnosticReceiver, ISymbol, DefaultParamCompilationData, IEnumerable{AttributeData}?)"/>
			public static bool AnalyzeAgainstProhibitedAttributes(IDiagnosticReceiver diagnosticReceiver, ISymbol symbol, DefaultParamCompilationData compilation, IEnumerable<AttributeData>? attributes = null)
			{
				return DefaultParamAnalyzer.WithDiagnostics.AnalyzeAgainstProhibitedAttributes(diagnosticReceiver, symbol, compilation, attributes);
			}

			/// <inheritdoc cref="DefaultParamAnalyzer.WithDiagnostics.AnalyzeAgainstProhibitedAttributes(IDiagnosticReceiver, ISymbol, DefaultParamCompilationData, out AttributeData[])"/>
			public static bool AnalyzeAgainstProhibitedAttributes(IDiagnosticReceiver diagnosticReceiver, IMethodSymbol symbol, DefaultParamCompilationData compilation, [NotNullWhen(true)] out AttributeData[]? attributes)
			{
				return DefaultParamAnalyzer.WithDiagnostics.AnalyzeAgainstProhibitedAttributes(diagnosticReceiver, symbol, compilation, out attributes);
			}

			/// <summary>
			/// Analyzes if the <paramref name="symbol"/> has valid <paramref name="typeParameters"/> when compared to the <paramref name="symbol"/>'s base method.
			/// </summary>
			/// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> that is used to report <see cref="Diagnostic"/>s.</param>
			/// <param name="symbol"><see cref="IMethodSymbol"/> to analyze.</param>
			/// <param name="typeParameters"><see cref="TypeParameterContainer"/> that contains type parameters to be analyzed.</param>
			/// <param name="combinedTypeParameters">Combined <see cref="TypeParameterContainer"/>s of the <paramref name="symbol"/>'s base methods.</param>
			/// <returns><see langword="true"/> if the <paramref name="symbol"/> is valid, otherwise <see langword="false"/>.</returns>
			public static bool AnalyzeBaseMethodAndTypeParameters(
				IDiagnosticReceiver diagnosticReceiver,
				IMethodSymbol symbol,
				ref TypeParameterContainer typeParameters,
				in TypeParameterContainer combinedTypeParameters
			)
			{
				if (!symbol.IsOverride)
				{
					return AnalyzeTypeParameters(diagnosticReceiver, symbol, in typeParameters);
				}

				if (DefaultParamAnalyzer.AnalyzeTypeParameters(symbol, in combinedTypeParameters) && AnalyzeBaseMethodParameters(diagnosticReceiver, in typeParameters, in combinedTypeParameters))
				{
					typeParameters = TypeParameterContainer.Combine(in typeParameters, in combinedTypeParameters);
					return true;
				}

				return false;
			}

			/// <inheritdoc cref="DefaultParamAnalyzer.WithDiagnostics.AnalyzeContainingTypes(IDiagnosticReceiver, ISymbol, DefaultParamCompilationData, out INamedTypeSymbol[], CancellationToken)"/>
			public static bool AnalyzeContainingTypes(
				IDiagnosticReceiver diagnosticReceiver,
				ISymbol symbol,
				DefaultParamCompilationData compilation,
				[NotNullWhen(true)] out INamedTypeSymbol[]? containingTypes,
				CancellationToken cancellationToken = default
			)
			{
				return DefaultParamAnalyzer.WithDiagnostics.AnalyzeContainingTypes(
					diagnosticReceiver,
					symbol,
					compilation,
					out containingTypes,
					cancellationToken
				);
			}

			/// <inheritdoc cref="DefaultParamAnalyzer.WithDiagnostics.AnalyzeContainingTypes(IDiagnosticReceiver, ISymbol, DefaultParamCompilationData, INamedTypeSymbol[], CancellationToken)"/>
			public static bool AnalyzeContainingTypes(
				IDiagnosticReceiver diagnosticReceiver,
				ISymbol symbol,
				DefaultParamCompilationData compilation,
				INamedTypeSymbol[]? containingTypes = null,
				CancellationToken cancellationToken = default
			)
			{
				return DefaultParamAnalyzer.WithDiagnostics.AnalyzeContainingTypes(
					diagnosticReceiver,
					symbol,
					compilation,
					containingTypes,
					cancellationToken
				);
			}

			/// <inheritdoc cref="DefaultParamAnalyzer.WithDiagnostics.AnalyzeContainingTypes(IDiagnosticReceiver, ISymbol, DefaultParamCompilationData, out ITypeData[])"/>
			public static bool AnalyzeContainingTypes(
				IDiagnosticReceiver diagnosticReceiver,
				IMethodSymbol symbol,
				DefaultParamCompilationData compilation,
				[NotNullWhen(true)] out ITypeData[]? containingTypes
			)
			{
				return DefaultParamAnalyzer.WithDiagnostics.AnalyzeContainingTypes(
					diagnosticReceiver,
					symbol,
					compilation,
					out containingTypes
				);
			}

			/// <summary>
			/// Analyzes if the signature of the <paramref name="symbol"/> is valid. If so, returns a <see cref="HashSet{T}"/> of indexes of type parameters with the <see cref="DefaultParamAttribute"/> applied for whom the <see langword="new"/> modifier should be applied.
			/// </summary>
			/// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> that is used to report <see cref="Diagnostic"/>s.</param>
			/// <param name="symbol"><see cref="IMethodSymbol"/> to analyze the signature of.</param>
			/// <param name="typeParameters"><see cref="TypeParameterContainer"/> containing type parameters of the <paramref name="symbol"/>.</param>
			/// <param name="compilation">Current <see cref="DefaultParamCompilationData"/>.</param>
			/// <param name="applyNew"><see langword="abstract"/><see cref="HashSet{T}"/> of indexes of type parameters with the <see cref="DefaultParamAttribute"/> applied for whom the <see langword="new"/> modifier should be applied. -or- <see langword="null"/> if the <paramref name="symbol"/> is not valid.</param>
			/// <param name="attributes">A collection of <see cref="AttributeData"/>a of the target <paramref name="symbol"/>.</param>
			/// <param name="containingTypes">An array of <see cref="INamedTypeSymbol"/>s of the <paramref name="symbol"/>'s containing types.</param>
			/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
			/// <returns><see langword="true"/> if the signature of <paramref name="symbol"/> is valid, otherwise <see langword="false"/>.</returns>
			public static bool AnalyzeMethodSignature(
				IDiagnosticReceiver diagnosticReceiver,
				IMethodSymbol symbol,
				in TypeParameterContainer typeParameters,
				DefaultParamCompilationData compilation,
				out HashSet<int>? applyNew,
				IEnumerable<AttributeData>? attributes = null,
				INamedTypeSymbol[]? containingTypes = null,
				CancellationToken cancellationToken = default
			)
			{
				IParameterSymbol[] symbolParameters = symbol.Parameters.ToArray();

				CollidingMember[] collidingMethods = GetPotentiallyCollidingMembers(
					symbol,
					compilation,
					null,
					typeParameters.Length,
					typeParameters.NumNonDefaultParam,
					symbolParameters.Length
				);

				if (collidingMethods.Length == 0)
				{
					applyNew = null;
					return true;
				}

				bool allowsNewModifier = AllowsNewModifier(symbol, compilation, attributes, containingTypes);

				return AnalyzeCollidingMembers(
					diagnosticReceiver,
					symbol,
					in typeParameters,
					collidingMethods,
					symbolParameters,
					allowsNewModifier,
					cancellationToken,
					out applyNew
				);
			}

			/// <inheritdoc cref="DefaultParamAnalyzer.WithDiagnostics.AnalyzeTypeParameters(IDiagnosticReceiver, ISymbol, in TypeParameterContainer)"/>
			public static bool AnalyzeTypeParameters(IDiagnosticReceiver diagnosticReceiver, IMethodSymbol symbol, in TypeParameterContainer typeParameters)
			{
				return DefaultParamAnalyzer.WithDiagnostics.AnalyzeTypeParameters(diagnosticReceiver, symbol, in typeParameters);
			}

			/// <summary>
			/// Reports <see cref="Diagnostic"/>s for the specified <paramref name="method"/>.
			/// </summary>
			/// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> that is used to report <see cref="Diagnostic"/>s.</param>
			/// <param name="method"><see cref="IMethodSymbol"/> of a method to report the <see cref="Diagnostic"/>s for.</param>
			public static void ReportDiagnosticForInvalidMethodType(IDiagnosticReceiver diagnosticReceiver, IMethodSymbol method)
			{
				diagnosticReceiver.ReportDiagnostic(DefaultParamDiagnostics.DUR0103_DefaultParamIsNotOnThisTypeOfMethod, method);
			}

			/// <summary>
			/// Determines, whether the specified <paramref name="symbol"/> should be analyzed.
			/// </summary>
			/// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> that is used to report <see cref="Diagnostic"/>s.</param>
			/// <param name="symbol"><see cref="IMethodSymbol"/> to check if should be analyzed.</param>
			/// <param name="compilation">Current <see cref="DefaultParamCompilationData"/>.</param>
			/// <param name="typeParameters"><see cref="TypeParameterContainer"/> that contains type parameters to be analyzed.</param>
			/// <param name="combinedTypeParameters">Combined <see cref="TypeParameterContainer"/>s of the <paramref name="symbol"/>'s base methods.</param>
			/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
			/// <returns><see langword="true"/> if the <paramref name="symbol"/> should be analyzer, <see langword="false"/> otherwise.</returns>
			public static bool ShouldBeAnalyzed(
				IDiagnosticReceiver diagnosticReceiver,
				IMethodSymbol symbol,
				DefaultParamCompilationData compilation,
				in TypeParameterContainer typeParameters,
				out TypeParameterContainer combinedTypeParameters,
				CancellationToken cancellationToken = default
			)
			{
				if (!symbol.IsOverride)
				{
					combinedTypeParameters = typeParameters;
					return true;
				}

				if (symbol.OverriddenMethod is not IMethodSymbol baseMethod)
				{
					combinedTypeParameters = default;
					return false;
				}

				if (IsDefaultParamGenerated(baseMethod, compilation))
				{
					diagnosticReceiver.ReportDiagnostic(DefaultParamDiagnostics.DUR0107_DoNotOverrideGeneratedMethods, symbol);
					combinedTypeParameters = default;
					return false;
				}

				TypeParameterContainer combined = GetBaseMethodTypeParameters(baseMethod, compilation, cancellationToken);

				if (typeParameters.HasDefaultParams || combined.HasDefaultParams)
				{
					combinedTypeParameters = combined;
					return true;
				}

				combinedTypeParameters = default;
				return false;
			}

			private static bool AnalyzeBaseMethodParameters(
				IDiagnosticReceiver diagnosticReceiver,
				in TypeParameterContainer typeParameters,
				in TypeParameterContainer baseTypeParameters
			)
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

			private static bool AnalyzeCollidingMembers(
				IDiagnosticReceiver diagnosticReceiver,
				IMethodSymbol symbol,
				in TypeParameterContainer typeParameters,
				CollidingMember[] collidingMembers,
				IParameterSymbol[] symbolParameters,
				bool applyNewModifierIfPossible,
				CancellationToken cancellationToken,
				out HashSet<int>? applyNew
			)
			{
				HashSet<int> diagnosed = new();
				HashSet<int> applyNewLocal = new();
				int numCollisions = collidingMembers.Length;
				bool isValid = true;
				bool allowsNewModifier = applyNewModifierIfPossible || HasNewModifier(symbol, cancellationToken);

				ParameterGeneration[][] generations = DefaultParamUtilities.GetParameterGenerations(in typeParameters, symbolParameters);

				for (int i = 0; i < numCollisions; i++)
				{
					ref readonly CollidingMember member = ref collidingMembers[i];

					// Type parameters are null when the member is neither IMethodSymbol or INamedTypeSymbol.
					if (member.TypeParameters is null)
					{
						int index = generations.Length - 1;

						if (allowsNewModifier && !SymbolEqualityComparer.Default.Equals(member.Symbol.ContainingType, symbol.ContainingType))
						{
							applyNewLocal.Add(index);
						}
						else
						{
							diagnosticReceiver.ReportDiagnostic(DefaultParamDiagnostics.DUR0116_MemberWithNameAlreadyExists, symbol, symbol.Name);
							isValid = false;

							if (diagnosed.Add(index) && diagnosed.Count == typeParameters.NumDefaultParam)
							{
								break;
							}
						}

						continue;
					}

					int targetIndex = member.TypeParameters.Length - typeParameters.NumNonDefaultParam;

					if (diagnosed.Contains(targetIndex))
					{
						continue;
					}

					ParameterGeneration[] targetParameters = generations[targetIndex];

					// Parameters are null when the member is not an IMethodSymbol.
					if (member.Parameters is null)
					{
						if (allowsNewModifier && !SymbolEqualityComparer.Default.Equals(member.Symbol.ContainingType, symbol.ContainingType))
						{
							applyNewLocal.Add(targetIndex);
						}
						else
						{
							diagnosticReceiver.ReportDiagnostic(DefaultParamDiagnostics.DUR0116_MemberWithNameAlreadyExists, symbol, GetMethodSignatureString(symbol.Name, in typeParameters, targetIndex, targetParameters));
							isValid = false;

							if (diagnosed.Add(targetIndex) && diagnosed.Count == typeParameters.NumDefaultParam)
							{
								break;
							}
						}

						continue;
					}

					if (!AnalyzeCollidingMethodParameters(
						diagnosticReceiver,
						symbol,
						in member,
						in typeParameters,
						targetParameters,
						targetIndex,
						diagnosed,
						applyNewLocal,
						allowsNewModifier,
						ref isValid)
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
				in CollidingMember collidingMember,
				in TypeParameterContainer typeParameters,
				ParameterGeneration[] targetGeneration,
				int targetIndex,
				HashSet<int> diagnosed,
				HashSet<int> applyNew,
				bool allowsNewModifier,
				ref bool isValid
			)
			{
				if (!HasCollidingParameters(targetGeneration, in collidingMember))
				{
					return true;
				}

				if (!allowsNewModifier || SymbolEqualityComparer.Default.Equals(collidingMember.Symbol.ContainingType, symbol.ContainingType))
				{
					string signature = GetMethodSignatureString(symbol.Name, in typeParameters, targetIndex, targetGeneration);
					diagnosticReceiver.ReportDiagnostic(DefaultParamDiagnostics.DUR0114_MethodWithSignatureAlreadyExists, symbol, signature);
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

			private static bool AnalyzeCore(
				IDiagnosticReceiver diagnosticReceiver,
				IMethodSymbol symbol,
				DefaultParamCompilationData compilation,
				ref TypeParameterContainer typeParameters,
				CancellationToken cancellationToken
			)
			{
				if (!ShouldBeAnalyzed(
					diagnosticReceiver,
					symbol,
					compilation,
					in typeParameters,
					out TypeParameterContainer combinedTypeParameters,
					cancellationToken)
				)
				{
					return false;
				}

				bool isValid = AnalyzeAgainstInvalidMethodType(diagnosticReceiver, symbol);
				isValid &= AnalyzeAgainstPartialOrExtern(diagnosticReceiver, symbol, cancellationToken);
				isValid &= AnalyzeAgainstProhibitedAttributes(diagnosticReceiver, symbol, compilation, out AttributeData[]? attributes);
				isValid &= AnalyzeContainingTypes(diagnosticReceiver, symbol, compilation, out INamedTypeSymbol[]? containingTypes, cancellationToken);
				isValid &= AnalyzeBaseMethodAndTypeParameters(diagnosticReceiver, symbol, ref typeParameters, in combinedTypeParameters);

				if (!isValid)
				{
					return false;
				}

				return AnalyzeMethodSignature(diagnosticReceiver, symbol, in typeParameters, compilation, out _, attributes!, containingTypes!, cancellationToken);
			}

			private static bool AnalyzeParameterInBaseMethod(
				IDiagnosticReceiver diagnosticReceiver,
				in TypeParameterData thisData,
				in TypeParameterData baseData
			)
			{
				if (baseData.IsValidDefaultParam)
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
