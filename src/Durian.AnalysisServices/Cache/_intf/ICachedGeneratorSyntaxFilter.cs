// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;
using Durian.Analysis.Data;
using Durian.Analysis.Filtration;
using Microsoft.CodeAnalysis;

namespace Durian.Analysis.Cache
{
	/// <summary>
	/// <see cref="ISyntaxFilter"/> that filtrates <see cref="SyntaxNode"/>s for the specified <see cref="IDurianGenerator"/>. If the value associated with a <see cref="SyntaxNode"/> is present in the <see cref="CachedGeneratorExecutionContext{T}"/>, it is re-used instead of creating a new one.
	/// </summary>
	/// <typeparam name="T">Type of values this syntax filter can retrieve from a <see cref="CachedGeneratorExecutionContext{T}"/>.</typeparam>
	public interface ICachedGeneratorSyntaxFilter<T> : IGeneratorSyntaxFilter
	{
		/// <summary>
		/// Decides, which <see cref="SyntaxNode"/>s collected by a <see cref="IDurianSyntaxReceiver"/> are valid and returns a collection of <see cref="IMemberData"/> based on those <see cref="SyntaxNode"/>s. If the <paramref name="context"/> contains a value associated with the current <see cref="SyntaxNode"/>, this value is used instead.
		/// </summary>
		/// <param name="context"><see cref="ICachedGeneratorPassContext{T}"/> that is used when filtrating the <see cref="IMemberData"/>s.</param>
		IEnumerable<IMemberData> Filtrate(ICachedGeneratorPassContext<T> context);

		/// <summary>
		/// Returns an <see cref="IEnumerator{T}"/> that allows to manually iterate through the filtrated <see cref="IMemberData"/>s.
		/// </summary>
		/// <param name="context"><see cref="ICachedGeneratorPassContext{T}"/> that is used when filtrating the <see cref="IMemberData"/>s.</param>
		IEnumerator<IMemberData> GetEnumerator(ICachedGeneratorPassContext<T> context);
	}
}
