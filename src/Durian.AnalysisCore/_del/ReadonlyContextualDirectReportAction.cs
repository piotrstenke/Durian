using Microsoft.CodeAnalysis;

namespace Durian
{
	/// <inheritdoc cref="ContextualDirectReportAction{T}"/>
	public delegate void ReadonlyContextualDirectReportAction<T>(in T context, Diagnostic diagnostic) where T : struct;
}
