// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp;

namespace Durian.Analysis
{
	/// <summary>
	/// Provides a method that returns an array of <see cref="CSharpSyntaxNode"/>s.
	/// </summary>
	public interface INodeProvider
	{
		/// <summary>
		/// Returns a collection of <see cref="CSharpSyntaxNode"/>s.
		/// </summary>
		IEnumerable<CSharpSyntaxNode> GetNodes();
	}
}
