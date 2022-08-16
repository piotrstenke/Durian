// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Diagnostics.CodeAnalysis;

namespace Durian.Analysis
{
	/// <summary>
	/// Thrown when there was an error when building an object using the builder pattern.
	/// </summary>
	[SuppressMessage("Roslynator", "RCS1194:Implement exception constructors.")]
	public class BuilderException : Exception
	{
		/// <summary>
		/// Target builder.
		/// </summary>
		public IBuilder<object> Builder { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="BuilderException"/> class.
		/// </summary>
		/// <param name="builder">Target builder.</param>
		public BuilderException(IBuilder<object> builder)
		{
			Builder = builder;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="BuilderException"/> class.
		/// </summary>
		/// <param name="builder">Target builder.</param>
		/// <param name="message">Message explaining the reason for the exception.</param>
		public BuilderException(IBuilder<object> builder, string message) : base(message)
		{
			Builder = builder;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="BuilderException"/> class.
		/// </summary>
		/// <param name="builder">Target builder.</param>
		/// <param name="message">Message explaining the reason for the exception.</param>
		/// <param name="innerException"><see cref="Exception"/> that caused this exception.</param>
		public BuilderException(IBuilder<object> builder, string message, Exception innerException) : base(message, innerException)
		{
			Builder = builder;
		}
	}
}
