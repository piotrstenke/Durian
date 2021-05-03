using System;

namespace Durian.Generator
{
	/// <summary>
	/// Specifies that the target member was generated from the <see cref="Source"/>.
	/// </summary>
	[AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = false)]
	public sealed class GeneratedFromAttribute : Attribute
	{
		/// <summary>
		/// Member this code was generated from.
		/// </summary>
		public string Source { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="GeneratedFromAttribute"/> class.
		/// </summary>
		/// <param name="source">Member this code was generated from.</param>
		public GeneratedFromAttribute(string source)
		{
			Source = source;
		}
	}
}
