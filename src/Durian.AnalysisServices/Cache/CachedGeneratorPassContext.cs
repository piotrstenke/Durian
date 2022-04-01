// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Durian.Analysis.Data;
using Durian.Analysis.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Threading;

namespace Durian.Analysis.Cache
{
	/// <summary>
	/// <see cref="GeneratorPassContext"/> that provides a <see cref="CachedGeneratorExecutionContext{T}"/> instead of <see cref="GeneratorExecutionContext"/>.
	/// </summary>
	public class CachedGeneratorPassContext<TData> : DurianGeneratorWithBuilder.BuilderContext, ICachedGeneratorPassContext<TData> where TData : IMemberData
	{
		internal CachedGeneratorExecutionContext<TData> _cachedContext;

		/// <summary>
		/// <see cref="CachedGeneratorExecutionContext{T}"/> created for the current generator pass.
		/// </summary>
		public new ref readonly CachedGeneratorExecutionContext<TData> OriginalContext => ref _cachedContext;

		/// <summary>
		/// Initializes a new instance of the <see cref="CachedGeneratorPassContext{TData}"/> class.
		/// </summary>
		/// <param name="fileNameProvider">Creates names for generated files.</param>
		/// <param name="parseOptions"><see cref="CSharpParseOptions"/> that will be used to parse any added sources.</param>
		protected internal CachedGeneratorPassContext(IHintNameProvider? fileNameProvider = default, CSharpParseOptions? parseOptions = default) : base(fileNameProvider, parseOptions)
		{
		}

		internal CachedGeneratorPassContext(
			in CachedGeneratorExecutionContext<TData> originalContext,
			IDurianGenerator generator,
			ICompilationData targetCompilation,
			IDurianSyntaxReceiver syntaxReceiver,
			CSharpParseOptions parseOptions,
			IHintNameProvider fileNameProvider,
			IGeneratorServiceContainer services,
			DiagnosticReceiver.ReadonlyContextual<GeneratorExecutionContext>? diagnosticReceiver = null,
			LoggableDiagnosticReceiver? logReceiver = null,
			CancellationToken cancellationToken = default
		) : base(in originalContext.GetContext(), generator, targetCompilation, syntaxReceiver, parseOptions, fileNameProvider, services, diagnosticReceiver, logReceiver, cancellationToken)
		{
			_cachedContext = originalContext;
		}
	}
}
