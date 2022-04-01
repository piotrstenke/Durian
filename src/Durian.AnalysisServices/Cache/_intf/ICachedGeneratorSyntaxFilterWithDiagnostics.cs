// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Durian.Analysis.Filters;
using Microsoft.CodeAnalysis.CSharp;

namespace Durian.Analysis.Cache
{
	/// <summary>
	/// <see cref="ICachedGeneratorSyntaxFilter{T}"/> that reports diagnostics about the received <see cref="CSharpSyntaxNode"/>s.
	/// </summary>
	/// <typeparam name="T">Type of values this syntax filter can retrieve from a <see cref="CachedGeneratorExecutionContext{T}"/>.</typeparam>
	public interface ICachedGeneratorSyntaxFilterWithDiagnostics<T> : ICachedGeneratorSyntaxFilter<T>, IGeneratorSyntaxFilterWithDiagnostics
	{
	}
}
