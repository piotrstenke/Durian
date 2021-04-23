using System;
using System.IO;

namespace Durian
{
	/// <summary>
	/// Determines how the source generator should behave when logging information.
	/// </summary>
	public sealed class GeneratorLoggingConfiguration
	{
		private readonly string _logDirectory;

		/// <summary>
		/// The directory the source generator logs will be written to.
		/// </summary>
		public string LogDirectory
		{
			get => _logDirectory;
			init
			{
				if (string.IsNullOrWhiteSpace(value))
				{
					throw new ArgumentException($"{nameof(LogDirectory)} cannot be null or empty!");
				}

				string dir = Path.GetFullPath(value);
				Directory.CreateDirectory(dir);
				_logDirectory = dir;
			}
		}

		/// <summary>
		/// Types of logs this source generator can produce.
		/// </summary>
		public GeneratorLogs SupportedLogs { get; init; }

		/// <summary>
		/// Determines whether to enable logging.
		/// </summary>
		public bool EnableLogging { get; init; }

		/// <summary>
		/// Creates a new instance of the <see cref="GeneratorLoggingConfiguration"/> class with its values set to default.
		/// </summary>
		public static GeneratorLoggingConfiguration Default => new();

		/// <summary>
		/// Initializes a new instance of the <see cref="GeneratorLoggingConfiguration"/> class.
		/// </summary>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
		public GeneratorLoggingConfiguration()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
		{
		}
	}
}
