// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Durian.Analysis.Extensions
{
	/// <summary>
	/// Contains various extension methods for the <see cref="Compilation"/> class.
	/// </summary>
	public static class CompilationExtensions
	{
		/// <summary>
		/// Returns a collection of all <see cref="INamespaceSymbol"/>s contained withing the specified <paramref name="compilation"/>.
		/// </summary>
		/// <param name="compilation"><see cref="Compilation"/> to get the namespaces from.</param>
		/// <param name="includeExternal">Determines whether to include namespaces from referenced assemblies.</param>
		public static IEnumerable<INamespaceSymbol> GetAllNamespaces(this Compilation compilation, bool includeExternal = false)
		{
			INamespaceSymbol globalNamespace = includeExternal ? compilation.GlobalNamespace : compilation.Assembly.GlobalNamespace;

			Stack<INamespaceSymbol> namespaces = new(32);

			foreach (INamespaceSymbol @namespace in globalNamespace.GetNamespaceMembers())
			{
				namespaces.Push(@namespace);
			}

			while(namespaces.Count > 0)
			{
				INamespaceSymbol current = namespaces.Pop();

				yield return current;

				foreach (INamespaceSymbol child in current.GetNamespaceMembers().Reverse())
				{
					namespaces.Push(child);
				}
			}
		}

		/// <summary>
		/// Returns a collection of all types declared in the specified <paramref name="compilation"/>.
		/// </summary>
		/// <param name="compilation"><see cref="Compilation"/> to get the types from.</param>
		/// <param name="includeExternal">Determines whether to include types from referenced assemblies.</param>
		public static IEnumerable<INamedTypeSymbol> GetAllTypes(this Compilation compilation, bool includeExternal = false)
		{
			const int capacity = 32;
			INamespaceSymbol globalNamespace = includeExternal ? compilation.GlobalNamespace : compilation.Assembly.GlobalNamespace;

			Stack<INamedTypeSymbol> innerTypes = new(capacity);

			foreach (INamedTypeSymbol globalType in globalNamespace.GetTypeMembers())
			{
				yield return globalType;

				if(FillStack(globalType))
				{
					while (innerTypes.Count > 0)
					{
						yield return PushChildren();
					}
				}
			}

			foreach (INamespaceSymbol @namespace in compilation.GetAllNamespaces(includeExternal))
			{
				if(FillStack(@namespace))
				{
					while(innerTypes.Count > 0)
					{
						yield return PushChildren();
					}
				}
			}

			bool FillStack(INamespaceOrTypeSymbol currentSymbol)
			{
				ImmutableArray<INamedTypeSymbol> array = currentSymbol.GetTypeMembers();

				if (array.Length == 0)
				{
					return false;
				}

				foreach (INamedTypeSymbol t in array.Reverse())
				{
					innerTypes.Push(t);
				}

				return true;
			}

			INamedTypeSymbol PushChildren()
			{
				INamedTypeSymbol t = innerTypes.Pop();
				ImmutableArray<INamedTypeSymbol>  array = t.GetTypeMembers();

				if (array.Length > 0)
				{
					foreach (INamedTypeSymbol child in array.Reverse())
					{
						innerTypes.Push(child);
					}
				}

				return t;
			}
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
