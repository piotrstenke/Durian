// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Microsoft.CodeAnalysis;

namespace Durian.Generator
{
	/// <inheritdoc cref="ContextualDirectReportAction{T}"/>
	public delegate void ReadonlyContextualDirectReportAction<T>(in T context, Diagnostic diagnostic) where T : struct;
}
