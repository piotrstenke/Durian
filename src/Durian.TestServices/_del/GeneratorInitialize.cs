// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Durian.Generator;
using Microsoft.CodeAnalysis;

namespace Durian.Tests
{
	/// <summary>
	/// A delegate that mirrors the signature of the <see cref="DurianGenerator{TCompilationData, TSyntaxReceiver, TFilter}.Initialize(GeneratorInitializationContext)"/> method.
	/// </summary>
	/// <param name="context">The <see cref="GeneratorInitializationContext"/> to be used when performing the action.</param>
	public delegate void GeneratorInitialize(GeneratorInitializationContext context);
}
