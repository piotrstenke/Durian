// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;

namespace Durian.Analysis
{
	/// <summary>
	/// Exception thrown when there was an attempt to modify a sealed object.
	/// </summary>
	public class SealedObjectException : Exception
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="SealedObjectException"/> class.
		/// </summary>
		public SealedObjectException()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="SealedObjectException"/> class.
		/// </summary>
		/// <param name="message">Message explaining the reason for the exception.</param>
		public SealedObjectException(string message) : base(message)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="SealedObjectException"/> class.
		/// </summary>
		/// <param name="message">Message explaining the reason for the exception.</param>
		/// <param name="innerException"><see cref="Exception"/> that caused this exception.</param>
		public SealedObjectException(string message, Exception innerException) : base(message, innerException)
		{
		}
	}
}
