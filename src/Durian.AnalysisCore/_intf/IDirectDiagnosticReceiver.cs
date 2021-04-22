using Microsoft.CodeAnalysis;

namespace Durian
{
	/// <summary>
	/// Provides a method for reporting a <see cref="Diagnostic"/>.
	/// </summary>
	public interface IDirectDiagnosticReceiver : IDiagnosticReceiver
	{
		/// <summary>
		/// Reports a <see cref="Diagnostic"/>.
		/// </summary>
		/// <param name="diagnostic"><see cref="Diagnostic"/> to report.</param>
		void ReportDiagnostic(Diagnostic diagnostic);
	}
}
