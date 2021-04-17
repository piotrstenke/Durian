using Microsoft.CodeAnalysis;

namespace Durian
{
	/// <inheritdoc cref="DiagnosticReceiverAction{T}"/>
	public delegate void ReadonlyDiagnosticReceiverAction<T>(in T context, DiagnosticDescriptor descriptor, Location? location, params object?[]? messageArgs) where T : struct;
}
