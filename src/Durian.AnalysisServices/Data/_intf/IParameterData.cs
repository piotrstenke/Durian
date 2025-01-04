using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.Analysis.Data
{
	/// <summary>
	/// Encapsulates data associated with a single <see cref="IParameterSymbol"/>.
	/// </summary>
	public interface IParameterData : IMemberData, ISymbolOrMember<IParameterSymbol, IParameterData>
	{
		/// <summary>
		/// Target <see cref="BaseParameterSyntax"/>.
		/// </summary>
		new BaseParameterSyntax Declaration { get; }

		/// <summary>
		/// <see cref="IParameterSymbol"/> associated with the <see cref="Declaration"/>.
		/// </summary>
		new IParameterSymbol Symbol { get; }

		/// <summary>
		/// Creates a shallow copy of the current data.
		/// </summary>
		new IParameterData Clone();
	}
}
