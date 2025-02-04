﻿using System.Threading;
using Durian.Analysis.Data;
using Durian.Analysis.Logging;
using Microsoft.CodeAnalysis;

namespace Durian.Analysis;

/// <summary>
/// Data used during a generator pass.
/// </summary>
public interface IGeneratorPassContext
{
	/// <summary>
	/// <see cref="System.Threading.CancellationToken"/> that can be checked to see if the generation should be canceled.
	/// </summary>
	CancellationToken CancellationToken { get; }

	/// <summary>
	/// Creates names for generated files.
	/// </summary>
	IHintNameProvider FileNameProvider { get; }

	/// <summary>
	/// <see cref="IDurianGenerator"/> this context was created for.
	/// </summary>
	ILoggableSourceGenerator Generator { get; }

	/// <summary>
	/// <see cref="GeneratorExecutionContext"/> created for the current generator pass.
	/// </summary>
	ref readonly GeneratorExecutionContext OriginalContext { get; }

	/// <summary>
	/// <see cref="ParseOptions"/> that will be used to parse any added sources.
	/// </summary>
	ParseOptions ParseOptions { get; }

	/// <summary>
	/// Container of services that can be resolved during the current generator pass.
	/// </summary>
	IGeneratorServiceResolver Services { get; }

	/// <summary>
	/// Current state of the generator.
	/// </summary>
	GeneratorState State { get; }

	/// <summary>
	/// <see cref="IDurianSyntaxReceiver"/> that provides the <see cref="SyntaxNode"/>es that will take part in the generation.
	/// </summary>
	IDurianSyntaxReceiver SyntaxReceiver { get; }

	/// <summary>
	/// <see cref="ICompilationData"/> this <see cref="IDurianGenerator"/> operates on.
	/// </summary>
	ICompilationData TargetCompilation { get; }

	/// <summary>
	/// Returns a <see cref="IDiagnosticReceiver"/> to be used during the current generation pass.
	/// </summary>
	IDiagnosticReceiver? GetDiagnosticReceiver();
}
