using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Collections.Immutable;
using Durian.Generator.Extensions;
using Microsoft.CodeAnalysis;
using Durian.Configuration;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Durian.Generator.DefaultParam
{
	/// <summary>
	/// Analyzes methods with type parameters marked by the <see cref="DefaultParamAttribute"/>.
	/// </summary>
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public partial class DefaultParamMethodAnalyzer : DefaultParamAnalyzer
	{
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

		/// <inheritdoc/>
		public override SymbolKind SupportedSymbolKind => SymbolKind.Method;

		/// <inheritdoc/>
		protected override IEnumerable<DiagnosticDescriptor> GetAnalyzerSpecificDiagnostics()
		{
			return new[]
			{
				DefaultParamDiagnostics.DUR0102_MethodCannotBePartialOrExtern,
				DefaultParamDiagnostics.DUR0103_DefaultParamIsNotValidOnLocalFunctionsOrLambdas,
				DefaultParamDiagnostics.DUR0107_DoNotOverrideGeneratedMethods,
				DefaultParamDiagnostics.DUR0108_ValueOfOverriddenMethodMustBeTheSameAsBase,
				DefaultParamDiagnostics.DUR0109_DoNotAddDefaultParamAttributeOnOverridenParameters,
				DefaultParamDiagnostics.DUR0110_OverriddenDefaultParamAttribuetShouldBeAddedForClarity,
				DefaultParamDiagnostics.DUR0116_DoNotChangeDefaultParamValueOfImplementedMethod,
				DefaultParamDiagnostics.DUR0118_MethodWithSignatureAlreadyExists
			};
		}

		/// <inheritdoc/>
		protected override void Register(CompilationStartAnalysisContext context, DefaultParamCompilationData compilation)
		{
			base.Register(context, compilation);
			context.RegisterSyntaxNodeAction(c => FindAndAnalyzeLocalFunction(c, compilation), SyntaxKind.LocalFunctionStatement);
		}

		/// <inheritdoc/>
		public override void Analyze(IDiagnosticReceiver diagnosticReceiver, ISymbol symbol, DefaultParamCompilationData compilation, CancellationToken cancellationToken = default)
		{
			if (symbol is not IMethodSymbol m || (m.TypeParameters.Length == 0 && !m.IsOverride))
			{
				return;
			}

			WithDiagnostics.Analyze(diagnosticReceiver, m, compilation, cancellationToken);
		}

		/// <summary>
		/// Fully analyzes the specified <paramref name="symbol"/>.
		/// </summary>
		/// <param name="symbol"><see cref="IMethodSymbol"/> to analyze.</param>
		/// <param name="compilation">Current <see cref="DefaultParamCompilationData"/>.</param>
		/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
		/// <returns><see langword="true"/> if the <paramref name="symbol"/> is valid, otherwise <see langword="false"/>.</returns>
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

		//private static bool AnalyzeInterfaceImplementation(IMethodSymbol symbol, ref TypeParameterContainer typeParameters)
		//{
		//	if(symbol.MethodKind == MethodKind.ExplicitInterfaceImplementation)
		//	{

		//	}

		//	ImmutableArray<IMethodSymbol> implementedMethods = symbol.ExplicitInterfaceImplementations;

		//	if(implementedMethods.Length == 0)
		//	{
		//		return true;
		//	}

		//	foreach (IMethodSymbol item in collection)
		//	{

		//	}
		//}

		/// <summary>
		/// Analyzes, if the <paramref name="symbol"/> has valid <paramref name="typeParameters"/> when compared to the <paramref name="symbol"/>'s base method.
		/// </summary>
		/// <param name="symbol"><see cref="IMethodSymbol"/> to analyze.</param>
		/// <param name="typeParameters"><see cref="TypeParameterContainer"/> that contains type parameters to be analyzed.</param>
		/// <param name="compilation">Current <see cref="DefaultParamCompilationData"/>.</param>
		/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
		/// <returns><see langword="true"/> if the <paramref name="symbol"/> is valid, otherwise <see langword="false"/>.</returns>
		public static bool AnalyzeBaseMethodAndTypeParameters(IMethodSymbol symbol, ref TypeParameterContainer typeParameters, DefaultParamCompilationData compilation, CancellationToken cancellationToken = default)
		{
			if (!symbol.IsOverride)
			{
				return AnalyzeTypeParameters(in typeParameters);
			}

			IMethodSymbol? baseMethod = symbol.OverriddenMethod;

			if (baseMethod is null)
			{
				return false;
			}

			if (IsDefaultParamGenerated(baseMethod, compilation))
			{
				return false;
			}

			TypeParameterContainer baseTypeParameters = GetBaseMethodTypeParameters(baseMethod, compilation, cancellationToken);

			if (HasAddedDefaultParamAttributes(in typeParameters, in baseTypeParameters) ||
				!AnalyzeTypeParameters(in baseTypeParameters) ||
				!AnalyzeBaseMethodParameters(in typeParameters, in baseTypeParameters))
			{
				return false;
			}

			typeParameters = TypeParameterContainer.Combine(in typeParameters, in baseTypeParameters);
			return true;
		}

		/// <summary>
		/// Analyzes, if the <paramref name="symbol"/> is a local function.
		/// </summary>
		/// <param name="symbol"><see cref="IMethodSymbol"/> to analyze.</param>
		/// <returns><see langword="true"/> if the <paramref name="symbol"/> is valid (is not local function), otherwise <see langword="false"/>.</returns>
		public static bool AnalyzeAgaintsLocalFunction(IMethodSymbol symbol)
		{
			return symbol.MethodKind != MethodKind.LocalFunction;
		}

		/// <summary>
		/// Analyzes, if the <paramref name="symbol"/> or either <see langword="partial"/> or <see langword="extern"/>.
		/// </summary>
		/// <param name="symbol"><see cref="IMethodSymbol"/> to analyze.</param>
		/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
		/// <returns><see langword="true"/> if the <paramref name="symbol"/> is valid (is not <see langword="partial"/> or <see langword="extern"/>), otherwise <see langword="false"/>.</returns>
		public static bool AnalyzeAgaintsPartialOrExtern(IMethodSymbol symbol, CancellationToken cancellationToken = default)
		{
			if (symbol?.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax(cancellationToken) is not MethodDeclarationSyntax declaration)
			{
				return false;
			}

			return AnalyzeAgaintsPartialOrExtern(symbol, declaration);
		}

		/// <summary>
		/// Analyzes, if the <paramref name="symbol"/> or either <see langword="partial"/> or <see langword="extern"/>.
		/// </summary>
		/// <param name="symbol"><see cref="IMethodSymbol"/> to analyze.</param>
		/// <param name="declaration">Main <see cref="MethodDeclarationSyntax"/> of the <paramref name="symbol"/>.</param>
		/// <returns><see langword="true"/> if the <paramref name="symbol"/> is valid (is not <see langword="partial"/> or <see langword="extern"/>), otherwise <see langword="false"/>.</returns>
		public static bool AnalyzeAgaintsPartialOrExtern(IMethodSymbol symbol, MethodDeclarationSyntax declaration)
		{
			return !symbol.IsExtern && !symbol.IsPartial(declaration);
		}

		/// <summary>
		/// Analyzes, if the signature of the <paramref name="symbol"/> is valid.
		/// </summary>
		/// <param name="symbol"><see cref="IMethodSymbol"/> to analyze the signature of.</param>
		/// <param name="typeParameters"><see cref="TypeParameterContainer"/> containing type parameters of the <paramref name="symbol"/>.</param>
		/// <param name="compilation">Current <see cref="DefaultParamCompilationData"/>.</param>
		/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
		/// <returns><see langword="true"/> if the signature of <paramref name="symbol"/> is valid, otherwise <see langword="false"/>.</returns>
		public static bool AnalyzeMethodSignature(IMethodSymbol symbol, in TypeParameterContainer typeParameters, DefaultParamCompilationData compilation, CancellationToken cancellationToken = default)
		{
			return AnalyzeMethodSignature(symbol, in typeParameters, compilation, out _, cancellationToken);
		}

		/// <summary>
		/// Analyzes, if the signature of the <paramref name="symbol"/> is valid. If so, returns a <see cref="HashSet{T}"/> of indexes of type parameters with the <see cref="DefaultParamAttribute"/> applied for whom the <see langword="new"/> modifier should be applied.
		/// </summary>
		/// <param name="symbol"><see cref="IMethodSymbol"/> to analyze the signature of.</param>
		/// <param name="typeParameters"><see cref="TypeParameterContainer"/> containing type parameters of the <paramref name="symbol"/>.</param>
		/// <param name="compilation">Current <see cref="DefaultParamCompilationData"/>.</param>
		/// <param name="applyNew"><see langword="abstract"/><see cref="HashSet{T}"/> of indexes of type parameters with the <see cref="DefaultParamAttribute"/> applied for whom the <see langword="new"/> modifier should be applied. -or- <see langword="null"/> if the <paramref name="symbol"/> is not valid.</param>
		/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
		/// <returns><see langword="true"/> if the signature of <paramref name="symbol"/> is valid, otherwise <see langword="false"/>.</returns>
		public static bool AnalyzeMethodSignature(
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
		/// Determines, whether the <see cref="DefaultParamGenerator"/> should call the target <paramref name="method"/> instead of copying its contents.
		/// </summary>
		/// <param name="method"><see cref="IMethodSymbol"/> to check.</param>
		/// <param name="compilation">Current <see cref="DefaultParamCompilationData"/>.</param>
		public static bool CheckShouldCallInsteadOfCopying(IMethodSymbol method, DefaultParamCompilationData compilation)
		{
			return CheckShouldCallInsteadOfCopying(method.GetAttributes(), compilation);
		}

		/// <summary>
		/// Determines, whether the <see cref="DefaultParamGenerator"/> should call a <see cref="IMethodSymbol"/> instead of copying its contents.
		/// </summary>
		/// <param name="attributes">A collection of <see cref="IMethodSymbol"/>' attributes.</param>
		/// <param name="compilation">Current <see cref="DefaultParamCompilationData"/>.</param>
		public static bool CheckShouldCallInsteadOfCopying(IEnumerable<AttributeData> attributes, DefaultParamCompilationData compilation)
		{
			DPMethodConvention valueToCompare;

			if (attributes is null)
			{
				valueToCompare = compilation.Configuration.MethodConvention;
			}
			else
			{
				AttributeData? attr = attributes.FirstOrDefault(attr => SymbolEqualityComparer.Default.Equals(attr.AttributeClass, compilation.ConfigurationAttribute));

				if (attr is not null && attr.TryGetNamedArgumentValue(nameof(DefaultParamConfigurationAttribute.MethodConvention), out int value))
				{
					valueToCompare = (DPMethodConvention)value;
				}
				else
				{
					valueToCompare = compilation.Configuration.MethodConvention;
				}
			}

			return valueToCompare == DPMethodConvention.Call;
		}

		private static void FindAndAnalyzeLocalFunction(SyntaxNodeAnalysisContext context, DefaultParamCompilationData compilation)
		{
			if (context.Node is not LocalFunctionStatementSyntax l)
			{
				return;
			}

			ISymbol? symbol = context.SemanticModel.GetDeclaredSymbol(l);

			if (symbol is not IMethodSymbol m)
			{
				return;
			}

			ITypeParameterSymbol[] typeParameters = m.TypeParameters.ToArray();

			if (typeParameters.Any(t => t.HasAttribute(compilation.MainAttribute!)))
			{
				DiagnosticDescriptor d = DefaultParamDiagnostics.DUR0103_DefaultParamIsNotValidOnLocalFunctionsOrLambdas;
				context.ReportDiagnostic(Diagnostic.Create(d, l.GetLocation(), m));
			}
		}

		private static bool AnalyzeCore(IMethodSymbol symbol, DefaultParamCompilationData compilation, ref TypeParameterContainer typeParameters, CancellationToken cancellationToken)
		{
			return
				AnalyzeAgaintsPartialOrExtern(symbol, cancellationToken) &&
				AnalyzeAgaintsProhibitedAttributes(symbol, compilation) &&
				AnalyzeContainingTypes(symbol, cancellationToken) &&
				AnalyzeBaseMethodAndTypeParameters(symbol, ref typeParameters, compilation, cancellationToken) &&
				AnalyzeMethodSignature(symbol, typeParameters, compilation, cancellationToken);
		}

		private static bool AnalyzeBaseMethodParameters(in TypeParameterContainer typeParameters, in TypeParameterContainer baseTypeParameters)
		{
			int length = baseTypeParameters.Length;

			for (int i = baseTypeParameters.FirstDefaultParamIndex; i < length; i++)
			{
				ref readonly TypeParameterData baseData = ref baseTypeParameters[i];
				ref readonly TypeParameterData thisData = ref typeParameters[i];

				if (!AnalyzeParameterInBaseMethod(in thisData, in baseData))
				{
					return false;
				}
			}

			return true;
		}

		private static bool AnalyzeParameterInBaseMethod(in TypeParameterData thisData, in TypeParameterData baseData)
		{
			if (baseData.IsDefaultParam)
			{
				if (thisData.IsDefaultParam && !SymbolEqualityComparer.Default.Equals(thisData.TargetType, baseData.TargetType))
				{
					return false;
				}
			}
			else if (thisData.IsDefaultParam)
			{
				return false;
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
					if (!configuration.ApplyNewModifierWhenPossible || SymbolEqualityComparer.Default.Equals(currentMethod.Symbol.ContainingType, symbol))
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

		private static TypeParameterContainer GetBaseMethodTypeParameters(IMethodSymbol baseMethod, DefaultParamCompilationData compilation, CancellationToken cancellationToken)
		{
			return TypeParameterContainer.CreateFrom(baseMethod, compilation, cancellationToken);
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

		private static bool HasAddedDefaultParamAttributes(in TypeParameterContainer typeParameters, in TypeParameterContainer baseTypeParameters)
		{
			if (typeParameters.FirstDefaultParamIndex == -1)
			{
				return false;
			}

			return typeParameters.FirstDefaultParamIndex < baseTypeParameters.FirstDefaultParamIndex;
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
					sb.Append(param.RefKind.ToString().ToLower()).Append(' ');
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

		private static CollidingMethod[] GetCollidingMethods(IMethodSymbol symbol, DefaultParamCompilationData compilation, int numParameters, int numTypeParameters, int numNonDefaultParam)
		{
			string fullName = symbol.ToString();
			INamedTypeSymbol generatedFrom = compilation.DurianGeneratedAttribute!;

			IEnumerable<CollidingMethod> collection = symbol.ContainingType.GetAllMembers(symbol.Name)
				.OfType<IMethodSymbol>()
				.Select(m => ((IMethodSymbol symbol, ITypeParameterSymbol[] typeParameters))(m, m.TypeParameters.ToArray()))
				.Where(m => m.typeParameters.Length >= numNonDefaultParam && m.typeParameters.Length < numTypeParameters)
				.Select(m => new CollidingMethod(m.symbol, m.symbol.Parameters.ToArray(), m.typeParameters))
				.Where(m => m.Parameters.Length == numParameters);

			if (symbol.IsOverride && symbol.OverriddenMethod is IMethodSymbol baseMethod)
			{
				string baseFullName = baseMethod.ToString();
				collection = collection.Where(m =>
				{
					AttributeData? data = GetAttribute(m.Symbol);

					if (data?.ConstructorArguments.FirstOrDefault().Value is string value)
					{
						return value != fullName && value != baseFullName;
					}

					return true;
				});
			}
			else
			{
				collection = collection.Where(m =>
				{
					AttributeData? data = GetAttribute(m.Symbol);

					if (data?.ConstructorArguments.FirstOrDefault().Value is string value)
					{
						return value != fullName;
					}

					return true;
				});
			}

			return collection.ToArray();

			AttributeData? GetAttribute(IMethodSymbol s)
			{
				AttributeData[] attributes = s.GetAttributes().ToArray();
				return Array.Find(attributes, attr => SymbolEqualityComparer.Default.Equals(attr.AttributeClass, generatedFrom));
			}
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
