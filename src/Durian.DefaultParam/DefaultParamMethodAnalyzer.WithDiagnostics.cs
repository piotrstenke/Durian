using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Durian.Data;
using Durian.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Diagnostics.CodeAnalysis;

namespace Durian.DefaultParam
{
	public partial class DefaultParamMethodAnalyzer
	{
		public static new class WithDiagnostics
		{
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

			public static bool AnalyzeOverrideMethod(
				IDiagnosticReceiver diagnosticReceiver,
				IMethodSymbol symbol,
				[NotNullWhen(true)] IMethodSymbol? baseMethod,
				ref TypeParameterContainer typeParameters,
				DefaultParamCompilationData compilation,
				CancellationToken cancellationToken,
				out bool hasValidTypeParameters
			)
			{
				if (baseMethod is null)
				{
					hasValidTypeParameters = false;
					return false;
				}

				if (IsDefaultParamGenerated(baseMethod, compilation))
				{
					DefaultParamDiagnostics.DoNotOverrideMethodsGeneratedUsingDefaultParamAttribute(diagnosticReceiver, symbol);
					hasValidTypeParameters = AnalyzeTypeParameters(diagnosticReceiver, in typeParameters);
					return false;
				}

				TypeParameterContainer baseTypeParameters = GetBaseMethodTypeParameters(baseMethod, compilation, cancellationToken);
				bool isValid;

				if (typeParameters.FirstDefaultParamIndex == -1)
				{
					isValid = AnalyzeTypeParameters(diagnosticReceiver, in baseTypeParameters);
					hasValidTypeParameters = isValid;

					if (isValid)
					{
						isValid &= AnalyzeBaseMethodParameters(diagnosticReceiver, compilation.Configuration, in typeParameters, in baseTypeParameters);
					}

					typeParameters = TypeParameterContainer.Combine(in typeParameters, in baseTypeParameters);
				}
				else if (HasAddedDefaultParamAttributes(in typeParameters, in baseTypeParameters))
				{
					bool addingTypeParametersIsValid = TryUpdateTypeParameters(ref typeParameters, in baseTypeParameters, compilation.Configuration);
					isValid = AnalyzeTypeParameters(diagnosticReceiver, in typeParameters);
					hasValidTypeParameters = isValid;

					if (isValid)
					{
						isValid &= AnalyzeBaseMethodParameters(diagnosticReceiver, compilation.Configuration, in typeParameters, in baseTypeParameters);

						if (!addingTypeParametersIsValid)
						{
							ReportAddedTypeParameters(diagnosticReceiver, typeParameters, baseTypeParameters);
							isValid = false;
						}
					}
				}
				else
				{
					typeParameters = TypeParameterContainer.Combine(in typeParameters, in baseTypeParameters);
					isValid = AnalyzeTypeParameters(diagnosticReceiver, in typeParameters);
					hasValidTypeParameters = isValid;

					if (isValid)
					{
						isValid &= AnalyzeBaseMethodParameters(diagnosticReceiver, compilation.Configuration, in typeParameters, in baseTypeParameters);
					}
				}

				return isValid;
			}

			public static bool AnalyzeAgaintsLocalFunction(IDiagnosticReceiver diagnosticReceiver, IMethodSymbol symbol)
			{
				if (symbol.MethodKind == MethodKind.LocalFunction)
				{
					ReportDiagnosticForLocalFunction(diagnosticReceiver, symbol);
					return false;
				}

				return true;
			}

			public static bool AnalyzeAgaintsPartialOrExtern(IDiagnosticReceiver diagnosticReceiver, IMethodSymbol symbol, CancellationToken cancellationToken = default)
			{
				if (symbol?.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax(cancellationToken) is not MethodDeclarationSyntax declaration)
				{
					return false;
				}

				return AnalyzeAgaintsPartialOrExtern(diagnosticReceiver, symbol, declaration);
			}

			public static bool AnalyzeAgaintsPartialOrExtern(IDiagnosticReceiver diagnosticReceiver, IMethodSymbol symbol, MethodDeclarationSyntax declaration)
			{
				if (symbol.IsExtern)
				{
					DefaultParamDiagnostics.DefaultParamMethodCannotBePartialOrExtern(diagnosticReceiver, symbol);
					return false;
				}
				else if (symbol.IsPartial(declaration))
				{
					DefaultParamDiagnostics.DefaultParamMethodCannotBePartialOrExtern(diagnosticReceiver, symbol);
					return false;
				}

				return true;
			}

			public static bool AnalyzeMethodSignature(IDiagnosticReceiver diagnosticReceiver, IMethodSymbol symbol, in TypeParameterContainer typeParameters, DefaultParamCompilationData compilation, CancellationToken cancellationToken = default)
			{
				return AnalyzeMethodSignature(diagnosticReceiver, symbol, in typeParameters, compilation, out _, cancellationToken);
			}

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

			public static void ReportDiagnosticForLocalFunction(IDiagnosticReceiver diagnosticReceiver, IMethodSymbol symbol)
			{
				DefaultParamDiagnostics.DefaultParamAttributeIsNotValidOnLocalFunctions(diagnosticReceiver, symbol);
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

			public static bool AnalyzeAgaintsProhibitedAttributes(IDiagnosticReceiver diagnosticReceiver, ISymbol symbol, DefaultParamCompilationData compialation)
			{
				return DefaultParamAnalyzer.WithDiagnostics.AnalyzeAgaintsProhibitedAttributes(diagnosticReceiver, symbol, compialation);
			}

			public static bool AnalyzeAgaintsProhibitedAttributes(IDiagnosticReceiver diagnosticReceiver, ISymbol symbol, DefaultParamCompilationData compilation, [NotNullWhen(true)]out AttributeData[]? attributes)
			{
				return DefaultParamAnalyzer.WithDiagnostics.AnalyzeAgaintsProhibitedAttributes(diagnosticReceiver, symbol, compilation, out attributes);
			}

			public static bool AnalyzeContainingTypes(IDiagnosticReceiver diagnosticReceiver, ISymbol symbol, CancellationToken cancellationToken = default)
			{
				return DefaultParamAnalyzer.WithDiagnostics.AnalyzeContainingTypes(diagnosticReceiver, symbol, cancellationToken);
			}

			public static bool AnalyzeContainingTypes(IDiagnosticReceiver diagnosticReceiver, ISymbol symbol, DefaultParamCompilationData compilation, [NotNullWhen(true)] out ITypeData[]? containingTypes)
			{
				return DefaultParamAnalyzer.WithDiagnostics.AnalyzeContainingTypes(diagnosticReceiver, symbol, compilation, out containingTypes);
			}

			public static bool AnalyzeTypeParameters(IDiagnosticReceiver diagnosticReceiver, in TypeParameterContainer typeParameters)
			{
				return DefaultParamAnalyzer.WithDiagnostics.AnalyzeTypeParameters(diagnosticReceiver, in typeParameters);
			}

			private static bool AnalyzeCore(IDiagnosticReceiver diagnosticReceiver, IMethodSymbol symbol, DefaultParamCompilationData compilation, ref TypeParameterContainer typeParameters, CancellationToken cancellationToken)
			{
				bool isValid = AnalyzeAgaintsPartialOrExtern(diagnosticReceiver, symbol, cancellationToken);
				isValid &= AnalyzeAgaintsProhibitedAttributes(diagnosticReceiver, symbol, compilation);
				isValid &= AnalyzeContainingTypes(diagnosticReceiver, symbol, cancellationToken);

				bool hasValidTypeParameters;

				if (IsOverride(symbol, out IMethodSymbol? baseMethod))
				{
					isValid &= AnalyzeOverrideMethod(diagnosticReceiver, symbol, baseMethod, ref typeParameters, compilation, cancellationToken, out hasValidTypeParameters);
				}
				else
				{
					hasValidTypeParameters = AnalyzeTypeParameters(diagnosticReceiver, in typeParameters);
					isValid &= hasValidTypeParameters;
				}

				if (hasValidTypeParameters)
				{
					isValid &= AnalyzeMethodSignature(diagnosticReceiver, symbol, typeParameters, compilation, cancellationToken);
				}

				return isValid;
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

				if (!configuration.ApplyNewToGeneratedMembersWithEquivalentSignature || SymbolEqualityComparer.Default.Equals(collidingMethod.Symbol.ContainingType, symbol.ContainingType))
				{
					if (HasNewModifier(symbol, collidingMethod.Symbol, cancellationToken))
					{
						applyNew.Add(targetIndex);
						return true;
					}

					string signature = GetMethodSignatureString(symbol.Name, in typeParameters, targetIndex, targetGeneration);
					DurianDiagnostics.MethodWithSignatureAlreadyExists(diagnosticReceiver, symbol, signature, symbol.Locations.FirstOrDefault());
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

			private static bool AnalyzeBaseMethodParameters(IDiagnosticReceiver diagnosticReceiver, DefaultParamConfiguration configuration, in TypeParameterContainer typeParameters, in TypeParameterContainer baseTypeParameters)
			{
				int length = baseTypeParameters.Length;
				bool isValid = true;

				int firstIndex = GetFirstDefaultParamIndex(in typeParameters, in baseTypeParameters);

				for (int i = firstIndex; i < length; i++)
				{
					ref readonly TypeParameterData baseData = ref baseTypeParameters[i];
					ref readonly TypeParameterData thisData = ref typeParameters[i];

					if (!AnalyzeParameterInBaseMethod(diagnosticReceiver, in thisData, in baseData, configuration))
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

			private static bool AnalyzeParameterInBaseMethod(IDiagnosticReceiver diagnosticReceiver, in TypeParameterData thisData, in TypeParameterData baseData, DefaultParamConfiguration configuration)
			{
				if (baseData.IsDefaultParam)
				{
					if (!thisData.IsDefaultParam)
					{
						DefaultParamDiagnostics.OverriddenDefaultParamAttributeShouldBeAddedForClarity(diagnosticReceiver, thisData.Symbol);
						return true;
					}
					else if (thisData.TargetType is null)
					{
						return true;
					}
					else if (!SymbolEqualityComparer.Default.Equals(thisData.TargetType, baseData.TargetType))
					{
						if (configuration.AllowOverridingOfDefaultParamValues)
						{
							if (!thisData.TargetType.IsValidForTypeParameter(thisData.Symbol))
							{
								DurianDiagnostics.TypeIsNotValidTypeParameter(diagnosticReceiver, thisData.TargetType, thisData.Symbol);
								return false;
							}
						}
						else
						{
							DefaultParamDiagnostics.ValueOfDefaultParamAttributeMustBeTheSameAsValueForOverridenMethod(diagnosticReceiver, thisData.Symbol, thisData.Location);
							return false;
						}
					}
				}

				return true;
			}

			private static void ReportAddedTypeParameters(IDiagnosticReceiver diagnosticReceiver, in TypeParameterContainer typeParameters, in TypeParameterContainer baseTypeParameters)
			{
				int length = GetNumAddedParameters(in typeParameters, in baseTypeParameters);
				for (int i = typeParameters.FirstDefaultParamIndex; i < length; i++)
				{
					ref readonly TypeParameterData thisData = ref typeParameters[i];

					DefaultParamDiagnostics.DoNotAddDefaultParamAttributeOnOverriddenVirtualTypeParameter(diagnosticReceiver, thisData.Symbol, thisData.Location);
				}
			}
		}
	}
}
