using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Durian
{
	public static partial class DurianDiagnostics
	{
		/// <summary>
		/// Reports diagnostics using the specified <see cref="DiagnosticDescriptor"/>.
		/// </summary>
		/// <param name="context"><see cref="GeneratorExecutionContext"/> to register the diagnostics to.</param>
		/// <param name="diagnostic">Diagnostics to report.</param>
		/// <param name="location">Location where the error occurred.</param>
		/// <param name="messageArgs">Arguments to use when formatting the diagnostic message.</param>
		/// <exception cref="ArgumentNullException"><paramref name="diagnostic"/> is <c>null</c>.</exception>
		public static void ReportDiagnostic(in GeneratorExecutionContext context, DiagnosticDescriptor diagnostic, Location? location, params object?[]? messageArgs)
		{
			if (diagnostic is null)
			{
				throw new ArgumentNullException(nameof(diagnostic));
			}

			context.ReportDiagnostic(Diagnostic.Create(diagnostic, location, messageArgs));
		}

		/// <summary>
		/// Reports diagnostics using the specified <see cref="DiagnosticDescriptor"/>.
		/// </summary>
		/// <param name="context"><see cref="SymbolAnalysisContext"/> to register the diagnostics to.</param>
		/// <param name="diagnostic">Diagnostics to report.</param>
		/// <param name="location">Location where the error occurred.</param>
		/// <param name="messageArgs">Arguments to use when formatting the diagnostic message.</param>
		/// <exception cref="ArgumentNullException"><paramref name="diagnostic"/> is <c>null</c>.</exception>
		public static void ReportDiagnostic(SymbolAnalysisContext context, DiagnosticDescriptor diagnostic, Location? location, params object?[]? messageArgs)
		{
			if (diagnostic is null)
			{
				throw new ArgumentNullException(nameof(diagnostic));
			}

			context.ReportDiagnostic(Diagnostic.Create(diagnostic, location, messageArgs));
		}

		/// <summary>
		/// Reports diagnostics using the specified <see cref="DiagnosticDescriptor"/>.
		/// </summary>
		/// <param name="context"><see cref="SyntaxNodeAnalysisContext"/> to register the diagnostics to.</param>
		/// <param name="diagnostic">Diagnostics to report.</param>
		/// <param name="location">Location where the error occurred.</param>
		/// <param name="messageArgs">Arguments to use when formatting the diagnostic message.</param>
		/// <exception cref="ArgumentNullException"><paramref name="diagnostic"/> is <c>null</c>.</exception>
		public static void ReportDiagnostic(SyntaxNodeAnalysisContext context, DiagnosticDescriptor diagnostic, Location? location, params object?[]? messageArgs)
		{
			if (diagnostic is null)
			{
				throw new ArgumentNullException(nameof(diagnostic));
			}

			context.ReportDiagnostic(Diagnostic.Create(diagnostic, location, messageArgs));
		}

		/// <summary>
		/// Reports diagnostics using the specified <see cref="DiagnosticDescriptor"/>.
		/// </summary>
		/// <param name="context"><see cref="CompilationAnalysisContext"/> to register the diagnostics to.</param>
		/// <param name="diagnostic">Diagnostics to report.</param>
		/// <param name="location">Location where the error occurred.</param>
		/// <param name="messageArgs">Arguments to use when formatting the diagnostic message.</param>
		/// <exception cref="ArgumentNullException"><paramref name="diagnostic"/> is <c>null</c>.</exception>
		public static void ReportDiagnostic(CompilationAnalysisContext context, DiagnosticDescriptor diagnostic, Location? location, params object?[]? messageArgs)
		{
			if (diagnostic is null)
			{
				throw new ArgumentNullException(nameof(diagnostic));
			}

			context.ReportDiagnostic(Diagnostic.Create(diagnostic, location, messageArgs));
		}

		/// <summary>
		/// Reports diagnostics using the specified <see cref="DiagnosticDescriptor"/>.
		/// </summary>
		/// <param name="context"><see cref="AdditionalFileAnalysisContext"/> to register the diagnostics to.</param>
		/// <param name="diagnostic">Diagnostics to report.</param>
		/// <param name="location">Location where the error occurred.</param>
		/// <param name="messageArgs">Arguments to use when formatting the diagnostic message.</param>
		/// <exception cref="ArgumentNullException"><paramref name="diagnostic"/> is <c>null</c>.</exception>
		public static void ReportDiagnostic(in AdditionalFileAnalysisContext context, DiagnosticDescriptor diagnostic, Location? location, params object?[]? messageArgs)
		{
			if (diagnostic is null)
			{
				throw new ArgumentNullException(nameof(diagnostic));
			}

			context.ReportDiagnostic(Diagnostic.Create(diagnostic, location, messageArgs));
		}

		/// <summary>
		/// Reports diagnostics using the specified <see cref="DiagnosticDescriptor"/>.
		/// </summary>
		/// <param name="context"><see cref="CodeBlockAnalysisContext"/> to register the diagnostics to.</param>
		/// <param name="diagnostic">Diagnostics to report.</param>
		/// <param name="location">Location where the error occurred.</param>
		/// <param name="messageArgs">Arguments to use when formatting the diagnostic message.</param>
		/// <exception cref="ArgumentNullException"><paramref name="diagnostic"/> is <c>null</c>.</exception>
		public static void ReportDiagnostic(CodeBlockAnalysisContext context, DiagnosticDescriptor diagnostic, Location? location, params object?[]? messageArgs)
		{
			if (diagnostic is null)
			{
				throw new ArgumentNullException(nameof(diagnostic));
			}

			context.ReportDiagnostic(Diagnostic.Create(diagnostic, location, messageArgs));
		}

		/// <summary>
		/// Reports diagnostics using the specified <see cref="DiagnosticDescriptor"/>.
		/// </summary>
		/// <param name="context"><see cref="OperationAnalysisContext"/> to register the diagnostics to.</param>
		/// <param name="diagnostic">Diagnostics to report.</param>
		/// <param name="location">Location where the error occurred.</param>
		/// <param name="messageArgs">Arguments to use when formatting the diagnostic message.</param>
		/// <exception cref="ArgumentNullException"><paramref name="diagnostic"/> is <c>null</c>.</exception>
		public static void ReportDiagnostic(OperationAnalysisContext context, DiagnosticDescriptor diagnostic, Location? location, params object?[]? messageArgs)
		{
			if (diagnostic is null)
			{
				throw new ArgumentNullException(nameof(diagnostic));
			}

			context.ReportDiagnostic(Diagnostic.Create(diagnostic, location, messageArgs));
		}

		/// <summary>
		/// Reports diagnostics using the specified <see cref="DiagnosticDescriptor"/>.
		/// </summary>
		/// <param name="context"><see cref="OperationBlockAnalysisContext"/> to register the diagnostics to.</param>
		/// <param name="diagnostic">Diagnostics to report.</param>
		/// <param name="location">Location where the error occurred.</param>
		/// <param name="messageArgs">Arguments to use when formatting the diagnostic message.</param>
		/// <exception cref="ArgumentNullException"><paramref name="diagnostic"/> is <c>null</c>.</exception>
		public static void ReportDiagnostic(OperationBlockAnalysisContext context, DiagnosticDescriptor diagnostic, Location? location, params object?[]? messageArgs)
		{
			if (diagnostic is null)
			{
				throw new ArgumentNullException(nameof(diagnostic));
			}

			context.ReportDiagnostic(Diagnostic.Create(diagnostic, location, messageArgs));
		}

		/// <summary>
		/// Reports diagnostics using the specified <see cref="DiagnosticDescriptor"/>.
		/// </summary>
		/// <param name="context"><see cref="SemanticModelAnalysisContext"/> to register the diagnostics to.</param>
		/// <param name="diagnostic">Diagnostics to report.</param>
		/// <param name="location">Location where the error occurred.</param>
		/// <param name="messageArgs">Arguments to use when formatting the diagnostic message.</param>
		/// <exception cref="ArgumentNullException"><paramref name="diagnostic"/> is <c>null</c>.</exception>
		public static void ReportDiagnostic(SemanticModelAnalysisContext context, DiagnosticDescriptor diagnostic, Location? location, params object?[]? messageArgs)
		{
			if (diagnostic is null)
			{
				throw new ArgumentNullException(nameof(diagnostic));
			}

			context.ReportDiagnostic(Diagnostic.Create(diagnostic, location, messageArgs));
		}

		/// <summary>
		/// Reports diagnostics using the specified <see cref="DiagnosticDescriptor"/>.
		/// </summary>
		/// <param name="context"><see cref="SyntaxTreeAnalysisContext"/> to register the diagnostics to.</param>
		/// <param name="diagnostic">Diagnostics to report.</param>
		/// <param name="location">Location where the error occurred.</param>
		/// <param name="messageArgs">Arguments to use when formatting the diagnostic message.</param>
		/// <exception cref="ArgumentNullException"><paramref name="diagnostic"/> is <c>null</c>.</exception>
		public static void ReportDiagnostic(SyntaxTreeAnalysisContext context, DiagnosticDescriptor diagnostic, Location? location, params object?[]? messageArgs)
		{
			if (diagnostic is null)
			{
				throw new ArgumentNullException(nameof(diagnostic));
			}

			context.ReportDiagnostic(Diagnostic.Create(diagnostic, location, messageArgs));
		}
	}
}
