// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

namespace Durian.Analysis
{
	/// <summary>
	/// Defines available constructor initializer targets.
	/// </summary>
	public enum ConstructorInitializer
	{
		/// <summary>
		/// The constructor does not have an initializer.
		/// </summary>
		None = 0,

		/// <summary>
		/// The constructor uses the '<see langword="base"/>' initializer.
		/// </summary>
		Base = 1,

		/// <summary>
		/// The constructor uses the '<see langword="this"/>' initializer.
		/// </summary>
		This = 2
	}
}
