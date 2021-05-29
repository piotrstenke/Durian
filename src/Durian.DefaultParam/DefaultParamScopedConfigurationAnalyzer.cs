using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Durian.Generator.DefaultParam
{
	/// <summary>
	/// Analyzes the usage of the <see cref="DefaultParamScopedConfigurationAnalyzer"/>
	/// </summary>
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public sealed class DefaultParamScopedConfigurationAnalyzer : DurianAnalyzer<DefaultParamCompilationData>
	{
		/// <inheritdoc/>
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(
			DefaultParamDiagnostics.DUR0125_ScopedConfigurationShouldNotBePlacedOnATypeWithoutDefaultParamMembers
		);

		/// <summary>
		/// Initializes a new instance of the <see cref="DefaultParamScopedConfigurationAnalyzer"/> class.
		/// </summary>
		public DefaultParamScopedConfigurationAnalyzer()
		{
		}

		/// <inheritdoc/>
		protected override DefaultParamCompilationData CreateCompilation(CSharpCompilation compilation)
		{
			return new DefaultParamCompilationData(compilation);
		}

		/// <inheritdoc/>
		protected override void Register(CompilationStartAnalysisContext context, DefaultParamCompilationData compilation)
		{
			context.RegisterSyntaxNodeAction(c => Analyze(c, compilation), SyntaxKind.Attribute);
		}

		private static void Analyze(SyntaxNodeAnalysisContext context, DefaultParamCompilationData compilation)
		{
			if (context.Node is not AttributeSyntax syntax || syntax.Parent?.Parent is not TypeDeclarationSyntax node)
			{
				return;
			}

			INamedTypeSymbol? configurationAttribute = context.SemanticModel.GetSymbolInfo(syntax).Symbol?.ContainingType;

			if (configurationAttribute is null || !SymbolEqualityComparer.Default.Equals(configurationAttribute, compilation.ScopedConfigurationAttribute))
			{
				return;
			}

			if (context.SemanticModel.GetDeclaredSymbol(node) is not INamedTypeSymbol symbol)
			{
				return;
			}

			if(!HasMemberWithDefaultParamAttribute(symbol, compilation.MainAttribute!))
			{
				context.ReportDiagnostic(Diagnostic.Create(DefaultParamDiagnostics.DUR0125_ScopedConfigurationShouldNotBePlacedOnATypeWithoutDefaultParamMembers, symbol.Locations.FirstOrDefault(), symbol));
			}
		}

		private static bool HasMemberWithDefaultParamAttribute(INamedTypeSymbol symbol, INamedTypeSymbol attributeSymbol)
		{
			foreach (ISymbol member in symbol.GetMembers())
			{
				ImmutableArray<ITypeParameterSymbol> typeParameters;

				if (member is INamedTypeSymbol t)
				{
					typeParameters = t.TypeParameters;
				}
				else if (member is IMethodSymbol m)
				{
					typeParameters = m.TypeParameters;
				}
				else
				{
					continue;
				}

				if (typeParameters.Length == 0)
				{
					continue;
				}

				foreach (ITypeParameterSymbol p in typeParameters)
				{
					if (p.GetAttributes().Any(attr => SymbolEqualityComparer.Default.Equals(attr.AttributeClass, attributeSymbol)))
					{
						return true;
					}
				}
			}

			return false;
		}
	}
}
