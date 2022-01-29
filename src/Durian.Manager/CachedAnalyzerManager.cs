// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Durian.Analysis;
using Durian.Analysis.Cache;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Durian.Manager
{
	/// <summary>
	/// <see cref="AnalyzerManager"/> that caches the results of analysis.
	/// </summary>
	/// <typeparam name="T">Type of values this manager can cache.</typeparam>
	public abstract class CachedAnalyzerManager<T> : AnalyzerManager
	{
		/// <summary>
		/// <see cref="ConcurrentDictionary{TKey, TValue}"/> that contains the cached values.
		/// </summary>
		protected ConcurrentDictionary<FileLinePositionSpan, T> Cached { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="CachedAnalyzerManager{T}"/> class.
		/// </summary>
		protected CachedAnalyzerManager()
		{
			Cached = new();
		}

		/// <inheritdoc/>
		public override void Register(IDurianAnalysisContext context, CSharpCompilation compilation)
		{
			foreach (IDurianAnalyzer analyzer in GetAnalyzers())
			{
				if (analyzer is ICachedAnalyzer<T> cachedAnalyzer)
				{
					cachedAnalyzer.Register(context, compilation, Cached);
				}
				else
				{
					analyzer.Register(context, compilation);
				}
			}
		}

		/// <summary>
		/// Returns a collection of registered <see cref="ICachedAnalyzer{T}"/>s.
		/// </summary>
		public IEnumerable<ICachedAnalyzer<T>> GetCachedAnalyzers()
		{
			return GetAnalyzers().OfType<ICachedAnalyzer<T>>();
		}
	}
}