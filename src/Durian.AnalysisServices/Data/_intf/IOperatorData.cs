using Microsoft.CodeAnalysis;

namespace Durian.Analysis.Data
{
	/// <summary>
	/// Encapsulates data associated with a single <see cref="IMethodSymbol"/> representing an operator.
	/// </summary>
	public interface IOperatorData : IMethodData, ISymbolOrMember<IMethodSymbol, IOperatorData>
	{
		/// <summary>
		/// Kind of this operator.
		/// </summary>
		OverloadableOperator OperatorKind { get; }

		/// <summary>
		/// Creates a shallow copy of the current data.
		/// </summary>
		new IOperatorData Clone();
	}
}