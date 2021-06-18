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
#pragma warning disable RS1001 // Missing diagnostic analyzer attribute.

	/// <summary>
	/// Analyzes if the current compilation is <see cref="CSharpCompilation"/>.
	/// </summary>
#if !MAIN_PACKAGE

	[DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.FSharp, LanguageNames.VisualBasic)]
#endif

	public sealed class IsCSharpCompilationAnalyzer : DurianAnalyzer
#pragma warning restore RS1001 // Missing diagnostic analyzer attribute.
	{
		/// <inheritdoc/>
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(
			DUR0004_DurianModulesAreValidOnlyInCSharp,
			DUR0006_ProjectMustUseCSharp9
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
			if (GetDiagnostic(compilation) is DiagnosticDescriptor d)
			{
				diagnosticReceiver.ReportDiagnostic(d);
				return false;
			}

			return true;
		}

		/// <summary>
		/// Analyzes the specified <paramref name="compilation"/>.
		/// </summary>
		/// <param name="compilation"><see cref="Compilation"/> to analyze.</param>
		public static bool Analyze(Compilation compilation)
		{
			return GetDiagnostic(compilation) is null;
		}

		/// <inheritdoc/>
		public override void Register(IDurianAnalysisContext context)
		{
			context.RegisterCompilationAction(Analyze);
		}

		private static void Analyze(CompilationAnalysisContext context)
		{
			if (GetDiagnostic(context.Compilation) is DiagnosticDescriptor d)
			{
				context.ReportDiagnostic(Diagnostic.Create(d, Location.None));
			}
		}

		private static DiagnosticDescriptor? GetDiagnostic(Compilation compilation)
		{
			if (compilation is not CSharpCompilation c)
			{
				return DUR0004_DurianModulesAreValidOnlyInCSharp;
			}

			if (c.LanguageVersion < LanguageVersion.CSharp9)
			{
				return DUR0006_ProjectMustUseCSharp9;
			}

			return null;
		}
	}
}
