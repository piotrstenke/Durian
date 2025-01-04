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
		/// <inheritdoc/>
		public override string GeneratorName => nameof(EnableCoreModuleGenerator);

		/// <inheritdoc/>
		public override string GeneratorVersion => "3.0.0";

		/// <summary>
		/// Initializes a new instance of the <see cref="EnableCoreModuleGenerator"/> class.
		/// </summary>
		public EnableCoreModuleGenerator()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="EnableCoreModuleGenerator"/> class.
		/// </summary>
		/// <param name="context">Configures how this <see cref="EnableCoreModuleGenerator"/> is initialized.</param>
		public EnableCoreModuleGenerator(in GeneratorLogCreationContext context) : base(in context)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="EnableCoreModuleGenerator"/> class.
		/// </summary>
		/// <param name="loggingConfiguration">Determines how the source generator should behave when logging information.</param>
		public EnableCoreModuleGenerator(LoggingConfiguration? loggingConfiguration) : base(loggingConfiguration)
		{
		}

		/// <inheritdoc/>
		public override DurianModule[] GetRequiredModules()
		{
			return new DurianModule[] { DurianModule.Core };
		}
	}
}