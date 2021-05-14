using System;
using System.Diagnostics;
using Microsoft.CodeAnalysis;

namespace Durian.Generator
{
	/// <summary>
	/// Specifies that the target <see cref="DiagnosticDescriptor"/> does not have a specific location.
	/// </summary>
	[Conditional("DEBUG")]
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
