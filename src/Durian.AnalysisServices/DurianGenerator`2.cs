// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Durian.Generator.Data;
using Durian.Generator.Logging;

namespace Durian.Generator
{
	/// <inheritdoc cref="DurianGenerator{TCompilationData, TSyntaxReceiver, TFilter}"/>
	public abstract class DurianGenerator<TCompilationData, TSyntaxReceiver> : DurianGenerator<TCompilationData, TSyntaxReceiver, IGeneratorSyntaxFilterWithDiagnostics>
		where TCompilationData : class, ICompilationDataWithSymbols
		where TSyntaxReceiver : class, IDurianSyntaxReceiver
	{
		/// <inheritdoc cref="DurianGenerator{TCompilationData, TSyntaxReceiver}.DurianGenerator(in LoggableGeneratorConstructionContext, IHintNameProvider?)"/>
		protected DurianGenerator()
		{
		}

		/// <inheritdoc cref="DurianGenerator{TCompilationData, TSyntaxReceiver}.DurianGenerator(in LoggableGeneratorConstructionContext, IHintNameProvider?)"/>
		protected DurianGenerator(in LoggableGeneratorConstructionContext context) : base(in context)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DurianGenerator{TCompilationData, TSyntaxReceiver}"/> class.
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
		/// Initializes a new instance of the <see cref="DurianGenerator{TCompilationData, TSyntaxReceiver}"/> class.
		/// </summary>
		/// <param name="loggingConfiguration">Determines how the source generator should behave when logging information.</param>
		/// <param name="fileNameProvider">Creates names for generated files.</param>
		protected DurianGenerator(GeneratorLoggingConfiguration? loggingConfiguration, IHintNameProvider? fileNameProvider) : base(loggingConfiguration, fileNameProvider)
		{
		}
	}
}
