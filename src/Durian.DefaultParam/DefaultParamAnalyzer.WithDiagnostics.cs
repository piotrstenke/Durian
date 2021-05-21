using System.CodeDom.Compiler;
using System.Linq;
using System.Threading;
using Durian.Generator.Data;
using Durian.Generator.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Immutable;
using System.Reflection.Metadata;
using System.Data.Common;

namespace Durian.Generator.DefaultParam
{
	public abstract partial class DefaultParamAnalyzer
	{
		/// <summary>
		/// Contains <see langword="static"/> methods that perform the most basic DefaultParam-related analysis and report <see cref="Diagnostic"/>s if the Analyzes <see cref="ISymbol"/> is not valid.
		/// </summary>
		public static class WithDiagnostics
		{
			/// <summary>
			/// Performs basic analysis of the <paramref name="symbol"/> and reports <see cref="Diagnostic"/>s if the <paramref name="symbol"/> is not valid.
			/// </summary>
			/// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> that is used to report <see cref="Diagnostic"/>s.</param>
			/// <param name="symbol"><see cref="ISymbol"/> to analyze.</param>
			/// <param name="compilation">Current <see cref="DefaultParamCompilationData"/>.</param>
			/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
			/// <returns><see langword="true"/> if the <paramref name="symbol"/> is valid, otherwise <see langword="false"/>.</returns>
			public static bool DefaultAnalyze(IDiagnosticReceiver diagnosticReceiver, ISymbol symbol, DefaultParamCompilationData compilation, CancellationToken cancellationToken = default)
			{
				if (!TryGetTypeParameters(symbol, compilation, cancellationToken, out TypeParameterContainer typeParameters))
				{
					return false;
				}

				return DefaultAnalyze(diagnosticReceiver, symbol, compilation, in typeParameters, cancellationToken);
			}

			/// <summary>
			/// Performs basic analysis of the <paramref name="symbol"/> and reports <see cref="Diagnostic"/>s if the <paramref name="symbol"/> is not valid.
			/// </summary>
			/// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> that is used to report <see cref="Diagnostic"/>s.</param>
			/// <param name="symbol"><see cref="ISymbol"/> to analyze.</param>
			/// <param name="compilation">Current <see cref="DefaultParamCompilationData"/>.</param>
			/// <param name="typeParameters"><see cref="TypeParameterContainer"/> that contains the <paramref name="symbol"/>'s type parameters.</param>
			/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
			/// <returns><see langword="true"/> if the <paramref name="symbol"/> is valid, otherwise <see langword="false"/>.</returns>
			public static bool DefaultAnalyze(IDiagnosticReceiver diagnosticReceiver, ISymbol symbol, DefaultParamCompilationData compilation, in TypeParameterContainer typeParameters, CancellationToken cancellationToken = default)
			{
				if (!typeParameters.HasDefaultParams)
				{
					return false;
				}

				bool isValid = AnalyzeAgaintsProhibitedAttributes(diagnosticReceiver, symbol, compilation);
				isValid &= AnalyzeContainingTypes(diagnosticReceiver, symbol, cancellationToken);
				isValid &= AnalyzeTypeParameters(diagnosticReceiver, symbol, in typeParameters);

				return isValid;
			}

			/// <summary>
			/// Analyzes, if the <paramref name="symbol"/> has <see cref="DurianGeneratedAttribute"/> or <see cref="GeneratedCodeAttribute"/> and reports <see cref="Diagnostic"/>s if the <paramref name="symbol"/> is not valid.
			/// </summary>
			/// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> that is used to report <see cref="Diagnostic"/>s.</param>
			/// <param name="symbol"><see cref="ISymbol"/> to analyze.</param>
			/// <param name="compilation">Current <see cref="DefaultParamCompilationData"/>.</param>
			/// <returns><see langword="true"/> if the <paramref name="symbol"/> is valid (does not have the prohibited attributes), otherwise <see langword="false"/>.</returns>
			public static bool AnalyzeAgaintsProhibitedAttributes(IDiagnosticReceiver diagnosticReceiver, ISymbol symbol, DefaultParamCompilationData compilation)
			{
				return AnalyzeAgaintsProhibitedAttributes(diagnosticReceiver, symbol, compilation, out _);
			}

			/// <summary>
			/// Analyzes, if the <paramref name="symbol"/> has <see cref="DurianGeneratedAttribute"/> or <see cref="GeneratedCodeAttribute"/> and reports <see cref="Diagnostic"/>s if the <paramref name="symbol"/> is not valid. If the <paramref name="symbol"/> is valid, returns an array of <paramref name="attributes"/> of that <paramref name="symbol"/>.
			/// </summary>
			/// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> that is used to report <see cref="Diagnostic"/>s.</param>
			/// <param name="symbol"><see cref="ISymbol"/> to analyze.</param>
			/// <param name="compilation">Current <see cref="DefaultParamCompilationData"/>.</param>
			/// <param name="attributes">An array of <see cref="AttributeData"/>s of the <paramref name="symbol"/>. Returned if the method itself returns <see langword="true"/>.</param>
			/// <returns><see langword="true"/> if the <paramref name="symbol"/> is valid (does not have the prohibited attributes), otherwise <see langword="false"/>.</returns>
			public static bool AnalyzeAgaintsProhibitedAttributes(IDiagnosticReceiver diagnosticReceiver, ISymbol symbol, DefaultParamCompilationData compilation, out AttributeData[]? attributes)
			{
				AttributeData[] attrs = symbol.GetAttributes().ToArray();
				bool isValid = true;

				foreach (AttributeData attr in attrs)
				{
					if (SymbolEqualityComparer.Default.Equals(attr.AttributeClass, compilation.GeneratedCodeAttribute) ||
						SymbolEqualityComparer.Default.Equals(attr.AttributeClass, compilation.DurianGeneratedAttribute))
					{
						diagnosticReceiver.ReportDiagnostic(DefaultParamDiagnostics.DUR0104_DefaultParamCannotBeAppliedWhenGenerationAttributesArePresent, symbol);
						isValid = false;
						break;
					}
				}

				attributes = isValid ? attrs : null;
				return isValid;
			}

