// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Durian.Analysis.Data;
using Durian.Analysis.Logging;

namespace Durian.Analysis.Cache
{
	/// <inheritdoc cref="CachedGenerator{TData, TCompilationData, TSyntaxReceiver, TFilter}"/>
	public abstract class CachedGenerator<TData> : CachedGenerator<TData, ICompilationDataWithSymbols, IDurianSyntaxReceiver, ICachedGeneratorSyntaxFilterWithDiagnostics<TData>>
	{
		/// <inheritdoc cref="CachedGenerator(in ConstructionContext, IHintNameProvider?)"/>
		protected CachedGenerator()
		{
		}

		/// <inheritdoc cref="CachedGenerator(in ConstructionContext, IHintNameProvider?)"/>
		protected CachedGenerator(in ConstructionContext context) : base(in context)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="CachedGenerator{TData}"/> class.
		/// </summary>
		/// <param name="context">Configures how this <see cref="LoggableGenerator"/> is initialized.</param>
		/// <param name="fileNameProvider">Creates names for generated files.</param>
		protected CachedGenerator(in ConstructionContext context, IHintNameProvider? fileNameProvider) : base(in context, fileNameProvider)
		{
		}

		/// <inheritdoc cref="CachedGenerator(LoggingConfiguration?, IHintNameProvider?)"/>
		protected CachedGenerator(LoggingConfiguration? loggingConfiguration) : base(loggingConfiguration)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="CachedGenerator{TData}"/> class.
		/// </summary>
		/// <param name="loggingConfiguration">Determines how the source generator should behave when logging information.</param>
		/// <param name="fileNameProvider">Creates names for generated files.</param>
		protected CachedGenerator(LoggingConfiguration? loggingConfiguration, IHintNameProvider? fileNameProvider) : base(loggingConfiguration, fileNameProvider)
		{
		}
	}
}