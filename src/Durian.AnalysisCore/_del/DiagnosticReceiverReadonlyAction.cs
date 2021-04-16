using Microsoft.CodeAnalysis;

namespace Durian
{
	/// <inheritdoc cref="DiagnosticReceiverAction{T}"/>
	public delegate void DiagnosticReceiverReadonlyAction<T>(in T context, DiagnosticDescriptor descriptor, Location? location, params object?[]? messageArgs) where T : struct;
}
