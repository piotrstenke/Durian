// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System.Collections;
using Durian.Analysis.Data;

namespace Durian.Analysis.SymbolContainers
{
	/// <summary>
	/// Contains optional data that can be passed to a <see cref="ISymbolContainer"/>.
	/// </summary>
	public struct SymbolContainerBuilder
	{
		internal IEnumerable Collection { readonly get; set; }

		internal ICompilationData? ParentCompilation { readonly get; set; }

		internal ISymbolNameResolver? SymbolNameResolver { readonly get; set; }

		internal SymbolContainerBuilder(IEnumerable collection)
		{
			Collection = collection;
			ParentCompilation = default;
			SymbolNameResolver = default;
		}

		/// <summary>
		/// Changes the type of object being built.
		/// </summary>
		/// <typeparam name="TNew">Type of container being built.</typeparam>
		public readonly SymbolContainerBuilder<TNew> Cast<TNew>()
			where TNew : ISymbolContainer, IBuilderReceiver<SymbolContainerBuilder>, new()
		{
			return new SymbolContainerBuilder<TNew>(Collection)
			{
				ParentCompilation = ParentCompilation,
				SymbolNameResolver = SymbolNameResolver
			};
		}
	}

	/// <summary>
	/// Contains optional data that can be passed to a <see cref="ISymbolContainer"/>.
	/// </summary>
	/// <typeparam name="TContainer">Type of container being built.</typeparam>
	public struct SymbolContainerBuilder<TContainer> : IBuilder<TContainer>
		where TContainer : ISymbolContainer, IBuilderReceiver<SymbolContainerBuilder>, new()
	{
		readonly bool IBuilder<TContainer>.IsValid => Collection is not null;

		internal IEnumerable Collection { readonly get; set; }

		internal ICompilationData? ParentCompilation { readonly get; set; }

		internal ISymbolNameResolver? SymbolNameResolver { readonly get; set; }

		internal SymbolContainerBuilder(IEnumerable collection)
		{
			Collection = collection;
			ParentCompilation = default;
			SymbolNameResolver = default;
		}

		/// <summary>
		/// Implicitly converts an <see cref="SymbolContainerBuilder{T}"/> to a <typeparamref name="TContainer"/>.
		/// </summary>
		/// <param name="other"><see cref="SymbolContainerBuilder{T}"/> to convert.</param>
		public static implicit operator TContainer(SymbolContainerBuilder<TContainer> other)
		{
			return other.Build();
		}

		/// <summary>
		/// Actually builds the objects.
		/// </summary>
		public readonly TContainer Build()
		{
			TContainer t = new();
			t.Receive(NonGeneric());
			return t;
		}

		/// <summary>
		/// Changes the type of object being built.
		/// </summary>
		/// <typeparam name="TNew">Type of container being built.</typeparam>
		public readonly SymbolContainerBuilder<TNew> Cast<TNew>()
			where TNew : ISymbolContainer, IBuilderReceiver<SymbolContainerBuilder>, new()
		{
			return new SymbolContainerBuilder<TNew>(Collection)
			{
				ParentCompilation = ParentCompilation,
				SymbolNameResolver = SymbolNameResolver
			};
		}

		/// <summary>
		/// Makes the current builder non-generic.
		/// </summary>
		public readonly SymbolContainerBuilder NonGeneric()
		{
			return new SymbolContainerBuilder(Collection)
			{
				ParentCompilation = ParentCompilation,
				SymbolNameResolver = SymbolNameResolver
			};
		}
	}
}
