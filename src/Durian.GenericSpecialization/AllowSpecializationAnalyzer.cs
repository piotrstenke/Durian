// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Durian.Analysis.Extensions;
using Durian.Configuration;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using static Durian.Analysis.GenericSpecialization.GenSpecDiagnostics;

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
			DUR0219_CannotForceInheritSealedClass,
			DUR0220_DefaultImplementationCannotBeAbstractOrStatic,
			DUR0222_TargetNameCannotBeTheSameAsContainingClass,
			DUR0221_DefaultImplementationCannotBeGeneric,
			DUR0224_SpecializationCannotInheritTypeItIsSpecializing,
			DUR0223_DefaultSpecializationMustBeClass
		);

		/// <summary>
		/// Initializes a new instance of the <see cref="AllowSpecializationAnalyzer"/> class.
		/// </summary>
		public AllowSpecializationAnalyzer()
		{
		}

		/// <inheritdoc cref="WithDiagnostics.AnalyzeContainingTypes(IDiagnosticReceiver, INamedTypeSymbol, INamedTypeSymbol[], CancellationToken)"/>
		public static bool AnalyzeContainingTypes(
			INamedTypeSymbol symbol,
			INamedTypeSymbol[]? containingTypes = null,
			CancellationToken cancellationToken = default
		)
		{
			InitializeContainingTypes(ref containingTypes, symbol);

			if (containingTypes.Length > 0)
			{
				foreach (INamedTypeSymbol type in containingTypes)
				{
					if (!HasPartialKeyword(type, cancellationToken))
					{
						return false;
					}
				}
			}

			return true;
		}

		/// <inheritdoc cref="WithDiagnostics.AnalyzeContainingTypes(IDiagnosticReceiver, INamedTypeSymbol, out INamedTypeSymbol[], CancellationToken)"/>
		public static bool AnalyzeContainingTypes(
			INamedTypeSymbol symbol,
			[NotNullWhen(true)] out INamedTypeSymbol[]? containingTypes,
			CancellationToken cancellationToken = default
		)
		{
			INamedTypeSymbol[] symbols = symbol.GetContainingTypeSymbols().ToArray();
			bool isValid = AnalyzeContainingTypes(symbol, symbols, cancellationToken);
			containingTypes = isValid ? symbols : null;
			return isValid;
		}

		/// <inheritdoc cref="WithDiagnostics.AnalyzeDeclaration(IDiagnosticReceiver, INamedTypeSymbol, CancellationToken)"/>
		public static bool AnalyzeDeclaration(INamedTypeSymbol symbol, CancellationToken cancellationToken = default)
		{
			if (symbol.TypeParameters.Length == 0 || symbol.BaseType is not null || symbol.Interfaces.Length > 0)
			{
				return false;
			}

			return AnalyzeModifiers(symbol, cancellationToken);
		}

		/// <inheritdoc cref="WithDiagnostics.AnalyzeMembers(IDiagnosticReceiver, INamedTypeSymbol, GenSpecCompilationData, IEnumerable{AttributeData}?, INamedTypeSymbol[])"/>
		public static bool AnalyzeMembers(
			INamedTypeSymbol symbol,
			GenSpecCompilationData compilation,
			IEnumerable<AttributeData>? attributes = null,
			INamedTypeSymbol[]? containingTypes = null
		)
		{
			if (TryGetConfiguration(symbol, compilation, out GenSpecConfiguration? configuration, attributes, containingTypes))
			{
				return AnalyzeMembers(symbol, compilation, configuration);
			}

			return false;
		}

		/// <inheritdoc cref="WithDiagnostics.AnalyzeMembers(IDiagnosticReceiver, INamedTypeSymbol, GenSpecCompilationData, GenSpecConfiguration)"/>
		public static bool AnalyzeMembers(
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
				return false;
			}

			string interfaceFullName = AnalysisUtilities.CreateName(symbol.JoinNamespaces(), $"{symbol.Name}`{symbol.Arity}", configuration.InterfaceName);

			if (members.Length > 1)
			{
				foreach (ISymbol member in members)
				{
					if (!MemberIsValidInTargetClass(member, symbol, compilation, configuration, interfaceFullName))
					{
						return false;
					}
				}
			}

			if (Array.Find(members, m => m.Name == configuration.TemplateName) is not INamedTypeSymbol type)
			{
				return false;
			}

			return AnalyzeDefaultImplementation(symbol, type, configuration, interfaceFullName);
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
		public static bool HasAllowSpecializationAttribute(
			INamedTypeSymbol symbol,
			GenSpecCompilationData compilation,
			[NotNullWhen(true)] out AttributeData[]? attributes
		)
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
		public static bool ShouldAnalyze(
			INamedTypeSymbol symbol,
			GenSpecCompilationData compilation,
			IEnumerable<AttributeData>? attributes = null
		)
		{
			if (symbol.TypeKind != TypeKind.Class)
			{
				return false;
			}

			InitializeAttributes(ref attributes, symbol);

			return HasAllowSpecializationAttribute(attributes, compilation);
		}

		/// <summary>
		/// Determines whether the specified <paramref name="symbol"/> is valid for analysis.
		/// </summary>
		/// <param name="symbol"><see cref="INamedTypeSymbol"/> to check if is valid for analysis.</param>
		/// <param name="compilation">Current <see cref="GenSpecCompilationData"/>.</param>
		/// <param name="attributes">An array of <see cref="AttributeData"/>s of the specified <paramref name="symbol"/>. Included only if the method returns <see langword="true"/>.</param>
		public static bool ShouldAnalyze(
			INamedTypeSymbol symbol,
			GenSpecCompilationData compilation,
			[NotNullWhen(true)] out AttributeData[]? attributes
		)
		{
			if (symbol.TypeKind != TypeKind.Class)
			{
				attributes = null;
				return false;
			}

			return HasAllowSpecializationAttribute(symbol, compilation, out attributes);
		}

		/// <inheritdoc cref="WithDiagnostics.TryGetConfiguration(IDiagnosticReceiver, INamedTypeSymbol, GenSpecCompilationData, out GenSpecConfiguration?, IEnumerable{AttributeData}?, INamedTypeSymbol[])"/>
		public static bool TryGetConfiguration(
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
						if (!SetConfigurationValueScoped(pair.Key, pair.Value.Value, symbol, c, includedProperties))
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
				configuration = null;
				return false;
			}

			configuration = c;
			return true;
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

		private static bool AnalyzeDefaultImplementation(
			INamedTypeSymbol targetClass,
			INamedTypeSymbol defaultImpl,
			GenSpecConfiguration configuration,
			string interfaceFullName
		)
		{
			return
				!defaultImpl.IsAbstract &&
				!defaultImpl.IsStatic &&
				!defaultImpl.IsGenericType &&
				defaultImpl.TypeKind == TypeKind.Class &&
				!defaultImpl.IsRecord &&
				!(defaultImpl.IsSealed && configuration.ForceInherit) &&
				!(defaultImpl.BaseType is not null && SymbolEqualityComparer.Default.Equals(defaultImpl.BaseType, targetClass)) &&
				HasSpecInterface(defaultImpl, configuration.InterfaceName, interfaceFullName);
		}

		private static bool AnalyzeModifiers(INamedTypeSymbol symbol, CancellationToken cancellationToken)
		{
			bool hasPartial = false;

			foreach (SyntaxToken modifier in symbol.GetModifiers(cancellationToken))
			{
				if (modifier.IsKind(SyntaxKind.StaticKeyword) || modifier.IsKind(SyntaxKind.AbstractKeyword))
				{
					return false;
				}
				else if (modifier.IsKind(SyntaxKind.PartialKeyword))
				{
					hasPartial = true;
				}
			}

			return hasPartial;
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

		private static bool HasAllProperties(bool[] includedProperties)
		{
			return includedProperties[0] && includedProperties[1] && includedProperties[2] && includedProperties[3];
		}

		private static bool HasPartialKeyword(INamedTypeSymbol symbol, CancellationToken cancellationToken)
		{
			return symbol.GetModifiers(cancellationToken).Any(m => m.IsKind(SyntaxKind.PartialKeyword));
		}

		private static bool HasSpecInterface(INamedTypeSymbol symbol, string interfaceName, string interfaceFullName)
		{
			return symbol.AllInterfaces.Any(intf => intf.Name == interfaceName && intf.ToString() == interfaceFullName);
		}

		private static bool MemberIsValidInTargetClass(
			ISymbol member,
			INamedTypeSymbol targetClass,
			GenSpecCompilationData compilation,
			GenSpecConfiguration configuration,
			string interfaceFullName
		)
		{
			if (member is not INamedTypeSymbol t || t.TypeKind != TypeKind.Class)
			{
				return false;
			}

			if (t.GetAttributeData(compilation.GenericSpecializationAttribute!) is AttributeData attr &&
				attr.TryGetConstructorArgumentValue(0, out INamedTypeSymbol? value))
			{
				return SymbolEqualityComparer.Default.Equals(targetClass, value);
			}

			return HasSpecInterface(t, configuration.InterfaceName, interfaceFullName);
		}

		private static void SetConfigurationValueLocally(
			string name,
			object? value,
			string symbolName,
			GenSpecConfiguration config,
			bool[] includedProperties
		)
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

		private static bool SetConfigurationValueScoped(
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

		private static void SetForceInheritOrImportOptions(
			string name,
			object? value,
			GenSpecConfiguration config,
			bool[] includedProperties
		)
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

		private static bool TrySetAllConfigurationPropertiesLocally(
			GenSpecCompilationData compilation,
			GenSpecConfiguration configuration,
			string symbolName,
			IEnumerable<AttributeData> attributes,
			out bool[] includedProperties
		)
		{
			includedProperties = new bool[4];

			// Diagnostics are not reported for local configuration, as it is analyzed separately by the GenericSpecializationConfigurationAnalyzer.

			foreach (AttributeData attribute in attributes)
			{
				if (!SymbolEqualityComparer.Default.Equals(compilation.ConfigurationAttribute))
				{
					continue;
				}

				foreach (KeyValuePair<string, TypedConstant> pair in attribute.NamedArguments)
				{
					SetConfigurationValueLocally(pair.Key, pair.Value.Value, symbolName, configuration, includedProperties);
				}

				break;
			}

			return HasAllProperties(includedProperties);
		}
	}
}
