// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Durian.Analysis.CopyFrom.Methods;

namespace Durian.Analysis.CopyFrom
{
	public partial class CopyFromGenerator
	{
		private bool GenerateMethod(CopyFromMethodData method, string hintName, CopyFromPassContext context)
		{
			context.CodeBuilder.WriteDeclarationLead(method, method.Target.Usings);

			context.CodeBuilder.Indent();
			//context.CodeBuilder.BeginMethodDeclaration(method, MethodBody.Block);
			context.CodeBuilder.EndAllBlocks();

			AddSourceWithOriginal(method.Declaration, hintName, context);

			return true;
		}
	}
}
