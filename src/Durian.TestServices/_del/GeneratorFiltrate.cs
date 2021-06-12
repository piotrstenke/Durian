// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Durian.Generator;
using Microsoft.CodeAnalysis;

namespace Durian.Tests
{
	/// <summary>
	/// A delegate that accepts a <see cref="FilterGroup{TFilter}"/> and <see cref="GeneratorExecutionContext"/>.
	/// </summary>
	/// <typeparam name="TFilter">Type of <see cref="IGeneratorSyntaxFilter"/> the <paramref name="filterGroup"/> can store.</typeparam>
	/// <param name="filterGroup">Target <see cref="FilterGroup{TFilter}"/>.</param>
	/// <param name="context">The <see cref="GeneratorExecutionContext"/> to be used when performing the action.</param>
	public delegate void GeneratorFiltrate<TFilter>(FilterGroup<TFilter> filterGroup, in GeneratorExecutionContext context) where TFilter : IGeneratorSyntaxFilter;
}
