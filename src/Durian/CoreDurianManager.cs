// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;
using Durian.Generator.Core;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using static Durian.Generator.Core.DurianDiagnostics;

namespace Durian.Generator.Manager
{
	/// <summary>
	/// Manages the analyzers from the <c>Durian.Core.Analyzer</c> package.
	/// </summary>
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public sealed class CoreDurianManager : DurianManager
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="CoreDurianManager"/> class.
		/// </summary>
		public CoreDurianManager()
		{
		}

		/// <inheritdoc/>
		protected override IAnalyzerInfo[] GetAnalyzers()
		{
			return new IAnalyzerInfo[]
			{
				new CustomTypesInGeneratorNamespaceAnalyzer(),
				new TypeImportAnalyzer()
			};
		}

		/// <inheritdoc/>
		protected override IEnumerable<DiagnosticDescriptor> GetManagerSpecificDiagnostics()
		{
			return new DiagnosticDescriptor[]
			{
				DUR0002_ModuleOfTypeIsNotImported,
				DUR0003_DoNotUseTypeFromDurianGeneratorNamespace,
				DUR0005_DoNotAddTypesToGeneratorNamespace
			};
		}
	}
}
