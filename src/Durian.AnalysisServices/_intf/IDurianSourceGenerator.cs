// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System.Threading;
using Durian.Analysis.Data;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Durian.Analysis
{
	/// <summary>
	/// <see cref="ISourceGenerator"/> that provides additional information about the current generator pass.
	/// </summary>
	public interface IDurianSourceGenerator : ISourceGenerator
	{
		/// <summary>
		/// A <see cref="System.Threading.CancellationToken"/> that can be checked to see if the generation should be canceled.
		/// </summary>
		CancellationToken CancellationToken { get; }

		/// <summary>
		/// Determines whether this <see cref="IDurianSourceGenerator"/> allows to report any <see cref="Diagnostic"/>s during the current execution pass.
		/// </summary>
		bool EnableDiagnostics { get; set; }

		/// <summary>
		/// Determines whether this <see cref="IDurianSourceGenerator"/> allows to create log files during the current execution pass.
		/// </summary>
		bool EnableLogging { get; set; }

		/// <summary>
		/// Name of this <see cref="IDurianSourceGenerator"/>.
		/// </summary>
		string GeneratorName { get; }

		/// <summary>
		/// <see cref="CSharpParseOptions"/> that will be used to parse any added sources.
		/// </summary>
		CSharpParseOptions ParseOptions { get; }

		/// <summary>
		/// Determines whether this <see cref="IDurianSourceGenerator"/> supports reporting of <see cref="Diagnostic"/>s.
		/// </summary>
		/// <remarks>Value of this property should never change.</remarks>
		bool SupportsDiagnostics { get; }

		/// <summary>
		/// <see cref="IDurianSyntaxReceiver"/> that provides the <see cref="SyntaxNode"/>es that will take part in the generation.
		/// </summary>
		IDurianSyntaxReceiver SyntaxReceiver { get; }

		/// <summary>
		/// <see cref="ICompilationData"/> this <see cref="IDurianSourceGenerator"/> operates on.
		/// </summary>
		ICompilationData TargetCompilation { get; }

		/// <summary>
		/// Version of this <see cref="IDurianSourceGenerator"/>.
		/// </summary>
		string Version { get; }

		/// <summary>
		/// Creates a new <see cref="IDurianSyntaxReceiver"/> to be used during the generator execution pass.
		/// </summary>
		IDurianSyntaxReceiver CreateSyntaxReceiver();

		/// <inheritdoc cref="ISourceGenerator.Execute(GeneratorExecutionContext)"/>
		void Execute(in GeneratorExecutionContext context);
	}
}
