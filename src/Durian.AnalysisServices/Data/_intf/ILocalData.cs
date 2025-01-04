using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.Analysis.Data
{
	/// <summary>
	/// Encapsulates data associated with a single <see cref="ILocalSymbol"/>.
	/// </summary>
	public interface ILocalData : IMemberData, IVariableDeclarator<LocalDeclarationStatementSyntax>, ISymbolOrMember<ILocalSymbol, ILocalData>
	{
		/// <summary>
		/// <see cref="ILocalSymbol"/> associated with the <see cref="IMemberData.Declaration"/>.
		/// </summary>
		new ILocalSymbol Symbol { get; }

		/// <summary>
		/// Creates a shallow copy of the current data.
		/// </summary>
		new ILocalData Clone();

		/// <summary>
		/// Returns a collection of <see cref="ILocalSymbol"/>s of all variables defined in the <see cref="IVariableDeclarator{T}.Declaration"/>.
		/// </summary>
		IEnumerable<ISymbolOrMember<ILocalSymbol, ILocalData>> GetUnderlayingLocals();
	}
}
