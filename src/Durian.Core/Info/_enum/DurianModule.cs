namespace Durian.Info
{
	/// <summary>
	/// Defines all modules a package can be part of.
	/// </summary>
	public enum DurianModule
	{
		/// <summary>
		/// This package does not belong to any Durian module.
		/// </summary>
		None = 0,

		/// <summary>
		/// Represents the <c>Durian.Core</c> module.
		/// </summary>
		Core = 1,

		/// <summary>
		/// Represents the <c>Durian.DefaultParam</c> module.
		/// </summary>
		DefaultParam = 2,
	}
}
