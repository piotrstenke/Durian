using System;
using Microsoft.CodeAnalysis;

namespace Durian.Analysis;

/// <summary>
/// <see cref="IDiagnosticReceiver"/> that reports <see cref="Diagnostic"/>s for a specified <see cref="SyntaxNode"/>.
/// </summary>
public interface INodeDiagnosticReceiver : IDiagnosticReceiverWithBuffer
{
	/// <summary>
	/// Sets the <see cref="SyntaxNode"/> that the diagnostics will be reported for.
	/// </summary>
	/// <param name="node"><see cref="SyntaxNode"/> to set as the target.</param>
	/// <param name="hintName">Name that is used to identify the <paramref name="node"/>.</param>
	/// <exception cref="ArgumentNullException"><paramref name="node"/> is <see langword="null"/>.</exception>
	/// <exception cref="ArgumentException"><paramref name="hintName"/> is <see langword="null"/> or empty.</exception>
	void SetTargetNode(SyntaxNode node, string hintName);
}
