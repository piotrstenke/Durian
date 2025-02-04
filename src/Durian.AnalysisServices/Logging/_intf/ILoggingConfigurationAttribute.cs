﻿namespace Durian.Analysis.Logging;

internal interface ILoggingConfigurationAttribute
{
	NodeOutput DefaultNodeOutput { get; }
	bool EnableExceptions { get; }
	string? LogDirectory { get; }
	bool RelativeToDefault { get; }
	bool RelativeToGlobal { get; }
	GeneratorLogs SupportedLogs { get; }
	bool SupportsDiagnostics { get; }
}
