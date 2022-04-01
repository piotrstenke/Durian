// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Durian.Analysis.Data;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;

namespace Durian.Analysis
{

	/// <summary>
	/// <see cref="ISourceGenerator"/> that provides additional information about the current generator pass.
	/// </summary>
	public interface IDurianGenerator : ISourceGenerator
	{
		/// <summary>
		/// Determines whether this <see cref="IDurianGenerator"/> allows to report any <see cref="Diagnostic"/>s during the current execution pass.
		/// </summary>
		bool EnableDiagnostics { get; set; }

		/// <summary>
		/// Determines whether this <see cref="IDurianGenerator"/> allows to create log files during the current execution pass.
		/// </summary>
		bool EnableLogging { get; set; }

		/// <summary>
		/// Name of this <see cref="IDurianGenerator"/>.
		/// </summary>
		string? GeneratorName { get; }

		/// <summary>
		/// Version of this <see cref="IDurianGenerator"/>.
		/// </summary>
		string? GeneratorVersion { get; }

		/// <summary>
		/// Number of trees generated statically by this generator.
		/// </summary>
		int NumStaticTrees { get; }

		/// <summary>
		/// Determines whether this <see cref="IDurianGenerator"/> supports reporting of <see cref="Diagnostic"/>s.
		/// </summary>
		/// <remarks>Value of this property should never change.</remarks>
		bool SupportsDiagnostics { get; }

		/// <inheritdoc cref="ISourceGenerator.Execute(GeneratorExecutionContext)"/>
		void Execute(in GeneratorExecutionContext context);

		/// <summary>
		/// Returns data used the current generator pass.
		/// </summary>
		/// <exception cref="InvalidOperationException">Generator is not initialized.</exception>
		IGeneratorPassContext GetCurrentPassContext();
	}
}
