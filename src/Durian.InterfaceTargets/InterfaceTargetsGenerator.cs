// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;
using Durian.Analysis.Logging;
using Durian.Info;
using Microsoft.CodeAnalysis;

namespace Durian.Analysis.InterfaceTargets
{
	/// <summary>
	/// Generates syntax tree of types required by the <c>InterfaceTargets</c> module.
	/// </summary>
	[Generator(LanguageNames.CSharp)]
	[LoggingConfiguration(SupportedLogs = GeneratorLogs.All, LogDirectory = "InterfaceTargets", SupportsDiagnostics = true, RelativeToGlobal = true, EnableExceptions = true)]
	public class InterfaceTargetsGenerator : DurianGeneratorBase
	{
		/// <summary>
		/// Name of this source generator.
		/// </summary>
		public static string GeneratorName => "InterfaceTargets";

		/// <summary>
		/// Version of this source generator.
		/// </summary>
		public static string Version => "1.1.0";

		/// <inheritdoc cref="InterfaceTargetsGenerator(in ConstructionContext, IHintNameProvider?)"/>
		public InterfaceTargetsGenerator()
		{
		}

		/// <inheritdoc cref="InterfaceTargetsGenerator(in ConstructionContext, IHintNameProvider?)"/>
		public InterfaceTargetsGenerator(in ConstructionContext context) : base(in context)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="InterfaceTargetsGenerator"/> class.
		/// </summary>
		/// <param name="context">Configures how this <see cref="LoggableGenerator"/> is initialized.</param>
		/// <param name="fileNameProvider">Creates names for generated files.</param>
		public InterfaceTargetsGenerator(in ConstructionContext context, IHintNameProvider? fileNameProvider) : base(in context, fileNameProvider)
		{
		}

		/// <inheritdoc cref="InterfaceTargetsGenerator(LoggingConfiguration?, IHintNameProvider?)"/>
		public InterfaceTargetsGenerator(LoggingConfiguration? loggingConfiguration) : base(loggingConfiguration)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="InterfaceTargetsGenerator"/> class.
		/// </summary>
		/// <param name="loggingConfiguration">Determines how the source generator should behave when logging information.</param>
		/// <param name="fileNameProvider">Creates names for generated files.</param>
		public InterfaceTargetsGenerator(LoggingConfiguration? loggingConfiguration, IHintNameProvider? fileNameProvider) : base(loggingConfiguration, fileNameProvider)
		{
		}

		/// <inheritdoc/>
		protected override DurianModule[] GetRequiredModules()
		{
			return new DurianModule[] { DurianModule.InterfaceTargets };
		}

		/// <inheritdoc/>
		protected override string GetGeneratorVersion()
		{
			return Version;
		}

		/// <inheritdoc/>
		protected override string GetGeneratorName()
		{
			return GeneratorName;
		}

		/// <inheritdoc/>
		protected override IEnumerable<ISourceTextProvider>? GetInitialSources()
		{
			return GetSourceProviders();
		}

		/// <summary>
		/// Returns a collection of <see cref="ISourceTextProvider"/> used by this generator to create initial sources.
		/// </summary>
		public static IEnumerable<ISourceTextProvider> GetSourceProviders()
		{
			return new ISourceTextProvider[]
			{
				new InterfaceTargetsProvider(),
				new InterfaceTargetsAttributeProvider()
			};
		}
	}
}