using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using Durian.Configuration;
using Durian.Generator.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using static Durian.Generator.DefaultParam.DefaultParamDiagnostics;

namespace Durian.Generator.DefaultParam
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
			DUR0112_TypeConvetionShouldNotBeUsedOnMembersOtherThanTypes,
			DUR0113_MethodConventionShouldNotBeUsedOnMembersOtherThanMethods,
			DUR0115_DefaultParamConfigurationIsNotValidOnThisTypeOfMethod,
			DUR0117_InheritTypeConventionCannotBeUsedOnStructOrSealedType,
			DUR0123_InheritTypeConventionCannotBeUsedOnTypeWithNoAccessibleConstructor
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
			if (context.Node is not AttributeSyntax syntax || syntax.Parent?.Parent is not CSharpSyntaxNode node)
			{
				return;
			}

			INamedTypeSymbol? configurationAttribute = context.SemanticModel.GetSymbolInfo(syntax).Symbol?.ContainingType;

			if (configurationAttribute is null || !SymbolEqualityComparer.Default.Equals(configurationAttribute, compilation.ConfigurationAttribute))
			{
				return;
			}

			ISymbol? symbol = context.SemanticModel.GetDeclaredSymbol(node);

			if (symbol is null)
			{
				return;
			}

			_diagnosticReceiver.SetContext(in context);

			if (symbol is INamedTypeSymbol t)
			{
				AnalyzeType(t, compilation, syntax, context.CancellationToken);
			}
			else if (symbol is IMethodSymbol m)
			{
				AnalyzeMethod(m, compilation, syntax);
			}
			else
			{
				ReportConfig(symbol, syntax);

				(AttributeArgumentSyntax syntax, string name)[] arguments = GetArguments(syntax);

				if (CheckArguments(arguments, nameof(DefaultParamConfigurationAttribute.MethodConvention), out AttributeArgumentSyntax? arg))
				{
					_diagnosticReceiver.ReportDiagnostic(DUR0113_MethodConventionShouldNotBeUsedOnMembersOtherThanMethods, arg.GetLocation(), symbol);
				}

				if (CheckArguments(arguments, nameof(DefaultParamConfigurationAttribute.TypeConvention), out arg))
				{
					_diagnosticReceiver.ReportDiagnostic(DUR0112_TypeConvetionShouldNotBeUsedOnMembersOtherThanTypes, arg.GetLocation(), symbol);
				}
			}
		}

		private void AnalyzeType(INamedTypeSymbol type, DefaultParamCompilationData compilation, AttributeSyntax node, CancellationToken cancellationToken)
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

			if (CheckArguments(arguments, nameof(DefaultParamConfigurationAttribute.MethodConvention), out AttributeArgumentSyntax? arg))
			{
				_diagnosticReceiver.ReportDiagnostic(DUR0113_MethodConventionShouldNotBeUsedOnMembersOtherThanMethods, arg.GetLocation(), type);
			}

			const string propertyName = nameof(DefaultParamConfigurationAttribute.TypeConvention);

			if (type.TypeKind == TypeKind.Delegate)
			{
				if (CheckArguments(arguments, propertyName, out arg))
				{
					_diagnosticReceiver.ReportDiagnostic(DUR0112_TypeConvetionShouldNotBeUsedOnMembersOtherThanTypes, arg.GetLocation(), type);
				}
			}
			else if (type.TypeKind == TypeKind.Struct || type.IsSealed)
			{
				if (CheckArguments(arguments, propertyName, out arg) &&
					type.GetAttributeData(node, cancellationToken) is AttributeData attr &&
					attr.TryGetNamedArgumentValue(propertyName, out int value)
				)
				{
					DPTypeConvention convention = (DPTypeConvention)value;

					if (convention == DPTypeConvention.Inherit)
					{
						_diagnosticReceiver.ReportDiagnostic(DUR0117_InheritTypeConventionCannotBeUsedOnStructOrSealedType, arg.GetLocation(), type);
					}
				}
			}
			else if (type.TypeKind == TypeKind.Class)
			{
				if (CheckArguments(arguments, propertyName, out arg) &&
					!type.InstanceConstructors.Any(ctor => ctor.DeclaredAccessibility >= Accessibility.Protected) &&
					type.GetAttributeData(node, cancellationToken) is AttributeData attr &&
					attr.TryGetNamedArgumentValue(propertyName, out int value)
				)
				{
					DPTypeConvention convention = (DPTypeConvention)value;

					if (convention == DPTypeConvention.Inherit)
					{
						_diagnosticReceiver.ReportDiagnostic(DUR0123_InheritTypeConventionCannotBeUsedOnTypeWithNoAccessibleConstructor, arg.GetLocation(), type);
					}
				}
			}
		}

		private void AnalyzeMethod(IMethodSymbol method, DefaultParamCompilationData compilation, AttributeSyntax node)
		{
			if (method.MethodKind != MethodKind.Ordinary || method.ContainingType.TypeKind == TypeKind.Interface)
			{
				_diagnosticReceiver.ReportDiagnostic(DUR0115_DefaultParamConfigurationIsNotValidOnThisTypeOfMethod, node.GetLocation(), method);
				return;
			}

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

			if (CheckArguments(arguments, nameof(DefaultParamConfigurationAttribute.TypeConvention), out AttributeArgumentSyntax? arg))
			{
				_diagnosticReceiver.ReportDiagnostic(DUR0112_TypeConvetionShouldNotBeUsedOnMembersOtherThanTypes, arg.GetLocation(), method);
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
