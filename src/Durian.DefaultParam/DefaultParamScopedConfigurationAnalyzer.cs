// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Durian.Analysis.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

namespace Durian.Analysis.DefaultParam
{
	/// <summary>
	/// Analyzes the usage of the <see cref="DefaultParamScopedConfigurationAnalyzer"/>
	/// </summary>
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public sealed class DefaultParamScopedConfigurationAnalyzer : DurianAnalyzer<DefaultParamCompilationData>
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

		/// <summary>
		/// Analyzes whether the specified <paramref name="configuration"/> is valid.
		/// </summary>
		/// <param name="configuration"><see cref="DefaultParamConfiguration"/> to analyze.</param>
		public static bool AnalyzeConfiguration(DefaultParamConfiguration configuration)
		{
			return IsValidTargetNamespace(configuration.TargetNamespace);
		}

		/// <inheritdoc/>
		public override void Register(IDurianAnalysisContext context, DefaultParamCompilationData compilation)
		{
			context.RegisterSyntaxNodeAction(c => Analyze(c, compilation), SyntaxKind.Attribute);
		}

		/// <inheritdoc/>
		protected override DefaultParamCompilationData CreateCompilation(CSharpCompilation compilation, IDiagnosticReceiver diagnosticReceiver)
		{
			return new DefaultParamCompilationData(compilation);
		}

		private static void Analyze(SyntaxNodeAnalysisContext context, DefaultParamCompilationData compilation)
		{
			if (context.Node is not AttributeSyntax syntax)
			{
				return;
			}

			INamedTypeSymbol? configurationAttribute = context.SemanticModel.GetSymbolInfo(syntax).Symbol?.ContainingType;

			if (configurationAttribute is null ||
				!SymbolEqualityComparer.Default.Equals(configurationAttribute, compilation.DefaultParamScopedConfigurationAttribute))
			{
				return;
			}

			ISymbol symbol;

			if (syntax.Parent?.Parent is MemberDeclarationSyntax parent)
			{
				if (context.SemanticModel.GetDeclaredSymbol(parent) is not INamedTypeSymbol type)
				{
					return;
				}

				if (!HasMemberWithDefaultParamAttribute(type, compilation.DefaultParamAttribute!))
				{
					context.ReportDiagnostic(Diagnostic.Create(DefaultParamDiagnostics.DUR0125_ScopedConfigurationShouldNotBePlacedOnATypeWithoutDefaultParamMembers, type.Locations.FirstOrDefault(), type));
				}

				symbol = type;
			}
			else
			{
				if (syntax.Parent is not AttributeListSyntax list || list.Target is null || !list.Target.Identifier.IsKind(SyntaxKind.AssemblyKeyword))
				{
					return;
				}

				symbol = compilation.Compilation.Assembly;
			}

			Diagnostic? diag = GetDiagnosticIfInvalidTargetNamespace(symbol, syntax);

			if (diag is not null)
			{
				context.ReportDiagnostic(diag);
			}
		}

		private static Diagnostic? GetDiagnosticIfInvalidTargetNamespace(ISymbol symbol, AttributeSyntax node)
		{
			const string propertyName = DefaultParamConfigurationAttributeProvider.TargetNamespace;

			if (node.ArgumentList is null || !node.ArgumentList.Arguments.Any())
			{
				return null;
			}

			if (node.GetArgument(propertyName) is AttributeArgumentSyntax argument &&
				symbol.GetAttribute(node) is AttributeData data &&
				data.TryGetNamedArgumentValue(propertyName, out string? value) &&
				!IsValidTargetNamespace(value))
			{
				Location? location = argument.GetLocation();

				return Diagnostic.Create(DefaultParamDiagnostics.DUR0127_InvalidTargetNamespace, location, symbol, value);
			}

			return null;
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

		private static bool IsValidTargetNamespace(string? targetNamespace)
		{
			return
				targetNamespace is null ||
				(AnalysisUtilities.IsValidNamespaceIdentifier(targetNamespace) &&
				targetNamespace != "Durian.Generator");
		}
	}
}
