// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Durian.Analysis.Data;
using Durian.Analysis.Logging;

namespace Durian.Analysis
{
	/// <inheritdoc cref="DurianGeneratorWithBuilder{TCompilationData, TSyntaxReceiver, TFilter}"/>
	public abstract class DurianGeneratorWithBuilder : DurianGeneratorWithBuilder<ICompilationDataWithSymbols, IDurianSyntaxReceiver, IGeneratorSyntaxFilterWithDiagnostics>
	{
		/// <inheritdoc cref="DurianGeneratorWithBuilder(in ConstructionContext, IHintNameProvider?)"/>
		protected DurianGeneratorWithBuilder()
		{
		}

		/// <inheritdoc cref="DurianGeneratorWithBuilder(in ConstructionContext, IHintNameProvider?)"/>
		protected DurianGeneratorWithBuilder(in ConstructionContext context) : base(in context)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DurianGenerator"/> class.
		/// </summary>
		/// <param name="context">Configures how this <see cref="LoggableGenerator"/> is initialized.</param>
		/// <param name="fileNameProvider">Creates names for generated files.</param>
		protected DurianGeneratorWithBuilder(in ConstructionContext context, IHintNameProvider? fileNameProvider) : base(in context, fileNameProvider)
		{
		}

		/// <inheritdoc cref="DurianGeneratorWithBuilder(LoggingConfiguration?, IHintNameProvider?)"/>
		protected DurianGeneratorWithBuilder(LoggingConfiguration? loggingConfiguration) : base(loggingConfiguration)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DurianGenerator"/> class.
		/// </summary>
		/// <param name="loggingConfiguration">Determines how the source generator should behave when logging information.</param>
		/// <param name="fileNameProvider">Creates names for generated files.</param>
		protected DurianGeneratorWithBuilder(LoggingConfiguration? loggingConfiguration, IHintNameProvider? fileNameProvider) : base(loggingConfiguration, fileNameProvider)
		{
		}
	}
}