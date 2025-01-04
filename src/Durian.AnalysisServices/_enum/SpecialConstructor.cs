namespace Durian.Analysis
{
	/// <summary>
	/// Defines kinds of special constructors.
	/// </summary>
	public enum SpecialConstructor
	{
		/// <summary>
		/// The constructor is not special.
		/// </summary>
		None = 0,

		/// <summary>
		/// The constructor is a default constructor.
		/// </summary>
		Default = 1,

		/// <summary>
		/// The constructor is a parameterless constructor.
		/// </summary>
		Parameterless = 2,

		/// <summary>
		/// The constructor is a copy constructor.
		/// </summary>
		Copy = 3,

		/// <summary>
		/// The constructor is a static constructor.
		/// </summary>
		Static = 4
	}
}
