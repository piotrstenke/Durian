// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Microsoft.CodeAnalysis;

namespace Durian.Analysis
{
	/// <summary>
	/// Durian-specific <see cref="ISyntaxReceiver"/>.
	/// </summary>
	public interface IDurianSyntaxReceiver : ISyntaxReceiver, INodeProvider
	{
		/// <summary>
		/// Determines whether the <see cref="ISyntaxReceiver"/> is empty, i.e. it didn't collect any <see cref="SyntaxNode"/>s.
		/// </summary>
		bool IsEmpty();
	}
}
