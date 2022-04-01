﻿// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Durian.Analysis.Data;
using Durian.Analysis.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Threading;

namespace Durian.Analysis
{
	/// <summary>
	/// Data used during a generator pass.
	/// </summary>
	public interface IGeneratorPassContext
	{
		/// <summary>
		/// <see cref="GeneratorExecutionContext"/> created for the current generator pass.
		/// </summary>
		ref readonly GeneratorExecutionContext OriginalContext { get; }

		/// <summary>
		/// <see cref="System.Threading.CancellationToken"/> that can be checked to see if the generation should be canceled.
		/// </summary>
		CancellationToken CancellationToken { get; }

		/// <summary>
		/// <see cref="IDiagnosticReceiver"/> that is used to report diagnostics during the current generator pass.
		/// </summary>
		DiagnosticReceiver.ReadonlyContextual<GeneratorExecutionContext>? DiagnosticReceiver { get; }

		/// <summary>
		/// Creates names for generated files.
		/// </summary>
		IHintNameProvider FileNameProvider { get; }

		/// <summary>
		/// <see cref="IDurianGenerator"/> this context was created for.
		/// </summary>
		IDurianGenerator Generator { get; }

		/// <summary>
		/// <see cref="LoggableDiagnosticReceiver"/> that is used to report diagnostics to log files.
		/// </summary>
		LoggableDiagnosticReceiver? LogReceiver { get; }

		/// <summary>
		/// <see cref="IDurianSyntaxReceiver"/> that provides the <see cref="SyntaxNode"/>es that will take part in the generation.
		/// </summary>
		IDurianSyntaxReceiver SyntaxReceiver { get; }

		/// <summary>
		/// <see cref="ICompilationData"/> this <see cref="IDurianGenerator"/> operates on.
		/// </summary>
		ICompilationData TargetCompilation { get; }

		/// <summary>
		/// <see cref="CSharpParseOptions"/> that will be used to parse any added sources.
		/// </summary>
		CSharpParseOptions ParseOptions { get; }

		/// <summary>
		/// Container of services that can be resolved during the current generator pass.
		/// </summary>
		IGeneratorServiceContainer Services { get; }
	}
}
