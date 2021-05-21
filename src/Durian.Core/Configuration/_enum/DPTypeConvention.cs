namespace Durian.Configuration
{
	/// <summary>
	/// Determines how a <c>DefaultParam</c> type is generated.
	/// </summary>
	public enum DPTypeConvention
	{
		/// <summary>
		/// Uses default convention, which is <see cref="Copy"/>.
		/// </summary>
		Default = Copy,

		/// <summary>
		/// Copies contents of the type.
		/// </summary>
		Copy = 1,

		/// <summary>
		/// Inherits the type.
		/// </summary>
		Inherit = 2
	}
}
