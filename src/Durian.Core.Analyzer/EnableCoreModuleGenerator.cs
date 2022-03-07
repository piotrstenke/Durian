// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Durian.Analysis.Logging;
using Durian.Info;
using Microsoft.CodeAnalysis;

namespace Durian.Analysis
{
    /// <summary>
    /// Enables the <see cref="DurianModule.Core"/> module.
    /// </summary>
    [Generator(LanguageNames.CSharp)]
    [LoggingConfiguration(
        SupportedLogs = GeneratorLogs.All,
        LogDirectory = "Core",
        SupportsDiagnostics = false,
        RelativeToGlobal = true,
        EnableExceptions = true)]
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
        public override string GetGeneratorName()
        {
            return nameof(EnableCoreModuleGenerator);
        }

        /// <inheritdoc/>
        public override string? GetGeneratorVersion()
        {
            return "2.1.0";
        }

        /// <inheritdoc/>
        public override DurianModule[] GetRequiredModules()
        {
            return new DurianModule[] { DurianModule.Core };
        }
    }
}