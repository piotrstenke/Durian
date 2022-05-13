// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Durian.Analysis.Data;
using Durian.Analysis.Extensions;
using Microsoft.CodeAnalysis;

namespace Durian.Analysis.SymbolContainers
{
	/// <summary>
	/// <see cref="ISymbolContainer"/> that handles <see cref="INamespaceOrTypeSymbol"/>s.
	/// </summary>
	public sealed class NamespacesAndTypesContainer : SymbolContainer, IEnumerable<INamespaceOrTypeSymbol>
	{
		/// <inheritdoc cref="TypeContainer.UseArguments"/>
		public bool UseArguments { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="NamespacesAndTypesContainer"/> class.
		/// </summary>
		public NamespacesAndTypesContainer()
		{
		}

		internal NamespacesAndTypesContainer(IEnumerable<INamespaceOrTypeSymbol> collection, ICompilationData? compilation, bool useArguments, ReturnOrder order) : base(collection, compilation, order)
		{
			UseArguments = useArguments;
		}

		internal NamespacesAndTypesContainer(IEnumerable<NamespaceOrTypeData> collection, bool useArguments, ReturnOrder order) : base(collection, order)
		{
			UseArguments = useArguments;
		}

		/// <inheritdoc/>
		public override void Build(StringBuilder builder)
		{
			DefaultBuild(builder, GetNames());
		}

		/// <inheritdoc/>
		public override ImmutableArray<string> GetNames()
		{
			if(!TryGetArray(out object[]? array))
			{
				return ImmutableArray<string>.Empty;
			}

			GenericSubstitution substituion = UseArguments ? GenericSubstitution.TypeArguments : default;

			ImmutableArray<string>.Builder builder = ImmutableArray.CreateBuilder<string>(array.Length);

			if (array is IMemberData[] members)
			{
				for (int i = 0; i < array.Length; i++)
				{
					builder.Add(members[i].Symbol.GetGenericName(substituion));
				}
			}
			else
			{
				ISymbol[] symbols = (array as ISymbol[])!;

				for (int i = 0; i < array.Length; i++)
				{
					builder.Add(symbols[i].GetGenericName(substituion));
				}
			}

			return builder.ToImmutableArray();
		}

		/// <summary>
		/// Returns all types contained within this container.
		/// </summary>
		public TypeContainer GetTypes()
		{
			if(!TryGetArray(out object[]? array))
			{
				return SymbolContainerFactory.Empty<TypeContainer>();
			}

			if (array is ISymbol[])
			{
				List<INamedTypeSymbol> types = new(array.Length);

				foreach (INamespaceOrTypeSymbol symbol in array)
				{
					if (symbol.IsNamespace)
					{
						types.Add((symbol as INamedTypeSymbol)!);
					}
				}

				return types.ToContainer(TargetCompilation, UseArguments, Order);
			}

			List<ITypeData> list = new();

			foreach (NamespaceOrTypeData data in array)
			{
				if (data.AsType is not null)
				{
					list.Add(data);
				}
			}

			return list.ToContainer(UseArguments, Order);
		}

		/// <summary>
		/// Returns all namespaces contained within this container.
		/// </summary>
		public NamespaceContainer GetNamespaces()
		{
			if (!TryGetArray(out object[]? array))
			{
				return SymbolContainerFactory.Empty<NamespaceContainer>();
			}

			if (array is ISymbol[])
			{
				List<INamespaceSymbol> namespaces = new(array.Length);

				foreach (INamespaceOrTypeSymbol symbol in array)
				{
					if (symbol.IsNamespace)
					{
						namespaces.Add((symbol as INamespaceSymbol)!);
					}
				}

				return namespaces.ToContainer(TargetCompilation, Order);
			}

			List<NamespaceData> list = new();

			foreach (NamespaceOrTypeData data in array)
			{
				if(data.AsNamespace is not null)
				{
					list.Add(data);
				}
			}

			return list.ToContainer(Order);
		}

		/// <summary>
		/// Returns the <see cref="NamespaceOrTypeData"/>s contained within this instance.
		/// </summary>
		/// <exception cref="InvalidOperationException"><see cref="ISymbol"/>s cannot be converted into <see cref="IMemberData"/>, because <see cref="SymbolContainer.TargetCompilation"/> wasn't specified for the current container.</exception>
		public new ImmutableArray<NamespaceOrTypeData> GetData()
		{
			return base.GetData().CastArray<NamespaceOrTypeData>();
		}

		/// <summary>
		/// Returns the <see cref="INamedTypeSymbol"/>s contained within this instance.
		/// </summary>
		public new ImmutableArray<INamespaceOrTypeSymbol> GetSymbols()
		{
			return base.GetSymbols().CastArray<INamespaceOrTypeSymbol>();
		}

		/// <inheritdoc/>
		protected override IMemberData GetData(ISymbol symbol, ICompilationData compilation)
		{
			return new NamespaceOrTypeData((symbol as INamespaceOrTypeSymbol)!, compilation);
		}

		/// <inheritdoc/>
		public IEnumerator<INamespaceOrTypeSymbol> GetEnumerator()
		{
			IEnumerable<INamespaceOrTypeSymbol> symbols = GetSymbols();
			return symbols.GetEnumerator();
		}
	}

	public static partial class SymbolContainerFactory
	{
		/// <summary>
		/// Creates a new <see cref="NamespacesAndTypesContainer"/>.
		/// </summary>
		/// <param name="types">Collection of <see cref="NamespaceOrTypeData"/>s to add to the container.</param>
		/// <param name="useArguments">Determines whether to use type arguments instead of type parameters when building a <see cref="string"/>.</param>
		/// <param name="order">Specifies ordering of the returned members.</param>
		public static NamespacesAndTypesContainer ToContainer(this IEnumerable<NamespaceOrTypeData> types, bool useArguments = false, ReturnOrder order = ReturnOrder.Root)
		{
			return new(types, useArguments, order);
		}

		/// <summary>
		/// Creates a new <see cref="NamespacesAndTypesContainer"/>.
		/// </summary>
		/// <param name="symbols">Collection of <see cref="INamespaceOrTypeSymbol"/>s to add to the container.</param>
		/// <param name="compilation"><see cref="ICompilationData"/> to use when converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>.</param>
		/// <param name="useArguments">Determines whether to use type arguments instead of type parameters when building a <see cref="string"/>.</param>
		/// <param name="order">Specifies ordering of the returned members.</param>
		public static NamespacesAndTypesContainer ToContainer(this IEnumerable<INamespaceOrTypeSymbol> symbols, ICompilationData? compilation = default, bool useArguments = false, ReturnOrder order = ReturnOrder.Root)
		{
			return new(symbols, compilation, useArguments, order);
		}
	}
}
