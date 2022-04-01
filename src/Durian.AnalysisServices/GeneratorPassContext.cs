// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Durian.Analysis.Data;
using Durian.Analysis.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Generic;
using System.Threading;

namespace Durian.Analysis
{
	/// <summary>
	/// Data used during a generator pass.
	/// </summary>
	public class GeneratorPassContext : IGeneratorPassContext
	{
		internal GeneratorExecutionContext _originalContext;

		/// <inheritdoc/>
		public ref readonly GeneratorExecutionContext OriginalContext => ref _originalContext;

		/// <inheritdoc/>
		public CancellationToken CancellationToken { get; internal set; }

		/// <inheritdoc/>
		public DiagnosticReceiver.ReadonlyContextual<GeneratorExecutionContext>? DiagnosticReceiver { get; internal set; }

		/// <inheritdoc/>
		public IHintNameProvider FileNameProvider { get; }

		/// <inheritdoc/>
		public IDurianGenerator Generator { get; internal set; }

		/// <inheritdoc/>
		public LoggableDiagnosticReceiver? LogReceiver { get; internal set; }

		/// <inheritdoc/>
		public IDurianSyntaxReceiver SyntaxReceiver { get; internal set; }

		/// <inheritdoc/>
		public ICompilationData TargetCompilation { get; internal set; }

		/// <inheritdoc/>
		public CSharpParseOptions ParseOptions { get; }

		/// <inheritdoc/>
		public IGeneratorServiceContainer Services { get; internal set; }

		internal List<CSharpSyntaxTree> GenerationQueue { get; } = new();
		internal bool IsFilterWithGeneratedSymbols { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="GeneratorPassContext"/> class.
		/// </summary>
		/// <param name="fileNameProvider">Creates names for generated files.</param>
		/// <param name="parseOptions"><see cref="CSharpParseOptions"/> that will be used to parse any added sources.</param>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
		protected internal GeneratorPassContext(IHintNameProvider? fileNameProvider = default, CSharpParseOptions? parseOptions = default)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
		{
			FileNameProvider = fileNameProvider ?? new SymbolNameToFile();
			ParseOptions = parseOptions ?? CSharpParseOptions.Default;
		}

		internal GeneratorPassContext(
			in GeneratorExecutionContext originalContext,
			IDurianGenerator generator,
			ICompilationData targetCompilation,
			IDurianSyntaxReceiver syntaxReceiver,
			CSharpParseOptions parseOptions,
			IHintNameProvider fileNameProvider,
			IGeneratorServiceContainer services,
			DiagnosticReceiver.ReadonlyContextual<GeneratorExecutionContext>? diagnosticReceiver = default,
			LoggableDiagnosticReceiver? logReceiver = default,
			CancellationToken cancellationToken = default
		)
		{
			_originalContext = originalContext;
			Generator = generator;
			TargetCompilation = targetCompilation;
			SyntaxReceiver = syntaxReceiver;
			ParseOptions = parseOptions;
			FileNameProvider = fileNameProvider;
			Services = services;
			DiagnosticReceiver = diagnosticReceiver;
			LogReceiver = logReceiver;
			CancellationToken = cancellationToken;
		}
	}
}
