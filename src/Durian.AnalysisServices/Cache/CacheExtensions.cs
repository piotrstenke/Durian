// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Diagnostics.CodeAnalysis;
using Durian.Analysis.Data;
using Durian.Analysis.Filters;
using Microsoft.CodeAnalysis;

namespace Durian.Analysis.Cache
{
	/// <summary>
	/// Contains extension methods for the <see cref="CachedGeneratorExecutionContext{T}"/> struct.
	/// </summary>
	public static class CacheExtensions
	{
		/// <summary>
		/// Checks if value associated with a <see cref="FileLinePositionSpan"/> retrieved from the specified <paramref name="location"/> is cached in the <paramref name="context"/>.
		/// </summary>
		/// <param name="context">Target <see cref="CachedGeneratorExecutionContext{T}"/>.</param>
		/// <param name="location"><see cref="Location"/> to retrieve the <see cref="FileLinePositionSpan"/> from.</param>
		/// <exception cref="ArgumentNullException"><paramref name="location"/> is <see langword="null"/>.</exception>
		public static bool IsCached<T>(this in CachedGeneratorExecutionContext<T> context, Location location)
		{
			if (location is null)
			{
				throw new ArgumentNullException(nameof(location));
			}

			return context.IsCached(location.GetLineSpan());
		}

		/// <summary>
		/// Checks if value associated with a <see cref="FileLinePositionSpan"/> retrieved from the specified <paramref name="node"/> is cached in the <paramref name="context"/>.
		/// </summary>
		/// <param name="context">Target <see cref="CachedGeneratorExecutionContext{T}"/>.</param>
		/// <param name="node"><see cref="SyntaxNode"/> to retrieve the <see cref="FileLinePositionSpan"/> from.</param>
		/// <exception cref="ArgumentNullException"><paramref name="node"/> is <see langword="null"/>.</exception>
		public static bool IsCached<T>(this in CachedGeneratorExecutionContext<T> context, SyntaxNode node)
		{
			if (node is null)
			{
				throw new ArgumentNullException(nameof(node));
			}

			return context.IsCached(node.GetLocation().GetLineSpan());
		}

		/// <summary>
		/// Checks if value associated with a <see cref="FileLinePositionSpan"/> retrieved from the specified <paramref name="location"/> is cached in the <paramref name="cache"/>.
		/// </summary>
		/// <param name="cache">Target <see cref="CachedData{T}"/>.</param>
		/// <param name="location"><see cref="Location"/> to retrieve the <see cref="FileLinePositionSpan"/> from.</param>
		/// <exception cref="ArgumentNullException"><paramref name="location"/> is <see langword="null"/>.</exception>
		public static bool IsCached<T>(this in CachedData<T> cache, Location location)
		{
			if (location is null)
			{
				throw new ArgumentNullException(nameof(location));
			}

			return cache.IsCached(location.GetLineSpan());
		}

		/// <summary>
		/// Checks if value associated with a <see cref="FileLinePositionSpan"/> retrieved from the specified <paramref name="node"/> is cached in the <paramref name="cache"/>.
		/// </summary>
		/// <param name="cache">Target <see cref="CachedData{T}"/>.</param>
		/// <param name="node"><see cref="SyntaxNode"/> to retrieve the <see cref="FileLinePositionSpan"/> from.</param>
		/// <exception cref="ArgumentNullException"><paramref name="node"/> is <see langword="null"/>.</exception>
		public static bool IsCached<T>(this in CachedData<T> cache, SyntaxNode node)
		{
			if (node is null)
			{
				throw new ArgumentNullException(nameof(node));
			}

			return cache.IsCached(node.GetLocation().GetLineSpan());
		}

		/// <summary>
		/// Returns a reference to the internal <see cref="CachedData{T}"/> of the given <paramref name="context"/>.
		/// </summary>
		/// <typeparam name="T">Type of values the <paramref name="context"/> can cache.</typeparam>
		/// <param name="context"><see cref="CachedGeneratorExecutionContext{T}"/> to get the internal <see cref="CachedData{T}"/> of.</param>
		public static ref readonly CachedData<T> GetCachedData<T>(this in CachedGeneratorExecutionContext<T> context)
		{
			return ref context._data;
		}

#pragma warning disable RCS1242 // Do not pass non-read-only struct by read-only reference.

		/// <summary>
		/// Returns a reference to the internal <see cref="CachedData{T}"/> of the given <paramref name="enumerator"/>.
		/// </summary>
		/// <typeparam name="TData">Type of cached data.</typeparam>
		/// <typeparam name="TContext">Type of target <see cref="ISyntaxValidatorContext"/>.</typeparam>
		/// <param name="enumerator"><see cref="CachedFilterEnumerator{TData, TContext}"/> to get the internal <see cref="CachedData{T}"/> of.</param>
		public static ref readonly CachedData<TData> GetCachedData<TData, TContext>(this in CachedFilterEnumerator<TData, TContext> enumerator)
			where TData : class, IMemberData
			where TContext : ISyntaxValidatorContext
		{
			return ref enumerator._cache;
		}

		/// <summary>
		/// Returns a reference to the internal <see cref="CachedData{T}"/> of the given <paramref name="enumerator"/>.
		/// </summary>
		/// <typeparam name="TData">Type of cached data.</typeparam>
		/// <typeparam name="TContext">Type of target <see cref="ISyntaxValidatorContext"/>.</typeparam>
		/// <param name="enumerator"><see cref="CachedFilterEnumeratorWithDiagnostics{TData, TContext}"/> to get the internal <see cref="CachedData{T}"/> of.</param>
		public static ref readonly CachedData<TData> GetCachedData<TData, TContext>(this in CachedFilterEnumeratorWithDiagnostics<TData, TContext> enumerator)
			where TData : class, IMemberData
			where TContext : ISyntaxValidatorContext
		{
			return ref enumerator._cache;
		}

