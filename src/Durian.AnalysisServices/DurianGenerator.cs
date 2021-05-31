﻿using Durian.Generator.Data;
using Durian.Generator.Logging;

namespace Durian.Generator
{
	/// <inheritdoc cref="DurianGenerator{TCompilationData, TSyntaxReceiver, TFilter}"/>
	public abstract class DurianGenerator : DurianGenerator<ICompilationDataWithSymbols, IDurianSyntaxReceiver, IGeneratorSyntaxFilterWithDiagnostics>
	{
		/// <inheritdoc cref="DurianGenerator(in LoggableGeneratorConstructionContext, IFileNameProvider?)"/>
		protected DurianGenerator()
		{
		}

		/// <inheritdoc cref="DurianGenerator(in LoggableGeneratorConstructionContext, IFileNameProvider?)"/>
		protected DurianGenerator(in LoggableGeneratorConstructionContext context) : base(in context)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DurianGenerator"/> class.
		/// </summary>
		/// <param name="context">Configures how this <see cref="LoggableSourceGenerator"/> is initialized.</param>
		/// <param name="fileNameProvider">Creates names for generated files.</param>
		protected DurianGenerator(in LoggableGeneratorConstructionContext context, IFileNameProvider? fileNameProvider) : base(in context, fileNameProvider)
		{
		}

		/// <inheritdoc cref="DurianGenerator(GeneratorLoggingConfiguration?, IFileNameProvider?)"/>
		protected DurianGenerator(GeneratorLoggingConfiguration? loggingConfiguration) : base(loggingConfiguration)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DurianGenerator"/> class.
		/// </summary>
		/// <param name="loggingConfiguration">Determines how the source generator should behave when logging information.</param>
		/// <param name="fileNameProvider">Creates names for generated files.</param>
		protected DurianGenerator(GeneratorLoggingConfiguration? loggingConfiguration, IFileNameProvider? fileNameProvider) : base(loggingConfiguration, fileNameProvider)
		{
		}
	}
}