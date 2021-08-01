// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Durian.Analysis.Logging;
using Durian.Info;

#if !MAIN_PACKAGE

using Microsoft.CodeAnalysis;

#endif

namespace Durian.Analysis
{
	/// <summary>
	/// Enables the <see cref="DurianModule.Core"/> module.
	/// </summary>
#if !MAIN_PACKAGE

	[Generator(LanguageNames.CSharp)]
#endif

	[GeneratorLoggingConfiguration(SupportedLogs = GeneratorLogs.All, LogDirectory = "Core", SupportsDiagnostics = false, RelativeToGlobal = true, EnableExceptions = true)]
	public sealed class EnableCoreGenerator : DurianGeneratorBase
	{
		/// <summary>
		/// Number of trees generated statically by this generator.
		/// </summary>
#if MAIN_PACKAGE
		public const int NumStaticTrees = 0;
#else
		public const int NumStaticTrees = 1;
#endif

		/// <inheritdoc cref="DurianGeneratorBase.GetGeneratorName"/>
		public static string GeneratorName => nameof(EnableCoreGenerator);

		/// <inheritdoc cref="DurianGeneratorBase.GetVersion"/>
		public static string Version => "2.0.0";

		/// <inheritdoc cref="EnableCoreGenerator(in LoggableGeneratorConstructionContext, IHintNameProvider?)"/>
		public EnableCoreGenerator()
		{
		}

		/// <inheritdoc cref="EnableCoreGenerator(in LoggableGeneratorConstructionContext, IHintNameProvider?)"/>
		public EnableCoreGenerator(in LoggableGeneratorConstructionContext context) : base(in context)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="EnableCoreGenerator"/> class.
		/// </summary>
		/// <param name="context">Configures how this <see cref="LoggableSourceGenerator"/> is initialized.</param>
		/// <param name="fileNameProvider">Creates names for generated files.</param>
		public EnableCoreGenerator(in LoggableGeneratorConstructionContext context, IHintNameProvider? fileNameProvider) : base(in context, fileNameProvider)
		{
		}

		/// <inheritdoc cref="EnableCoreGenerator(GeneratorLoggingConfiguration?, IHintNameProvider?)"/>
		public EnableCoreGenerator(GeneratorLoggingConfiguration? loggingConfiguration) : base(loggingConfiguration)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="EnableCoreGenerator"/> class.
		/// </summary>
		/// <param name="loggingConfiguration">Determines how the source generator should behave when logging information.</param>
		/// <param name="fileNameProvider">Creates names for generated files.</param>
		public EnableCoreGenerator(GeneratorLoggingConfiguration? loggingConfiguration, IHintNameProvider? fileNameProvider) : base(loggingConfiguration, fileNameProvider)
		{
		}

		/// <inheritdoc/>
		protected override DurianModule[] GetEnabledModules()
		{
			return new DurianModule[] { DurianModule.Core };
		}

		/// <inheritdoc/>
		protected override string GetGeneratorName()
		{
			return GeneratorName;
		}

		/// <inheritdoc/>
		protected override string GetVersion()
		{
			return Version;
		}
	}
}
