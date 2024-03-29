// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using Durian.Analysis.Logging;
using Microsoft.CodeAnalysis;

namespace Durian.Analysis
{
	/// <summary>
	/// <see cref="ISourceGenerator"/> that provides additional information about the current generator pass.
	/// </summary>
	public interface IDurianGenerator : ISourceGenerator, IDisposable
	{
		/// <summary>
		/// Name of this <see cref="IDurianGenerator"/>.
		/// </summary>
		string? GeneratorName { get; }

		/// <summary>
		/// Version of this <see cref="IDurianGenerator"/>.
		/// </summary>
		string? GeneratorVersion { get; }

		/// <summary>
		/// Service that handles log files for this generator.
		/// </summary>
		IGeneratorLogHandler? LogHandler { get; }

		/// <summary>
		/// Number of trees generated statically by this generator.
		/// </summary>
		int NumStaticTrees { get; }

		/// <summary>
		/// Determines whether the current generator supports creating <see cref="LoggingConfiguration"/> using the <see cref="EnableLoggingAttribute"/> during generator execution.
		/// </summary>
		bool SupportsDynamicLoggingConfiguration { get; }

		/// <inheritdoc cref="ISourceGenerator.Execute(GeneratorExecutionContext)"/>
		bool Execute(in GeneratorExecutionContext context);

		/// <summary>
		/// Returns data used during the current generator pass.
		/// </summary>
		IGeneratorPassContext? GetCurrentPassContext();
	}
}
