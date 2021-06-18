// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace Durian.Analysis.Logging
{
	/// <summary>
	/// An <see cref="ISourceGenerator"/> that can create log files.
	/// </summary>
	public interface ILoggableSourceGenerator : ISourceGenerator
	{
		/// <inheritdoc cref="GeneratorLoggingConfiguration"/>
		GeneratorLoggingConfiguration LoggingConfiguration { get; }

		/// <summary>
		/// Logs an input <see cref="SyntaxNode"/> and <see cref="Diagnostic"/>s that were created for that node.
		/// </summary>
		/// <param name="node"><see cref="SyntaxNode"/> the diagnostics were created for.</param>
		/// <param name="hintName">Name of the log file to log to.</param>
		/// <param name="diagnostics">A collection of <see cref="Diagnostic"/>s that were created for this <paramref name="node"/>.</param>
		void LogDiagnostics(SyntaxNode node, string hintName, IEnumerable<Diagnostic> diagnostics);

		/// <summary>
		/// Logs an <see cref="Exception"/>.
		/// </summary>
		/// <param name="exception"><see cref="Exception"/> to log.</param>
		void LogException(Exception exception);

		/// <summary>
		/// Logs an input and output <see cref="SyntaxNode"/>.
		/// </summary>
		/// <param name="input">Input <see cref="SyntaxNode"/>.</param>
		/// <param name="output">Output <see cref="SyntaxNode"/>.</param>
		/// <param name="hintName">Name of the log file to log to.</param>
		void LogInputOutput(SyntaxNode input, SyntaxNode output, string hintName);

		/// <summary>
		/// Logs a generated <see cref="SyntaxNode"/>.
		/// </summary>
		/// <param name="node"><see cref="SyntaxNode"/> to log.</param>
		/// <param name="hintName">Name of the log file to log to.</param>
		void LogNode(SyntaxNode node, string hintName);
	}
}
