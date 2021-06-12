// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Concurrent;
using Durian.Generator.Cache;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Durian.Generator.Manager
{
	/// <summary>
	/// <see cref="DurianManager"/> that caches the results of analysis. Useful when dealing with source generators.
	/// </summary>
	/// <typeparam name="T">Type of values this manager can cache.</typeparam>
	public abstract class CachedDurianManager<T> : DurianManager
	{
		/// <summary>
		/// <see cref="ConcurrentDictionary{TKey, TValue}"/> that contains the cached values.
		/// </summary>
		protected ConcurrentDictionary<FileLinePositionSpan, T> Cached { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="CachedDurianManager{T}"/> class.
		/// </summary>
		protected CachedDurianManager()
		{
			Cached = new();
		}

		/// <inheritdoc/>
		protected override IAnalyzerInfo[] GetAnalyzers()
		{
			return Array.Empty<IAnalyzerInfo>();
		}

		/// <summary>
		/// Returns an array of analyzer that provide actions to execute and cache the result of analysis.
		/// </summary>
		protected abstract ICachedAnalyzerInfo<T>[] GetCachedAnalyzers();

		/// <inheritdoc/>
		protected override void RegisterActions(CompilationStartAnalysisContext context)
		{
			base.RegisterActions(context);

			ICachedAnalyzerInfo<T>[] analyzers = GetCachedAnalyzers();

			IDurianAnalysisContext c = new DurianCompilationStartAnalysisContext(context);
			CSharpCompilation compilation = (CSharpCompilation)context.Compilation;

			foreach (ICachedAnalyzerInfo<T> analyzer in analyzers)
			{
				analyzer.Register(c, compilation, Cached);
			}
		}
	}
}
