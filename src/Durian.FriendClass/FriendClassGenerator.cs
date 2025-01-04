using System.Collections.Generic;
using Durian.Analysis.Logging;
using Durian.Info;
using Microsoft.CodeAnalysis;

namespace Durian.Analysis.FriendClass;

/// <summary>
/// Generates syntax tree of types required by the <c>FriendClass</c> module.
/// </summary>
[Generator(LanguageNames.CSharp)]
[LoggingConfiguration(
	SupportedLogs = GeneratorLogs.All,
	LogDirectory = "FriendClass",
	SupportsDiagnostics = true,
	RelativeToGlobal = true,
	EnableExceptions = true)]
public sealed class FriendClassGenerator : DurianBasicGenerator
{
	/// <summary>
	/// Name of this source generator.
	/// </summary>
	public override string GeneratorName => "FriendClass";

	/// <summary>
	/// Version of this source generator.
	/// </summary>
	public override string GeneratorVersion => "2.0.0";

	/// <summary>
	/// Initializes a new instance of the <see cref="FriendClassGenerator"/> class.
	/// </summary>
	public FriendClassGenerator()
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="FriendClassGenerator"/> class.
	/// </summary>
	/// <param name="context">Configures how this <see cref="FriendClassGenerator"/> is initialized.</param>
	public FriendClassGenerator(in GeneratorLogCreationContext context) : base(in context)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="FriendClassGenerator"/> class.
	/// </summary>
	/// <param name="loggingConfiguration">Determines how the source generator should behave when logging information.</param>
	public FriendClassGenerator(LoggingConfiguration? loggingConfiguration) : base(loggingConfiguration)
	{
	}

	/// <summary>
	/// Returns a collection of <see cref="ISourceTextProvider"/> used by this generator to create initial sources.
	/// </summary>
	public static IEnumerable<ISourceTextProvider> GetSourceProviders()
	{
		return new ISourceTextProvider[]
		{
			new FriendClassAttributeProvider(),
			new FriendClassConfigurationAttributeProvider()
		};
	}

	/// <inheritdoc/>
	protected internal override IEnumerable<ISourceTextProvider>? GetInitialSources()
	{
		return GetSourceProviders();
	}

	/// <inheritdoc/>
	protected internal override DurianModule[] GetRequiredModules()
	{
		return new DurianModule[] { DurianModule.FriendClass };
	}
}
