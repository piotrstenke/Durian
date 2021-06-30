// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using Durian.Analysis.Extensions;
using Durian.Configuration;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using static Durian.Analysis.GenericSpecialization.GenSpecDiagnostics;

#if !MAIN_PACKAGE

#endif

namespace Durian.Analysis.GenericSpecialization
{
	public sealed partial class AllowSpecializationAnalyzer
	{
		/// <summary>
		/// Contains <see langword="static"/> methods that perform the most basic GenSpec-related analysis and report <see cref="Diagnostic"/>s if the analyzed <see cref="INamedTypeSymbol"/> is not valid.
		/// </summary>
		public static class WithDiagnostics
		{
			/// <summary>
			/// Fully analyzes the specified <paramref name="symbol"/>.
			/// </summary>
			/// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> that is used to report <see cref="Diagnostic"/>s.</param>
			/// <param name="symbol"><see cref="INamedTypeSymbol"/> to analyzer.</param>
			/// <param name="compilation">Current <see cref="GenSpecCompilationData"/>.</param>
			/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
			public static bool Analyze(IDiagnosticReceiver diagnosticReceiver, INamedTypeSymbol symbol, GenSpecCompilationData compilation, CancellationToken cancellationToken = default)
			{
				if (!ShouldAnalyze(symbol, compilation, out AttributeData[]? attributes))
				{
					return false;
				}

				INamedTypeSymbol[] containingTypes = symbol.GetContainingTypeSymbols().ToArray();

				bool isValid = AnalyzeDeclaration(diagnosticReceiver, symbol, cancellationToken);
				isValid &= AnalyzeContainingTypes(diagnosticReceiver, symbol, containingTypes, cancellationToken);
				isValid &= AnalyzeMembers(diagnosticReceiver, symbol, compilation, attributes, containingTypes);

				return isValid;
			}

			/// <summary>
			/// Determines whether the <paramref name="containingTypes"/> of the <paramref name="symbol"/> are valid.
			/// </summary>
			/// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> that is used to report <see cref="Diagnostic"/>s.</param>
			/// <param name="symbol"><see cref="INamedTypeSymbol"/> the <paramref name="containingTypes"/> are associated with.</param>
			/// <param name="containingTypes">An array of <see cref="INamedTypeSymbol"/>s to analyze.</param>
			/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
			public static bool AnalyzeContainingTypes(
				IDiagnosticReceiver diagnosticReceiver,
				INamedTypeSymbol symbol,
				INamedTypeSymbol[]? containingTypes = null,
				CancellationToken cancellationToken = default
			)
			{
				InitializeContainingTypes(ref containingTypes, symbol);

				bool isValid = true;

				if (containingTypes.Length > 0)
				{
					foreach (INamedTypeSymbol type in containingTypes)
					{
						if (!HasPartialKeyword(type, cancellationToken))
						{
							diagnosticReceiver.ReportDiagnostic(DUR0210_ContainingTypesMustBePartial, type);
							isValid = false;
						}
					}
				}

				return isValid;
			}

			/// <summary>
			/// Determines whether the <paramref name="containingTypes"/> of the <paramref name="symbol"/> are valid.
			/// </summary>
			/// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> that is used to report <see cref="Diagnostic"/>s.</param>
			/// <param name="symbol"><see cref="INamedTypeSymbol"/> the <paramref name="containingTypes"/> are associated with.</param>
			/// <param name="containingTypes">An array of <see cref="INamedTypeSymbol"/>s representing the types the <paramref name="symbol"/> is contained within. Included if the method returns <see langword="true"/>.</param>
			/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
			public static bool AnalyzeContainingTypes(
				IDiagnosticReceiver diagnosticReceiver,
				INamedTypeSymbol symbol,
				[NotNullWhen(true)] out INamedTypeSymbol[]? containingTypes,
				CancellationToken cancellationToken = default
			)
			{
				INamedTypeSymbol[] symbols = symbol.GetContainingTypeSymbols().ToArray();
				bool isValid = AnalyzeContainingTypes(diagnosticReceiver, symbol, symbols, cancellationToken);
				containingTypes = isValid ? symbols : null;
				return isValid;
			}

