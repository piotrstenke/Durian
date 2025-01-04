using Microsoft.CodeAnalysis;

namespace Durian.Analysis.Logging
{
	/// <summary>
	/// Specifies whether a <see cref="IDiagnosticReceiver"/> should log the <see cref="Diagnostic"/>s, report them or both or neither.
	/// </summary>
	public enum ReportDiagnosticTarget
	{
		/// <summary>
		/// Hides all reported <see cref="Diagnostic"/>s.
		/// </summary>
		None = 0,

		/// <summary>
		/// Reports <see cref="Diagnostic"/>s only to the <see cref="IGeneratorLogHandler"/>.
		/// </summary>
		Log = 1,

		/// <summary>
		/// Reports <see cref="Diagnostic"/>s only to a context struct.
		/// </summary>
		Report = 2,

		/// <summary>
		/// Reports to both <see cref="IGeneratorLogHandler"/> and a context struct.
		/// </summary>
		Both = 3
	}
}
