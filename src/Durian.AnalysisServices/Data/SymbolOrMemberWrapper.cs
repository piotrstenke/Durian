using System;
using System.Diagnostics;
using Durian.Analysis.Extensions;
using Microsoft.CodeAnalysis;

namespace Durian.Analysis.Data
{
	/// <summary>
	/// Simple wrapper for <see cref="ISymbol"/> that implements the <see cref="ISymbolOrMember"/> interface.
	/// </summary>
	[DebuggerDisplay("{Symbol ?? string.Empty}")]
	internal class SymbolOrMemberWrapper<TSymbol, TData> : ISymbolOrMember<TSymbol, TData>
		where TSymbol : class, ISymbol
		where TData : class, IMemberData
	{
		private TData? _member;

		/// <inheritdoc/>
		public bool HasMember { get; private set; }

		/// <inheritdoc/>
		/// <exception cref="InvalidOperationException"><see cref="Symbol"/> cannot be converted to <see cref="IMemberData"/>, because no parent <see cref="Compilation"/> was specified</exception>
		public TData Member
		{
			get
			{
				if (_member is null)
				{
					if (Compilation is null)
					{
						throw new InvalidOperationException($"Symbol '{Symbol}' cannot be converted to IMemberData, because no parent compilation was specified");
					}

					if (Symbol.ToData(Compilation) is not TData data)
					{
						throw new InvalidOperationException($"Symbol data is not of type '{typeof(TData)}'");
					}

					_member = data;
					HasMember = true;
				}

				return _member;
			}
		}

		/// <inheritdoc/>
		public TSymbol Symbol { get; }

		/// <summary>
		/// Parent compilation of the current member.
		/// </summary>
		public ICompilationData? Compilation { get; set; }

		IMemberData ISymbolOrMember.Member => Member;

		ISymbol ISymbolOrMember.Symbol => Symbol;

		/// <summary>
		/// Initializes a new instance of the <see cref="SymbolOrMemberWrapper{TSymbol, TData}"/> class.
		/// </summary>
		/// <param name="symbol"><see cref="ISymbol"/> of the current member.</param>
		/// <param name="compilation">Parent compilation of the current member.</param>
		public SymbolOrMemberWrapper(TSymbol symbol, ICompilationData? compilation = default)
		{
			Symbol = symbol;
			Compilation = compilation;
		}
	}
}
