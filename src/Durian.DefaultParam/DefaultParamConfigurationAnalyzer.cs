using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Durian.Analysis.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using static Durian.Analysis.DefaultParam.DefaultParamDiagnostics;

namespace Durian.Analysis.DefaultParam;

/// <summary>
/// Analyzes the usage of the <c>Durian.Configuration.DefaultParamConfigurationAttribute</c>.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class DefaultParamConfigurationAnalyzer : DurianAnalyzer<DefaultParamCompilationData>
{
	/// <inheritdoc/>
	public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(
		DUR0111_DefaultParamConfigurationAttributeCannotBeAppliedToMembersWithoutDefaultParamAttribute,
		DUR0112_TypeConventionShouldNotBeUsedOnMembersOtherThanTypes,
		DUR0113_MethodConventionShouldNotBeUsedOnMembersOtherThanMethods,
		DUR0115_DefaultParamConfigurationIsNotValidOnThisTypeOfMethod,
		DUR0117_InheritTypeConventionCannotBeUsedOnStructOrSealedType,
		DUR0123_InheritTypeConventionCannotBeUsedOnTypeWithNoAccessibleConstructor,
		DUR0124_ApplyNewModifierShouldNotBeUsedWhenIsNotChildOfType,
		DUR0127_InvalidTargetNamespace,
		DUR0128_DoNotSpecifyTargetNamespaceForNestedMembers
	);

	/// <summary>
	/// Initializes a new instance of the <see cref="DefaultParamConfigurationAnalyzer"/> class.
	/// </summary>
	public DefaultParamConfigurationAnalyzer()
	{
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
		if (context.Node is not AttributeSyntax syntax || syntax.Parent?.Parent is not CSharpSyntaxNode parent)
		{
			return;
		}

		INamedTypeSymbol? configurationAttribute = context.SemanticModel.GetSymbolInfo(syntax).Symbol?.ContainingType;

		if (configurationAttribute is null || !SymbolEqualityComparer.Default.Equals(configurationAttribute, compilation.DefaultParamConfigurationAttribute))
		{
			return;
		}

		if (context.SemanticModel.GetDeclaredSymbol(parent) is not ISymbol symbol)
		{
			return;
		}

		DiagnosticReceiver.Contextual<SyntaxNodeAnalysisContext> diagnosticReceiver = DiagnosticReceiver.Factory.SyntaxNode(context);

		if (symbol is INamedTypeSymbol t)
		{
			AnalyzeType(t, compilation, syntax, diagnosticReceiver);
		}
		else if (symbol is IMethodSymbol m)
		{
			AnalyzeMethod(m, compilation, syntax, diagnosticReceiver);
		}
		else
		{
			ReportConfig(symbol, syntax, diagnosticReceiver);

			(AttributeArgumentSyntax syntax, string name)[] arguments = GetArguments(syntax);

			ReportIfInvalidTargetNamespace(symbol, syntax, arguments, diagnosticReceiver);

			if (CheckArguments(arguments, DefaultParamConfigurationAttributeProvider.MethodConvention, out AttributeArgumentSyntax? arg))
			{
				diagnosticReceiver.ReportDiagnostic(DUR0113_MethodConventionShouldNotBeUsedOnMembersOtherThanMethods, arg.GetLocation(), symbol);
			}

			if (CheckArguments(arguments, DefaultParamConfigurationAttributeProvider.TypeConvention, out arg))
			{
				diagnosticReceiver.ReportDiagnostic(DUR0112_TypeConventionShouldNotBeUsedOnMembersOtherThanTypes, arg.GetLocation(), symbol);
			}
		}
	}

	private static void AnalyzeMethod(
		IMethodSymbol method,
		DefaultParamCompilationData compilation,
		AttributeSyntax node,
		DiagnosticReceiver.Contextual<SyntaxNodeAnalysisContext> diagnosticReceiver
	)
	{
		if (method.MethodKind != MethodKind.Ordinary || method.ContainingType.TypeKind == TypeKind.Interface)
		{
			diagnosticReceiver.ReportDiagnostic(DUR0115_DefaultParamConfigurationIsNotValidOnThisTypeOfMethod, node.GetLocation(), method);
			return;
		}

		ImmutableArray<ITypeParameterSymbol> typeParameters = method.TypeParameters;

		if (typeParameters.Length == 0)
		{
			IMethodSymbol? baseMethod = method.OverriddenMethod;

			if (baseMethod is null)
			{
				ReportConfig(method, node, diagnosticReceiver);
			}
			else
			{
				ReportIfMethodIsNotDefaultParam(baseMethod, compilation, node, diagnosticReceiver);
			}
		}
		else if (!typeParameters.Any(m => m.HasAttribute(compilation.DefaultParamAttribute!)))
		{
			ReportConfig(method, node, diagnosticReceiver);
		}

		if (node.ArgumentList is null)
		{
			return;
		}

		(AttributeArgumentSyntax syntax, string name)[] arguments = GetArguments(node);

		ReportIfInvalidTargetNamespace(method, node, arguments, diagnosticReceiver);

		if (CheckArguments(arguments, DefaultParamConfigurationAttributeProvider.TypeConvention, out AttributeArgumentSyntax? arg))
		{
			diagnosticReceiver.ReportDiagnostic(DUR0112_TypeConventionShouldNotBeUsedOnMembersOtherThanTypes, arg.GetLocation(), method);
		}
	}

	private static void AnalyzeType(
		INamedTypeSymbol type,
		DefaultParamCompilationData compilation,
		AttributeSyntax node,
		DiagnosticReceiver.Contextual<SyntaxNodeAnalysisContext> diagnosticReceiver
	)
	{
		if (!type.TypeParameters.Any(t => t.HasAttribute(compilation.DefaultParamAttribute!)))
		{
			ReportConfig(type, node, diagnosticReceiver);
		}

		if (node.ArgumentList is null)
		{
			return;
		}

		(AttributeArgumentSyntax syntax, string name)[] arguments = GetArguments(node);

		ReportIfInvalidTargetNamespace(type, node, arguments, diagnosticReceiver);

		if (type.ContainingType is null && CheckArguments(arguments, DefaultParamConfigurationAttributeProvider.ApplyNewModifierWhenPossible, out AttributeArgumentSyntax? arg))
		{
			diagnosticReceiver.ReportDiagnostic(DUR0124_ApplyNewModifierShouldNotBeUsedWhenIsNotChildOfType, arg.GetLocation(), type);
		}

		if (CheckArguments(arguments, DefaultParamConfigurationAttributeProvider.MethodConvention, out arg))
		{
			diagnosticReceiver.ReportDiagnostic(DUR0113_MethodConventionShouldNotBeUsedOnMembersOtherThanMethods, arg.GetLocation(), type);
		}

		const string propertyName = DefaultParamConfigurationAttributeProvider.TypeConvention;

		if (type.TypeKind == TypeKind.Delegate)
		{
			if (CheckArguments(arguments, propertyName, out arg))
			{
				diagnosticReceiver.ReportDiagnostic(DUR0112_TypeConventionShouldNotBeUsedOnMembersOtherThanTypes, arg.GetLocation(), type);
			}
		}
		else if (type.TypeKind == TypeKind.Struct || type.IsSealed || type.IsStatic)
		{
			if (CheckArguments(arguments, propertyName, out arg) &&
				type.GetAttribute(node) is AttributeData attr &&
				attr.TryGetNamedArgumentValue(propertyName, out int value)
			)
			{
				TypeConvention convention = (TypeConvention)value;

				if (convention == TypeConvention.Inherit)
				{
					diagnosticReceiver.ReportDiagnostic(DUR0117_InheritTypeConventionCannotBeUsedOnStructOrSealedType, arg.GetLocation(), type);
				}
			}
		}
		else if (type.TypeKind == TypeKind.Class)
		{
			if (CheckArguments(arguments, propertyName, out arg) &&
				!type.InstanceConstructors.Any(ctor => ctor.DeclaredAccessibility >= Accessibility.Protected) &&
				type.GetAttribute(node) is AttributeData attr &&
				attr.TryGetNamedArgumentValue(propertyName, out int value)
			)
			{
				TypeConvention convention = (TypeConvention)value;

				if (convention == TypeConvention.Inherit)
				{
					diagnosticReceiver.ReportDiagnostic(DUR0123_InheritTypeConventionCannotBeUsedOnTypeWithNoAccessibleConstructor, arg.GetLocation(), type);
				}
			}
		}
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

	private static (AttributeArgumentSyntax syntax, string name)[] GetArguments(AttributeSyntax node)
	{
		return node.ArgumentList!.Arguments
			.Where(arg => arg.NameEquals is not null)
			.Select(arg => (arg, arg.NameEquals!.Name.ToString()))
			.ToArray();
	}

	private static void ReportConfig(
		ISymbol symbol,
		AttributeSyntax node,
		DiagnosticReceiver.Contextual<SyntaxNodeAnalysisContext> diagnosticReceiver
	)
	{
		diagnosticReceiver.ReportDiagnostic(DUR0111_DefaultParamConfigurationAttributeCannotBeAppliedToMembersWithoutDefaultParamAttribute, node.GetLocation(), symbol);
	}

	private static void ReportIfInvalidTargetNamespace(
		ISymbol symbol,
		AttributeSyntax node,
		(AttributeArgumentSyntax, string)[] arguments,
		DiagnosticReceiver.Contextual<SyntaxNodeAnalysisContext> diagnosticReceiver
	)
	{
		const string propertyName = DefaultParamConfigurationAttributeProvider.TargetNamespace;

		if (CheckArguments(arguments, propertyName, out AttributeArgumentSyntax? arg) &&
			symbol.GetAttribute(node) is AttributeData data &&
			data.TryGetNamedArgumentValue(propertyName, out string? value) &&
			value is not null)
		{
			if (value == "Durian.Generator" || !AnalysisUtilities.IsValidNamespaceIdentifier(value))
			{
				diagnosticReceiver.ReportDiagnostic(DUR0127_InvalidTargetNamespace, arg.GetLocation(), symbol, value);
			}

			if (symbol.ContainingType is not null)
			{
				diagnosticReceiver.ReportDiagnostic(DUR0128_DoNotSpecifyTargetNamespaceForNestedMembers, arg.GetLocation(), symbol);
			}
		}
	}

	private static void ReportIfMethodIsNotDefaultParam(
		IMethodSymbol method,
		DefaultParamCompilationData compilation,
		AttributeSyntax node,
		DiagnosticReceiver.Contextual<SyntaxNodeAnalysisContext> diagnosticReceiver
	)
	{
		if (!method.TypeParameters.Any(m => m.HasAttribute(compilation.DefaultParamAttribute!)))
		{
			ReportConfig(method, node, diagnosticReceiver);
		}
	}
}
