// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

namespace Durian.Analysis
{
	/// <summary>
	/// Represents stage of source generation.
	/// </summary>
	public enum GeneratorState
	{
		/// <summary>
		/// No valid state.
		/// </summary>
		None,

		/// <summary>
		/// The generator is still running.
		/// </summary>
		Running,

		/// <summary>
		/// The generator pass has successfully ended.
		/// </summary>
		Success,

		/// <summary>
		/// The generator pass has failed.
		/// </summary>
		Failed
	}
}
