// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Immutable;
using Durian.Analysis.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using static Durian.Analysis.DurianDiagnostics;

namespace Durian.Analysis
{
	/// <summary>
	/// Analyzes if the current compilation is <see cref="CSharpCompilation"/>.
	/// </summary>
#pragma warning disable RS1001 // Missing diagnostic analyzer attribute.
#if !MAIN_PACKAGE
	[DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.FSharp, LanguageNames.VisualBasic)]
#endif
	public sealed class IsCSharpCompilationAnalyzer : DurianAnalyzer
#pragma warning restore RS1001 // Missing diagnostic analyzer attribute.
	{
		/// <inheritdoc/>
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(
			DUR0004_DurianModulesAreValidOnlyInCSharp
		);

		/// <summary>
		/// Initializes a new instance of the <see cref="IsCSharpCompilationAnalyzer"/> class.
		/// </summary>
		public IsCSharpCompilationAnalyzer()
		{
		}

		/// <summary>
		/// Analyzes the specified <paramref name="compilation"/>.
		/// </summary>
		/// <param name="diagnosticReceiver"><see cref="IDirectDiagnosticReceiver"/> that is used to report <see cref="Diagnostic"/>s.</param>
		/// <param name="compilation"><see cref="Compilation"/> to analyze.</param>
		public static bool Analyze(IDirectDiagnosticReceiver diagnosticReceiver, Compilation compilation)
		{
			bool value = Analyze(compilation);

			if (!value)
			{
				diagnosticReceiver.ReportDiagnostic(DUR0004_DurianModulesAreValidOnlyInCSharp);
			}

			return value;
		}

		/// <summary>
		/// Analyzes the specified <paramref name="compilation"/>.
		/// </summary>
		/// <param name="compilation"><see cref="Compilation"/> to analyze.</param>
		public static bool Analyze(Compilation compilation)
		{
			return compilation is CSharpCompilation;
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
				context.ReportDiagnostic(Diagnostic.Create(DUR0004_DurianModulesAreValidOnlyInCSharp, Location.None));
			}
		}
	}
}