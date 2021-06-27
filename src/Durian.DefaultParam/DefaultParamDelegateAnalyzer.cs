﻿// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;

#if !MAIN_PACKAGE

using Microsoft.CodeAnalysis.Diagnostics;

#endif

namespace Durian.Analysis.DefaultParam
{
	/// <summary>
	/// Analyzes delegates with type parameters marked by the <see cref="DefaultParamAttribute"/>.
	/// </summary>
#if !MAIN_PACKAGE

	[DiagnosticAnalyzer(LanguageNames.CSharp)]
#endif

#pragma warning disable RS1001 // Missing diagnostic analyzer attribute.
	public sealed partial class DefaultParamDelegateAnalyzer : DefaultParamAnalyzer
#pragma warning restore RS1001 // Missing diagnostic analyzer attribute.
	{
		/// <inheritdoc/>
		public override SymbolKind SupportedSymbolKind => SymbolKind.NamedType;

		/// <summary>
		/// Initializes a new instance of the <see cref="DefaultParamDelegateAnalyzer"/> class.
		/// </summary>
		public DefaultParamDelegateAnalyzer()
		{
		}

		/// <summary>
		/// Fully analyzes the specified <paramref name="symbol"/>.
		/// </summary>
		/// <param name="symbol"><see cref="INamedTypeSymbol"/> to analyze.</param>
		/// <param name="compilation">Current <see cref="DefaultParamCompilationData"/>.</param>
		/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
		/// <returns><see langword="true"/> if the <paramref name="symbol"/> is valid, otherwise <see langword="false"/>.</returns>
		public static bool Analyze(
			INamedTypeSymbol symbol,
			DefaultParamCompilationData compilation,
			CancellationToken cancellationToken = default
		)
		{
			TypeParameterContainer typeParameters = TypeParameterContainer.CreateFrom(symbol, compilation, cancellationToken);

			if (!typeParameters.HasDefaultParams)
			{
				return false;
			}

			if (AnalyzeAgainstProhibitedAttributes(symbol, compilation, out AttributeData[]? attributes) &&
				AnalyzeContainingTypes(symbol, compilation, out INamedTypeSymbol[]? containingTypes, cancellationToken) &&
				AnalyzeTypeParameters(symbol, in typeParameters))
			{
				string targetNamespace = GetTargetNamespace(symbol, compilation, attributes, containingTypes);

				return AnalyzeCollidingMembers(symbol, in typeParameters, compilation, targetNamespace, out _, attributes, containingTypes, cancellationToken);
			}

			return false;
		}

		/// <summary>
		/// Analyzes all the colliding members of the given <paramref name="symbol"/>.
		/// </summary>
		/// <param name="symbol"><see cref="INamedTypeSymbol"/> to analyze the colliding members of.</param>
		/// <param name="typeParameters"><see cref="TypeParameterContainer"/> containing type parameters of the <paramref name="symbol"/>.</param>
		/// <param name="compilation">Current <see cref="DefaultParamCompilationData"/>.</param>
		/// <param name="targetNamespace">Namespace where the generated members are located.</param>
		/// <param name="applyNew"><see langword="abstract"/><see cref="HashSet{T}"/> of indexes of type parameters with the <see cref="DefaultParamAttribute"/> applied for whom the <see langword="new"/> modifier should be applied. -or- <see langword="null"/> if the <paramref name="symbol"/> is not valid.</param>
		/// <param name="attributes">A collection of <see cref="AttributeData"/>a of the target <paramref name="symbol"/>.</param>
		/// <param name="containingTypes">An array of <see cref="INamedTypeSymbol"/>s of the <paramref name="symbol"/>'s containing types.</param>
		/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
		/// <returns><see langword="true"/> if there aren't any collisions with the <paramref name="symbol"/>, otherwise <see langword="false"/>.</returns>
		public static bool AnalyzeCollidingMembers(
			INamedTypeSymbol symbol,
			in TypeParameterContainer typeParameters,
			DefaultParamCompilationData compilation,
			string targetNamespace,
			out HashSet<int>? applyNew,
			IEnumerable<AttributeData>? attributes = null,
			INamedTypeSymbol[]? containingTypes = null,
			CancellationToken cancellationToken = default
		)
		{
			InitializeAttributes(ref attributes, symbol);
			InitializeContainingTypes(ref containingTypes, symbol);

			bool allowsNewModifier = AllowsNewModifier(symbol, compilation, attributes, containingTypes);

			return AnalyzeCollidingMembers(
				symbol,
				in typeParameters,
				compilation,
				targetNamespace,
				out applyNew,
				allowsNewModifier,
				cancellationToken
			);
		}

		/// <summary>
		/// Analyzes all the colliding members of the given <paramref name="symbol"/>.
		/// </summary>
		/// <param name="symbol"><see cref="INamedTypeSymbol"/> to analyze the colliding members of.</param>
		/// <param name="typeParameters"><see cref="TypeParameterContainer"/> containing type parameters of the <paramref name="symbol"/>.</param>
		/// <param name="compilation">Current <see cref="DefaultParamCompilationData"/>.</param>
		/// <param name="targetNamespace">Namespace where the generated members are located.</param>
		/// <param name="applyNew"><see langword="abstract"/><see cref="HashSet{T}"/> of indexes of type parameters with the <see cref="DefaultParamAttribute"/> applied for whom the <see langword="new"/> modifier should be applied. -or- <see langword="null"/> if the <paramref name="symbol"/> is not valid.</param>
		/// <param name="allowsNewModifier">Determines whether to allows applying the <see langword="new"/> modifier.</param>
		/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
		/// <returns><see langword="true"/> if there aren't any collisions with the <paramref name="symbol"/>, otherwise <see langword="false"/>.</returns>
		public static bool AnalyzeCollidingMembers(
			INamedTypeSymbol symbol,
			in TypeParameterContainer typeParameters,
			DefaultParamCompilationData compilation,
			string targetNamespace,
			out HashSet<int>? applyNew,
			bool allowsNewModifier,
			CancellationToken cancellationToken = default
		)
		{
			CollidingMember[] collidingMethods = GetPotentiallyCollidingMembers(
				symbol,
				compilation,
				targetNamespace,
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
		/// Returns a collection of all supported diagnostics of <see cref="DefaultParamDelegateAnalyzer"/>.
		/// </summary>
		public static IEnumerable<DiagnosticDescriptor> GetSupportedDiagnostics()
		{
			return GetBaseDiagnostics().Concat(GetAnalyzerSpecificDiagnosticsAsArray());
		}

		/// <inheritdoc/>
		public override void Analyze(
			IDiagnosticReceiver diagnosticReceiver,
			ISymbol symbol,
			DefaultParamCompilationData compilation,
			CancellationToken cancellationToken = default
		)
		{
			WithDiagnostics.Analyze(diagnosticReceiver, (INamedTypeSymbol)symbol, compilation, cancellationToken);
		}

		/// <inheritdoc/>
		protected override IEnumerable<DiagnosticDescriptor> GetAnalyzerSpecificDiagnostics()
		{
			return GetAnalyzerSpecificDiagnosticsAsArray();
		}

		/// <inheritdoc/>
		protected override bool ShouldAnalyze(ISymbol symbol, DefaultParamCompilationData compilation)
		{
			return symbol is INamedTypeSymbol t && t.TypeKind == TypeKind.Delegate;
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

		private static DiagnosticDescriptor[] GetAnalyzerSpecificDiagnosticsAsArray()
		{
			return new DiagnosticDescriptor[]
			{
				DefaultParamDiagnostics.DUR0129_TargetNamespaceAlreadyContainsMemberWithName
			};
		}
	}
}
