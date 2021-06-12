// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Durian.Generator.Cache;
using Microsoft.CodeAnalysis;

namespace Durian.Generator.Manager
{
	/// <summary>
	/// <see cref="DurianManager"/> that can execute source generators.
	/// </summary>
	/// <typeparam name="T">Type of values this manager can cache.</typeparam>
	public abstract class DurianManagerWithGenerators<T> : CachedDurianManager<T>, ISourceGenerator
	{
		private ICachedDurianSourceGenerator<T>[]? _generators;

		/// <summary>
		/// Initializes a new instance of the <see cref="DurianManagerWithGenerators{T}"/> class.
		/// </summary>
		protected DurianManagerWithGenerators()
		{
			_generators = GetSourceGenerators();
		}

		/// <inheritdoc/>
		public void Execute(GeneratorExecutionContext context)
		{
			if (_generators is null || _generators.Length == 0)
			{
				return;
			}

			CachedGeneratorExecutionContext<T> c = new(in context, Cached);

			foreach (ICachedDurianSourceGenerator<T> generator in _generators)
			{
				generator.Execute(in c);
			}
		}

		/// <inheritdoc/>
		public void Initialize(GeneratorInitializationContext context)
		{
			context.RegisterForSyntaxNotifications(CreateSyntaxReceiver);

			if (_generators is null)
			{
				_generators = GetSourceGenerators();
			}
		}

		/// <summary>
		/// Creates a new <see cref="ISyntaxReceiver"/> to be used during the generator execution pass.
		/// </summary>
		protected abstract IDurianSyntaxReceiver CreateSyntaxReceiver();

		/// <summary>
		/// Returns an array of all <see cref="IDurianSourceGenerator"/>s that should be executed by this manager.
		/// </summary>
		protected abstract ICachedDurianSourceGenerator<T>[] GetSourceGenerators();
	}
}
