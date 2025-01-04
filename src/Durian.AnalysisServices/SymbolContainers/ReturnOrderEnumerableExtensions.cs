using System;
using System.Collections.Generic;
using System.Linq;

namespace Durian.Analysis.SymbolContainers
{
	/// <summary>
	/// Contains LINQ-like extension methods for the <see cref="IReturnOrderEnumerable{T}"/> interface.
	/// </summary>
	public static class ReturnOrderEnumerableExtensions
	{
		/// <summary>
		/// Casts the elements of a sequence to the specified type.
		/// </summary>
		/// <typeparam name="TResult">The type to cast the elements of source to.</typeparam>
		/// <param name="source">The <see cref="IReturnOrderEnumerable"/> that contains the elements to be cast to type <typeparamref name="TResult"/>.</param>
		/// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
		/// <exception cref="InvalidCastException">An element in the sequence cannot be cast to type <typeparamref name="TResult"/>.</exception>
		public static IReturnOrderEnumerable<TResult> Cast<TResult>(this IReturnOrderEnumerable source)
		{
			source.OfType<string>();
			IEnumerable<TResult> enumerable = Enumerable.Cast<TResult>(source);
			return new ReturnOrderEnumerable<TResult>(enumerable, source.Order);
		}

		/// <summary>
		/// Filters the elements of a sequence based on a specified type.
		/// </summary>
		/// <typeparam name="TResult">The type to filter the elements of the sequence on.</typeparam>
		/// <param name="source">The <see cref="IReturnOrderEnumerable"/> whose elements to filter.</param>
		/// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
		public static IReturnOrderEnumerable<TResult> OfType<TResult>(this IReturnOrderEnumerable source)
		{
			IEnumerable<TResult> enumerable = Enumerable.OfType<TResult>(source);
			return new ReturnOrderEnumerable<TResult>(enumerable, source.Order);
		}

		/// <summary>
		/// Projects each element of a sequence into a new form.
		/// </summary>
		/// <typeparam name="TSource">The type of the elements of source.</typeparam>
		/// <typeparam name="TResult">The type of the value returned by selector.</typeparam>
		/// <param name="source">A sequence of values to invoke a transform function on.</param>
		/// <param name="selector">A transform function to apply to each element.</param>
		/// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>. -or- <paramref name="selector"/> is <see langword="null"/>.</exception>
		public static IReturnOrderEnumerable<TResult> Select<TSource, TResult>(this IReturnOrderEnumerable<TSource> source, Func<TSource, TResult> selector)
		{
			IEnumerable<TResult> enumerable = Enumerable.Select(source, selector);
			return new ReturnOrderEnumerable<TResult>(enumerable, source.Order);
		}

		/// <summary>
		/// Projects each element of a sequence to an <see cref="IEnumerable{T}"/> and flattens the resulting sequences into one sequence.
		/// </summary>
		/// <typeparam name="TSource">The type of the elements of source.</typeparam>
		/// <typeparam name="TResult">The type of the value returned by selector.</typeparam>
		/// <param name="source">A sequence of values to project.</param>
		/// <param name="selector">A transform function to apply to each element.</param>
		/// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>. -or- <paramref name="selector"/> is <see langword="null"/>.</exception>
		public static IReturnOrderEnumerable<TResult> SelectMany<TSource, TResult>(this IReturnOrderEnumerable<TSource> source, Func<TSource, IEnumerable<TResult>> selector)
		{
			IEnumerable<TResult> enumerable = Enumerable.SelectMany(source, selector);
			return new ReturnOrderEnumerable<TResult>(enumerable, source.Order);
		}

		/// <summary>
		/// Filters a sequence of values based on a predicate.
		/// </summary>
		/// <typeparam name="TSource">The type of the elements of source.</typeparam>
		/// <param name="source">An <see cref="IReturnOrderEnumerable{T}"/> to filter.</param>
		/// <param name="predicate">A function to test each element for a condition.</param>
		/// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>. -or- <paramref name="predicate"/> is <see langword="null"/>.</exception>
		public static IReturnOrderEnumerable<TSource> Where<TSource>(this IReturnOrderEnumerable<TSource> source, Func<TSource, bool> predicate)
		{
			IEnumerable<TSource> enumerable = Enumerable.Where(source, predicate);
			return new ReturnOrderEnumerable<TSource>(enumerable, source.Order);
		}
	}
}
