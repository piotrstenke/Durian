// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Durian.Analysis.Data;
using Durian.Analysis.Logging;

namespace Durian.Analysis
{
	/// <inheritdoc cref="DurianGenerator{TCompilationData, TSyntaxReceiver, TFilter}"/>
	public abstract class DurianGenerator<TCompilationData> : DurianGenerator<TCompilationData, IDurianSyntaxReceiver, IGeneratorSyntaxFilterWithDiagnostics>
		where TCompilationData : class, ICompilationDataWithSymbols
	{
		/// <inheritdoc cref="DurianGenerator{TCompilationData}.DurianGenerator(in LoggableGeneratorConstructionContext, IHintNameProvider?)"/>
		protected DurianGenerator()
		{
		}

		/// <inheritdoc cref="DurianGenerator{TCompilationData}.DurianGenerator(in LoggableGeneratorConstructionContext, IHintNameProvider?)"/>
		protected DurianGenerator(in LoggableGeneratorConstructionContext context) : base(in context)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DurianGenerator{TCompilationData}"/> class.
		/// </summary>
		/// <param name="context">Configures how this <see cref="LoggableSourceGenerator"/> is initialized.</param>
		/// <param name="fileNameProvider">Creates names for generated files.</param>
		protected DurianGenerator(in LoggableGeneratorConstructionContext context, IHintNameProvider? fileNameProvider) : base(in context, fileNameProvider)
		{
		}

		/// <inheritdoc cref="DurianGenerator(GeneratorLoggingConfiguration?, IHintNameProvider?)"/>
		protected DurianGenerator(GeneratorLoggingConfiguration? loggingConfiguration) : base(loggingConfiguration)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DurianGenerator{TCompilationData}"/> class.
		/// </summary>
		/// <param name="loggingConfiguration">Determines how the source generator should behave when logging information.</param>
		/// <param name="fileNameProvider">Creates names for generated files.</param>
		protected DurianGenerator(GeneratorLoggingConfiguration? loggingConfiguration, IHintNameProvider? fileNameProvider) : base(loggingConfiguration, fileNameProvider)
		{
		}
	}
}