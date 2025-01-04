namespace Durian.Analysis
{
	/// <summary>
	/// Defines all possible kinds of literals.
	/// </summary>
	public enum LiteralKind
	{
		/// <summary>
		/// Not a literal.
		/// </summary>
		None = 0,

		/// <summary>
		/// Numeric literal.
		/// </summary>
		Number = 1,

		/// <summary>
		/// A <see cref="string"/> literal.
		/// </summary>
		String = 2,

		/// <summary>
		/// A <see cref="char"/> literal.
		/// </summary>
		Character = 3,

		/// <summary>
		/// The <see langword="true"/> literal.
		/// </summary>
		True = 4,

		/// <summary>
		/// The <see langword="false"/> literal.
		/// </summary>
		False = 5,

		/// <summary>
		/// The <see langword="null"/> literal.
		/// </summary>
		Null = 6,

		/// <summary>
		/// The <see langword="default"/> literal.
		/// </summary>
		Default = 7,

		/// <summary>
		/// The <see langword="__arglist"/> literal.
		/// </summary>
		ArgList = 8,
	}
}
