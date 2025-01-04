namespace Durian.Analysis.CodeGeneration;

/// <summary>
/// Defines available kinds of a method body.
/// </summary>
public enum MethodStyle
{
	/// <summary>
	/// Method does not have a body.
	/// </summary>
	None = 0,

	/// <summary>
	/// Method uses a code block as a body.
	/// </summary>
	Block = 1,

	/// <summary>
	/// Method uses an arrow expression '=>' as a body.
	/// </summary>
	Expression = 2
}
