// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Durian.Analysis.Logging;
using Durian.Info;
using Microsoft.CodeAnalysis;

namespace Durian.Analysis.InterfaceTargets
{
	/// <summary>
	/// Enables the <see cref="DurianModule.InterfaceTargets"/> module.
	/// </summary>
#if !MAIN_PACKAGE

	[Generator(LanguageNames.CSharp)]
#endif

	[GeneratorLoggingConfiguration(SupportedLogs = GeneratorLogs.All, LogDirectory = "InterfaceTargets", SupportsDiagnostics = false, RelativeToGlobal = true, EnableExceptions = true)]
	public sealed class EnableModuleGenerator_IntfTarg : DurianGeneratorBase
	{
		/// <inheritdoc cref="EnableModuleGenerator_IntfTarg(in LoggableGeneratorConstructionContext, IHintNameProvider?)"/>
		public EnableModuleGenerator_IntfTarg()
		{
		}

		/// <inheritdoc cref="EnableModuleGenerator_IntfTarg(in LoggableGeneratorConstructionContext, IHintNameProvider?)"/>
		public EnableModuleGenerator_IntfTarg(in LoggableGeneratorConstructionContext context) : base(in context)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="EnableModuleGenerator_IntfTarg"/> class.
		/// </summary>
		/// <param name="context">Configures how this <see cref="LoggableSourceGenerator"/> is initialized.</param>
		/// <param name="fileNameProvider">Creates names for generated files.</param>
		public EnableModuleGenerator_IntfTarg(in LoggableGeneratorConstructionContext context, IHintNameProvider? fileNameProvider) : base(in context, fileNameProvider)
		{
		}

		/// <inheritdoc cref="EnableModuleGenerator_IntfTarg(GeneratorLoggingConfiguration?, IHintNameProvider?)"/>
		public EnableModuleGenerator_IntfTarg(GeneratorLoggingConfiguration? loggingConfiguration) : base(loggingConfiguration)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="EnableModuleGenerator_IntfTarg"/> class.
		/// </summary>
		/// <param name="loggingConfiguration">Determines how the source generator should behave when logging information.</param>
		/// <param name="fileNameProvider">Creates names for generated files.</param>
		public EnableModuleGenerator_IntfTarg(GeneratorLoggingConfiguration? loggingConfiguration, IHintNameProvider? fileNameProvider) : base(loggingConfiguration, fileNameProvider)
		{
		}

		/// <inheritdoc/>
		protected override DurianModule[] GetEnabledModules()
		{
			return new DurianModule[] { DurianModule.InterfaceTargets };
		}

		/// <inheritdoc/>
		protected override string GetGeneratorName()
		{
			return nameof(EnableModuleGenerator_IntfTarg);
		}
	}
}
