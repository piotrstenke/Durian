using System;
using System.IO;
using Microsoft.CodeAnalysis;

namespace Durian.Logging
{
	/// <inheritdoc cref="GeneratorLoggingConfiguration"/>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
	public sealed class GeneratorLoggingConfigurationAttribute : Attribute
	{
		private string? _validatedDirectory;

		/// <summary>
		/// The directory the source generator logs will be written to. If not specified, <see cref="GeneratorLoggingConfiguration.DefaultLogDirectory"/> is used instead.
		/// </summary>
		public string? LogDirectory { get; init; }

		/// <inheritdoc cref="GeneratorLoggingConfiguration.SupportedLogs"/>
		public GeneratorLogs SupportedLogs { get; init; }

		/// <summary>
		/// Determines whether the <see cref="LogDirectory"/> is relative to the <see cref="DefaultGeneratorLoggingConfigurationAttribute.LogDirectory"/>. Defaults to <see langword="true"/>.
		/// </summary>
		public bool RelativeToGlobal { get; init; } = true;

		/// <summary>
		/// Determines whether the <see cref="LogDirectory"/> is relative to the <see cref="GeneratorLoggingConfiguration.DefaultLogDirectory"/>. Defaults to <see langword="false"/>.
		/// </summary>
		/// <remarks>If <see cref="RelativeToDefault"/> is set to <see langword="true"/>, value of <see cref="RelativeToGlobal"/> is irrelevant.</remarks>
		public bool RelativeToDefault { get; init; }

		/// <summary>
		/// Determines whether the <see cref="ISourceGenerator"/> supports reporting <see cref="Diagnostic"/>s. Defaults to <see langword="false"/>
		/// </summary>
		public bool SupportsDiagnostics { get; init; }

		/// <summary>
		/// Initializes a new instance of the <see cref="GeneratorLoggingConfigurationAttribute"/> class.
		/// </summary>
		public GeneratorLoggingConfigurationAttribute()
		{
		}

		/// <summary>
		/// Gets the full <see cref="LogDirectory"/> and checks if its valid.
		/// </summary>
		/// <exception cref="ArgumentException">
		/// <see cref="LogDirectory"/> of the <see cref="GeneratorLoggingConfigurationAttribute"/> cannot be empty or white space only. -or-
		/// <paramref name="globalConfiguration"/> must be specified if <see cref="RelativeToGlobal"/> is set to <see langword="true"/> -or-
		/// <see cref="LogDirectory"/> must be specified if both <see cref="RelativeToDefault"/> and <see cref="RelativeToGlobal"/> are set to <see langword="false"/>. -or-
		/// <see cref="LogDirectory"/> contains one or more of invalid characters defined in the <see cref="Path.GetInvalidPathChars"/>.
		/// </exception>
		public string GetAndValidateFullLogDirectory(GeneratorLoggingConfiguration? globalConfiguration)
		{
			if (_validatedDirectory is not null)
			{
				return _validatedDirectory;
			}

			if (LogDirectory is null)
			{
				if (RelativeToGlobal)
				{
					if (globalConfiguration is null)
					{
						throw Exc_GlobalNotSpecified(nameof(globalConfiguration));
					}

					return globalConfiguration!.LogDirectory;
				}
				else if (RelativeToDefault)
				{
					return GeneratorLoggingConfiguration.DefaultLogDirectory;
				}
				else
				{
					throw new ArgumentException($"{nameof(LogDirectory)} must be specified if both {nameof(RelativeToDefault)} and {nameof(RelativeToGlobal)} are set to false.");
				}
			}

			if (string.IsNullOrWhiteSpace(LogDirectory))
			{
				throw new ArgumentException($"{nameof(LogDirectory)} of the {nameof(GeneratorLoggingConfigurationAttribute)} cannot be empty or white space only!");
			}

			string? dir;

			if (RelativeToGlobal)
			{
				if (globalConfiguration is null)
				{
					throw Exc_GlobalNotSpecified(nameof(globalConfiguration));
				}

				dir = CombineWithRoot(globalConfiguration.LogDirectory);
			}
			else if (RelativeToDefault)
			{
				dir = CombineWithRoot(GeneratorLoggingConfiguration.DefaultLogDirectory);
			}
			else
			{
				dir = LogDirectory;
			}

			// Checks if the directory is valid.
			Path.GetFullPath(dir);

			_validatedDirectory = dir;
			return dir;

			static ArgumentException Exc_GlobalNotSpecified(string argName)
			{
				return new ArgumentException($"{argName} must be specified if {nameof(RelativeToGlobal)} is set to true!");
			}
		}

		private string CombineWithRoot(string root)
		{
			if (LogDirectory![0] == '/')
			{
				return root + LogDirectory;
			}
			else
			{
				return root + "/" + LogDirectory;
			}
		}
	}
}
