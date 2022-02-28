// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Durian.Analysis.Data;
using Durian.Analysis.Extensions;
using Durian.Generator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Durian.Analysis.DefaultParam
{
	/// <summary>
	/// Base class for all DefaultParam analyzers. Contains <see langword="static"/> methods that perform the most basic DefaultParam-related analysis.
	/// </summary>
	public abstract partial class DefaultParamAnalyzer : DurianAnalyzer<DefaultParamCompilationData>
	{
		/// <inheritdoc/>
		public sealed override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.CreateRange(GetBaseDiagnostics().Concat(GetAnalyzerSpecificDiagnostics()));

		/// <summary>
		/// <see cref="SymbolKind"/> this analyzer can handle.
		/// </summary>
		public abstract SymbolKind SupportedSymbolKind { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="DefaultParamAnalyzer"/> class.
		/// </summary>
		protected DefaultParamAnalyzer()
		{
		}

		/// <summary>
		/// Analyzes if the provided collection of <see cref="AttributeData"/>s contains <see cref="DurianGeneratedAttribute"/> or <see cref="GeneratedCodeAttribute"/> and reports appropriate <see cref="Diagnostic"/>s.
		/// </summary>
		/// <param name="symbol"><see cref="ISymbol"/> that owns the <paramref name="attributes"/>.</param>
		/// <param name="compilation">Current <see cref="DefaultParamCompilationData"/>.</param>
		/// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> that is used to report <see cref="Diagnostic"/>s.</param>
		/// <param name="attributes">A collection of <see cref="AttributeData"/> to analyze.</param>
		/// <returns><see langword="true"/> if all the <paramref name="attributes"/> are valid (neither of them is prohibited), otherwise <see langword="false"/>.</returns>
		public static bool AnalyzeAgainstProhibitedAttributes(
			ISymbol symbol,
			DefaultParamCompilationData compilation,
			IDiagnosticReceiver diagnosticReceiver,
			IEnumerable<AttributeData>? attributes = null
		)
		{
			InitializeAttributes(ref attributes, symbol);

			foreach (AttributeData attr in attributes)
			{
				if (SymbolEqualityComparer.Default.Equals(attr.AttributeClass, compilation.GeneratedCodeAttribute) ||
					SymbolEqualityComparer.Default.Equals(attr.AttributeClass, compilation.DurianGeneratedAttribute))
				{
					diagnosticReceiver.ReportDiagnostic(DefaultParamDiagnostics.DUR0104_DefaultParamCannotBeAppliedWhenGenerationAttributesArePresent, symbol);

					return false;
				}
			}

			return true;
		}

		/// <summary>
		/// Analyzes if the <paramref name="symbol"/> has <see cref="DurianGeneratedAttribute"/> or <see cref="GeneratedCodeAttribute"/> and reports <see cref="Diagnostic"/>s if the <paramref name="symbol"/> is not valid. If the <paramref name="symbol"/> is valid, returns an array of <paramref name="attributes"/> of that <paramref name="symbol"/>.
		/// </summary>
		/// <param name="symbol"><see cref="ISymbol"/> to analyze.</param>
		/// <param name="compilation">Current <see cref="DefaultParamCompilationData"/>.</param>
		/// <param name="attributes">An array of <see cref="AttributeData"/>s of the <paramref name="symbol"/>. Returned if the method itself returns <see langword="true"/>.</param>
		/// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> that is used to report <see cref="Diagnostic"/>s.</param>
		/// <returns><see langword="true"/> if the <paramref name="symbol"/> is valid (does not have the prohibited attributes), otherwise <see langword="false"/>.</returns>
		public static bool AnalyzeAgainstProhibitedAttributes(
			ISymbol symbol,
			DefaultParamCompilationData compilation,
			out AttributeData[]? attributes,
			IDiagnosticReceiver diagnosticReceiver
		)
		{
			AttributeData[] attrs = symbol.GetAttributes().ToArray();
			bool isValid = AnalyzeAgainstProhibitedAttributes(symbol, compilation, diagnosticReceiver, attrs);
			attributes = isValid ? attrs : null;
			return isValid;
		}

		/// <summary>
		/// Analyzes if the containing types of the <paramref name="symbol"/> are valid and reports appropriate <see cref="Diagnostic"/>s.
		/// </summary>
		/// <param name="symbol"><see cref="ISymbol"/> to analyze.</param>
		/// <param name="compilation">Current <see cref="DefaultParamCompilationData"/>.</param>
		/// <param name="containingTypes">An array of the <paramref name="symbol"/>'s containing types' <see cref="INamedTypeSymbol"/>s. Returned if the method itself returns <see langword="true"/>.</param>
		/// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> that is used to report <see cref="Diagnostic"/>s.</param>
		/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
		/// <returns><see langword="true"/> if the containing types of the <paramref name="symbol"/> are valid, otherwise <see langword="false"/>.</returns>
		public static bool AnalyzeContainingTypes(
			ISymbol symbol,
			DefaultParamCompilationData compilation,
			[NotNullWhen(true)] out INamedTypeSymbol[]? containingTypes,
			IDiagnosticReceiver diagnosticReceiver,
			CancellationToken cancellationToken = default
		)
		{
			INamedTypeSymbol[] types = symbol.GetContainingTypeSymbols().ToArray();
			bool isValid = AnalyzeContainingTypes(symbol, compilation, diagnosticReceiver, types, cancellationToken);
			containingTypes = isValid ? types : null;
			return isValid;
		}

		/// <summary>
		/// Analyzes if the containing types of the <paramref name="symbol"/> are valid and reports appropriate <see cref="Diagnostic"/>s.
		/// </summary>
		/// <param name="symbol"><see cref="ISymbol"/> to analyze.</param>
		/// <param name="compilation">Current <see cref="DefaultParamCompilationData"/>.</param>
		/// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> that is used to report <see cref="Diagnostic"/>s.</param>
		/// <param name="containingTypes">An array of the <paramref name="symbol"/>'s containing types' <see cref="INamedTypeSymbol"/>s. Returned if the method itself returns <see langword="true"/>.</param>
		/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
		/// <returns><see langword="true"/> if the containing types of the <paramref name="symbol"/> are valid, otherwise <see langword="false"/>.</returns>
		public static bool AnalyzeContainingTypes(
			ISymbol symbol,
			DefaultParamCompilationData compilation,
			IDiagnosticReceiver diagnosticReceiver,
			INamedTypeSymbol[]? containingTypes = null,
			CancellationToken cancellationToken = default
		)
		{
			InitializeContainingTypes(ref containingTypes, symbol);

			bool isValid = true;

			if (containingTypes.Length > 0)
			{
				foreach (INamedTypeSymbol parent in containingTypes)
				{
					if (!HasPartialKeyword(parent, cancellationToken))
					{
						diagnosticReceiver.ReportDiagnostic(DefaultParamDiagnostics.DUR0101_ContainingTypeMustBePartial, parent);
						isValid = false;
					}

					ImmutableArray<ITypeParameterSymbol> typeParameters = parent.TypeParameters;

					if (typeParameters.Length > 0 && typeParameters.SelectMany(t => t.GetAttributes()).Any(attr => SymbolEqualityComparer.Default.Equals(attr.AttributeClass, compilation.DefaultParamAttribute)))
					{
						diagnosticReceiver.ReportDiagnostic(DefaultParamDiagnostics.DUR0126_DefaultParamMembersCannotBeNested, symbol);
						isValid = false;
					}
				}
			}

			return isValid;
		}

		/// <summary>
		/// Analyzes if the containing types of the <paramref name="symbol"/> are valid and reports appropriate <see cref="Diagnostic"/>s. If the <paramref name="symbol"/> is valid, returns an array of <see cref="ITypeData"/>s of its containing types.
		/// </summary>
		/// <param name="symbol"><see cref="ISymbol"/> to analyze.</param>
		/// <param name="compilation">Current <see cref="DefaultParamCompilationData"/>.</param>
		/// <param name="containingTypes">An array of the <paramref name="symbol"/>'s containing types' <see cref="ITypeData"/>s. Returned if the method itself returns <see langword="true"/>.</param>
		/// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> that is used to report <see cref="Diagnostic"/>s.</param>
		/// <returns><see langword="true"/> if the containing types of the <paramref name="symbol"/> are valid, otherwise <see langword="false"/>.</returns>
		public static bool AnalyzeContainingTypes(
			ISymbol symbol,
			DefaultParamCompilationData compilation,
			[NotNullWhen(true)] out ITypeData[]? containingTypes,
			IDiagnosticReceiver diagnosticReceiver
		)
		{
			ITypeData[] types = symbol.GetContainingTypes(compilation).ToArray();
			bool isValid = true;

			if (types.Length > 0)
			{
				foreach (ITypeData parent in types)
				{
					if (!parent.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword)))
					{
						diagnosticReceiver.ReportDiagnostic(DefaultParamDiagnostics.DUR0101_ContainingTypeMustBePartial, parent.Symbol);
						isValid = false;
					}

					ImmutableArray<ITypeParameterSymbol> typeParameters = parent.Symbol.TypeParameters;

					if (typeParameters.Length > 0 && typeParameters.SelectMany(t => t.GetAttributes()).Any(attr => SymbolEqualityComparer.Default.Equals(attr.AttributeClass, compilation.DefaultParamAttribute)))
					{
						diagnosticReceiver.ReportDiagnostic(DefaultParamDiagnostics.DUR0126_DefaultParamMembersCannotBeNested, symbol);
						isValid = false;
					}
				}
			}

			containingTypes = isValid ? types : null;

			return isValid;
		}

		/// <summary>
		/// Checks, if the specified <paramref name="typeParameters"/> are valid and reports <see cref="Diagnostic"/> if they are not.
		/// </summary>
		/// <param name="symbol"><see cref="ISymbol"/> to analyze the type parameters of.</param>
		/// <param name="typeParameters"><see cref="TypeParameterContainer"/> to analyze.</param>
		/// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> that is used to report <see cref="Diagnostic"/>s.</param>
		/// <returns><see langword="true"/> if the type parameters contained within the <see cref="TypeParameterContainer"/> are valid, otherwise <see langword="false"/>.</returns>
		public static bool AnalyzeTypeParameters(
			ISymbol symbol,
			in TypeParameterContainer typeParameters,
			IDiagnosticReceiver diagnosticReceiver
		)
		{
			if (!typeParameters.HasDefaultParams)
			{
				return false;
			}

			int length = typeParameters.Length;
			bool isValid = true;
			int lastDefaultParam = typeParameters.FirstDefaultParamIndex;

			for (int i = typeParameters.FirstDefaultParamIndex; i < length; i++)
			{
				ref readonly TypeParameterData data = ref typeParameters[i];

				if (data.IsDefaultParam)
				{
					if (!ValidateTargetTypeParameter(symbol, in data, in typeParameters, diagnosticReceiver))
					{
						isValid = false;
					}

					lastDefaultParam = i;
				}
				else if (lastDefaultParam != -1)
				{
					ref readonly TypeParameterData errorData = ref typeParameters[lastDefaultParam];
					diagnosticReceiver.ReportDiagnostic(DefaultParamDiagnostics.DUR0105_DefaultParamMustBeLast, errorData.Location, errorData.Symbol);
					isValid = false;
					lastDefaultParam = -1;
				}
			}

			return isValid;
		}

		/// <summary>
		/// Performs basic analysis of the <paramref name="symbol"/> and reports <see cref="Diagnostic"/>s if the <paramref name="symbol"/> is not valid.
		/// </summary>
		/// <param name="symbol"><see cref="ISymbol"/> to analyze.</param>
		/// <param name="compilation">Current <see cref="DefaultParamCompilationData"/>.</param>
		/// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> that is used to report <see cref="Diagnostic"/>s.</param>
		/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
		/// <returns><see langword="true"/> if the <paramref name="symbol"/> is valid, otherwise <see langword="false"/>.</returns>
		public static bool DefaultAnalyze(
			ISymbol symbol,
			DefaultParamCompilationData compilation,
			IDiagnosticReceiver diagnosticReceiver,
			CancellationToken cancellationToken = default
		)
		{
			if (!TryGetTypeParameters(symbol, compilation, cancellationToken, out TypeParameterContainer typeParameters))
			{
				return false;
			}

			return DefaultAnalyze(symbol, compilation, in typeParameters, diagnosticReceiver, cancellationToken);
		}

		/// <summary>
		/// Performs basic analysis of the <paramref name="symbol"/> and reports <see cref="Diagnostic"/>s if the <paramref name="symbol"/> is not valid.
		/// </summary>
		/// <param name="symbol"><see cref="ISymbol"/> to analyze.</param>
		/// <param name="compilation">Current <see cref="DefaultParamCompilationData"/>.</param>
		/// <param name="typeParameters"><see cref="TypeParameterContainer"/> that contains the <paramref name="symbol"/>'s type parameters.</param>
		/// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> that is used to report <see cref="Diagnostic"/>s.</param>
		/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
		/// <returns><see langword="true"/> if the <paramref name="symbol"/> is valid, otherwise <see langword="false"/>.</returns>
		public static bool DefaultAnalyze(
			ISymbol symbol,
			DefaultParamCompilationData compilation,
			in TypeParameterContainer typeParameters,
			IDiagnosticReceiver diagnosticReceiver,
			CancellationToken cancellationToken = default
		)
		{
			if (!typeParameters.HasDefaultParams)
			{
				return false;
			}

			bool isValid = AnalyzeAgainstProhibitedAttributes(symbol, compilation, diagnosticReceiver);
			isValid &= AnalyzeContainingTypes(symbol, compilation, diagnosticReceiver, cancellationToken: cancellationToken);
			isValid &= AnalyzeTypeParameters(symbol, in typeParameters, diagnosticReceiver);

			return isValid;
		}

		private static bool ValidateTargetTypeParameter(
			ISymbol symbol,
			in TypeParameterData currentTypeParameter,
			in TypeParameterContainer typeParameters,
			IDiagnosticReceiver diagnosticReceiver
		)
		{
			ITypeSymbol? targetType = currentTypeParameter.TargetType;
			ITypeParameterSymbol typeParameterSymbol = currentTypeParameter.Symbol;

			if (targetType is null ||
				targetType is IErrorTypeSymbol ||
				targetType.IsStatic ||
				targetType.IsRefLikeType ||
				targetType is IFunctionPointerTypeSymbol ||
				targetType is IPointerTypeSymbol ||
				(targetType is INamedTypeSymbol t && (t.IsUnboundGenericType || t.SpecialType == SpecialType.System_Void))
			)
			{
				diagnosticReceiver.ReportDiagnostic(DefaultParamDiagnostics.DUR0121_TypeIsNotValidDefaultParamValue, currentTypeParameter.Location, typeParameterSymbol, targetType);
				return false;
			}

			if (!HasValidParameterAccessibility(symbol, targetType, typeParameterSymbol, in typeParameters))
			{
				diagnosticReceiver.ReportDiagnostic(DefaultParamDiagnostics.DUR0119_DefaultParamValueCannotBeLessAccessibleThanTargetMember, currentTypeParameter.Location, typeParameterSymbol, targetType);
				return false;
			}

			if (targetType.SpecialType == SpecialType.System_Object ||
				targetType.SpecialType == SpecialType.System_Array ||
				targetType.SpecialType == SpecialType.System_ValueType ||
				targetType is IArrayTypeSymbol ||
				targetType.IsValueType ||
				targetType.IsSealed
			)
			{
				if (HasTypeParameterAsConstraint(typeParameterSymbol, in typeParameters))
				{
					diagnosticReceiver.ReportDiagnostic(DefaultParamDiagnostics.DUR0120_TypeCannotBeUsedWithConstraint, currentTypeParameter.Location, typeParameters, targetType);
					return false;
				}
			}

			if (!IsValidForConstraint(targetType, currentTypeParameter.Symbol, in typeParameters))
			{
				diagnosticReceiver.ReportDiagnostic(DefaultParamDiagnostics.DUR0106_TargetTypeDoesNotSatisfyConstraint, currentTypeParameter.Location, typeParameterSymbol, targetType);
				return false;
			}

			return true;
		}

		/// <summary>
		/// Determines whether the 'new' modifier is allowed to be applied to the target <see cref="ISymbol"/> according to the most specific <c>Durian.Configuration.DefaultParamConfigurationAttribute</c> or <c>Durian.Configuration.DefaultParamScopedConfigurationAttribute</c>.
		/// </summary>
		/// <param name="symbol"><see cref="ISymbol"/> to check.</param>
		/// <param name="compilation">Current <see cref="DefaultParamCompilationData"/>.</param>
		/// <param name="attributes">A collection of the target <see cref="ISymbol"/>'s attributes.</param>
		/// <param name="containingTypes"><see cref="INamedTypeSymbol"/>s that contain this <see cref="IMethodSymbol"/>.</param>
		public static bool AllowsNewModifier(
			ISymbol symbol,
			DefaultParamCompilationData compilation,
			IEnumerable<AttributeData>? attributes = null,
			INamedTypeSymbol[]? containingTypes = null
		)
		{
			InitializeAttributes(ref attributes, symbol);
			InitializeContainingTypes(ref containingTypes, symbol);

			const string configPropertyName = DefaultParamConfigurationAttributeProvider.ApplyNewModifierWhenPossible;
			const string scopedPropertyName = DefaultParamScopedConfigurationAttributeProvider.ApplyNewModifierWhenPossible;

			if (DefaultParamUtilities.TryGetConfigurationPropertyValue(attributes, compilation.DefaultParamConfigurationAttribute!, configPropertyName, out bool value))
			{
				return value;
			}
			else
			{
				int length = containingTypes.Length;

				if (length > 0)
				{
					INamedTypeSymbol scopedAttribute = compilation.DefaultParamScopedConfigurationAttribute!;

					foreach (INamedTypeSymbol type in containingTypes.Reverse())
					{
						if (DefaultParamUtilities.TryGetConfigurationPropertyValue(type.GetAttributes(), scopedAttribute, scopedPropertyName, out value))
						{
							return value;
						}
					}
				}

				return compilation.GlobalConfiguration.ApplyNewModifierWhenPossible;
			}
		}

		/// <summary>
		/// Analyzes if the provided collection of <see cref="AttributeData"/>s contains <see cref="DurianGeneratedAttribute"/> or <see cref="GeneratedCodeAttribute"/>.
		/// </summary>
		/// <param name="attributes">A collection of <see cref="AttributeData"/> to analyze.</param>
		/// <param name="compilation">Current <see cref="DefaultParamCompilationData"/>.</param>
		/// <returns><see langword="true"/> if all the <paramref name="attributes"/> are valid (neither of them is prohibited), otherwise <see langword="false"/>.</returns>
		public static bool AnalyzeAgainstProhibitedAttributes(IEnumerable<AttributeData> attributes, DefaultParamCompilationData compilation)
		{
			foreach (AttributeData attr in attributes)
			{
				if (SymbolEqualityComparer.Default.Equals(attr.AttributeClass, compilation.GeneratedCodeAttribute) ||
					SymbolEqualityComparer.Default.Equals(attr.AttributeClass, compilation.DurianGeneratedAttribute))
				{
					return false;
				}
			}

			return true;
		}

		/// <summary>
		/// Analyzes if the <paramref name="symbol"/> has <see cref="DurianGeneratedAttribute"/> or <see cref="GeneratedCodeAttribute"/>.
		/// </summary>
		/// <param name="symbol"><see cref="ISymbol"/> to analyze.</param>
		/// <param name="compilation">Current <see cref="DefaultParamCompilationData"/>.</param>
		/// <returns><see langword="true"/> if the <paramref name="symbol"/> is valid (does not have the prohibited attributes), otherwise <see langword="false"/>.</returns>
		public static bool AnalyzeAgainstProhibitedAttributes(ISymbol symbol, DefaultParamCompilationData compilation)
		{
			return AnalyzeAgainstProhibitedAttributes(symbol, compilation, out _);
		}

		/// <summary>
		/// Analyzes if the <paramref name="symbol"/> has <see cref="DurianGeneratedAttribute"/> or <see cref="GeneratedCodeAttribute"/>. If the <paramref name="symbol"/> is valid, returns an array of <paramref name="attributes"/> of that <paramref name="symbol"/>.
		/// </summary>
		/// <param name="symbol"><see cref="ISymbol"/> to analyze.</param>
		/// <param name="compilation">Current <see cref="DefaultParamCompilationData"/>.</param>
		/// <param name="attributes">An array of <see cref="AttributeData"/>s of the <paramref name="symbol"/>. Returned if the method itself returns <see langword="true"/>.</param>
		/// <returns><see langword="true"/> if the <paramref name="symbol"/> is valid (does not have the prohibited attributes), otherwise <see langword="false"/>.</returns>
		public static bool AnalyzeAgainstProhibitedAttributes(
			ISymbol symbol,
			DefaultParamCompilationData compilation,
			[NotNullWhen(true)] out AttributeData[]? attributes
		)
		{
			AttributeData[] attrs = symbol.GetAttributes().ToArray();
			bool isValid = AnalyzeAgainstProhibitedAttributes(attrs, compilation);
			attributes = isValid ? attrs : null;
			return isValid;
		}

		/// <summary>
		/// Analyzes if the containing types of the <paramref name="symbol"/> are valid.
		/// </summary>
		/// <param name="symbol"><see cref="ISymbol"/> to analyze.</param>
		/// <param name="compilation">Current <see cref="DefaultParamCompilationData"/>.</param>
		/// <param name="containingTypes">An array of the <paramref name="symbol"/>'s containing types' <see cref="INamedTypeSymbol"/>s. Returned if the method itself returns <see langword="true"/>.</param>
		/// <param name="cancellationToken"></param>
		/// <returns><see langword="true"/> if the containing types of the <paramref name="symbol"/> are valid, otherwise <see langword="false"/>.</returns>
		public static bool AnalyzeContainingTypes(
			ISymbol symbol,
			DefaultParamCompilationData compilation,
			[NotNullWhen(true)] out INamedTypeSymbol[]? containingTypes,
			CancellationToken cancellationToken = default
		)
		{
			INamedTypeSymbol[] containing = symbol.GetContainingTypeSymbols().ToArray();
			bool isValid = AnalyzeContainingTypes(containing, compilation, cancellationToken);
			containingTypes = isValid ? containing : null;
			return isValid;
		}

		/// <summary>
		/// Analyzes if the <paramref name="containingTypes"/> of the target <see cref="ISymbol"/> are valid.
		/// </summary>
		/// <param name="containingTypes">An array of <see cref="INamedTypeSymbol"/>s to analyze.</param>
		/// <param name="compilation">Current <see cref="DefaultParamCompilationData"/>.</param>
		/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
		/// <returns><see langword="true"/> if the <paramref name="containingTypes"/> of the target <see cref="ISymbol"/> are valid, otherwise <see langword="false"/>.</returns>
		public static bool AnalyzeContainingTypes(
			INamedTypeSymbol[] containingTypes,
			DefaultParamCompilationData compilation,
			CancellationToken cancellationToken = default
		)
		{
			if (containingTypes.Length > 0)
			{
				foreach (INamedTypeSymbol parent in containingTypes)
				{
					if (!HasPartialKeyword(parent, cancellationToken))
					{
						return false;
					}

					ImmutableArray<ITypeParameterSymbol> typeParameters = parent.TypeParameters;

					if (typeParameters.Length > 0 && typeParameters.SelectMany(t => t.GetAttributes()).Any(attr => SymbolEqualityComparer.Default.Equals(attr.AttributeClass, compilation.DefaultParamAttribute)))
					{
						return false;
					}
				}
			}

			return true;
		}

		/// <summary>
		/// Analyzes if the containing types of the <paramref name="symbol"/> are valid.
		/// </summary>
		/// <param name="symbol"><see cref="ISymbol"/> to analyze.</param>
		/// <param name="compilation">Current <see cref="DefaultParamCompilationData"/>.</param>
		/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
		/// <returns><see langword="true"/> if the containing types of the <paramref name="symbol"/> are valid, otherwise <see langword="false"/>.</returns>
		public static bool AnalyzeContainingTypes(
			ISymbol symbol,
			DefaultParamCompilationData compilation,
			CancellationToken cancellationToken = default
		)
		{
			INamedTypeSymbol[] types = symbol.GetContainingTypeSymbols().ToArray();

			return AnalyzeContainingTypes(types, compilation, cancellationToken);
		}

		/// <summary>
		/// Analyzes if the <paramref name="symbol"/> and its containing types are valid.
		/// </summary>
		/// <param name="symbol"><see cref="ISymbol"/> to analyze.</param>
		/// <param name="compilation">Current <see cref="DefaultParamCompilationData"/>.</param>
		/// <param name="containingTypes">An array of the <paramref name="symbol"/>'s containing types' <see cref="ITypeData"/>s. Returned if the method itself returns <see langword="true"/>.</param>
		/// <returns><see langword="true"/> if the containing types of the <paramref name="symbol"/> are valid, otherwise <see langword="false"/>.</returns>
		public static bool AnalyzeContainingTypes(
			ISymbol symbol,
			DefaultParamCompilationData compilation,
			[NotNullWhen(true)] out ITypeData[]? containingTypes
		)
		{
			ITypeData[] types = symbol.GetContainingTypes(compilation).ToArray();

			if (types.Length > 0)
			{
				foreach (ITypeData parent in types)
				{
					if (!HasPartialKeyword(parent))
					{
						containingTypes = null;
						return false;
					}

					ImmutableArray<ITypeParameterSymbol> typeParameters = parent.Symbol.TypeParameters;

					if (typeParameters.Length > 0 && typeParameters.SelectMany(t => t.GetAttributes()).Any(attr => SymbolEqualityComparer.Default.Equals(attr.AttributeClass, compilation.DefaultParamAttribute)))
					{
						containingTypes = null;
						return false;
					}
				}
			}

			containingTypes = types;
			return true;
		}

		/// <summary>
		/// Checks, if the specified <paramref name="typeParameters"/> are valid.
		/// </summary>
		/// <param name="symbol"><see cref="ISymbol"/> to analyze the type parameters of.</param>
		/// <param name="typeParameters"><see cref="TypeParameterContainer"/> to analyze.</param>
		/// <returns><see langword="true"/> if the type parameters contained within the <see cref="TypeParameterContainer"/> are valid, otherwise <see langword="false"/>.</returns>
		public static bool AnalyzeTypeParameters(ISymbol symbol, in TypeParameterContainer typeParameters)
		{
			if (!typeParameters.HasDefaultParams)
			{
				return false;
			}

			int length = typeParameters.Length;

			for (int i = typeParameters.FirstDefaultParamIndex; i < length; i++)
			{
				ref readonly TypeParameterData data = ref typeParameters[i];

				if (data.IsDefaultParam)
				{
					if (!ValidateTargetTypeParameter(symbol, in data, in typeParameters))
					{
						return false;
					}
				}
				else
				{
					return false;
				}
			}

			return true;
		}

		/// <summary>
		/// Performs basic analysis of the <paramref name="symbol"/>.
		/// </summary>
		/// <param name="symbol"><see cref="ISymbol"/> to analyze.</param>
		/// <param name="compilation">Current <see cref="DefaultParamCompilationData"/>.</param>
		/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
		/// <returns><see langword="true"/> if the <paramref name="symbol"/> is valid, otherwise <see langword="false"/>.</returns>
		public static bool DefaultAnalyze(ISymbol symbol, DefaultParamCompilationData compilation, CancellationToken cancellationToken = default)
		{
			if (!TryGetTypeParameters(symbol, compilation, cancellationToken, out TypeParameterContainer typeParameters))
			{
				return false;
			}

			return DefaultAnalyze(symbol, compilation, in typeParameters, cancellationToken);
		}

		/// <summary>
		/// Performs basic analysis of the <paramref name="symbol"/>.
		/// </summary>
		/// <param name="symbol"><see cref="ISymbol"/> to analyze.</param>
		/// <param name="compilation">Current <see cref="DefaultParamCompilationData"/>.</param>
		/// <param name="typeParameters"><see cref="TypeParameterContainer"/> that contains the <paramref name="symbol"/>'s type parameters.</param>
		/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
		/// <returns><see langword="true"/> if the <paramref name="symbol"/> is valid, otherwise <see langword="false"/>.</returns>
		public static bool DefaultAnalyze(
			ISymbol symbol,
			DefaultParamCompilationData compilation,
			in TypeParameterContainer typeParameters,
			CancellationToken cancellationToken = default
		)
		{
			if (!typeParameters.HasDefaultParams)
			{
				return false;
			}

			return
				AnalyzeAgainstProhibitedAttributes(symbol, compilation) &&
				AnalyzeContainingTypes(symbol, compilation, cancellationToken) &&
				AnalyzeTypeParameters(symbol, in typeParameters);
		}

		/// <summary>
		/// Returns a collection of <see cref="CollidingMember"/>s representing <see cref="ISymbol"/>s that can potentially collide with members generated from the specified <paramref name="symbol"/>.
		/// </summary>
		/// <param name="symbol"><see cref="ISymbol"/> to get the colliding members of.</param>
		/// <param name="compilation">Current <see cref="DefaultParamCompilationData"/>.</param>
		/// <param name="targetNamespace">Namespace where the generated members are located.</param>
		/// <param name="typeParameters"><see cref="TypeParameterContainer"/> that contains the <paramref name="symbol"/>'s type parameters.</param>
		/// <param name="numParameters">Number of parameters of this <paramref name="symbol"/>. Always use <c>0</c> for members other than methods.</param>
		public static CollidingMember[] GetPotentiallyCollidingMembers(
			ISymbol symbol,
			DefaultParamCompilationData compilation,
			string? targetNamespace,
			in TypeParameterContainer typeParameters,
			int numParameters = 0
		)
		{
			return GetPotentiallyCollidingMembers(symbol, compilation, targetNamespace, typeParameters.Length, typeParameters.NumNonDefaultParam, numParameters);
		}

		/// <summary>
		/// Returns a collection of <see cref="CollidingMember"/>s representing <see cref="ISymbol"/>s that can potentially collide with members generated from the specified <paramref name="symbol"/>.
		/// </summary>
		/// <param name="symbol"><see cref="ISymbol"/> to get the colliding members of.</param>
		/// <param name="compilation">Current <see cref="DefaultParamCompilationData"/>.</param>
		/// <param name="targetNamespace">Namespace where the generated members are located.</param>
		/// <param name="numTypeParameters">Number of type parameters of this <paramref name="symbol"/>.</param>
		/// <param name="numNonDefaultParam">Number of type parameters of this <paramref name="symbol"/> that don't have the <c>Durian.DefaultParamAttribute</c>.</param>
		/// <param name="numParameters">Number of parameters of this <paramref name="symbol"/>. Always use <c>0</c> for members other than methods.</param>
		public static CollidingMember[] GetPotentiallyCollidingMembers(
			ISymbol symbol,
			DefaultParamCompilationData compilation,
			string? targetNamespace,
			int numTypeParameters,
			int numNonDefaultParam,
			int numParameters = 0
		)
		{
			return GetPotentiallyCollidingMembers_Internal(symbol, compilation, targetNamespace, numTypeParameters, numNonDefaultParam, numParameters)
				.Select(s =>
				{
					if (s is IMethodSymbol m)
					{
						return new CollidingMember(m, m.TypeParameters.ToArray(), m.Parameters.ToArray());
					}
					else if (s is INamedTypeSymbol t)
					{
						return new CollidingMember(t, t.TypeParameters.ToArray(), null);
					}

					return new CollidingMember(s, null, null);
				})
				.ToArray();
		}

		/// <summary>
		/// Returns a <see cref="string"/> representing a namespace the target member should be generated in.
		/// </summary>
		/// <param name="symbol">Original <see cref="ISymbol"/> the generated member is based on.</param>
		/// <param name="compilation">Current <see cref="DefaultParamCompilationData"/>.</param>
		/// <param name="attributes">A collection of <see cref="AttributeData"/>a of the target <paramref name="symbol"/>.</param>
		/// <param name="containingTypes">An array of <see cref="INamedTypeSymbol"/>s of the <paramref name="symbol"/>'s containing types in root-first order.</param>
		public static string GetTargetNamespace(
			ISymbol symbol,
			DefaultParamCompilationData compilation,
			IEnumerable<AttributeData>? attributes = null,
			INamedTypeSymbol[]? containingTypes = null
		)
		{
			const string propertyName = DefaultParamConfigurationAttributeProvider.TargetNamespace;

			InitializeAttributes(ref attributes, symbol);
			InitializeContainingTypes(ref containingTypes, symbol);

			if (DefaultParamUtilities.TryGetConfigurationPropertyValue(attributes, compilation.DefaultParamConfigurationAttribute!, propertyName, out string? value))
			{
				return GetValueOrParentNamespace(value);
			}

			INamedTypeSymbol scopedConfigurationAttribute = compilation.DefaultParamScopedConfigurationAttribute!;

			foreach (INamedTypeSymbol type in containingTypes.Reverse())
			{
				if (DefaultParamUtilities.TryGetConfigurationPropertyValue(type.GetAttributes(), scopedConfigurationAttribute, propertyName, out value))
				{
					return GetValueOrParentNamespace(value);
				}
			}

			return GetValueOrParentNamespace(compilation.GlobalConfiguration.TargetNamespace);

			string GetValueOrParentNamespace(string? value)
			{
				if (symbol.ContainingType is not null || value == "Durian.Generator" || !AnalysisUtilities.IsValidNamespaceIdentifier(value!))
				{
					string n = symbol.JoinNamespaces();

					return string.IsNullOrWhiteSpace(n) ? "global" : n;
				}

				return value!;
			}
		}

		/// <summary>
		/// Determines whether the specified <paramref name="collidingMember"/> actually collides with the <paramref name="targetParameters"/>.
		/// </summary>
		/// <param name="targetParameters">Array of <see cref="ParameterGeneration"/> representing parameters of a generated method.</param>
		/// <param name="collidingMember"><see cref="CollidingMember"/> to check of actually collides with the <paramref name="targetParameters"/>.</param>
		public static bool HasCollidingParameters(ParameterGeneration[] targetParameters, in CollidingMember collidingMember)
		{
			int numParameters = targetParameters.Length;

			for (int i = 0; i < numParameters; i++)
			{
				ref readonly ParameterGeneration generation = ref targetParameters[i];
				IParameterSymbol parameter = collidingMember.Parameters![i];

				if (IsValidParameterInCollidingMember(collidingMember.TypeParameters!, parameter, in generation))
				{
					return false;
				}
			}

			return true;
		}

		/// <summary>
		/// Determines whether the specified <paramref name="symbol"/> has the <see langword="new"/> modifier.
		/// </summary>
		/// <param name="symbol"><see cref="ISymbol"/> to check.</param>
		/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
		public static bool HasNewModifier(ISymbol symbol, CancellationToken cancellationToken = default)
		{
			if (symbol.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax(cancellationToken) is MemberDeclarationSyntax m)
			{
				return m.Modifiers.Any(m => m.IsKind(SyntaxKind.NewKeyword));
			}

			return false;
		}

		/// <summary>
		/// Determines, whether the specified <paramref name="symbol"/> has a <see cref="GeneratedCodeAttribute"/> with the <see cref="DefaultParamGenerator.GeneratorName"/> specified as the <see cref="GeneratedCodeAttribute.Tool"/>.
		/// </summary>
		/// <param name="symbol"><see cref="ISymbol"/> to check.</param>
		/// <param name="compilation">Current <see cref="DefaultParamCompilationData"/>.</param>
		public static bool IsDefaultParamGenerated(ISymbol symbol, DefaultParamCompilationData compilation)
		{
			AttributeData? attr = symbol.GetAttribute(compilation.GeneratedCodeAttribute!);

			if (attr is null)
			{
				return false;
			}

			if (attr.ConstructorArguments.FirstOrDefault().Value is not string tool)
			{
				return false;
			}

			return tool == DefaultParamGenerator.GeneratorName;
		}

		/// <summary>
		/// Analyzes the specified <paramref name="symbol"/>.
		/// </summary>
		/// <param name="symbol"><see cref="ISymbol"/> to analyze.</param>
		/// <param name="compilation">Current <see cref="DefaultParamCompilationData"/>.</param>
		/// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> that is used to report <see cref="Diagnostic"/>s.</param>
		/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
		protected virtual void Analyze(
			ISymbol symbol,
			DefaultParamCompilationData compilation,
			IDiagnosticReceiver diagnosticReceiver,
			CancellationToken cancellationToken = default
		)
		{
			DefaultAnalyze(symbol, compilation, diagnosticReceiver, cancellationToken);
		}

		/// <inheritdoc/>
		public override void Register(IDurianAnalysisContext context, DefaultParamCompilationData compilation)
		{
			context.RegisterSymbolAction(c => AnalyzeSymbol(c, compilation), SupportedSymbolKind);
		}

		private protected static HashSet<int>? GetApplyNewOrNull(HashSet<int> applyNew)
		{
			if (applyNew.Count == 0)
			{
				return null;
			}

			return applyNew;
		}

		private protected static IEnumerable<DiagnosticDescriptor> GetBaseDiagnostics()
		{
			return new[]
			{
				DefaultParamDiagnostics.DUR0101_ContainingTypeMustBePartial,
				DefaultParamDiagnostics.DUR0104_DefaultParamCannotBeAppliedWhenGenerationAttributesArePresent,
				DefaultParamDiagnostics.DUR0105_DefaultParamMustBeLast,
				DefaultParamDiagnostics.DUR0106_TargetTypeDoesNotSatisfyConstraint,
				DefaultParamDiagnostics.DUR0116_MemberWithNameAlreadyExists,
				DefaultParamDiagnostics.DUR0119_DefaultParamValueCannotBeLessAccessibleThanTargetMember,
				DefaultParamDiagnostics.DUR0120_TypeCannotBeUsedWithConstraint,
				DefaultParamDiagnostics.DUR0121_TypeIsNotValidDefaultParamValue,
				DefaultParamDiagnostics.DUR0126_DefaultParamMembersCannotBeNested
			};
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private protected static void InitializeAttributes([NotNull] ref IEnumerable<AttributeData>? attributes, ISymbol symbol)
		{
			if (attributes is null)
			{
				attributes = symbol.GetAttributes();
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private protected static void InitializeContainingTypes([NotNull] ref INamedTypeSymbol[]? containingTypes, ISymbol symbol)
		{
			if (containingTypes is null)
			{
				containingTypes = symbol.GetContainingTypeSymbols().ToArray();
			}
		}

		/// <inheritdoc/>
		protected override DefaultParamCompilationData CreateCompilation(CSharpCompilation compilation, IDiagnosticReceiver diagnosticReceiver)
		{
			return new DefaultParamCompilationData(compilation);
		}

		/// <summary>
		/// Returns a collection of <see cref="DiagnosticDescriptor"/>s that are used by this analyzer specifically.
		/// </summary>
		protected abstract IEnumerable<DiagnosticDescriptor> GetAnalyzerSpecificDiagnostics();

		/// <summary>
		/// Determines whether analysis should be performed for the specified <paramref name="symbol"/> and <paramref name="compilation"/>.
		/// </summary>
		/// <param name="symbol"><see cref="ISymbol"/> to check if should be analyzed.</param>
		/// <param name="compilation">Current <see cref="DefaultParamCompilationData"/>.</param>
		protected abstract bool ShouldAnalyze(ISymbol symbol, DefaultParamCompilationData compilation);

		private static IEnumerable<ISymbol> GetCollidingMembersForMethodSymbol(IMethodSymbol method, string fullName, IEnumerable<ISymbol> symbols, INamedTypeSymbol generatedFromAttribute, int numParameters)
		{
			IEnumerable<ISymbol> members = symbols.Where(s =>
			{
				if (s is IMethodSymbol m)
				{
					return m.Parameters.Length == numParameters;
				}

				return true;
			});

			if (method.IsOverride && method.OverriddenMethod is IMethodSymbol baseMethod)
			{
				string baseFullName = baseMethod.ToString();
				List<string> methodNames = new() { fullName, baseFullName };
				methodNames.AddRange(baseMethod.GetBaseMethods().Select(m => m.ToString()));

				return members.Where(s =>
				{
					if (s is IMethodSymbol m)
					{
						foreach (AttributeData attr in m.GetAttributes())
						{
							if (SymbolEqualityComparer.Default.Equals(generatedFromAttribute, attr.AttributeClass) && attr.TryGetConstructorArgumentValue(0, out string? value))
							{
								return value is not null && !methodNames.Contains(value);
							}
						}
					}

					return true;
				});
			}

			return members.Where(s =>
			{
				if (s is IMethodSymbol)
				{
					return !IsGeneratedFrom(s, fullName, generatedFromAttribute);
				}

				return true;
			});
		}

		private static INamedTypeSymbol[] GetCollidingNotNestedTypes(
			ISymbol symbol,
			DefaultParamCompilationData compilation,
			string? targetNamespace,
			int numTypeParameters,
			int numNonDefaultParam
		)
		{
			INamedTypeSymbol generatedFromAttribute = compilation.DurianGeneratedAttribute!;
			int numDefaultParam = numTypeParameters - numNonDefaultParam;
			string name = symbol.Name;
			string metadata = string.IsNullOrWhiteSpace(targetNamespace) || targetNamespace == "global" ? name : $"{targetNamespace}.{name}";
			string fullName = symbol.ToString();

			List<INamedTypeSymbol> symbols = new(numDefaultParam);

			if (numTypeParameters == numDefaultParam)
			{
				TryAdd(metadata);
			}

			for (int i = numNonDefaultParam; i < numTypeParameters; i++)
			{
				TryAdd($"{metadata}`{i}");
			}

			return symbols.ToArray();

			void TryAdd(string metadataName)
			{
				INamedTypeSymbol? type = compilation.Compilation.GetTypeByMetadataName(metadataName);

				if (type is not null && !IsGeneratedFrom(type, fullName, generatedFromAttribute))
				{
					symbols.Add(type);
				}
			}
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

		private static IEnumerable<ISymbol> GetPotentiallyCollidingMembers_Internal(
			ISymbol symbol,
			DefaultParamCompilationData compilation,
			string? targetNamespace,
			int numTypeParameters,
			int numNonDefaultParam,
			int numParameters
		)
		{
			INamedTypeSymbol? containingType = symbol.ContainingType;

			if (containingType is null)
			{
				return GetCollidingNotNestedTypes(symbol, compilation, targetNamespace, numTypeParameters, numNonDefaultParam);
			}

			string name = symbol.Name;
			string fullName = symbol.ToString();
			INamedTypeSymbol generatedFrom = compilation.DurianGeneratedAttribute!;
			int numDefaultParam = numTypeParameters - numNonDefaultParam;

			IEnumerable<ISymbol> symbols = containingType.GetMembers(name);

			if (containingType.TypeKind == TypeKind.Interface)
			{
				symbols = symbols
					.Concat(containingType.AllInterfaces
						.SelectMany(t => t.GetMembers(name)));
			}
			else
			{
				symbols = symbols
					.Concat(containingType.GetBaseTypes()
						.SelectMany(t => t.GetMembers(name))
						.Where(s => s.DeclaredAccessibility > Accessibility.Private));
			}

			bool includeNonGenericCompatibleMembers = numDefaultParam == numTypeParameters;

			symbols = symbols.Where(s =>
			{
				if (s is IMethodSymbol m)
				{
					ImmutableArray<ITypeParameterSymbol> typeParameters = m.TypeParameters;
					return typeParameters.Length >= numNonDefaultParam && typeParameters.Length < numTypeParameters;
				}
				else if (s is INamedTypeSymbol t)
				{
					ImmutableArray<ITypeParameterSymbol> typeParameters = t.TypeParameters;
					return typeParameters.Length >= numNonDefaultParam && typeParameters.Length < numTypeParameters;
				}

				return includeNonGenericCompatibleMembers;
			});

			if (symbol is IMethodSymbol m)
			{
				return GetCollidingMembersForMethodSymbol(m, fullName, symbols, generatedFrom, numParameters);
			}

			if (symbol is INamedTypeSymbol type)
			{
				return symbols.Where(s =>
				{
					if (s is INamedTypeSymbol t && t.TypeKind == type.TypeKind)
					{
						return !IsGeneratedFrom(s, fullName, generatedFrom);
					}

					return true;
				});
			}

			return symbols.Where(s => !IsGeneratedFrom(s, fullName, generatedFrom));
		}

		private static bool HasPartialKeyword(ITypeData data)
		{
			return data.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword));
		}

		private static bool HasPartialKeyword(INamedTypeSymbol symbol, CancellationToken cancellationToken)
		{
			return symbol.GetModifiers(cancellationToken).Any(m => m.IsKind(SyntaxKind.PartialKeyword));
		}

		private static bool HasTypeParameterAsConstraint(ITypeParameterSymbol currentTypeParameter, in TypeParameterContainer typeParameters)
		{
			int length = typeParameters.Length;

			for (int i = 0; i < length; i++)
			{
				ref readonly TypeParameterData param = ref typeParameters[i];

				foreach (ITypeSymbol constraint in param.Symbol.ConstraintTypes)
				{
					if (SymbolEqualityComparer.Default.Equals(constraint, currentTypeParameter))
					{
						return true;
					}
				}
			}

			return false;
		}

		private static bool HasValidParameterAccessibility(
			ISymbol symbol,
			ITypeSymbol targetType,
			ITypeParameterSymbol currentTypeParameter,
			in TypeParameterContainer typeParameters
		)
		{
			if (targetType.GetEffectiveAccessibility() < symbol.GetEffectiveAccessibility())
			{
				if (symbol is IMethodSymbol m)
				{
					if (IsInvalidMethod(m))
					{
						return false;
					}
				}
				else if (symbol is INamedTypeSymbol t)
				{
					if (t.TypeKind == TypeKind.Delegate && IsInvalidMethod(t.DelegateInvokeMethod))
					{
						return false;
					}
				}
				else
				{
					return true;
				}

				return !HasTypeParameterAsConstraint(currentTypeParameter, in typeParameters);
			}

			return true;

			bool IsInvalidMethod(IMethodSymbol? method)
			{
				return method is null || method.ReturnType.IsOrUsesTypeParameter(currentTypeParameter) || method.Parameters.Any(p => p.Type.IsOrUsesTypeParameter(currentTypeParameter));
			}
		}

		private static bool IsGeneratedFrom(ISymbol symbol, string fullName, INamedTypeSymbol generatedFromAttribute)
		{
			return symbol.GetAttributes().Any(attr =>
			{
				if (SymbolEqualityComparer.Default.Equals(attr.AttributeClass, generatedFromAttribute) && attr.TryGetConstructorArgumentValue(0, out string? value))
				{
					return value == fullName;
				}

				return false;
			});
		}

		private static bool IsValidForConstraint(ITypeSymbol type, ITypeParameterSymbol parameter, in TypeParameterContainer typeParameters)
		{
			if (parameter.HasReferenceTypeConstraint)
			{
				if (!type.IsReferenceType)
				{
					return false;
				}
			}
			else if (parameter.HasUnmanagedTypeConstraint)
			{
				if (!type.IsUnmanagedType)
				{
					return false;
				}
			}
			else if (parameter.HasValueTypeConstraint)
			{
				if (!type.IsValueType)
				{
					return false;
				}
			}

			if (parameter.HasConstructorConstraint)
			{
				if (type is INamedTypeSymbol n)
				{
					if (!n.InstanceConstructors.Any(ctor => ctor.Parameters.Length == 0 && ctor.DeclaredAccessibility == Accessibility.Public))
					{
						return false;
					}
				}
				else if (type is not IDynamicTypeSymbol)
				{
					return false;
				}
			}

			foreach (ITypeSymbol constraint in parameter.ConstraintTypes)
			{
				if (constraint is ITypeParameterSymbol p)
				{
					ref readonly TypeParameterData data = ref typeParameters[p.Ordinal];

					if (data.IsValidDefaultParam)
					{
						if (!type.InheritsFrom(data.TargetType))
						{
							return false;
						}
					}
					else if (!IsValidForConstraint(type, p, in typeParameters))
					{
						return false;
					}
				}
				else if (!type.InheritsFrom(constraint))
				{
					return false;
				}
			}

			return true;
		}

		private static bool IsValidParameterInCollidingMember(ITypeParameterSymbol[] typeParameters, IParameterSymbol parameter, in ParameterGeneration targetGeneration)
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

		private static bool TryGetTypeParameters(
			ISymbol symbol,
			DefaultParamCompilationData compilation,
			CancellationToken cancellationToken,
			out TypeParameterContainer typeParameters
		)
		{
			if (symbol is IMethodSymbol m)
			{
				typeParameters = TypeParameterContainer.CreateFrom(m, compilation, cancellationToken);
				return true;
			}
			else if (symbol is INamedTypeSymbol t)
			{
				typeParameters = TypeParameterContainer.CreateFrom(t, compilation, cancellationToken);
				return true;
			}

			typeParameters = default;
			return false;
		}

		private static bool ValidateTargetTypeParameter(
			ISymbol symbol,
			in TypeParameterData currentTypeParameter,
			in TypeParameterContainer typeParameters
		)
		{
			ITypeSymbol? targetType = currentTypeParameter.TargetType;
			ITypeParameterSymbol typeParameterSymbol = currentTypeParameter.Symbol;

			if (targetType is null || targetType is IErrorTypeSymbol)
			{
				return false;
			}

			if (targetType.IsStatic ||
				targetType.IsRefLikeType ||
				targetType is IFunctionPointerTypeSymbol ||
				targetType is IPointerTypeSymbol ||
				(targetType is INamedTypeSymbol t && (t.IsUnboundGenericType || t.SpecialType == SpecialType.System_Void))
			)
			{
				return false;
			}

			if (!HasValidParameterAccessibility(symbol, targetType, typeParameterSymbol, in typeParameters))
			{
				return false;
			}

			if (targetType.SpecialType == SpecialType.System_Object ||
				targetType.SpecialType == SpecialType.System_Array ||
				targetType.SpecialType == SpecialType.System_ValueType ||
				targetType is IArrayTypeSymbol ||
				targetType.IsValueType ||
				targetType.IsSealed
			)
			{
				if (HasTypeParameterAsConstraint(typeParameterSymbol, in typeParameters))
				{
					return false;
				}
			}

			return IsValidForConstraint(targetType, currentTypeParameter.Symbol, in typeParameters);
		}

		private void AnalyzeSymbol(SymbolAnalysisContext context, DefaultParamCompilationData compilation)
		{
			if (!ShouldAnalyze(context.Symbol, compilation))
			{
				return;
			}

			DiagnosticReceiver.Contextual<SymbolAnalysisContext> diagnosticReceiver = DiagnosticReceiver.Factory.Symbol(context);
			Analyze(context.Symbol, compilation, diagnosticReceiver, context.CancellationToken);
		}
	}
}