		/// <summary>
		/// Returns a reference to the internal <see cref="CachedData{T}"/> of the given <paramref name="enumerator"/>.
		/// </summary>
		/// <typeparam name="TData">Type of cached data.</typeparam>
		/// <typeparam name="TContext">Type of target <see cref="ISyntaxValidatorContext"/>.</typeparam>
		/// <param name="enumerator"><see cref="CachedLoggableFilterEnumerator{TData, TContext}"/> to get the internal <see cref="CachedData{T}"/> of.</param>
		public static ref readonly CachedData<TData> GetCachedData<TData, TContext>(this in CachedLoggableFilterEnumerator<TData, TContext> enumerator)
			where TData : class, IMemberData
			where TContext : ISyntaxValidatorContext
		{
			return ref enumerator._cache;
		}

#pragma warning restore RCS1242 // Do not pass non-read-only struct by read-only reference.

		/// <summary>
		/// Returns a reference to the internal <see cref="GeneratorExecutionContext"/> of the given <paramref name="context"/>.
		/// </summary>
		/// <typeparam name="T">Type of values the <paramref name="context"/> can cache.</typeparam>
		/// <param name="context"><see cref="CachedGeneratorExecutionContext{T}"/> to get the internal <see cref="GeneratorExecutionContext"/> of.</param>
		public static ref readonly GeneratorExecutionContext GetContext<T>(this in CachedGeneratorExecutionContext<T> context)
		{
			return ref context._context;
		}

		/// <summary>
		/// Attempts to return cached <paramref name="value"/> associated with a <see cref="FileLinePositionSpan"/> retrieved from the specified <paramref name="location"/>.
		/// </summary>
		/// <param name="context">Target <see cref="CachedGeneratorExecutionContext{T}"/>.</param>
		/// <param name="location"><see cref="Location"/> to retrieve the <see cref="FileLinePositionSpan"/> from.</param>
		/// <param name="value">Returned cached value.</param>
		/// <exception cref="ArgumentNullException"><paramref name="location"/> is <see langword="null"/>.</exception>
		public static bool TryGetCachedValue<T>(this in CachedGeneratorExecutionContext<T> context, Location location, [NotNullWhen(true)] out T? value)
		{
			if (location is null)
			{
				throw new ArgumentNullException(nameof(location));
			}

			return context.TryGetCachedValue(location.GetLineSpan(), out value);
		}

		/// <summary>
		/// Attempts to return cached <paramref name="value"/> associated with a <see cref="FileLinePositionSpan"/> retrieved from the specified <paramref name="node"/>.
		/// </summary>
		/// <param name="context">Target <see cref="CachedGeneratorExecutionContext{T}"/>.</param>
		/// <param name="node"><see cref="SyntaxNode"/> to retrieve the <see cref="FileLinePositionSpan"/> from.</param>
		/// <param name="value">Returned cached value.</param>
		/// <exception cref="ArgumentNullException"><paramref name="node"/> is <see langword="null"/>.</exception>
		public static bool TryGetCachedValue<T>(this in CachedGeneratorExecutionContext<T> context, SyntaxNode node, [NotNullWhen(true)] out T? value)
		{
			if (node is null)
			{
				throw new ArgumentNullException(nameof(node));
			}

			return context.TryGetCachedValue(node.GetLocation().GetLineSpan(), out value);
		}

		/// <summary>
		/// Attempts to return cached <paramref name="value"/> associated with a <see cref="FileLinePositionSpan"/> retrieved from the specified <paramref name="location"/>.
		/// </summary>
		/// <param name="cache">Target <see cref="CachedData{T}"/>.</param>
		/// <param name="location"><see cref="Location"/> to retrieve the <see cref="FileLinePositionSpan"/> from.</param>
		/// <param name="value">Returned cached value.</param>
		/// <exception cref="ArgumentNullException"><paramref name="location"/> is <see langword="null"/>.</exception>
		public static bool TryGetCachedValue<T>(this in CachedData<T> cache, Location location, [NotNullWhen(true)] out T? value)
		{
			if (location is null)
			{
				throw new ArgumentNullException(nameof(location));
			}

			return cache.TryGetCachedValue(location.GetLineSpan(), out value);
		}

		/// <summary>
		/// Attempts to return cached <paramref name="value"/> associated with a <see cref="FileLinePositionSpan"/> retrieved from the specified <paramref name="node"/>.
		/// </summary>
		/// <param name="cache">Target <see cref="CachedData{T}"/>.</param>
		/// <param name="node"><see cref="SyntaxNode"/> to retrieve the <see cref="FileLinePositionSpan"/> from.</param>
		/// <param name="value">Returned cached value.</param>
		/// <exception cref="ArgumentNullException"><paramref name="node"/> is <see langword="null"/>.</exception>
		public static bool TryGetCachedValue<T>(this in CachedData<T> cache, SyntaxNode node, [NotNullWhen(true)] out T? value)
		{
			if (node is null)
			{
				throw new ArgumentNullException(nameof(node));
			}

			return cache.TryGetCachedValue(node.GetLocation().GetLineSpan(), out value);
		}
	}
}
