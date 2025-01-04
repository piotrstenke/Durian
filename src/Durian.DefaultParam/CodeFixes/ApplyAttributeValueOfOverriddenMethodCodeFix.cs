// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System.Threading.Tasks;
using Durian.Analysis.CodeFixes;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.Analysis.DefaultParam.CodeFixes
{
	/// <summary>
	/// Code fox for the <see cref="DefaultParamDiagnostics.DUR0110_OverriddenDefaultParamAttributeShouldBeAddedForClarity"/> diagnostic.
	/// </summary>
	[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ApplyAttributeValueOfOverriddenMethodCodeFix))]
	public sealed class ApplyAttributeValueOfOverriddenMethodCodeFix : DurianCodeFixBase
	{
		/// <inheritdoc/>
		public override string Id => Title + " [DefaultParam]";

		/// <inheritdoc/>
		public override string Title => "Add missing DefaultParamAttribute";

		/// <summary>
		/// Creates a new instance of the <see cref="ApplyAttributeValueOfOverriddenMethodCodeFix"/> class.
		/// </summary>
		public ApplyAttributeValueOfOverriddenMethodCodeFix()
		{
		}

		/// <inheritdoc/>
		public override async Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			CodeFixData<TypeParameterSyntax> data = await CodeFixData<TypeParameterSyntax>.FromAsync(context, true).ConfigureAwait(false);

			if (!data.Success || !data.HasNode || !data.HasSemanticModel)
			{
				return;
			}

			if (data.SemanticModel.GetDeclaredSymbol(data.Node, data.CancellationToken) is not ITypeParameterSymbol typeParameter ||
				MakeValueOfOverriddenAttributeEquivalentCodeFix.GetTargetType(data.Node, data.SemanticModel, typeParameter.Ordinal, data.CancellationToken) is not ITypeSymbol targetType)
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

		/// <inheritdoc/>
		protected override DiagnosticDescriptor[] GetSupportedDiagnostics()
		{
			return new DiagnosticDescriptor[] { DefaultParamDiagnostics.DUR0110_OverriddenDefaultParamAttributeShouldBeAddedForClarity };
		}

		private static Task<Document> ExecuteAsync(CodeFixExecutionContext<TypeParameterSyntax> context, ITypeSymbol targetType, INamedTypeSymbol attribute)
		{
			INamespaceSymbol? @namespace = (context.SemanticModel.GetDeclaredSymbol(context.Node)?.ContainingNamespace) ?? context.Compilation.GlobalNamespace;

			NameSyntax attributeName = context.SemanticModel.GetNameSyntaxForAttribute(context.Root.Usings, @namespace, attribute, context.CancellationToken);

			TypeParameterSyntax parameter = context.Node.AddAttributeLists(SyntaxFactory.AttributeList(SyntaxFactory.SingletonSeparatedList(SyntaxFactory.Attribute(attributeName))));

			context.RegisterChange(parameter);

			AttributeSyntax attr = context.Node.AttributeLists.Last().Attributes[0];

			return Task.FromResult(MakeValueOfOverriddenAttributeEquivalentCodeFix.Execute(context.WithNode(attr), targetType));
		}

		private CodeAction? GetCodeAction(in CodeFixData<TypeParameterSyntax> data, ITypeSymbol targetType)
		{
			if (!data.HasSemanticModel)
			{
				return null;
			}

			SemanticModel semanticModel = data.SemanticModel;
			INamedTypeSymbol? attribute = semanticModel.Compilation.GetTypeByMetadataName(DefaultParamAttributeProvider.FullName);

			if (attribute is null)
			{
				return null;
			}

			Document document = data.Document!;
			TypeParameterSyntax node = data.Node!;
			CompilationUnitSyntax root = data.Root!;
			Diagnostic diagnostic = data.Diagnostic!;

			return CodeAction.Create(Title, cancenllationToken => ExecuteAsync(CodeFixExecutionContext<TypeParameterSyntax>.From(diagnostic, document, root, node, semanticModel, cancenllationToken), targetType, attribute), Id);
		}
	}
}
