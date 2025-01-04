namespace Durian.Analysis
{
	/// <summary>
	/// Defines all attributes that are considered to be part of the nullable reference types features.
	/// </summary>
	public enum NullableAnnotationAttribute
	{
		/// <summary>
		/// The attribute is not a nullable annotation attribute.
		/// </summary>
		None = 0,

		/// <summary>
		/// The attribute is <c>System.Diagnostics.CodeAnalysis.AllowNullAttribute</c>.
		/// </summary>
		AllowNull = 1,

		/// <summary>
		/// The attribute is <c>System.Diagnostics.CodeAnalysis.DisallowNullAttribute</c>.
		/// </summary>
		DisallowNull = 2,

		/// <summary>
		/// The attribute is <c>System.Diagnostics.CodeAnalysis.MaybeNullAttribute</c>.
		/// </summary>
		MaybeNull = 3,

		/// <summary>
		/// The attribute is <c>System.Diagnostics.CodeAnalysis.NotNullAttribute</c>.
		/// </summary>
		NotNull = 4,

		/// <summary>
		/// The attribute is <c>System.Diagnostics.CodeAnalysis.NotNullWhenAttribute</c>.
		/// </summary>
		NotNullWhen = 5,

		/// <summary>
		/// The attribute is <c>System.Diagnostics.CodeAnalysis.NotNullIfNotNullAttribute</c>.
		/// </summary>
		NotNullIfNotNull = 6,

		/// <summary>
		/// The attribute is <c>System.Diagnostics.CodeAnalysis.MemberNotNullAttribute</c>.
		/// </summary>
		MemberNotNull = 7,

		/// <summary>
		/// The attribute is <c>System.Diagnostics.CodeAnalysis.MemberNotNullWhenAttribute</c>.
		/// </summary>
		MemberNotNullWhen = 8,
	}
}
