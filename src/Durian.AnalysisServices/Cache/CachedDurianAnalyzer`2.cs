﻿// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Concurrent;
using Microsoft.CodeAnalysis;
using Durian.Analysis.Data;
using Microsoft.CodeAnalysis.CSharp;
using System;

namespace Durian.Analysis.Cache
{
	/// <summary>
	/// <see cref="DurianAnalyzer{TCompilation}"/> that caches the result of analysis.
	/// </summary>
	/// <typeparam name="TTarget">Type of values this analyzer can cache.</typeparam>
	/// <typeparam name="TCompilation">Type of <see cref="ICompilationData"/> this <see cref="DurianAnalyzer"/> uses.</typeparam>
	public abstract class CachedDurianAnalyzer<TTarget, TCompilation> : DurianAnalyzer<TCompilation>, ICachedAnalyzerInfo<TTarget> where TCompilation : class, ICompilationData
	{
		private static readonly IDiagnosticReceiver _diagnosticReceiver = DiagnosticReceiverFactory.Empty();

		/// <summary>
		/// Initializes a new instance of the <see cref="CachedDurianAnalyzer{TTarget, TCompilation}"/> class.
		/// </summary>
		protected CachedDurianAnalyzer()
		{
		}

		/// <inheritdoc/>
		[Obsolete("This method shouldn't be used directly - use Register(IDurianAnalysisContext, TCompilation, ConcurrentDictionary<FileLinePositionSpan, TTarget>) instead.")]
#pragma warning disable CS0809 // Obsolete member overrides non-obsolete member
		public sealed override void Register(IDurianAnalysisContext context, TCompilation compilation)
#pragma warning restore CS0809 // Obsolete member overrides non-obsolete member
		{
			ConcurrentDictionary<FileLinePositionSpan, TTarget> dict = new();
			Register(context, compilation, dict);
		}

		/// <summary>
		/// Registers actions to be performed by the analyzer and caches the result of analysis.
		/// </summary>
		/// <param name="context"><see cref="IDurianAnalysisContext"/> used to register the actions to be performed.</param>
		/// <param name="compilation">Compilation to be used during the analysis.</param>
		/// <param name="cached">A <see cref="ConcurrentDictionary{TKey, TValue}"/> that contains the cached values.</param>
		public abstract void Register(IDurianAnalysisContext context, TCompilation compilation, ConcurrentDictionary<FileLinePositionSpan, TTarget> cached);

		void ICachedAnalyzerInfo<TTarget>.Register(IDurianAnalysisContext context, CSharpCompilation compilation, ConcurrentDictionary<FileLinePositionSpan, TTarget> cached)
		{
			TCompilation c = CreateCompilation(compilation, _diagnosticReceiver);
			Register(context, c, cached);
		}
	}
}