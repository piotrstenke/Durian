using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace Durian.Analysis.Logging
{
	/// <summary>
	/// <see cref="IGeneratorLogHandler"/> that allows to replace the underlaying <see cref="LoggingConfiguration"/> instance after the object is created.
	/// </summary>
	public sealed class DynamicGeneratorLogHandler : IGeneratorLogHandler
	{
		private GeneratorLogHandler _internalHandler;

		/// <inheritdoc/>
		/// <exception cref="ArgumentNullException">Value cannot be <see langword="null"/>.</exception>
		public LoggingConfiguration LoggingConfiguration
		{
			get
			{
				return _internalHandler.LoggingConfiguration;
			}
			set
			{
				if (value is null)
				{
					throw new ArgumentNullException(nameof(value));
				}

				_internalHandler = new(value);
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DynamicGeneratorLogHandler"/> class.
		/// </summary>
		public DynamicGeneratorLogHandler()
		{
			_internalHandler = new GeneratorLogHandler();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DynamicGeneratorLogHandler"/> class.
		/// </summary>
		/// <param name="configuration">Configures how logging is handled.</param>s
		public DynamicGeneratorLogHandler(LoggingConfiguration? configuration)
		{
			_internalHandler = new(configuration);
		}

		/// <inheritdoc/>
		public void LogDiagnostics(SyntaxNode node, string hintName, IEnumerable<Diagnostic> diagnostics, NodeOutput nodeOutput = NodeOutput.Default)
		{
			_internalHandler.LogDiagnostics(node, hintName, diagnostics, nodeOutput);
		}

		/// <inheritdoc/>
		public void LogException(Exception exception)
		{
			_internalHandler.LogException(exception);
		}

		/// <inheritdoc/>
		public void LogException(Exception exception, string source)
		{
			_internalHandler.LogException(exception, source);
		}

		/// <inheritdoc/>
		public void LogInputOutput(SyntaxNode input, SyntaxNode output, string hintName, NodeOutput nodeOutput = NodeOutput.Default)
		{
			_internalHandler.LogInputOutput(input, output, hintName, nodeOutput);
		}

		/// <inheritdoc/>
		public void LogNode(SyntaxNode node, string hintName, NodeOutput nodeOutput = NodeOutput.Default)
		{
			_internalHandler.LogNode(node, hintName, nodeOutput);
		}
	}
}
