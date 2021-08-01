// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using Durian.Analysis.Extensions;
using Durian.Configuration;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using static Durian.Analysis.DefaultParam.DefaultParamDiagnostics;

namespace Durian.Analysis.DefaultParam
{
#pragma warning disable RS1001 // Missing diagnostic analyzer attribute.

	/// <summary>
	/// Analyzes the usage of the <see cref="DefaultParamConfigurationAttribute"/>.
	/// </summary>
#if !MAIN_PACKAGE

	[DiagnosticAnalyzer(LanguageNames.CSharp)]
#endif

	public sealed class DefaultParamConfigurationAnalyzer : DurianAnalyzer<DefaultParamCompilationData>
#pragma warning restore RS1001 // Missing diagnostic analyzer attribute.
	{
		/// <inheritdoc/>
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(
			DUR0111_DefaultParamConfigurationAttributeCannotBeAppliedToMembersWithoutDefaultParamAttribute,
			DUR0112_TypeConvetionShouldNotBeUsedOnMembersOtherThanTypes,
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
		protected override DefaultParamCompilationData CreateCompilation(CSharpCompilation compilation)
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

			if (configurationAttribute is null || !SymbolEqualityComparer.Default.Equals(configurationAttribute, compilation.ConfigurationAttribute))
			{
				return;
			}

			if (context.SemanticModel.GetDeclaredSymbol(parent) is not ISymbol symbol)
			{
				return;
			}

			ContextualDiagnosticReceiver<SyntaxNodeAnalysisContext> diagnosticReceiver = DiagnosticReceiverFactory.SyntaxNode(context);

			if (symbol is INamedTypeSymbol t)
			{
				AnalyzeType(diagnosticReceiver, t, compilation, syntax, context.CancellationToken);
			}
			else if (symbol is IMethodSymbol m)
			{
				AnalyzeMethod(diagnosticReceiver, m, compilation, syntax, context.CancellationToken);
			}
			else
			{
				ReportConfig(diagnosticReceiver, symbol, syntax);

				(AttributeArgumentSyntax syntax, string name)[] arguments = GetArguments(syntax);

				ReportIfInvalidTargetNamespace(diagnosticReceiver, symbol, syntax, arguments, context.CancellationToken);

				if (CheckArguments(arguments, nameof(DefaultParamConfigurationAttribute.MethodConvention), out AttributeArgumentSyntax? arg))
				{
					diagnosticReceiver.ReportDiagnostic(DUR0113_MethodConventionShouldNotBeUsedOnMembersOtherThanMethods, arg.GetLocation(), symbol);
				}

				if (CheckArguments(arguments, nameof(DefaultParamConfigurationAttribute.TypeConvention), out arg))
				{
					diagnosticReceiver.ReportDiagnostic(DUR0112_TypeConvetionShouldNotBeUsedOnMembersOtherThanTypes, arg.GetLocation(), symbol);
				}
			}
		}

		private static void AnalyzeMethod(ContextualDiagnosticReceiver<SyntaxNodeAnalysisContext> diagnosticReceiver, IMethodSymbol method, DefaultParamCompilationData compilation, AttributeSyntax node, CancellationToken cancellationToken)
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
					ReportConfig(diagnosticReceiver, method, node);
				}
				else
				{
					ReportIfMethodIsNotDefaultParam(diagnosticReceiver, baseMethod, compilation, node);
				}
			}
			else if (!typeParameters.Any(m => m.HasAttribute(compilation.MainAttribute!)))
			{
				ReportConfig(diagnosticReceiver, method, node);
			}

			if (node.ArgumentList is null)
			{
				return;
			}

			(AttributeArgumentSyntax syntax, string name)[] arguments = GetArguments(node);

			ReportIfInvalidTargetNamespace(diagnosticReceiver, method, node, arguments, cancellationToken);

			if (CheckArguments(arguments, nameof(DefaultParamConfigurationAttribute.TypeConvention), out AttributeArgumentSyntax? arg))
			{
				diagnosticReceiver.ReportDiagnostic(DUR0112_TypeConvetionShouldNotBeUsedOnMembersOtherThanTypes, arg.GetLocation(), method);
			}
		}

		private static void AnalyzeType(
			ContextualDiagnosticReceiver<SyntaxNodeAnalysisContext> diagnosticReceiver,
			INamedTypeSymbol type,
			DefaultParamCompilationData compilation,
			AttributeSyntax node,
			CancellationToken cancellationToken
		)
		{
			if (!type.TypeParameters.Any(t => t.HasAttribute(compilation.MainAttribute!)))
			{
				ReportConfig(diagnosticReceiver, type, node);
			}

			if (node.ArgumentList is null)
			{
				return;
			}

			(AttributeArgumentSyntax syntax, string name)[] arguments = GetArguments(node);

			ReportIfInvalidTargetNamespace(diagnosticReceiver, type, node, arguments, cancellationToken);

			if (type.ContainingType is null && CheckArguments(arguments, nameof(DefaultParamConfigurationAttribute.ApplyNewModifierWhenPossible), out AttributeArgumentSyntax? arg))
			{
				diagnosticReceiver.ReportDiagnostic(DUR0124_ApplyNewModifierShouldNotBeUsedWhenIsNotChildOfType, arg.GetLocation(), type);
			}

			if (CheckArguments(arguments, nameof(DefaultParamConfigurationAttribute.MethodConvention), out arg))
			{
				diagnosticReceiver.ReportDiagnostic(DUR0113_MethodConventionShouldNotBeUsedOnMembersOtherThanMethods, arg.GetLocation(), type);
			}

			const string propertyName = nameof(DefaultParamConfigurationAttribute.TypeConvention);

			if (type.TypeKind == TypeKind.Delegate)
			{
				if (CheckArguments(arguments, propertyName, out arg))
				{
					diagnosticReceiver.ReportDiagnostic(DUR0112_TypeConvetionShouldNotBeUsedOnMembersOtherThanTypes, arg.GetLocation(), type);
				}
			}
			else if (type.TypeKind == TypeKind.Struct || type.IsSealed || type.IsStatic)
			{
				if (CheckArguments(arguments, propertyName, out arg) &&
					type.GetAttribute(node, cancellationToken) is AttributeData attr &&
					attr.TryGetNamedArgumentValue(propertyName, out int value)
				)
				{
					DPTypeConvention convention = (DPTypeConvention)value;

					if (convention == DPTypeConvention.Inherit)
					{
						diagnosticReceiver.ReportDiagnostic(DUR0117_InheritTypeConventionCannotBeUsedOnStructOrSealedType, arg.GetLocation(), type);
					}
				}
			}
			else if (type.TypeKind == TypeKind.Class)
			{
				if (CheckArguments(arguments, propertyName, out arg) &&
					!type.InstanceConstructors.Any(ctor => ctor.DeclaredAccessibility >= Accessibility.Protected) &&
					type.GetAttribute(node, cancellationToken) is AttributeData attr &&
					attr.TryGetNamedArgumentValue(propertyName, out int value)
				)
				{
					DPTypeConvention convention = (DPTypeConvention)value;

					if (convention == DPTypeConvention.Inherit)
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
			ContextualDiagnosticReceiver<SyntaxNodeAnalysisContext> diagnosticReceiver,
			ISymbol symbol,
			AttributeSyntax node
		)
		{
			diagnosticReceiver.ReportDiagnostic(DUR0111_DefaultParamConfigurationAttributeCannotBeAppliedToMembersWithoutDefaultParamAttribute, node.GetLocation(), symbol);
		}

		private static void ReportIfInvalidTargetNamespace(
			ContextualDiagnosticReceiver<SyntaxNodeAnalysisContext> diagnosticReceiver,
			ISymbol symbol,
			AttributeSyntax node,
			(AttributeArgumentSyntax, string)[] arguments,
			CancellationToken cancellationToken
		)
		{
			const string propertyName = nameof(DefaultParamConfigurationAttribute.TargetNamespace);

			if (CheckArguments(arguments, propertyName, out AttributeArgumentSyntax? arg) &&
				symbol.GetAttribute(node, cancellationToken) is AttributeData data &&
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
			ContextualDiagnosticReceiver<SyntaxNodeAnalysisContext> diagnosticReceiver,
			IMethodSymbol method,
			DefaultParamCompilationData compilation,
			AttributeSyntax node
		)
		{
			if (!method.TypeParameters.Any(m => m.HasAttribute(compilation.MainAttribute!)))
			{
				ReportConfig(diagnosticReceiver, method, node);
			}
		}
	}
}
