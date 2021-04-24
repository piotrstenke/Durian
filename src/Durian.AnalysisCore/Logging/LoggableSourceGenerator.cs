#if ENABLE_GENERATOR_LOGS
using System.IO;
using System.Linq;
using System.Text;
#endif
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.CodeAnalysis;

namespace Durian.Logging
{
	/// <summary>
	/// An <see cref="ISourceGenerator"/> that can create log files.
	/// </summary>
	[DebuggerDisplay("{GetGeneratorName()}, {GetVersion()}")]
	public abstract partial class LoggableSourceGenerator : ISourceGenerator
	{
		/// <inheritdoc cref="GeneratorLoggingConfiguration"/>
		public GeneratorLoggingConfiguration LoggingConfiguration { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="LoggableSourceGenerator"/> class.
		/// </summary>
		protected LoggableSourceGenerator(bool checkForConfigurationAttribute)
		{
#if ENABLE_GENERATOR_LOGS
			LoggingConfiguration = checkForConfigurationAttribute ? GeneratorLoggingConfiguration.CreateConfigurationForGenerator(this) : GeneratorLoggingConfiguration.Default;
#else
			LoggingConfiguration = GeneratorLoggingConfiguration.Default;
#endif
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="LoggableSourceGenerator"/> class.
		/// </summary>
		/// <param name="configuration">Determines how the source generator should behave when logging information. If <see langword="null"/>, <see cref="GeneratorLoggingConfiguration.Default"/> is used instead.</param>
		protected LoggableSourceGenerator(GeneratorLoggingConfiguration? configuration)
		{
			LoggingConfiguration = configuration ?? GeneratorLoggingConfiguration.Default;
		}

		/// <inheritdoc/>
		public abstract void Initialize(GeneratorInitializationContext context);

		/// <inheritdoc/>
		public abstract void Execute(in GeneratorExecutionContext context);

		void ISourceGenerator.Execute(GeneratorExecutionContext context)
		{
			Execute(in context);
		}

		/// <summary>
		/// Returns version of this <see cref="IDurianSourceGenerator"/>.
		/// </summary>
		protected virtual string GetVersion()
		{
			return "1.0.0";
		}

		/// <summary>
		/// Returns name of this <see cref="IDurianSourceGenerator"/>.
		/// </summary>
		protected virtual string GetGeneratorName()
		{
			return nameof(LoggableSourceGenerator);
		}

#if !ENABLE_GENERATOR_LOGS
#pragma warning disable RCS1163 // Unused parameter.
#pragma warning disable CA1822 // Mark members as static
#endif

		/// <summary>
		/// Logs an <see cref="Exception"/>.
		/// </summary>
		/// <param name="exception"></param>
		protected void LogException(Exception exception)
		{
#if ENABLE_GENERATOR_LOGS
			if (GeneratorLoggingConfiguration.IsEnabled && LoggingConfiguration.EnableLogging && LoggingConfiguration.SupportedLogs.HasFlag(GeneratorLogs.Exception) && exception is not null)
			{
				LogException_Internal(exception);
			}
#endif
		}

		/// <summary>
		/// Logs an input and output <see cref="SyntaxNode"/>.
		/// </summary>
		/// <param name="input">Input <see cref="SyntaxNode"/>.</param>
		/// <param name="output">Output <see cref="SyntaxNode"/>.</param>
		/// <param name="hintName">Name of the log file to log to.</param>
		protected void LogNode(SyntaxNode input, SyntaxNode output, string hintName)
		{
#if ENABLE_GENERATOR_LOGS
			if (GeneratorLoggingConfiguration.IsEnabled && LoggingConfiguration.EnableLogging && LoggingConfiguration.SupportedLogs.HasFlag(GeneratorLogs.Node) && !(input is null && output is null))
			{
				LogNode_Internal(input!, output, hintName);
			}
#endif
		}

		/// <summary>
		/// Logs an input <see cref="SyntaxNode"/> and <see cref="Diagnostic"/>s that were created for that node.
		/// </summary>
		/// <param name="node"><see cref="SyntaxNode"/> the diagnostics were created for.</param>
		/// <param name="hintName">Name of the log file to log to.</param>
		/// <param name="diagnostics">A collection of <see cref="Diagnostic"/>s that were created for this <paramref name="node"/>.</param>
		protected void LogDiagnostics(SyntaxNode node, string hintName, IEnumerable<Diagnostic> diagnostics)
		{
#if ENABLE_GENERATOR_LOGS
			LogDiagnostics(node, hintName, diagnostics?.ToArray()!);
#endif
		}

		/// <summary>
		/// Logs an input <see cref="SyntaxNode"/> and <see cref="Diagnostic"/>s that were created for that node.
		/// </summary>
		/// <param name="node"><see cref="SyntaxNode"/> the diagnostics were created for.</param>
		/// <param name="hintName">Name of the log file to log to.</param>
		/// <param name="diagnostics">An array of <see cref="Diagnostic"/>s that were created for this <paramref name="node"/>.</param>
		protected void LogDiagnostics(SyntaxNode node, string hintName, params Diagnostic[] diagnostics)
		{
#if ENABLE_GENERATOR_LOGS
			if (GeneratorLoggingConfiguration.IsEnabled && LoggingConfiguration.EnableLogging && LoggingConfiguration.SupportedLogs.HasFlag(GeneratorLogs.Diagnostics) && diagnostics is not null && diagnostics.Length > 0)
			{
				LogDiagnostics_Internal(node, hintName, diagnostics);
			}
#endif
		}

#if !ENABLE_GENERATOR_LOGS
#pragma warning restore CA1822 // Mark members as static
#pragma warning restore RCS1163 // Unused parameter.
#endif

#if ENABLE_GENERATOR_LOGS
		internal void LogException_Internal(Exception exception)
		{
			Directory.CreateDirectory(LoggingConfiguration.LogDirectory);
			File.AppendAllText(LoggingConfiguration.LogDirectory + "/exception.log", exception.ToString());
		}

		internal void LogNode_Internal(SyntaxNode input, SyntaxNode output, string hintName)
		{
			StringBuilder sb = new();

			if (input is not null)
			{
				sb.AppendLine("input::");
				sb.AppendLine();

				sb.AppendLine(input.ToFullString());
				sb.AppendLine();

				for (int i = 0; i < 100; i++)
				{
					sb.Append('-');
				}

				sb.AppendLine();
			}

			if (output is not null)
			{
				sb.AppendLine("output::");
				sb.AppendLine();
				sb.AppendLine(output.ToFullString());
			}

			string name = string.IsNullOrWhiteSpace(hintName) ? "generated" : hintName;
			string path = LoggingConfiguration.LogDirectory + "/.generated";
			Directory.CreateDirectory(LoggingConfiguration.LogDirectory);
			Directory.CreateDirectory(path);
			File.WriteAllText(path + $"/{name}.log", sb.ToString());
		}

		internal void LogDiagnostics_Internal(SyntaxNode node, string hintName, Diagnostic[] diagnostics)
		{
			StringBuilder sb = new();

			if (node is not null)
			{
				sb.AppendLine("input::");
				sb.AppendLine();

				sb.AppendLine(node.ToFullString());
				sb.AppendLine();

				for (int i = 0; i < 100; i++)
				{
					sb.Append('-');
				}

				sb.AppendLine();
			}

			sb.AppendLine("diagnostics::");
			sb.AppendLine();

			foreach (Diagnostic diagnostic in diagnostics)
			{
				sb.AppendLine(diagnostic.ToString());
			}

			sb.AppendLine();

			string name = string.IsNullOrWhiteSpace(hintName) ? "generated" : hintName;
			string path = LoggingConfiguration.LogDirectory + "/.diag";
			Directory.CreateDirectory(LoggingConfiguration.LogDirectory);
			Directory.CreateDirectory(path);
			File.WriteAllText(path + $"/{name}.log", sb.ToString());
		}
#endif
	}
}
