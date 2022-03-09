// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Concurrent;

namespace Durian.Analysis.Cache
{
	/// <summary>
	/// <see cref="DurianAnalyzer"/> that caches the result of analysis.
	/// </summary>
	/// <typeparam name="TTarget">Type of values this analyzer can cache.</typeparam>
	public abstract class CachedAnalyzer<TTarget> : DurianAnalyzer, ICachedAnalyzer<TTarget>
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="CachedAnalyzer{TTarget}"/> class.
		/// </summary>
		protected CachedAnalyzer()
		{
		}

		/// <inheritdoc/>
#pragma warning disable CS0809 // Obsolete member overrides non-obsolete member

		[Obsolete("This method shouldn't be used directly - use Register(IDurianAnalysisContext, ConcurrentDictionary<FileLinePositionSpan, TTarget>) instead.")]
		public override sealed void Register(IDurianAnalysisContext context)
#pragma warning restore CS0809 // Obsolete member overrides non-obsolete member
		{
			ConcurrentDictionary<FileLinePositionSpan, TTarget> dict = new();
			Register(context, dict);
		}

		/// <inheritdoc cref="ICachedAnalyzer{T}.Register(IDurianAnalysisContext, CSharpCompilation, ConcurrentDictionary{FileLinePositionSpan, T})"/>
		public abstract void Register(IDurianAnalysisContext context, ConcurrentDictionary<FileLinePositionSpan, TTarget> cached);

		void ICachedAnalyzer<TTarget>.Register(IDurianAnalysisContext context, CSharpCompilation compilation, ConcurrentDictionary<FileLinePositionSpan, TTarget> cached)
		{
			Register(context, cached);
		}
	}
}