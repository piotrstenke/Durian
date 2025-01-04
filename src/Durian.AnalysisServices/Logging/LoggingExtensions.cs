namespace Durian.Analysis.Logging
{
	/// <summary>
	/// Contains extension methods for better logging experience.
	/// </summary>
	public static class LoggingExtensions
	{
		/// <summary>
		/// Sets data of the <paramref name="logHandler"/>'s <see cref="LoggingConfiguration"/> in accordance to the specified <paramref name="context"/>.
		/// </summary>
		/// <param name="logHandler"><see cref="IGeneratorLogHandler"/> to set the data of.</param>
		/// <param name="context"><see cref="GeneratorLogCreationContext"/> to get the data from.</param>
		public static void AcceptContext(this IGeneratorLogHandler logHandler, in GeneratorLogCreationContext context)
		{
			AcceptContext(logHandler.LoggingConfiguration, in context);
		}

		/// <summary>
		/// Sets data of the <paramref name="configuration"/> in accordance to the specified <paramref name="context"/>.
		/// </summary>
		/// <param name="configuration"><see cref="LoggingConfiguration"/> to set the data of.</param>
		/// <param name="context"><see cref="GeneratorLogCreationContext"/> to get the data from.</param>
		public static void AcceptContext(this LoggingConfiguration configuration, in GeneratorLogCreationContext context)
		{
			if (!context.EnableLoggingIfSupported)
			{
				configuration.EnableLogging = false;
			}
			else if (LoggingConfiguration.IsGloballyEnabled)
			{
				configuration.EnableLogging = true;
			}

			if (!context.EnableDiagnosticsIfSupported)
			{
				configuration.EnableDiagnostics = false;
			}
			else if (configuration.SupportsDiagnostics)
			{
				configuration.EnableDiagnostics = true;
			}

			if (context.EnableExceptions)
			{
				configuration.EnableExceptions = true;
			}
		}

		/// <summary>
		/// Enables diagnostics if <see cref="LoggingConfiguration.SupportedLogs"/> of the <see cref="LoggingConfiguration"/> is <see langword="true"/>.
		/// </summary>
		/// <param name="logHandler"><see cref="IGeneratorLogHandler"/> to enable diagnostics for.</param>
		public static void EnableDiagnosticsIfSupported(this IGeneratorLogHandler logHandler)
		{
			logHandler.LoggingConfiguration.EnableDiagnosticsIfSupported();
		}

		/// <summary>
		/// Enables diagnostics if <see cref="LoggingConfiguration.SupportedLogs"/> of the <paramref name="configuration"/> is <see langword="true"/>.
		/// </summary>
		/// <param name="configuration"><see cref="LoggingConfiguration"/> to enable diagnostics for.</param>
		public static void EnableDiagnosticsIfSupported(this LoggingConfiguration configuration)
		{
			if (configuration.SupportsDiagnostics)
			{
				configuration.EnableDiagnostics = true;
			}
		}

		/// <summary>
		/// Enables generator logging if <see cref="LoggingConfiguration.IsGloballyEnabled"/> is <see langword="true"/>.
		/// </summary>
		/// <param name="logHandler"><see cref="IGeneratorLogHandler"/> to enable logging for.</param>
		public static void EnableLoggingIfSupported(this IGeneratorLogHandler logHandler)
		{
			logHandler.LoggingConfiguration.EnableLoggingIfSupported();
		}

		/// <summary>
		/// Enables generator logging if <see cref="LoggingConfiguration.IsGloballyEnabled"/> is <see langword="true"/>.
		/// </summary>
		/// <param name="configuration"><see cref="LoggingConfiguration"/> to enable logging for.</param>
		public static void EnableLoggingIfSupported(this LoggingConfiguration configuration)
		{
			if (LoggingConfiguration.IsGloballyEnabled)
			{
				configuration.EnableLogging = true;
			}
		}
	}
}
