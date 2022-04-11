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
	[LoggingConfiguration(
		SupportedLogs = GeneratorLogs.All,
		LogDirectory = "InterfaceTargets",
		SupportsDiagnostics = true,
		RelativeToGlobal = true,
		EnableExceptions = true)]
	public class InterfaceTargetsGenerator : DurianGeneratorBase
	{
		/// <summary>
		/// Name of this source generator.
		/// </summary>
		public override string GeneratorName => "InterfaceTargets";

		/// <summary>
		/// Version of this source generator.
		/// </summary>
		public override string GeneratorVersion => "2.0.0";

		/// <summary>
		/// Initializes a new instance of the <see cref="InterfaceTargetsGenerator"/> class.
		/// </summary>
		public InterfaceTargetsGenerator()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="InterfaceTargetsGenerator"/> class.
		/// </summary>
		/// <param name="context">Configures how this <see cref="InterfaceTargetsGenerator"/> is initialized.</param>
		public InterfaceTargetsGenerator(in GeneratorLogCreationContext context) : base(in context)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="InterfaceTargetsGenerator"/> class.
		/// </summary>
		/// <param name="loggingConfiguration">Determines how the source generator should behave when logging information.</param>
		public InterfaceTargetsGenerator(LoggingConfiguration? loggingConfiguration) : base(loggingConfiguration)
		{
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

		/// <inheritdoc/>
		public override IEnumerable<ISourceTextProvider>? GetInitialSources()
		{
			return GetSourceProviders();
		}

		/// <inheritdoc/>
		public override DurianModule[] GetRequiredModules()
		{
			return new DurianModule[] { DurianModule.InterfaceTargets };
		}
	}
}
