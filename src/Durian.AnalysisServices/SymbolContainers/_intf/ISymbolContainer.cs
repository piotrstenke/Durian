// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using Durian.Analysis.Data;
using Durian.Analysis.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Durian.Analysis.SymbolContainers
{
	/// <summary>
	/// Provides methods for returning symbol representations using either <see cref="ISymbol"/>s or <see cref="IMemberData"/>s.
	/// </summary>
	public interface ISymbolContainer
	{
		/// <summary>
		/// Order in which the symbols are returned.
		/// </summary>
		ReturnOrder Order { get; }

		/// <summary>
		/// Writes a string representation of the container into the specified <paramref name="builder"/>.
		/// </summary>
		/// <param name="builder"><see cref="StringBuilder"/> to write to.</param>
		void Build(StringBuilder builder);

		/// <summary>
		/// Returns the <see cref="IMemberData"/>s contained within this instance.
		/// </summary>
		ImmutableArray<IMemberData> GetData();

		/// <summary>
		/// Returns the names of symbols contained within this instance.
		/// </summary>
		ImmutableArray<string> GetNames();

		/// <summary>
		/// Returns the <see cref="ISymbol"/>s contained within this instance.
		/// </summary>
		ImmutableArray<ISymbol> GetSymbols();
	}
}
