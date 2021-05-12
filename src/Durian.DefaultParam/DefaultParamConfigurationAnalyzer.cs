using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Durian.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using static Durian.DefaultParam.DefaultParamDiagnostics;

namespace Durian.DefaultParam
{
	/// <summary>
	/// Analyzes the usage of the <see cref="DefaultParamConfigurationAttribute"/>
	/// </summary>
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public sealed class DefaultParamConfigurationAnalyzer : DurianAnalyzer<DefaultParamCompilationData>
	{
		private readonly ContextualDiagnosticReceiver<SyntaxNodeAnalysisContext> _diagnosticReceiver;

		/// <inheritdoc/>
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(
			DUR0111_DefaultParamConfigurationAttributeCannotBeAppliedToMembersWithoutDefaultParamAttribute,
			DUR0112_TypeConvetionShouldNotBeUsedOnMethodsOrDelegates,
			DUR0113_MethodConvetionShouldNotBeUsedOnMembersOtherThanMethods,
			DUR0114_NewModifierPropertyShouldNotBeUsedOnMembers
		);

		/// <summary>
		/// Initializes a new instance of the <see cref="DefaultParamConfigurationAnalyzer"/> class.
		/// </summary>
		public DefaultParamConfigurationAnalyzer()
		{
			_diagnosticReceiver = DiagnosticReceiverFactory.SyntaxNode();
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

		private void Analyze(SyntaxNodeAnalysisContext context, DefaultParamCompilationData compilation)
		{
			ISymbol? symbol = context.ContainingSymbol;

			if (context.Node is not AttributeSyntax syntax || symbol is null || symbol.Kind is not SymbolKind.Method and not SymbolKind.NamedType)
			{
				return;
			}

			AttributeData? configurationAttribute = symbol.GetAttributeData(compilation.ConfigurationAttribute!);

			if (configurationAttribute is null)
			{
				return;
			}

			_diagnosticReceiver.SetContext(in context);

			if (symbol is INamedTypeSymbol t)
			{
				AnalyzeType(t, compilation, syntax);
			}
			else if (symbol is IMethodSymbol m)
			{
				AnalyzeMethod(m, compilation, syntax);
			}
			else
			{
				ReportConfig(symbol, syntax);

				(AttributeArgumentSyntax syntax, string name)[] arguments = GetArguments(syntax);

				if (CheckArguments(arguments, DefaultParamConfigurationAttribute.ApplyNewModifierWhenPossibleProperty, out AttributeArgumentSyntax? arg))
				{
					_diagnosticReceiver.ReportDiagnostic(DUR0114_NewModifierPropertyShouldNotBeUsedOnMembers, arg.GetLocation(), symbol);
				}

				if (CheckArguments(arguments, DefaultParamConfigurationAttribute.MethodConvetionProperty, out arg))
				{
					_diagnosticReceiver.ReportDiagnostic(DUR0113_MethodConvetionShouldNotBeUsedOnMembersOtherThanMethods, arg.GetLocation(), symbol);
				}

				if (CheckArguments(arguments, DefaultParamConfigurationAttribute.TypeConventionProperty, out arg))
				{
					_diagnosticReceiver.ReportDiagnostic(DUR0112_TypeConvetionShouldNotBeUsedOnMethodsOrDelegates, arg.GetLocation(), symbol);
				}
			}
		}

		private void AnalyzeType(INamedTypeSymbol type, DefaultParamCompilationData compilation, AttributeSyntax node)
		{
			if (!type.TypeParameters.Any(t => t.HasAttribute(compilation.MainAttribute!)))
			{
				ReportConfig(type, node);
			}

			if (node.ArgumentList is null)
			{
				return;
			}

			(AttributeArgumentSyntax syntax, string name)[] arguments = GetArguments(node);

			if (CheckArguments(arguments, DefaultParamConfigurationAttribute.ApplyNewModifierWhenPossibleProperty, out AttributeArgumentSyntax? arg))
			{
				_diagnosticReceiver.ReportDiagnostic(DUR0114_NewModifierPropertyShouldNotBeUsedOnMembers, arg.GetLocation(), type);
			}

			if (CheckArguments(arguments, DefaultParamConfigurationAttribute.MethodConvetionProperty, out arg))
			{
				_diagnosticReceiver.ReportDiagnostic(DUR0113_MethodConvetionShouldNotBeUsedOnMembersOtherThanMethods, arg.GetLocation(), type);
			}

			if (type.TypeKind == TypeKind.Delegate && CheckArguments(arguments, DefaultParamConfigurationAttribute.TypeConventionProperty, out arg))
			{
				_diagnosticReceiver.ReportDiagnostic(DUR0112_TypeConvetionShouldNotBeUsedOnMethodsOrDelegates, arg.GetLocation(), type);
			}
		}

		private void AnalyzeMethod(IMethodSymbol method, DefaultParamCompilationData compilation, AttributeSyntax node)
		{
			ImmutableArray<ITypeParameterSymbol> typeParameters = method.TypeParameters;

			if (typeParameters.Length == 0)
			{
				IMethodSymbol? baseMethod = method.OverriddenMethod;

				if (baseMethod is null)
				{
					ReportConfig(method, node);
				}
				else
				{
					ReportIfMethodIsNotDefaultParam(baseMethod, compilation, node);
				}
			}
			else if (!typeParameters.Any(m => m.HasAttribute(compilation.MainAttribute!)))
			{
				ReportConfig(method, node);
			}

			if (node.ArgumentList is null)
			{
				return;
			}

			(AttributeArgumentSyntax syntax, string name)[] arguments = GetArguments(node);

			if (CheckArguments(arguments, DefaultParamConfigurationAttribute.ApplyNewModifierWhenPossibleProperty, out AttributeArgumentSyntax? arg))
			{
				_diagnosticReceiver.ReportDiagnostic(DUR0114_NewModifierPropertyShouldNotBeUsedOnMembers, arg.GetLocation(), method);
			}

			if (CheckArguments(arguments, DefaultParamConfigurationAttribute.TypeConventionProperty, out arg))
			{
				_diagnosticReceiver.ReportDiagnostic(DUR0112_TypeConvetionShouldNotBeUsedOnMethodsOrDelegates, arg.GetLocation(), method);
			}
		}

		private static (AttributeArgumentSyntax syntax, string name)[] GetArguments(AttributeSyntax node)
		{
			return node.ArgumentList!.Arguments
				.Where(arg => arg.NameEquals is not null)
				.Select(arg => (arg, arg.NameEquals!.Name.ToString()))
				.ToArray();
		}

		private static bool CheckArguments((AttributeArgumentSyntax, string)[] arguments, string property, [NotNullWhen(true)] out AttributeArgumentSyntax? arg)
		{
			foreach ((AttributeArgumentSyntax syntax, string name) in arguments)
			{
				if (name == property)
				{
					arg = syntax;
					return true;
				}
			}

			arg = null;
			return false;
		}

		private void ReportIfMethodIsNotDefaultParam(IMethodSymbol method, DefaultParamCompilationData compilation, AttributeSyntax node)
		{
			if (!method.TypeParameters.Any(m => m.HasAttribute(compilation.MainAttribute!)))
			{
				ReportConfig(method, node);
			}
		}

		private void ReportConfig(ISymbol symbol, AttributeSyntax node)
		{
			_diagnosticReceiver.ReportDiagnostic(DUR0111_DefaultParamConfigurationAttributeCannotBeAppliedToMembersWithoutDefaultParamAttribute, node.GetLocation(), symbol);
		}
	}
}
