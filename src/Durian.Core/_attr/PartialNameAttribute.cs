// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Diagnostics;

namespace Durian
{
	/// <summary>
	/// Applies a debug-only name to a partial part of a type.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface, AllowMultiple = true, Inherited = false)]
	[Conditional("DEBUG")]
	public sealed class PartialNameAttribute : Attribute
	{
		/// <summary>
		/// Name of the partial part.
		/// </summary>
		public string Name { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="PartialNameAttribute"/> class.
		/// </summary>
		/// <param name="name">Name of the partial part.</param>
		public PartialNameAttribute(string name)
		{
			Name = name;
		}
	}
}
