// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace Durian.Analysis
{
	/// <summary>
	/// Durian-specific wrapper for <see cref="GeneratorExecutionContext"/> with retrievable diagnostics.
	/// </summary>
	public readonly struct DurianGeneratorExecutionContext : IDirectDiagnosticReceiver
	{
		internal const string Exc_NotInitialized = "Context is not initialized!";

		private readonly GeneratorExecutionContext _context;

		private readonly DiagnosticBag _diagnostics;

		/// <inheritdoc cref="GeneratorExecutionContext.AdditionalFiles"/>
		public ImmutableArray<AdditionalText> AdditionalFiles => _context.AdditionalFiles;

		/// <summary>
		/// Provides access to options defined by analyzer configuration.
		/// </summary>
		public AnalyzerConfigOptionsProvider AnalyzerConfigOptions => _context.AnalyzerConfigOptions;

		/// <inheritdoc cref="GeneratorExecutionContext.CancellationToken"/>
		public CancellationToken CancellationToken => _context.CancellationToken;

		/// <summary>
		/// Current <see cref="CSharpCompilation"/> at the time of execution.
		/// </summary>
		/// <remarks>
		/// This compilation contains only the user supplied code; other generated code is not available.
		/// As user code can depend on the results of generation, it is possible that this compilation will contain errors.
		/// </remarks>
		public CSharpCompilation Compilation => (_context.Compilation as CSharpCompilation)!;

		/// <summary>
		/// Determines whether the current instance is initialize, i.e. it wasn't created using the default constructor.
		/// </summary>
		public bool IsInitialized => _diagnostics is not null;

		/// <summary>
		/// <see cref="CSharpParseOptions"/> that will be used to parse any added sources.
		/// </summary>
		public CSharpParseOptions ParseOptions => (_context.ParseOptions as CSharpParseOptions)!;

		/// <summary>
		/// <see cref="IDurianSyntaxReceiver"/> created for the current generation pass.
		/// </summary>
		public IDurianSyntaxReceiver? SyntaxReceiver
		{
			get
			{
				if (_context.SyntaxReceiver is not null)
				{
					return (_context.SyntaxReceiver as IDurianSyntaxReceiver)!;
				}

				if (_context.SyntaxContextReceiver is not null)
				{
					return (_context.SyntaxContextReceiver as IDurianSyntaxReceiver)!;
				}

				return null;
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DurianGeneratorExecutionContext"/> struct.
		/// </summary>
		/// <param name="context"><see cref="GeneratorExecutionContext"/> to wrap.</param>
		public DurianGeneratorExecutionContext(in GeneratorExecutionContext context)
		{
			_context = context;
			_diagnostics = new DiagnosticBag();
		}

		/// <inheritdoc cref="GeneratorExecutionContext.AddSource(string, string)"/>
		public void AddSource(string hintName, string source)
		{
			_context.AddSource(hintName, source);
		}

		/// <summary>
		/// Adds a <see cref="SourceText"/> to the compilation.
		/// </summary>
		/// <param name="hintName">An identifier that can be used to reference this source text, must be unique within this generator.</param>
		/// <param name="sourceText"><see cref="SourceText"/> to add to the compilation.</param>
		public void AddSource(string hintName, SourceText sourceText)
		{
			_context.AddSource(hintName, sourceText);
		}

		/// <summary>
		/// Returns a collection of all <see cref="Diagnostic"/>s reported by the current context.
		/// </summary>
		public IEnumerable<Diagnostic> GetReportedDiagnostics()
		{
			return _diagnostics.GetDiagnostics();
		}

		/// <summary>
		/// Add a <see cref="Diagnostic"/> to the user's compilation.
		/// </summary>
		/// <param name="diagnostic"><see cref="Diagnostic"/> to report.</param>
		/// <exception cref="InvalidOperationException">Context is not initialized.</exception>
		public void ReportDiagnostic(Diagnostic? diagnostic)
		{
			if (diagnostic is null)
			{
				return;
			}

			if (!IsInitialized)
			{
				throw new InvalidOperationException(Exc_NotInitialized);
			}

			_diagnostics.ReportDiagnostic(diagnostic);
		}

		/// <summary>
		/// Add a <see cref="Diagnostic"/> to the user's compilation.
		/// </summary>
		/// <param name="descriptor"><see cref="DiagnosticDescriptor"/> that is used to create the <see cref="Diagnostic"/>.</param>
		/// <param name="location">Source <see cref="Location"/> of the reported diagnostic.</param>>
		/// <param name="messageArgs">Arguments of the diagnostic message.</param>
		/// <exception cref="InvalidOperationException">Context is not initialized.</exception>
		public void ReportDiagnostic(DiagnosticDescriptor? descriptor, Location? location, params object?[]? messageArgs)
		{
			if (descriptor is null)
			{
				return;
			}

			if (!IsInitialized)
			{
				throw new InvalidOperationException(Exc_NotInitialized);
			}

			_diagnostics.ReportDiagnostic(Diagnostic.Create(descriptor, location, messageArgs));
		}
	}
}