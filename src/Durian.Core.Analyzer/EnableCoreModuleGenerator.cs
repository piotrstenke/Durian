﻿// Copyright (c) Piotr Stenke. All rights reserved.
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

	[LoggingConfiguration(SupportedLogs = GeneratorLogs.All, LogDirectory = "Core", SupportsDiagnostics = false, RelativeToGlobal = true, EnableExceptions = true)]
	public sealed class EnableCoreModuleGenerator : DurianGeneratorBase
	{
		/// <inheritdoc cref="EnableCoreModuleGenerator(in ConstructionContext, IHintNameProvider?)"/>
		public EnableCoreModuleGenerator()
		{
		}

		/// <inheritdoc cref="EnableCoreModuleGenerator(in ConstructionContext, IHintNameProvider?)"/>
		public EnableCoreModuleGenerator(in ConstructionContext context) : base(in context)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="EnableCoreModuleGenerator"/> class.
		/// </summary>
		/// <param name="context">Configures how this <see cref="LoggableGenerator"/> is initialized.</param>
		/// <param name="fileNameProvider">Creates names for generated files.</param>
		public EnableCoreModuleGenerator(in ConstructionContext context, IHintNameProvider? fileNameProvider) : base(in context, fileNameProvider)
		{
		}

		/// <inheritdoc cref="EnableCoreModuleGenerator(LoggingConfiguration?, IHintNameProvider?)"/>
		public EnableCoreModuleGenerator(LoggingConfiguration? loggingConfiguration) : base(loggingConfiguration)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="EnableCoreModuleGenerator"/> class.
		/// </summary>
		/// <param name="loggingConfiguration">Determines how the source generator should behave when logging information.</param>
		/// <param name="fileNameProvider">Creates names for generated files.</param>
		public EnableCoreModuleGenerator(LoggingConfiguration? loggingConfiguration, IHintNameProvider? fileNameProvider) : base(loggingConfiguration, fileNameProvider)
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
			return nameof(EnableCoreModuleGenerator);
		}

		/// <inheritdoc/>
		protected override string? GetGeneratorVersion()
		{
			return "2.0.0";
		}
	}
}