namespace Durian.Generator.Logging
{
	/// <summary>
	/// Contains data passed to a <see cref="LoggableSourceGenerator"/>'s constructor.
	/// </summary>
	public readonly struct LoggableGeneratorConstructionContext
	{
		/// <summary>
		/// Determines whether to try to create a <see cref="GeneratorLoggingConfiguration"/> based on one of the logging attributes.
		/// <para>See: <see cref="GeneratorLoggingConfigurationAttribute"/>, <see cref="DefaultGeneratorLoggingConfigurationAttribute"/></para>
		/// </summary>
		public bool CheckForConfigurationAttribute { get; init; }

		/// <summary>
		/// Determines whether to enable logging for this <see cref="LoggableSourceGenerator"/> instance if logging is supported.
		/// </summary>
		public bool EnableLoggingIfSupported { get; init; }

		/// <summary>
		/// Determines whether to set <see cref="GeneratorLoggingConfiguration.EnableDiagnostics"/> to <see langword="true"/> if <see cref="GeneratorLoggingConfiguration.SupportsDiagnostics"/> is <see langword="true"/>.
		/// </summary>
		public bool EnableDiagnosticsIfSupported { get; init; }

		/// <summary>
		/// Determines whether to set enable exceptions to be thrown.
		/// </summary>
		public bool EnableExceptions { get; init; }

		/// <summary>
		/// Returns a new <see cref="LoggableGeneratorConstructionContext"/> with all properties set to <see langword="true"/>.
		/// </summary>
		public static LoggableGeneratorConstructionContext Debug => new()
		{
			CheckForConfigurationAttribute = true,
			EnableLoggingIfSupported = true,
			EnableDiagnosticsIfSupported = true,
			EnableExceptions = true
		};

		/// <summary>
		/// Returns a new <see cref="LoggableGeneratorConstructionContext"/> with all properties set to <see langword="false"/>.
		/// </summary>
		public static LoggableGeneratorConstructionContext Runtime => new();

		/// <summary>
		/// Returns <see cref="Debug"/> or <see cref="Runtime"/>, depending on the current configuration.
		/// </summary>
		public static LoggableGeneratorConstructionContext CurrentConfig =>
#if DEBUG
			Debug;
#else
			Runtime;
#endif
	}
}
