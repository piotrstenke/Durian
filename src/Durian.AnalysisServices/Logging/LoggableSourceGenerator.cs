// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;

namespace Durian.Analysis.Logging
{
	/// <inheritdoc cref="ILoggableSourceGenerator"/>
	public abstract class LoggableSourceGenerator : ILoggableSourceGenerator
	{
		/// <inheritdoc/>
		public GeneratorLoggingConfiguration LoggingConfiguration { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="LoggableSourceGenerator"/> class.
		/// </summary>
		protected LoggableSourceGenerator() : this(null)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="LoggableSourceGenerator"/> class.
		/// </summary>
		/// <param name="context">Configures how this <see cref="LoggableSourceGenerator"/> is initialized.</param>
		protected LoggableSourceGenerator(in LoggableGeneratorConstructionContext context)
		{
			LoggingConfiguration = context.CheckForConfigurationAttribute ? GeneratorLoggingConfiguration.CreateConfigurationForGenerator(this) : GeneratorLoggingConfiguration.Default;

			if (!context.EnableLoggingIfSupported)
			{
				LoggingConfiguration.EnableLogging = false;
			}
			else if (GeneratorLoggingConfiguration.IsEnabled)
			{
				LoggingConfiguration.EnableLogging = true;
			}

			if (!context.EnableDiagnosticsIfSupported)
			{
				LoggingConfiguration.EnableDiagnostics = false;
			}
			else if (LoggingConfiguration.SupportsDiagnostics)
			{
				LoggingConfiguration.EnableDiagnostics = true;
			}

			if (context.EnableExceptions)
			{
				LoggingConfiguration.EnableExceptions = true;
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="LoggableSourceGenerator"/> class.
		/// </summary>
		/// <param name="configuration">Determines how the source generator should behave when logging information. If <see langword="null"/>, <see cref="GeneratorLoggingConfiguration.Default"/> is used instead.</param>
		protected LoggableSourceGenerator(GeneratorLoggingConfiguration? configuration)
		{
			LoggingConfiguration = configuration ?? GeneratorLoggingConfiguration.Default;
		}

		/// <summary>
		/// Enables diagnostics if <see cref="GeneratorLoggingConfiguration.SupportedLogs"/> of the <see cref="LoggingConfiguration"/> is <see langword="true"/>.
		/// </summary>
		public void EnableDiagnosticsIfSupported()
		{
			if (LoggingConfiguration.SupportsDiagnostics)
			{
				LoggingConfiguration.EnableDiagnostics = true;
			}
		}

		/// <summary>
		/// Enables generator logging if <see cref="GeneratorLoggingConfiguration.IsEnabled"/> is <see langword="true"/>.
		/// </summary>
		public void EnableLoggingIfSupported()
		{
			if (GeneratorLoggingConfiguration.IsEnabled)
			{
				LoggingConfiguration.EnableLogging = true;
			}
		}

		/// <inheritdoc/>
		public abstract void Execute(in GeneratorExecutionContext context);

		/// <inheritdoc/>
		public abstract void Initialize(GeneratorInitializationContext context);

		/// <inheritdoc/>
		public void LogDiagnostics(SyntaxNode node, string hintName, IEnumerable<Diagnostic> diagnostics)
		{
			if (LoggingConfiguration.EnableLogging && LoggingConfiguration.SupportedLogs.HasFlag(GeneratorLogs.Diagnostics) && diagnostics is not null)
			{
				Diagnostic[] array = diagnostics.ToArray();

				if (array.Length > 0)
				{
					LogDiagnostics_Internal(node, hintName, array);
				}
			}
		}

		/// <inheritdoc/>
		public void LogException(Exception exception)
		{
			if (LoggingConfiguration.EnableLogging && LoggingConfiguration.SupportedLogs.HasFlag(GeneratorLogs.Exception) && exception is not null)
			{
				LogException_Internal(exception);
			}
		}

		/// <inheritdoc/>
		public void LogInputOutput(SyntaxNode input, SyntaxNode output, string hintName)
		{
			if (LoggingConfiguration.EnableLogging && LoggingConfiguration.SupportedLogs.HasFlag(GeneratorLogs.InputOutput) && !(input is null && output is null))
			{
				LogInputOutput_Internal(input!, output, hintName);
			}
		}

		/// <inheritdoc/>
		public void LogNode(SyntaxNode node, string hintName)
		{
			if (LoggingConfiguration.EnableLogging && LoggingConfiguration.SupportedLogs.HasFlag(GeneratorLogs.Node) && node is not null)
			{
				LogNode_Internal(node, hintName);
			}
		}

		void ISourceGenerator.Execute(GeneratorExecutionContext context)
		{
			Execute(in context);
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

		private protected void LogException_Internal(Exception exception)
		{
			Directory.CreateDirectory(LoggingConfiguration.LogDirectory);
			TryAppendAllText(LoggingConfiguration.LogDirectory + "/exception.log", exception.ToString() + "\n\n");
		}

		private protected void LogInputOutput_Internal(SyntaxNode input, SyntaxNode output, string hintName)
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

		private protected void LogNode_Internal(SyntaxNode node, string hintName)
		{
			StringBuilder sb = new();

			AppendSection(sb, "generated");

			sb.AppendLine(node.ToFullString());
			sb.AppendLine();

			WriteToFile(hintName, sb, ".generated");
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

		private void WriteToFile(string hintName, StringBuilder sb, string subDirectory)
		{
			string name = string.IsNullOrWhiteSpace(hintName) ? "generated" : hintName;
			string path = LoggingConfiguration.LogDirectory + $"/{subDirectory}";
			Directory.CreateDirectory(LoggingConfiguration.LogDirectory);
			Directory.CreateDirectory(path);
			TryWriteAllText(path + $"/{name}.log", sb.ToString());
		}
	}
}