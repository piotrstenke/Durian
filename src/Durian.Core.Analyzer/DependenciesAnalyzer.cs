// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;
using System.Collections.Immutable;
using Durian.Info;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using static Durian.Analysis.DurianDiagnostics;

namespace Durian.Analysis
{
#pragma warning disable RS1001 // Missing diagnostic analyzer attribute.

	/// <summary>
	/// Analyzer that checks if the current compilation references the <c>Durian.Core</c> package.
	/// </summary>
#if !MAIN_PACKAGE

	[DiagnosticAnalyzer(LanguageNames.CSharp)]
#endif

	public sealed class DependenciesAnalyzer : DurianAnalyzer
#pragma warning restore RS1001 // Missing diagnostic analyzer attribute.
	{
		/// <inheritdoc/>
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(
			DUR0001_ProjectMustReferenceDurianCore,
			DUR0007_DoNotReferencePackageIfManagerIsPresent
		);

		/// <summary>
		/// Initializes a new instance of the <see cref="DependenciesAnalyzer"/> class.
		/// </summary>
		public DependenciesAnalyzer()
		{
		}

		/// <summary>
		/// Analyzes the specified <paramref name="compilation"/>.
		/// </summary>
		/// <param name="diagnosticReceiver"><see cref="IDirectDiagnosticReceiver"/> that is used to report <see cref="Diagnostic"/>s.</param>
		/// <param name="compilation"><see cref="Compilation"/> to analyze.</param>
		public static bool Analyze(IDirectDiagnosticReceiver diagnosticReceiver, Compilation compilation)
		{
			bool isValid = Analyze(compilation, out Diagnostic[] diagnostics);

			foreach (Diagnostic diag in diagnostics)
			{
				diagnosticReceiver.ReportDiagnostic(diag);
			}

			return isValid;
		}

		/// <summary>
		/// Analyzes the specified <paramref name="compilation"/>.
		/// </summary>
		/// <param name="compilation"><see cref="Compilation"/> to analyze.</param>
		public static bool Analyze(Compilation compilation)
		{
			bool hasManager = false;
			bool hasCore = false;
			bool hasAnalyzerPackage = false;

			foreach (AssemblyIdentity assembly in compilation.ReferencedAssemblyNames)
			{
				string name = assembly.Name;

				if (IsDurianManager(name))
				{
					hasCore = true;
					hasManager = true;
				}
				else if (IsDurianCore(name))
				{
					hasCore = true;
				}
				else if (IsDurianAnalyzerPackage(name))
				{
					hasAnalyzerPackage = true;
				}
				else if (hasCore && hasAnalyzerPackage && hasManager)
				{
					break;
				}
			}

			if (!hasCore)
			{
				return false;
			}

			if (hasManager && hasAnalyzerPackage)
			{
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
			Analyze(context.Compilation, out Diagnostic[] diagnostics);

			foreach (Diagnostic diag in diagnostics)
			{
				context.ReportDiagnostic(diag);
			}
		}

		private static bool Analyze(Compilation compilation, out Diagnostic[] diagnostics)
		{
			bool hasManager = false;
			bool hasCore = false;
			List<string> assemblies = new(8);

			foreach (AssemblyIdentity assembly in compilation.ReferencedAssemblyNames)
			{
				string name = assembly.Name;

				if (IsDurianManager(name))
				{
					hasManager = true;
					hasCore = true;
				}
				else if (IsDurianCore(name))
				{
					hasCore = true;
				}
				else if (IsDurianAnalyzerPackage(name))
				{
					assemblies.Add(name);
				}
				else if (hasManager && hasCore && assemblies.Count >= GlobalInfo.NumAnalyzerPackages)
				{
					break;
				}
			}

			List<Diagnostic> d = new(assemblies.Count + 1);
			bool isValid = true;

			if (!hasCore)
			{
				d.Add(Diagnostic.Create(DUR0001_ProjectMustReferenceDurianCore, Location.None));
				isValid = false;
			}

			if (assemblies.Count > 0 && hasManager)
			{
				foreach (string a in assemblies)
				{
					d.Add(Diagnostic.Create(DUR0007_DoNotReferencePackageIfManagerIsPresent, Location.None, a));
				}

				isValid = false;
			}

			diagnostics = d.ToArray();
			return isValid;
		}

		private static bool IsDurianAnalyzerPackage(string assembly)
		{
			return assembly == "Durian.Core.Analyzer" || assembly == "Durian.DefaultParam";
		}

		private static bool IsDurianCore(string assembly)
		{
			return assembly == "Durian.Core";
		}

		private static bool IsDurianManager(string assembly)
		{
			return assembly == "Durian";
		}
	}
}
