﻿// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Durian.Generator.Data;
using Durian.Generator.Logging;

namespace Durian.Generator
{
	/// <inheritdoc cref="DurianGeneratorWithBuilder{TCompilationData, TSyntaxReceiver, TFilter}"/>
	public abstract class DurianGeneratorWithBuilder : DurianGeneratorWithBuilder<ICompilationDataWithSymbols, IDurianSyntaxReceiver, IGeneratorSyntaxFilterWithDiagnostics>
	{
		/// <inheritdoc cref="DurianGeneratorWithBuilder(in LoggableGeneratorConstructionContext, IHintNameProvider?)"/>
		protected DurianGeneratorWithBuilder()
		{
		}

		/// <inheritdoc cref="DurianGeneratorWithBuilder(in LoggableGeneratorConstructionContext, IHintNameProvider?)"/>
		protected DurianGeneratorWithBuilder(in LoggableGeneratorConstructionContext context) : base(in context)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DurianGenerator"/> class.
		/// </summary>
		/// <param name="context">Configures how this <see cref="LoggableSourceGenerator"/> is initialized.</param>
		/// <param name="fileNameProvider">Creates names for generated files.</param>
		protected DurianGeneratorWithBuilder(in LoggableGeneratorConstructionContext context, IHintNameProvider? fileNameProvider) : base(in context, fileNameProvider)
		{
		}

		/// <inheritdoc cref="DurianGeneratorWithBuilder(GeneratorLoggingConfiguration?, IHintNameProvider?)"/>
		protected DurianGeneratorWithBuilder(GeneratorLoggingConfiguration? loggingConfiguration) : base(loggingConfiguration)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DurianGenerator"/> class.
		/// </summary>
		/// <param name="loggingConfiguration">Determines how the source generator should behave when logging information.</param>
		/// <param name="fileNameProvider">Creates names for generated files.</param>
		protected DurianGeneratorWithBuilder(GeneratorLoggingConfiguration? loggingConfiguration, IHintNameProvider? fileNameProvider) : base(loggingConfiguration, fileNameProvider)
		{
		}
	}
}