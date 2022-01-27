// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Durian.Analysis.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

#if !MAIN_PACKAGE

using Microsoft.CodeAnalysis.Diagnostics;

#endif

namespace Durian.Analysis.DefaultParam
{
	/// <summary>
	/// Analyzes methods with type parameters marked by the <c>Durian.DefaultParamAttribute</c>.
	/// </summary>
#pragma warning disable RS1001 // Missing diagnostic analyzer attribute.
#if !MAIN_PACKAGE
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
#endif
	public sealed partial class DefaultParamMethodAnalyzer : DefaultParamAnalyzer
#pragma warning restore RS1001 // Missing diagnostic analyzer attribute.
	{
		/// <inheritdoc/>
		public override SymbolKind SupportedSymbolKind => SymbolKind.Method;

		/// <summary>
		/// Initializes a new instance of the <see cref="DefaultParamMethodAnalyzer"/> class.
		/// </summary>
		public DefaultParamMethodAnalyzer()
		{
		}

		/// <summary>
		/// Fully analyzes the specified <paramref name="symbol"/>.
		/// </summary>
		/// <param name="symbol"><see cref="IMethodSymbol"/> to analyze.</param>
		/// <param name="compilation">Current <see cref="DefaultParamCompilationData"/>.</param>
		/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
		/// <returns><see langword="true"/> if the <paramref name="symbol"/> is valid, otherwise <see langword="false"/>.</returns>
		public static bool Analyze(
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

			return AnalyzeCore(symbol, compilation, ref typeParameters, cancellationToken);
		}

		/// <summary>
		/// Analyzes if the <paramref name="symbol"/> is of an invalid type.
		/// </summary>
		/// <param name="symbol"><see cref="IMethodSymbol"/> to analyze.</param>
		/// <returns><see langword="true"/> if the <paramref name="symbol"/> is valid, otherwise <see langword="false"/>.</returns>
		public static bool AnalyzeAgainstInvalidMethodType(IMethodSymbol symbol)
		{
			if (symbol.MethodKind is MethodKind.LocalFunction or MethodKind.AnonymousFunction or MethodKind.ExplicitInterfaceImplementation)
			{
				return false;
			}

			if (symbol.ContainingType is INamedTypeSymbol t && t.TypeKind == TypeKind.Interface)
			{
				return false;
			}

			return true;
		}

		/// <summary>
		/// Analyzes if the <paramref name="symbol"/> or either <see langword="partial"/> or <see langword="extern"/>.
		/// </summary>
		/// <param name="symbol"><see cref="IMethodSymbol"/> to analyze.</param>
		/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
		/// <returns><see langword="true"/> if the <paramref name="symbol"/> is valid (is not <see langword="partial"/> or <see langword="extern"/>), otherwise <see langword="false"/>.</returns>
		public static bool AnalyzeAgainstPartialOrExtern(IMethodSymbol symbol, CancellationToken cancellationToken = default)
		{
			if (symbol?.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax(cancellationToken) is not MethodDeclarationSyntax declaration)
			{
				return false;
			}

			return AnalyzeAgainstPartialOrExtern(symbol, declaration);
		}

		/// <summary>
		/// Analyzes if the <paramref name="symbol"/> or either <see langword="partial"/> or <see langword="extern"/>.
		/// </summary>
		/// <param name="symbol"><see cref="IMethodSymbol"/> to analyze.</param>
		/// <param name="declaration">Main <see cref="MethodDeclarationSyntax"/> of the <paramref name="symbol"/>.</param>
		/// <returns><see langword="true"/> if the <paramref name="symbol"/> is valid (is not <see langword="partial"/> or <see langword="extern"/>), otherwise <see langword="false"/>.</returns>
		public static bool AnalyzeAgainstPartialOrExtern(IMethodSymbol symbol, MethodDeclarationSyntax declaration)
		{
			return !symbol.IsExtern && !symbol.IsPartial(declaration);
		}

		/// <inheritdoc cref="WithDiagnostics.AnalyzeBaseMethodAndTypeParameters(IDiagnosticReceiver, IMethodSymbol, ref TypeParameterContainer, in TypeParameterContainer)"/>
		public static bool AnalyzeBaseMethodAndTypeParameters(
			IMethodSymbol symbol,
			ref TypeParameterContainer typeParameters,
			in TypeParameterContainer combinedTypeParameters
		)
		{
			if (!symbol.IsOverride)
			{
				return AnalyzeTypeParameters(symbol, in typeParameters);
			}

			if (HasAddedDefaultParamAttributes(in typeParameters, in combinedTypeParameters) ||
				!AnalyzeTypeParameters(symbol, in combinedTypeParameters) ||
				!AnalyzeBaseMethodParameters(in typeParameters, in combinedTypeParameters))

			{
				return false;
			}

			typeParameters = TypeParameterContainer.Combine(in typeParameters, in combinedTypeParameters);
			return true;
		}

		/// <summary>
		/// Analyzes if the signature of the <paramref name="symbol"/> is valid. If so, returns a <see cref="HashSet{T}"/> of indexes of type parameters with the <c>Durian.DefaultParamAttribute</c> applied for whom the <see langword="new"/> modifier should be applied.
		/// </summary>
		/// <param name="symbol"><see cref="IMethodSymbol"/> to analyze the signature of.</param>
		/// <param name="typeParameters"><see cref="TypeParameterContainer"/> containing type parameters of the <paramref name="symbol"/>.</param>
		/// <param name="compilation">Current <see cref="DefaultParamCompilationData"/>.</param>
		/// <param name="applyNew"><see langword="abstract"/><see cref="HashSet{T}"/> of indexes of type parameters with the <c>Durian.DefaultParamAttribute</c> applied for whom the <see langword="new"/> modifier should be applied. -or- <see langword="null"/> if the <paramref name="symbol"/> is not valid.</param>
		/// <param name="attributes">A collection of <see cref="AttributeData"/>a of the target <paramref name="symbol"/>.</param>
		/// <param name="containingTypes">An array of <see cref="INamedTypeSymbol"/>s of the <paramref name="symbol"/>'s containing types.</param>
		/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
		/// <returns><see langword="true"/> if the signature of <paramref name="symbol"/> is valid, otherwise <see langword="false"/>.</returns>
		public static bool AnalyzeMethodSignature(
			IMethodSymbol symbol,
			in TypeParameterContainer typeParameters,
			DefaultParamCompilationData compilation,
			out HashSet<int>? applyNew,
			IEnumerable<AttributeData>? attributes = null,
			INamedTypeSymbol[]? containingTypes = null,
			CancellationToken cancellationToken = default
		)
		{
			InitializeAttributes(ref attributes, symbol);
			InitializeContainingTypes(ref containingTypes, symbol);

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

			return AnalyzeCollidingMembers(
				symbol,
				in typeParameters,
				collidingMethods,
				symbolParameters,
				AllowsNewModifier(symbol, compilation, attributes, containingTypes),
				cancellationToken,
				out applyNew
			);
		}

		/// <summary>
		/// Returns a collection of all supported diagnostics of <see cref="DefaultParamMethodAnalyzer"/>.
		/// </summary>
		public static IEnumerable<DiagnosticDescriptor> GetSupportedDiagnostics()
		{
			return GetBaseDiagnostics().Concat(GetAnalyzerSpecificDiagnosticsAsArray());
		}

		/// <inheritdoc cref="WithDiagnostics.ShouldBeAnalyzed(IDiagnosticReceiver, IMethodSymbol, DefaultParamCompilationData, in TypeParameterContainer, out TypeParameterContainer, CancellationToken)"/>
		public static bool ShouldBeAnalyzed(
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

		/// <summary>
		/// Determines, whether the <see cref="DefaultParamGenerator"/> should call a <see cref="IMethodSymbol"/> instead of copying its contents.
		/// </summary>
		/// <param name="symbol"><see cref="IMethodSymbol"/> to check.</param>
		/// <param name="compilation">Current <see cref="DefaultParamCompilationData"/>.</param>
		/// <param name="attributes">A collection of <see cref="IMethodSymbol"/>' attributes.</param>
		/// <param name="containingTypes">An array of <see cref="INamedTypeSymbol"/>s of the target <see cref="IMethodSymbol"/>.</param>
		public static bool ShouldCallInsteadOfCopying(
			IMethodSymbol symbol,
			DefaultParamCompilationData compilation,
			IEnumerable<AttributeData>? attributes = null,
			INamedTypeSymbol[]? containingTypes = null
		)
		{
			InitializeAttributes(ref attributes, symbol);
			InitializeContainingTypes(ref containingTypes, symbol);

			if (symbol.IsAbstract)
			{
				return false;
			}

			return DefaultParamUtilities.GetConfigurationEnumValue(MemberNames.Config_MethodConvention, attributes, containingTypes, compilation, (int)compilation.GlobalConfiguration.MethodConvention) == (int)MethodConvention.Call;
		}

		/// <inheritdoc/>
		public override void Analyze(IDiagnosticReceiver diagnosticReceiver, ISymbol symbol, DefaultParamCompilationData compilation, CancellationToken cancellationToken = default)
		{
			WithDiagnostics.Analyze(diagnosticReceiver, (IMethodSymbol)symbol, compilation, cancellationToken);
		}

		/// <inheritdoc/>
		protected override IEnumerable<DiagnosticDescriptor> GetAnalyzerSpecificDiagnostics()
		{
			return GetAnalyzerSpecificDiagnosticsAsArray();
		}

		/// <inheritdoc/>
		protected override bool ShouldAnalyze(ISymbol symbol, DefaultParamCompilationData compilation)
		{
			return symbol is IMethodSymbol;
		}

		private static bool AnalyzeBaseMethodParameters(in TypeParameterContainer typeParameters, in TypeParameterContainer baseTypeParameters)
		{
			int length = baseTypeParameters.Length;
			int firstIndex = GetFirstDefaultParamIndex(in typeParameters, in baseTypeParameters);

			for (int i = firstIndex; i < length; i++)
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

		private static bool AnalyzeCollidingMembers(
			IMethodSymbol symbol,
			in TypeParameterContainer typeParameters,
			CollidingMember[] collidingMethods,
			IParameterSymbol[] symbolParameters,
			bool applyNewModifierIfPossible,
			CancellationToken cancellationToken,
			out HashSet<int>? applyNew
		)
		{
			ParameterGeneration[][] generations = DefaultParamUtilities.GetParameterGenerations(in typeParameters, symbolParameters);

			HashSet<int> applyNewLocal = new();
			int numMethods = collidingMethods.Length;
			bool allowsNewModifier = applyNewModifierIfPossible || HasNewModifier(symbol, cancellationToken);

			for (int i = 0; i < numMethods; i++)
			{
				ref readonly CollidingMember currentMember = ref collidingMethods[i];

				if (currentMember.TypeParameters is null)
				{
					if (allowsNewModifier && !SymbolEqualityComparer.Default.Equals(currentMember.Symbol.ContainingType, symbol.ContainingType))
					{
						applyNewLocal.Add(typeParameters.Length - 1);
						continue;
					}
					else
					{
						applyNew = null;
						return false;
					}
				}

				int targetIndex = currentMember.TypeParameters.Length - typeParameters.NumNonDefaultParam;

				if (currentMember.Parameters is null)
				{
					if (allowsNewModifier && !SymbolEqualityComparer.Default.Equals(currentMember.Symbol.ContainingType, symbol.ContainingType))
					{
						applyNewLocal.Add(targetIndex);
						continue;
					}
					else
					{
						applyNew = null;
						return false;
					}
				}

				ParameterGeneration[] targetParameters = generations[targetIndex];

				if (HasCollidingParameters(targetParameters, in currentMember))
				{
					if (!allowsNewModifier || SymbolEqualityComparer.Default.Equals(currentMember.Symbol.ContainingType, symbol))
					{
						applyNew = null;
						return false;
					}

					applyNewLocal.Add(targetIndex);
				}
			}

			applyNew = GetApplyNewOrNull(applyNewLocal);
			return true;
		}

		private static bool AnalyzeCore(
			IMethodSymbol symbol,
			DefaultParamCompilationData compilation,
			ref TypeParameterContainer typeParameters,
			CancellationToken cancellationToken
		)
		{
			return
				ShouldBeAnalyzed(symbol, compilation, in typeParameters, out TypeParameterContainer combinedTypeParameters, cancellationToken) &&
				AnalyzeAgainstInvalidMethodType(symbol) &&
				AnalyzeAgainstPartialOrExtern(symbol, cancellationToken) &&
				AnalyzeAgainstProhibitedAttributes(symbol, compilation, out AttributeData[]? attributes) &&
				AnalyzeContainingTypes(symbol, compilation, out INamedTypeSymbol[]? containingTypes, cancellationToken) &&
				AnalyzeBaseMethodAndTypeParameters(symbol, ref typeParameters, combinedTypeParameters) &&
				AnalyzeMethodSignature(symbol, in typeParameters, compilation, out _, attributes, containingTypes, cancellationToken);
		}

		private static bool AnalyzeParameterInBaseMethod(in TypeParameterData thisData, in TypeParameterData baseData)
		{
			if (baseData.IsValidDefaultParam)
			{
				if (thisData.IsValidDefaultParam && !SymbolEqualityComparer.Default.Equals(thisData.TargetType, baseData.TargetType))
				{
					return false;
				}
			}
			else if (thisData.IsValidDefaultParam)
			{
				return false;
			}

			return true;
		}

		private static DiagnosticDescriptor[] GetAnalyzerSpecificDiagnosticsAsArray()
		{
			return new[]
			{
				DefaultParamDiagnostics.DUR0102_MethodCannotBePartialOrExtern,
				DefaultParamDiagnostics.DUR0103_DefaultParamIsNotValidOnThisTypeOfMethod,
				DefaultParamDiagnostics.DUR0107_DoNotOverrideGeneratedMethods,
				DefaultParamDiagnostics.DUR0108_ValueOfOverriddenMethodMustBeTheSameAsBase,
				DefaultParamDiagnostics.DUR0109_DoNotAddDefaultParamAttributeOnOverridenParameters,
				DefaultParamDiagnostics.DUR0110_OverriddenDefaultParamAttribuetShouldBeAddedForClarity,
				DefaultParamDiagnostics.DUR0114_MethodWithSignatureAlreadyExists,
			};
		}

		private static TypeParameterContainer GetBaseMethodTypeParameters(
			IMethodSymbol baseMethod,
			DefaultParamCompilationData compilation,
			CancellationToken cancellationToken
		)
		{
			TypeParameterContainer parameters = TypeParameterContainer.CreateFrom(baseMethod, compilation, cancellationToken);

			foreach (IMethodSymbol m in baseMethod.GetBaseMethods())
			{
				TypeParameterContainer t = TypeParameterContainer.CreateFrom(m, compilation, cancellationToken);

				parameters = TypeParameterContainer.Combine(parameters, t);
			}

			return parameters;
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

		private static string GetMethodSignatureString(
			string name,
			in TypeParameterContainer typeParameters,
			int numTypeParameters,
			ParameterGeneration[] parameters
		)
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

		private static bool HasAddedDefaultParamAttributes(in TypeParameterContainer typeParameters, in TypeParameterContainer baseTypeParameters)
		{
			if (typeParameters.FirstDefaultParamIndex == -1)
			{
				return false;
			}

			return typeParameters.FirstDefaultParamIndex < baseTypeParameters.FirstDefaultParamIndex;
		}
	}
}