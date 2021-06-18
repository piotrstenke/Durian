// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp;

namespace Durian.Analysis
{
	/// <summary>
	/// Provides a method that returns an array of <see cref="CSharpSyntaxNode"/>s of type <typeparamref name="T"/>.
	/// </summary>
	/// <typeparam name="T">Type of <see cref="CSharpSyntaxNode"/> this <see cref="INodeProvider{T}"/> can return.</typeparam>
	public interface INodeProvider<out T> : INodeProvider where T : CSharpSyntaxNode
	{
		/// <summary>
		/// Returns a collection of <see cref="CSharpSyntaxNode"/>s of type <typeparamref name="T"/>.
		/// </summary>
		new IEnumerable<T> GetNodes();
	}
}
