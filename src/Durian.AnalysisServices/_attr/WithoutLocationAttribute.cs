// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

#if DEBUG

using System;
using Microsoft.CodeAnalysis;

namespace Durian.Analysis
{
	/// <summary>
	/// Specifies that the target <see cref="DiagnosticDescriptor"/> does not have a specific location.
	/// </summary>
	[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
	public sealed class WithoutLocationAttribute : Attribute
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="WithoutLocationAttribute"/> class.
		/// </summary>
		public WithoutLocationAttribute()
		{
		}
	}
}

#endif