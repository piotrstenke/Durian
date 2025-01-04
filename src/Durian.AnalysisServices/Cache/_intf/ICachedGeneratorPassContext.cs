using Microsoft.CodeAnalysis;

namespace Durian.Analysis.Cache;

/// <summary>
/// <see cref="IGeneratorPassContext"/> that provides a <see cref="CachedGeneratorExecutionContext{T}"/> instead of <see cref="GeneratorExecutionContext"/>.
/// </summary>
/// <typeparam name="T">Type of values this context can store.</typeparam>
public interface ICachedGeneratorPassContext<T> : IGeneratorPassContext
{
	/// <summary>
	/// <see cref="CachedGeneratorExecutionContext{T}"/> created for the current generator pass.
	/// </summary>
	new ref readonly CachedGeneratorExecutionContext<T> OriginalContext { get; }
}
