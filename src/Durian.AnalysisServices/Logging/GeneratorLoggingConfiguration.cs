// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Diagnostics;
using System.IO;
using Microsoft.CodeAnalysis;

namespace Durian.Analysis.Logging
{
	/// <summary>
	/// Determines the behavior of the target <see cref="ISourceGenerator"/> when creating log files.
	/// </summary>
	[DebuggerDisplay("Enabled = {EnableLogging}, Supported = {SupportedLogs}")]
	public sealed partial class GeneratorLoggingConfiguration : IEquatable<GeneratorLoggingConfiguration>, ICloneable
	{
		private bool _enableDiagnostics;

		private bool _enableLogging;

		private string? _logDirectory;

		private bool _supportsDiagnostics;

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
		/// Determines whether to enable the <see cref="ISourceGenerator"/> can throw <see cref="Exception"/>s.
		/// </summary>
		public bool EnableExceptions { get; set; }

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
		/// The directory the source generator logs will be written to.
		/// </summary>
		/// <remarks>The input value gets converted to a full path.
		/// <para>The default value is the physical <c>Documents</c> directory.</para></remarks>
		/// <exception cref="ArgumentException"><see cref="LogDirectory"/> cannot be <see langword="null"/> or empty.</exception>
		public string LogDirectory
		{
			get => _logDirectory ??= Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
			set
			{
				if (string.IsNullOrWhiteSpace(value))
				{
					throw new ArgumentException($"{nameof(LogDirectory)} cannot be null or empty!");
				}

				_logDirectory = Path.GetFullPath(value);
			}
		}

		/// <summary>
		/// Types of logs this source generator can produce.
		/// </summary>
		public GeneratorLogs SupportedLogs { get; set; }

		/// <summary>
		/// Determines whether the <see cref="ISourceGenerator"/> supports reporting or logging <see cref="Diagnostic"/>s.
		/// </summary>
		public bool SupportsDiagnostics
		{
			get => _supportsDiagnostics;
			set
			{
				if (!value)
				{
					_enableLogging = false;
				}

				_supportsDiagnostics = value;
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GeneratorLoggingConfiguration"/> class.
		/// </summary>
		public GeneratorLoggingConfiguration()
		{
		}

		/// <inheritdoc/>
		public static bool operator !=(GeneratorLoggingConfiguration a, GeneratorLoggingConfiguration b)
		{
			return !(a == b);
		}

		/// <inheritdoc/>
		public static bool operator ==(GeneratorLoggingConfiguration a, GeneratorLoggingConfiguration b)
		{
			return
				a._supportsDiagnostics == b._supportsDiagnostics &&
				a._enableLogging == b._enableLogging &&
				a._enableDiagnostics == b._enableDiagnostics &&
				a.EnableExceptions == b.EnableExceptions &&
				a.EnableLogging == b.EnableLogging &&
				a.LogDirectory == b.LogDirectory &&
				a.SupportedLogs == b.SupportedLogs;
		}

		/// <summary>
		/// Creates a new object that is a copy of the current instance.
		/// </summary>
		/// <returns>A new object that is a copy of this instance.</returns>
		public GeneratorLoggingConfiguration Clone()
		{
			return new GeneratorLoggingConfiguration()
			{
				_enableDiagnostics = _enableDiagnostics,
				_enableLogging = _enableLogging,
				_logDirectory = _logDirectory,
				_supportsDiagnostics = _supportsDiagnostics,
				SupportedLogs = SupportedLogs,
				SupportsDiagnostics = SupportsDiagnostics,
				EnableExceptions = EnableExceptions
			};
		}

		/// <inheritdoc/>
		public override bool Equals(object obj)
		{
			return base.Equals(obj);
		}

		/// <inheritdoc/>
		public bool Equals(GeneratorLoggingConfiguration other)
		{
			return other == this;
		}

		/// <inheritdoc/>
		public override int GetHashCode()
		{
			int hashCode = -604981020;
			hashCode = (hashCode * -1521134295) + LogDirectory.GetHashCode();
			hashCode = (hashCode * -1521134295) + _enableLogging.GetHashCode();
			hashCode = (hashCode * -1521134295) + _enableDiagnostics.GetHashCode();
			hashCode = (hashCode * -1521134295) + SupportedLogs.GetHashCode();
			hashCode = (hashCode * -1521134295) + SupportsDiagnostics.GetHashCode();
			hashCode = (hashCode * -1521134295) + EnableExceptions.GetHashCode();
			return hashCode;
		}

		object ICloneable.Clone()
		{
			return Clone();
		}
	}
}