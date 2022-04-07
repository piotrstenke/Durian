// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace Durian.Analysis.Logging
{
	/// <inheritdoc cref="IGeneratorLogHandler"/>
	public class GeneratorLogHandler : IGeneratorLogHandler
	{
		/// <inheritdoc/>
		public LoggingConfiguration LoggingConfiguration { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="GeneratorLogHandler"/> class.
		/// </summary>
		public GeneratorLogHandler() : this(default)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GeneratorLogHandler"/> class.
		/// </summary>
		/// <param name="configuration">Configures how logging is handled.</param>
		public GeneratorLogHandler(LoggingConfiguration? configuration)
		{
			LoggingConfiguration = configuration ?? LoggingConfiguration.Default;
		}

		/// <inheritdoc/>
		public void LogDiagnostics(SyntaxNode node, string hintName, IEnumerable<Diagnostic> diagnostics, NodeOutput nodeOutput = default)
		{
			if (LoggingConfiguration.EnableLogging && LoggingConfiguration.SupportedLogs.HasFlag(GeneratorLogs.Diagnostics) && diagnostics is not null)
			{
				Diagnostic[] array = diagnostics.ToArray();

				if (array.Length > 0)
				{
					LogDiagnostics_Internal(node, hintName, array, nodeOutput);
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
		public void LogException(Exception exception, string source)
		{
			if (LoggingConfiguration.EnableLogging && LoggingConfiguration.SupportedLogs.HasFlag(GeneratorLogs.Exception) && exception is not null)
			{
				LogException_Internal(exception, source);
			}
		}

		/// <inheritdoc/>
		public void LogInputOutput(SyntaxNode input, SyntaxNode output, string hintName, NodeOutput nodeOutput = default)
		{
			if (LoggingConfiguration.EnableLogging && LoggingConfiguration.SupportedLogs.HasFlag(GeneratorLogs.InputOutput) && !(input is null && output is null))
			{
				LogInputOutput_Internal(input!, output, hintName, nodeOutput);
			}
		}

		/// <inheritdoc/>
		public void LogNode(SyntaxNode node, string hintName, NodeOutput nodeOutput = default)
		{
			if (LoggingConfiguration.EnableLogging && LoggingConfiguration.SupportedLogs.HasFlag(GeneratorLogs.Node) && node is not null)
			{
				LogNode_Internal(node, hintName, nodeOutput);
			}
		}

		internal void LogDiagnostics_Internal(SyntaxNode node, string hintName, Diagnostic[] diagnostics, NodeOutput nodeOutput)
		{
			StringBuilder sb = new();

			if (node is not null)
			{
				AppendSection(sb, "input");
				sb.AppendLine(GetNodeOutput(node, nodeOutput));
			}

			AppendSection(sb, "diagnostics");

			foreach (Diagnostic diagnostic in diagnostics)
			{
				sb.AppendLine(diagnostic.ToString());
			}

			sb.AppendLine();

			WriteToFile(hintName, sb, ".diag");
		}

		internal void LogException_Internal(Exception exception)
		{
			Directory.CreateDirectory(LoggingConfiguration.LogDirectory);
			TryAppendAllText(LoggingConfiguration.LogDirectory + "/exception.log", exception.ToString() + "\n\n");
		}

		internal void LogException_Internal(Exception exception, string source)
		{
			Directory.CreateDirectory(LoggingConfiguration.LogDirectory);
			TryAppendAllText(LoggingConfiguration.LogDirectory + "/exception.log", source + "::\n\n" + exception.ToString() + "\n\n");
		}

		internal void LogInputOutput_Internal(SyntaxNode input, SyntaxNode output, string hintName, NodeOutput nodeOutput)
		{
			StringBuilder sb = new();

			if (input is not null)
			{
				AppendSection(sb, "input");

				sb.AppendLine(GetNodeOutput(input, nodeOutput));
				sb.AppendLine();
			}

			if (output is not null)
			{
				AppendSection(sb, "output");
				sb.AppendLine(GetNodeOutput(output, nodeOutput));
			}

			WriteToFile(hintName, sb, ".generated");
		}

		internal void LogNode_Internal(SyntaxNode node, string hintName, NodeOutput nodeOutput)
		{
			StringBuilder sb = new();

			AppendSection(sb, "generated");

			sb.AppendLine(GetNodeOutput(node, nodeOutput));
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

		private string GetNodeOutput(SyntaxNode node, NodeOutput nodeOutput)
		{
			return nodeOutput switch
			{
				NodeOutput.Node => node.ToFullString(),
				NodeOutput.Containing => node.Parent?.ToFullString() ?? node.ToFullString(),
				NodeOutput.SyntaxTree => node.SyntaxTree.ToString(),
				_ => GetNodeOutput(node, LoggingConfiguration.DefaultNodeOutput),
			};
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
