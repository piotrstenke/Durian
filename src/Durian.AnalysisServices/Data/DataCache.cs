using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;

namespace Durian.Analysis.Data
{
	internal static class DataCache
	{
		private static readonly Dictionary<ISymbol, ISymbolOrMember> _cache = new(SymbolEqualityComparer.Default);

		public static bool TryGetData(ISymbol symbol, [NotNullWhen(true)] out ISymbolOrMember? member)
		{
			return _cache.TryGetValue(symbol, out member);
		}

		public static void AddOrUpdate(ISymbol symbol, ISymbolOrMember member)
		{
			_cache[symbol] = member;
		}
	}
}
