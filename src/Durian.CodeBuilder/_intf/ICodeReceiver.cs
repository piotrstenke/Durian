// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

namespace Durian.Builder
{
	/// <summary>
	/// A buffer that generated code can be written to.
	/// </summary>
	public interface ICodeReceiver
	{
		/// <summary>
		/// Writes the specified <paramref name="value"/> to the buffer.
		/// </summary>
		/// <param name="value">Value to write to the buffer.</param>
		void Write(string value);

		/// <summary>
		/// Writes the specified <paramref name="value"/> to the buffer.
		/// </summary>
		/// <param name="value">Value to write to the buffer.</param>
		void Write(char value);

		/// <summary>
		/// Writes an empty line.
		/// </summary>
		void WriteLine();

		/// <summary>
		/// Writes the specified <paramref name="value"/> to the buffer, followed by a new line.
		/// </summary>
		/// <param name="value">Value to write to the buffer.</param>
		void WriteLine(string value);

		/// <summary>
		/// Writes the specified <paramref name="value"/> to the buffer, followed by a new line.
		/// </summary>
		/// <param name="value">Value to write to the buffer.</param>
		void WriteLine(char value);
	}
}