			/// <summary>
			/// Determines whether declaration of the specified <paramref name="symbol"/> is valid.
			/// </summary>
			/// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> that is used to report <see cref="Diagnostic"/>s.</param>
			/// <param name="symbol"><see cref="INamedTypeSymbol"/> to analyze the declaration of.</param>
			/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
			public static bool AnalyzeDeclaration(
				IDiagnosticReceiver diagnosticReceiver,
				INamedTypeSymbol symbol,
				CancellationToken cancellationToken = default
			)
			{
				bool isValid = true;

				if (symbol.TypeParameters.Length == 0)
				{
					diagnosticReceiver.ReportDiagnostic(DUR0201_NonGenericTypesCannotUseTheAllowSpecializationAttribute, symbol);
					isValid = false;
				}

				if (symbol.BaseType is not null || symbol.Interfaces.Length != 0)
				{
					diagnosticReceiver.ReportDiagnostic(DUR0218_DoNotSpecifyBaseTypesOrInterfacesForTargetClass, symbol);
					isValid = false;
				}

				isValid &= AnalyzeModifiers(diagnosticReceiver, symbol, cancellationToken);

				return isValid;
			}

			/// <summary>
			/// Determines whether all the <paramref name="symbol"/>'s members are valid.
			/// </summary>
			/// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> that is used to report <see cref="Diagnostic"/>s.</param>
			/// <param name="symbol"><see cref="INamedTypeSymbol"/> to analyze the members of.</param>
			/// <param name="compilation">Current <see cref="GenSpecCompilationData"/>.</param>
			/// <param name="attributes">A collection of <see cref="AttributeData"/>s associated with the specified <paramref name="symbol"/>.</param>
			/// <param name="containingTypes">An array of <see cref="INamedTypeSymbol"/>s representing the containing types of the <paramref name="symbol"/> in root-first order.</param>
			public static bool AnalyzeMembers(
				IDiagnosticReceiver diagnosticReceiver,
				INamedTypeSymbol symbol,
				GenSpecCompilationData compilation,
				IEnumerable<AttributeData>? attributes = null,
				INamedTypeSymbol[]? containingTypes = null
			)
			{
				if (TryGetConfiguration(diagnosticReceiver, symbol, compilation, out GenSpecConfiguration? configuration, attributes, containingTypes))
				{
					return AnalyzeMembers(diagnosticReceiver, symbol, compilation, configuration);
				}

				return false;
			}

			/// <summary>
			/// Determines whether all the <paramref name="symbol"/>'s members are valid.
			/// </summary>
			/// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> that is used to report <see cref="Diagnostic"/>s.</param>
			/// <param name="symbol"><see cref="INamedTypeSymbol"/> to analyze the members of.</param>
			/// <param name="compilation">Current <see cref="GenSpecCompilationData"/>.</param>
			/// <param name="configuration"><see cref="GenSpecConfiguration"/> applied to the target <paramref name="symbol"/>.</param>
			public static bool AnalyzeMembers(
				IDiagnosticReceiver diagnosticReceiver,
				INamedTypeSymbol symbol,
				GenSpecCompilationData compilation,
				GenSpecConfiguration configuration
			)
			{
				ISymbol[] members = symbol.GetMembers()
					.Where(m => !m.IsImplicitlyDeclared)
					.ToArray();

				if (members.Length == 0)
				{
					diagnosticReceiver.ReportDiagnostic(DUR0208_ProvideDefaultImplementation, symbol);
					return false;
				}

				bool isValid = false;
				string interfaceFullName = AnalysisUtilities.CreateName(symbol.JoinNamespaces(), $"{symbol.Name}`{symbol.Arity}", configuration.InterfaceName);

				if (members.Length > 1)
				{
					foreach (ISymbol member in members)
					{
						if (MemberIsValidInTargetClass(member, symbol, compilation, configuration, interfaceFullName))
						{
							continue;
						}

						diagnosticReceiver.ReportDiagnostic(DUR0217_DoNotAddMembersToTargetClass, member);
						isValid = false;
					}
				}

				if (Array.Find(members, m => m.Name == configuration.TemplateName) is not INamedTypeSymbol type)
				{
					diagnosticReceiver.ReportDiagnostic(DUR0208_ProvideDefaultImplementation, symbol);
					return false;
				}

				isValid &= AnalyzeDefaultImplementation(diagnosticReceiver, symbol, type, configuration, interfaceFullName);

				return isValid;
			}

