// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Microsoft.CodeAnalysis;
using Durian.Info;
using Durian.Analysis.Logging;

namespace Durian.Analysis.FriendClass
{
	/// <summary>
	/// Enables the <see cref="DurianModule.FriendClass"/> module.
	/// </summary>
#if !MAIN_PACKAGE

	[Generator(LanguageNames.CSharp)]
#endif

	[GeneratorLoggingConfiguration(SupportedLogs = GeneratorLogs.All, LogDirectory = "FriendClass", SupportsDiagnostics = false, RelativeToGlobal = true, EnableExceptions = true)]
	public sealed class EnableModuleGenerator_FriendClass : DurianGeneratorBase
	{
		/// <inheritdoc cref="EnableModuleGenerator_FriendClass(in LoggableGeneratorConstructionContext, IHintNameProvider?)"/>
		public EnableModuleGenerator_FriendClass()
		{
		}

		/// <inheritdoc cref="EnableModuleGenerator_FriendClass(in LoggableGeneratorConstructionContext, IHintNameProvider?)"/>
		public EnableModuleGenerator_FriendClass(in LoggableGeneratorConstructionContext context) : base(in context)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="EnableModuleGenerator_FriendClass"/> class.
		/// </summary>
		/// <param name="context">Configures how this <see cref="LoggableSourceGenerator"/> is initialized.</param>
		/// <param name="fileNameProvider">Creates names for generated files.</param>
		public EnableModuleGenerator_FriendClass(in LoggableGeneratorConstructionContext context, IHintNameProvider? fileNameProvider) : base(in context, fileNameProvider)
		{
		}

		/// <inheritdoc cref="EnableModuleGenerator_FriendClass(GeneratorLoggingConfiguration?, IHintNameProvider?)"/>
		public EnableModuleGenerator_FriendClass(GeneratorLoggingConfiguration? loggingConfiguration) : base(loggingConfiguration)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="EnableModuleGenerator_FriendClass"/> class.
		/// </summary>
		/// <param name="loggingConfiguration">Determines how the source generator should behave when logging information.</param>
		/// <param name="fileNameProvider">Creates names for generated files.</param>
		public EnableModuleGenerator_FriendClass(GeneratorLoggingConfiguration? loggingConfiguration, IHintNameProvider? fileNameProvider) : base(loggingConfiguration, fileNameProvider)
		{
		}

		/// <inheritdoc/>
		protected override DurianModule[] GetEnabledModules()
		{
			return new DurianModule[] { DurianModule.FriendClass };
		}

		/// <inheritdoc/>
		protected override string GetGeneratorName()
		{
			return nameof(EnableModuleGenerator_FriendClass);
		}
	}
}
