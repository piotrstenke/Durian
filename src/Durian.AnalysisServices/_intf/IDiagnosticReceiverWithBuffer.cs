// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Microsoft.CodeAnalysis;

namespace Durian.Analysis
{
	/// <summary>
	/// <see cref="IDiagnosticReceiver"/> that writes the <see cref="Diagnostic"/>s into an internal buffer.
	/// </summary>
	public interface IDiagnosticReceiverWithBuffer : IDiagnosticReceiver
	{
		/// <summary>
		/// Number of <see cref="Diagnostic"/>s that were reported after the last call to <see cref="Push"/>.
		/// </summary>
		int Count { get; }

		/// <summary>
		/// Removes all <see cref="Diagnostic"/>s from the internal buffer.
		/// </summary>
		void Clear();

		/// <summary>
		/// Actually writes the <see cref="Diagnostic"/> to the target buffer.
		/// </summary>
		void Push();
	}
}