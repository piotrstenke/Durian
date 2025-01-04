using System.Collections.Generic;
using System.Collections.Immutable;
using Durian.Analysis.CopyFrom.Methods;
using Durian.Analysis.Extensions;
using Durian.Analysis.SyntaxVisitors;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.Analysis.CopyFrom
{
	public partial class CopyFromGenerator
	{
		private static void HandleAdditionalNodes(CopyFromMethodData method, TargetMethodData target, SyntaxNode declaration, CopyFromPassContext context)
		{
			if (target.AdditionalNodes.HasFlag(AdditionalNodes.Documentation) && declaration.GetXmlDocumentation() is DocumentationCommentTriviaSyntax doc)
			{
				string documentationText = TryApplyPattern(method, context, doc.ToFullString());
				context.CodeBuilder.Indent();
				context.CodeBuilder.Write(documentationText);
			}

			if (target.AdditionalNodes.HasFlag(AdditionalNodes.Attributes))
			{
				SyntaxList<AttributeListSyntax> attributeLists;

				switch (declaration)
				{
					case BaseMethodDeclarationSyntax decl:
						attributeLists = decl.AttributeLists;
						break;

					case AccessorDeclarationSyntax accessor:
						attributeLists = accessor.AttributeLists;
						break;

					default:
						return;
				}

				TrySkipGeneratorAttributes(ref attributeLists, method.SemanticModel, context);
				string attributeText = HandleAttributeText(method, attributeLists, context);
				context.CodeBuilder.Write(attributeText);
			}
		}

		private static void HandleGenericMethod(IMethodSymbol target, ref SyntaxNode currentNode)
		{
			if (!target.IsGenericMethod)
			{
				return;
			}

			ImmutableArray<ITypeSymbol> arguments = target.TypeArguments;

			List<(string identifier, string replacement)> replacements = new(arguments.Length);
			AddTypeParameterReplacements(target.TypeParameters, arguments, replacements);

			TypeParameterReplacer replacer = new();
			ReplaceTypeParameters(ref currentNode, replacer, replacements);
		}

		private void WriteBlockBody(CopyFromMethodData method, CopyFromPassContext context, BlockSyntax decl)
		{
			BlockSyntax block = (BlockSyntax)WriteMethodHead(method, context, decl);
			WriteGeneratedMember(method, block, method.Symbol, context, CodeGeneration.GenerateDocumentation.Never, false);
		}

		private void WriteExpressionBody(CopyFromMethodData method, CopyFromPassContext context, ArrowExpressionClauseSyntax decl)
		{
			ArrowExpressionClauseSyntax expression = (ArrowExpressionClauseSyntax)WriteMethodHead(method, context, decl);
			WriteGeneratedMember(method, expression, method.Symbol, context, CodeGeneration.GenerateDocumentation.Never, false);
			context.CodeBuilder.Write(';');
			context.CodeBuilder.NewLine();
		}

		private static SyntaxNode WriteMethodHead(CopyFromMethodData method, CopyFromPassContext context, SyntaxNode currentNode)
		{
			context.CodeBuilder.Indent();

			context.CodeBuilder.Accessibility(method.Symbol, true);

			if (method.Symbol.IsStatic)
			{
				context.CodeBuilder.Write("static ");
			}
			else if (method.Symbol.IsReadOnly)
			{
				context.CodeBuilder.Write("readonly ");
			}

			if (method.Symbol.IsExtern)
			{
				context.CodeBuilder.Write("extern ");
			}

			if (method.IsUnsafe)
			{
				context.CodeBuilder.Write("unsafe ");
			}

			context.CodeBuilder.Write("partial ");

			if (method.Symbol.RefKind == RefKind.Ref)
			{
				context.CodeBuilder.Write("ref ");
			}
			else if (method.Symbol.RefKind == RefKind.RefReadOnly)
			{
				context.CodeBuilder.Write("ref readonly ");
			}

			context.CodeBuilder.Write(method.Declaration.ReturnType.ToString()).Space();
			context.CodeBuilder.Name(method, SymbolName.Generic);
			context.CodeBuilder.Write(method.Declaration.ParameterList.ToString());

			if (method.Declaration.ConstraintClauses.Any())
			{
				context.CodeBuilder.Space();
				context.CodeBuilder.Write(method.Declaration.ConstraintClauses.ToString());
			}

			HandleGenericMethod(method.Target.Symbol, ref currentNode);
			return currentNode;
		}

		private bool GenerateMethod(CopyFromMethodData method, string hintName, CopyFromPassContext context)
		{
			SyntaxNode? node;
			SyntaxNode declaration;

			if (method.Target.Symbol.IsAccessor())
			{
				declaration = method.Target.Symbol.GetSyntax<SyntaxNode>();

				switch (declaration)
				{
					case AccessorDeclarationSyntax accessor:
						node = accessor.GetBody();
						break;

					case ArrowExpressionClauseSyntax arrow:
						node = arrow;
						break;

					default:
						return false;
				}
			}
			else
			{
				BaseMethodDeclarationSyntax targetMethod = method.Target.Symbol.GetSyntax<BaseMethodDeclarationSyntax>();
				declaration = targetMethod;
				node = targetMethod.GetBody();
			}

			switch (node)
			{
				case BlockSyntax block:
					BeginDeclaration(method, context, declaration);
					WriteBlockBody(method, context, block);
					break;

				case ArrowExpressionClauseSyntax arrow:
					BeginDeclaration(method, context, declaration);
					WriteExpressionBody(method, context, arrow);
					break;

				default:
					return false;
			}

			context.CodeBuilder.EndAllBlocks();

			AddSourceWithOriginal(method.Declaration, hintName, context);

			return true;

			void BeginDeclaration(CopyFromMethodData method, CopyFromPassContext context, SyntaxNode declaration)
			{
				WriteDeclarationLead(context.CodeBuilder, method, method.Target.Usings);
				HandleAdditionalNodes(method, method.Target, declaration, context);
				WriteGenerationAttributes(method.Target.Symbol.ConstructedFrom, context);
			}
		}
	}
}
