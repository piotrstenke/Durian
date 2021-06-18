// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Concurrent;
using Microsoft.CodeAnalysis;

namespace Durian.Analysis.Cache
{
	/// <summary>
	/// Offers functionality equivalent to the <see cref="GeneratorExecutionContext"/> with additional methods that can retrieve cached data.
	/// </summary>
	/// <typeparam name="T">Type of values this context can cache.</typeparam>
	public readonly struct CachedGeneratorExecutionContext<T>
	{
		internal readonly GeneratorExecutionContext _context;
		internal readonly CachedData<T> _data;
		private readonly bool _hasData;

		/// <inheritdoc cref="CachedGeneratorExecutionContext(in GeneratorExecutionContext, ConcurrentDictionary{FileLinePositionSpan, T})"/>
		public CachedGeneratorExecutionContext(in GeneratorExecutionContext context)
		{
			_context = context;
			_data = default;
			_hasData = false;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="CachedGeneratorExecutionContext{T}"/> struct.
		/// </summary>
		/// <param name="context">Original <see cref="GeneratorExecutionContext"/>.</param>
		/// <param name="cached"><see cref="ConcurrentDictionary{TKey, TValue}"/> that contains the cached values.</param>
		public CachedGeneratorExecutionContext(in GeneratorExecutionContext context, ConcurrentDictionary<FileLinePositionSpan, T> cached)
		{
			_context = context;
			_data = new(cached);
			_hasData = true;
		}

		/// <inheritdoc/>
		public static explicit operator GeneratorExecutionContext(in CachedGeneratorExecutionContext<T> context)
		{
			return context._context;
		}

		/// <inheritdoc/>
		public static implicit operator CachedGeneratorExecutionContext<T>(in GeneratorExecutionContext context)
		{
			return new CachedGeneratorExecutionContext<T>(context);
		}

		/// <summary>
		/// Checks if value associated with the specified <paramref name="position"/> is cached in the context.
		/// </summary>
		/// <param name="position"><see cref="FileLinePositionSpan"/> the value is associated with.</param>
		public readonly bool IsCached(FileLinePositionSpan position)
		{
			if (!_hasData)
			{
				return false;
			}

			return _data.IsCached(position);
		}

		/// <summary>
		/// Attempts to return cached <paramref name="value"/> associated with the specified <paramref name="position"/>.
		/// </summary>
		/// <param name="position"><see cref="FileLinePositionSpan"/> the value is assigned to.</param>
		/// <param name="value">Returned cached value.</param>
		public readonly bool TryGetCachedValue(FileLinePositionSpan position, out T? value)
		{
			if (!_hasData)
			{
				value = default;
				return false;
			}

			return _data.TryGetCachedValue(position, out value);
		}
	}
}
