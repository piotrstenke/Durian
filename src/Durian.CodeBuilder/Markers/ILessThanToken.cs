﻿// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Durian.Builder.Markers;

namespace Durian.Builder.Markers
{
	/// <summary>
	/// Provides a method for writing the less than '&lt;' token.
	/// </summary>
	/// <typeparam name="TReturn">Type of returned builder.</typeparam>
	public interface ILessThanToken<out TReturn> : IWhiteSpace where TReturn : class
	{
	}
}

namespace Durian.Builder
{
	public partial class RawCodeBuilder
	{
		internal partial class Adapter : ILessThanToken<IWhiteSpace>
		{
		}
	}

	public static partial class RawCodeBuilderExtensions
	{
		/// <summary>
		/// Writes a less than '&lt;' token.
		/// </summary>
		/// <typeparam name="T">Type of target builder.</typeparam>
		/// <param name="builder">Code builder to write to.</param>
		public static T LessThan<T>(this ILessThanToken<T> builder) where T : class
		{
			ICodeBuilder code = (builder as ICodeBuilder)!;
			code.CodeReceiver.Write('<');
			return (code as T)!;
		}
	}
}
