using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using Durian.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Durian.DefaultParam
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class DefaultParamMethodAnalyzer : DefaultParamAnalyzer
	{
		//[DebuggerDisplay("{Symbol}")]
		//private sealed class DefaultParamCollidingMethod
		//{
		//	private readonly TypeParameterContainer _typeParameters;

		//	public ref readonly TypeParameterContainer TypeParameters => ref _typeParameters;
		//	public IParameterSymbol[] Parameters { get; }
		//	public IMethodSymbol Symbol { get; }

		//	public DefaultParamCollidingMethod(IMethodSymbol symbol, DefaultParamCompilationData compilation, CancellationToken cancellationToken)
		//	{
		//		_typeParameters = TypeParameterContainer.CreateFrom(symbol, compilation, cancellationToken);
		//		Parameters = symbol.Parameters.ToArray();
		//		Symbol = symbol;
		//	}
		//}

		[DebuggerDisplay("{Symbol}")]
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

		[DebuggerDisplay("{Type}")]
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
				return (RefKind != RefKind.None ? $"{RefKind} " : "") + Type.Name;
			}
		}

		public override SymbolKind SupportedSymbolKind => SymbolKind.Method;

		protected override IEnumerable<DiagnosticDescriptor> GetAnalyzerSpecificDiagnostics()
		{
			return new[]
			{
				DefaultParamDiagnostics.Descriptors.DefaultParamMethodCannotBePartialOrExtern,
				DefaultParamDiagnostics.Descriptors.DefaultParamAttributeIsNotValidOnLocalFunctions,
				DefaultParamDiagnostics.Descriptors.OverriddenDefaultParamAttributeShouldBeAddedForClarity,
				DefaultParamDiagnostics.Descriptors.DoNotOverrideMethodsGeneratedUsingDefaultParamAttribute,
				DefaultParamDiagnostics.Descriptors.DoNotAddDefaultParamAttributeOnOverriddenVirtualTypeParameter,
				DefaultParamDiagnostics.Descriptors.ValueOfDefaultParamAttributeMustBeTheSameAsValueForOverridenMethod,
				Descriptors.MethodWithSignatureAlreadyExists
			};
		}

		public override void Analyze(IDiagnosticReceiver diagnosticReceiver, ISymbol symbol, DefaultParamCompilationData compilation, CancellationToken cancellationToken = default)
		{
			if (symbol is not IMethodSymbol m || m.TypeParameters.Length == 0)
			{
				return;
			}

			AnalyzeWithDiagnostics(diagnosticReceiver, m, compilation, cancellationToken);
		}

		#region -Without Diagnostics-
		public static bool Analyze(IMethodSymbol symbol, DefaultParamCompilationData compilation, CancellationToken cancellationToken = default)
		{
			TypeParameterContainer typeParameters = TypeParameterContainer.CreateFrom(symbol, compilation, cancellationToken);

			if (typeParameters.HasDefaultParams)
			{
				if (!AnalyzeAgaintsLocalFunction(symbol))
				{
					return false;
				}
			}
			else if (!symbol.IsOverride)
			{
				return false;
			}

			return AnalyzeCore(symbol, compilation, ref typeParameters, cancellationToken);
		}

		private static bool AnalyzeCore(IMethodSymbol symbol, DefaultParamCompilationData compilation, ref TypeParameterContainer typeParameters, CancellationToken cancellationToken)
		{
			if (AnalyzeAgaintsPartialOrExtern(symbol, cancellationToken) &&
				AnalyzeAgainstGeneratedCodeAttribute(symbol, compilation) &&
				AnalyzeContainingTypes(symbol, cancellationToken))
			{
				if ((IsOverride(symbol, out IMethodSymbol? baseMethod) &&
					AnalyzeOverrideMethod(baseMethod, ref typeParameters, compilation, cancellationToken)) ||
					AnalyzeTypeParameters(in typeParameters))
				{
					return AnalyzeMethodSignature(symbol, typeParameters, compilation, cancellationToken);
				}
			}

			return false;
		}

		public static bool AnalyzeOverrideMethod(IMethodSymbol? baseMethod, ref TypeParameterContainer typeParameters, DefaultParamCompilationData compilation, CancellationToken cancellationToken)
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

			if (typeParameters.FirstDefaultParamIndex == -1)
			{
				bool isValid = AnalyzeTypeParameters(in baseTypeParameters);

				if (isValid)
				{
					isValid &= AnalyzeBaseMethodParameters(compilation.Configuration, in typeParameters, in baseTypeParameters);
				}

				typeParameters = TypeParameterContainer.Combine(in typeParameters, in baseTypeParameters);
				return isValid;
			}
			else if (HasAddedDefaultParamAttributes(in typeParameters, in baseTypeParameters))
			{
				return
					TryUpdateTypeParameters(ref typeParameters, in baseTypeParameters, compilation.Configuration) &&
					AnalyzeTypeParameters(in typeParameters) &&
					AnalyzeBaseMethodParameters(compilation.Configuration, in typeParameters, in baseTypeParameters);
			}
			else
			{
				typeParameters = TypeParameterContainer.Combine(in typeParameters, in baseTypeParameters);

				return AnalyzeTypeParameters(in typeParameters) && AnalyzeBaseMethodParameters(compilation.Configuration, in typeParameters, in baseTypeParameters);
			}
		}

		public static bool AnalyzeAgaintsLocalFunction(IMethodSymbol symbol)
		{
			return symbol.MethodKind != MethodKind.LocalFunction;
		}

		public static bool AnalyzeAgaintsPartialOrExtern(IMethodSymbol symbol, CancellationToken cancellationToken = default)
		{
			if (symbol?.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax(cancellationToken) is not MethodDeclarationSyntax declaration)
			{
				return false;
			}

			return AnalyzeAgaintsPartialOrExtern(symbol, declaration);
		}

		public static bool AnalyzeAgaintsPartialOrExtern(IMethodSymbol symbol, MethodDeclarationSyntax declaration)
		{
			return !symbol.IsExtern && !symbol.IsPartial(declaration);
		}

		public static bool AnalyzeMethodSignature(IMethodSymbol symbol, in TypeParameterContainer typeParameters, DefaultParamCompilationData compilation, CancellationToken cancellationToken = default)
		{
			return AnalyzeMethodSignature(symbol, in typeParameters, compilation, out _, cancellationToken);
		}

		public static bool AnalyzeMethodSignature(
			IMethodSymbol symbol,
			in TypeParameterContainer typeParameters,
			DefaultParamCompilationData compilation,
			out HashSet<int>? applyNew,
			CancellationToken cancellationToken = default
		)
		{
			IParameterSymbol[] symbolParameters = symbol.Parameters.ToArray();
			CollidingMethod[] collidingMethods = GetCollidingMethods(symbol, symbolParameters.Length, typeParameters.Length, typeParameters.NumNonDefaultParam);

			if (collidingMethods.Length == 0)
			{
				applyNew = null;
				return true;
			}

			return AnalyzeCollidingMethods(
				symbol,
				in typeParameters,
				collidingMethods,
				symbolParameters,
				compilation.Configuration,
				cancellationToken,
				out applyNew
			);
		}

		private static bool AnalyzeBaseMethodParameters(DefaultParamConfiguration configuration, in TypeParameterContainer typeParameters, in TypeParameterContainer baseTypeParameters)
		{
			int length = baseTypeParameters.Length;

			for (int i = baseTypeParameters.FirstDefaultParamIndex; i < length; i++)
			{
				ref readonly TypeParameterData baseData = ref baseTypeParameters[i];
				ref readonly TypeParameterData thisData = ref typeParameters[i];

				if (!AnalyzeParameterInBaseMethod(in thisData, in baseData, configuration))
				{
					return false;
				}
			}

			return true;
		}

		private static bool AnalyzeParameterInBaseMethod(in TypeParameterData thisData, in TypeParameterData baseData, DefaultParamConfiguration configuration)
		{
			if (baseData.IsDefaultParam && thisData.IsDefaultParam && !SymbolEqualityComparer.Default.Equals(thisData.TargetType, baseData.TargetType))
			{
				return configuration.AllowOverridingOfDefaultParamValues && thisData.TargetType!.IsValidForTypeParameter(thisData.Symbol);
			}

			return true;
		}
		private static bool AnalyzeCollidingMethods(
			IMethodSymbol symbol,
			in TypeParameterContainer typeParameters,
			CollidingMethod[] collidingMethods,
			IParameterSymbol[] symbolParameters,
			DefaultParamConfiguration configuration,
			CancellationToken cancellationToken,
			out HashSet<int>? applyNew
		)
		{
			ParameterGeneration[][] generations = GetParameterGenerations(in typeParameters, symbolParameters);

			HashSet<int> applyNewLocal = new();
			int numMethods = collidingMethods.Length;

			for (int i = 0; i < numMethods; i++)
			{
				ref readonly CollidingMethod currentMethod = ref collidingMethods[i];
				int targetIndex = currentMethod.TypeParameters.Length - typeParameters.NumNonDefaultParam;

				ParameterGeneration[] targetParameters = generations[targetIndex];

				if (HasCollidingParameters(targetParameters, in currentMethod))
				{
					if (!configuration.ApplyNewToGeneratedMembersWithEquivalentSignature || SymbolEqualityComparer.Default.Equals(currentMethod.Symbol.ContainingType, symbol))
					{
						if (!HasNewModifier(symbol, currentMethod.Symbol, cancellationToken))
						{
							applyNew = null;
							return false;
						}
					}

					applyNewLocal.Add(targetIndex);
				}
			}

			applyNew = GetApplyNewOrNull(applyNewLocal);
			return true;
		}
		#endregion

		#region -With Diagnostics-
		public static bool AnalyzeWithDiagnostics(IDiagnosticReceiver diagnosticReceiver, IMethodSymbol symbol, DefaultParamCompilationData compilation, CancellationToken cancellationToken = default)
		{
			TypeParameterContainer typeParameters = TypeParameterContainer.CreateFrom(symbol, compilation, cancellationToken);

			if (typeParameters.HasDefaultParams)
			{
				if (!AnalyzeAgaintsLocalFunctionWithDiagnostics(diagnosticReceiver, symbol))
				{
					return false;
				}
			}
			else if (!symbol.IsOverride)
			{
				return false;
			}

			return AnalyzeWithDiagnosticsCore(diagnosticReceiver, symbol, compilation, ref typeParameters, cancellationToken);
		}

		private static bool AnalyzeWithDiagnosticsCore(IDiagnosticReceiver diagnosticReceiver, IMethodSymbol symbol, DefaultParamCompilationData compilation, ref TypeParameterContainer typeParameters, CancellationToken cancellationToken)
		{
			bool isValid = AnalyzeAgaintsPartialOrExternWithDiagnostics(diagnosticReceiver, symbol, cancellationToken);
			isValid &= AnalyzeAgainstGeneratedCodeAttributeWithDiagnostics(diagnosticReceiver, symbol, compilation);
			isValid &= AnalyzeContainingTypesWithDiagnostics(diagnosticReceiver, symbol, cancellationToken);

			bool hasValidTypeParameters;

			if (IsOverride(symbol, out IMethodSymbol? baseMethod))
			{
				isValid &= AnalyzeOverrideMethodWithDiagnostics(diagnosticReceiver, symbol, baseMethod, ref typeParameters, compilation, cancellationToken, out hasValidTypeParameters);
			}
			else
			{
				hasValidTypeParameters = AnalyzeTypeParametersWithDiagnostics(diagnosticReceiver, in typeParameters);
				isValid &= hasValidTypeParameters;
			}

			if (hasValidTypeParameters)
			{
				isValid &= AnalyzeMethodSignatureWithDiagnostics(diagnosticReceiver, symbol, typeParameters, compilation, cancellationToken);
			}

			return isValid;
		}

		public static bool AnalyzeOverrideMethodWithDiagnostics(IDiagnosticReceiver diagnosticReceiver, IMethodSymbol symbol, IMethodSymbol? baseMethod, ref TypeParameterContainer typeParameters, DefaultParamCompilationData compilation, CancellationToken cancellationToken, out bool hasValidTypeParameters)
		{
			if (baseMethod is null)
			{
				hasValidTypeParameters = false;
				return false;
			}

			if (IsDefaultParamGenerated(baseMethod, compilation))
			{
				DefaultParamDiagnostics.DoNotOverrideMethodsGeneratedUsingDefaultParamAttribute(diagnosticReceiver, symbol);
				hasValidTypeParameters = AnalyzeTypeParametersWithDiagnostics(diagnosticReceiver, in typeParameters);
				return false;
			}

			TypeParameterContainer baseTypeParameters = GetBaseMethodTypeParameters(baseMethod, compilation, cancellationToken);
			bool isValid;

			if (typeParameters.FirstDefaultParamIndex == -1)
			{
				isValid = AnalyzeTypeParametersWithDiagnostics(diagnosticReceiver, in baseTypeParameters);
				hasValidTypeParameters = isValid;

				if (isValid)
				{
					isValid &= AnalyzeBaseMethodParametersWithDiagnostics(diagnosticReceiver, compilation.Configuration, in typeParameters, in baseTypeParameters);
				}

				typeParameters = TypeParameterContainer.Combine(in typeParameters, in baseTypeParameters);
			}
			else if (HasAddedDefaultParamAttributes(in typeParameters, in baseTypeParameters))
			{
				bool addingTypeParametersIsValid = TryUpdateTypeParameters(ref typeParameters, in baseTypeParameters, compilation.Configuration);
				isValid = AnalyzeTypeParametersWithDiagnostics(diagnosticReceiver, in typeParameters);
				hasValidTypeParameters = isValid;

				if (isValid)
				{
					isValid &= AnalyzeBaseMethodParametersWithDiagnostics(diagnosticReceiver, compilation.Configuration, in typeParameters, in baseTypeParameters);

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
				isValid = AnalyzeTypeParametersWithDiagnostics(diagnosticReceiver, in typeParameters);
				hasValidTypeParameters = isValid;

				if (isValid)
				{
					isValid &= AnalyzeBaseMethodParametersWithDiagnostics(diagnosticReceiver, compilation.Configuration, in typeParameters, in baseTypeParameters);
				}
			}

			return isValid;
		}

		public static bool AnalyzeAgaintsLocalFunctionWithDiagnostics(IDiagnosticReceiver diagnosticReceiver, IMethodSymbol symbol)
		{
			if (symbol.MethodKind == MethodKind.LocalFunction)
			{
				ReportDiagnosticForLocalFunction(diagnosticReceiver, symbol);
				return false;
			}

			return true;
		}

		public static bool AnalyzeAgaintsPartialOrExternWithDiagnostics(IDiagnosticReceiver diagnosticReceiver, IMethodSymbol symbol, CancellationToken cancellationToken = default)
		{
			if (symbol?.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax(cancellationToken) is not MethodDeclarationSyntax declaration)
			{
				return false;
			}

			return AnalyzeAgaintsPartialOrExternWithDiagnostics(diagnosticReceiver, symbol, declaration);
		}

		public static bool AnalyzeAgaintsPartialOrExternWithDiagnostics(IDiagnosticReceiver diagnosticReceiver, IMethodSymbol symbol, MethodDeclarationSyntax declaration)
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

		public static bool AnalyzeMethodSignatureWithDiagnostics(IDiagnosticReceiver diagnosticReceiver, IMethodSymbol symbol, in TypeParameterContainer typeParameters, DefaultParamCompilationData compilation, CancellationToken cancellationToken = default)
		{
			return AnalyzeMethodSignatureWithDiagnostics(diagnosticReceiver, symbol, in typeParameters, compilation, out _, cancellationToken);
		}

		public static bool AnalyzeMethodSignatureWithDiagnostics(
			IDiagnosticReceiver diagnosticReceiver,
			IMethodSymbol symbol,
			in TypeParameterContainer typeParameters,
			DefaultParamCompilationData compilation,
			out HashSet<int>? applyNew,
			CancellationToken cancellationToken = default
		)
		{
			IParameterSymbol[] symbolParameters = symbol.Parameters.ToArray();
			CollidingMethod[] collidingMethods = GetCollidingMethods(symbol, symbolParameters.Length, typeParameters.Length, typeParameters.NumNonDefaultParam);

			if (collidingMethods.Length == 0)
			{
				applyNew = null;
				return true;
			}

			return AnalyzeCollidingMethodsWithDiagnostics(
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

		private static bool AnalyzeCollidingMethodsWithDiagnostics(
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

				if (!AnalyzeCollidingMethodParametersWithDiagnostics(
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

		private static bool AnalyzeCollidingMethodParametersWithDiagnostics(
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

		private static bool HasNewModifier(IMethodSymbol symbol, IMethodSymbol collidingMethod, CancellationToken cancellationToken)
		{
			bool isInBaseType = symbol.ContainingType.GetBaseTypes().Any(m => SymbolEqualityComparer.Default.Equals(collidingMethod.ContainingType, m));

			if (!isInBaseType)
			{
				return false;
			}

			if (symbol.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax(cancellationToken) is MethodDeclarationSyntax m)
			{
				return m.Modifiers.Any(m => m.IsKind(SyntaxKind.NewKeyword));
			}

			return false;
		}

		private static bool AnalyzeBaseMethodParametersWithDiagnostics(IDiagnosticReceiver diagnosticReceiver, DefaultParamConfiguration configuration, in TypeParameterContainer typeParameters, in TypeParameterContainer baseTypeParameters)
		{
			int length = baseTypeParameters.Length;
			bool isValid = true;

			int firstIndex = GetFirstDefaultParamIndex(in typeParameters, in baseTypeParameters);

			for (int i = firstIndex; i < length; i++)
			{
				ref readonly TypeParameterData baseData = ref baseTypeParameters[i];
				ref readonly TypeParameterData thisData = ref typeParameters[i];

				if (!AnalyzeParameterInBaseMethodWithDiagnostics(diagnosticReceiver, in thisData, in baseData, configuration))
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

		private static bool AnalyzeParameterInBaseMethodWithDiagnostics(IDiagnosticReceiver diagnosticReceiver, in TypeParameterData thisData, in TypeParameterData baseData, DefaultParamConfiguration configuration)
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

		#endregion

		public static bool CheckShouldCallInsteadOfCopying(IMethodSymbol method, DefaultParamCompilationData compilation)
		{
			return CheckShouldCallInsteadOfCopying(method.GetAttributes(), compilation);
		}

		public static bool CheckShouldCallInsteadOfCopying(IEnumerable<AttributeData> attributes, DefaultParamCompilationData compilation)
		{
			if (attributes is null)
			{
				return compilation.Configuration.CallInsteadOfCopying;
			}

			AttributeData? attr = attributes.FirstOrDefault(attr => SymbolEqualityComparer.Default.Equals(attr.AttributeClass, compilation.MethodConfigurationAttribute));

			if (attr is not null && attr.TryGetNamedArgumentValue(DefaultParamMethodConfigurationAttribute.CallInsteadOfCopyingProperty, out bool value))
			{
				return value;
			}

			return compilation.Configuration.CallInsteadOfCopying;
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

		private static int GetNumAddedParameters(in TypeParameterContainer typeParameters, in TypeParameterContainer baseTypeParameters)
		{
			int diff = baseTypeParameters.FirstDefaultParamIndex - typeParameters.FirstDefaultParamIndex;
			return typeParameters.FirstDefaultParamIndex + diff;
		}

		private static string GetMethodSignatureString(string name, in TypeParameterContainer typeParameters, int numTypeParameters, ParameterGeneration[] parameters)
		{
			StringBuilder sb = new();

			sb.Append(name);

			if (numTypeParameters > 0)
			{
				sb.Append('<').Append(typeParameters[0].Symbol);

				for (int i = 1; i < numTypeParameters; i++)
				{
					sb.Append(", ").Append(typeParameters[i].Symbol);
				}

				sb.Append('>');
			}

			int paramLength = parameters.Length;

			if (paramLength == 0)
			{
				sb.Append("()");
				return sb.ToString();
			}

			ref readonly ParameterGeneration first = ref parameters[0];
			sb.Append('(');
			WriteParameter(in first);

			for (int i = 1; i < paramLength; i++)
			{
				ref readonly ParameterGeneration parameter = ref parameters[i];
				sb.Append(", ");
				WriteParameter(in parameter);
			}

			sb.Append(')');
			return sb.ToString();

			void WriteParameter(in ParameterGeneration param)
			{
				if (param.RefKind != RefKind.None)
				{
					sb.Append(param.RefKind.ToString().ToLower());
				}

				sb.Append(param.Type);
			}
		}

		private static bool HasCollidingParameters(ParameterGeneration[] targetParameters, in CollidingMethod collidingMethod)
		{
			int numParameters = targetParameters.Length;

			for (int i = 0; i < numParameters; i++)
			{
				ref readonly ParameterGeneration generation = ref targetParameters[i];
				IParameterSymbol parameter = collidingMethod.Parameters[i];

				if (IsValidParameterInCollidingMethod(collidingMethod.TypeParameters, parameter, in generation))
				{
					return false;
				}
			}

			return true;
		}

		private static bool IsValidParameterInCollidingMethod(ITypeParameterSymbol[] typeParameters, IParameterSymbol parameter, in ParameterGeneration targetGeneration)
		{
			if (parameter.Type is ITypeParameterSymbol)
			{
				if (targetGeneration.GenericParameterIndex > -1)
				{
					int typeParameterIndex = GetIndexOfTypeParameterInCollidingMethod(typeParameters, parameter);

					if (targetGeneration.GenericParameterIndex == typeParameterIndex && !AnalysisUtilities.IsValidRefKindForOverload(parameter.RefKind, targetGeneration.RefKind))
					{
						return false;
					}
				}
			}
			else if (SymbolEqualityComparer.Default.Equals(parameter.Type, targetGeneration.Type) && !AnalysisUtilities.IsValidRefKindForOverload(parameter.RefKind, targetGeneration.RefKind))
			{
				return false;
			}

			return true;
		}

		private static CollidingMethod[] GetCollidingMethods(IMethodSymbol symbol, int numParameters, int numTypeParameters, int numNonDefaultParam)
		{
			return symbol.ContainingType.GetAllMembers(symbol.Name)
				.OfType<IMethodSymbol>()
				.Select(m => ((IMethodSymbol symbol, ITypeParameterSymbol[] typeParameters))(m, m.TypeParameters.ToArray()))
				.Where(m => m.typeParameters.Length >= numNonDefaultParam && m.typeParameters.Length < numTypeParameters)
				.Select(m => new CollidingMethod(m.symbol, m.symbol.Parameters.ToArray(), m.typeParameters))
				.Where(m => m.Parameters.Length == numParameters)
				.ToArray();
		}

		private static ParameterGeneration[][] GetParameterGenerations(in TypeParameterContainer typeParameters, IParameterSymbol[] symbolParameters)
		{
			int numParameters = symbolParameters.Length;
			ParameterGeneration[] defaultParameters = new ParameterGeneration[numParameters];
			bool hasTypeArgumentAsParameter = false;

			for (int i = 0; i < numParameters; i++)
			{
				IParameterSymbol parameter = symbolParameters[i];
				ITypeSymbol type = parameter.Type;

				if (type is ITypeParameterSymbol)
				{
					hasTypeArgumentAsParameter = true;
					defaultParameters[i] = CreateDefaultGenerationForTypeArgumentParameter(parameter, in typeParameters);
				}
				else
				{
					defaultParameters[i] = new ParameterGeneration(type, parameter.RefKind, -1);
				}
			}

			if (!hasTypeArgumentAsParameter)
			{
				int numTypeParameters = typeParameters.Length;
				ParameterGeneration[][] generations = new ParameterGeneration[numTypeParameters][];

				for (int i = 0; i < numTypeParameters; i++)
				{
					generations[i] = defaultParameters;
				}

				return generations;
			}
			else
			{
				return CreateParameterGenerationsForTypeParameters(in typeParameters, defaultParameters, numParameters);
			}
		}

		private static ParameterGeneration CreateDefaultGenerationForTypeArgumentParameter(IParameterSymbol parameter, in TypeParameterContainer typeParameters)
		{
			ITypeSymbol type = parameter.Type;

			for (int i = 0; i < typeParameters.Length; i++)
			{
				ref readonly TypeParameterData data = ref typeParameters[i];

				if (SymbolEqualityComparer.Default.Equals(data.Symbol, type))
				{
					return new ParameterGeneration(type, parameter.RefKind, i);
				}
			}

			throw new InvalidOperationException($"Unknown type parameter used as argument for parameter '{parameter.Name}'");
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

		private static int GetIndexOfTypeParameterInCollidingMethod(ITypeParameterSymbol[] typeParameters, IParameterSymbol parameter)
		{
			int currentTypeParameterCount = typeParameters.Length;

			for (int i = 0; i < currentTypeParameterCount; i++)
			{
				if (SymbolEqualityComparer.Default.Equals(parameter.Type, typeParameters[i]))
				{
					return i;
				}
			}

			throw new InvalidOperationException($"Unknown parameter: {parameter}");
		}

		private static HashSet<int>? GetApplyNewOrNull(HashSet<int> applyNew)
		{
			if (applyNew.Count == 0)
			{
				return null;
			}

			return applyNew;
		}
	}
}
