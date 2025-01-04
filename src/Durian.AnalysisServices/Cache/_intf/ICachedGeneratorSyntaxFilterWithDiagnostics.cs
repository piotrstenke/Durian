using Durian.Analysis.Filtering;
using Microsoft.CodeAnalysis;

namespace Durian.Analysis.Cache;

/// <summary>
/// <see cref="ICachedGeneratorSyntaxFilter{T}"/> that reports diagnostics about the received <see cref="SyntaxNode"/>s.
/// </summary>
/// <typeparam name="T">Type of values this syntax filter can retrieve from a <see cref="CachedGeneratorExecutionContext{T}"/>.</typeparam>
public interface ICachedGeneratorSyntaxFilterWithDiagnostics<T> : ICachedGeneratorSyntaxFilter<T>, IGeneratorSyntaxFilterWithDiagnostics
{
}
