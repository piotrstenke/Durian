namespace Durian.Analysis
{
	/// <summary>
	/// Defines all possible property accessor kinds.
	/// </summary>
	public enum PropertyAccessorKind
	{
		/// <summary>
		/// Member is not an accessor.
		/// </summary>
		None = 0,

		/// <summary>
		/// Represents the <see langword="get"/> accessor.
		/// </summary>
		Get = 1,

		/// <summary>
		/// Represents the <see langword="set"/> accessor.
		/// </summary>
		Set = 2,

		/// <summary>
		/// Represents the <see langword="init"/> accessor.
		/// </summary>
		Init = 3,
	}
}
