using System.Threading;
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

			ITypeSymbol? targetType = await GetTargetTypeAsync(data.Document, data.Node, context.CancellationToken).ConfigureAwait(false);

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

		private CodeAction GetCodeAction(in CodeFixData<AttributeSyntax> data, ITypeSymbol targetType)
		{
			Document document = data.Document!;
			AttributeSyntax node = data.Node!;
			CompilationUnitSyntax root = data.Root!;
			Diagnostic diagnostic = data.Diagnostic!;
			SemanticModel? semanticModel = data.SemanticModel;

			return CodeAction.Create(Title, cancenllationToken => ExecuteAsync(CodeFixExecutionContext<AttributeSyntax>.From(diagnostic, document, semanticModel, root, node!, cancenllationToken), targetType), Id);
		}

		private static async Task<Document> ExecuteAsync(CodeFixExecutionContext<AttributeSyntax> context, ITypeSymbol targetType)
		{
			SemanticModel? semanticModel = await context.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false);

			return Execute(context, targetType, semanticModel!);
		}

		internal static Document Execute(CodeFixExecutionContext<AttributeSyntax> context, ITypeSymbol targetType, SemanticModel semanticModel)
		{
			NameSyntax name;

			if (CodeFixUtility.HasUsingDirective(semanticModel, context.Root.Usings, targetType, context.CancellationToken))
			{
				name = SyntaxFactory.ParseName(targetType.GetGenericName(false));
			}
			else
			{
				name = SyntaxFactory.ParseName(targetType.ToString());
			}

			AttributeSyntax attr = context.Node
				.WithArgumentList(SyntaxFactory.AttributeArgumentList(SyntaxFactory.SingletonSeparatedList(
					SyntaxFactory.AttributeArgument(SyntaxFactory.TypeOfExpression(name)))));

			context.RegisterChangeAndUpdateDocument(context.Node, attr);
			return context.Document;
		}

		internal static async Task<ITypeSymbol?> GetTargetTypeAsync(Document document, CSharpSyntaxNode node, CancellationToken cancellationToken)
		{
			SemanticModel? semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);

			if (semanticModel is null ||
				node.Parent is null ||
				node.Parent.Parent is not MethodDeclarationSyntax method ||
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
				if (m.GetAttributeData(attribute) is AttributeData data)
				{
					return data.GetConstructorArgumentTypeValue<INamedTypeSymbol>(0);
				}
			}

			return null;
		}
	}
}
