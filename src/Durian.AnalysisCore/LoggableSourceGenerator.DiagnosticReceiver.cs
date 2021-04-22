using System;
using Microsoft.CodeAnalysis;

namespace Durian
{
	public abstract partial class LoggableSourceGenerator
	{
		/// <summary>
		/// A <see cref="IDirectDiagnosticReceiver"/> that uses a <see cref="LoggableSourceGenerator"/> to log the received <see cref="Diagnostic"/>s.
		/// </summary>
		public sealed class DiagnosticReceiver : IDirectDiagnosticReceiver
		{
			private readonly DiagnosticBag _bag;
			private SyntaxNode? _node;
			private string? _hintName;

			/// <summary>
			/// <see cref="LoggableSourceGenerator"/> this <see cref="DiagnosticReceiver"/> reports the diagnostics to.
			/// </summary>
			public LoggableSourceGenerator Generator { get; }

			/// <summary>
			/// Initializes a new instance of the <see cref="LoggableSourceGenerator"/> class.
			/// </summary>
			/// <param name="generator"><see cref="LoggableSourceGenerator"/> that will log the received <see cref="Diagnostic"/>s.</param>
			public DiagnosticReceiver(LoggableSourceGenerator generator)
			{
				if (generator is null)
				{
					throw new ArgumentNullException(nameof(generator));
				}

				_bag = new();
				Generator = generator;
			}

			/// <summary>
			/// Sets the <see cref="SyntaxNode"/> that the diagnostics will be reported for.
			/// </summary>
			/// <param name="node"><see cref="SyntaxNode"/> to set.</param>
			/// <param name="hintName">Name of the log file to log to.</param>
			public void SetTargetNode(SyntaxNode? node, string? hintName)
			{
				_node = node;
				_hintName = hintName;
			}

			/// <inheritdoc/>
			public void ReportDiagnostic(DiagnosticDescriptor descriptor, Location? location, params object?[]? messageArgs)
			{
#if ENABLE_GENERATOR_LOGS
				_bag.ReportDiagnostic(descriptor, location, messageArgs);
#endif
			}

			/// <inheritdoc/>
			public void ReportDiagnostic(Diagnostic diagnostic)
			{
#if ENABLE_GENERATOR_LOGS
				_bag.ReportDiagnostic(diagnostic);
#endif
			}

			/// <summary>
			/// Actually writes the diagnostics to the target file.
			/// </summary>
			public void Push()
			{
#if ENABLE_GENERATOR_LOGS
				Generator.LogDiagnostics(_node!, _hintName!, _bag.Diagnostics.ToArray());
#endif
			}
		}
	}
}
