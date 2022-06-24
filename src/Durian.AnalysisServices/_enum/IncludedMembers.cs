// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Durian.Analysis.SymbolContainers;
using Microsoft.CodeAnalysis;

namespace Durian.Analysis
{
	/// <summary>
	/// Specifies all possible sub-sets of an <see cref="ISymbolContainer"/>.
	/// </summary>
	public enum IncludedMembers
	{
		/// <summary>
		/// No members included.
		/// </summary>
		None = 0,

		/// <summary>
		/// Only members that are direct children of the <see cref="ISymbol"/> are included.
		/// </summary>
		Direct = 1,

		/// <summary>
		/// Only members that are children of the <see cref="ISymbol"/> or one of its children are included.
		/// </summary>
		Inner = 2,

		/// <summary>
		/// All child members are included.
		/// </summary>
		All = 3
	}
}
