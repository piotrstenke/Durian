// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Text;

namespace Durian.Builder
{
	/// <summary>
	/// <see cref="ICodeReceiver"/> that uses a <see cref="StringBuilder"/> as a buffer for the generated code.
	/// </summary>
	public class CodeReceiver : ICodeReceiver
	{
		/// <summary>
		/// <see cref="StringBuilder"/> the code is written to.
		/// </summary>
		public StringBuilder Buffer { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="CodeReceiver"/> class.
		/// </summary>
		public CodeReceiver()
		{
			Buffer = new();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="CodeReceiver"/> class.
		/// </summary>
		/// <param name="buffer"><see cref="StringBuilder"/> the code is written to.</param>
		/// <exception cref="ArgumentNullException"><paramref name="buffer"/> is <see langword="null"/>.</exception>
		public CodeReceiver(StringBuilder buffer)
		{
			if(buffer is null)
			{
				throw new ArgumentNullException(nameof(buffer));
			}

			Buffer = buffer;
		}

		/// <inheritdoc/>
		public void Write(string value)
		{
			Buffer.Append(value);
		}

		/// <inheritdoc/>
		public void Write(char value)
		{
			Buffer.Append(value);
		}

		/// <inheritdoc/>
		public void WriteLine()
		{
			Buffer.AppendLine();
		}

		/// <inheritdoc/>
		public void WriteLine(string value)
		{
			Buffer.AppendLine(value);
		}

		/// <inheritdoc/>
		public void WriteLine(char value)
		{
			Buffer.Append(value).AppendLine();
		}
	}
}
