using System;
using System.Diagnostics;

namespace Durian.Generator
{
	/// <summary>
	/// Specifies that the target member was moved from another location.
	/// </summary>
	[Conditional("DEBUG")]
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface | AttributeTargets.Method | AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Enum | AttributeTargets.Delegate | AttributeTargets.Event, AllowMultiple = false, Inherited = false)]
	public sealed class MovedFromAttribute : Attribute
	{
		/// <summary>
		/// Location the target member was moved from.
		/// </summary>
		public string Source { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="MovedFromAttribute"/> class.
		/// </summary>
		/// <param name="source">Location the target member was moved from.</param>
		public MovedFromAttribute(string source)
		{
			Source = source;
		}
	}
}
