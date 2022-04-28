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
		/// Returns a collection of all types declared in the specified <paramref name="compilation"/>.
		/// </summary>
		/// <param name="compilation"><see cref="Compilation"/> to get the types from.</param>
		/// <param name="includeReferences">Determines whether to include types from referenced assemblies.</param>
		public static IEnumerable<INamedTypeSymbol> GetAllTypes(this Compilation compilation, bool includeReferences = false)
		{
			if (includeReferences)
			{
				return compilation.GlobalNamespace.GetInnerTypes();
			}

			return compilation.Assembly.GlobalNamespace.GetInnerTypes();
		}

		/// <summary>
		/// Returns all special types in the <paramref name="compilation"/>. See: <see cref="SpecialType"/>.
		/// </summary>
		/// <param name="compilation"><see cref="Compilation"/> to get the special types from.</param>
		public static IEnumerable<INamedTypeSymbol> GetSpecialTypes(this Compilation compilation)
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
