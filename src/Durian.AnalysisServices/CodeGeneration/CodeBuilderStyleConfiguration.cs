// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;

namespace Durian.Analysis.CodeGeneration
{
	/// <summary>
	/// Configures style of the generated code.
	/// </summary>
	public sealed record CodeBuilderStyleConfiguration
	{
		/// <summary>
		/// Function that overrides the result of the <see cref="Default()"/> method.
		/// </summary>
		public static Func<CodeBuilderStyleConfiguration>? CustomProvider { get; set; }

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
		/// Determines how enum declarations are written.
		/// </summary>
		public EnumStyle EnumStyle { get; set; }

		/// <summary>
		/// Determines whether to use explicit <see langword="managed"/> keyword in function pointers.
		/// </summary>
		public bool UseExplicitManaged { get; set; }

		/// <summary>
		/// Determines whether to use always explicit attribute targets.
		/// </summary>
		public bool UseExplicitAttributeTargets { get; set; }

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

		/// <summary>
		/// Returns the default <see cref="CodeBuilderStyleConfiguration"/>.
		/// </summary>
		public static CodeBuilderStyleConfiguration Default()
		{
			if(CustomProvider is not null)
			{
				return CustomProvider();
			}

			return new CodeBuilderStyleConfiguration()
			{
				UseBuiltInAliases = true,
				LambdaStyle = LambdaStyle.Expression,
				MethodStyle = MethodStyle.Block,
				RecordStyle = RecordStyle.PrimaryConstructor,
				UseExplicitInternal = true,
				UseExplicitPrivate = true,
			};
		}
	}
}
