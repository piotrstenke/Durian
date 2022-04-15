// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;
using System.Threading;
using Durian.Analysis.Data;
using Durian.Analysis.Extensions;
using Durian.Analysis.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

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
		public IHintNameProvider FileNameProvider { get; internal set; }

		/// <inheritdoc/>
		public IDurianGenerator Generator { get; internal set; }

		/// <inheritdoc/>
		public IDurianSyntaxReceiver SyntaxReceiver { get; internal set; }

		/// <inheritdoc/>
		public ICompilationData TargetCompilation { get; internal set; }

		/// <inheritdoc/>
		public CSharpParseOptions ParseOptions { get; internal set; }

		/// <inheritdoc/>
		public IGeneratorServiceResolver Services { get; internal set; }

		/// <inheritdoc/>
		public GeneratorState State { get; internal set; }

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
			FileNameProvider = fileNameProvider!;
			ParseOptions = parseOptions!;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GeneratorPassContext"/> class.
		/// </summary>
		/// <param name="originalContext"><see cref="GeneratorExecutionContext"/> created for the current generator pass.</param>
		/// <param name="generator"><see cref="IDurianGenerator"/> this context was created for.</param>
		/// <param name="targetCompilation"><see cref="ICompilationData"/> this <see cref="IDurianGenerator"/> operates on.</param>
		/// <param name="syntaxReceiver"><see cref="IDurianSyntaxReceiver"/> that provides the <see cref="SyntaxNode"/>es that will take part in the generation.</param>
		/// <param name="parseOptions"><see cref="CSharpParseOptions"/> that will be used to parse any added sources.</param>
		/// <param name="fileNameProvider">Creates names for generated files.</param>
		/// <param name="services">Container of services that can be resolved during the current generator pass.</param>
		/// <param name="cancellationToken"><see cref="System.Threading.CancellationToken"/> that can be checked to see if the generation should be canceled.</param>
		public GeneratorPassContext(
			in GeneratorExecutionContext originalContext,
			IDurianGenerator generator,
			ICompilationData targetCompilation,
			IDurianSyntaxReceiver syntaxReceiver,
			CSharpParseOptions parseOptions,
			IHintNameProvider fileNameProvider,
			IGeneratorServiceResolver services,
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
			CancellationToken = cancellationToken;
		}

		/// <summary>
		/// Returns a <see cref="IDiagnosticReceiver"/> that will be actually used during the current generation pass.
		/// </summary>
		public IDiagnosticReceiver? GetActualDiagnosticReceiver()
		{
			return Generator.GetFilterMode() switch
			{
				FilterMode.Diagnostics => DiagnosticReceiver.Factory.SourceGenerator(),
				FilterMode.Logs => GetLogReceiverOrEmpty(false),
				FilterMode.Both => GetLogReceiverOrEmpty(true),
				_ => default
			};
		}

		IDiagnosticReceiver? IGeneratorPassContext.GetDiagnosticReceiver()
		{
			return GetActualDiagnosticReceiver();
		}

		private INodeDiagnosticReceiver GetLogReceiverOrEmpty(bool includeDiagnostics)
		{
			if (Generator.LogHandler is null)
			{
				return DiagnosticReceiver.Factory.Empty();
			}

			LogReceiver logReceiver = new(Generator.LogHandler);

			if (includeDiagnostics)
			{
				DiagnosticReceiver.Composite dr = DiagnosticReceiver.Factory.Composite();

				dr.AddReceiver(DiagnosticReceiver.Factory.SourceGenerator(_originalContext));
				dr.AddReceiver(logReceiver, true);

				return dr;
			}

			return logReceiver;
		}
	}
}
