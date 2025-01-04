using System;

namespace Durian.Analysis.SymbolContainers;

/// <summary>
/// Exception thrown when there was an attempt to retrieve data from an empty container.
/// </summary>
public class EmptyContainerException : Exception
{
	/// <summary>
	/// Initializes a new instance of the <see cref="EmptyContainerException"/> class.
	/// </summary>
	public EmptyContainerException()
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="EmptyContainerException"/> class.
	/// </summary>
	/// <param name="message">Message explaining the reason for the exception.</param>
	public EmptyContainerException(string message) : base(message)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="EmptyContainerException"/> class.
	/// </summary>
	/// <param name="message">Message explaining the reason for the exception.</param>
	/// <param name="innerException"><see cref="Exception"/> that caused this exception.</param>
	public EmptyContainerException(string message, Exception innerException) : base(message, innerException)
	{
	}
}
