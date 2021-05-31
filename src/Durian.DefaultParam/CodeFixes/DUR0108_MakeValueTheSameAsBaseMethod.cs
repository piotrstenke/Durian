﻿using System.Threading;
using System.Threading.Tasks;
using Durian.Generator.CodeFixes;
using Durian.Generator.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.Generator.DefaultParam.CodeFixes
{
	/// <summary>
	/// Code fox for the <see cref="DefaultParamDiagnostics.DUR0108_ValueOfOverriddenMethodMustBeTheSameAsBase"/> diagnostic.
	/// </summary>
	[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(DUR0108_MakeValueTheSameAsBaseMethod))]
	public sealed class DUR0108_MakeValueTheSameAsBaseMethod : DurianCodeFixBase
	{
		/// <inheritdoc/>
		public override string Title => "Make DefaultParam value the same as base method";

		/// <inheritdoc/>
		public override string Id => Title + " [DefaultParam]";

		/// <summary>
		/// Creates a new instance of the <see cref="DUR0108_MakeValueTheSameAsBaseMethod"/> class.
		/// </summary>
		public DUR0108_MakeValueTheSameAsBaseMethod()
		{
		}

		/// <inheritdoc/>
		protected override DiagnosticDescriptor[] GetSupportedDiagnostics()
		{
			return new DiagnosticDescriptor[] { DefaultParamDiagnostics.DUR0108_ValueOfOverriddenMethodMustBeTheSameAsBase };
		}

		/// <inheritdoc/>
		public override async Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			CodeFixData<AttributeSyntax> data = await CodeFixData<AttributeSyntax>.FromAsync(context, true).ConfigureAwait(false);

			if (!data.Success || !data.HasNode || !data.HasSemanticModel)
			{
				return;
			}

			if (data.Node.Parent?.Parent is not TypeParameterSyntax typeParameter ||
				data.SemanticModel.GetDeclaredSymbol(typeParameter, data.CancellationToken) is not ITypeParameterSymbol parameterSymbol ||
				GetTargetType(data.Node, data.SemanticModel, parameterSymbol.Ordinal, data.CancellationToken) is not ITypeSymbol targetType)
			{
				return;
			}

			CodeAction? action = GetCodeAction(in data, targetType);

			if (action is null)
			{
				return;
			}

			context.RegisterCodeFix(action, data.Diagnostic);
		}

		private CodeAction GetCodeAction(in CodeFixData<AttributeSyntax> data, ITypeSymbol targetType)
		{
			Document document = data.Document!;
			AttributeSyntax node = data.Node!;
			CompilationUnitSyntax root = data.Root!;
			Diagnostic diagnostic = data.Diagnostic!;
			SemanticModel semanticModel = data.SemanticModel!;

			return CodeAction.Create(Title, cancenllationToken =>
			{
				CodeFixExecutionContext<AttributeSyntax> context = CodeFixExecutionContext<AttributeSyntax>.From(diagnostic, document, root, node, semanticModel, cancenllationToken);

				return Task.FromResult(Execute(context, targetType));
			},
			Id);
		}

		internal static Document Execute(CodeFixExecutionContext<AttributeSyntax> context, ITypeSymbol targetType)
		{
			INamespaceSymbol? @namespace = (context.SemanticModel.GetSymbolInfo(context.Node).Symbol?.ContainingNamespace) ?? context.Compilation.GlobalNamespace;

			NameSyntax name = CodeFixUtility.GetNameSyntax(context.SemanticModel, context.Root.Usings, @namespace, targetType, context.CancellationToken);

			AttributeSyntax attr = context.Node
				.WithArgumentList(SyntaxFactory.AttributeArgumentList(SyntaxFactory.SingletonSeparatedList(
					SyntaxFactory.AttributeArgument(SyntaxFactory.TypeOfExpression(name)))));

			context.RegisterChange(context.Node, attr);
			return context.Document;
		}

		internal static ITypeSymbol? GetTargetType(CSharpSyntaxNode node, SemanticModel semanticModel, int ordinal, CancellationToken cancellationToken)
		{
			MethodDeclarationSyntax? method = node.FirstAncestorOrSelf<MethodDeclarationSyntax>();

			if (method is null ||
				semanticModel.GetDeclaredSymbol(method, cancellationToken) is not IMethodSymbol symbol ||
				symbol.OverriddenMethod is not IMethodSymbol
			)
			{
				return null;
			}

			Compilation compilation = semanticModel.Compilation;
			INamedTypeSymbol? attribute = compilation.GetTypeByMetadataName(typeof(DefaultParamAttribute).ToString());

			if (attribute is null)
			{
				return null;
			}

			foreach (IMethodSymbol m in symbol.GetBaseMethods())
			{
				if (m.TypeParameters[ordinal].GetAttributeData(attribute) is AttributeData data)
				{
					return data.GetConstructorArgumentTypeValue<ITypeSymbol>(0);
				}
			}

			return null;
		}
	}
}