// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Microsoft.CodeAnalysis;
using Durian.Analysis.Cache;
using System.Collections.Immutable;
using Durian.Analysis.Extensions;
using Microsoft.CodeAnalysis.Diagnostics;
using static Durian.Analysis.FriendClass.FriendClassDiagnostics;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.CodeAnalysis.CSharp;
using Durian.Configuration;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;

namespace Durian.Analysis.FriendClass
{
	/// <summary>
	/// Analyzes classes marked by the <see cref="FriendClassAttribute"/>.
	/// </summary>
#if !MAIN_PACKAGE

	[DiagnosticAnalyzer(LanguageNames.CSharp)]
#endif
#pragma warning disable RS1001 // Missing diagnostic analyzer attribute.
	public class FriendClassDeclarationAnalyzer : DurianAnalyzer<FriendClassCompilationData>
#pragma warning restore RS1001 // Missing diagnostic analyzer attribute.
	{
		/// <inheritdoc/>
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(
			DUR0301_TargetTypeIsOutsideOfAssembly,
			DUR0303_DoNotUseFriendClassConfigurationAttributeOnTypesWithNoFriends,
			DUR0304_DoNotUseApplyToTypeOnNonInternalTypes,
			DUR0305_ValueOfFriendClassCannotAccessTargetType,
			DUR0306_TypeDoesNotDeclareInternalMembers,
			DUR0307_FriendTypeSpecifiedByMultipleAttributes,
			DUR0310_TypeIsNotValid,
			DUR0311_TypeCannotBeFriendOfItself,
			DUR0312_InternalsVisibleToNotFound,
			DUR0314_DoNotAllowChildrenOnSealedType
		);

		/// <summary>
		/// Initializes a new instance of the <see cref="FriendClassDeclarationAnalyzer"/> class.
		/// </summary>
		public FriendClassDeclarationAnalyzer()
		{
		}

		/// <inheritdoc/>
		public override void Register(IDurianAnalysisContext context, FriendClassCompilationData compilation)
		{
			context.RegisterSymbolAction(context => Analyze(context, compilation), SymbolKind.NamedType);
		}

		/// <inheritdoc/>
		protected override FriendClassCompilationData CreateCompilation(CSharpCompilation compilation)
		{
			return new FriendClassCompilationData(compilation);
		}

		private static void Analyze(SymbolAnalysisContext context, FriendClassCompilationData compilation)
		{
			if (context.Symbol is not INamedTypeSymbol symbol || !(symbol.TypeKind is TypeKind.Class or TypeKind.Struct))
			{
				return;
			}

			AttributeData[] attributes = symbol.GetAttributes(compilation.FriendClassAttribute!).ToArray();

			if (attributes.Length == 0)
			{
				if (TryGetInvalidConfigurationDiagnostic(symbol, compilation, out Diagnostic? diagnostic))
				{
					context.ReportDiagnostic(diagnostic);
				}

				return;
			}

			FriendClassConfiguration config = GetConfiguration(symbol, compilation);

			ValidateConfiguration(context, symbol, config);

			AnalyzeAttributes(attributes, symbol, compilation, config);
		}

		private static IEnumerable<Diagnostic> AnalyzeAttributes(
			AttributeData[] attributes,
			INamedTypeSymbol symbol,
			FriendClassCompilationData compilation,
			FriendClassConfiguration configuration
		)
		{
#pragma warning disable RS1024 // Compare symbols correctly
			HashSet<ITypeSymbol> friendTypes = new(SymbolEqualityComparer.Default);
#pragma warning restore RS1024 // Compare symbols correctly

			foreach (AttributeData attribute in attributes)
			{
				if (!attribute.TryGetConstructorArgumentTypeValue(0, out ITypeSymbol? friend))
				{
					continue;
				}

				if (TryGetInvalidFriendTypeDiagnostic(symbol, friend, attribute, friendTypes, out Diagnostic? diagnostic))
				{
					yield return diagnostic;
					continue;
				}

				if (TryGetInvalidExternalFriendTypeDiagnostic(symbol, friend!, attribute, compilation.Compilation, configuration, out diagnostic))
				{
					yield return diagnostic;
					continue;
				}

				Location? location = null;

				if (!compilation.Compilation.IsSymbolAccessibleWithin(symbol, friend!))
				{
					InitializeFriendArgumentLocation(attribute, symbol, ref location);

					yield return Diagnostic.Create(
						descriptor: DUR0305_ValueOfFriendClassCannotAccessTargetType,
						location: location,
						messageArgs: new[] { symbol, friend }
					);
				}

				if (!symbol.GetMembers().Any(m => m.DeclaredAccessibility == Accessibility.Internal))
				{
					InitializeFriendArgumentLocation(attribute, symbol, ref location);

					yield return Diagnostic.Create(
						descriptor: DUR0306_TypeDoesNotDeclareInternalMembers,
						location: location,
						messageArgs: new[] { symbol }
					);
				}
			}
		}

		private static FriendClassConfiguration GetConfiguration(INamedTypeSymbol symbol, FriendClassCompilationData compilation)
		{
			FriendClassConfiguration @default = FriendClassConfiguration.Default;

			if (symbol.GetAttributes(compilation.FriendClassConfigurationAttribute!) is not AttributeData attr ||
				attr.ApplicationSyntaxReference is null ||
				attr.ApplicationSyntaxReference.GetSyntax() is not AttributeSyntax syntax
			)
			{
				return @default;
			}

			bool allowsChildren = GetBoolProperty(nameof(FriendClassConfigurationAttribute.AllowsChildren), @default.AllowsChildren);
			bool allowsExternalAssembly = GetBoolProperty(nameof(FriendClassConfigurationAttribute.AllowsExternalAssembly), @default.AllowsExternalAssembly);
			bool applyToType = GetBoolProperty(nameof(FriendClassConfigurationAttribute.ApplyToType), @default.ApplyToType);

			return new()
			{
				AllowsChildren = allowsChildren,
				AllowsExternalAssembly = allowsExternalAssembly,
				ApplyToType = applyToType,
				Syntax = syntax
			};

			bool GetBoolProperty(string name, bool @default)
			{
				if (!attr.TryGetNamedArgumentValue(name, out bool value))
				{
					value = @default;
				}

				return value;
			}
		}

		private static Location GetFriendArgumentLocation(AttributeData attribute, INamedTypeSymbol symbol)
		{
			if (attribute.ApplicationSyntaxReference?.GetSyntax() is not AttributeSyntax syntax ||
				syntax.ArgumentList is null ||
				syntax.ArgumentList.Arguments is not SeparatedSyntaxList<AttributeArgumentSyntax> { Count: > 0 } arguments
			)
			{
				return symbol.Locations.FirstOrDefault() ?? Location.None;
			}

			return arguments[0].GetLocation();
		}

		private static void InitializeFriendArgumentLocation(AttributeData attribute, INamedTypeSymbol symbol, [NotNull] ref Location? location)
		{
			if (location is null)
			{
				location = GetFriendArgumentLocation(attribute, symbol);
			}
		}

		private static bool TryGetInvalidConfigurationDiagnostic(
			INamedTypeSymbol symbol,
			FriendClassCompilationData compilation,
			[NotNullWhen(true)] out Diagnostic? diagnostic
		)
		{
			if (symbol.GetAttributes(compilation.FriendClassConfigurationAttribute!) is AttributeData attr)
			{
				diagnostic = Diagnostic.Create(
					descriptor: DUR0303_DoNotUseFriendClassConfigurationAttributeOnTypesWithNoFriends,
					location: attr.ApplicationSyntaxReference?.GetSyntax()?.GetLocation() ?? symbol.Locations.FirstOrDefault(),
					messageArgs: symbol
				);

				return true;
			}

			diagnostic = null;
			return false;
		}

		private static bool TryGetInvalidExternalFriendTypeDiagnostic(
			INamedTypeSymbol symbol,
			ITypeSymbol friend,
			AttributeData attribute,
			CSharpCompilation compilation,
			FriendClassConfiguration configuration,
			[NotNullWhen(true)] out Diagnostic? diagnostic
		)
		{
			if (SymbolEqualityComparer.Default.Equals(compilation.Assembly, friend.ContainingAssembly))
			{
				diagnostic = null;
				return false;
			}

			if (friend.ContainingAssembly is null || !configuration.AllowsExternalAssembly)
			{
				diagnostic = Diagnostic.Create(
					descriptor: DUR0301_TargetTypeIsOutsideOfAssembly,
					location: GetFriendArgumentLocation(attribute, symbol),
					messageArgs: new[] { symbol, friend }
				);

				return true;
			}

			if (!compilation.IsSymbolAccessibleWithin(symbol, friend.ContainingAssembly))
			{
				diagnostic = Diagnostic.Create(
					descriptor: DUR0312_InternalsVisibleToNotFound,
					location: GetFriendArgumentLocation(attribute, symbol),
					messageArgs: new object[] { symbol, friend, friend.ContainingAssembly.Name }
				);

				return true;
			}

			diagnostic = null;
			return false;
		}

		private static bool TryGetInvalidFriendTypeDiagnostic(
			INamedTypeSymbol symbol,
			ITypeSymbol? friend,
			AttributeData attribute,
			HashSet<ITypeSymbol> friendTypes,
			[NotNullWhen(true)] out Diagnostic? diagnostic
		)
		{
			if (friend is null ||
				friend is not INamedTypeSymbol ||
				!(friend.TypeKind is TypeKind.Interface or TypeKind.Class or TypeKind.Struct)
			)
			{
				diagnostic = Diagnostic.Create(
					descriptor: DUR0310_TypeIsNotValid,
					location: GetFriendArgumentLocation(attribute, symbol),
					messageArgs: new[] { symbol, friend }
				);

				return true;
			}

			if (SymbolEqualityComparer.Default.Equals(friend, symbol))
			{
				diagnostic = Diagnostic.Create(
					descriptor: DUR0311_TypeCannotBeFriendOfItself,
					location: GetFriendArgumentLocation(attribute, symbol),
					messageArgs: new[] { symbol }
				);

				return true;
			}

			if (!friendTypes.Add(friend))
			{
				diagnostic = Diagnostic.Create(
					descriptor: DUR0307_FriendTypeSpecifiedByMultipleAttributes,
					location: GetFriendArgumentLocation(attribute, symbol),
					messageArgs: new[] { symbol, friend }
				);

				return true;
			}

			diagnostic = null;
			return false;
		}

		private static void ValidateConfiguration(
			SymbolAnalysisContext context,
			INamedTypeSymbol symbol,
			FriendClassConfiguration configuration
		)
		{
			if (configuration.ApplyToType && symbol.DeclaredAccessibility != Accessibility.Internal)
			{
				context.ReportDiagnostic(Diagnostic.Create(
					descriptor: DUR0304_DoNotUseApplyToTypeOnNonInternalTypes,
					location: GetArgumentLocation(nameof(FriendClassConfigurationAttribute.ApplyToType)),
					messageArgs: new[] { symbol }
				));
			}

			if (configuration.AllowsChildren && (symbol.TypeKind == TypeKind.Struct || symbol.IsSealed || symbol.IsStatic))
			{
				context.ReportDiagnostic(Diagnostic.Create(
					descriptor: DUR0314_DoNotAllowChildrenOnSealedType,
					location: GetArgumentLocation(nameof(FriendClassConfigurationAttribute.AllowsChildren)),
					messageArgs: new[] { symbol }
				));
			}

			Location GetArgumentLocation(string argName)
			{
				if (configuration.Syntax is not null &&
					configuration.Syntax.ArgumentList is not null &&
					configuration.Syntax.ArgumentList.Arguments is SeparatedSyntaxList<AttributeArgumentSyntax> { Count: > 0 } arguments &&
					arguments.FirstOrDefault(arg => arg.NameEquals is not null && arg.NameEquals.Name.ToString() == argName) is AttributeArgumentSyntax arg
				)
				{
					return arg.GetLocation();
				}

				return symbol.Locations.FirstOrDefault() ?? Location.None;
			}
		}
	}
}
