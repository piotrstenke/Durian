using Microsoft.CodeAnalysis;

namespace Durian.Generator
{
	/// <inheritdoc cref="ContextualReportAction{T}"/>
	public delegate void ReadonlyContextualReportAction<T>(in T context, DiagnosticDescriptor descriptor, Location? location, params object?[]? messageArgs) where T : struct;
}
