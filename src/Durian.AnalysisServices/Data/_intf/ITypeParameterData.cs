using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.Analysis.Data
{
	/// <summary>
	/// Encapsulates data associated with a single <see cref="IParameterSymbol"/>.
	/// </summary>
	public interface ITypeParameterData : IMemberData, ISymbolOrMember<ITypeParameterSymbol, ITypeParameterData>
	{
		/// <summary>
		/// <see cref="TypeParameterConstraintSyntax"/> associated with the <see cref="Declaration"/>.
		/// </summary>
		TypeParameterConstraintClauseSyntax? ConstraintClause { get; }

		/// <summary>
		/// Generic constraints applied to this type parameter.
		/// </summary>
		GenericConstraint Constraints { get; }

		/// <summary>
		/// Target <see cref="TypeParameterSyntax"/>.
		/// </summary>
		new TypeParameterSyntax Declaration { get; }

		/// <summary>
		/// <see cref="ITypeParameterSymbol"/> associated with the <see cref="Declaration"/>.
		/// </summary>
		new ITypeParameterSymbol Symbol { get; }

		/// <summary>
		/// Creates a shallow copy of the current data.
		/// </summary>
		new ITypeParameterData Clone();
	}
}
