using System;
using System.Collections;
using System.Collections.Generic;
using Durian.Analysis.Data;
using Microsoft.CodeAnalysis;

namespace Durian.Analysis.SymbolContainers
{
	public partial class LeveledSymbolContainer<TSymbol, TData> where TSymbol : class, ISymbol
		where TData : class, IMemberData
	{
		private struct ReversedContainerEnumerator : IEnumerator<ISymbolOrMember<TSymbol, TData>>
		{
			private readonly List<ISymbolOrMember<TSymbol, TData>> _data;
			private int _currentIndex;

			public ISymbolOrMember<TSymbol, TData> Current => _data[_currentIndex];

			public int EndIndex { get; }

			object IEnumerator.Current => Current;

			public ReversedContainerEnumerator(List<ISymbolOrMember<TSymbol, TData>> data, int endIndex)
			{
				_data = data;
				EndIndex = endIndex;
				_currentIndex = EndIndex;
			}

			public bool MoveNext()
			{
				if (_currentIndex <= 0)
				{
					return false;
				}

				_currentIndex--;
				return true;
			}

			public void Reset()
			{
				_currentIndex = EndIndex;
			}

			void IDisposable.Dispose()
			{
				// Do nothing/
			}
		}
	}
}
