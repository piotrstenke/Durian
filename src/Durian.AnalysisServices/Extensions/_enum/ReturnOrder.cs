// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

namespace Durian.Generator.Extensions
{
	/// <summary>
	/// Specifies order at which members should be returned.
	/// </summary>
	public enum ReturnOrder
	{
		/// <summary>
		/// Root is returned first.
		/// </summary>
		Root = 1,

		/// <summary>
		/// Root is returned last.
		/// </summary>
		Parent = 2
	}
}
