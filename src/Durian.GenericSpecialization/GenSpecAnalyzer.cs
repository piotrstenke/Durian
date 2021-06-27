// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using static Durian.Analysis.GenericSpecialization.GenSpecDiagnostics;
using Durian.Analysis.Extensions;
using System.Linq;
using System.Collections.Generic;
using Durian.Configuration;
using System;
using System.Runtime.CompilerServices;

#if !MAIN_PACKAGE

using Microsoft.CodeAnalysis.Diagnostics;

#endif

namespace Durian.Analysis.GenericSpecialization
{
	/// <summary>
	/// Analyzes classes marked by the <see cref="AllowSpecializationAttribute"/>.
	/// </summary>
#if !MAIN_PACKAGE

	[DiagnosticAnalyzer(LanguageNames.CSharp)]
#endif
#pragma warning disable RS1001 // Missing diagnostic analyzer attribute.

	public sealed partial class AllowSpecializationAnalyzer : DurianAnalyzer<GenSpecCompilationData>
#pragma warning restore RS1001 // Missing diagnostic analyzer attribute.
	{
		private const string _forceInherit = nameof(GenericSpecializationConfigurationAttribute.ForceInherit);
		private const string _importOptions = nameof(GenericSpecializationConfigurationAttribute.ImportOptions);
		private const string _interfaceName = nameof(GenericSpecializationConfigurationAttribute.InterfaceName);
		private const string _templateName = nameof(GenericSpecializationConfigurationAttribute.TemplateName);

		/// <inheritdoc/>
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(
			DUR0201_NonGenericTypesCannotUseTheAllowSpecializationAttribute,
			DUR0206_SpecializationMustImplementInterface,
			DUR0208_ProvideDefaultImplementation,
			DUR0209_TargetGenericClassMustBePartial,
			DUR0210_ContainingTypesMustBePartial,
			DUR0211_TargetGenericClassCannotBeAbstractOrStatic,
			DUR0217_DoNotAddMembersToTargetClass,
			DUR0218_DoNotSpecifyBaseTypesOrInterfacesForTargetClass,
			DUR0219_DoNotSpecifyAttributesForTargetClass,
			DUR0220_CannotForceInheritSealedClass,
			DUR0221_DefaultImplementationCannotBeAbstractOrStatic,
			DUR0222_TargetNameCannotBeTheSameAsContainingClass,
			DUR0223_DefaultImplementationCannotBeGeneric,
			DUR0224_SpecializationCannotInheritTypeItIsSpecializing
		);

		/// <summary>
		/// Initializes a new instance of the <see cref="AllowSpecializationAnalyzer"/> class.
		/// </summary>
		public AllowSpecializationAnalyzer()
		{
		}

		/// <summary>
		/// Determines whether an <see cref="AllowSpecializationAttribute"/> is contained withing the specified collection of <paramref name="attributes"/>.
		/// </summary>
		/// <param name="attributes">A collection of <see cref="AttributeData"/> to analyze.</param>
		/// <param name="compilation">Current <see cref="GenSpecCompilationData"/>.</param>
		public static bool HasAllowSpecializationAttribute(IEnumerable<AttributeData> attributes, GenSpecCompilationData compilation)
		{
			foreach (AttributeData attr in attributes)
			{
				if (SymbolEqualityComparer.Default.Equals(attr.AttributeClass, compilation.AllowSpecializationAttribute))
				{
					return true;
				}
			}

			return false;
		}

		/// <inheritdoc cref="HasAllowSpecializationAttribute(INamedTypeSymbol, GenSpecCompilationData, out AttributeData[])"/>
		public static bool HasAllowSpecializationAttribute(INamedTypeSymbol symbol, GenSpecCompilationData compilation)
		{
			IEnumerable<AttributeData> attributes = symbol.GetAttributes();
			return HasAllowSpecializationAttribute(attributes, compilation);
		}

		/// <summary>
		/// Determines whether the specified <paramref name="symbol"/> has an <see cref="AllowSpecializationAttribute"/> applied.
		/// </summary>
		/// <param name="symbol"><see cref="INamedTypeSymbol"/> to check if has an <see cref="AllowSpecializationAttribute"/> applied.</param>
		/// <param name="compilation">Current <see cref="GenSpecCompilationData"/>.</param>
		/// <param name="attributes">An array of <see cref="AttributeData"/>s of the specified <paramref name="symbol"/>. Included only if the method returns <see langword="true"/>.</param>
		public static bool HasAllowSpecializationAttribute(INamedTypeSymbol symbol, GenSpecCompilationData compilation, [NotNullWhen(true)] out AttributeData[]? attributes)
		{
			ImmutableArray<AttributeData> attrs = symbol.GetAttributes();
			bool isValid = HasAllowSpecializationAttribute(attrs, compilation);
			attributes = isValid ? attrs.ToArray() : null;
			return isValid;
		}

		/// <summary>
		/// Determines whether the specified <paramref name="symbol"/> is valid for analysis.
		/// </summary>
		/// <param name="symbol"><see cref="INamedTypeSymbol"/> to check if is valid for analysis.</param>
		/// <param name="compilation">Current <see cref="GenSpecCompilationData"/>.</param>
		/// <param name="attributes">A collection of <see cref="AttributeData"/>s associated with the <paramref name="symbol"/>.</param>
		public static bool ShouldAnalyze(INamedTypeSymbol symbol, GenSpecCompilationData compilation, IEnumerable<AttributeData>? attributes = null)
		{
			InitializeAttributes(ref attributes, symbol);

			return symbol.TypeKind == TypeKind.Class && HasAllowSpecializationAttribute(attributes, compilation);
		}

		/// <summary>
		/// Determines whether the specified <paramref name="symbol"/> is valid for analysis.
		/// </summary>
		/// <param name="symbol"><see cref="INamedTypeSymbol"/> to check if is valid for analysis.</param>
		/// <param name="compilation">Current <see cref="GenSpecCompilationData"/>.</param>
		/// <param name="attributes">An array of <see cref="AttributeData"/>s of the specified <paramref name="symbol"/>. Included only if the method returns <see langword="true"/>.</param>
		public static bool ShouldAnalyze(INamedTypeSymbol symbol, GenSpecCompilationData compilation, [NotNullWhen(true)] out AttributeData[]? attributes)
		{
			if (symbol.TypeKind != TypeKind.Class)
			{
				attributes = null;
				return false;
			}

			return HasAllowSpecializationAttribute(symbol, compilation, out attributes);
		}

		/// <inheritdoc/>
		public override void Register(IDurianAnalysisContext context, GenSpecCompilationData compilation)
		{
			context.RegisterSymbolAction(context => AnalyzeSymbol(context, compilation), SymbolKind.NamedType);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static void InitializeAttributes([NotNull] ref IEnumerable<AttributeData>? attributes, INamedTypeSymbol symbol)
		{
			if (attributes is null)
			{
				attributes = symbol.GetAttributes();
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static void InitializeContainingTypes([NotNull] ref INamedTypeSymbol[]? containingTypes, INamedTypeSymbol symbol)
		{
			if (containingTypes is null)
			{
				containingTypes = symbol.GetContainingTypeSymbols().ToArray();
			}
		}

		/// <inheritdoc/>
		protected override GenSpecCompilationData CreateCompilation(CSharpCompilation compilation)
		{
			return new GenSpecCompilationData(compilation);
		}

		private static void AnalyzeSymbol(SymbolAnalysisContext context, GenSpecCompilationData compilation)
		{
			if (context.Symbol is not INamedTypeSymbol t || t.TypeKind != TypeKind.Class)
			{
				return;
			}

			ContextualDiagnosticReceiver<SymbolAnalysisContext> diagnosticReceiver = DiagnosticReceiverFactory.Symbol(context);
			WithDiagnostics.Analyze(diagnosticReceiver, (INamedTypeSymbol)context.Symbol, compilation, context.CancellationToken);
		}

		private static bool HasPartialKeyword(INamedTypeSymbol symbol, CancellationToken cancellationToken)
		{
			return symbol.GetModifiers(cancellationToken).Any(m => m.IsKind(SyntaxKind.PartialKeyword));
		}
	}

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

				if (symbol.TypeParameters.Length == 0)
				{
					diagnosticReceiver.ReportDiagnostic(DUR0201_NonGenericTypesCannotUseTheAllowSpecializationAttribute, symbol);
				}

				INamedTypeSymbol[] containingTypes = symbol.GetContainingTypeSymbols().ToArray();

				bool isValid = AnalyzeModifiers(diagnosticReceiver, symbol, cancellationToken);
				isValid &= AnalyzeContainingTypes(diagnosticReceiver, symbol, containingTypes, cancellationToken);
				isValid &= AnalyzeDeclaration(diagnosticReceiver, symbol, attributes, cancellationToken);
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
			/// <param name="attributes">A collection of <see cref="AttributeData"/>s associated with the specified <paramref name="symbol"/>.</param>
			/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
			public static bool AnalyzeDeclaration(
				IDiagnosticReceiver diagnosticReceiver,
				INamedTypeSymbol symbol,
				IEnumerable<AttributeData>? attributes = null,
				CancellationToken cancellationToken = default
			)
			{
				InitializeAttributes(ref attributes, symbol);

				bool isValid = AnalyzeModifiers(diagnosticReceiver, symbol, cancellationToken);

				if (symbol.BaseType is not null || symbol.Interfaces.Length != 0)
				{
					diagnosticReceiver.ReportDiagnostic(DUR0218_DoNotSpecifyBaseTypesOrInterfacesForTargetClass, symbol);
					isValid = false;
				}

				if (attributes.Any())
				{
					diagnosticReceiver.ReportDiagnostic(DUR0219_DoNotSpecifyAttributesForTargetClass, symbol);
					isValid = false;
				}

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

				if (members.Length > 1)
				{
					foreach (ISymbol member in members)
					{
						if (member.GetAttributeData(compilation.GenericSpecializationAttribute!) is AttributeData attr &&
							attr.TryGetConstructorArgumentValue(0, out INamedTypeSymbol? target) &&
							SymbolEqualityComparer.Default.Equals(symbol, target)
						)
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

				isValid &= AnalyzeDefaultImplementation(diagnosticReceiver, symbol, type, configuration);

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
				bool[] includedProperties = new bool[4];

				// Diagnostics are not reported for local configuration, as it is analyzed separately by the GenericSpecializationConfigurationAnalyzer.

				foreach (AttributeData attribute in attributes)
				{
					if (!SymbolEqualityComparer.Default.Equals(compilation.ConfigurationAttribute))
					{
						continue;
					}

					foreach (KeyValuePair<string, TypedConstant> pair in attribute.NamedArguments)
					{
						SetConfigurationValueLocally(pair.Key, pair.Value.Value, symbolName, c, includedProperties);
					}

					break;
				}

				if (HasAllProperties())
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

					if (HasAllProperties())
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

				bool HasAllProperties()
				{
					return includedProperties[0] && includedProperties[1] && includedProperties[2] && includedProperties[3];
				}
			}

			private static bool AnalyzeDefaultImplementation(
				IDiagnosticReceiver diagnosticReceiver,
				INamedTypeSymbol targetClass,
				INamedTypeSymbol defaultImpl,
				GenSpecConfiguration configuration
			)
			{
				bool isValid = true;

				if (defaultImpl.IsAbstract || defaultImpl.IsStatic)
				{
					diagnosticReceiver.ReportDiagnostic(DUR0221_DefaultImplementationCannotBeAbstractOrStatic, defaultImpl);
					isValid = false;
				}

				if (defaultImpl.IsGenericType)
				{
					diagnosticReceiver.ReportDiagnostic(DUR0223_DefaultImplementationCannotBeGeneric, defaultImpl);
					isValid = false;
				}

				if (defaultImpl.IsSealed && configuration.ForceInherit)
				{
					diagnosticReceiver.ReportDiagnostic(DUR0220_CannotForceInheritSealedClass, defaultImpl);
					isValid = false;
				}

				if (defaultImpl.BaseType is not null && SymbolEqualityComparer.Default.Equals(defaultImpl.BaseType, targetClass))
				{
					diagnosticReceiver.ReportDiagnostic(DUR0224_SpecializationCannotInheritTypeItIsSpecializing, defaultImpl);
					isValid = false;
				}

				if (targetClass.Arity > 0)
				{
					string interfaceName = AnalysisUtilities.CreateName(targetClass.JoinNamespaces(), $"{targetClass.Name}`{targetClass.Arity}", defaultImpl.Name);

					if (!defaultImpl.AllInterfaces.Any(intf => intf.Name == configuration.InterfaceName && intf.ToString() == interfaceName))
					{
						diagnosticReceiver.ReportDiagnostic(DUR0206_SpecializationMustImplementInterface, defaultImpl, interfaceName);
					}
				}

				return isValid;
			}

			private static bool AnalyzeModifiers(IDiagnosticReceiver diagnosticReceiver, INamedTypeSymbol symbol, CancellationToken cancellationToken = default)
			{
				bool hasPartial = false;
				bool hasInvalidModifiers = false;

				foreach (SyntaxToken modifier in symbol.GetModifiers(cancellationToken))
				{
					if (modifier.IsKind(SyntaxKind.StaticKeyword) || modifier.IsKind(SyntaxKind.AbstractKeyword))
					{
						diagnosticReceiver.ReportDiagnostic(DUR0211_TargetGenericClassCannotBeAbstractOrStatic, symbol);
						hasInvalidModifiers = true;
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

				return hasPartial && !hasInvalidModifiers;
			}

			private static void SetConfigurationValueLocally(string name, object? value, string symbolName, GenSpecConfiguration config, bool[] includedProperties)
			{
				if (name == _templateName)
				{
					if (includedProperties[0])
					{
						return;
					}

					if (value is string str && AnalysisUtilities.IsValidIdentifier(str) && str != symbolName)
					{
						config.TemplateName = str;
					}

					includedProperties[0] = true;
				}
				else if (name == _interfaceName)
				{
					if (includedProperties[1])
					{
						return;
					}

					if (value is string str && AnalysisUtilities.IsValidIdentifier(str) && str != symbolName)
					{
						config.InterfaceName = str;
					}

					includedProperties[1] = true;
				}
				else
				{
					SetForceInheritOrImportOptions(name, value, config, includedProperties);
				}
			}

			private static bool SetConfigurationValueScoped(IDiagnosticReceiver diagnosticReceiver, string name, object? value, INamedTypeSymbol symbol, GenSpecConfiguration config, bool[] includedProperties)
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

			private static void SetForceInheritOrImportOptions(string name, object? value, GenSpecConfiguration config, bool[] includedProperties)
			{
				if (name == _forceInherit)
				{
					if (includedProperties[2])
					{
						return;
					}

					if (value is bool b)
					{
						config.ForceInherit = b;
					}

					includedProperties[2] = true;
				}
				else if (name == _importOptions)
				{
					if (includedProperties[3])
					{
						return;
					}

					if (value is int i)
					{
						if (i < (int)default(GenSpecImportOptions) || i > (int)GenSpecImportOptions.OverrideAny)
						{
							i = default;
						}

						config.ImportOptions = (GenSpecImportOptions)i;
					}

					includedProperties[3] = true;
				}
			}
		}
	}
}
