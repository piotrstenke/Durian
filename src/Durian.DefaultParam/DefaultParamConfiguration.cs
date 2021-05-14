using Durian.Configuration;

namespace Durian.Generator.DefaultParam
{
	/// <summary>
	/// Configures optional features of the <see cref="DefaultParamGenerator"/>.
	/// </summary>
	public sealed record DefaultParamConfiguration
	{
		/// <summary>
		/// Determines whether to apply the <see langword="new"/> modifier to the generated member when possible instead of reporting an error. Defaults to <see langword="false"/>.
		/// </summary>
		public bool ApplyNewModifierWhenPossible { get; set; }

		/// <summary>
		/// Determines, how the <c>DefaultParam</c> generator generates a type.
		/// </summary>
		public DPTypeConvention TypeConvention { get; set; }

		/// <summary>
		/// Determines, how the <c>DefaultParam</c> generator generates a method.
		/// </summary>
		public DPMethodConvention MethodConvention { get; set; }

		/// <summary>
		/// Returns a new instance of <see cref="DefaultParamConfiguration"/> with all values set to <see langword="default"/>.
		/// </summary>
		public static DefaultParamConfiguration Default => new();

		/// <summary>
		/// Initializes a new instance of the <see cref="DefaultParamConfiguration"/> class.
		/// </summary>
		public DefaultParamConfiguration()
		{
		}

		/// <inheritdoc/>
		public override int GetHashCode()
		{
			int hashCode = -726504116;
			hashCode = (hashCode * -1521134295) + ApplyNewModifierWhenPossible.GetHashCode();
			hashCode = (hashCode * -1521134295) + MethodConvention.GetHashCode();
			hashCode = (hashCode * -1521134295) + TypeConvention.GetHashCode();
			return hashCode;
		}

#pragma warning disable RCS1132 // Remove redundant overriding member.
		/// <inheritdoc/>
		public override string ToString()
		{
			return base.ToString();
		}
#pragma warning restore RCS1132 // Remove redundant overriding member.
	}
}
