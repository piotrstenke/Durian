// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.Analysis.Data
{
	internal interface IDeclaratorProperties
	{
		int? Index { get; set; }
		VariableDeclaratorSyntax? Variable { get; set; }
	}
}
