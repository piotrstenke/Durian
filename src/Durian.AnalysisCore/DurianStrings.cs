namespace Durian
{
	/// <summary>
	/// Contains some of the most commonly-used <see cref="string"/>s.
	/// </summary>
	public static class DurianStrings
	{
		/// <summary>
		/// Main namespace of the Durian project.
		/// </summary>
		public static readonly string MainNamespace = "Durian";

		/// <summary>
		/// Configuration namespace of the Durian project.
		/// </summary>
		public static readonly string ConfigurationNamespace = $"{MainNamespace}.Configuration";

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
