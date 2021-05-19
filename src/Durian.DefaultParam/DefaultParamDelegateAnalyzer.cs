using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Durian.Configuration;
using Durian.Generator.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Durian.Generator.DefaultParam
{
	/// <summary>
	/// Analyzes delegates with type parameters marked by the <see cref="DefaultParamAttribute"/>.
	/// </summary>
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public partial class DefaultParamDelegateAnalyzer : DefaultParamAnalyzer
	{
		/// <inheritdoc/>
		public override SymbolKind SupportedSymbolKind => SymbolKind.NamedType;

		/// <summary>
		/// Initializes a new instance of the <see cref="DefaultParamDelegateAnalyzer"/> class.
		/// </summary>
		public DefaultParamDelegateAnalyzer()
		{
		}

		/// <inheritdoc/>
		protected override IEnumerable<DiagnosticDescriptor> GetAnalyzerSpecificDiagnostics()
		{
			return Array.Empty<DiagnosticDescriptor>();
		}

		/// <inheritdoc/>
		public override void Analyze(IDiagnosticReceiver diagnosticReceiver, ISymbol symbol, DefaultParamCompilationData compilation, CancellationToken cancellationToken = default)
		{
			if (symbol is not INamedTypeSymbol t || t.TypeKind != TypeKind.Delegate)
			{
				return;
			}

			WithDiagnostics.Analyze(diagnosticReceiver, t, compilation, cancellationToken);
		}

		/// <summary>
		/// Fully analyzes the specified <paramref name="symbol"/>.
		/// </summary>
		/// <param name="symbol"><see cref="INamedTypeSymbol"/> to analyze.</param>
		/// <param name="compilation">Current <see cref="DefaultParamCompilationData"/>.</param>
		/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
		/// <returns><see langword="true"/> if the <paramref name="symbol"/> is valid, otherwise <see langword="false"/>.</returns>
		public static bool Analyze(INamedTypeSymbol symbol, DefaultParamCompilationData compilation, CancellationToken cancellationToken = default)
		{
			TypeParameterContainer typeParameters = TypeParameterContainer.CreateFrom(symbol, compilation, cancellationToken);

			if (!typeParameters.HasDefaultParams)
			{
				return false;
			}

			return
				AnalyzeAgaintsProhibitedAttributes(symbol, compilation) &&
				AnalyzeContainingTypes(symbol, cancellationToken) &&
				AnalyzeTypeParameters(in typeParameters) &&
				AnalyzeCollidingMembers(symbol, in typeParameters, compilation, out _, cancellationToken);
		}

		/// <inheritdoc cref="AnalyzeCollidingMembers(INamedTypeSymbol, in TypeParameterContainer, DefaultParamCompilationData, bool, out HashSet{int}?, CancellationToken)"/>
		public static bool AnalyzeCollidingMembers(
			INamedTypeSymbol symbol,
			in TypeParameterContainer typeParameters,
			DefaultParamCompilationData compilation,
			out HashSet<int>? applyNew,
			CancellationToken cancellationToken = default
		)
		{
			return AnalyzeCollidingMembers(symbol, in typeParameters, compilation, symbol.GetAttributes(), symbol.GetContainingTypeSymbols().ToArray(), out applyNew, cancellationToken);
		}

		/// <summary>
		/// Analyzes all the colliding members of the given <paramref name="symbol"/>.
		/// </summary>
		/// <param name="symbol"><see cref="INamedTypeSymbol"/> to analyze the colliding members of.</param>
		/// <param name="typeParameters"><see cref="TypeParameterContainer"/> containing type parameters of the <paramref name="symbol"/>.</param>
		/// <param name="compilation">Current <see cref="DefaultParamCompilationData"/>.</param>
		/// <param name="attributes">A collection of <see cref="AttributeData"/>a of the target <paramref name="symbol"/>.</param>
		/// <param name="containingTypes">An array of <see cref="INamedTypeSymbol"/>s of the <paramref name="symbol"/>'s containing types.</param>
		/// <param name="applyNew"><see langword="abstract"/><see cref="HashSet{T}"/> of indexes of type parameters with the <see cref="DefaultParamAttribute"/> applied for whom the <see langword="new"/> modifier should be applied. -or- <see langword="null"/> if the <paramref name="symbol"/> is not valid.</param>
		/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
		/// <returns><see langword="true"/> if there aren't any collisions with the <paramref name="symbol"/>, otherwise <see langword="false"/>.</returns>
		public static bool AnalyzeCollidingMembers(
			INamedTypeSymbol symbol,
			in TypeParameterContainer typeParameters,
			DefaultParamCompilationData compilation,
			IEnumerable<AttributeData> attributes,
			INamedTypeSymbol[] containingTypes,
			out HashSet<int>? applyNew,
			CancellationToken cancellationToken = default
		)
		{
			bool allowsNewModifier = AllowsNewModifier(attributes, containingTypes, compilation);

			return AnalyzeCollidingMembers(
				symbol,
				in typeParameters,
				compilation,
				allowsNewModifier,
				out applyNew,
				cancellationToken
			);
		}

		/// <summary>
		/// Analyzes all the colliding members of the given <paramref name="symbol"/>.
		/// </summary>
		/// <param name="symbol"><see cref="INamedTypeSymbol"/> to analyze the colliding members of.</param>
		/// <param name="typeParameters"><see cref="TypeParameterContainer"/> containing type parameters of the <paramref name="symbol"/>.</param>
		/// <param name="compilation">Current <see cref="DefaultParamCompilationData"/>.</param>
		/// <param name="allowsNewModifier">Determines whether to allows applying the <see langword="new"/> modifier.</param>
		/// <param name="applyNew"><see langword="abstract"/><see cref="HashSet{T}"/> of indexes of type parameters with the <see cref="DefaultParamAttribute"/> applied for whom the <see langword="new"/> modifier should be applied. -or- <see langword="null"/> if the <paramref name="symbol"/> is not valid.</param>
		/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
		/// <returns><see langword="true"/> if there aren't any collisions with the <paramref name="symbol"/>, otherwise <see langword="false"/>.</returns>
		public static bool AnalyzeCollidingMembers(
			INamedTypeSymbol symbol,
			in TypeParameterContainer typeParameters,
			DefaultParamCompilationData compilation,
			bool allowsNewModifier,
			out HashSet<int>? applyNew,
			CancellationToken cancellationToken = default
		)
		{
			CollidingMember[] collidingMethods = GetPotentiallyCollidingMembers(
				symbol,
				compilation,
				typeParameters.Length,
				typeParameters.NumNonDefaultParam
			);

			if (collidingMethods.Length == 0)
			{
				applyNew = null;
				return true;
			}

			return AnalyzeCollidingMembers_Internal(
				symbol,
				in typeParameters,
				collidingMethods,
				allowsNewModifier,
				cancellationToken,
				out applyNew
			);
		}

		/// <summary>
		/// Determines whether the 'new' modifier is allowed to be applied to the target <see cref="INamedTypeSymbol"/> according to the most specific <see cref="DefaultParamConfigurationAttribute"/> or <see cref="DefaultParamScopedConfigurationAttribute"/>.
		/// </summary>
		/// <param name="symbol"><see cref="INamedTypeSymbol"/> to check.</param>
		/// <param name="compilation">Current <see cref="DefaultParamCompilationData"/>.</param>
		public static bool AllowsNewModifier(INamedTypeSymbol symbol, DefaultParamCompilationData compilation)
		{
			return AllowsNewModifier(symbol.GetAttributes(), symbol.GetContainingTypeSymbols().ToArray(), compilation);
		}

		/// <summary>
		/// Determines whether the 'new' modifier is allowed to the target <see cref="INamedTypeSymbol"/> according to the most specific <see cref="DefaultParamConfigurationAttribute"/> or <see cref="DefaultParamScopedConfigurationAttribute"/>.
		/// </summary>
		/// <param name="attributes">A collection of the target <see cref="ISymbol"/>'s attributes.</param>
		/// <param name="containingTypes"><see cref="INamedTypeSymbol"/>s that contain this <see cref="IMethodSymbol"/>.</param>
		/// <param name="compilation">Current <see cref="DefaultParamCompilationData"/>.</param>
		public static bool AllowsNewModifier(IEnumerable<AttributeData> attributes, INamedTypeSymbol[] containingTypes, DefaultParamCompilationData compilation)
		{
			return DefaultParamUtilities.AllowsNewModifier(attributes, containingTypes, compilation);
		}

		private static bool AnalyzeCollidingMembers_Internal(
			INamedTypeSymbol symbol,
			in TypeParameterContainer typeParameters,
			CollidingMember[] collidingMembers,
			bool applyNewModifierIsPossible,
			CancellationToken cancellationToken,
			out HashSet<int>? applyNew
		)
		{
			HashSet<int> applyNewLocal = new();
			int numCollisions = collidingMembers.Length;
			bool allowsNewModifier = applyNewModifierIsPossible || HasNewModifier(symbol, cancellationToken);

			for (int i = 0; i < numCollisions; i++)
			{
				ref readonly CollidingMember member = ref collidingMembers[i];

				if (member.TypeParameters is null)
				{
					if (allowsNewModifier && member.IsChild && !SymbolEqualityComparer.Default.Equals(member.Symbol.ContainingType, symbol.ContainingType))
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

				int targetIndex = member.TypeParameters.Length - typeParameters.NumNonDefaultParam;

				if (member.Parameters is null && allowsNewModifier && member.IsChild && !SymbolEqualityComparer.Default.Equals(member.Symbol.ContainingType, symbol.ContainingType))
				{
					applyNewLocal.Add(targetIndex);
					continue;
				}

				applyNew = null;
				return false;
			}

			applyNew = GetApplyNewOrNull(applyNewLocal);
			return true;
		}
	}
}
