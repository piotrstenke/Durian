// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Durian.Analysis.CodeGeneration;
using Durian.Analysis.CopyFrom.Methods;
using Durian.Analysis.Extensions;
using Durian.Analysis.SyntaxVisitors;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.Analysis.CopyFrom
{
	public partial class CopyFromGenerator
	{
		private bool GenerateMethod(CopyFromMethodData method, string hintName, CopyFromPassContext context)
		{
			MethodDeclarationSyntax targetMethod = method.Target.Symbol.GetSyntax<MethodDeclarationSyntax>();

			if(targetMethod.Body is not null)
			{
				WriteDeclarationLead(context.CodeBuilder, method, method.Target.Usings);
				WriteGenerationAttributes(method.Target.Symbol, context);
				WriteBlockBody(method, context, targetMethod);
			}
			else if (targetMethod.ExpressionBody is not null)
			{
				WriteDeclarationLead(context.CodeBuilder, method, method.Target.Usings);
				WriteGenerationAttributes(method.Target.Symbol, context);
				WriteExpressionBody(method, context, targetMethod);
			}
			else
			{
				return false;
			}

			context.CodeBuilder.EndAllBlocks();

			AddSourceWithOriginal(method.Declaration, hintName, context);

			return true;
		}

		private static void WriteExpressionBody(CopyFromMethodData method, CopyFromPassContext context, MethodDeclarationSyntax decl)
		{
			ArrowExpressionClauseSyntax expression = (ArrowExpressionClauseSyntax)WriteMethodHead(method, context, decl.Modifiers, decl.ExpressionBody!);
			context.CodeBuilder.Write(" =>");
			context.CodeBuilder.Write(expression.Expression.ToFullString());
			context.CodeBuilder.WriteLine(';');
		}

		private static void WriteBlockBody(CopyFromMethodData method, CopyFromPassContext context, MethodDeclarationSyntax decl)
		{
			BlockSyntax block = (BlockSyntax)WriteMethodHead(method, context, decl.Modifiers, decl.Body!);
			context.CodeBuilder.BeginBlock();

			foreach (StatementSyntax statement in block.Statements)
			{
				context.CodeBuilder.Indent();
				context.CodeBuilder.WriteLine(statement.ToString());
			}
		}

		private static void HandleGenericMethod(IMethodSymbol target, ref SyntaxNode currentNode)
		{
			if(!target.IsGenericMethod)
			{
				return;
			}

			ImmutableArray<ITypeSymbol> arguments = target.TypeArguments;

			List<(string identifier, string replacement)> replacements = new(arguments.Length);
			AddTypeParameterReplacements(target.TypeParameters, arguments, replacements);

			TypeParameterReplacer replacer = new();
			ReplaceTypeParameters(ref currentNode, replacer, replacements);
		}

		private static SyntaxNode WriteMethodHead(CopyFromMethodData method, CopyFromPassContext context, SyntaxTokenList modifiers, SyntaxNode currentNode)
		{
			context.CodeBuilder.Indent();

			if (modifiers.Any(m => SyntaxFacts.IsAccessibilityModifier(m.Kind())))
			{
				context.CodeBuilder.Accessibility(method.Symbol);
			}

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
	}
}
