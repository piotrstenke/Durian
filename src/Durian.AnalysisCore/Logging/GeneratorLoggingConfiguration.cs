using System;
using System.IO;
using Microsoft.CodeAnalysis;

namespace Durian.Logging
{
	/// <summary>
	/// Determines the behavior of the target <see cref="ISourceGenerator"/> when creating log files.
	/// </summary>
	public sealed partial record GeneratorLoggingConfiguration
	{
		private readonly string _logDirectory;
		private readonly bool _enableLogging;

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

				if (IsEnabled)
				{
					Directory.CreateDirectory(dir);
				}

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
		/// <exception cref="InvalidOperationException"><see cref="EnableLogging"/> cannot be set to <see langword="true"/> when generator logging is globally disabled.</exception>
		public bool EnableLogging
		{
			get => _enableLogging;
			init
			{
				if (value && !IsEnabled)
				{
					throw new InvalidOperationException($"{nameof(EnableLogging)} cannot be set to true when generator logging is globally disabled!");
				}

				_enableLogging = value;
			}
		}

		/// <summary>
		/// Determines whether the <see cref="ISourceGenerator"/> supports reporting <see cref="Diagnostic"/>s.
		/// </summary>
		public bool SupportsDiagnostics { get; init; }

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
