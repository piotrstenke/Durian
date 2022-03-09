// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Durian.Info;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Generic;
using System.Collections.Immutable;
using static Durian.Analysis.DurianDiagnostics;

namespace Durian.Analysis
{
	/// <summary>
	/// Analyzer that checks if the current compilation references the <c>Durian.Core</c> package.
	/// </summary>
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public sealed class DependencyAnalyzer : DurianAnalyzer
	{
		/// <inheritdoc/>
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(
			DUR0001_ProjectMustReferenceDurianCore,
			DUR0007_DoNotReferencePackageIfManagerIsPresent,
			DUR0008_MultipleAnalyzers
		);

		/// <summary>
		/// Initializes a new instance of the <see cref="DependencyAnalyzer"/> class.
		/// </summary>
		public DependencyAnalyzer()
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
			List<string> analyzerAssemblies = new(8);

			foreach (AssemblyIdentity assembly in compilation.ReferencedAssemblyNames)
			{
				string name = assembly.Name;

				if (!name.StartsWith("Durian"))
				{
					continue;
				}

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
					analyzerAssemblies.Add(name);
				}
				else if (hasManager && hasCore && analyzerAssemblies.Count >= GlobalInfo.NumAnalyzerPackages)
				{
					break;
				}
			}

			List<Diagnostic> d = new(analyzerAssemblies.Count + 2);
			bool isValid = true;

			if (!hasCore)
			{
				d.Add(Diagnostic.Create(DUR0001_ProjectMustReferenceDurianCore, Location.None));
				isValid = false;
			}

			if (analyzerAssemblies.Count > 0)
			{
				if (hasManager)
				{
					foreach (string a in analyzerAssemblies)
					{
						d.Add(Diagnostic.Create(DUR0007_DoNotReferencePackageIfManagerIsPresent, Location.None, a));
					}

					isValid = false;
				}
				else if (analyzerAssemblies.Count > 1)
				{
					d.Add(Diagnostic.Create(DUR0008_MultipleAnalyzers, Location.None));
				}
			}

			diagnostics = d.ToArray();
			return isValid;
		}

		private static bool IsDurianAnalyzerPackage(string assembly)
		{
			return
				assembly == "Durian.Core.Analyzer" ||
				assembly == "Durian.DefaultParam" ||
				assembly == "Durian.InterfaceTargets" ||
				assembly == "Durian.FriendClass";
		}

		private static bool IsDurianCore(string assembly)
		{
			return assembly == "Durian.Core";
		}

		private static bool IsDurianManager(string assembly)
		{
			return assembly == "Durian" || assembly == "Durian.Manager";
		}
	}
}