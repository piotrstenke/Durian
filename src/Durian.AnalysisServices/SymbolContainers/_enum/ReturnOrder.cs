// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

namespace Durian.Analysis.SymbolContainers
{
	/// <summary>
	/// Specifies order at which members should be returned.
	/// </summary>
	public enum ReturnOrder
	{
		/// <summary>
		/// Root is returned first.
		/// </summary>
		Root = 0,

		/// <summary>
		/// Root is returned last.
		/// </summary>
		Parent = 1
	}
}
