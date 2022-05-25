// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Durian.Analysis.CodeGeneration;
using Durian.Analysis.CopyFrom.Methods;
using Durian.Analysis.Extensions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.Analysis.CopyFrom
{
	public partial class CopyFromGenerator
	{
		private bool GenerateMethod(CopyFromMethodData method, string hintName, CopyFromPassContext context)
		{
			CSharpSyntaxNode targetMethod = method.Target.Symbol.GetSyntax<CSharpSyntaxNode>();

			if(targetMethod.GetBlock() is BlockSyntax block)
			{
				WriteDeclarationLead(context.CodeBuilder, method, method.Target.Usings);
				WriteGenerationAttributes(method.Target.Symbol, context);
				WriteBlockBody(method, context, block);
			}
			else if (targetMethod.GetExpressionBody() is ArrowExpressionClauseSyntax expression)
			{
				WriteDeclarationLead(context.CodeBuilder, method, method.Target.Usings);
				WriteGenerationAttributes(method.Target.Symbol, context);
				WriteExpressionBody(method, context, expression);
			}
			else
			{
				return false;
			}

			context.CodeBuilder.EndAllBlocks();

			AddSourceWithOriginal(method.Declaration, hintName, context);

			return true;
		}

		private static void WriteExpressionBody(CopyFromMethodData method, CopyFromPassContext context, ArrowExpressionClauseSyntax expression)
		{
			context.CodeBuilder.Declaration(method.Symbol, MethodStyle.Expression);
			context.CodeBuilder.Write(expression.Expression.ToFullString());
			context.CodeBuilder.WriteLine(';');
		}

		private static void WriteBlockBody(CopyFromMethodData method, CopyFromPassContext context, BlockSyntax block)
		{
			context.CodeBuilder.Declaration(method.Symbol, MethodStyle.Block);

			foreach (StatementSyntax statement in block.Statements)
			{
				context.CodeBuilder.Write(statement.ToFullString());
			}
		}
	}
}
