using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Durian.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.DefaultParam
{
	public class DefaultParamMethodAnalyzer : DefaultParamAnalyzer
	{
		private readonly struct CollidingMethod
		{
			public readonly IMethodSymbol Symbol { get; }
			public readonly IParameterSymbol[] Parameters { get; }
			public readonly ITypeParameterSymbol[] TypeParameters { get; }

			public CollidingMethod(IMethodSymbol symbol, IParameterSymbol[] parameters, ITypeParameterSymbol[] typeParameters)
			{
				Symbol = symbol;
				Parameters = parameters;
				TypeParameters = typeParameters;
			}
		}

		private readonly struct ParameterGeneration
		{
			public readonly ITypeSymbol Type { get; }
			public readonly RefKind RefKind { get; }
			public readonly int GenericParameterIndex { get; }

			public ParameterGeneration(ITypeSymbol type, RefKind refKind, int genericParameterIndex)
			{
				Type = type;
				RefKind = refKind;
				GenericParameterIndex = genericParameterIndex;
			}

			public override string ToString()
			{
				return $"{(RefKind != RefKind.None ? $"{RefKind} " : "")}{Type.Name}";
			}
		}

		public override SymbolKind SupportedSymbolKind => SymbolKind.Method;

		protected override IEnumerable<DiagnosticDescriptor> GetAnalyzerSpecificDiagnostics()
		{
			return new[]
			{
				DefaultParamDiagnostics.Descriptors.DefaultParamMethodCannotBeExtern,
				DefaultParamDiagnostics.Descriptors.DefaultParamMethodCannotBePartial,
				DefaultParamDiagnostics.Descriptors.DefaultParamAttributeIsNotValidOnLocalFunctions,
				DefaultParamDiagnostics.Descriptors.OverriddenDefaultParamAttributeShouldBeAddedForClarity,
				DefaultParamDiagnostics.Descriptors.DoNotOverrideMethodsGeneratedUsingDefaultParamAttribute,
				DefaultParamDiagnostics.Descriptors.DoNotAddDefaultParamAttributeOnOverriddenVirtualTypeParameter,
				DefaultParamDiagnostics.Descriptors.ValueOfDefaultParamAttributeMustBeTheSameAsValueForOverridenMethod,
				Descriptors.MethodWithSignatureAlreadyExists
			};
		}

		public override bool Validate(IDiagnosticReceiver diagnosticReceiver, ISymbol symbol, DefaultParamCompilationData compilation, CancellationToken cancellationToken = default)
		{
			if (symbol is not IMethodSymbol m || m.TypeParameters.Length == 0)
			{
				return base.Validate(diagnosticReceiver, symbol, compilation, cancellationToken);
			}

			return Validate(diagnosticReceiver, m, compilation, cancellationToken);
		}

		public static bool Validate(IDiagnosticReceiver diagnosticReceiver, IMethodSymbol symbol, DefaultParamCompilationData compilation, CancellationToken cancellationToken = default)
		{
			TypeParameterContainer typeParameters = TypeParameterContainer.CreateFrom(symbol, compilation, cancellationToken);

			if (!typeParameters.HasDefaultParams)
			{
				return true;
			}

			if (!ValidateIsLocalFunction(diagnosticReceiver, symbol))
			{
				return false;
			}

			return ValidateInternal(diagnosticReceiver, symbol, compilation, ref typeParameters, cancellationToken);
		}

		private static bool ValidateInternal(IDiagnosticReceiver diagnosticReceiver, IMethodSymbol symbol, DefaultParamCompilationData compilation, ref TypeParameterContainer typeParameters, CancellationToken cancellationToken)
		{
			bool isValid = ValidateIsPartialOrExtern(diagnosticReceiver, symbol, cancellationToken);
			isValid &= ValidateHasGeneratedCodeAttribute(diagnosticReceiver, symbol, compilation);
			isValid &= ValidateContainingTypes(diagnosticReceiver, symbol, cancellationToken);

			bool hasValidTypeParameters;

			if (IsOverride(symbol, out IMethodSymbol? baseMethod))
			{
				isValid &= ValidateOverrideMethod(diagnosticReceiver, symbol, baseMethod, ref typeParameters, compilation, cancellationToken, out hasValidTypeParameters);
			}
			else
			{
				hasValidTypeParameters = ValidateTypeParameters(diagnosticReceiver, in typeParameters);
				isValid &= hasValidTypeParameters;
			}

			if (hasValidTypeParameters)
			{
				isValid &= ValidateMethodSignature(diagnosticReceiver, symbol, typeParameters, compilation.Configuration);
			}

			return isValid;
		}

		public static bool ValidateOverrideMethod(IMethodSymbol? baseMethod, ref TypeParameterContainer typeParameters, DefaultParamCompilationData compilation, CancellationToken cancellationToken)
		{
			if (baseMethod is null)
			{
				return false;
			}

			if (IsDefaultParamGenerated(baseMethod, compilation))
			{
				return false;
			}

			TypeParameterContainer baseTypeParameters = GetBaseMethodTypeParameters(baseMethod, compilation, cancellationToken);

			if (HasAddedDefaultParamAttributes(in typeParameters, in baseTypeParameters))
			{
				return
					TryUpdateTypeParameters(ref typeParameters, in baseTypeParameters, compilation.Configuration) &&
					ValidateTypeParameters(in typeParameters) &&
					ValidateBaseMethodParameters(compilation.Configuration, in typeParameters, in baseTypeParameters);
			}
			else
			{
				typeParameters = TypeParameterContainer.Combine(in typeParameters, in baseTypeParameters);

				return ValidateTypeParameters(in typeParameters) && ValidateBaseMethodParameters(compilation.Configuration, in typeParameters, in baseTypeParameters);
			}
		}

		public static bool ValidateOverrideMethod(IDiagnosticReceiver diagnosticReceiver, IMethodSymbol symbol, IMethodSymbol? baseMethod, ref TypeParameterContainer typeParameters, DefaultParamCompilationData compilation, CancellationToken cancellationToken, out bool hasValidTypeParameters)
		{
			if (baseMethod is null)
			{
				hasValidTypeParameters = ValidateTypeParameters(diagnosticReceiver, in typeParameters);
				return false;
			}

			if (IsDefaultParamGenerated(baseMethod, compilation))
			{
				DefaultParamDiagnostics.DoNotOverrideMethodsGeneratedUsingDefaultParamAttribute(diagnosticReceiver, symbol);
				hasValidTypeParameters = ValidateTypeParameters(diagnosticReceiver, in typeParameters);
				return false;
			}

			TypeParameterContainer baseTypeParameters = GetBaseMethodTypeParameters(baseMethod, compilation, cancellationToken);
			bool isValid;

			if (HasAddedDefaultParamAttributes(in typeParameters, in baseTypeParameters))
			{
				bool addingTypeParametersIsValid = TryUpdateTypeParameters(ref typeParameters, in baseTypeParameters, compilation.Configuration);
				isValid = ValidateTypeParameters(diagnosticReceiver, in typeParameters);
				hasValidTypeParameters = isValid;

				if (isValid)
				{
					isValid &= ValidateBaseMethodParameters(diagnosticReceiver, compilation.Configuration, in typeParameters, in baseTypeParameters);

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
				isValid = ValidateTypeParameters(diagnosticReceiver, in typeParameters);
				hasValidTypeParameters = isValid;

				if (isValid)
				{
					isValid &= ValidateBaseMethodParameters(diagnosticReceiver, compilation.Configuration, in typeParameters, in baseTypeParameters);
				}
			}

			return isValid;
		}

		public static bool ValidateIsLocalFunction(IDiagnosticReceiver diagnosticReceiver, IMethodSymbol symbol)
		{
			if (symbol.MethodKind == MethodKind.LocalFunction)
			{
				ReportDiagnosticForLocalFunction(diagnosticReceiver, symbol);
				return false;
			}

			return true;
		}

		public static bool ValidateIsLocalFunction(IMethodSymbol symbol)
		{
			return symbol.MethodKind != MethodKind.LocalFunction;
		}

		public static void ReportDiagnosticForLocalFunction(IDiagnosticReceiver diagnosticReceiver, IMethodSymbol symbol)
		{
			DefaultParamDiagnostics.DefaultParamAttributeIsNotValidOnLocalFunctions(diagnosticReceiver, symbol);
		}

		public static bool ValidateIsPartialOrExtern(IDiagnosticReceiver diagnosticReceiver, IMethodSymbol symbol, CancellationToken cancellationToken = default)
		{
			if (symbol?.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax(cancellationToken) is not MethodDeclarationSyntax declaration)
			{
				return false;
			}

			return ValidateIsPartialOrExtern(diagnosticReceiver, symbol, declaration);
		}

		public static bool ValidateIsPartialOrExtern(IDiagnosticReceiver diagnosticReceiver, IMethodSymbol symbol, MethodDeclarationSyntax declaration)
		{
			if (symbol.IsExtern)
			{
				DefaultParamDiagnostics.DefaultParamMethodCannotBeExtern(diagnosticReceiver, symbol);
				return false;
			}
			else if (IsPartialMethod(symbol, declaration))
			{
				DefaultParamDiagnostics.DefaultParamMethodCannotBePartial(diagnosticReceiver, symbol);
				return false;
			}

			return true;
		}

		public static bool ValidateIsPartialOrExtern(IMethodSymbol symbol, CancellationToken cancellationToken = default)
		{
			if (symbol?.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax(cancellationToken) is not MethodDeclarationSyntax declaration)
			{
				return false;
			}

			return ValidateIsPartialOrExtern(symbol, declaration);
		}

		public static bool ValidateIsPartialOrExtern(IMethodSymbol symbol, MethodDeclarationSyntax declaration)
		{
			return !symbol.IsExtern && !IsPartialMethod(symbol, declaration);
		}

		private static bool IsPartialMethod(IMethodSymbol symbol, MethodDeclarationSyntax declaration)
		{
			return
				symbol.DeclaringSyntaxReferences.Length > 1 ||
				symbol.PartialImplementationPart is not null ||
				symbol.PartialDefinitionPart is not null ||
				declaration.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword));
		}

		public static bool IsOverride(IMethodSymbol symbol, out IMethodSymbol? baseMethod)
		{
			if (!symbol.IsOverride)
			{
				baseMethod = null;
				return false;
			}

			baseMethod = symbol.OverriddenMethod;
			return true;
		}

		private static TypeParameterContainer GetBaseMethodTypeParameters(IMethodSymbol baseMethod, DefaultParamCompilationData compilation, CancellationToken cancellationToken)
		{
			return TypeParameterContainer.CreateFrom(baseMethod, compilation, cancellationToken);
		}

		private static bool TryUpdateTypeParameters(ref TypeParameterContainer typeParameters, in TypeParameterContainer baseTypeParameters, DefaultParamConfiguration configuration)
		{
			if (configuration.ApplyNewToGeneratedMembersWithEquivalentSignature)
			{
				typeParameters = TypeParameterContainer.Combine(typeParameters, baseTypeParameters);
				return true;
			}

			return false;
		}

		private static bool HasAddedDefaultParamAttributes(in TypeParameterContainer typeParameters, in TypeParameterContainer baseTypeParameters)
		{
			return typeParameters.FirstDefaultParamIndex < baseTypeParameters.FirstDefaultParamIndex;
		}

		private static bool ValidateBaseMethodParameters(DefaultParamConfiguration configuration, in TypeParameterContainer typeParameters, in TypeParameterContainer baseTypeParameters)
		{
			int length = baseTypeParameters.Length;

			for (int i = baseTypeParameters.FirstDefaultParamIndex; i < length; i++)
			{
				ref readonly TypeParameterData baseData = ref baseTypeParameters[i];
				ref readonly TypeParameterData thisData = ref typeParameters[i];

				if (!ValidateParameterInBaseMethod(in thisData, in baseData, configuration))
				{
					return false;
				}
			}

			return true;
		}

		private static bool ValidateBaseMethodParameters(IDiagnosticReceiver diagnosticReceiver, DefaultParamConfiguration configuration, in TypeParameterContainer typeParameters, in TypeParameterContainer baseTypeParameters)
		{
			int length = baseTypeParameters.Length;
			bool isValid = true;

			for (int i = baseTypeParameters.FirstDefaultParamIndex; i < length; i++)
			{
				ref readonly TypeParameterData baseData = ref baseTypeParameters[i];
				ref readonly TypeParameterData thisData = ref typeParameters[i];

				if (!ValidateParameterInBaseMethod(diagnosticReceiver, in thisData, in baseData, configuration))
				{
					isValid = false;
				}
			}

			return isValid;
		}

		private static bool ValidateParameterInBaseMethod(in TypeParameterData thisData, in TypeParameterData baseData, DefaultParamConfiguration configuration)
		{
			if (baseData.IsDefaultParam && thisData.IsDefaultParam && !SymbolEqualityComparer.Default.Equals(thisData.TargetType, baseData.TargetType))
			{
				return configuration.AllowOverridingOfDefaultParamValues && thisData.TargetType!.IsValidForTypeParameter(thisData.Symbol);
			}

			return true;
		}

		private static bool ValidateParameterInBaseMethod(IDiagnosticReceiver diagnosticReceiver, in TypeParameterData thisData, in TypeParameterData baseData, DefaultParamConfiguration configuration)
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

		private static int GetNumAddedParameters(in TypeParameterContainer typeParameters, in TypeParameterContainer baseTypeParameters)
		{
			int diff = baseTypeParameters.FirstDefaultParamIndex - typeParameters.FirstDefaultParamIndex;
			return typeParameters.FirstDefaultParamIndex + diff;
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

		public static bool ValidateMethodSignature(IMethodSymbol symbol, in TypeParameterContainer typeParameters, DefaultParamConfiguration configuration)
		{
			return ValidateMethodSignature(symbol, in typeParameters, configuration, out _);
		}

		public static bool ValidateMethodSignature(IDiagnosticReceiver diagnosticReceiver, IMethodSymbol symbol, in TypeParameterContainer typeParameters, DefaultParamConfiguration configuration)
		{
			return ValidateMethodSignature(diagnosticReceiver, symbol, in typeParameters, configuration, out _);
		}

		public static bool ValidateMethodSignature(IMethodSymbol symbol, in TypeParameterContainer typeParameters, DefaultParamConfiguration configuration, out List<int>? typeParameterIndicesToApplyNewModifier)
		{
			IParameterSymbol[] symbolParameters = symbol.Parameters.ToArray();
			int numNonDefaultParam = typeParameters.Length - typeParameters.NumDefaultParam;

			CollidingMethod[] collidingMethods = GetCollidingMethods(symbol, in typeParameters, symbolParameters, numNonDefaultParam);
			int numMethods = collidingMethods.Length;

			if (numMethods == 0)
			{
				typeParameterIndicesToApplyNewModifier = new();
				return true;
			}

			return ValidateCollidingMethods(symbol, in typeParameters, collidingMethods, symbolParameters, configuration, out typeParameterIndicesToApplyNewModifier);
		}

		public static bool ValidateMethodSignature(IDiagnosticReceiver diagnosticReceiver, IMethodSymbol symbol, in TypeParameterContainer typeParameters, DefaultParamConfiguration configuration, out List<int>? typeParameterIndicesToApplyNewModifier)
		{
			IParameterSymbol[] symbolParameters = symbol.Parameters.ToArray();
			int numNonDefaultParam = typeParameters.Length - typeParameters.NumDefaultParam;

			CollidingMethod[] collidingMethods = GetCollidingMethods(symbol, in typeParameters, symbolParameters, numNonDefaultParam);
			int numMethods = collidingMethods.Length;

			if (numMethods == 0)
			{
				typeParameterIndicesToApplyNewModifier = new();
				return true;
			}

			return ValidateCollidingMethods(diagnosticReceiver, symbol, in typeParameters, collidingMethods, symbolParameters, configuration, out typeParameterIndicesToApplyNewModifier);
		}

		private static bool ValidateCollidingMethods(
			IDiagnosticReceiver diagnosticReceiver,
			IMethodSymbol symbol,
			in TypeParameterContainer typeParameters,
			CollidingMethod[] collidingMethods,
			IParameterSymbol[] symbolParameters,
			DefaultParamConfiguration configuration,
			out List<int>? typeParameterIndicesToApplyNewModifier
		)
		{
			ParameterGeneration[][] generations = GetParameterGenerations(in typeParameters, symbolParameters);
			int numParameters = symbolParameters.Length;
			int numMethods = collidingMethods.Length;
			bool isValid = true;
			HashSet<int> diagnosedGenerationIndices = new();
			List<int> applyNew = new(generations.Length);

			for (int i = 0; i < numMethods; i++)
			{
				ref readonly CollidingMethod currentMethod = ref collidingMethods[i];
				int targetIndex = typeParameters.NumDefaultParam - currentMethod.TypeParameters.Length;

				if (diagnosedGenerationIndices.Contains(targetIndex))
				{
					continue;
				}

				ParameterGeneration[] targetParameters = generations[targetIndex];

				if (HasCollidingParameters(targetParameters, in currentMethod, numParameters))
				{
					if (!configuration.ApplyNewToGeneratedMembersWithEquivalentSignature || SymbolEqualityComparer.Default.Equals(currentMethod.Symbol.ContainingType, symbol))
					{
						DurianDiagnostics.MethodWithSignatureAlreadyExists(diagnosticReceiver, symbol, GetMethodSignatureString(in typeParameters, targetParameters));
						diagnosedGenerationIndices.Add(targetIndex);
						isValid = false;
					}
					else if (isValid)
					{
						applyNew.Add(targetIndex);
					}
				}
			}

			typeParameterIndicesToApplyNewModifier = isValid ? applyNew : null;
			return isValid;
		}

		private static bool ValidateCollidingMethods(
			IMethodSymbol symbol,
			in TypeParameterContainer typeParameters,
			CollidingMethod[] collidingMethods,
			IParameterSymbol[] symbolParameters,
			DefaultParamConfiguration configuration,
			out List<int>? typeParameterIndicesToApplyNewModifier
		)
		{
			ParameterGeneration[][] generations = GetParameterGenerations(in typeParameters, symbolParameters);
			int numParameters = symbolParameters.Length;
			int numMethods = collidingMethods.Length;
			List<int> applyNew = new(generations.Length);

			for (int i = 0; i < numMethods; i++)
			{
				ref readonly CollidingMethod currentMethod = ref collidingMethods[i];
				int targetIndex = typeParameters.NumDefaultParam - currentMethod.TypeParameters.Length;

				ParameterGeneration[] targetParameters = generations[targetIndex];

				if (HasCollidingParameters(targetParameters, in currentMethod, numParameters))
				{
					if (!configuration.ApplyNewToGeneratedMembersWithEquivalentSignature || SymbolEqualityComparer.Default.Equals(currentMethod.Symbol.ContainingType, symbol))
					{
						typeParameterIndicesToApplyNewModifier = null;
						return false;
					}
					else
					{
						applyNew.Add(targetIndex);
					}
				}
			}

			typeParameterIndicesToApplyNewModifier = applyNew;
			return true;
		}

		private static string GetMethodSignatureString(in TypeParameterContainer typeParameters, ParameterGeneration[] parameters)
		{
			StringBuilder sb = new();

			sb.Append('<').Append(typeParameters[0].Symbol);
			int typeLength = typeParameters.Length;

			for (int i = 1; i < typeLength; i++)
			{
				sb.Append(", ").Append(typeParameters[i].Symbol);
			}

			sb.Append('>');

			int paramLength = parameters.Length;

			if (paramLength == 0)
			{
				sb.Append("()");
				return sb.ToString();
			}

			ref readonly ParameterGeneration first = ref parameters[0];
			sb.Append('(').Append(first.Type);

			for (int i = 1; i < paramLength; i++)
			{
				ref readonly ParameterGeneration parameter = ref parameters[i];
				sb.Append(", ").Append(parameter.Type);
			}

			sb.Append(')');
			return sb.ToString();
		}

		private static bool HasCollidingParameters(ParameterGeneration[] targetParameters, in CollidingMethod currentMethod, int numParameters)
		{
			for (int j = 0; j < numParameters; j++)
			{
				ref readonly ParameterGeneration generation = ref targetParameters[j];
				IParameterSymbol parameter = currentMethod.Parameters[j];

				if (IsValidParameterInCollidingMethod(in currentMethod, parameter, in generation))
				{
					return false;
				}
			}

			return true;
		}

		private static bool IsValidParameterInCollidingMethod(in CollidingMethod method, IParameterSymbol parameter, in ParameterGeneration generation)
		{
			if (parameter.Type is ITypeParameterSymbol)
			{
				if (generation.GenericParameterIndex > -1)
				{
					int typeParameterIndex = GetIndexOfTypeParameterInCollidingMethod(in method, parameter);

					if (generation.GenericParameterIndex == typeParameterIndex && !AnalysisUtilities.IsValidRefKindForOverload(parameter.RefKind, generation.RefKind))
					{
						return false;
					}
				}
			}
			else if (SymbolEqualityComparer.Default.Equals(parameter.Type, generation.Type) && !AnalysisUtilities.IsValidRefKindForOverload(parameter.RefKind, generation.RefKind))
			{
				return false;
			}

			return true;
		}

		private static CollidingMethod[] GetCollidingMethods(IMethodSymbol symbol, in TypeParameterContainer typeParameters, IParameterSymbol[] parameters, int numNonDefaultParam)
		{
			int numTypeParameters = typeParameters.Length;
			int numParameters = parameters.Length;

			return symbol.ContainingType.GetMembers(symbol.Name)
				.OfType<IMethodSymbol>()
				.Select(m => ((IMethodSymbol symbol, IParameterSymbol[] parameters))(m, m.Parameters.ToArray()))
				.Where(m => m.parameters.Length == numParameters)
				.Select(m => new CollidingMethod(m.symbol, m.parameters, m.symbol.TypeParameters.ToArray()))
				.Where(m => m.TypeParameters.Length >= numNonDefaultParam && m.TypeParameters.Length < numTypeParameters)
				.ToArray();
		}

		private static ParameterGeneration[][] GetParameterGenerations(in TypeParameterContainer typeParameters, IParameterSymbol[] symbolParameters)
		{
			int numParameters = symbolParameters.Length;
			ParameterGeneration[] defaultParameters = new ParameterGeneration[numParameters];

			for (int i = 0; i < numParameters; i++)
			{
				defaultParameters[i] = CreateDefaultGenerationForParameter(symbolParameters[i], in typeParameters);
			}

			return CreateParameterGenerationsForTypeParameters(in typeParameters, defaultParameters, numParameters);
		}

		private static ParameterGeneration[][] CreateParameterGenerationsForTypeParameters(in TypeParameterContainer typeParameters, ParameterGeneration[] defaultParameters, int numParameters)
		{
			ParameterGeneration[][] generations = new ParameterGeneration[typeParameters.NumDefaultParam][];
			ParameterGeneration[] previousParameters = defaultParameters;

			for (int i = typeParameters.Length - 1, genIndex = 0; i >= typeParameters.FirstDefaultParamIndex; i--, genIndex++)
			{
				ParameterGeneration[] currentParameters = new ParameterGeneration[numParameters];

				for (int j = 0; j < numParameters; j++)
				{
					ref readonly ParameterGeneration parameter = ref previousParameters[j];

					if (parameter.GenericParameterIndex == -1)
					{
						currentParameters[j] = parameter;
						continue;
					}

					ref readonly TypeParameterData data = ref typeParameters[parameter.GenericParameterIndex];

					if (data.IsDefaultParam)
					{
						currentParameters[j] = new ParameterGeneration(data.TargetType!, parameter.RefKind, parameter.GenericParameterIndex);
					}
					else
					{
						currentParameters[j] = parameter;
					}
				}

				previousParameters = currentParameters;
				generations[genIndex] = currentParameters;
			}

			return generations;
		}

		private static ParameterGeneration CreateDefaultGenerationForParameter(IParameterSymbol parameter, in TypeParameterContainer typeParameters)
		{
			ITypeSymbol type = parameter.Type;

			if (type is ITypeParameterSymbol)
			{
				for (int j = typeParameters.FirstDefaultParamIndex; j < typeParameters.Length; j++)
				{
					ref readonly TypeParameterData data = ref typeParameters[j];

					if (SymbolEqualityComparer.Default.Equals(data.Symbol, type))
					{
						return new ParameterGeneration(type, parameter.RefKind, j);
					}
				}
			}

			return new ParameterGeneration(type, parameter.RefKind, -1);
		}

		private static int GetIndexOfTypeParameterInCollidingMethod(in CollidingMethod method, IParameterSymbol parameter)
		{
			int currentTypeParameterCount = method.TypeParameters.Length;

			for (int k = 0; k < currentTypeParameterCount; k++)
			{
				if (SymbolEqualityComparer.Default.Equals(parameter.Type, method.TypeParameters[k]))
				{
					return k;
				}
			}

			// Not gonna happen
			return -1;
		}
	}
}
