// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Durian.Analysis;
using Durian.Analysis.Cache;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Durian.Manager
{
	/// <summary>
	/// <see cref="AnalysisManager"/> that caches the results of analysis.
	/// </summary>
	/// <typeparam name="T">Type of values this manager can cache.</typeparam>
	public abstract class CachedAnalysisManager<T> : AnalysisManager
	{
		private ICachedAnalyzerInfo<T>[] _cachedAnalyzers;

		/// <summary>
		/// <see cref="ConcurrentDictionary{TKey, TValue}"/> that contains the cached values.
		/// </summary>
		protected ConcurrentDictionary<FileLinePositionSpan, T> Cached { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="CachedAnalysisManager{T}"/> class.
		/// </summary>
		protected CachedAnalysisManager()
		{
			Cached = new();
			_cachedAnalyzers = GetCachedAnalyzersCore();
		}

		/// <summary>
		/// Returns a collection of all normal and cached <see cref="IAnalyzerInfo"/>s.
		/// </summary>
		public IEnumerable<IAnalyzerInfo> GetAllAnalyzers()
		{
			IAnalyzerInfo[] analyzers = new IAnalyzerInfo[_analyzers.Length + _cachedAnalyzers.Length];
			Array.Copy(_analyzers, analyzers, _analyzers.Length);
			Array.Copy(_cachedAnalyzers, 0, analyzers, _analyzers.Length, _cachedAnalyzers.Length);
			return analyzers;
		}

		/// <summary>
		/// Returns an collection of analyzers that provide actions to execute and cache the result of analysis.
		/// </summary>
		public IEnumerable<ICachedAnalyzerInfo<T>> GetCachedAnalyzers()
		{
			ICachedAnalyzerInfo<T>[] analyzers = new ICachedAnalyzerInfo<T>[_cachedAnalyzers.Length];
			Array.Copy(_cachedAnalyzers, analyzers, _cachedAnalyzers.Length);
			return analyzers;
		}

		/// <inheritdoc/>
		public override void Register(IDurianAnalysisContext context, Compilation compilation)
		{
			base.Register(context, compilation);
			CSharpCompilation c = (CSharpCompilation)compilation;

			foreach (ICachedAnalyzerInfo<T> analyzer in _cachedAnalyzers)
			{
				analyzer.Register(context, c, Cached);
			}
		}

		/// <inheritdoc/>
		public override void Reset()
		{
			base.Reset();
			_cachedAnalyzers = GetCachedAnalyzersCore();
		}

		/// <summary>
		/// Returns an array of analyzers that provide actions to execute and cache the result of analysis.
		/// </summary>
		protected abstract ICachedAnalyzerInfo<T>[] GetCachedAnalyzersCore();
	}
}
