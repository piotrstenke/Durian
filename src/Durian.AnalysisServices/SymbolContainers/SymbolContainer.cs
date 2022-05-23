// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using Durian.Analysis.Data;
using Durian.Analysis.Extensions;
using Microsoft.CodeAnalysis;

namespace Durian.Analysis.SymbolContainers
{
	/// <summary>
	/// Provides methods for returning symbol representations using either <see cref="ISymbol"/>s or <see cref="IMemberData"/>s.
	/// </summary>
	public abstract class SymbolContainer : ISymbolContainer, IEnumerable
	{
		private protected class ArrayHandler<TSymbol, TData> : IArrayHandler
			where TSymbol : class, ISymbol
			where TData : class, IMemberData
		{
			ISymbol[] IArrayHandler.CreateArray(int length)
			{
				return new TSymbol[length];
			}

			ISymbol[] IArrayHandler.CreateArray(IEnumerable<ISymbol> collection)
			{
				return collection.Cast<TSymbol>().ToArray();
			}

			IMemberData[] IArrayHandler.CreateArray(IEnumerable<IMemberData> collection)
			{
				return collection.Cast<TData>().ToArray();
			}

			ImmutableArray<IMemberData> IArrayHandler.ToImmutableData()
			{
				return ImmutableArray.Create<TData>().CastArray<IMemberData>();
			}

			ImmutableArray<IMemberData> IArrayHandler.ToImmutableData(IEnumerable<IMemberData> collection)
			{
				return ImmutableArray.CreateRange(collection.Cast<TData>()).CastArray<IMemberData>();
			}

			ImmutableArray<ISymbol> IArrayHandler.ToImmutableSymbol()
			{
				return ImmutableArray.Create<TSymbol>().CastArray<ISymbol>();
			}

			ImmutableArray<ISymbol> IArrayHandler.ToImmutableSymbol(IEnumerable<ISymbol> collection)
			{
				return ImmutableArray.CreateRange(collection.Cast<TSymbol>()).CastArray<ISymbol>();
			}
		}

		private protected interface IArrayHandler
		{
			ISymbol[] CreateArray(int length);

			ISymbol[] CreateArray(IEnumerable<ISymbol> collection);

			IMemberData[] CreateArray(IEnumerable<IMemberData> collection);

			ImmutableArray<IMemberData> ToImmutableData();

			ImmutableArray<IMemberData> ToImmutableData(IEnumerable<IMemberData> collection);

			ImmutableArray<ISymbol> ToImmutableSymbol();

			ImmutableArray<ISymbol> ToImmutableSymbol(IEnumerable<ISymbol> collection);
		}

		private readonly IArrayHandler _arrayHandler;
		private object[]? _array;
		private IEnumerable<object>? _collection;

		/// <summary>
		/// Determines whether the <see cref="GetData()"/> method is safe to call.
		/// </summary>
		public bool CanRetrieveData { get; }

		/// <inheritdoc/>
		public ReturnOrder Order { get; }

		/// <summary>
		/// <see cref="ICompilationData"/> to use when converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>.
		/// </summary>
		public ICompilationData? TargetCompilation { get; }

		private protected SymbolContainer(ReturnOrder order = default)
		{
			_collection = default!;
			_arrayHandler = GetArrayHandler();
			Order = order;
		}

		private protected SymbolContainer(IEnumerable<ISymbol> collection, ICompilationData? compilation, ReturnOrder order) : this(collection, order)
		{
			if (compilation is not null)
			{
				TargetCompilation = compilation;
				CanRetrieveData = true;
			}
		}

		private protected SymbolContainer(IEnumerable<IMemberData> collection, ReturnOrder order) : this(collection as IEnumerable<object>, order)
		{
			CanRetrieveData = true;
		}

		private SymbolContainer(IEnumerable<object> collection, ReturnOrder order)
		{
			if (collection is null)
			{
				throw new ArgumentNullException(nameof(collection));
			}

			_collection = collection;
			_arrayHandler = GetArrayHandler();
			Order = order;
		}

		/// <inheritdoc/>
		public abstract void Build(StringBuilder builder);

