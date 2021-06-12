// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Durian.Generator
{
	/// <summary>
	/// <see cref="ISyntaxReceiver"/> that provides an additional method for checking if any <see cref="CSharpSyntaxNode"/>s were collected.
	/// </summary>
	public interface IDurianSyntaxReceiver : ISyntaxReceiver, INodeProvider
	{
		/// <summary>
		/// Determines whether the <see cref="ISyntaxReceiver"/> is empty, i.e. it didn't collect any <see cref="CSharpSyntaxNode"/>s.
		/// </summary>
		bool IsEmpty();
	}
}
