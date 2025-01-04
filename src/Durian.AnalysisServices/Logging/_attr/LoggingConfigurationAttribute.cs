using System;
using System.Diagnostics;
using Microsoft.CodeAnalysis;

namespace Durian.Analysis.Logging;

/// <summary>
/// Determines the behavior of the target <see cref="ISourceGenerator"/> when creating log files.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Assembly, AllowMultiple = false, Inherited = true)]
[Conditional("DEBUG")]
public sealed class LoggingConfigurationAttribute : Attribute, ILoggingConfigurationAttribute
{
	/// <summary>
	/// Determines what to output when a <see cref="SyntaxNode"/> is being logged and no other <see cref="NodeOutput"/> is specified.
	/// </summary>
	public NodeOutput DefaultNodeOutput { get; set; }

	/// <summary>
	/// Determines whether to enable the <see cref="ISourceGenerator"/> can throw <see cref="Exception"/>s. Defaults to <see langword="false"/>
	/// </summary>
	public bool EnableExceptions { get; set; }

	/// <summary>
	/// The directory the source generator logs will be written to. If not specified, <see cref="LoggingConfiguration.DefaultLogDirectory"/> is used instead.
	/// </summary>
	public string? LogDirectory { get; set; }

	/// <summary>
	/// Determines whether the <see cref="LogDirectory"/> is relative to the <see cref="LoggingConfiguration.DefaultLogDirectory"/>. Defaults to <see langword="false"/>.
	/// </summary>
	public bool RelativeToDefault { get; set; }

	/// <inheritdoc cref="LoggingConfiguration.SupportedLogs"/>
	public GeneratorLogs SupportedLogs { get; set; }

	/// <summary>
	/// Determines whether the <see cref="ISourceGenerator"/> supports reporting <see cref="Diagnostic"/>s. Defaults to <see langword="false"/>
	/// </summary>
	public bool SupportsDiagnostics { get; set; }

	/// <summary>
	/// Determines whether the <see cref="LogDirectory"/> is relative to the global <see cref="LogDirectory"/>. Defaults to <see langword="true"/>.
	/// </summary>
	public bool RelativeToGlobal { get; set; } = true;

	/// <summary>
	/// Initializes a new instance of the <see cref="LoggingConfigurationAttribute"/> class.
	/// </summary>
	public LoggingConfigurationAttribute()
	{
	}
}
