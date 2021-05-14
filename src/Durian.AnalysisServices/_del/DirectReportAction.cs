using Microsoft.CodeAnalysis;

namespace Durian.Generator
{
	/// <summary>
	/// A delegate that reports a <see cref="Diagnostic"/>.
	/// </summary>
	/// <param name="diagnostic"><see cref="Diagnostic"/> to report.</param>
	public delegate void DirectReportAction(Diagnostic diagnostic);
}
