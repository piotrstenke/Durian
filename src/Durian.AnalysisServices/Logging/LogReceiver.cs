// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Durian.Analysis.Logging
{
	/// <summary>
	/// A <see cref="INodeDiagnosticReceiver"/> that uses a <see cref="IGeneratorLogHandler"/> to log the received <see cref="Diagnostic"/>s.
	/// </summary>
	public class LogReceiver : INodeDiagnosticReceiver
	{
		private readonly DiagnosticBag _bag;

		/// <inheritdoc/>
		public int Count => _bag.Count;

		/// <summary>
		/// Name of the log file to log to.
		/// </summary>
		public string? HintName { get; private set; }

		/// <summary>
		/// <see cref="IGeneratorLogHandler"/> to write the reported <see cref="Diagnostic"/>s to.
		/// </summary>
		public IGeneratorLogHandler LogHandler { get; }

		/// <summary>
		/// Determines what to output when a <see cref="SyntaxNode"/> is being logged.
		/// </summary>
		public NodeOutput NodeOutput { get; set; }

		/// <summary>
		/// Target <see cref="CSharpSyntaxNode"/>.
		/// </summary>
		public CSharpSyntaxNode? Node { get; private set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="LogReceiver"/> class.
		/// </summary>
		public LogReceiver() : this(default(LoggingConfiguration))
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="LogReceiver"/> class.
		/// </summary>
		/// <param name="configuration">Configures how logging is handled.</param>
		public LogReceiver(LoggingConfiguration? configuration)
		{
			LogHandler = new GeneratorLogHandler(configuration);
			_bag = new();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="LogReceiver"/> class.
		/// </summary>
		/// <param name="logHandler"><see cref="IGeneratorLogHandler"/> to write the reported <see cref="Diagnostic"/>s to.</param>
		/// <exception cref="ArgumentNullException"><paramref name="logHandler"/> is <see langword="null"/>.</exception>
		public LogReceiver(IGeneratorLogHandler logHandler)
		{
			if (logHandler is null)
			{
				throw new ArgumentNullException(nameof(logHandler));
			}

			LogHandler = logHandler;
			_bag = new();
		}

		/// <summary>
		/// Removes all <see cref="Diagnostic"/>s that weren't logged using the <see cref="Push"/> method.
		/// </summary>
		public virtual void Clear()
		{
			lock (_bag)
			{
				_bag.Clear();
			}
		}

		/// <summary>
		/// Actually writes the diagnostics to the target file.
		/// </summary>
		/// <exception cref="InvalidOperationException">Cannot push diagnostics when target node or hint name is null.</exception>
		public void Push()
		{
			if (_bag.Count <= 0)
			{
				return;
			}

			Diagnostic[]? diagnostics;
			string hintName;
			CSharpSyntaxNode node;
			NodeOutput nodeOutput;

			lock (_bag)
			{
				if (HintName is null || Node is null)
				{
					throw new InvalidOperationException("Cannot push diagnostics when target node or hint name is null");
				}

				hintName = HintName!;
				node = Node!;
				nodeOutput = NodeOutput;

				if (_bag.Count > 0)
				{
					diagnostics = _bag.GetDiagnostics();
				}
				else
				{
					diagnostics = default;
				}
			}

			if (diagnostics is null)
			{
				return;
			}

			LogHandler.LogDiagnostics(node, hintName, diagnostics, nodeOutput);
			ReportDiagnostics(diagnostics);
			Clear();
		}

		/// <inheritdoc/>
		public virtual void ReportDiagnostic(DiagnosticDescriptor descriptor, Location? location, params object?[]? messageArgs)
		{
			lock (_bag)
			{
				_bag.ReportDiagnostic(descriptor, location, messageArgs);
			}
		}

		/// <inheritdoc/>
		public virtual void ReportDiagnostic(Diagnostic diagnostic)
		{
			lock (_bag)
			{
				_bag.ReportDiagnostic(diagnostic);
			}
		}

		/// <inheritdoc/>
		public virtual void SetTargetNode(CSharpSyntaxNode? node, string? hintName)
		{
			lock (_bag)
			{
				Node = node;
				HintName = hintName;
			}
		}

		/// <summary>
		/// Reports the specified <paramref name="diagnostics"/>.
		/// </summary>
		/// <param name="diagnostics">A collection of <see cref="Diagnostic"/>s to report.</param>
		protected virtual void ReportDiagnostics(Diagnostic[] diagnostics)
		{
			// Do nothing by default.
		}
	}
}
