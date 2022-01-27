// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using Durian.Analysis;
using Durian.Analysis.Cache;
using Microsoft.CodeAnalysis;

namespace Durian.Manager
{
	/// <summary>
	/// <see cref="AnalysisManager"/> that can execute source generators.
	/// </summary>
	/// <typeparam name="T">Type of values this manager can cache.</typeparam>
	public abstract class AnalysisManagerWithGenerators<T> : CachedAnalysisManager<T>, ISourceGenerator
	{
		private ICachedDurianSourceGenerator<T>[] _generators;

		/// <summary>
		/// Initializes a new instance of the <see cref="AnalysisManagerWithGenerators{T}"/> class.
		/// </summary>
		protected AnalysisManagerWithGenerators()
		{
			_generators = GetSourceGeneratorsCore();
		}

		/// <summary>
		/// Creates a new <see cref="ISyntaxReceiver"/> to be used during the generator execution pass.
		/// </summary>
		public abstract IDurianSyntaxReceiver CreateSyntaxReceiver();

		/// <inheritdoc/>
		public void Execute(GeneratorExecutionContext context)
		{
			if (!ShouldAnalyze(context.Compilation))
			{
				return;
			}

			CachedGeneratorExecutionContext<T> c = new(in context, Cached);

			foreach (ICachedDurianSourceGenerator<T> generator in _generators)
			{
				generator.Execute(in c);
			}
		}

		/// <summary>
		/// Returns a collection of <see cref="ICachedDurianSourceGenerator{T}"/> to execute.
		/// </summary>
		public IEnumerable<ICachedDurianSourceGenerator<T>> GetSourceGenerators()
		{
			ICachedDurianSourceGenerator<T>[] generators = new ICachedDurianSourceGenerator<T>[_generators.Length];
			Array.Copy(_generators, generators, _generators.Length);
			return generators;
		}

		/// <inheritdoc/>
		public void Initialize(GeneratorInitializationContext context)
		{
			context.RegisterForSyntaxNotifications(CreateSyntaxReceiver);
		}

		/// <inheritdoc/>
		public override void Reset()
		{
			base.Reset();
			_generators = GetSourceGeneratorsCore();
		}

		/// <summary>
		/// Returns an array of <see cref="ICachedDurianSourceGenerator{T}"/> to execute.
		/// </summary>
		protected abstract ICachedDurianSourceGenerator<T>[] GetSourceGeneratorsCore();
	}
}