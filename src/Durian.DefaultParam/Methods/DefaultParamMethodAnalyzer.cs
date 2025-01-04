using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;
using Durian.Analysis.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Durian.Analysis.DefaultParam.Methods
{
	/// <summary>
	/// Analyzes methods with type parameters marked by the <c>Durian.DefaultParamAttribute</c>.
	/// </summary>
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public sealed class DefaultParamMethodAnalyzer : DefaultParamAnalyzer
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
		/// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> that is used to report <see cref="Diagnostic"/>s.</param>
		/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
		/// <returns><see langword="true"/> if the <paramref name="symbol"/> is valid, otherwise <see langword="false"/>.</returns>
		public static bool Analyze(
			IMethodSymbol symbol,
			DefaultParamCompilationData compilation,
			IDiagnosticReceiver diagnosticReceiver,
			CancellationToken cancellationToken = default
		)
		{
			TypeParameterContainer typeParameters = TypeParameterContainer.CreateFrom(symbol, compilation, cancellationToken);

			if (!typeParameters.HasDefaultParams && !symbol.IsOverride)
			{
				return false;
			}

			return AnalyzeCore(symbol, compilation, ref typeParameters, diagnosticReceiver, cancellationToken);
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
		/// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> that is used to report <see cref="Diagnostic"/>s.</param>
		/// <returns><see langword="true"/> if the <paramref name="symbol"/> is valid, otherwise <see langword="false"/>.</returns>
		public static bool AnalyzeAgainstInvalidMethodType(IMethodSymbol symbol, IDiagnosticReceiver diagnosticReceiver)
		{
			if (symbol.MethodKind != MethodKind.Ordinary || (symbol.ContainingType is INamedTypeSymbol t && t.TypeKind == TypeKind.Interface))
			{
				ReportDiagnosticForInvalidMethodType(symbol, diagnosticReceiver);
				return false;
			}

			return true;
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
		/// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> that is used to report <see cref="Diagnostic"/>s.</param>
		/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
		/// <returns><see langword="true"/> if the <paramref name="symbol"/> is valid (is not <see langword="partial"/> or <see langword="extern"/>), otherwise <see langword="false"/>.</returns>
		public static bool AnalyzeAgainstPartialOrExtern(
			IMethodSymbol symbol,
			IDiagnosticReceiver diagnosticReceiver,
			CancellationToken cancellationToken = default
		)
		{
			if (symbol?.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax(cancellationToken) is not MethodDeclarationSyntax declaration)
			{
				return false;
			}

			return AnalyzeAgainstPartialOrExtern(symbol, declaration, diagnosticReceiver);
		}

		/// <summary>
		/// Analyzes if the <paramref name="symbol"/> or either <see langword="partial"/> or <see langword="extern"/>.
		/// </summary>
		/// <param name="symbol"><see cref="IMethodSymbol"/> to analyze.</param>
		/// <param name="declaration">Main <see cref="MethodDeclarationSyntax"/> of the <paramref name="symbol"/>.</param>
		/// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> that is used to report <see cref="Diagnostic"/>s.</param>
		/// <returns><see langword="true"/> if the <paramref name="symbol"/> is valid (is not <see langword="partial"/> or <see langword="extern"/>), otherwise <see langword="false"/>.</returns>
		public static bool AnalyzeAgainstPartialOrExtern(
			IMethodSymbol symbol,
			MethodDeclarationSyntax declaration,
			IDiagnosticReceiver diagnosticReceiver
		)
		{
			if (symbol.IsExtern || symbol.IsPartial(declaration))
			{
				diagnosticReceiver.ReportDiagnostic(DefaultParamDiagnostics.DUR0102_MethodCannotBePartialOrExtern, symbol);
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

		/// <summary>
		/// Analyzes if the <paramref name="symbol"/> has valid <paramref name="typeParameters"/> when compared to the <paramref name="symbol"/>'s base method.
		/// </summary>
		/// <param name="symbol"><see cref="IMethodSymbol"/> to analyze.</param>
		/// <param name="typeParameters"><see cref="TypeParameterContainer"/> that contains type parameters to be analyzed.</param>
		/// <param name="combinedTypeParameters">Combined <see cref="TypeParameterContainer"/>s of the <paramref name="symbol"/>'s base methods.</param>
		/// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> that is used to report <see cref="Diagnostic"/>s.</param>
		/// <returns><see langword="true"/> if the <paramref name="symbol"/> is valid, otherwise <see langword="false"/>.</returns>
		public static bool AnalyzeBaseMethodAndTypeParameters(
			IMethodSymbol symbol,
			ref TypeParameterContainer typeParameters,
			in TypeParameterContainer combinedTypeParameters,
			IDiagnosticReceiver diagnosticReceiver
		)
		{
			if (!symbol.IsOverride)
			{
				return AnalyzeTypeParameters(symbol, in typeParameters, diagnosticReceiver);
			}

			if (AnalyzeTypeParameters(symbol, in combinedTypeParameters) && AnalyzeBaseMethodParameters(in typeParameters, in combinedTypeParameters, diagnosticReceiver))
			{
				typeParameters = TypeParameterContainer.Combine(in typeParameters, in combinedTypeParameters);
				return true;
			}

			return false;
		}

		/// <inheritdoc cref="AnalyzeBaseMethodAndTypeParameters(IMethodSymbol, ref TypeParameterContainer, in TypeParameterContainer, IDiagnosticReceiver)"/>
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
		/// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> that is used to report <see cref="Diagnostic"/>s.</param>
		/// <param name="attributes">A collection of <see cref="AttributeData"/>a of the target <paramref name="symbol"/>.</param>
		/// <param name="containingTypes">An array of <see cref="INamedTypeSymbol"/>s of the <paramref name="symbol"/>'s containing types.</param>
		/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
		/// <returns><see langword="true"/> if the signature of <paramref name="symbol"/> is valid, otherwise <see langword="false"/>.</returns>
		public static bool AnalyzeMethodSignature(
			IMethodSymbol symbol,
			in TypeParameterContainer typeParameters,
			DefaultParamCompilationData compilation,
			out HashSet<int>? applyNew,
			IDiagnosticReceiver diagnosticReceiver,
			IEnumerable<AttributeData>? attributes = null,
			ImmutableArray<INamedTypeSymbol> containingTypes = default,
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
				symbol,
				in typeParameters,
				collidingMethods,
				symbolParameters,
				allowsNewModifier,
				out applyNew,
				diagnosticReceiver,
				cancellationToken
			);
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
			ImmutableArray<INamedTypeSymbol> containingTypes = default,
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

		/// <summary>
		/// Reports <see cref="Diagnostic"/>s for the specified <paramref name="symbol"/>.
		/// </summary>
		/// <param name="symbol"><see cref="IMethodSymbol"/> of a method to report the <see cref="Diagnostic"/>s for.</param>
		/// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> that is used to report <see cref="Diagnostic"/>s.</param>
		public static void ReportDiagnosticForInvalidMethodType(ISymbol symbol, IDiagnosticReceiver diagnosticReceiver)
		{
			diagnosticReceiver.ReportDiagnostic(DefaultParamDiagnostics.DUR0103_DefaultParamIsNotValidOnThisTypeOfMethod, symbol);
		}

		/// <summary>
		/// Determines, whether the specified <paramref name="symbol"/> should be analyzed.
		/// </summary>
		/// <param name="symbol"><see cref="IMethodSymbol"/> to check if should be analyzed.</param>
		/// <param name="compilation">Current <see cref="DefaultParamCompilationData"/>.</param>
		/// <param name="typeParameters"><see cref="TypeParameterContainer"/> that contains type parameters to be analyzed.</param>
		/// <param name="combinedTypeParameters">Combined <see cref="TypeParameterContainer"/>s of the <paramref name="symbol"/>'s base methods.</param>
		/// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> that is used to report <see cref="Diagnostic"/>s.</param>
		/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
		/// <returns><see langword="true"/> if the <paramref name="symbol"/> should be analyzer, <see langword="false"/> otherwise.</returns>
		public static bool ShouldBeAnalyzed(
			IMethodSymbol symbol,
			DefaultParamCompilationData compilation,
			in TypeParameterContainer typeParameters,
			out TypeParameterContainer combinedTypeParameters,
			IDiagnosticReceiver diagnosticReceiver,
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

		/// <inheritdoc cref="ShouldBeAnalyzed(IMethodSymbol, DefaultParamCompilationData, in TypeParameterContainer, out TypeParameterContainer, IDiagnosticReceiver, CancellationToken)"/>
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
			ImmutableArray<INamedTypeSymbol> containingTypes = default
		)
		{
			InitializeAttributes(ref attributes, symbol);
			InitializeContainingTypes(ref containingTypes, symbol);

			if (symbol.IsAbstract)
			{
				return false;
			}

			int value = DefaultParamUtilities.GetConfigurationEnumValue(
				DefaultParamConfigurationAttributeProvider.MethodConvention,
				attributes,
				containingTypes,
				compilation,
				(int)compilation.GlobalConfiguration.MethodConvention
			);

			return value == (int)MethodConvention.Call;
		}

		/// <inheritdoc/>
		protected override void Analyze(
			ISymbol symbol,
			DefaultParamCompilationData compilation,
			IDiagnosticReceiver diagnosticReceiver,
			CancellationToken cancellationToken = default
		)
		{
			Analyze((IMethodSymbol)symbol, compilation, diagnosticReceiver, cancellationToken);
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

		private static bool AnalyzeBaseMethodParameters(
			in TypeParameterContainer typeParameters,
			in TypeParameterContainer baseTypeParameters,
			IDiagnosticReceiver diagnosticReceiver
		)
		{
			int length = baseTypeParameters.Length;
			bool isValid = true;

			int firstIndex = GetFirstDefaultParamIndex(in typeParameters, in baseTypeParameters);

			for (int i = firstIndex; i < length; i++)
			{
				ref readonly TypeParameterData baseData = ref baseTypeParameters[i];
				ref readonly TypeParameterData thisData = ref typeParameters[i];

				if (!AnalyzeParameterInBaseMethod(in thisData, in baseData, diagnosticReceiver))
				{
					isValid = false;
				}
			}

			return isValid;
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
			CollidingMember[] collidingMembers,
			IParameterSymbol[] symbolParameters,
			bool applyNewModifierIfPossible,
			out HashSet<int>? applyNew,
			IDiagnosticReceiver diagnosticReceiver,
			CancellationToken cancellationToken
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
					symbol,
					in member,
					in typeParameters,
					targetParameters,
					targetIndex,
					diagnosed,
					applyNewLocal,
					allowsNewModifier,
					ref isValid,
					diagnosticReceiver
				))
				{
					break;
				}
			}

			applyNew = GetApplyNewOrNull(applyNewLocal);
			return isValid;
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

		private static bool AnalyzeCollidingMethodParameters(
			IMethodSymbol symbol,
			in CollidingMember collidingMember,
			in TypeParameterContainer typeParameters,
			ParameterGeneration[] targetGeneration,
			int targetIndex,
			HashSet<int> diagnosed,
			HashSet<int> applyNew,
			bool allowsNewModifier,
			ref bool isValid,
			IDiagnosticReceiver diagnosticReceiver
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
			IMethodSymbol symbol,
			DefaultParamCompilationData compilation,
			ref TypeParameterContainer typeParameters,
			IDiagnosticReceiver diagnosticReceiver,
			CancellationToken cancellationToken
		)
		{
			if (!ShouldBeAnalyzed(
				symbol,
				compilation,
				in typeParameters,
				out TypeParameterContainer combinedTypeParameters,
				diagnosticReceiver,
				cancellationToken)
			)
			{
				return false;
			}

			bool isValid = AnalyzeAgainstInvalidMethodType(symbol, diagnosticReceiver);
			isValid &= AnalyzeAgainstPartialOrExtern(symbol, diagnosticReceiver, cancellationToken);
			isValid &= AnalyzeAgainstProhibitedAttributes(symbol, compilation, out AttributeData[]? attributes, diagnosticReceiver);
			isValid &= AnalyzeContainingTypes(symbol, compilation, out ImmutableArray<INamedTypeSymbol> containingTypes, diagnosticReceiver);
			isValid &= AnalyzeBaseMethodAndTypeParameters(symbol, ref typeParameters, in combinedTypeParameters, diagnosticReceiver);

			if (!isValid)
			{
				return false;
			}

			return AnalyzeMethodSignature(symbol, in typeParameters, compilation, out _, diagnosticReceiver, attributes, containingTypes, cancellationToken);
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
				AnalyzeContainingTypes(symbol, compilation, out ImmutableArray<INamedTypeSymbol> containingTypes) &&
				AnalyzeBaseMethodAndTypeParameters(symbol, ref typeParameters, combinedTypeParameters) &&
				AnalyzeMethodSignature(symbol, in typeParameters, compilation, out _, attributes, containingTypes, cancellationToken);
		}

		private static bool AnalyzeParameterInBaseMethod(
			in TypeParameterData thisData,
			in TypeParameterData baseData,
			IDiagnosticReceiver diagnosticReceiver
		)
		{
			if (baseData.IsValidDefaultParam)
			{
				if (!thisData.IsDefaultParam)
				{
					diagnosticReceiver.ReportDiagnostic(DefaultParamDiagnostics.DUR0110_OverriddenDefaultParamAttributeShouldBeAddedForClarity, thisData.Symbol);

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
				diagnosticReceiver.ReportDiagnostic(DefaultParamDiagnostics.DUR0109_DoNotAddDefaultParamAttributeOnOverriddenParameters, thisData.Location, thisData.Symbol);
				return false;
			}

			return true;
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
				DefaultParamDiagnostics.DUR0109_DoNotAddDefaultParamAttributeOnOverriddenParameters,
				DefaultParamDiagnostics.DUR0110_OverriddenDefaultParamAttributeShouldBeAddedForClarity,
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

			foreach (IMethodSymbol m in baseMethod.GetOverriddenSymbols())
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
