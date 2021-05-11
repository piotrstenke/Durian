using System.Collections.Generic;
using System.Diagnostics;

namespace Durian.DefaultParam
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
		public int TypeConvention { get; set; }

		/// <summary>
		/// Determines, how the <c>DefaultParam</c> generator generates a method.
		/// </summary>
		public int MethodConvention { get; set; }

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
			hashCode = (hashCode * -1521134295) + EqualityComparer<bool>.Default.GetHashCode(ApplyNewModifierWhenPossible);
			hashCode = (hashCode * -1521134295) + EqualityComparer<int>.Default.GetHashCode(MethodConvention);
			hashCode = (hashCode * -1521134295) + EqualityComparer<int>.Default.GetHashCode(TypeConvention);
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
