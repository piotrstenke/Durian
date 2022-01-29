// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

namespace Durian.Analysis
{
	/// <summary>
	/// Contains some of the most commonly-used <see cref="string"/>s.
	/// </summary>
	public static class DurianStrings
	{
		/// <summary>
		/// Configuration namespace of the Durian project.
		/// </summary>
		public const string ConfigurationNamespace = MainNamespace + ".Configuration";

		/// <summary>
		/// Namespace where the Durian code generation attributes are to be found.
		/// </summary>
		public const string GeneratorNamespace = MainNamespace + ".Generator";

		/// <summary>
		/// Namespace where module and package types are to be found.
		/// </summary>
		public const string InfoNamespace = MainNamespace + ".Info";

		/// <summary>
		/// Main namespace of the Durian project.
		/// </summary>
		public const string MainNamespace = "Durian";

		/// <summary>
		/// Returns the <paramref name="attributeName"/> with a 'Attribute' at the end.
		/// </summary>
		/// <param name="attributeName">Name of the attribute to get the full type name of.</param>
		public static string GetFullAttributeType(string attributeName)
		{
			return $"{attributeName}Attribute";
		}

		/// <summary>
		/// Returns the <paramref name="attributeName"/> with the <see cref="MainNamespace"/> at the start and 'Attribute' at the end.
		/// </summary>
		/// <param name="attributeName">Name of the attribute to get the full type name of.</param>
		public static string GetFullyQualifiedAttribute(string attributeName)
		{
			return $"{MainNamespace}.{GetFullAttributeType(attributeName)}";
		}

		/// <summary>
		/// Returns the <paramref name="attributeName"/> with the <see cref="ConfigurationNamespace"/> at the start and 'Attribute' at the end.
		/// </summary>
		/// <param name="attributeName">Name of the attribute to get the full type name of.</param>
		public static string GetFullyQualifiedConfigurationAttribute(string attributeName)
		{
			return $"{ConfigurationNamespace}.{GetFullAttributeType(attributeName)}";
		}
	}
}