using Microsoft.CodeAnalysis;

namespace Durian.Analysis.CodeGeneration;

/// <summary>
/// Specifies available sources of an explicit enum field value.
/// </summary>
public enum EnumValueSource
{
	/// <summary>
	/// No source specified.
	/// </summary>
	None = 0,

	/// <summary>
	/// The constant is provided by the caller, increased by 1 for each member.
	/// </summary>
	Constant = 1,

	/// <summary>
	/// The constant value is retrieved from the <see cref="IFieldSymbol.ConstantValue"/> property.
	/// </summary>
	Symbol = 2
}
