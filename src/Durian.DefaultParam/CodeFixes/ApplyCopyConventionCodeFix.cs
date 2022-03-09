// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Durian.Analysis.CodeFixes;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Threading.Tasks;

namespace Durian.Analysis.DefaultParam.CodeFixes
{
	/// <summary>
	/// Code fox for the <see cref="DefaultParamDiagnostics.DUR0118_ApplyCopyTypeConventionOnStructOrSealedTypeOrTypeWithNoPublicCtor"/> diagnostic.
	/// </summary>
	[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ApplyCopyConventionCodeFix))]
	public sealed class ApplyCopyConventionCodeFix : DurianCodeFixBase
	{
		/// <inheritdoc/>
		public override string Id => Title + " [DefaultParam]";

		/// <inheritdoc/>
		public override string Title => "Apply DPTypeConvention.Copy";

		/// <summary>
		/// Creates a new instance of the <see cref="ApplyCopyConventionCodeFix"/> class.
		/// </summary>
		public ApplyCopyConventionCodeFix()
		{
		}

		/// <inheritdoc/>
		public override async Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			CodeFixData<TypeDeclarationSyntax> data = await CodeFixData<TypeDeclarationSyntax>.FromAsync(context, true).ConfigureAwait(false);

			if (!data.Success || !data.HasNode || !data.HasSemanticModel)
			{
				return;
			}

			INamedTypeSymbol? attribute = data.SemanticModel.Compilation.GetTypeByMetadataName(DefaultParamConfigurationAttributeProvider.FullName);

			if (attribute is null)
			{
				return;
			}

			CodeAction? action = GetCodeAction(in data, attribute);

			if (action is null)
			{
				return;
			}

			context.RegisterCodeFix(action, data.Diagnostic);
		}

		/// <inheritdoc/>
		protected override DiagnosticDescriptor[] GetSupportedDiagnostics()
		{
			return new DiagnosticDescriptor[]
			{
				DefaultParamDiagnostics.DUR0118_ApplyCopyTypeConventionOnStructOrSealedTypeOrTypeWithNoPublicCtor
			};
		}

		private static Task<Document> ExecuteAsync(CodeFixExecutionContext<TypeDeclarationSyntax> context, INamedTypeSymbol attribute)
		{
			INamespaceSymbol? @namespace = (context.SemanticModel.GetSymbolInfo(context.Node).Symbol?.ContainingNamespace) ?? context.Compilation.GlobalNamespace;

			NameSyntax attrName;
			NameSyntax enumName;

			if (context.SemanticModel.HasUsingDirective(context.Root.Usings, @namespace, attribute, context.CancellationToken))
			{
				attrName = SyntaxFactory.IdentifierName("DefaultParamConfiguration");
				enumName = SyntaxFactory.IdentifierName(nameof(TypeConvention));
			}
			else
			{
				QualifiedNameSyntax n =
					SyntaxFactory.QualifiedName(
							SyntaxFactory.IdentifierName("Durian"),
							SyntaxFactory.IdentifierName("Configuration"));

				attrName = SyntaxFactory.QualifiedName(n, SyntaxFactory.IdentifierName("DefaultParamConfiguration"));
				enumName = SyntaxFactory.QualifiedName(n, SyntaxFactory.IdentifierName(nameof(TypeConvention)));
			}

			TypeDeclarationSyntax type = context.Node.AddAttributeLists(SyntaxFactory.AttributeList(SyntaxFactory.SingletonSeparatedList(
				SyntaxFactory.Attribute(attrName,
					SyntaxFactory.AttributeArgumentList(SyntaxFactory.SingletonSeparatedList(
						SyntaxFactory.AttributeArgument(
							SyntaxFactory.MemberAccessExpression(
								SyntaxKind.SimpleMemberAccessExpression,
								enumName,
								SyntaxFactory.IdentifierName(nameof(TypeConvention.Copy))))
						.WithNameEquals(
							SyntaxFactory.NameEquals(
								SyntaxFactory.IdentifierName(DefaultParamConfigurationAttributeProvider.TypeName),
								SyntaxFactory.Token(SyntaxKind.EqualsToken).WithTrailingTrivia(SyntaxFactory.Space)))))))));

			context.RegisterChange(context.Node, type);
			return Task.FromResult(context.Document);
		}

		private CodeAction GetCodeAction(in CodeFixData<TypeDeclarationSyntax> data, INamedTypeSymbol attribute)
		{
			Document document = data.Document!;
			TypeDeclarationSyntax node = data.Node!;
			CompilationUnitSyntax root = data.Root!;
			Diagnostic diagnostic = data.Diagnostic!;
			SemanticModel semanticModel = data.SemanticModel!;

			return CodeAction.Create(Title, cancenllationToken => ExecuteAsync(CodeFixExecutionContext<TypeDeclarationSyntax>.From(diagnostic, document, root, node!, semanticModel, cancenllationToken), attribute), Id);
		}
	}
}