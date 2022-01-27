// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;

namespace Durian.Analysis.Logging
{
	/// <summary>
	/// Contains data passed to a <see cref="LoggableSourceGenerator"/>'s constructor.
	/// </summary>
	public readonly struct LoggableGeneratorConstructionContext : IEquatable<LoggableGeneratorConstructionContext>
	{
		/// <summary>
		/// Returns <see cref="Debug"/> or <see cref="Runtime"/>, depending on the current configuration.
		/// </summary>
		public static LoggableGeneratorConstructionContext CurrentConfig =>
#if DEBUG
			Debug;

#else
			Runtime;

#endif

		/// <summary>
		/// Returns a new <see cref="LoggableGeneratorConstructionContext"/> with all properties set to <see langword="true"/>.
		/// </summary>
		public static LoggableGeneratorConstructionContext Debug => new(true, true, true, true);

		/// <summary>
		/// Returns a new <see cref="LoggableGeneratorConstructionContext"/> with all properties set to <see langword="false"/>.
		/// </summary>
		public static LoggableGeneratorConstructionContext Runtime => new();

		/// <summary>
		/// Determines whether to try to create a <see cref="GeneratorLoggingConfiguration"/> based on one of the logging attributes.
		/// <para>See: <see cref="GeneratorLoggingConfigurationAttribute"/>, <see cref="DefaultGeneratorLoggingConfigurationAttribute"/></para>
		/// </summary>
		public bool CheckForConfigurationAttribute { get; }

		/// <summary>
		/// Determines whether to set <see cref="GeneratorLoggingConfiguration.EnableDiagnostics"/> to <see langword="true"/> if <see cref="GeneratorLoggingConfiguration.SupportsDiagnostics"/> is <see langword="true"/>.
		/// </summary>
		public bool EnableDiagnosticsIfSupported { get; }

		/// <summary>
		/// Determines whether to set enable exceptions to be thrown.
		/// </summary>
		public bool EnableExceptions { get; }

		/// <summary>
		/// Determines whether to enable logging for this <see cref="LoggableSourceGenerator"/> instance if logging is supported.
		/// </summary>
		public bool EnableLoggingIfSupported { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="LoggableGeneratorConstructionContext"/> struct.
		/// </summary>
		/// <param name="checkForConfigurationAttribute"></param>
		/// <param name="enableDiagnosticsIfSupported"></param>
		/// <param name="enableLoggingIfSupported"></param>
		/// <param name="enableExceptions"></param>
		public LoggableGeneratorConstructionContext(
			bool checkForConfigurationAttribute = false,
			bool enableDiagnosticsIfSupported = false,
			bool enableLoggingIfSupported = false,
			bool enableExceptions = false)
		{
			CheckForConfigurationAttribute = checkForConfigurationAttribute;
			EnableDiagnosticsIfSupported = enableDiagnosticsIfSupported;
			EnableLoggingIfSupported = enableLoggingIfSupported;
			EnableExceptions = enableExceptions;
		}

		/// <inheritdoc/>
		public static bool operator !=(LoggableGeneratorConstructionContext left, LoggableGeneratorConstructionContext right)
		{
			return !(left == right);
		}

		/// <inheritdoc/>
		public static bool operator ==(LoggableGeneratorConstructionContext left, LoggableGeneratorConstructionContext right)
		{
			return
				left.CheckForConfigurationAttribute == right.CheckForConfigurationAttribute &&
				left.EnableDiagnosticsIfSupported == right.EnableDiagnosticsIfSupported &&
				left.EnableExceptions == right.EnableExceptions &&
				left.EnableLoggingIfSupported == right.EnableLoggingIfSupported;
		}

		/// <inheritdoc/>
		public override bool Equals(object obj)
		{
			if (obj is LoggableGeneratorConstructionContext other)
			{
				return other == this;
			}

			return false;
		}

		/// <inheritdoc/>
		public bool Equals(LoggableGeneratorConstructionContext other)
		{
			return other == this;
		}

		/// <inheritdoc/>
		public override int GetHashCode()
		{
			int hashCode = -2108380309;
			hashCode = (hashCode * -1521134295) + CheckForConfigurationAttribute.GetHashCode();
			hashCode = (hashCode * -1521134295) + EnableDiagnosticsIfSupported.GetHashCode();
			hashCode = (hashCode * -1521134295) + EnableExceptions.GetHashCode();
			hashCode = (hashCode * -1521134295) + EnableLoggingIfSupported.GetHashCode();
			return hashCode;
		}
	}
}