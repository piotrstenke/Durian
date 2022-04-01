// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

namespace Durian.Analysis
{
	/// <summary>
	/// Provides a mechanism for prohibiting and permitting modifications to the object.
	/// </summary>
	public interface ISealable
	{
		/// <summary>
		/// Determines whether the object is not allowed to be modified.
		/// </summary>
		bool IsSealed { get; }

		/// <summary>
		/// Prohibits this object to be modified.
		/// </summary>
		void Seal();

		/// <summary>
		/// Permits this object to be modified.
		/// </summary>
		void Unseal();
	}
}
