// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace Durian.Analysis.Extensions
{
	/// <summary>
	/// Contains various extension methods for the <see cref="Compilation"/> class.
	/// </summary>
	public static class CompilationExtensions
	{
		/// <summary>
		/// Returns all special types in the <paramref name="compilation"/>. See: <see cref="SpecialType"/>.
		/// </summary>
		/// <param name="compilation"><see cref="Compilation"/> to get the special types from.</param>
		/// <exception cref="ArgumentNullException"><paramref name="compilation"/> is <see langword="null"/>.</exception>
		public static IEnumerable<INamedTypeSymbol> GetSpecialTypes(this Compilation compilation)
		{
			if (compilation is null)
			{
				throw new ArgumentNullException(nameof(compilation));
			}

			return Yield();

			IEnumerable<INamedTypeSymbol> Yield()
			{
				Array array = Enum.GetValues(typeof(SpecialType));
				int length = array.Length;

				for (int i = 1; i < length; i++)
				{
					yield return compilation.GetSpecialType((SpecialType)array.GetValue(i)!);
				}
			}
		}
	}
}
