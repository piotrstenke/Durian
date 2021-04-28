using System;
using System.IO;

namespace Durian.Logging
{
	/// <summary>
	/// Globally determines how the source generator should behave when logging information.
	/// </summary>
	[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false, Inherited = false)]
	public sealed class GlobalGeneratorLoggingConfigurationAttribute : Attribute
	{
		private string? _validatedDirectory;

		/// <summary>
		/// Determines whether the <see cref="LogDirectory"/> is relative to the <see cref="GeneratorLoggingConfiguration.DefaultLogDirectory"/>. Defaults to <see langword="true"/>.
		/// </summary>
		public bool RelativeToDefault { get; init; }

		/// <summary>
		/// Default directory where the generator log files are to be found.
		/// </summary>
		public string? LogDirectory { get; init; }

		/// <summary>
		/// Defaults types of logs for this assembly.
		/// </summary>
		public GeneratorLogs SupportedLogs { get; init; }

		/// <summary>
		/// Initializes a new instance of the <see cref="GlobalGeneratorLoggingConfigurationAttribute"/> class.
		/// </summary>
		public GlobalGeneratorLoggingConfigurationAttribute()
		{
		}

		/// <summary>
		/// Gets the full <see cref="LogDirectory"/> and checks if its valid.
		/// </summary>
		/// <exception cref="ArgumentException">
		/// <see cref="LogDirectory"/> cannot be empty or whitespace only. -or-
		/// <see cref="LogDirectory"/> must be specified if <see cref="RelativeToDefault"/> is set to <see langword="false"/>.
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
				throw new ArgumentException($"{nameof(LogDirectory)} of the {nameof(GlobalGeneratorLoggingConfigurationAttribute)} cannot be empty or whitespace only!");
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

			Directory.CreateDirectory(dir);
			_validatedDirectory = dir;
			return dir;
		}
	}
}
