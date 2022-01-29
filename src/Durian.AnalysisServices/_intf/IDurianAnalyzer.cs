// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Durian.Analysis
{
	/// <summary>
	/// Durian-specific diagnostic analyzer.
	/// </summary>
	public interface IDurianAnalyzer
	{
		/// <summary>
		/// Determines whether this analyzer should analyze generated code.
		/// </summary>
		bool AllowGenerated { get; }

		/// <summary>
		/// Determines whether this analyzer can be run concurrently.
		/// </summary>
		bool Concurrent { get; }

		/// <summary>
		/// Returns a collection of <see cref="DiagnosticDescriptor"/>s this analyzer supports.
		/// </summary>
		IEnumerable<DiagnosticDescriptor> GetSupportedDiagnostics();

		/// <summary>
		/// Registers actions to be performed by the analyzer.
		/// </summary>
		/// <param name="context"><see cref="IDurianAnalysisContext"/> used to register the actions to be performed.</param>
		/// <param name="compilation"><see cref="CSharpCompilation"/> to be used during the analysis.</param>
		void Register(IDurianAnalysisContext context, CSharpCompilation compilation);
	}
}