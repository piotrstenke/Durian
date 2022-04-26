// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Durian.Builder.Markers;

namespace Durian.Builder.Markers
{
	/// <summary>
	/// Provides methods for building white-space tokens (spaces, tabs, new lines).
	/// </summary>
	public interface IWhiteSpace
	{
	}

	/// <summary>
	/// Provides methods for building white-space tokens (spaces, tabs, new lines).
	/// </summary>
	/// <typeparam name="TReturn">Type of returned builder.</typeparam>
	public interface IWhiteSpace<out TReturn> : IWhiteSpace where TReturn : class
	{
	}
}

namespace Durian.Builder
{
	public partial class RawCodeBuilder
	{
		internal partial class Adapter : IWhiteSpace<IWhiteSpace>
		{
		}
	}

	public static partial class RawCodeBuilderExtensions
	{
		/// <summary>
		/// Writes a space.
		/// </summary>
		/// <typeparam name="T">Type of target builder..</typeparam>
		/// <param name="builder"><see cref="IWhiteSpace"/> to use to insert a space.</param>
		public static T Space<T>(this T builder) where T : class, IWhiteSpace
		{
			RawCodeBuilder.Adapter raw = (builder as RawCodeBuilder.Adapter)!;
			return (raw.Space() as T)!;
		}

		/// <summary>
		/// Writes a specified number of spaces.
		/// </summary>
		/// <typeparam name="T">Type of target builder..</typeparam>
		/// <param name="builder"><see cref="IWhiteSpace"/> to use to insert a space.</param>
		/// <param name="count">Number of spaces to append.</param>
		public static T Space<T>(this T builder, int count) where T : class, IWhiteSpace
		{
			RawCodeBuilder.Adapter raw = (builder as RawCodeBuilder.Adapter)!;
			return (raw.Space(count) as T)!;
		}

		/// <summary>
		/// Writes a space.
		/// </summary>
		/// <typeparam name="T">Type of target builder..</typeparam>
		/// <param name="builder"><see cref="IWhiteSpace"/> to use to insert a space.</param>
		public static T Space<T>(this IWhiteSpace<T> builder) where T : class, IWhiteSpace
		{
			RawCodeBuilder.Adapter raw = (builder as RawCodeBuilder.Adapter)!;
			return (raw.Space() as T)!;
		}

		/// <summary>
		/// Writes a specified number of spaces.
		/// </summary>
		/// <typeparam name="T">Type of target builder..</typeparam>
		/// <param name="builder"><see cref="IWhiteSpace"/> to use to insert a space.</param>
		/// <param name="count">Number of spaces to append.</param>
		public static T Space<T>(this IWhiteSpace<T> builder, int count) where T : class, IWhiteSpace
		{
			RawCodeBuilder.Adapter raw = (builder as RawCodeBuilder.Adapter)!;
			return (raw.Space(count) as T)!;
		}

		/// <summary>
		/// Writes a tab.
		/// </summary>
		/// <typeparam name="T">Type of target builder..</typeparam>
		/// <param name="builder"><see cref="IWhiteSpace"/> to use to insert a tab.</param>
		public static T Tab<T>(this T builder) where T : class, IWhiteSpace
		{
			RawCodeBuilder.Adapter raw = (builder as RawCodeBuilder.Adapter)!;
			return (raw.Tab() as T)!;
		}

		/// <summary>
		/// Writes a specified number of tabs.
		/// </summary>
		/// <typeparam name="T">Type of target builder..</typeparam>
		/// <param name="builder"><see cref="IWhiteSpace"/> to use to insert a tab.</param>
		/// <param name="count">Number of tabs to append.</param>
		public static T Tab<T>(this T builder, int count) where T : class, IWhiteSpace
		{
			RawCodeBuilder.Adapter raw = (builder as RawCodeBuilder.Adapter)!;
			return (raw.Tab(count) as T)!;
		}

		/// <summary>
		/// Writes a tab.
		/// </summary>
		/// <typeparam name="T">Type of target builder..</typeparam>
		/// <param name="builder"><see cref="IWhiteSpace"/> to use to insert a tab.</param>
		public static T Tab<T>(this IWhiteSpace<T> builder) where T : class, IWhiteSpace
		{
			RawCodeBuilder.Adapter raw = (builder as RawCodeBuilder.Adapter)!;
			return (raw.Tab() as T)!;
		}

		/// <summary>
		/// Writes a specified number of tabs.
		/// </summary>
		/// <typeparam name="T">Type of target builder..</typeparam>
		/// <param name="builder"><see cref="IWhiteSpace"/> to use to insert a tab.</param>
		/// <param name="count">Number of tabs to append.</param>
		public static T Tab<T>(this IWhiteSpace<T> builder, int count) where T : class, IWhiteSpace
		{
			RawCodeBuilder.Adapter raw = (builder as RawCodeBuilder.Adapter)!;
			return (raw.Tab(count) as T)!;
		}

		/// <summary>
		/// Writes a new line.
		/// </summary>
		/// <typeparam name="T">Type of target builder..</typeparam>
		/// <param name="builder"><see cref="IWhiteSpace"/> to use to insert a new line.</param>
		public static T NewLine<T>(this T builder) where T : class, IWhiteSpace
		{
			RawCodeBuilder.Adapter raw = (builder as RawCodeBuilder.Adapter)!;
			return (raw.NewLine() as T)!;
		}

		/// <summary>
		/// Writes a specified number of new lines.
		/// </summary>
		/// <typeparam name="T">Type of target builder..</typeparam>
		/// <param name="builder"><see cref="IWhiteSpace"/> to use to insert a new line.</param>
		/// <param name="count">Number of new lines to append.</param>
		public static T NewLine<T>(this T builder, int count) where T : class, IWhiteSpace
		{
			RawCodeBuilder.Adapter raw = (builder as RawCodeBuilder.Adapter)!;
			return (raw.NewLine(count) as T)!;
		}

		/// <summary>
		/// Writes a new line.
		/// </summary>
		/// <typeparam name="T">Type of target builder..</typeparam>
		/// <param name="builder"><see cref="IWhiteSpace"/> to use to insert a new line.</param>
		public static T NewLine<T>(this IWhiteSpace<T> builder) where T : class, IWhiteSpace
		{
			RawCodeBuilder.Adapter raw = (builder as RawCodeBuilder.Adapter)!;
			return (raw.NewLine() as T)!;
		}

		/// <summary>
		/// Writes a specified number of new lines.
		/// </summary>
		/// <typeparam name="T">Type of target builder..</typeparam>
		/// <param name="builder"><see cref="IWhiteSpace"/> to use to insert a new line.</param>
		/// <param name="count">Number of new lines to append.</param>
		public static T NewLine<T>(this IWhiteSpace<T> builder, int count) where T : class, IWhiteSpace
		{
			RawCodeBuilder.Adapter raw = (builder as RawCodeBuilder.Adapter)!;
			return (raw.NewLine(count) as T)!;
		}
	}
}
