using System;

namespace Durian.Info
{
	/// <summary>
	/// Defines type of a Durian module.
	/// </summary>
	[Flags]
	public enum ModuleType
	{
		/// <summary>
		/// Unknown module type.
		/// </summary>
		Unknown = 0,

		/// <summary>
		/// Represents a library module. Libraries don't perform any actual code-related actions.
		/// </summary>
		Library = 1,

		/// <summary>
		/// Represents an analyzer module. Analyzers examine existing code and potentially provide code-fixes.
		/// </summary>
		Analyzer = 2,

		/// <summary>
		/// Represents a static generator module. Static generators always build the same code - for different inputs the output will be always the same.
		/// </summary>
		StaticGenerator = 4,

		/// <summary>
		/// Represents a syntax based generator module. Syntax based generators examine existing code and generate new nodes based on collected information.
		/// </summary>
		SyntaxBasedGenerator = 8,

		/// <summary>
		/// Represents a file based generator module. File based generators work on external files instead of actual C# source code.
		/// </summary>
		FileBasedGenerator = 16,

		/// <summary>
		/// Represents a code fix module. Code fix modules are basically libraries of non-abstract code fixes.
		/// </summary>
		CodeFixLibrary = 32
	}
}
