using System;
#if ENABLE_GENERATOR_LOGS
using System.IO;
using System.Reflection;
using Durian.Configuration;
#endif

namespace Durian
{
	/// <summary>
	/// Contains some of the most commonly-used <see cref="string"/>s.
	/// </summary>
	public static class DurianStrings
	{
#if ENABLE_GENERATOR_LOGS
#pragma warning disable IDE0032 // Use auto property
		private static string? _logDirectory;
#pragma warning restore IDE0032 // Use auto property
#endif

		/// <summary>
		/// Main namespace of the Durian project.
		/// </summary>
		public static readonly string MainNamespace = "Durian";

		/// <summary>
		/// Configuration namespace of the Durian project.
		/// </summary>
		public static readonly string ConfigurationNamespace = $"{MainNamespace}.Configuration";

		/// <summary>
		/// Default directory where the generator log files are placed, which is '<c>&lt;documents&gt;/Durian/logs</c>'.
		/// </summary>
		public static readonly string DefaultLogDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "/Durian/logs";

		/// <summary>
		/// Returns the string defined by the <see cref="GlobalGeneratorLoggingConfigurationAttribute"/> or <see cref="DefaultLogDirectory"/> if no such attribute is present.
		/// </summary>
		/// <exception cref="InvalidOperationException"><see cref="LogDirectory"/> cannot be accessed when the <c>ENABLE_GENERATOR_LOGS</c> symbol is not defined!</exception>
		public static string LogDirectory
		{
			get
			{
#if ENABLE_GENERATOR_LOGS
				if (_logDirectory is null)
				{
					GlobalGeneratorLoggingConfigurationAttribute? attr = Assembly.GetExecutingAssembly().GetCustomAttribute<GlobalGeneratorLoggingConfigurationAttribute>();

					if (attr is null)
					{
						_logDirectory = DefaultLogDirectory;
					}
					else
					{
						string? dir = attr.LogDirectory;

						if (dir is null)
						{
							_logDirectory = DefaultLogDirectory;
						}
						else
						{
							Directory.CreateDirectory(dir);
							_logDirectory = dir;
						}
					}
				}

				return _logDirectory;
#else
				throw new InvalidOperationException($"{nameof(LogDirectory)} cannot be accessed when the ENABLE_GENERATOR_LOGS symbol is not defined!");
#endif
			}
		}

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
