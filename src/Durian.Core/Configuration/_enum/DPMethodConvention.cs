// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

namespace Durian.Configuration
{
	/// <summary>
	/// Determines how a <c>DefaultParam</c> method is generated.
	/// </summary>
	public enum DPMethodConvention
	{
		/// <summary>
		/// Uses default convention, which is <see cref="Call"/>.
		/// </summary>
		Default = Call,

		/// <summary>
		/// Copies contents of the method.
		/// </summary>
		Call = 1,

		/// <summary>
		/// Call the method.
		/// </summary>
		Copy = 2
	}
}
