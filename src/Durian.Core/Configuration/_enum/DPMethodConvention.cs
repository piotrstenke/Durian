namespace Durian.Configuration
{
	/// <summary>
	/// Determines how a <c>DefaultParam</c> method is generated.
	/// </summary>
	public enum DPMethodConvention
	{
		/// <summary>
		/// Uses default convention, which is <see cref="Copy"/>.
		/// </summary>
		Default = Copy,

		/// <summary>
		/// Copies contents of the method.
		/// </summary>
		Copy = 1,

		/// <summary>
		/// Call the method.
		/// </summary>
		Call = 2
	}
}
