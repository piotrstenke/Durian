using System;
using System.Diagnostics;
using System.IO;
using Microsoft.CodeAnalysis;

namespace Durian.Logging
{
	/// <summary>
	/// Determines the behavior of the target <see cref="ISourceGenerator"/> when creating log files.
	/// </summary>
	[DebuggerDisplay("Enabled = {EnableLogging}, Supported = {SupportedLogs}")]
	public sealed partial record GeneratorLoggingConfiguration
	{
		private string _logDirectory;
		private bool _enableLogging;
		private bool _enableDiagnostics;

		/// <summary>
		/// The directory the source generator logs will be written to.
		/// </summary>
		/// <remarks>The input value gets converted to a full path.</remarks>
		/// <exception cref="ArgumentException"><see cref="LogDirectory"/> cannot be <see langword="null"/> or empty.</exception>
		public string LogDirectory
		{
			get => _logDirectory;
			set
			{
				if (string.IsNullOrWhiteSpace(value))
				{
					throw new ArgumentException($"{nameof(LogDirectory)} cannot be null or empty!");
				}

				string dir = Path.GetFullPath(value);

				if (EnableLogging)
				{
					Directory.CreateDirectory(dir);
				}

				_logDirectory = dir;
			}
		}

		/// <summary>
		/// Types of logs this source generator can produce.
		/// </summary>
		public GeneratorLogs SupportedLogs { get; set; }

		/// <summary>
		/// Gets the <see cref="FilterMode"/> the current configuration is valid for.
		/// </summary>
		public FilterMode CurrentFilterMode
		{
			get
			{
				FilterMode mode = FilterMode.None;

				if (EnableDiagnostics)
				{
					mode += (int)FilterMode.Diagnostics;
				}

				if (EnableLogging)
				{
					mode += (int)FilterMode.Logs;
				}

				return mode;
			}
		}

		/// <summary>
		/// Determines whether to enable logging.
		/// </summary>
		/// <exception cref="InvalidOperationException"><see cref="EnableLogging"/> cannot be set to <see langword="true"/> if generator logging is globally disabled.</exception>
		public bool EnableLogging
		{
			get => _enableLogging;
			set
			{
				if (value && !IsEnabled)
				{
					throw new InvalidOperationException($"{nameof(EnableLogging)} cannot be set to true if generator logging is globally disabled!");
				}

				_enableLogging = value;
			}
		}

		/// <summary>
		/// Determines whether this <see cref="IDurianSourceGenerator"/> allows to report any <see cref="Diagnostic"/>s during the current execution pass.
		/// </summary>
		/// <exception cref="InvalidOperationException"><see cref="EnableDiagnostics"/> cannot be set to <see langword="true"/> if <see cref="SupportsDiagnostics"/> is <see langword="false"/>.</exception>
		public bool EnableDiagnostics
		{
			get => _enableDiagnostics;
			set
			{
				if (value && !SupportsDiagnostics)
				{
					throw new InvalidOperationException($"{nameof(EnableDiagnostics)} cannot be set to true if {nameof(SupportsDiagnostics)} is false!");
				}

				_enableDiagnostics = value;
			}
		}

		/// <summary>
		/// Determines whether the <see cref="ISourceGenerator"/> supports reporting <see cref="Diagnostic"/>s.
		/// </summary>
		public bool SupportsDiagnostics { get; init; }

		/// <summary>
		/// Determines whether to enable the <see cref="ISourceGenerator"/> can throw <see cref="Exception"/>s.
		/// </summary>
		public bool EnableExceptions { get; set; }

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
