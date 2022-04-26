// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

namespace Durian.Builder
{
	/// <summary>
	/// Base interface of all code builders.
	/// </summary>
	public interface ICodeBuilder
	{
		/// <summary>
		/// <see cref="ICodeReceiver"/> the generated code is written to.
		/// </summary>
		ICodeReceiver CodeReceiver { get; }
	}
}