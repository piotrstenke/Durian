using Durian.Generator.Logging;
using Durian.Info;
using Microsoft.CodeAnalysis;

namespace Durian.Generator
{
	/// <summary>
	/// Enables the <see cref="DurianModule.Core"/> module.
	/// </summary>
	[Generator(LanguageNames.CSharp)]
	public sealed class EnableCoreGenerator : DurianGeneratorBase
	{
		/// <inheritdoc cref="DurianGeneratorBase.GetVersion"/>
		public static string Version => "1.0.0";

		/// <inheritdoc cref="DurianGeneratorBase.GetGeneratorName"/>
		public static string GeneratorName => nameof(EnableCoreGenerator);

		/// <summary>
		/// Number of trees generated statically by this generator.
		/// </summary>
		public const int NumStaticTrees = 1;

		/// <inheritdoc cref="EnableCoreGenerator(in LoggableGeneratorConstructionContext, IFileNameProvider?)"/>
		public EnableCoreGenerator()
		{
		}

		/// <inheritdoc cref="EnableCoreGenerator(in LoggableGeneratorConstructionContext, IFileNameProvider?)"/>
		public EnableCoreGenerator(in LoggableGeneratorConstructionContext context) : base(in context)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="EnableCoreGenerator"/> class.
		/// </summary>
		/// <param name="context">Configures how this <see cref="LoggableSourceGenerator"/> is initialized.</param>
		/// <param name="fileNameProvider">Creates names for generated files.</param>
		public EnableCoreGenerator(in LoggableGeneratorConstructionContext context, IFileNameProvider? fileNameProvider) : base(in context, fileNameProvider)
		{
		}

		/// <inheritdoc cref="EnableCoreGenerator(GeneratorLoggingConfiguration?, IFileNameProvider?)"/>
		public EnableCoreGenerator(GeneratorLoggingConfiguration? loggingConfiguration) : base(loggingConfiguration)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="EnableCoreGenerator"/> class.
		/// </summary>
		/// <param name="loggingConfiguration">Determines how the source generator should behave when logging information.</param>
		/// <param name="fileNameProvider">Creates names for generated files.</param>
		public EnableCoreGenerator(GeneratorLoggingConfiguration? loggingConfiguration, IFileNameProvider? fileNameProvider) : base(loggingConfiguration, fileNameProvider)
		{
		}

		/// <inheritdoc/>
		protected override string GetVersion()
		{
			return Version;
		}

		/// <inheritdoc/>
		protected override string GetGeneratorName()
		{
			return GeneratorName;
		}

		/// <inheritdoc/>
		protected override DurianModule[] GetEnabledModules()
		{
			return new DurianModule[] { DurianModule.Core };
		}
	}
}
