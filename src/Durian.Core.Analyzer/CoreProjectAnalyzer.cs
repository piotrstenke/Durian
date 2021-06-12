// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Immutable;
using Durian.Generator.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using static Durian.Generator.Core.DurianDiagnostics;

namespace Durian.Generator.Core
{
	/// <summary>
	/// Analyzer that checks if the current compilation references the <c>Durian.Core</c> package.
	/// </summary>
#if !MAIN_PACKAGE

	[DiagnosticAnalyzer(LanguageNames.CSharp)]
#endif
	public sealed class CoreProjectAnalyzer : DurianAnalyzer
	{
		/// <inheritdoc/>
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(
			DUR0001_ProjectMustReferenceDurianCore
		);

		/// <summary>
		/// Initializes a new instance of the <see cref="CoreProjectAnalyzer"/> class.
		/// </summary>
		public CoreProjectAnalyzer()
		{
		}

		/// <summary>
		/// Analyzes the specified <paramref name="compilation"/>.
		/// </summary>
		/// <param name="diagnosticReceiver"><see cref="IDiagnosticReceiver"/> that is used to report <see cref="Diagnostic"/>s.</param>
		/// <param name="compilation"><see cref="Compilation"/> to analyze.</param>
		public static bool Analyze(IDiagnosticReceiver diagnosticReceiver, Compilation compilation)
		{
			if (!Analyze(compilation))
			{
				diagnosticReceiver.ReportDiagnostic(DUR0001_ProjectMustReferenceDurianCore);
				return false;
			}

			return true;
		}

		/// <inheritdoc/>
		public override void Register(IDurianAnalysisContext context)
		{
			context.RegisterCompilationAction(Analyze);
		}

		private static void Analyze(CompilationAnalysisContext context)
		{
			if (!Analyze(context.Compilation))
			{
				context.ReportDiagnostic(Diagnostic.Create(DUR0001_ProjectMustReferenceDurianCore, Location.None));
			}
		}

		private static bool Analyze(Compilation compilation)
		{
			foreach (AssemblyIdentity assembly in compilation.ReferencedAssemblyNames)
			{
				if (assembly.Name == "Durian.Core")
				{
					return true;
				}
			}

			return false;
		}
	}
}
