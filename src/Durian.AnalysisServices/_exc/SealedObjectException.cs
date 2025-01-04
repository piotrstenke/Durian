using System;
using System.Diagnostics.CodeAnalysis;

namespace Durian.Analysis
{
	/// <summary>
	/// Exception thrown when there was an attempt to modify a sealed object.
	/// </summary>
	[SuppressMessage("Roslynator", "RCS1194:Implement exception constructors.")]
	public class SealedObjectException : Exception
	{
		/// <summary>
		/// Object that was attempted to modify.
		/// </summary>
		public ISealable SealedObject { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="SealedObjectException"/> class.
		/// </summary>
		/// <param name="sealedObject">Object that was attempted to modify.</param>
		public SealedObjectException(ISealable sealedObject)
		{
			SealedObject = sealedObject;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="SealedObjectException"/> class.
		/// </summary>
		/// <param name="sealedObject">Object that was attempted to modify.</param>
		/// <param name="message">Message explaining the reason for the exception.</param>
		public SealedObjectException(ISealable sealedObject, string message) : base(message)
		{
			SealedObject = sealedObject;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="SealedObjectException"/> class.
		/// </summary>
		/// <param name="sealedObject">Object that was attempted to modify.</param>
		/// <param name="message">Message explaining the reason for the exception.</param>
		/// <param name="innerException"><see cref="Exception"/> that caused this exception.</param>
		public SealedObjectException(ISealable sealedObject, string message, Exception innerException) : base(message, innerException)
		{
			SealedObject = sealedObject;
		}
	}
}
