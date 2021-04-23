using System;
using System.Diagnostics;
using System.IO;

namespace Durian.Configuration
{
	/// <inheritdoc cref="GeneratorLoggingConfiguration"/>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
	[Conditional("ENABLE_GENERATOR_LOGS")]
	public sealed class GeneratorLoggingConfigurationAttribute : Attribute
	{
		private string? _logDirectory;
		private readonly bool _relativeToGlobal;

		/// <summary>
		/// The directory the source generator logs will be written to. If not specified, <see cref="GlobalGeneratorLoggingConfigurationAttribute.LogDirectory"/> is used instead.
		/// </summary>
		public string? LogDirectory
		{
			get => _logDirectory;
			set
			{
				if (string.IsNullOrWhiteSpace(value))
				{
					throw new ArgumentException($"{nameof(LogDirectory)} cannot be null or empty");
				}

				string? dir;

				if (_relativeToGlobal)
				{
					if (value![0] == '/')
					{
						dir = DurianStrings.LogDirectory + value;
					}
					else
					{
						dir = DurianStrings.LogDirectory + "/" + value;
					}
				}
				else
				{
					dir = value;
				}

				Directory.CreateDirectory(dir);
				_logDirectory = dir;
			}
		}

		/// <inheritdoc cref="GeneratorLoggingConfiguration.SupportedLogs"/>
		public GeneratorLogs SupportedLogs { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="GeneratorLoggingConfigurationAttribute"/> class.
		/// </summary>
		public GeneratorLoggingConfigurationAttribute()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GeneratorLoggingConfigurationAttribute"/> class.
		/// </summary>
		/// <param name="relativeToGlobal">Determines whether the <see cref="LogDirectory"/> is relative to the <see cref="GlobalGeneratorLoggingConfigurationAttribute.LogDirectory"/>.</param>
		public GeneratorLoggingConfigurationAttribute(bool relativeToGlobal)
		{
			_relativeToGlobal = relativeToGlobal;
		}

		/// <summary>
		/// Returns a new instance of <see cref="GeneratorLoggingConfiguration"/> based on values of this <see cref="GeneratorLoggingConfigurationAttribute"/>.
		/// </summary>
		public GeneratorLoggingConfiguration GetLoggingConfiguration()
		{
			return new GeneratorLoggingConfiguration()
			{
				LogDirectory = LogDirectory ?? DurianStrings.LogDirectory,
				SupportedLogs = SupportedLogs,
				EnableLogging = true
			};
		}
	}
}
