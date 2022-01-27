// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Microsoft.CodeAnalysis;

namespace Durian.Analysis
{
	/// <summary>
	/// A delegate that reports a <see cref="Diagnostic"/> on the specified <paramref name="context"/>.
	/// </summary>
	/// <typeparam name="T">Type of the <paramref name="context"/>.</typeparam>
	/// <param name="context">Context to report the diagnostics to.</param>
	/// <param name="diagnostic"><see cref="Diagnostic"/> to report.</param>
	public delegate void ContextualDirectReportAction<in T>(T context, Diagnostic diagnostic) where T : struct;
}