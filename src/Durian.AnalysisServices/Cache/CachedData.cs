using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;

namespace Durian.Analysis.Cache;

/// <summary>
/// A read-only container of cached data of type <typeparamref name="T"/> associated with a <see cref="FileLinePositionSpan"/>.
/// </summary>
/// <typeparam name="T">Type of values this <see cref="CachedData{T}"/> can store.</typeparam>
public readonly struct CachedData<T>
{
	internal readonly ConcurrentDictionary<FileLinePositionSpan, T> _cached;

	/// <summary>
	/// Returns an instance of <see cref="CachedData{T}"/> that does not contain any data.
	/// </summary>
	public static CachedData<T> Empty => new(null);

	internal CachedData(ConcurrentDictionary<FileLinePositionSpan, T>? dictionary)
	{
		_cached = dictionary ?? new ConcurrentDictionary<FileLinePositionSpan, T>();
	}

	/// <inheritdoc cref="CachedGeneratorExecutionContext{T}.IsCached(FileLinePositionSpan)"/>
	public bool IsCached(FileLinePositionSpan position)
	{
		return _cached.ContainsKey(position);
	}

	/// <inheritdoc cref="CachedGeneratorExecutionContext{T}.TryGetCachedValue(FileLinePositionSpan, out T)"/>
	public bool TryGetCachedValue(FileLinePositionSpan position, [NotNullWhen(true)] out T? value)
	{
		return _cached.TryGetValue(position, out value);
	}
}
