// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

namespace Durian.Analysis.CodeGeneration
{
	/// <summary>
	/// Configures style of the generated code.
	/// </summary>
	public sealed record CodeBuilderStyleConfiguration
	{
		/// <summary>
		/// Determines how interface members are written.
		/// </summary>
		public InterfaceMemberStyle InterfaceMemberStyle { get; set; }

		/// <summary>
		/// Type of preferred namespace declaration.
		/// </summary>
		public NamespaceStyle NamespaceStyle { get; set; }

		/// <summary>
		/// Determines whether to write the <see langword="private"/> modifier.
		/// </summary>
		public bool UseExplicitPrivate { get; set; }

		/// <summary>
		/// Determines whether to write the <see langword="internal"/> for top-level types.
		/// </summary>
		public bool UseExplicitInternal { get; set; }

		/// <summary>
		/// Determines whether to write the <see langword="var"/> instead of explicit type for local variables.
		/// </summary>
		public bool UseImplicitType { get; set; }

		/// <summary>
		/// Determines whether to write return type of an lambda expression if possible.
		/// </summary>
		public bool UseLambdaReturnType { get; set; }

		/// <summary>
		/// Determines whether to use the '?' syntax instead of <see cref="System.Nullable{T}"/>.
		/// </summary>
		public bool UseNullableSyntax { get; set; }

		/// <summary>
		/// Determines how lambda expressions are written.
		/// </summary>
		public LambdaStyle LambdaStyle { get; set; }

		/// <summary>
		/// Determines how method bodies are written.
		/// </summary>
		public MethodStyle MethodStyle { get; set; }

		/// <summary>
		/// Determines how record declarations are written.
		/// </summary>
		public RecordStyle RecordStyle { get; set; }

		/// <summary>
		/// Determines whether to use explicit <see langword="managed"/> keyword in function pointers.
		/// </summary>
		public bool UseExplicitManaged { get; set; }

		/// <summary>
		/// Determines whether to use always explicit attribute targets.
		/// </summary>
		public bool UseExplicitAttributeTargets { get; set; }

		/// <summary>
		/// Maximum number of parameters that can be written on a single line. If a method has more parameters than that number allows, every parameter is written on a separate line.
		/// </summary>
		public int ParameterSingleLineLimit { get; set; }

		/// <summary>
		/// Determines whether to use aliases of built-in types instead of concrete types (e.g. <see langword="int"/> instead of <c>Int32</c>.
		/// </summary>
		public bool UseBuiltInAliases { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="CodeBuilderStyleConfiguration"/> class.
		/// </summary>
		public CodeBuilderStyleConfiguration()
		{
		}
	}
}
