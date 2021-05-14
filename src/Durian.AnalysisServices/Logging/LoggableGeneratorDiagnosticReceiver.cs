using System;
using Microsoft.CodeAnalysis;

namespace Durian.Generator.Logging
{
	/// <summary>
	/// A <see cref="IDirectDiagnosticReceiver"/> that uses a <see cref="LoggableSourceGenerator"/> to log the received <see cref="Diagnostic"/>s.
	/// </summary>
	public sealed class LoggableGeneratorDiagnosticReceiver : IDirectDiagnosticReceiver
	{
		private readonly DiagnosticBag _bag;

		/// <summary>
		/// <see cref="LoggableSourceGenerator"/> this <see cref="LoggableGeneratorDiagnosticReceiver"/> reports the diagnostics to.
		/// </summary>
		public LoggableSourceGenerator Generator { get; }

		/// <summary>
		/// Target <see cref="SyntaxNode"/>.
		/// </summary>
		public SyntaxNode? Node { get; private set; }

		/// <summary>
		/// Name of the log file to log to.
		/// </summary>
		public string? HintName { get; private set; }

		/// <summary>
		/// Returns the number of <see cref="Diagnostic"/> that weren't pushed using the <see cref="Push"/> method yet.
		/// </summary>
		public int Count => _bag.Count;

		/// <summary>
		/// Initializes a new instance of the <see cref="LoggableSourceGenerator"/> class.
		/// </summary>
		/// <param name="generator"><see cref="LoggableSourceGenerator"/> that will log the received <see cref="Diagnostic"/>s.</param>
		public LoggableGeneratorDiagnosticReceiver(LoggableSourceGenerator generator)
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
			Node = node;
			HintName = hintName;
		}

		/// <inheritdoc/>
		public void ReportDiagnostic(DiagnosticDescriptor descriptor, Location? location, params object?[]? messageArgs)
		{
			_bag.ReportDiagnostic(descriptor, location, messageArgs);
		}

		/// <inheritdoc/>
		public void ReportDiagnostic(Diagnostic diagnostic)
		{
			_bag.ReportDiagnostic(diagnostic);
		}

		/// <summary>
		/// Actually writes the diagnostics to the target file.
		/// </summary>
		public void Push()
		{
			PushWithoutClear();
			Clear();
		}

		/// <summary>
		/// Actually writes the diagnostics to the target file without clearing the bag afterwards.
		/// </summary>
		public void PushWithoutClear()
		{
			if (_bag.Count > 0)
			{
				Generator.LogDiagnostics(Node!, HintName!, _bag.GetDiagnostics());
			}
		}

		/// <summary>
		/// Removes all <see cref="Diagnostic"/>s that weren't logged using the <see cref="Push"/> method.
		/// </summary>
		public void Clear()
		{
			_bag.Clear();
		}
	}
}
