// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Durian.Analysis.Data;
using Durian.Analysis.Logging;

namespace Durian.Analysis.Cache
{
	/// <inheritdoc cref="CachedDurianGenerator{TCompilationData, TSyntaxReceiver, TFilter, TData}"/>
	public abstract class CachedDurianGenerator<TCompilationData, TData> : CachedDurianGenerator<TCompilationData, IDurianSyntaxReceiver, ICachedGeneratorSyntaxFilterWithDiagnostics<TData>, TData>
		where TCompilationData : class, ICompilationDataWithSymbols
	{
		/// <inheritdoc cref="CachedDurianGenerator{TCompilationData, TData}.CachedDurianGenerator(in LoggableGeneratorConstructionContext, IHintNameProvider?)"/>
		protected CachedDurianGenerator()
		{
		}

		/// <inheritdoc cref="CachedDurianGenerator{TCompilationData, TData}.CachedDurianGenerator(in LoggableGeneratorConstructionContext, IHintNameProvider?)"/>
		protected CachedDurianGenerator(in LoggableGeneratorConstructionContext context) : base(in context)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="CachedDurianGenerator{TCompilationData, TData}"/> class.
		/// </summary>
		/// <param name="context">Configures how this <see cref="LoggableSourceGenerator"/> is initialized.</param>
		/// <param name="fileNameProvider">Creates names for generated files.</param>
		protected CachedDurianGenerator(in LoggableGeneratorConstructionContext context, IHintNameProvider? fileNameProvider) : base(in context, fileNameProvider)
		{
		}

		/// <inheritdoc cref="CachedDurianGenerator(GeneratorLoggingConfiguration?, IHintNameProvider?)"/>
		protected CachedDurianGenerator(GeneratorLoggingConfiguration? loggingConfiguration) : base(loggingConfiguration)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="CachedDurianGenerator{TCompilationData, TData}"/> class.
		/// </summary>
		/// <param name="loggingConfiguration">Determines how the source generator should behave when logging information.</param>
		/// <param name="fileNameProvider">Creates names for generated files.</param>
		protected CachedDurianGenerator(GeneratorLoggingConfiguration? loggingConfiguration, IHintNameProvider? fileNameProvider) : base(loggingConfiguration, fileNameProvider)
		{
		}
	}
}
