// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Microsoft.CodeAnalysis;

namespace Durian.Generator.Cache
{
	/// <summary>
	/// <see cref="IDurianSourceGenerator"/> that can retrieve data defined in a <see cref="CachedGeneratorExecutionContext{T}"/>.
	/// </summary>
	/// <typeparam name="T">Type of values this generated can retrieve from the <see cref="CachedGeneratorExecutionContext{T}"/>.</typeparam>
	public interface ICachedDurianSourceGenerator<T> : IDurianSourceGenerator
	{
		/// <summary>
		/// Executes the generator using a <see cref="CachedGeneratorExecutionContext{T}"/> instead of usual <see cref="GeneratorExecutionContext"/>.
		/// </summary>
		/// <param name="context"><see cref="CachedGeneratorExecutionContext{T}"/> that is used to execute the generator.</param>
		void Execute(in CachedGeneratorExecutionContext<T> context);
	}
}
