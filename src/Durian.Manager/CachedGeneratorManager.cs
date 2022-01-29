// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Durian.Analysis;
using Durian.Analysis.Cache;
using Microsoft.CodeAnalysis;

namespace Durian.Manager
{
	/// <summary>
	/// <see cref="AnalyzerManager"/> that can execute source generators and cache results of analysis.
	/// </summary>
	/// <typeparam name="T">Type of values this manager can cache.</typeparam>
	public abstract class CachedGeneratorManager<T> : GeneratorManager
	{
		/// <summary>
		/// <see cref="ConcurrentDictionary{TKey, TValue}"/> that contains the cached values.
		/// </summary>
		protected ConcurrentDictionary<FileLinePositionSpan, T> Cached { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="CachedGeneratorManager{T}"/> class.
		/// </summary>
		protected CachedGeneratorManager()
		{
			Cached = new();
		}

		/// <inheritdoc/>
		public override void Execute(GeneratorExecutionContext context)
		{
			if (!ShouldAnalyze(context.Compilation))
			{
				return;
			}

			CachedGeneratorExecutionContext<T> cachedContext = new(in context, Cached);

			foreach (ISourceGenerator generator in GetGenerators())
			{
				if (generator is IDurianGenerator durianGenerator)
				{
					if (durianGenerator is ICachedGenerator<T> cachedGenerator)
					{
						cachedGenerator.Execute(in cachedContext);
					}
					else
					{
						durianGenerator.Execute(in context);
					}
				}
				else
				{
					generator.Execute(context);
				}
			}
		}

		/// <summary>
		/// Returns a collection of registered <see cref="ICachedGenerator{T}"/>s.
		/// </summary>
		public IEnumerable<ICachedGenerator<T>> GetCachedGenerators()
		{
			return GetGenerators().OfType<ICachedGenerator<T>>();
		}
	}
}