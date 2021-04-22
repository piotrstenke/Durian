#if ENABLE_GENERATOR_LOGS
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
#endif
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.CodeAnalysis;

namespace Durian
{
	/// <summary>
	/// An <see cref="ISourceGenerator"/> that can create log files.
	/// </summary>
	[DebuggerDisplay("{GetGeneratorName()}, {GetVersion()}")]
	public abstract partial class LoggableSourceGenerator : ISourceGenerator
	{
		/// <inheritdoc cref="SourceGeneratorLoggingConfiguration"/>
		public SourceGeneratorLoggingConfiguration LoggingConfiguration { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="LoggableSourceGenerator"/> class.
		/// </summary>
		/// <param name="checkforConfigurationAttribute">If <c>true</c>, the value defined on this class using the <see cref="SourceGeneratorConfigurationAttribute"/> is used as the target <see cref="LoggingConfiguration"/>.</param>
		protected LoggableSourceGenerator(bool checkforConfigurationAttribute)
		{
			LoggingConfiguration = checkforConfigurationAttribute ? GetConfigurationFromAttribute() : SourceGeneratorLoggingConfiguration.Default;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="LoggableSourceGenerator"/> class.
		/// </summary>
		/// <param name="configuration">Determines how the source generator should behave when logging information. If <c>null</c>, <see cref="SourceGeneratorLoggingConfiguration.Default"/> is used instead.</param>
		protected LoggableSourceGenerator(SourceGeneratorLoggingConfiguration? configuration)
		{
			LoggingConfiguration = configuration ?? SourceGeneratorLoggingConfiguration.Default;
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

		/// <summary>
		/// Logs an <see cref="Exception"/>.
		/// </summary>
		/// <param name="exception"></param>
		protected void LogException(Exception exception)
		{
#if ENABLE_GENERATOR_LOGS
			if (LoggingConfiguration.EnableLogging && LoggingConfiguration.SupportedLogs.HasFlag(GeneratorLogs.Exception) && exception is not null)
			{
				Directory.CreateDirectory(LoggingConfiguration.LogDirectory);
				File.AppendAllText(LoggingConfiguration.LogDirectory + "/exception.log", exception.ToString());
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
			if (LoggingConfiguration.EnableLogging && LoggingConfiguration.SupportedLogs.HasFlag(GeneratorLogs.Node) && !(input is null && output is null))
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
			if (LoggingConfiguration.EnableLogging && LoggingConfiguration.SupportedLogs.HasFlag(GeneratorLogs.Diagnostics) && diagnostics is not null && diagnostics.Length > 0)
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

#if ENABLE_GENERATOR_LOGS
		private SourceGeneratorLoggingConfiguration GetConfigurationFromAttribute()
		{
			return GetType().GetCustomAttribute<SourceGeneratorConfigurationAttribute>()?.GetConfiguration() ?? SourceGeneratorLoggingConfiguration.Default;
		}
#endif
	}
}
