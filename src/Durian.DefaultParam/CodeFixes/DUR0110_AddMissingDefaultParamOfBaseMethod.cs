using System.Threading.Tasks;
using Durian.Generator.CodeFixes;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.Generator.DefaultParam.CodeFixes
{
	/// <summary>
	/// Code fox for the <see cref="DefaultParamDiagnostics.DUR0110_OverriddenDefaultParamAttribuetShouldBeAddedForClarity"/> diagnostic.
	/// </summary>
	[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(DUR0110_AddMissingDefaultParamOfBaseMethod))]
	public sealed class DUR0110_AddMissingDefaultParamOfBaseMethod : DurianCodeFixBase
	{
		/// <inheritdoc/>
		public override string Title => "Add missing DefaultParamAttribute";

		/// <inheritdoc/>
		public override string Id => Title + " [DefaultParam]";

		/// <summary>
		/// Creates a new instance of the <see cref="DUR0110_AddMissingDefaultParamOfBaseMethod"/> class.
		/// </summary>
		public DUR0110_AddMissingDefaultParamOfBaseMethod()
		{
		}

		/// <inheritdoc/>
		protected override DiagnosticDescriptor[] GetSupportedDiagnostics()
		{
			return new DiagnosticDescriptor[] { DefaultParamDiagnostics.DUR0110_OverriddenDefaultParamAttribuetShouldBeAddedForClarity };
		}

		/// <inheritdoc/>
		public override async Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			CodeFixData<TypeParameterSyntax> data = await CodeFixData<TypeParameterSyntax>.FromAsync(context, true).ConfigureAwait(false);

			if (!data.Success || !data.HasNode || !data.HasSemanticModel)
			{
				return;
			}

			if(data.SemanticModel.GetDeclaredSymbol(data.Node, data.CancellationToken) is not ITypeParameterSymbol typeParameter ||
				DUR0108_MakeValueTheSameAsBaseMethod.GetTargetType(data.Node, data.SemanticModel, typeParameter.Ordinal, data.CancellationToken) is not ITypeSymbol targetType)
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

		private CodeAction? GetCodeAction(in CodeFixData<TypeParameterSyntax> data, ITypeSymbol targetType)
		{
			if (!data.HasSemanticModel)
			{
				return null;
			}

			SemanticModel semanticModel = data.SemanticModel;
			INamedTypeSymbol? attribute = semanticModel.Compilation.GetTypeByMetadataName(typeof(DefaultParamAttribute).ToString());

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

		private static Task<Document> ExecuteAsync(CodeFixExecutionContext<TypeParameterSyntax> context, ITypeSymbol targetType, INamedTypeSymbol attribute)
		{
			INamespaceSymbol? @namespace = (context.SemanticModel.GetDeclaredSymbol(context.Node)?.ContainingNamespace) ?? context.Compilation.GlobalNamespace;

			NameSyntax attributeName = CodeFixUtility.GetNameSyntaxForAttribute(context.SemanticModel, context.Root.Usings, @namespace, attribute, context.CancellationToken);

			TypeParameterSyntax parameter = context.Node.AddAttributeLists(SyntaxFactory.AttributeList(SyntaxFactory.SingletonSeparatedList(SyntaxFactory.Attribute(attributeName))));

			context.RegisterChange(parameter);

			AttributeSyntax attr = context.Node.AttributeLists.Last().Attributes[0];

			return Task.FromResult(DUR0108_MakeValueTheSameAsBaseMethod.Execute(context.WithNode(attr), targetType));
		}
	}
}
