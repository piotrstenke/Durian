using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Durian.Analysis.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using static Durian.Analysis.FriendClass.FriendClassDiagnostics;

namespace Durian.Analysis.FriendClass
{
	/// <summary>
	/// Analyzes classes marked by the <c>Durian.FriendClassAttribute</c>.
	/// </summary>
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class FriendClassDeclarationAnalyzer : DurianAnalyzer<FriendClassCompilationData>
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
			DUR0313_ConfigurationIsRedundant,
			DUR0315_DoNotAllowInheritedOnTypeWithoutBaseType,
			DUR0316_BaseTypeHasNoInternalInstanceMembers
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

		internal static bool IsInternal(ISymbol symbol)
		{
			return symbol.DeclaredAccessibility is Accessibility.Internal or Accessibility.ProtectedOrInternal;
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

			List<Diagnostic> diagnostics = new(attributes.Length * 2);

			ValidateConfiguration(symbol, config, diagnostics);
			AnalyzeAttributes(attributes, symbol, config, compilation, diagnostics);

			foreach (Diagnostic d in diagnostics)
			{
				context.ReportDiagnostic(d);
			}
		}

		private static void AnalyzeAttributes(
			AttributeData[] attributes,
			INamedTypeSymbol symbol,
			FriendClassConfiguration config,
			FriendClassCompilationData compilation,
			List<Diagnostic> diagnostics
		)
		{
			HashSet<ITypeSymbol> friendTypes = new(SymbolEqualityComparer.Default);

			foreach (AttributeData attribute in attributes)
			{
				if (!attribute.TryGetConstructorArgumentTypeValue(0, out ITypeSymbol? friend))
				{
					continue;
				}

				if (TryGetInvalidFriendTypeDiagnostic(symbol, friend, attribute, friendTypes, out Diagnostic? diagnostic))
				{
					diagnostics.Add(diagnostic);
					continue;
				}

				if (TryGetInvalidExternalFriendTypeDiagnostic(symbol, friend!, attribute, compilation.Compilation, out diagnostic))
				{
					diagnostics.Add(diagnostic);
					continue;
				}

				Location? location = null;

				if (!compilation.Compilation.IsSymbolAccessibleWithin(symbol, friend!))
				{
					InitializeFriendArgumentLocation(attribute, symbol, ref location);

					diagnostics.Add(Diagnostic.Create(
						descriptor: DUR0304_ValueOfFriendClassCannotAccessTargetType,
						location: location,
						messageArgs: new[] { symbol, friend }
					));
				}

				if (!config.IncludeInherited && !symbol.GetMembers().Any(IsInternal))
				{
					Location? attrLocation = attribute.GetLocation();

					if (attrLocation is null)
					{
						InitializeFriendArgumentLocation(attribute, symbol, ref location);
						attrLocation = location;
					}

					diagnostics.Add(Diagnostic.Create(
						descriptor: DUR0305_TypeDoesNotDeclareInternalMembers,
						location: attrLocation,
						messageArgs: new[] { symbol }
					));
				}
			}
		}

		private static Location GetArgumentLocation(AttributeSyntax? syntax, ISymbol symbol, string argName)
		{
			if (syntax is not null &&
				syntax.ArgumentList is not null &&
				syntax.ArgumentList.Arguments is SeparatedSyntaxList<AttributeArgumentSyntax> { Count: > 0 } arguments &&
				arguments.FirstOrDefault(arg => arg.NameEquals is not null && arg.NameEquals.Name.ToString() == argName) is AttributeArgumentSyntax arg
			)
			{
				return arg.GetLocation();
			}

			return symbol.Locations.FirstOrDefault() ?? Location.None;
		}

		private static FriendClassConfiguration GetConfiguration(INamedTypeSymbol symbol, FriendClassCompilationData compilation)
		{
			if (symbol.GetAttribute(compilation.FriendClassConfigurationAttribute!) is not AttributeData attr ||
				attr.ApplicationSyntaxReference is null ||
				attr.ApplicationSyntaxReference.GetSyntax() is not AttributeSyntax syntax
			)
			{
				return FriendClassConfiguration.Default;
			}

			bool isZeroed = true;

			bool allowChildren = GetBoolProperty(FriendClassConfigurationAttributeProvider.AllowChildren, ref isZeroed);
			bool includeInherited = GetBoolProperty(FriendClassConfigurationAttributeProvider.IncludeInherited, ref isZeroed);

			return new()
			{
				AllowChildren = allowChildren,
				IncludeInherited = includeInherited,
				IsZeroed = isZeroed,
				Syntax = syntax
			};

			bool GetBoolProperty(string name, ref bool isZeroed)
			{
				if (attr.TryGetNamedArgumentValue(name, out bool value))
				{
					isZeroed = false;
				}
				else
				{
					value = default;
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

		private static bool IsValidInternalParentMember(ISymbol symbol)
		{
			if (!IsInternal(symbol))
			{
				return false;
			}

			if (symbol.IsStatic)
			{
				return false;
			}

			if (symbol is IMethodSymbol method && method.MethodKind == MethodKind.Constructor)
			{
				return false;
			}

			return true;
		}

		private static bool TryGetInvalidConfigurationDiagnostic(
			INamedTypeSymbol symbol,
			FriendClassCompilationData compilation,
			[NotNullWhen(true)] out Diagnostic? diagnostic
		)
		{
			if (symbol.GetAttribute(compilation.FriendClassConfigurationAttribute!) is AttributeData attr &&
				!attr.GetNamedArgumentValue<bool>(FriendClassConfigurationAttributeProvider.AllowChildren))
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

		private static void ValidateConfiguration(
			INamedTypeSymbol symbol,
			FriendClassConfiguration configuration,
			List<Diagnostic> diagnostics
		)
		{
			if (configuration.IsZeroed)
			{
				diagnostics.Add(Diagnostic.Create(
					descriptor: DUR0313_ConfigurationIsRedundant,
					location: configuration.Syntax.GetLocation(),
					messageArgs: new[] { symbol }
				));

				return;
			}

			if (configuration.AllowChildren && symbol.IsSealedKind())
			{
				diagnostics.Add(Diagnostic.Create(
					descriptor: DUR0311_DoNotAllowChildrenOnSealedType,
					location: GetArgumentLocation(configuration.Syntax, symbol, FriendClassConfigurationAttributeProvider.AllowChildren),
					messageArgs: new[] { symbol }
				));
			}

			if (configuration.IncludeInherited)
			{
				if (symbol.HasExplicitBaseType())
				{
					if (!symbol.BaseType!.GetAllMembers().Any(IsValidInternalParentMember))
					{
						diagnostics.Add(Diagnostic.Create(
							descriptor: DUR0316_BaseTypeHasNoInternalInstanceMembers,
							location: GetArgumentLocation(configuration.Syntax, symbol, FriendClassConfigurationAttributeProvider.IncludeInherited),
							messageArgs: new[] { symbol }
						));
					}
				}
				else
				{
					diagnostics.Add(Diagnostic.Create(
						descriptor: DUR0315_DoNotAllowInheritedOnTypeWithoutBaseType,
						location: GetArgumentLocation(configuration.Syntax, symbol, FriendClassConfigurationAttributeProvider.IncludeInherited),
						messageArgs: new[] { symbol }
					));
				}
			}
		}
	}
}
