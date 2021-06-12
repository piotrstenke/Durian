// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Durian.Generator.Data;
using Microsoft.CodeAnalysis;

namespace Durian.Generator.Cache
{
	/// <summary>
	/// Contains extension methods for the <see cref="CachedGeneratorExecutionContext{T}"/> struct.
	/// </summary>
	public static class CachedValueTypesExtensions
	{
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
		/// <typeparam name="T">Type of values this <paramref name="enumerator"/> enumerates on.</typeparam>
		/// <param name="enumerator"><see cref="CachedFilterEnumerator{T}"/> to get the internal <see cref="CachedData{T}"/> of.</param>
		public static ref readonly CachedData<T> GetCachedData<T>(this in CachedFilterEnumerator<T> enumerator) where T : IMemberData
		{
			return ref enumerator._cache;
		}

		/// <summary>
		/// Returns a reference to the internal <see cref="CachedData{T}"/> of the given <paramref name="enumerator"/>.
		/// </summary>
		/// <typeparam name="T">Type of values this <paramref name="enumerator"/> enumerates on.</typeparam>
		/// <param name="enumerator"><see cref="CachedFilterEnumeratorWithDiagnostics{T}"/> to get the internal <see cref="CachedData{T}"/> of.</param>
		public static ref readonly CachedData<T> GetCachedData<T>(this in CachedFilterEnumeratorWithDiagnostics<T> enumerator) where T : IMemberData
		{
			return ref enumerator._cache;
		}

		/// <summary>
		/// Returns a reference to the internal <see cref="CachedData{T}"/> of the given <paramref name="enumerator"/>.
		/// </summary>
		/// <typeparam name="T">Type of values this <paramref name="enumerator"/> enumerates on.</typeparam>
		/// <param name="enumerator"><see cref="CachedLoggableFilterEnumerator{T}"/> to get the internal <see cref="CachedData{T}"/> of.</param>
		public static ref readonly CachedData<T> GetCachedData<T>(this in CachedLoggableFilterEnumerator<T> enumerator) where T : IMemberData
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

#pragma warning disable RCS1242 // Do not pass non-read-only struct by read-only reference.
#pragma warning restore RCS1242 // Do not pass non-read-only struct by read-only reference.
	}
}
