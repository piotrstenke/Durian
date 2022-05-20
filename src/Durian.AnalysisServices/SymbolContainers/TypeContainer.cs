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
	/// <see cref="ISymbolContainer"/> that handles <see cref="INamedTypeSymbol"/>s.
	/// </summary>
	public sealed class TypeContainer : SymbolContainer
	{
		/// <summary>
		/// Determines whether to use type arguments instead of type parameters when building a <see cref="string"/>.
		/// </summary>
		public bool UseArguments { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="TypeContainer"/> class.
		/// </summary>
		public TypeContainer()
		{
		}

		internal TypeContainer(IEnumerable<INamedTypeSymbol> collection, ICompilationData? compilation, bool useArguments, ReturnOrder order) : base(collection, compilation, order)
		{
			UseArguments = useArguments;
		}

		internal TypeContainer(IEnumerable<ITypeData> collection, bool useArguments, ReturnOrder order) : base(collection, order)
		{
			UseArguments = useArguments;
		}

		/// <inheritdoc/>
		public override void Build(StringBuilder builder)
		{
			DefaultBuild(builder, GetNames());
		}

		/// <summary>
		/// Returns the <see cref="ITypeData"/>s contained within this instance.
		/// </summary>
		/// <exception cref="InvalidOperationException"><see cref="ISymbol"/>s cannot be converted into <see cref="IMemberData"/>, because <see cref="SymbolContainer.TargetCompilation"/> wasn't specified for the current container.</exception>
		public new ImmutableArray<ITypeData> GetData()
		{
			return base.GetData().CastArray<ITypeData>();
		}

		/// <inheritdoc/>
		public override ImmutableArray<string> GetNames()
		{
			if(!TryGetArray(out object[]? array))
			{
				return ImmutableArray<string>.Empty;
			}

			ImmutableArray<string>.Builder builder = ImmutableArray.CreateBuilder<string>(array.Length);

			if (array is IMemberData[] members)
			{
				for (int i = 0; i < array.Length; i++)
				{
					builder.Add(members[i].Symbol.GetGenericName(UseArguments));
				}
			}
			else
			{
				ISymbol[] symbols = (array as ISymbol[])!;

				for (int i = 0; i < array.Length; i++)
				{
					builder.Add(symbols[i].GetGenericName(UseArguments));
				}
			}

			return builder.ToImmutableArray();
		}

		/// <summary>
		/// Returns the <see cref="INamedTypeSymbol"/>s contained within this instance.
		/// </summary>
		public new ImmutableArray<INamedTypeSymbol> GetSymbols()
		{
			return base.GetSymbols().CastArray<INamedTypeSymbol>();
		}

		/// <summary>
		/// Converts the current array into an array of <see cref="INamedTypeSymbol"/>s.
		/// </summary>
		public new INamedTypeSymbol[] ToArray()
		{
			if (!TryGetArray(out object[]? array))
			{
				return Array.Empty<INamedTypeSymbol>();
			}

			INamedTypeSymbol[] newArray = new INamedTypeSymbol[array.Length];

			if (array is ISymbol[] symbols)
			{
				for (int i = 0; i < array.Length; i++)
				{
					newArray[i] = (symbols[i] as INamedTypeSymbol)!;
				}
			}
			else
			{
				IMemberData[] members = (array as IMemberData[])!;

				for (int i = 0; i < members.Length; i++)
				{
					newArray[i] = (members[i].Symbol as INamedTypeSymbol)!;
				}
			}

			return newArray;
		}

		/// <inheritdoc/>
		protected override IMemberData GetData(ISymbol symbol, ICompilationData compilation)
		{
			return (symbol as INamedTypeSymbol)!.ToData(compilation);
		}

		private protected override IMemberData[] CreateMemberArray(IEnumerable<IMemberData> collection)
		{
			return collection.Cast<ITypeData>().ToArray();
		}

		private protected override IMemberData[] CreateMemberArray(int length)
		{
			return new ITypeData[length];
		}

		private protected override ISymbol[] CreateSymbolArray(IEnumerable<ISymbol> collection)
		{
			return collection.Cast<INamedTypeSymbol>().ToArray();
		}

		private protected override ISymbol[] CreateSymbolArray(int length)
		{
			return new INamedTypeSymbol[length];
		}
	}

	public static partial class SymbolContainerFactory
	{
		/// <summary>
		/// Creates a new <see cref="TypeContainer"/>.
		/// </summary>
		/// <param name="types">Collection of <see cref="ITypeData"/>s to add to the container.</param>
		/// <param name="useArguments">Determines whether to use type arguments instead of type parameters when building a <see cref="string"/>.</param>
		/// <param name="order">Specifies ordering of the returned members.</param>
		public static TypeContainer ToContainer(this IEnumerable<ITypeData> types, bool useArguments = false, ReturnOrder order = ReturnOrder.Root)
		{
			return new(types, useArguments, order);
		}

		/// <summary>
		/// Creates a new <see cref="TypeContainer"/>.
		/// </summary>
		/// <param name="symbols">Collection of <see cref="INamedTypeSymbol"/>s to add to the container.</param>
		/// <param name="compilation"><see cref="ICompilationData"/> to use when converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>.</param>
		/// <param name="useArguments">Determines whether to use type arguments instead of type parameters when building a <see cref="string"/>.</param>
		/// <param name="order">Specifies ordering of the returned members.</param>
		public static TypeContainer ToContainer(this IEnumerable<INamedTypeSymbol> symbols, ICompilationData? compilation = default, bool useArguments = false, ReturnOrder order = ReturnOrder.Root)
		{
			return new(symbols, compilation, useArguments, order);
		}
	}
}
