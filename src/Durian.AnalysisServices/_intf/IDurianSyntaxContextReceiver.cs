using Microsoft.CodeAnalysis;

namespace Durian.Analysis;

/// <summary>
/// Durian-specific <see cref="ISyntaxContextReceiver"/>.
/// </summary>
public interface IDurianSyntaxContextReceiver : IDurianSyntaxReceiver, ISyntaxContextReceiver
{
}
