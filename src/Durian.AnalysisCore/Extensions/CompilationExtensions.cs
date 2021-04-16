using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace Durian.Extensions
{
	/// <summary>
	/// Contains various extension methods for the <see cref="Compilation"/> class.
	/// </summary>
	public static class CompilationExtensions
	{
		/// <summary>
		/// Returns all special types in the compilation. See: <see cref="SpecialType"/>.
		/// </summary>
		/// <param name="compilation"></param>
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