			/// <summary>
			/// Analyzes, if the <paramref name="symbol"/> and its containing types are <see langword="partial"/> and reports <see cref="Diagnostic"/>s if the <paramref name="symbol"/> is not valid.
			/// </summary>
			/// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> that is used to report <see cref="Diagnostic"/>s.</param>
			/// <param name="symbol"><see cref="ISymbol"/> to analyze.</param>
			/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
			/// <returns><see langword="true"/> if the <paramref name="symbol"/> is valid, otherwise <see langword="false"/>.</returns>
			public static bool AnalyzeContainingTypes(IDiagnosticReceiver diagnosticReceiver, ISymbol symbol, CancellationToken cancellationToken = default)
			{
				INamedTypeSymbol[] types = symbol.GetContainingTypeSymbols().ToArray();
				bool isValid = true;

				if (types.Length > 0)
				{
					foreach (INamedTypeSymbol parent in types)
					{
						if (!HasPartialKeyword(parent, cancellationToken))
						{
							diagnosticReceiver.ReportDiagnostic(DefaultParamDiagnostics.DUR0101_ContainingTypeMustBePartial, parent);
							isValid = false;
						}
					}
				}

				return isValid;
			}

			/// <summary>
			/// Analyzes, if the <paramref name="symbol"/> and its containing types are see <see langword="partial"/> and reports <see cref="Diagnostic"/>s if the <paramref name="symbol"/> is not valid. If the <paramref name="symbol"/> is valid, returns an array of <see cref="ITypeData"/>s of its containing types.
			/// </summary>
			/// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> that is used to report <see cref="Diagnostic"/>s.</param>
			/// <param name="symbol"><see cref="ISymbol"/> to analyze.</param>
			/// <param name="compilation">Current <see cref="DefaultParamCompilationData"/>.</param>
			/// <param name="containingTypes">An array of this <paramref name="symbol"/>'s containing types' <see cref="ITypeData"/>s. Returned if the method itself returns <see langword="true"/>.</param>
			/// <returns><see langword="true"/> if the <paramref name="symbol"/> is valid, otherwise <see langword="false"/>.</returns>
			public static bool AnalyzeContainingTypes(IDiagnosticReceiver diagnosticReceiver, ISymbol symbol, DefaultParamCompilationData compilation, out ITypeData[]? containingTypes)
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
					}
				}

				containingTypes = isValid ? types : null;

				return isValid;
			}

			/// <summary>
			/// Checks, if the specified <paramref name="typeParameters"/> are valid and reports <see cref="Diagnostic"/> if they are not.
			/// </summary>
			/// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> that is used to report <see cref="Diagnostic"/>s.</param>
			/// <param name="symbol"><see cref="ISymbol"/> to analyze the type parameters of.</param>
			/// <param name="typeParameters"><see cref="TypeParameterContainer"/> to analyze.</param>
			/// <returns><see langword="true"/> if the type parameters contained within the <see cref="TypeParameterContainer"/> are valid, otherwise <see langword="false"/>.</returns>
			public static bool AnalyzeTypeParameters(IDiagnosticReceiver diagnosticReceiver, ISymbol symbol, in TypeParameterContainer typeParameters)
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
						if(!ValidateTargetTypeParameter(diagnosticReceiver, symbol, in data, in typeParameters))
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

			private static bool ValidateTargetTypeParameter(IDiagnosticReceiver diagnosticReceiver, ISymbol symbol, in TypeParameterData currentTypeParameter, in TypeParameterContainer typeParameters)
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
					diagnosticReceiver.ReportDiagnostic(DefaultParamDiagnostics.DUR0125_TypeIsNotValidDefaultParamValue, currentTypeParameter.Location, typeParameterSymbol, targetType);
					return false;
				}

				if(!HasValidParameterAccessibility(symbol, targetType, typeParameterSymbol, in typeParameters))
				{
					diagnosticReceiver.ReportDiagnostic(DefaultParamDiagnostics.DUR0123_DefaultParamValueCannotBeLessAccessibleThanTargetMember, currentTypeParameter.Location, typeParameterSymbol, targetType);
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
					if(HasTypeParameterAsConstraint(typeParameterSymbol, in typeParameters))
					{
						diagnosticReceiver.ReportDiagnostic(DefaultParamDiagnostics.DUR0124_TypeCannotBeUsedWithConstraint, currentTypeParameter.Location, typeParameters, targetType);
						return false;
					}
				}

				if(!IsValidForConstraint(targetType, currentTypeParameter.Symbol, in typeParameters))
				{
					diagnosticReceiver.ReportDiagnostic(DefaultParamDiagnostics.DUR0106_TargetTypeDoesNotSatisfyConstraint, currentTypeParameter.Location, typeParameterSymbol, targetType);
					return false;
				}

				return true;
			}
		}
	}
}
