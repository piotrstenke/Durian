// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Microsoft.CodeAnalysis;
using System;
using System.IO;

namespace Durian.Analysis.Logging
{
	/// <summary>
	/// Determines the behavior of the target <see cref="ISourceGenerator"/> when creating log files.
	/// </summary>
	/// <remarks>This class implements the <see cref="IEquatable{T}"/> interface - two instances are compared by their values, not references.</remarks>
	public sealed partial class LoggingConfiguration : IEquatable<LoggingConfiguration>, ICloneable
	{
		private bool _enableDiagnostics;

		private bool _enableLogging;

		private NodeOutput _nodeOutput;

		private string? _logDirectory;

		private bool _supportsDiagnostics;

		/// <summary>
		/// Returns a <see cref="FilterMode"/> associated with the current configuration.
		/// </summary>
		public FilterMode CurrentFilterMode => Extensions.GeneratorExtensions.GetFilterMode(EnableDiagnostics, EnableLogging);

		/// <summary>
		/// Determines what to output when a <see cref="SyntaxNode"/> is being logged and no other <see cref="NodeOutput"/> is specified.
		/// </summary>
		/// <exception cref="ArgumentException">Value cannot be equal to <see cref="NodeOutput.Default"/>. -or- Invalid <see cref="NodeOutput"/> value.</exception>
		public NodeOutput DefaultNodeOutput
		{
			get => _nodeOutput;
			set
			{
				if(value == NodeOutput.Default)
				{
					throw new ArgumentException($"{nameof(DefaultNodeOutput)} cannot be equal to {nameof(NodeOutput)}.{nameof(NodeOutput.Default)}", nameof(DefaultNodeOutput));
				}

				if(value is NodeOutput.Node or NodeOutput.Containing or NodeOutput.SyntaxTree)
				{
					_nodeOutput = value;
				}
				else
				{
					throw new ArgumentException($"Invalid '{nameof(NodeOutput)}' value", nameof(DefaultNodeOutput));
				}
			}
		}

		/// <summary>
		/// Determines whether this <see cref="IDurianGenerator"/> allows to report any <see cref="Diagnostic"/>s during the current execution pass.
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
		/// Determines whether to allow the <see cref="ISourceGenerator"/> to throw <see cref="Exception"/>s.
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
					throw new ArgumentException($"{nameof(LogDirectory)} cannot be null or empty", nameof(LogDirectory));
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
		/// Initializes a new instance of the <see cref="LoggingConfiguration"/> class.
		/// </summary>
		public LoggingConfiguration()
		{
		}

		/// <inheritdoc/>
		public static bool operator !=(LoggingConfiguration? a, LoggingConfiguration? b)
		{
			return !(a == b);
		}

		/// <inheritdoc/>
		public static bool operator ==(LoggingConfiguration? a, LoggingConfiguration? b)
		{
			if (a is null)
			{
				return b is null;
			}

			if (b is null)
			{
				return false;
			}

			return
				a._supportsDiagnostics == b._supportsDiagnostics &&
				a._enableLogging == b._enableLogging &&
				a._enableDiagnostics == b._enableDiagnostics &&
				a._logDirectory == b._logDirectory &&
				a.EnableExceptions == b.EnableExceptions &&
				a.SupportedLogs == b.SupportedLogs &&
				a.DefaultNodeOutput == b.DefaultNodeOutput;
		}

		/// <inheritdoc cref="ICloneable.Clone"/>
		public LoggingConfiguration Clone()
		{
			return new LoggingConfiguration()
			{
				_enableDiagnostics = EnableDiagnostics,
				_enableLogging = EnableLogging,
				_supportsDiagnostics = SupportsDiagnostics,
				SupportedLogs = SupportedLogs,
				_logDirectory = LogDirectory,
				EnableExceptions = EnableExceptions,
				DefaultNodeOutput = DefaultNodeOutput
			};
		}

		/// <inheritdoc/>
		public override bool Equals(object obj)
		{
			if (obj is not LoggingConfiguration other)
			{
				return false;
			}

			return other == this;
		}

		/// <inheritdoc/>
		public bool Equals(LoggingConfiguration? other)
		{
			return other == this;
		}

		/// <inheritdoc/>
		public override int GetHashCode()
		{
			int hashCode = -604981020;
			hashCode = (hashCode * -1521134295) + LogDirectory.GetHashCode();
			hashCode = (hashCode * -1521134295) + EnableLogging.GetHashCode();
			hashCode = (hashCode * -1521134295) + EnableDiagnostics.GetHashCode();
			hashCode = (hashCode * -1521134295) + SupportedLogs.GetHashCode();
			hashCode = (hashCode * -1521134295) + SupportsDiagnostics.GetHashCode();
			hashCode = (hashCode * -1521134295) + EnableExceptions.GetHashCode();
			hashCode = (hashCode * -1521134295) + DefaultNodeOutput.GetHashCode();
			return hashCode;
		}

		object ICloneable.Clone()
		{
			return Clone();
		}
	}
}