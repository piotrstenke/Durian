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
using static Durian.Analysis.GenericSpecialization.GenSpecDiagnostics;

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
			DUR0222_TargetNameCannotBeTheSameAsContainingClass
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

			Diagnostic? intf = GetDiagnosticIfInvalidIdentifier(compilation.Compilation.Assembly, attribute, data, arguments, nameof(GenericSpecializationConfigurationAttribute.InterfaceName));
			Diagnostic? templ = GetDiagnosticIfInvalidIdentifier(compilation.Compilation.Assembly, attribute, data, arguments, nameof(GenericSpecializationConfigurationAttribute.TemplateName));

			if (intf is not null)
			{
				context.ReportDiagnostic(intf);
			}

			if (templ is not null)
			{
				context.ReportDiagnostic(templ);
			}
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

			(AttributeArgumentSyntax, string)[] arguments = GetArguments(attr);

			Diagnostic? intf = GetDiagnosticIfInvalidIdentifierOrSameAsContainingClass(symbol, attr, data, arguments, nameof(GenericSpecializationConfigurationAttribute.InterfaceName));
			Diagnostic? templ = GetDiagnosticIfInvalidIdentifierOrSameAsContainingClass(symbol, attr, data, arguments, nameof(GenericSpecializationConfigurationAttribute.TemplateName));

			if (intf is not null)
			{
				context.ReportDiagnostic(intf);
			}

			if (templ is not null)
			{
				context.ReportDiagnostic(templ);
			}

			if (symbol.TypeParameters.Length == 0 || !symbol.HasAttribute(compilation.AllowSpecializationAttribute!))
			{
				if (!symbol.GetMembers().Any(member => member.HasAttribute(compilation.AllowSpecializationAttribute!)))
				{
					context.ReportDiagnostic(Diagnostic.Create(DUR0204_DoNotSpecifyConfigurationAttributeOnMemberWithNoSpecializations, attr.Name.GetLocation(), symbol));
				}
			}
		}

		private static (AttributeArgumentSyntax syntax, string name)[] GetArguments(AttributeSyntax node)
		{
			return node.ArgumentList!.Arguments
				.Where(arg => arg.NameEquals is not null)
				.Select(arg => (arg, arg.NameEquals!.Name.ToString()))
				.ToArray();
		}

		private static Diagnostic? GetDiagnosticIfInvalidIdentifier(ISymbol symbol, AttributeSyntax node, AttributeData attr, (AttributeArgumentSyntax syntax, string name)[] arguments, string propertyName)
		{
			if (attr.TryGetNamedArgumentValue(propertyName, out string? value) && !AnalysisUtilities.IsValidIdentifier(value))
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

		private static Diagnostic? GetDiagnosticIfInvalidIdentifierOrSameAsContainingClass(ISymbol symbol, AttributeSyntax node, AttributeData attr, (AttributeArgumentSyntax syntax, string name)[] arguments, string propertyName)
		{
			if (attr.TryGetNamedArgumentValue(propertyName, out string? value))
			{
				DiagnosticDescriptor? diag = null;

				if (!AnalysisUtilities.IsValidIdentifier(value))
				{
					diag = DUR0203_SpecifiedNameIsNotAValidIdentifier;
				}
				else if (value == symbol.Name)
				{
					diag = DUR0222_TargetNameCannotBeTheSameAsContainingClass;
				}

				if (diag is not null)
				{
					foreach ((AttributeArgumentSyntax syntax, string name) in arguments)
					{
						if (name == propertyName)
						{
							return Diagnostic.Create(diag, syntax.GetLocation(), symbol, value);
						}
					}

					return Diagnostic.Create(diag, node.GetLocation(), symbol, value);
				}
			}

			return null;
		}
	}
}
