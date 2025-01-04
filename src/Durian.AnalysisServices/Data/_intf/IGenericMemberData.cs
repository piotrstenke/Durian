using Durian.Analysis.SymbolContainers;
using Microsoft.CodeAnalysis;

namespace Durian.Analysis.Data
{
	/// <summary>
	/// <see cref="IMemberData"/> that represents a generic member.
	/// </summary>
	public interface IGenericMemberData : IMemberData
	{
		/// <summary>
		/// Type arguments of this member.
		/// </summary>
		ISymbolContainer<ITypeSymbol, ITypeData> TypeArguments { get; }

		/// <summary>
		/// Type parameters of this member.
		/// </summary>
		ISymbolContainer<ITypeParameterSymbol, ITypeParameterData> TypeParameters { get; }
	}
}
