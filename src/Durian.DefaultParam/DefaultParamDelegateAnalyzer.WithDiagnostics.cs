// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using Durian.Analysis.Data;
using Durian.Analysis.Extensions;
using Microsoft.CodeAnalysis;

namespace Durian.Analysis.DefaultParam
{
#pragma warning disable RS1001 // Missing diagnostic analyzer attribute.

	public partial class DefaultParamDelegateAnalyzer
#pragma warning restore RS1001 // Missing diagnostic analyzer attribute.
	{
		/// <summary>
		/// Contains static methods that analyze delegate with type parameters marked using the <see
		/// cref="DefaultParamAttribute"/> and report <see cref="Diagnostic"/> s for the invalid ones.
		/// </summary>
		public static new class WithDiagnostics
		{
			/// <summary>
			/// Fully analyzes the specified <paramref name="symbol"/>.
			/// </summary>
			/// <param name="diagnosticReceiver">
			/// <see cref="IDiagnosticReceiver"/> that is used to report <see cref="Diagnostic"/> s.
			/// </param>
			/// <param name="symbol"><see cref="INamedTypeSymbol"/> to analyze.</param>
			/// <param name="compilation">Current <see cref="DefaultParamCompilationData"/>.</param>
			/// <param name="cancellationToken">
			/// <see cref="CancellationToken"/> that specifies if the operation should be canceled.
			/// </param>
			/// <returns>
			/// <see langword="true"/> if the <paramref name="symbol"/> is valid, otherwise <see langword="false"/>.
			/// </returns>
			public static bool Analyze(IDiagnosticReceiver diagnosticReceiver, INamedTypeSymbol symbol, DefaultParamCompilationData compilation, CancellationToken cancellationToken = default)
			{
				TypeParameterContainer typeParameters = TypeParameterContainer.CreateFrom(symbol, compilation, cancellationToken);

				if (!typeParameters.HasDefaultParams)
				{
					return false;
				}

				bool isValid = AnalyzeAgainstProhibitedAttributes(diagnosticReceiver, symbol, compilation);
				isValid &= AnalyzeContainingTypes(diagnosticReceiver, symbol, compilation, cancellationToken);
				isValid &= AnalyzeTypeParameters(diagnosticReceiver, symbol, in typeParameters);

				if (isValid)
				{
					IEnumerable<AttributeData> attributes = symbol.GetAttributes();
					INamedTypeSymbol[] containingTypes = symbol.GetContainingTypeSymbols().ToArray();

					string targetNamespace = GetTargetNamespace(symbol, attributes, containingTypes, compilation);

					return AnalyzeCollidingMembers(diagnosticReceiver, symbol, in typeParameters, compilation, targetNamespace, attributes, containingTypes, out _, cancellationToken);
				}

				return false;
			}

			/// <inheritdoc cref="DefaultParamAnalyzer.WithDiagnostics.AnalyzeAgainstProhibitedAttributes(IDiagnosticReceiver, ISymbol, DefaultParamCompilationData)"/>
			public static bool AnalyzeAgainstProhibitedAttributes(IDiagnosticReceiver diagnosticReceiver, INamedTypeSymbol symbol, DefaultParamCompilationData compilation)
			{
				return DefaultParamAnalyzer.WithDiagnostics.AnalyzeAgainstProhibitedAttributes(diagnosticReceiver, symbol, compilation);
			}

			/// <inheritdoc cref="DefaultParamAnalyzer.WithDiagnostics.AnalyzeAgainstProhibitedAttributes(IDiagnosticReceiver, ISymbol, DefaultParamCompilationData, out AttributeData[])"/>
			public static bool AnalyzeAgainstProhibitedAttributes(IDiagnosticReceiver diagnosticReceiver, INamedTypeSymbol symbol, DefaultParamCompilationData compilation, [NotNullWhen(true)] out AttributeData[]? attributes)
			{
				return DefaultParamAnalyzer.WithDiagnostics.AnalyzeAgainstProhibitedAttributes(diagnosticReceiver, symbol, compilation, out attributes);
			}

			/// <inheritdoc cref="AnalyzeCollidingMembers(IDiagnosticReceiver, INamedTypeSymbol, in TypeParameterContainer, DefaultParamCompilationData, string, IEnumerable{AttributeData}, INamedTypeSymbol[], out HashSet{int}?, CancellationToken)"/>
			public static bool AnalyzeCollidingMembers(
				IDiagnosticReceiver diagnosticReceiver,
				INamedTypeSymbol symbol,
				in TypeParameterContainer typeParameters,
				DefaultParamCompilationData compilation,
				string targetNamespace,
				out HashSet<int>? applyNew,
				CancellationToken cancellationToken = default
			)
			{
				return AnalyzeCollidingMembers(
					diagnosticReceiver,
					symbol,
					in typeParameters,
					compilation,
					targetNamespace,
					symbol.GetAttributes(),
					symbol.GetContainingTypeSymbols().ToArray(),
					out applyNew,
					cancellationToken
				);
			}

			/// <summary>
			/// Analyzes all the colliding members of the given <paramref name="symbol"/>.
			/// </summary>
			/// <param name="diagnosticReceiver">
			/// <see cref="IDiagnosticReceiver"/> that is used to report <see cref="Diagnostic"/> s.
			/// </param>
			/// <param name="symbol">
			/// <see cref="INamedTypeSymbol"/> to analyze the colliding members of.
			/// </param>
			/// <param name="typeParameters">
			/// <see cref="TypeParameterContainer"/> containing type parameters of the <paramref name="symbol"/>.
			/// </param>
			/// <param name="compilation">Current <see cref="DefaultParamCompilationData"/>.</param>
			/// <param name="targetNamespace">Namespace where the generated members are located.</param>
			/// <param name="attributes">
			/// A collection of <see cref="AttributeData"/> a of the target <paramref name="symbol"/>.
			/// </param>
			/// <param name="containingTypes">
			/// An array of <see cref="INamedTypeSymbol"/> s of the <paramref name="symbol"/>'s
			/// containing types.
			/// </param>
			/// <param name="applyNew">
			/// <see langword="abstract"/><see cref="HashSet{T}"/> of indexes of type parameters
			/// with the <see cref="DefaultParamAttribute"/> applied for whom the <see
			/// langword="new"/> modifier should be applied. -or- <see langword="null"/> if the
			/// <paramref name="symbol"/> is not valid.
			/// </param>
			/// <param name="cancellationToken">
			/// <see cref="CancellationToken"/> that specifies if the operation should be canceled.
			/// </param>
			/// <returns>
			/// <see langword="true"/> if there aren't any collisions with the <paramref
			/// name="symbol"/>, otherwise <see langword="false"/>.
			/// </returns>
			public static bool AnalyzeCollidingMembers(
				IDiagnosticReceiver diagnosticReceiver,
				INamedTypeSymbol symbol,
				in TypeParameterContainer typeParameters,
				DefaultParamCompilationData compilation,
				string targetNamespace,
				IEnumerable<AttributeData> attributes,
				INamedTypeSymbol[] containingTypes,
				out HashSet<int>? applyNew,
				CancellationToken cancellationToken = default
			)
			{
				bool allowsNewModifier = AllowsNewModifier(attributes, containingTypes, compilation);

				return AnalyzeCollidingMembers(
					diagnosticReceiver,
					symbol,
					in typeParameters,
					compilation,
					targetNamespace,
					allowsNewModifier,
					out applyNew,
					cancellationToken
				);
			}

			/// <summary>
			/// Analyzes all the colliding members of the given <paramref name="symbol"/>.
			/// </summary>
			/// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> that is used to report <see cref="Diagnostic"/>s.</param>
			/// <param name="symbol"><see cref="INamedTypeSymbol"/> to analyze the colliding members of.</param>
			/// <param name="typeParameters"><see cref="TypeParameterContainer"/> containing type parameters of the <paramref name="symbol"/>.</param>
			/// <param name="compilation">Current <see cref="DefaultParamCompilationData"/>.</param>
			/// <param name="targetNamespace">Namespace where the generated members are located.</param>
			/// <param name="allowsNewModifier">Determines whether to allows applying the <see langword="new"/> modifier.</param>
			/// <param name="applyNew"><see langword="abstract"/><see cref="HashSet{T}"/> of indexes of type parameters with the <see cref="DefaultParamAttribute"/> applied for whom the <see langword="new"/> modifier should be applied. -or- <see langword="null"/> if the <paramref name="symbol"/> is not valid.</param>
			/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
			/// <returns><see langword="true"/> if there aren't any collisions with the <paramref name="symbol"/>, otherwise <see langword="false"/>.</returns>
			public static bool AnalyzeCollidingMembers(
				IDiagnosticReceiver diagnosticReceiver,
				INamedTypeSymbol symbol,
				in TypeParameterContainer typeParameters,
				DefaultParamCompilationData compilation,
				string targetNamespace,
				bool allowsNewModifier,
				out HashSet<int>? applyNew,
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
					diagnosticReceiver,
					symbol,
					in typeParameters,
					targetNamespace,
					collidingMethods,
					allowsNewModifier,
					cancellationToken,
					out applyNew
				);
			}

			/// <inheritdoc cref="DefaultParamAnalyzer.WithDiagnostics.AnalyzeContainingTypes(IDiagnosticReceiver, ISymbol, DefaultParamCompilationData, CancellationToken)"/>
			public static bool AnalyzeContainingTypes(IDiagnosticReceiver diagnosticReceiver, INamedTypeSymbol symbol, DefaultParamCompilationData compilation, CancellationToken cancellationToken = default)
			{
				return DefaultParamAnalyzer.WithDiagnostics.AnalyzeContainingTypes(diagnosticReceiver, symbol, compilation, cancellationToken);
			}

			/// <inheritdoc cref="DefaultParamAnalyzer.WithDiagnostics.AnalyzeContainingTypes(IDiagnosticReceiver, ISymbol, DefaultParamCompilationData, out ITypeData[])"/>
			public static bool AnalyzeContainingTypes(IDiagnosticReceiver diagnosticReceiver, INamedTypeSymbol symbol, DefaultParamCompilationData compilation, [NotNullWhen(true)] out ITypeData[]? containingTypes)
			{
				return DefaultParamAnalyzer.WithDiagnostics.AnalyzeContainingTypes(diagnosticReceiver, symbol, compilation, out containingTypes);
			}

			/// <inheritdoc cref="DefaultParamAnalyzer.WithDiagnostics.AnalyzeTypeParameters(IDiagnosticReceiver, ISymbol,in TypeParameterContainer)"/>
			public static bool AnalyzeTypeParameters(IDiagnosticReceiver diagnosticReceiver, INamedTypeSymbol symbol, in TypeParameterContainer typeParameters)
			{
				return DefaultParamAnalyzer.WithDiagnostics.AnalyzeTypeParameters(diagnosticReceiver, symbol, in typeParameters);
			}

			private static bool AnalyzeCollidingMembers_Internal(
				IDiagnosticReceiver diagnosticReceiver,
				INamedTypeSymbol symbol,
				in TypeParameterContainer typeParameters,
				string targetNamespace,
				CollidingMember[] collidingMembers,
				bool applyNewModifierIsPossible,
				CancellationToken cancellationToken,
				out HashSet<int>? applyNew
			)
			{
				HashSet<int> diagnosed = new();
				HashSet<int> applyNewLocal = new();
				int numCollisions = collidingMembers.Length;
				bool isValid = true;
				bool allowsNewModifier = applyNewModifierIsPossible || HasNewModifier(symbol, cancellationToken);

				for (int i = 0; i < numCollisions; i++)
				{
					ref readonly CollidingMember member = ref collidingMembers[i];

					if (member.TypeParameters is null)
					{
						int index = typeParameters.Length - 1;

						if (allowsNewModifier && member.IsChild && !SymbolEqualityComparer.Default.Equals(member.Symbol.ContainingType, symbol.ContainingType))
						{
							applyNewLocal.Add(index);
						}
						else
						{
							if (member.IsChild)
							{
								diagnosticReceiver.ReportDiagnostic(DefaultParamDiagnostics.DUR0116_MemberWithNameAlreadyExists, symbol, symbol.Name);
							}
							else
							{
								diagnosticReceiver.ReportDiagnostic(DefaultParamDiagnostics.DUR0129_TargetNamespaceAlreadyContainsMemberWithName, symbol, targetNamespace, symbol.Name);
							}

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

					if (member.Parameters is null && allowsNewModifier && member.IsChild && !SymbolEqualityComparer.Default.Equals(member.Symbol.ContainingType, symbol.ContainingType))
					{
						applyNewLocal.Add(targetIndex);
						continue;
					}

					if (member.IsChild)
					{
						diagnosticReceiver.ReportDiagnostic(DefaultParamDiagnostics.DUR0116_MemberWithNameAlreadyExists, symbol, member.Symbol.MetadataName);
					}
					else
					{
						diagnosticReceiver.ReportDiagnostic(DefaultParamDiagnostics.DUR0129_TargetNamespaceAlreadyContainsMemberWithName, symbol, targetNamespace, member.Symbol.MetadataName);
					}

					isValid = false;

					if (diagnosed.Add(targetIndex) && diagnosed.Count == typeParameters.NumDefaultParam)
					{
						break;
					}
				}

				applyNew = GetApplyNewOrNull(applyNewLocal);
				return isValid;
			}
		}
	}
}
