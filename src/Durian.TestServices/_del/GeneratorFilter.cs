using Durian.Analysis.Filtering;
using Microsoft.CodeAnalysis;

namespace Durian.TestServices;

/// <summary>
/// A delegate that accepts a <see cref="FilterGroup{TFilter}"/> and <see cref="GeneratorExecutionContext"/>.
/// </summary>
/// <typeparam name="TFilter">Type of <see cref="IGeneratorSyntaxFilter"/> the <paramref name="filterGroup"/> can store.</typeparam>
/// <param name="filterGroup">Target <see cref="FilterGroup{TFilter}"/>.</param>
/// <param name="context">The <see cref="GeneratorExecutionContext"/> to be used when performing the action.</param>
public delegate void GeneratorFilter<TFilter>(IFilterGroup<TFilter> filterGroup, GeneratorExecutionContext context) where TFilter : IGeneratorSyntaxFilter;
