using System;

namespace Durian.Generator.Logging
{
	/// <summary>
	/// Disables generator logging for the entire assembly and all its references.
	/// </summary>
	[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false, Inherited = false)]
	public sealed class GloballyDisableGeneratorLoggingAttribute : Attribute
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="GloballyDisableGeneratorLoggingAttribute"/> class.
		/// </summary>
		public GloballyDisableGeneratorLoggingAttribute()
		{
		}
	}
}
