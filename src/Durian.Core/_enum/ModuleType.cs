namespace Durian.Generator
{
	/// <summary>
	/// Defines type of a Durian module.
	/// </summary>
	public enum ModuleType
	{
		/// <summary>
		/// Represents a library module. Libraries don't perform any actual code-related actions.
		/// </summary>
		Library,

		/// <summary>
		/// Represents an analyzer module. Analyzers examine existing code and potentially provide code-fixes.
		/// </summary>
		Analyzer,

		/// <summary>
		/// Represents a static generator module. Static generators always build the same code - for different inputs the output will be always the same.
		/// </summary>
		StaticGenerator,

		/// <summary>
		/// Represents a syntax based generator module. Syntax based generators examine existing code and generate new nodes based on collected information.
		/// </summary>
		SyntaxBasedGenerator,

		/// <summary>
		/// Represents a file based generator module. File based generators work on external files instead of actual C# source code.
		/// </summary>
		FileBasedGenerator
	}
}
