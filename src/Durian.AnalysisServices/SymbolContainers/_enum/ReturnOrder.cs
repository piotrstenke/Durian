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
		/// Parent value is returned first, followed by its children.
		/// </summary>
		ParentToChild = 0,

		/// <summary>
		/// Parent value is returned last, proceeded by its children.
		/// </summary>
		ChildToParent = 1
	}
}
