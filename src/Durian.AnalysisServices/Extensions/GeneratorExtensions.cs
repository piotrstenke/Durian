// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using Durian.Analysis.Logging;
using Microsoft.CodeAnalysis;

namespace Durian.Analysis.Extensions
{
    /// <summary>
    /// Contains various extension methods for <see cref="ISourceGenerator"/>s.
    /// </summary>
    public static class GeneratorExtensions
    {
		/// <summary>
		/// Returns a <see cref="FilterMode"/> associated with the specified <paramref name="generator"/>.
		/// </summary>
		/// <param name="generator"><see cref="IDurianGenerator"/> to get the <see cref="FilterMode"/> associated with.</param>
		/// <exception cref="ArgumentNullException"><paramref name="generator"/> is <see langword="null"/>.</exception>
		public static FilterMode GetFilterMode(this IDurianGenerator generator)
        {
			if(generator is ILoggableGenerator loggable)
            {
				return GetFilterMode(loggable);
            }

			if (generator is null)
			{
				throw new ArgumentNullException(nameof(generator));
			}

			return GetFilterMode(generator.EnableDiagnostics, generator.EnableLogging);
		}

		/// <summary>
		/// Returns a <see cref="FilterMode"/> associated with the specified <paramref name="generator"/>.
		/// </summary>
		/// <param name="generator"><see cref="ILoggableGenerator"/> to get the <see cref="FilterMode"/> associated with.</param>
		/// <exception cref="ArgumentNullException"><paramref name="generator"/> is <see langword="null"/>.</exception>
		public static FilterMode GetFilterMode(this ILoggableGenerator generator)
        {
			if (generator is null)
			{
				throw new ArgumentNullException(nameof(generator));
			}

			return generator.LoggingConfiguration.CurrentFilterMode;
		}

#pragma warning disable RCS1224 // Make method an extension method.
        internal static FilterMode GetFilterMode(bool enableDiagnostics, bool enableLogging)
#pragma warning restore RCS1224 // Make method an extension method.
        {
			FilterMode mode = FilterMode.None;

			if (enableDiagnostics)
			{
				mode += (int)FilterMode.Diagnostics;
			}

			if (enableLogging)
			{
				mode += (int)FilterMode.Logs;
			}

			return mode;
		}
    }
}