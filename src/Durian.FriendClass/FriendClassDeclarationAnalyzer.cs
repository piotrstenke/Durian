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
			DUR0304_ValueOfFriendClassCannotAccessTargetType,
			DUR0305_TypeDoesNotDeclareInternalMembers,
			DUR0306_FriendTypeSpecifiedByMultipleAttributes,
			DUR0308_TypeIsNotValid,
			DUR0309_TypeCannotBeFriendOfItself,
			DUR0311_DoNotAllowChildrenOnSealedType,
			DUR0312_InnerTypeIsImplicitFriend,
			DUR0313_ConfigurationIsRedundant
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
		protected override FriendClassCompilationData CreateCompilation(CSharpCompilation compilation, IDiagnosticReceiver diagnosticReceiver)
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
				if (TryGetInvalidConfigurationDiagnostic(symbol, compilation, out Diagnostic? d))
				{
					context.ReportDiagnostic(d);
				}

				return;
			}

			FriendClassConfiguration config = GetConfiguration(symbol, compilation);

			if (!ValidateConfiguration(symbol, config, out Diagnostic? diagnostic))
			{
				context.ReportDiagnostic(diagnostic);
			}

			foreach (Diagnostic d in AnalyzeAttributes(attributes, symbol, compilation))
			{
				context.ReportDiagnostic(d);
			}
		}

		private static IEnumerable<Diagnostic> AnalyzeAttributes(
			AttributeData[] attributes,
			INamedTypeSymbol symbol,
			FriendClassCompilationData compilation
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

				if (TryGetInvalidExternalFriendTypeDiagnostic(symbol, friend!, attribute, compilation.Compilation, out diagnostic))
				{
					yield return diagnostic;
					continue;
				}

				Location? location = null;

				if (!compilation.Compilation.IsSymbolAccessibleWithin(symbol, friend!))
				{
					InitializeFriendArgumentLocation(attribute, symbol, ref location);

					yield return Diagnostic.Create(
						descriptor: DUR0304_ValueOfFriendClassCannotAccessTargetType,
						location: location,
						messageArgs: new[] { symbol, friend }
					);
				}

				if (!symbol.GetMembers().Any(m => m.DeclaredAccessibility == Accessibility.Internal))
				{
					InitializeFriendArgumentLocation(attribute, symbol, ref location);

					yield return Diagnostic.Create(
						descriptor: DUR0305_TypeDoesNotDeclareInternalMembers,
						location: location,
						messageArgs: new[] { symbol }
					);
				}
			}
		}

		private static FriendClassConfiguration GetConfiguration(INamedTypeSymbol symbol, FriendClassCompilationData compilation)
		{
			FriendClassConfiguration @default = FriendClassConfiguration.Default;

			if (symbol.GetAttribute(compilation.FriendClassConfigurationAttribute!) is not AttributeData attr ||
				attr.ApplicationSyntaxReference is null ||
				attr.ApplicationSyntaxReference.GetSyntax() is not AttributeSyntax syntax
			)
			{
				return @default;
			}

			bool allowsChildren = GetBoolProperty(nameof(FriendClassConfigurationAttribute.AllowsChildren), @default.AllowsChildren);

			return new()
			{
				AllowsChildren = allowsChildren,
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
			if (symbol.GetAttribute(compilation.FriendClassConfigurationAttribute!) is AttributeData attr &&
				!attr.GetNamedArgumentValue<bool>(nameof(FriendClassConfigurationAttribute.AllowsChildren)))
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
			[NotNullWhen(true)] out Diagnostic? diagnostic
		)
		{
			if (SymbolEqualityComparer.Default.Equals(compilation.Assembly, friend.ContainingAssembly))
			{
				diagnostic = null;
				return false;
			}

			diagnostic = Diagnostic.Create(
				descriptor: DUR0301_TargetTypeIsOutsideOfAssembly,
				location: GetFriendArgumentLocation(attribute, symbol),
				messageArgs: new[] { symbol, friend }
			);

			return true;
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
					descriptor: DUR0308_TypeIsNotValid,
					location: GetFriendArgumentLocation(attribute, symbol),
					messageArgs: new[] { symbol, friend }
				);

				return true;
			}

			if (SymbolEqualityComparer.Default.Equals(friend, symbol))
			{
				diagnostic = Diagnostic.Create(
					descriptor: DUR0309_TypeCannotBeFriendOfItself,
					location: GetFriendArgumentLocation(attribute, symbol),
					messageArgs: new[] { symbol }
				);

				return true;
			}

			if (!friendTypes.Add(friend))
			{
				diagnostic = Diagnostic.Create(
					descriptor: DUR0306_FriendTypeSpecifiedByMultipleAttributes,
					location: GetFriendArgumentLocation(attribute, symbol),
					messageArgs: new[] { symbol, friend }
				);

				return true;
			}

			if (symbol.ContainsSymbol(friend))
			{
				diagnostic = Diagnostic.Create(
					descriptor: DUR0312_InnerTypeIsImplicitFriend,
					location: GetFriendArgumentLocation(attribute, symbol),
					messageArgs: new[] { symbol }
				);

				return true;
			}

			diagnostic = null;
			return false;
		}

		private static bool ValidateConfiguration(
			INamedTypeSymbol symbol,
			FriendClassConfiguration configuration,
			[NotNullWhen(false)] out Diagnostic? diagnostic
		)
		{
			if (!configuration.AllowsChildren)
			{
				if (configuration.Syntax is not null)
				{
					diagnostic = Diagnostic.Create(
						descriptor: DUR0313_ConfigurationIsRedundant,
						location: configuration.Syntax.GetLocation(),
						messageArgs: new[] { symbol }
					);

					return false;
				}
			}
			else if (symbol.TypeKind == TypeKind.Struct || symbol.IsSealed || symbol.IsStatic)
			{
				diagnostic = Diagnostic.Create(
					descriptor: DUR0311_DoNotAllowChildrenOnSealedType,
					location: GetArgumentLocation(nameof(FriendClassConfigurationAttribute.AllowsChildren)),
					messageArgs: new[] { symbol }
				);

				return false;
			}

			diagnostic = null;
			return true;

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
