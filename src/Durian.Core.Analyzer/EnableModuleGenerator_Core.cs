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
	public sealed class EnableModuleGenerator_Core : DurianGeneratorBase
	{
		/// <inheritdoc cref="EnableModuleGenerator_Core(in LoggableGeneratorConstructionContext, IHintNameProvider?)"/>
		public EnableModuleGenerator_Core()
		{
		}

		/// <inheritdoc cref="EnableModuleGenerator_Core(in LoggableGeneratorConstructionContext, IHintNameProvider?)"/>
		public EnableModuleGenerator_Core(in LoggableGeneratorConstructionContext context) : base(in context)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="EnableModuleGenerator_Core"/> class.
		/// </summary>
		/// <param name="context">Configures how this <see cref="LoggableSourceGenerator"/> is initialized.</param>
		/// <param name="fileNameProvider">Creates names for generated files.</param>
		public EnableModuleGenerator_Core(in LoggableGeneratorConstructionContext context, IHintNameProvider? fileNameProvider) : base(in context, fileNameProvider)
		{
		}

		/// <inheritdoc cref="EnableModuleGenerator_Core(GeneratorLoggingConfiguration?, IHintNameProvider?)"/>
		public EnableModuleGenerator_Core(GeneratorLoggingConfiguration? loggingConfiguration) : base(loggingConfiguration)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="EnableModuleGenerator_Core"/> class.
		/// </summary>
		/// <param name="loggingConfiguration">Determines how the source generator should behave when logging information.</param>
		/// <param name="fileNameProvider">Creates names for generated files.</param>
		public EnableModuleGenerator_Core(GeneratorLoggingConfiguration? loggingConfiguration, IHintNameProvider? fileNameProvider) : base(loggingConfiguration, fileNameProvider)
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
			return nameof(EnableModuleGenerator_Core);
		}
	}
}