			/// <summary>
			/// Tries to create a new instance of <see cref="GenSpecConfiguration"/> based on <see cref="GenericSpecializationConfigurationAttribute"/>s applied to the specified <paramref name="symbol"/> or its <paramref name="containingTypes"/>.
			/// </summary>
			/// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> that is used to report <see cref="Diagnostic"/>s.</param>
			/// <param name="symbol"><see cref="INamedTypeSymbol"/> to get the <see cref="GenSpecConfiguration"/> of.</param>
			/// <param name="compilation">Current <see cref="GenSpecCompilationData"/>.</param>
			/// <param name="configuration">A new instance of <see cref="GenSpecConfiguration"/> created for the specified <paramref name="symbol"/>.</param>
			/// <param name="attributes">A collection of <see cref="AttributeData"/>s associated with the specified <paramref name="symbol"/>.</param>
			/// <param name="containingTypes">An array of <see cref="INamedTypeSymbol"/>s representing the containing types of the <paramref name="symbol"/> in root-first order.</param>
			public static bool TryGetConfiguration(
				IDiagnosticReceiver diagnosticReceiver,
				INamedTypeSymbol symbol,
				GenSpecCompilationData compilation,
				[NotNullWhen(true)] out GenSpecConfiguration? configuration,
				IEnumerable<AttributeData>? attributes = null,
				INamedTypeSymbol[]? containingTypes = null
			)
			{
				InitializeAttributes(ref attributes, symbol);
				InitializeContainingTypes(ref containingTypes, symbol);

				GenSpecConfiguration c = compilation.Configuration.Clone();

				string symbolName = symbol.Name;

				if (TrySetAllConfigurationPropertiesLocally(compilation, c, symbolName, attributes, out bool[] includedProperties))
				{
					configuration = c;
					return true;
				}

				foreach (INamedTypeSymbol type in containingTypes.Reverse())
				{
					foreach (AttributeData attribute in type.GetAttributes())
					{
						if (!SymbolEqualityComparer.Default.Equals(compilation.ConfigurationAttribute))
						{
							continue;
						}

						foreach (KeyValuePair<string, TypedConstant> pair in attribute.NamedArguments)
						{
							if (!SetConfigurationValueScoped(diagnosticReceiver, pair.Key, pair.Value.Value, symbol, c, includedProperties))
							{
								configuration = null;
								return false;
							}
						}

						break;
					}

					if (HasAllProperties(includedProperties))
					{
						configuration = c;
						return true;
					}
				}

				if (c.InterfaceName == symbolName || c.TemplateName == symbolName)
				{
					diagnosticReceiver.ReportDiagnostic(DUR0222_TargetNameCannotBeTheSameAsContainingClass, symbol);
					configuration = null;
					return false;
				}

				configuration = c;
				return true;
			}

