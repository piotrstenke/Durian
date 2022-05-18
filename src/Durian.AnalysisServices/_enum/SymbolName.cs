// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Microsoft.CodeAnalysis;

namespace Durian.Analysis
{
	/// <summary>
	/// Determines format of name of a single <see cref="ISymbol"/>.
	/// </summary>
	public enum SymbolName
	{
		/// <summary>
		/// Includes only actual name of the symbol, excluding type parameters/arguments.
		/// </summary>
		/// <remarks>For built-in types includes only their aliased names (e.g. <see langword="int"/> instead of <c>Int32</c>.</remarks>
		Default = 0,

		/// <summary>
		/// Type parameters of the symbol are included in the name.
		/// </summary>
		Generic = 1,

		/// <summary>
		/// Type parameters of the symbol are included in the name along with their variance.
		/// </summary>
		VarianceGeneric = 2,

		/// <summary>
		/// Type arguments are used in place of type parameters.
		/// </summary>
		Substituted = 3,

		/// <summary>
		/// For built-in types includes only their actual, non-aliased names (e.g. <c>Int32</c> instead of <see langword="int"/>).
		/// </summary>
		SystemName = 4,

		/// <summary>
		/// Removes the 'Attribute' suffix from the name.
		/// </summary>
		Attribute = 5
	}
}
