#if ENABLE_GENERATOR_LOGS
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
#endif
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.CodeAnalysis;

namespace Durian.Logging
{
	/// <inheritdoc cref="ILoggableSourceGenerator"/>
	[DebuggerDisplay("{GetGeneratorName()}, {GetVersion()}")]
	public abstract partial class LoggableSourceGenerator : ILoggableSourceGenerator
	{
		/// <inheritdoc/>
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

		/// <inheritdoc/>
		public void LogException(Exception exception)
		{
#if ENABLE_GENERATOR_LOGS
			if (GeneratorLoggingConfiguration.IsEnabled && LoggingConfiguration.EnableLogging && LoggingConfiguration.SupportedLogs.HasFlag(GeneratorLogs.Exception) && exception is not null)
			{
				LogException_Internal(exception);
			}
#endif
		}

		/// <inheritdoc/>
		public void LogNode(SyntaxNode node, string hintName)
		{
#if ENABLE_GENERATOR_LOGS
			if (GeneratorLoggingConfiguration.IsEnabled && LoggingConfiguration.EnableLogging && LoggingConfiguration.SupportedLogs.HasFlag(GeneratorLogs.Node) && node is not null)
			{
				LogNode_Internal(node, hintName);
			}
#endif
		}

		/// <inheritdoc/>
		public void LogInputOutput(SyntaxNode input, SyntaxNode output, string hintName)
		{
#if ENABLE_GENERATOR_LOGS
			if (GeneratorLoggingConfiguration.IsEnabled && LoggingConfiguration.EnableLogging && LoggingConfiguration.SupportedLogs.HasFlag(GeneratorLogs.InputOutput) && !(input is null && output is null))
			{
				LogInputOutput_Internal(input!, output, hintName);
			}
#endif
		}

		/// <inheritdoc/>
		public void LogDiagnostics(SyntaxNode node, string hintName, IEnumerable<Diagnostic> diagnostics)
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
		public void LogDiagnostics(SyntaxNode node, string hintName, params Diagnostic[] diagnostics)
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
			TryAppendAllText(LoggingConfiguration.LogDirectory + "/exception.log", exception.ToString() + "\n\n");
		}

		internal void LogNode_Internal(SyntaxNode node, string hintName)
		{
			StringBuilder sb = new();

			AppendSection(sb, "generated");

			sb.AppendLine(node.ToFullString());
			sb.AppendLine();

			WriteToFile(hintName, sb, ".generated");
		}

		internal void LogInputOutput_Internal(SyntaxNode input, SyntaxNode output, string hintName)
		{
			StringBuilder sb = new();

			if (input is not null)
			{
				AppendSection(sb, "input");

				sb.AppendLine(input.ToFullString());
				sb.AppendLine();
			}

			if (output is not null)
			{
				AppendSection(sb, "output");
				sb.AppendLine(output.ToFullString());
			}

			WriteToFile(hintName, sb, ".generated");
		}

		internal void LogDiagnostics_Internal(SyntaxNode node, string hintName, Diagnostic[] diagnostics)
		{
			StringBuilder sb = new();

			if (node is not null)
			{
				AppendSection(sb, "input");
				sb.AppendLine(node.ToFullString());
			}

			AppendSection(sb, "diagnostics");

			foreach (Diagnostic diagnostic in diagnostics)
			{
				sb.AppendLine(diagnostic.ToString());
			}

			sb.AppendLine();

			WriteToFile(hintName, sb, ".diag");
		}

		private void WriteToFile(string hintName, StringBuilder sb, string subDirectory)
		{
			string name = string.IsNullOrWhiteSpace(hintName) ? "generated" : hintName;
			string path = LoggingConfiguration.LogDirectory + $"/{subDirectory}";
			Directory.CreateDirectory(LoggingConfiguration.LogDirectory);
			Directory.CreateDirectory(path);
			TryWriteAllText(path + $"/{name}.log", sb.ToString());
		}

		private static void AppendSection(StringBuilder sb, string sectionName)
		{
			for (int i = 0; i < 100; i++)
			{
				sb.Append('-');
			}

			sb.AppendLine();
			sb.Append(sectionName).AppendLine("::");

			for (int i = 0; i < 100; i++)
			{
				sb.Append('-');
			}

			sb.AppendLine();
			sb.AppendLine();
		}

		private static void TryAppendAllText(string file, string text)
		{
			try
			{
				File.AppendAllText(file, text);
			}
			catch (IOException e) when (e.GetType() == typeof(IOException))
			{
				Thread.Sleep(50);

				int numTries = 1;

				while (true)
				{
					try
					{
						File.AppendAllText(file, text);
					}
					catch (IOException) when (numTries < 10)
					{
						numTries++;
						Thread.Sleep(50);
					}
				}
			}
		}

		private static void TryWriteAllText(string file, string text)
		{
			try
			{
				File.WriteAllText(file, text);
			}
			catch (IOException e) when (e.GetType() == typeof(IOException))
			{
				Thread.Sleep(50);

				int numTries = 1;

				while (true)
				{
					try
					{
						File.WriteAllText(file, text);
					}
					catch (IOException) when (numTries < 20)
					{
						numTries++;
						Thread.Sleep(50);
					}
				}
			}
		}
#endif
	}
}
