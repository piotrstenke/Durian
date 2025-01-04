namespace Durian.Analysis.Logging;

/// <summary>
/// Determines what to output when a node is being logged.
/// </summary>
public enum NodeOutput
{
	/// <summary>
	/// Uses the default value specified in configuration.
	/// </summary>
	Default,

	/// <summary>
	/// Outputs only the target node.
	/// </summary>
	Node,

	/// <summary>
	/// Outputs containing node of the node.
	/// </summary>
	Containing,

	/// <summary>
	/// Outputs whole syntax tree associated with the node.
	/// </summary>
	SyntaxTree
}
