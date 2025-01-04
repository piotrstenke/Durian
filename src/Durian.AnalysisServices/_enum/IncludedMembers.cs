using Durian.Analysis.SymbolContainers;
using Microsoft.CodeAnalysis;

namespace Durian.Analysis
{
	/// <summary>
	/// Specifies all possible sub-sets of an <see cref="ISymbolContainer"/>.
	/// </summary>
	public enum IncludedMembers
	{
		/// <summary>
		/// No members included.
		/// </summary>
		None = 0,

		/// <summary>
		/// Members that are direct children of the <see cref="ISymbol"/> are included.
		/// </summary>
		Direct = 1,

		/// <summary>
		/// Members that are inherited by the <see cref="ISymbol"/> are included.
		/// </summary>
		Inherited = 2,

		/// <summary>
		/// Members that are children of the <see cref="ISymbol"/> or one of its children are included.
		/// </summary>
#pragma warning disable RCS1234 // Duplicate enum value.
#pragma warning disable CA1069 // Enums values should not be duplicated
		Inner = 2,
#pragma warning restore CA1069 // Enums values should not be duplicated
#pragma warning restore RCS1234 // Duplicate enum value.

		/// <summary>
		/// All child members are included.
		/// </summary>
		All = 3
	}
}
