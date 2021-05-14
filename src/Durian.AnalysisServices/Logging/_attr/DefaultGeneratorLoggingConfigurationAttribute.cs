using System;
using System.IO;
using Microsoft.CodeAnalysis;

namespace Durian.Generator.Logging
{
	/// <summary>
	/// Determines the default behavior of <see cref="ISourceGenerator"/>s in the current assembly when creating log files.
	/// </summary>
	[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false, Inherited = false)]
	public sealed class DefaultGeneratorLoggingConfigurationAttribute : Attribute
	{
		private string? _validatedDirectory;

		/// <summary>
		/// Default directory where the generator log files are to be found.
		/// </summary>
		public string? LogDirectory { get; init; }

		/// <summary>
		/// Defaults types of logs for this assembly.
		/// </summary>
		public GeneratorLogs SupportedLogs { get; init; }

		/// <summary>
		/// Determines whether the <see cref="LogDirectory"/> is relative to the <see cref="GeneratorLoggingConfiguration.DefaultLogDirectory"/>. Defaults to <see langword="true"/>.
		/// </summary>
		public bool RelativeToDefault { get; init; }

		/// <summary>
		/// Determines whether the <see cref="ISourceGenerator"/> supports reporting <see cref="Diagnostic"/>s. Defaults to <see langword="false"/>
		/// </summary>
		public bool SupportsDiagnostics { get; init; }

		/// <summary>
		/// Determines whether to enable the <see cref="ISourceGenerator"/> can throw <see cref="Exception"/>s. Defaults to <see langword="false"/>
		/// </summary>
		public bool EnableExceptions { get; init; }

		/// <summary>
		/// Initializes a new instance of the <see cref="DefaultGeneratorLoggingConfigurationAttribute"/> class.
		/// </summary>
		public DefaultGeneratorLoggingConfigurationAttribute()
		{
		}

		/// <summary>
		/// Gets the full <see cref="LogDirectory"/> and checks if its valid.
		/// </summary>
		/// <exception cref="ArgumentException">
		/// <see cref="LogDirectory"/> of the <see cref="DefaultGeneratorLoggingConfigurationAttribute"/> cannot be empty or whitespace only. -or-
		/// <see cref="LogDirectory"/> must be specified if <see cref="RelativeToDefault"/> is set to <see langword="false"/>. -or-
		/// <see cref="LogDirectory"/> contains one or more of invalid characters defined in the <see cref="Path.GetInvalidPathChars"/>.
		/// </exception>
		public string GetAndValidateFullLogDirectory()
		{
			if (_validatedDirectory is not null)
			{
				return _validatedDirectory;
			}

			if (LogDirectory is null)
			{
				if (!RelativeToDefault)
				{
					throw new ArgumentException($"{nameof(LogDirectory)} must be specified if {nameof(RelativeToDefault)} is set to false!");
				}

				return GeneratorLoggingConfiguration.DefaultLogDirectory;
			}

			if (string.IsNullOrWhiteSpace(LogDirectory))
			{
				throw new ArgumentException($"{nameof(LogDirectory)} of the {nameof(DefaultGeneratorLoggingConfigurationAttribute)} cannot be empty or whitespace only!");
			}

			string? dir;

			if (RelativeToDefault)
			{
				if (LogDirectory![0] == '/')
				{
					dir = GeneratorLoggingConfiguration.DefaultLogDirectory + LogDirectory;
				}
				else
				{
					dir = GeneratorLoggingConfiguration.DefaultLogDirectory + "/" + LogDirectory;
				}
			}
			else
			{
				dir = LogDirectory;
			}

			// Checks if the directory is valid.
			Path.GetFullPath(dir);

			_validatedDirectory = dir;
			return dir;
		}
	}
}
