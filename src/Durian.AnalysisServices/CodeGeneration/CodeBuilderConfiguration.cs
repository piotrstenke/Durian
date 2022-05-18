// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Microsoft.CodeAnalysis.CSharp;

namespace Durian.Analysis.CodeGeneration
{
	/// <summary>
	/// Configures a code builder.
	/// </summary>
	public sealed record CodeBuilderConfiguration
	{
		/// <summary>
		/// Configures style of the generated code.
		/// </summary>
		public CodeBuilderStyleConfiguration Style { get; }

		/// <summary>
		/// Determines whether generic attributes are allowed.
		/// </summary>
		public bool AllowGenericAttributes { get; init; }

		/// <summary>
		/// Current language version.
		/// </summary>
		public LanguageVersion LanguageVersion { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="CodeBuilderConfiguration"/> class.
		/// </summary>
		public CodeBuilderConfiguration()
		{
			LanguageVersion = LanguageVersion.Latest.MapSpecifiedToEffectiveVersion();
		}

		private CodeBuilderConfiguration(LanguageVersion languageVersion)
		{
			LanguageVersion = languageVersion;
		}

		/// <summary>
		/// Creates a new <see cref="CodeBuilderConfiguration"/> instance with rules according to the specified <paramref name="version"/> of the C# language.
		/// </summary>
		/// <param name="version">Version of the C# to create a new <see cref="CodeBuilderConfiguration"/> for.</param>
		public static CodeBuilderConfiguration Create(LanguageVersion version)
		{
			return new CodeBuilderConfiguration(version);
		}
	}
}
