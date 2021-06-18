// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Microsoft.CodeAnalysis;

namespace Durian.Analysis
{
	/// <inheritdoc cref="ContextualReportAction{T}"/>
	public delegate void ReadonlyContextualReportAction<T>(in T context, DiagnosticDescriptor descriptor, Location? location, params object?[]? messageArgs) where T : struct;
}
