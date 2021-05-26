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

			ITypeSymbol? targetType = await DUR0108_MakeValueTheSameAsBaseMethod.GetTargetTypeAsync(data.Document, data.Node, context.CancellationToken).ConfigureAwait(false);

			if (targetType is null)
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

			return CodeAction.Create(Title, cancenllationToken => ExecuteAsync(CodeFixExecutionContext<TypeParameterSyntax>.From(diagnostic, document, semanticModel, root, node!, cancenllationToken), targetType, attribute), Id);
		}

		private static async Task<Document> ExecuteAsync(CodeFixExecutionContext<TypeParameterSyntax> context, ITypeSymbol targetType, INamedTypeSymbol attribute)
		{
			SemanticModel? semanticModel = await context.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false);

			NameSyntax attributeName;

			if (CodeFixUtility.HasUsingDirective(semanticModel!, context.Root.Usings, attribute, context.CancellationToken))
			{
				attributeName = SyntaxFactory.IdentifierName(attribute.Name);
			}
			else
			{
				attributeName = SyntaxFactory.ParseName(attribute.ToString());
			}

			AttributeSyntax attr = SyntaxFactory.Attribute(attributeName);
			TypeParameterSyntax parameter = context.Node.AddAttributeLists(SyntaxFactory.AttributeList(SyntaxFactory.SingletonSeparatedList(attr)));

			context.RegisterChange(context.Node, parameter);

			return DUR0108_MakeValueTheSameAsBaseMethod.Execute(context.WithNode(attr), targetType, semanticModel!);
		}
	}
}
