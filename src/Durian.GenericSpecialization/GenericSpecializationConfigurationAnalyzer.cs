// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Durian.Configuration;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using static Durian.Analysis.GenericSpecialization.GenSpecDiagnostics;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Durian.Analysis.Extensions;
using System.Linq;
using System;

namespace Durian.Analysis.GenericSpecialization
{
#pragma warning disable RS1001 // Missing diagnostic analyzer attribute.

	/// <summary>
	/// Analyzes the usage of the <see cref="GenericSpecializationConfigurationAttribute"/>.
	/// </summary>
#if !MAIN_PACKAGE

	[DiagnosticAnalyzer(LanguageNames.CSharp)]
#endif

	public sealed class GenericSpecializationConfigurationAnalyzer : DurianAnalyzer<GenSpecCompilationData>
#pragma warning restore RS1001 // Missing diagnostic analyzer attribute.
	{
		/// <inheritdoc/>
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(
			DUR0203_SpecifiedNameIsNotAValidIdentifier,
			DUR0204_DoNotSpecifyConfigurationAttributeOnMemberWithNoSpecializations,
			DUR0220_CannotForceInheritSealedClass
		);

		/// <summary>
		/// Initializes a new instance of the <see cref="GenericSpecializationConfigurationAnalyzer"/> class.
		/// </summary>
		public GenericSpecializationConfigurationAnalyzer()
		{
		}

		/// <inheritdoc/>
		public override void Register(IDurianAnalysisContext context, GenSpecCompilationData compilation)
		{
			context.RegisterSyntaxNodeAction(context => Analyze(context, compilation), SyntaxKind.Attribute);
		}

		/// <inheritdoc/>
		protected override GenSpecCompilationData CreateCompilation(CSharpCompilation compilation)
		{
			return new GenSpecCompilationData(compilation);
		}

		private static void AnalayzeAssemblyAttribute(AttributeSyntax attribute, GenSpecCompilationData compilation, SyntaxNodeAnalysisContext context)
		{
			if (compilation.Compilation.Assembly.GetAttributeData(attribute) is not AttributeData data)
			{
				return;
			}

			(AttributeArgumentSyntax, string)[] arguments = GetArguments(attribute);
			AnalyzeStringProperties(compilation.Compilation.Assembly, attribute, data, arguments, context);
		}

		private static void Analyze(SyntaxNodeAnalysisContext context, GenSpecCompilationData compilation)
		{
			if (context.Node is not AttributeSyntax syntax || syntax.Parent is not AttributeListSyntax list)
			{
				return;
			}

			if (list.Parent is ClassDeclarationSyntax decl)
			{
				if (!IsConfiguration(context.SemanticModel))
				{
					return;
				}

				AnalyzeClassAttribute(decl, syntax, compilation, context);
			}
			else if (list.Parent is CompilationUnitSyntax && list.Target is not null && list.Target.Identifier.IsKind(SyntaxKind.AssemblyKeyword))
			{
				if (!IsConfiguration(context.SemanticModel))
				{
					return;
				}

				AnalayzeAssemblyAttribute(syntax, compilation, context);
			}

			bool IsConfiguration(SemanticModel semanticModel)
			{
				INamedTypeSymbol? configurationAttribute = semanticModel.GetSymbolInfo(syntax).Symbol?.ContainingType;

				return configurationAttribute is not null && SymbolEqualityComparer.Default.Equals(configurationAttribute, compilation.ConfigurationAttribute);
			}
		}

		private static void AnalyzeClassAttribute(ClassDeclarationSyntax declaration, AttributeSyntax attr, GenSpecCompilationData compilation, SyntaxNodeAnalysisContext context)
		{
			if (context.SemanticModel.GetDeclaredSymbol(declaration) is not INamedTypeSymbol symbol || symbol.TypeKind != TypeKind.Class)
			{
				return;
			}

			if (symbol.GetAttributeData(attr) is not AttributeData data)
			{
				return;
			}

			if (symbol.TypeParameters.Length > 0 && symbol.HasAttribute(compilation.AllowSpecializationAttribute!))
			{
				if (AnalyzeForceInherit(data, attr, symbol) is Diagnostic diagnostic)
				{
					context.ReportDiagnostic(diagnostic);
				}
			}
			else
			{
				if (!symbol.GetMembers().Any(member => member.HasAttribute(compilation.AllowSpecializationAttribute!)))
				{
					context.ReportDiagnostic(Diagnostic.Create(DUR0204_DoNotSpecifyConfigurationAttributeOnMemberWithNoSpecializations, attr.Name.GetLocation(), symbol));
				}
			}
		}

		private static Diagnostic? AnalyzeForceInherit(AttributeData data, AttributeSyntax syntax, INamedTypeSymbol symbol)
		{
			const string propertyName = nameof(GenericSpecializationConfigurationAttribute.ForceInherit);

			if (symbol.IsSealed && data.TryGetNamedArgumentValue(propertyName, out bool value) && value)
			{
				foreach ((AttributeArgumentSyntax arg, string name) in GetArguments(syntax))
				{
					if (name == propertyName)
					{
						return Diagnostic.Create(DUR0220_CannotForceInheritSealedClass, arg.GetLocation(), symbol);
					}
				}

				return Diagnostic.Create(DUR0220_CannotForceInheritSealedClass, syntax.GetLocation(), symbol);
			}

			return null;
		}

		private static void AnalyzeStringProperties(ISymbol symbol, AttributeSyntax attribute, AttributeData data, (AttributeArgumentSyntax, string)[] arguments, SyntaxNodeAnalysisContext context)
		{
			Diagnostic? intf = GetDiagnosticIfInvalidStringArgument(symbol, attribute, data, arguments, nameof(GenericSpecializationConfigurationAttribute.InterfaceName));
			Diagnostic? templ = GetDiagnosticIfInvalidStringArgument(symbol, attribute, data, arguments, nameof(GenericSpecializationConfigurationAttribute.TemplateName));

			if (intf is not null)
			{
				context.ReportDiagnostic(intf);
			}

			if (templ is not null)
			{
				context.ReportDiagnostic(templ);
			}
		}

		private static (AttributeArgumentSyntax syntax, string name)[] GetArguments(AttributeSyntax node)
		{
			return node.ArgumentList!.Arguments
				.Where(arg => arg.NameEquals is not null)
				.Select(arg => (arg, arg.NameEquals!.Name.ToString()))
				.ToArray();
		}

		private static Diagnostic? GetDiagnosticIfInvalidStringArgument(ISymbol symbol, AttributeSyntax node, AttributeData attr, (AttributeArgumentSyntax syntax, string name)[] arguments, string propertyName)
		{
			if (attr.TryGetNamedArgumentValue(propertyName, out string? value) && !AnalysisUtilities.IsValidIdentifier(propertyName))
			{
				foreach ((AttributeArgumentSyntax syntax, string name) in arguments)
				{
					if (name == propertyName)
					{
						return Diagnostic.Create(DUR0203_SpecifiedNameIsNotAValidIdentifier, syntax.GetLocation(), symbol, value);
					}
				}

				return Diagnostic.Create(DUR0203_SpecifiedNameIsNotAValidIdentifier, node.GetLocation(), symbol, value);
			}

			return null;
		}
	}
}
