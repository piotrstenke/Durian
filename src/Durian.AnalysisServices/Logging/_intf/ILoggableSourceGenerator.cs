namespace Durian.Analysis.Logging;

/// <summary>
/// Provides support for logging inside a source generator.
/// </summary>
public interface ILoggableSourceGenerator
{
	/// <summary>
	/// Service that handles log files for this generator.
	/// </summary>
	IGeneratorLogHandler? LogHandler { get; }

	/// <summary>
	/// Determines whether the current generator supports creating <see cref="LoggingConfiguration"/> using the <see cref="EnableLoggingAttribute"/> during generator execution.
	/// </summary>
	bool SupportsDynamicLoggingConfiguration { get; }
}
