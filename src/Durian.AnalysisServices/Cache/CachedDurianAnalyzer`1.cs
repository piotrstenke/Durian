// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Concurrent;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;

namespace Durian.Analysis.Cache
{
	/// <summary>
	/// <see cref="DurianAnalyzer"/> that caches the result of analysis.
	/// </summary>
	/// <typeparam name="TTarget">Type of values this analyzer can cache.</typeparam>
	public abstract class CachedDurianAnalyzer<TTarget> : DurianAnalyzer, ICachedAnalyzerInfo<TTarget>
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="CachedDurianAnalyzer{TTarget}"/> class.
		/// </summary>
		protected CachedDurianAnalyzer()
		{
		}

		/// <inheritdoc/>
		[Obsolete("This method shouldn't be used directly - use Register(IDurianAnalysisContext, ConcurrentDictionary<FileLinePositionSpan, TTarget>) instead.")]
#pragma warning disable CS0809 // Obsolete member overrides non-obsolete member
		public sealed override void Register(IDurianAnalysisContext context)
#pragma warning restore CS0809 // Obsolete member overrides non-obsolete member
		{
			ConcurrentDictionary<FileLinePositionSpan, TTarget> dict = new();
			Register(context, dict);
		}

		/// <inheritdoc cref="ICachedAnalyzerInfo{T}.Register(IDurianAnalysisContext, CSharpCompilation, ConcurrentDictionary{FileLinePositionSpan, T})"/>
		public abstract void Register(IDurianAnalysisContext context, ConcurrentDictionary<FileLinePositionSpan, TTarget> cached);

		void ICachedAnalyzerInfo<TTarget>.Register(IDurianAnalysisContext context, CSharpCompilation compilation, ConcurrentDictionary<FileLinePositionSpan, TTarget> cached)
		{
			Register(context, cached);
		}
	}
}
