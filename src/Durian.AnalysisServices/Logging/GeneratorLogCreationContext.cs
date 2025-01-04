using System;

namespace Durian.Analysis.Logging
{
	/// <summary>
	/// Contains data passed to a <see cref="GeneratorLogHandler"/>'s constructor.
	/// </summary>
	public readonly struct GeneratorLogCreationContext : IEquatable<GeneratorLogCreationContext>
	{
		/// <summary>
		/// Returns <see cref="Debug"/> or <see cref="Runtime"/>, depending on the current configuration.
		/// </summary>
		public static GeneratorLogCreationContext CurrentConfig =>
#if DEBUG
			Debug;

#else
			Runtime;

#endif

		/// <summary>
		/// Returns a new <see cref="GeneratorLogCreationContext"/> with all properties set to <see langword="true"/>.
		/// </summary>
		public static GeneratorLogCreationContext Debug => new(true, true, true, true);

		/// <summary>
		/// Returns a new <see cref="GeneratorLogCreationContext"/> with all properties set to <see langword="false"/>.
		/// </summary>
		public static GeneratorLogCreationContext Runtime => new();

		/// <summary>
		/// Determines whether to try to create a <see cref="Logging.LoggingConfiguration"/> based on one of the <see cref="LoggingConfigurationAttribute"/>.
		/// </summary>
		public bool CheckForConfigurationAttribute { get; }

		/// <summary>
		/// Determines whether to set <see cref="LoggingConfiguration.EnableDiagnostics"/> to <see langword="true"/> if <see cref="LoggingConfiguration.SupportsDiagnostics"/> is <see langword="true"/>.
		/// </summary>
		public bool EnableDiagnosticsIfSupported { get; }

		/// <summary>
		/// Determines whether to set enable exceptions to be thrown.
		/// </summary>
		public bool EnableExceptions { get; }

		/// <summary>
		/// Determines whether to enable logging for this <see cref="GeneratorLogHandler"/> instance if logging is supported.
		/// </summary>
		public bool EnableLoggingIfSupported { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="GeneratorLogCreationContext"/> struct.
		/// </summary>
		/// <param name="checkForConfigurationAttribute">Determines whether to try to create a <see cref="LoggingConfiguration"/> based on one of the <see cref="LoggingConfigurationAttribute"/>.</param>
		/// <param name="enableDiagnosticsIfSupported">Determines whether to set <see cref="LoggingConfiguration.EnableDiagnostics"/> to <see langword="true"/> if <see cref="LoggingConfiguration.SupportsDiagnostics"/> is <see langword="true"/>.</param>
		/// <param name="enableLoggingIfSupported">Determines whether to enable logging for this <see cref="GeneratorLogHandler"/> instance if logging is supported.</param>
		/// <param name="enableExceptions">Determines whether to set enable exceptions to be thrown.</param>
		public GeneratorLogCreationContext(
			bool checkForConfigurationAttribute = false,
			bool enableDiagnosticsIfSupported = false,
			bool enableLoggingIfSupported = false,
			bool enableExceptions = false
		)
		{
			CheckForConfigurationAttribute = checkForConfigurationAttribute;
			EnableDiagnosticsIfSupported = enableDiagnosticsIfSupported;
			EnableLoggingIfSupported = enableLoggingIfSupported;
			EnableExceptions = enableExceptions;
		}

		/// <inheritdoc/>
		public static bool operator !=(GeneratorLogCreationContext left, GeneratorLogCreationContext right)
		{
			return !(left == right);
		}

		/// <inheritdoc/>
		public static bool operator ==(GeneratorLogCreationContext left, GeneratorLogCreationContext right)
		{
			return
				left.CheckForConfigurationAttribute == right.CheckForConfigurationAttribute &&
				left.EnableDiagnosticsIfSupported == right.EnableDiagnosticsIfSupported &&
				left.EnableExceptions == right.EnableExceptions &&
				left.EnableLoggingIfSupported == right.EnableLoggingIfSupported;
		}

		/// <inheritdoc/>
		public override bool Equals(object? obj)
		{
			if (obj is GeneratorLogCreationContext other)
			{
				return other == this;
			}

			return false;
		}

		/// <inheritdoc/>
		public bool Equals(GeneratorLogCreationContext other)
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
