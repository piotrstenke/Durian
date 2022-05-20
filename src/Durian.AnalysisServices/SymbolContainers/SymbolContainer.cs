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
		private IEnumerable<object>? _collection;
		private object[]? _array;

		/// <inheritdoc/>
		public ReturnOrder Order { get; }

		/// <summary>
		/// <see cref="ICompilationData"/> to use when converting <see cref="ISymbol"/>s to <see cref="IMemberData"/>.
		/// </summary>
		public ICompilationData? TargetCompilation { get; }

		/// <summary>
		/// Determines whether the <see cref="GetData()"/> method is safe to call.
		/// </summary>
		public bool CanRetrieveData { get; }

		private protected SymbolContainer(ReturnOrder order = default)
		{
			_collection = default!;
			Order = order;
		}

		private SymbolContainer(IEnumerable<object> collection, ReturnOrder order)
		{
			if (collection is null)
			{
				throw new ArgumentNullException(nameof(collection));
			}

			_collection = collection;
			Order = order;
		}

		private protected SymbolContainer(IEnumerable<ISymbol> collection, ICompilationData? compilation, ReturnOrder order) : this(collection, order)
		{
			if(compilation is not null)
			{
				TargetCompilation = compilation;
				CanRetrieveData = true;
			}
		}

		private protected SymbolContainer(IEnumerable<IMemberData> collection, ReturnOrder order) : this(collection as IEnumerable<object>, order)
		{
			CanRetrieveData = true;
		}

		/// <inheritdoc/>
		public abstract void Build(StringBuilder builder);

		/// <inheritdoc/>
		/// <exception cref="InvalidOperationException"><see cref="ISymbol"/>s cannot be converted into <see cref="IMemberData"/>, because <see cref="TargetCompilation"/> wasn't specified for the current container.</exception>
		public ImmutableArray<IMemberData> GetData()
		{
			if (!InitArray())
			{
				return ImmutableArray<IMemberData>.Empty;
			}

			if (_array is IMemberData[] members)
			{
				return ImmutableArray.Create(members);
			}

			if (TargetCompilation is null)
			{
				throw new InvalidOperationException("ISymbols cannot be converted into IMemberDatas, because TargetCompilation wasn't specified for the current container");
			}

			ISymbol[] symbols = (_array as ISymbol[])!;
			members = CreateMemberArray(_array.Length);

			ImmutableArray<IMemberData>.Builder builder = ImmutableArray.CreateBuilder<IMemberData>(symbols.Length);

			for (int i = 0; i < symbols.Length; i++)
			{
				IMemberData member = GetData(symbols[i], TargetCompilation!);
				builder.Add(member);
				members[i] = member;
			}

			_array = members;
			return builder.ToImmutable();
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
			if(!InitArray())
			{
				return ImmutableArray<ISymbol>.Empty;
			}

			if(_array is ISymbol[] symbols)
			{
				return ImmutableArray.Create(symbols);
			}

			IMemberData[] members = (_array as IMemberData[])!;

			ImmutableArray<ISymbol>.Builder builder = ImmutableArray.CreateBuilder<ISymbol>(members.Length);

			for (int i = 0; i < members.Length; i++)
			{
				builder.Add(members[i].Symbol);
			}

			return builder.ToImmutable();
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

			ISymbol[] newArray = CreateSymbolArray(array.Length);

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

		private protected virtual ISymbol[] CreateSymbolArray(int length)
		{
			return new ISymbol[length];
		}

		private protected virtual ISymbol[] CreateSymbolArray(IEnumerable<ISymbol> collection)
		{
			return collection.ToArray();
		}

		private protected virtual IMemberData[] CreateMemberArray(int length)
		{
			return new IMemberData[length];
		}

		private protected virtual IMemberData[] CreateMemberArray(IEnumerable<IMemberData> collection)
		{
			return collection.ToArray();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			if(InitArray())
			{
				return _array.GetEnumerator();
			}

			return Array.Empty<object>().GetEnumerator();
		}

		private protected bool TryGetArray([NotNullWhen(true)]out object[]? array)
		{
			if(InitArray())
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
			if(_array is not null)
			{
				return true;
			}

			if(_collection is null)
			{
				return false;
			}

			if(_collection is IEnumerable<ISymbol> symbols)
			{
				_array = CreateSymbolArray(symbols.ToArray());
			}
			else
			{
				_array = CreateMemberArray((_collection as IEnumerable<IMemberData>)!);
			}

			_collection = default;
			return true;
		}

		internal static void DefaultBuild(StringBuilder builder, ImmutableArray<string> names)
		{
			if (names.Length == 0)
			{
				return;
			}

			builder.Append(names[1]);

			for (int i = 1; i < names.Length; i++)
			{
				builder.Append('.');
				builder.Append(names[i]);
			}
		}
	}
}