		/// <inheritdoc/>
		/// <exception cref="InvalidOperationException"><see cref="ISymbol"/>s cannot be converted into <see cref="IMemberData"/>, because <see cref="TargetCompilation"/> wasn't specified for the current container.</exception>
		public ImmutableArray<IMemberData> GetData()
		{
			if (!InitArray())
			{
				return _arrayHandler.ToImmutableData();
			}

			if (_array is IMemberData[] members)
			{
				return _arrayHandler.ToImmutableData(members);
			}

			if (TargetCompilation is null)
			{
				throw new InvalidOperationException("ISymbols cannot be converted into IMemberDatas, because TargetCompilation wasn't specified for the current container");
			}

			ISymbol[] symbols = (_array as ISymbol[])!;
			members = symbols.Select(s => GetData(s, TargetCompilation!)).ToArray();

			return _arrayHandler.ToImmutableData(members);
		}

		/// <inheritdoc/>
		public virtual ImmutableArray<string> GetNames()
		{
			if (!InitArray())
			{
				return ImmutableArray<string>.Empty;
			}

			ImmutableArray<string>.Builder builder = ImmutableArray.CreateBuilder<string>(_array.Length);

			if (_array is IMemberData[] members)
			{
				for (int i = 0; i < _array.Length; i++)
				{
					builder.Add(members[i].Name);
				}
			}
			else
			{
				ISymbol[] symbols = (_array as ISymbol[])!;

				for (int i = 0; i < _array.Length; i++)
				{
					builder.Add(symbols[i].GetVerbatimName());
				}
			}

			return builder.ToImmutableArray();
		}

		/// <inheritdoc/>
		public ImmutableArray<ISymbol> GetSymbols()
		{
			if (!InitArray())
			{
				return _arrayHandler.ToImmutableSymbol();
			}

			if (_array is ISymbol[] symbols)
			{
				return _arrayHandler.ToImmutableSymbol(symbols);
			}

			IMemberData[] members = (_array as IMemberData[])!;

			return _arrayHandler.ToImmutableSymbol(members.Select(m => m.Symbol));
		}

		/// <summary>
		/// Converts the current array into an array of <see cref="ISymbol"/>s.
		/// </summary>
		public ISymbol[] ToArray()
		{
			if (!TryGetArray(out object[]? array))
			{
				return Array.Empty<ISymbol>();
			}

			ISymbol[] newArray = _arrayHandler.CreateArray(array.Length);

			if (array is ISymbol[] symbols)
			{
				symbols.CopyTo(newArray, 0);
				return newArray;
			}

			IMemberData[] members = (array as IMemberData[])!;

			for (int i = 0; i < members.Length; i++)
			{
				newArray[i] = members[i].Symbol;
			}

			return newArray;
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			StringBuilder builder = new();
			Build(builder);
			return builder.ToString();
		}

		/// <summary>
		/// Returns a <see cref="IMemberData"/>s created for the specified <paramref name="symbol"/>.
		/// </summary>
		/// <param name="symbol"><see cref="ISymbol"/>s to create the <see cref="IMemberData"/> for.</param>
		/// <param name="compilation"><see cref="ICompilationData"/> the given <see cref="ISymbol"/> is part of.</param>
		protected abstract IMemberData GetData(ISymbol symbol, ICompilationData compilation);

		IEnumerator IEnumerable.GetEnumerator()
		{
			if (InitArray())
			{
				return _array.GetEnumerator();
			}

			return Array.Empty<object>().GetEnumerator();
		}

		internal static void DefaultBuild(StringBuilder builder, ImmutableArray<string> names)
		{
			if (names.Length == 0)
			{
				return;
			}

			builder.Append(names[0]);

			for (int i = 1; i < names.Length; i++)
			{
				builder.Append('.');
				builder.Append(names[i]);
			}
		}

		private protected abstract IArrayHandler GetArrayHandler();

		private protected bool TryGetArray([NotNullWhen(true)] out object[]? array)
		{
			if (InitArray())
			{
				array = _array;
				return true;
			}

			array = default;
			return false;
		}

		[MemberNotNullWhen(true, nameof(_array))]
		private bool InitArray()
		{
			if (_array is not null)
			{
				return true;
			}

			if (_collection is null)
			{
				return false;
			}

			if (_collection is IEnumerable<ISymbol> symbols)
			{
				_array = _arrayHandler.CreateArray(symbols);
			}
			else
			{
				_array = _arrayHandler.CreateArray((_collection as IEnumerable<IMemberData>)!);
			}

			_collection = default;
			return true;
		}
	}
}
