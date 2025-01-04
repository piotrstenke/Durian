using System;
using System.Diagnostics;
using Microsoft.CodeAnalysis;

namespace Durian.Analysis
{
	/// <summary>
	/// Specifies that the target <see cref="DiagnosticDescriptor"/> does not have a specific location.
	/// </summary>
	[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
	[Conditional("DEBUG")]
	internal sealed class WithoutLocationAttribute : Attribute
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="WithoutLocationAttribute"/> class.
		/// </summary>
		public WithoutLocationAttribute()
		{
		}
	}
}
