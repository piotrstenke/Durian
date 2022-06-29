// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;
using Durian.Analysis.Data;

namespace Durian.Analysis.SymbolContainers
{
	/// <summary>
	/// Contains optional data that can be passed to a <see cref="ISymbolContainer"/>.
	/// </summary>
	/// <typeparam name="T">Type of object being built</typeparam>
	public struct SymbolContainerBuilder<T> : IBuilder<T> where T : ISymbolContainer, IBuilderReceiver<SymbolContainerBuilder<T>>, new()
	{
		bool IBuilder<T>.IsValid => Collection is not null;
		internal IEnumerable<ISymbolOrMember> Collection { get; set; }
		internal ICompilationData? ParentCompilation { get; set; }
		internal ISymbolNameResolver? SymbolNameResolver { get; set; }

		internal SymbolContainerBuilder(IEnumerable<ISymbolOrMember> collection)
		{
			Collection = collection;
			ParentCompilation = default;
			SymbolNameResolver = default;
		}

		/// <summary>
		/// Implicitly converts an <see cref="SymbolContainerBuilder{T}"/> to a <typeparamref name="T"/>.
		/// </summary>
		/// <param name="other"><see cref="SymbolContainerBuilder{T}"/> to convert.</param>
		public static implicit operator T(SymbolContainerBuilder<T> other)
		{
			return other.Build();
		}

		/// <inheritdoc/>
		public readonly T Build()
		{
			if (Collection is null)
			{
				throw new BuilderException("Builder was created using the default constructor, which is not valid");
			}

			T t = new();
			t.Receive(this);
			return t;
		}
	}
}