			private static bool AnalyzeDefaultImplementation(
				IDiagnosticReceiver diagnosticReceiver,
				INamedTypeSymbol targetClass,
				INamedTypeSymbol defaultImpl,
				GenSpecConfiguration configuration,
				string interfaceFullName
			)
			{
				bool isValid = true;

				if (defaultImpl.IsAbstract || defaultImpl.IsStatic)
				{
					diagnosticReceiver.ReportDiagnostic(DUR0220_DefaultImplementationCannotBeAbstractOrStatic, defaultImpl);
					isValid = false;
				}

				if (defaultImpl.IsGenericType)
				{
					diagnosticReceiver.ReportDiagnostic(DUR0221_DefaultImplementationCannotBeGeneric, defaultImpl);
					isValid = false;
				}

				if (defaultImpl.TypeKind != TypeKind.Class || defaultImpl.IsRecord)
				{
					diagnosticReceiver.ReportDiagnostic(DUR0223_DefaultSpecializationMustBeClass, defaultImpl);
					isValid = false;
				}

				if (defaultImpl.IsSealed && configuration.ForceInherit)
				{
					diagnosticReceiver.ReportDiagnostic(DUR0219_CannotForceInheritSealedClass, defaultImpl);
					isValid = false;
				}

				if (defaultImpl.BaseType is not null && SymbolEqualityComparer.Default.Equals(defaultImpl.BaseType, targetClass))
				{
					diagnosticReceiver.ReportDiagnostic(DUR0224_SpecializationCannotInheritTypeItIsSpecializing, defaultImpl);
					isValid = false;
				}

				if (targetClass.Arity > 0 && !HasSpecInterface(defaultImpl, configuration.InterfaceName, interfaceFullName))
				{
					diagnosticReceiver.ReportDiagnostic(DUR0206_SpecializationMustImplementInterface, defaultImpl, interfaceFullName);
					isValid = false;
				}

				return isValid;
			}

			private static bool AnalyzeModifiers(
				IDiagnosticReceiver diagnosticReceiver,
				INamedTypeSymbol symbol,
				CancellationToken cancellationToken
			)
			{
				bool hasPartial = false;
				bool hasValidModifiers = true;

				foreach (SyntaxToken modifier in symbol.GetModifiers(cancellationToken))
				{
					if (modifier.IsKind(SyntaxKind.StaticKeyword) || modifier.IsKind(SyntaxKind.AbstractKeyword))
					{
						diagnosticReceiver.ReportDiagnostic(DUR0211_TargetGenericClassCannotBeAbstractOrStatic, symbol);
						hasValidModifiers = false;
					}
					else if (modifier.IsKind(SyntaxKind.PartialKeyword))
					{
						hasPartial = true;
					}
				}

				if (!hasPartial)
				{
					diagnosticReceiver.ReportDiagnostic(DUR0209_TargetGenericClassMustBePartial, symbol);
				}

				return hasPartial && hasValidModifiers;
			}

			private static bool SetConfigurationValueScoped(
				IDiagnosticReceiver diagnosticReceiver,
				string name,
				object? value,
				INamedTypeSymbol symbol,
				GenSpecConfiguration config,
				bool[] includedProperties
			)
			{
				if (name == _templateName)
				{
					if (includedProperties[0])
					{
						return true;
					}

					if (value is string str && AnalysisUtilities.IsValidIdentifier(str))
					{
						if (str == symbol.Name)
						{
							diagnosticReceiver.ReportDiagnostic(DUR0222_TargetNameCannotBeTheSameAsContainingClass, symbol);
							return false;
						}

						config.TemplateName = str;
					}

					includedProperties[0] = true;
				}
				else if (name == _interfaceName)
				{
					if (includedProperties[1])
					{
						return true;
					}

					if (value is string str && AnalysisUtilities.IsValidIdentifier(str))
					{
						if (str == symbol.Name)
						{
							diagnosticReceiver.ReportDiagnostic(DUR0222_TargetNameCannotBeTheSameAsContainingClass, symbol);
							return false;
						}

						config.InterfaceName = str;
					}

					includedProperties[1] = true;
				}
				else
				{
					SetForceInheritOrImportOptions(name, value, config, includedProperties);
				}

				return true;
			}
		}
	}
}
