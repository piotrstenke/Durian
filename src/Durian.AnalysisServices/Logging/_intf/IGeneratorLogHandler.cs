﻿using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace Durian.Analysis.Logging;

/// <summary>
/// Provides methods for creating log files for source generators.
/// </summary>
public interface IGeneratorLogHandler
{
	/// <summary>
	/// Configures how log files are handled.
	/// </summary>
	LoggingConfiguration LoggingConfiguration { get; }

	/// <summary>
	/// Logs an input <see cref="SyntaxNode"/> and <see cref="Diagnostic"/>s that were created for that node.
	/// </summary>
	/// <param name="node"><see cref="SyntaxNode"/> the diagnostics were created for.</param>
	/// <param name="hintName">Name of the log file to log to.</param>
	/// <param name="diagnostics">A collection of <see cref="Diagnostic"/>s that were created for this <paramref name="node"/>.</param>
	/// <param name="nodeOutput">Determines what to output when the <see cref="SyntaxNode"/> is being logged.</param>
	void LogDiagnostics(SyntaxNode node, string hintName, IEnumerable<Diagnostic> diagnostics, NodeOutput nodeOutput = default);

	/// <summary>
	/// Logs an <see cref="Exception"/>.
	/// </summary>
	/// <param name="exception"><see cref="Exception"/> to log.</param>
	void LogException(Exception exception);

	/// <summary>
	/// Logs an <see cref="Exception"/>.
	/// </summary>
	/// <param name="exception"><see cref="Exception"/> to log.</param>
	/// <param name="source">Source of the <paramref name="exception"/>.</param>
	void LogException(Exception exception, string source);

	/// <summary>
	/// Logs an input and output <see cref="SyntaxNode"/>.
	/// </summary>
	/// <param name="input">Input <see cref="SyntaxNode"/>.</param>
	/// <param name="output">Output <see cref="SyntaxNode"/>.</param>
	/// <param name="hintName">Name of the log file to log to.</param>
	/// <param name="nodeOutput">Determines what to output when a <see cref="SyntaxNode"/> is being logged.</param>
	void LogInputOutput(SyntaxNode input, SyntaxNode output, string hintName, NodeOutput nodeOutput = default);

	/// <summary>
	/// Logs a generated <see cref="SyntaxNode"/>.
	/// </summary>
	/// <param name="node"><see cref="SyntaxNode"/> to log.</param>
	/// <param name="hintName">Name of the log file to log to.</param>
	/// <param name="nodeOutput">Determines what to output when the <see cref="SyntaxNode"/> is being logged.</param>
	void LogNode(SyntaxNode node, string hintName, NodeOutput nodeOutput = default);
}
