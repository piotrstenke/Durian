using System;
using System.Diagnostics;

namespace Durian.Configuration
{
	/// <summary>
	/// Configures how the <see cref="DefaultParamAttribute"/> behaves in the current assembly.
	/// </summary>
	[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface | AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
	[Conditional("DEBUG")]
	public sealed class DefaultParamConfigurationAttribute : Attribute
	{
		/// <inheritdoc cref="DPMethodConvention"/>
		public DPMethodConvention MethodConvention { get; set; }

		/// <inheritdoc cref="DPTypeConvention"/>
		public DPTypeConvention TypeConvention { get; set; }

		/// <summary>
		/// Determines whether to apply the <see langword="new"/> modifier to the generated member when possible instead of reporting an error. Defaults to <see langword="false"/>.
		/// </summary>
		public bool ApplyNewModifierWhenPossible { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="DefaultParamConfigurationAttribute"/> class.
		/// </summary>
		public DefaultParamConfigurationAttribute()
		{
		}
	}
}
