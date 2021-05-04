using System.Linq;
using System.Threading;
using Durian.Data;
using Durian.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Durian.DefaultParam
{
	public abstract partial class DefaultParamAnalyzer
	{
		public static class WithDiagnostics
		{
			public static bool DefaultAnalyze(IDiagnosticReceiver diagnosticReceiver, ISymbol symbol, DefaultParamCompilationData compilation, CancellationToken cancellationToken = default)
			{
				if (!TryGetTypeParameters(symbol, compilation, cancellationToken, out TypeParameterContainer typeParameters))
				{
					return false;
				}

				return DefaultAnalyze(diagnosticReceiver, symbol, compilation, in typeParameters, cancellationToken);
			}

			public static bool DefaultAnalyze(IDiagnosticReceiver diagnosticReceiver, ISymbol symbol, DefaultParamCompilationData compilation, in TypeParameterContainer typeParameters, CancellationToken cancellationToken = default)
			{
				if (!typeParameters.HasDefaultParams)
				{
					return false;
				}

				bool isValid = AnalyzeAgaintsProhibitedAttributes(diagnosticReceiver, symbol, compilation);
				isValid &= AnalyzeContainingTypes(diagnosticReceiver, symbol, cancellationToken);
				isValid &= AnalyzeTypeParameters(diagnosticReceiver, in typeParameters);

				return isValid;
			}

			public static bool AnalyzeAgaintsProhibitedAttributes(IDiagnosticReceiver diagnosticReceiver, ISymbol symbol, DefaultParamCompilationData compialation)
			{
				return AnalyzeAgaintsProhibitedAttributes(diagnosticReceiver, symbol, compialation, out _);
			}

			public static bool AnalyzeAgaintsProhibitedAttributes(IDiagnosticReceiver diagnosticReceiver, ISymbol symbol, DefaultParamCompilationData compilation, out AttributeData[]? attributes)
			{
				AttributeData[] attrs = symbol.GetAttributes().ToArray();
				(INamedTypeSymbol type, string name)[] prohibitedAttributes = GetProhibitedAttributes(compilation);
				bool isValid = true;

				foreach (AttributeData attr in attrs)
				{
					foreach ((INamedTypeSymbol type, string name) in prohibitedAttributes)
					{
						if (SymbolEqualityComparer.Default.Equals(type, attr.AttributeClass))
						{
							DefaultParamDiagnostics.DefaultParamAttributeCannotBeAppliedToMembersWithAttribute(diagnosticReceiver, symbol, name);
							isValid = false;
							break;
						}
					}
				}

				attributes = isValid ? attrs : null;
				return isValid;
			}

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
							DefaultParamDiagnostics.ParentTypeOfMemberWithDefaultParamAttributeMustBePartial(diagnosticReceiver, parent);
							isValid = false;
						}
					}
				}

				return isValid;
			}

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
							DefaultParamDiagnostics.ParentTypeOfMemberWithDefaultParamAttributeMustBePartial(diagnosticReceiver, parent.Symbol);
							isValid = false;
						}
					}
				}

				containingTypes = isValid ? types : null;

				return isValid;
			}

			public static bool AnalyzeTypeParameters(IDiagnosticReceiver diagnosticReceiver, in TypeParameterContainer typeParameters)
			{
				if (!typeParameters.HasDefaultParams)
				{
					return false;
				}

				int length = typeParameters.Length;
				bool isValid = true;
				int lastDefaultParam = typeParameters.FirstDefaultParamIndex;

				for (int i = lastDefaultParam; i < length; i++)
				{
					ref readonly TypeParameterData data = ref typeParameters[i];

					if (data.IsDefaultParam)
					{
						if (data.TargetType is null)
						{
							isValid = false;
						}
						else if (!data.TargetType.IsValidForTypeParameter(data.Symbol))
						{
							DurianDiagnostics.TypeIsNotValidTypeParameter(diagnosticReceiver, data.TargetType, data.Symbol, data.Location);
							isValid = false;
						}

						lastDefaultParam = i;
					}
					else if (lastDefaultParam != -1)
					{
						ref readonly TypeParameterData errorData = ref typeParameters[lastDefaultParam];
						DefaultParamDiagnostics.TypeParameterWithDefaultParamAttributeMustBeLast(diagnosticReceiver, errorData.Symbol, errorData.Location);
						isValid = false;
						lastDefaultParam = -1;
					}
				}

				return isValid;
			}
		}
	}
}
