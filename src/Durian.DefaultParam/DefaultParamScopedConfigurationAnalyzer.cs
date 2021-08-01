// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Immutable;
using System.Linq;
using Durian.Analysis.Extensions;
using Durian.Configuration;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Durian.Analysis.DefaultParam
{
#pragma warning disable RS1001 // Missing diagnostic analyzer attribute.

	/// <summary>
	/// Analyzes the usage of the <see cref="DefaultParamScopedConfigurationAnalyzer"/>
	/// </summary>
#if !MAIN_PACKAGE

	[DiagnosticAnalyzer(LanguageNames.CSharp)]
#endif

	public sealed class DefaultParamScopedConfigurationAnalyzer : DurianAnalyzer<DefaultParamCompilationData>
#pragma warning restore RS1001 // Missing diagnostic analyzer attribute.
	{
		/// <inheritdoc/>
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(
			DefaultParamDiagnostics.DUR0125_ScopedConfigurationShouldNotBePlacedOnATypeWithoutDefaultParamMembers,
			DefaultParamDiagnostics.DUR0127_InvalidTargetNamespace
		);

		/// <summary>
		/// Initializes a new instance of the <see cref="DefaultParamScopedConfigurationAnalyzer"/> class.
		/// </summary>
		public DefaultParamScopedConfigurationAnalyzer()
		{
		}

		/// <inheritdoc/>
		public override void Register(IDurianAnalysisContext context, DefaultParamCompilationData compilation)
		{
			context.RegisterSyntaxNodeAction(c => Analyze(c, compilation), SyntaxKind.Attribute);
		}

		/// <inheritdoc/>
		protected override DefaultParamCompilationData CreateCompilation(CSharpCompilation compilation)
		{
			return new DefaultParamCompilationData(compilation);
		}

		private static void Analyze(SyntaxNodeAnalysisContext context, DefaultParamCompilationData compilation)
		{
			if (context.Node is not AttributeSyntax syntax || syntax.Parent?.Parent is not MemberDeclarationSyntax parent)
			{
				return;
			}

			INamedTypeSymbol? configurationAttribute = context.SemanticModel.GetSymbolInfo(syntax).Symbol?.ContainingType;

			if (configurationAttribute is null ||
				!SymbolEqualityComparer.Default.Equals(configurationAttribute, compilation.ScopedConfigurationAttribute) ||
				context.SemanticModel.GetDeclaredSymbol(parent) is not ISymbol symbol)
			{
				return;
			}

			if (parent is TypeDeclarationSyntax && symbol is INamedTypeSymbol type && !HasMemberWithDefaultParamAttribute(type, compilation.MainAttribute!))
			{
				context.ReportDiagnostic(Diagnostic.Create(DefaultParamDiagnostics.DUR0125_ScopedConfigurationShouldNotBePlacedOnATypeWithoutDefaultParamMembers, symbol.Locations.FirstOrDefault(), symbol));
			}

			ReportIfInvalidTargetNamespace(symbol, syntax, context);
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

		private static void ReportIfInvalidTargetNamespace(ISymbol symbol, AttributeSyntax node, SyntaxNodeAnalysisContext context)
		{
			const string propertyName = nameof(DefaultParamConfigurationAttribute.TargetNamespace);

			if (node.ArgumentList is null || !node.ArgumentList.Arguments.Any())
			{
				return;
			}

			AttributeArgumentSyntax? argument = node.ArgumentList.Arguments.FirstOrDefault(arg => arg.NameEquals is not null && arg.NameEquals.Name.ToString() == propertyName);

			if (argument is not null &&
				symbol.GetAttribute(node, context.CancellationToken) is AttributeData data &&
				data.TryGetNamedArgumentValue(propertyName, out string? value) &&
				value is not null)
			{
				if (value == "Durian.Generator" || !AnalysisUtilities.IsValidNamespaceIdentifier(value))
				{
					context.ReportDiagnostic(Diagnostic.Create(DefaultParamDiagnostics.DUR0127_InvalidTargetNamespace, argument.GetLocation(), symbol, value));
				}
			}
		}
	}
}
