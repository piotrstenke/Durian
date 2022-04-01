// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Microsoft.CodeAnalysis.CSharp;

namespace Durian.Analysis.Filters
{
	/// <summary>
	/// <see cref="IGeneratorSyntaxFilter"/> that reports diagnostics about the received <see cref="CSharpSyntaxNode"/>s.
	/// </summary>
	public interface IGeneratorSyntaxFilterWithDiagnostics : IGeneratorSyntaxFilter, ISyntaxFilterWithDiagnostics
	{
	}
}
