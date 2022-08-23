// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

internal static class Program
{
	private static readonly Regex _categoryRegex = new(@"category\s*:\s*""\s*([\w.]+)\s*""", RegexOptions.Singleline);
	private static readonly Regex _diagnosticAttributeRegex = new(@"\[assembly\s*:\s*DiagnosticFiles\s*\(\s*(.*?)\]", RegexOptions.Singleline);
	private static readonly Regex _diagnosticAttributeValueRegex = new(@"\s*(?:nameof\s*\(\s*(\w+)\s*\)|"".*?"")", RegexOptions.Singleline);
	private static readonly Regex _diagnosticRegex = new(@"public\s*static\s*readonly\s*DiagnosticDescriptor\s*\w+\s*=\s*new\s*\w*\s*\(.*?\)\s*;", RegexOptions.Singleline);
	private static readonly Regex _idRegex = new(@"id\s*:\s*""\s*(\w+)\s*""", RegexOptions.Singleline);
	private static readonly Regex _packageRegex = new(@"DurianPackage\s*\.\s*(\w+)\s*,", RegexOptions.Singleline);
	private static readonly Regex _severityRegex = new(@"defaultSeverity\s*:\s*DiagnosticSeverity\s*.\s*(\w+)", RegexOptions.Singleline);
	private static readonly Regex _titleRegex = new(@"title\s*:\s*""\s*(.*?)\s*""", RegexOptions.Singleline);

	internal static void Main(string[] args)
	{
		for (int i = 0; i < args.Length; i++)
		{
			string configFile = args[i];
			HandleConfigFile(configFile);
		}
	}

	private static string[] GetDiagnosticFiles(string content)
	{
		string attribute = _diagnosticAttributeRegex.Match(content).Groups[1].ToString();

		if (string.IsNullOrWhiteSpace(attribute))
		{
			return Array.Empty<string>();
		}

		MatchCollection matches = _diagnosticAttributeValueRegex.Matches(attribute);

		if (matches.Count == 0)
		{
			return Array.Empty<string>();
		}

		List<string> files = new(matches.Count);

		for (int i = 0; i < matches.Count; i++)
		{
			Match match = matches[i];
			string value = match.Groups[1].ToString();

			if (!string.IsNullOrWhiteSpace(value))
			{
				value = value.Trim();

				if (!value.EndsWith(".cs"))
				{
					value += ".cs";
				}

				files.Add(value);
			}
		}

		return files.ToArray();
	}

	private static string? GetPackageName(string content)
	{
		Match match = _packageRegex.Match(content);
		string moduleName = match.Groups[1].ToString();

		if (!string.IsNullOrWhiteSpace(moduleName))
		{
			return moduleName;
		}

		return null;
	}

	private static void HandleConfigFile(string configFile)
	{
		string? dir = Path.GetDirectoryName(configFile);

		if (dir is null)
		{
			return;
		}

		string content = File.ReadAllText(configFile);
		string? moduleName = GetPackageName(content);

		if (moduleName is null)
		{
			return;
		}

		string[] files = GetDiagnosticFiles(content);

		if (files.Length == 0)
		{
			return;
		}

		HandleDiagnosticFiles(files, dir, moduleName);
	}

	private static void HandleDiagnosticFiles(string[] files, string currentDirectory, string moduleName)
	{
		StringBuilder builder = new StringBuilder()
			.AppendLine("## Release 1.0.0")
			.AppendLine()
			.AppendLine("### New Rules")
			.AppendLine("Rule ID | Category | Severity | Notes")
			.AppendLine("--------|----------|----------|-----------------------------------------");

		foreach (string file in files)
		{
			MatchCollection matches = _diagnosticRegex.Matches(File.ReadAllText(currentDirectory + @"\" + file));

			if (matches.Count == 0)
			{
				continue;
			}

			WriteMatchData(matches, moduleName, builder);
		}

		File.WriteAllText(currentDirectory + @"\AnalyzerReleases.Shipped.md", builder.ToString(), Encoding.UTF8);
	}

	private static DiagnosticData? RetrieveDiagnosticData(string match)
	{
		Match id = _idRegex.Match(match);
		Match category = _categoryRegex.Match(match);
		Match title = _titleRegex.Match(match);
		Match severity = _severityRegex.Match(match);

		return new DiagnosticData(id.Groups[1].ToString(), title.Groups[1].ToString(), category.Groups[1].ToString(), severity.Groups[1].ToString());
	}

	private static void WriteMatchData(MatchCollection matches, string moduleName, StringBuilder builder)
	{
		foreach (Match match in matches.Cast<Match>())
		{
			DiagnosticData? data = RetrieveDiagnosticData(match.ToString());

			if (!data.HasValue)
			{
				continue;
			}

			builder
				.Append(data.Value.Id)
				.Append(" | ")
				.Append(data.Value.Category)
				.Append(" | ")
				.Append(data.Value.Severity)
				.Append(" | ")
				.AppendLine($"{data.Value.Title}. [[DOC](https://github.com/piotrstenke/Durian/tree/master/docs/{moduleName}/{data.Value.Id}.md)]");
		}
	}
}
