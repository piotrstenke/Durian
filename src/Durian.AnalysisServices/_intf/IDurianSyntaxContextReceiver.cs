// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Microsoft.CodeAnalysis;

namespace Durian.Analysis
{
	/// <summary>
	/// Durian-specific <see cref="ISyntaxContextReceiver"/>.
	/// </summary>
	public interface IDurianSyntaxContextReceiver : IDurianSyntaxReceiver, ISyntaxContextReceiver
	{
	}
}
