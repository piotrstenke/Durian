using Durian.Analysis.Logging;
using Microsoft.CodeAnalysis;

namespace Durian.Analysis.Extensions;

/// <summary>
/// Contains various extension methods for <see cref="ISourceGenerator"/>s.
/// </summary>
public static class GeneratorExtensions
{
	/// <summary>
	/// Determines a <see cref="FilterMode"/> from values provided by the specified <paramref name="context"/>.
	/// </summary>
	/// <param name="context"><see cref="IGeneratorPassContext"/> to get the <see cref="FilterMode"/> value for.</param>
	public static FilterMode GetFilterMode(this IGeneratorPassContext context)
	{
		return context.Generator.GetFilterMode();
	}

	/// <summary>
	/// Determines a <see cref="FilterMode"/> from values provided by the specified <paramref name="generator"/>.
	/// </summary>
	/// <param name="generator"><see cref="IDurianGenerator"/> to get the <see cref="FilterMode"/> value for.</param>
	public static FilterMode GetFilterMode(this ILoggableSourceGenerator generator)
	{
		if (generator.LogHandler is null)
		{
			return FilterMode.None;
		}

		return generator.LogHandler.LoggingConfiguration.GetFilterMode();
	}

	/// <summary>
	/// Determines a <see cref="FilterMode"/> from values provided by the specified <paramref name="logHandler"/>.
	/// </summary>
	/// <param name="logHandler"><see cref="IGeneratorLogHandler"/> to get the <see cref="FilterMode"/> value for.</param>
	public static FilterMode GetFilterMode(this IGeneratorLogHandler logHandler)
	{
		return logHandler.LoggingConfiguration.GetFilterMode();
	}

	/// <summary>
	/// Determines a <see cref="FilterMode"/> from values provided by the specified <paramref name="configuration"/>.
	/// </summary>
	/// <param name="configuration"><see cref="LoggingConfiguration"/> to get the <see cref="FilterMode"/> value for.</param>
	public static FilterMode GetFilterMode(this LoggingConfiguration configuration)
	{
		return GetFilterMode(configuration.EnableDiagnostics, configuration.EnableLogging);
	}

#pragma warning disable RCS1224 // Make method an extension method.

	internal static FilterMode GetFilterMode(bool enableDiagnostics, bool enableLogging)
#pragma warning restore RCS1224 // Make method an extension method.
	{
		FilterMode mode = FilterMode.None;

		if (enableDiagnostics)
		{
			mode += (int)FilterMode.Diagnostics;
		}

		if (enableLogging)
		{
			mode += (int)FilterMode.Logs;
		}

		return mode;
	}
}
