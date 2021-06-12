// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Durian.Generator.Core;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using static Durian.Generator.Core.DurianDiagnostics;

namespace Durian.Generator.Manager
{
	/// <summary>
	/// Base class for all Durian analysis managers.
	/// </summary>
	public abstract class DurianManager : AnalysisManager
	{
		private bool _allowGenerated;
		private IAnalyzerInfo[]? _analyzers;
		private bool _concurrent;

		/// <inheritdoc/>
		public override bool AllowGenerated => _allowGenerated;

		/// <inheritdoc/>
		public override bool Concurrent => _concurrent;

		/// <inheritdoc/>
		public sealed override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
		{
			get
			{
				DiagnosticDescriptor[] descriptors = GetManagerSpecificDiagnostics().ToArray();

				DiagnosticDescriptor[] all = new DiagnosticDescriptor[descriptors.Length + 3];
				all[0] = DUR0001_ProjectMustReferenceDurianCore;
				all[1] = DUR0004_DurianModulesAreValidOnlyInCSharp;
				all[2] = DUR0006_ProjectMustUseCSharp9;

				Array.Copy(descriptors, 0, all, 3, descriptors.Length);
				return ImmutableArray.Create(all);
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DurianManager"/> class.
		/// </summary>
		protected DurianManager()
		{
		}

		/// <inheritdoc/>
#pragma warning disable RS1025 // Configure generated code analysis
#pragma warning disable RS1026 // Enable concurrent execution

		public sealed override void Initialize(AnalysisContext context)
#pragma warning restore RS1026 // Enable concurrent execution
#pragma warning restore RS1025 // Configure generated code analysis
		{
			if (_analyzers is null)
			{
				_analyzers = GetAnalyzers();
			}

			bool concurrent = true;
			bool allowGenerated = true;

			foreach (IAnalyzerInfo analyzer in _analyzers)
			{
				if (!analyzer.Concurrent)
				{
					concurrent = false;
				}

				if (!analyzer.AllowGenerated)
				{
					allowGenerated = false;
				}

				if (!concurrent && !allowGenerated)
				{
					break;
				}
			}

			_allowGenerated = allowGenerated;
			_concurrent = concurrent;

			base.Initialize(context);
		}

		/// <summary>
		/// Returns an array of analyzers that provide actions to execute.
		/// </summary>
		protected abstract IAnalyzerInfo[] GetAnalyzers();

		/// <summary>
		/// Returns a collection of <see cref="DiagnosticDescriptor"/> that are supported by this manager.
		/// </summary>
		/// <returns></returns>
		protected abstract IEnumerable<DiagnosticDescriptor> GetManagerSpecificDiagnostics();

		/// <inheritdoc/>
		protected override void RegisterActions(CompilationStartAnalysisContext context)
		{
			IAnalyzerInfo[] analyzers = _analyzers ?? GetAnalyzers();

			IDurianAnalysisContext c = new DurianCompilationStartAnalysisContext(context);
			CSharpCompilation compilation = (CSharpCompilation)context.Compilation;

			foreach (IAnalyzerInfo analyzer in analyzers)
			{
				analyzer.Register(c, compilation);
			}

			_analyzers = null;
		}

		/// <inheritdoc/>
		protected override bool ShouldAnalyze(Compilation compilation, out Diagnostic[]? diagnostics)
		{
			DiagnosticBag bag = new(4);

			bool isValid = IsCSharpCompilationAnalyzer.Analyze(bag, compilation);
			isValid &= CoreProjectAnalyzer.Analyze(bag, compilation);

			diagnostics = isValid ? bag.GetDiagnostics() : null;
			return isValid;
		}
	}
}
