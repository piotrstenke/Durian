using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Collections.Generic;

internal class Program
{
	private static readonly Regex _diagnosticRegex = new(@"public\s*static\s*readonly\s*DiagnosticDescriptor\s*\w+\s*=\s*new\s*\w*\s*\(.*?\)\s*;", RegexOptions.Singleline);
	private static readonly Regex _idRegex = new(@"id\s*:\s*""\s*(\w+)\s*""", RegexOptions.Singleline);
	private static readonly Regex _titleRegex = new(@"title\s*:\s*""\s*(.*?)\s*""", RegexOptions.Singleline);
	private static readonly Regex _severityRegex = new(@"defaultSeverity\s*:\s*DiagnosticSeverity\s*.\s*(\w+)", RegexOptions.Singleline);
	private static readonly Regex _locationRegex = new(@"\[\s*WithoutLocation\s*\]");
	private static readonly Regex _definitionAttributeRegex = new(@"\[\s*assembly\s*:\s*ModuleDefinition\s*\(\s*DurianModule\s*\.\s*(\w+)\s*,\s*(.*?)\s?,\s*""(.*?)""\s*(?:,\s*(\d+)\s*)?\)\s*\]", RegexOptions.Singleline);
	private static readonly Regex _diagnosticFilesAttributeRegex = new(@"\[assembly\s*:\s*DiagnosticFiles\s*\(\s*(.*?)\]", RegexOptions.Singleline);
	private static readonly Regex _includedTypesAttributeRegex = new(@"\[assembly\s*:\s*IncludeTypes\s*\(\s*(.*?)\]", RegexOptions.Singleline);
	private static readonly Regex _includedDiagnosticsAttributeRegex = new(@"\[assembly\s*:\s*IncludeDiagnostics\s*\(\s*(.*?)\]", RegexOptions.Singleline);
	private static readonly Regex _attributeValueRegex = new(@"\s*(?:nameof\s*\(\s*(\w+)\s*\)|""(.*?)"")", RegexOptions.Singleline);
	private static readonly Regex _namespaceRegex = new(@"namespace\s*([\w.]+)", RegexOptions.Singleline);

	private static void Main(string[] args)
	{
		if (args.Length < 2)
		{
			return;
		}

		string dir = args[0];
		string output = args[1];

		HandleConfigFiles(dir, output);
	}

	private static void HandleConfigFiles(string directory, string outputPath)
	{
		StringBuilder builder = InitializeStringBuilder();
		string[] coreFiles = GetFilesInCoreProject(directory);

		List<DiagnosticData> diagnostics = new(64);

		foreach (string dir in Directory.GetDirectories(directory))
		{
			string configFile = dir + @"\" + "_Configuration.cs";

			if(!File.Exists(configFile))
			{
				continue;
			}

			string content = File.ReadAllText(configFile);
			Configuration? configuration = GetConfiguration(content);

			if(!configuration.HasValue)
			{
				continue;
			}

			Configuration config = configuration.Value;

			builder.AppendLine();
			BeginIdentity(in config, builder);
			HandleDiagnosticFiles(in config, dir, diagnostics, builder);
			FindAndWriteIncludedTypes(coreFiles, config.IncludedTypes, builder);

			EndIdentity(builder);
		}

		EndClass(builder);

		File.WriteAllText(outputPath, builder.ToString(), Encoding.UTF8);
	}

	private static string[] GetFilesInCoreProject(string directory)
	{
		string dir = directory + @"\" + "Durian.Core";
		return Directory.GetFiles(dir, "*.cs", SearchOption.AllDirectories);
	}

	private static Configuration? GetConfiguration(string content)
	{
		Match match = _definitionAttributeRegex.Match(content);

		if(!match.Success || match.Groups.Count < 4)
		{
			return null;
		}

		string moduleName = GetDefinitionValue(match, 1);
		string moduleType = GetDefinitionValue(match, 2);
		string version = GetDefinitionValue(match, 3);
		string? id = GetId(match);
		string[] files = GetDiagnosticFiles(content);
		string[] includedTypes = GetIncludedTypes(content);
		string[] includedDiagnostics = GetIncludedDiagnostics(content);

		return new Configuration(moduleName, moduleType, version, id, includedDiagnostics, includedTypes, files);
	}

	private static void HandleDiagnosticFiles(in Configuration configuration, string currentDirectory, List<DiagnosticData> diagnostics, StringBuilder builder)
	{
		if(configuration.DiagnosticFiles.Length == 0 && configuration.ExternalDiagnostics.Length == 0)
		{
			return;
		}

		WriteDocPath(configuration.ModuleName, builder);

		builder.Append(
@"			diagnostics: new DiagnosticData[]
			{
");

		foreach (string file in configuration.DiagnosticFiles)
		{
			MatchCollection matches = _diagnosticRegex.Matches(File.ReadAllText(currentDirectory + @"\" + file));

			if (matches.Count == 0)
			{
				continue;
			}

			DiagnosticData[] datas = new DiagnosticData[matches.Count];

			for (int i = 0; i < datas.Length; i++)
			{
				datas[i] = GetDiagnosticData(matches[i].ToString(), configuration.ModuleName);
			}

			WriteDiagnostics(datas, builder);

			diagnostics.AddRange(datas);
		}

		FindAndWriteExternalDiagnostics(diagnostics, in configuration, builder);
	}

	private static DiagnosticData GetDiagnosticData(string match, string moduleName)
	{
		Match id = _idRegex.Match(match);
		Match title = _titleRegex.Match(match);
		Match severity = _severityRegex.Match(match);
		bool hasLocation = !_locationRegex.IsMatch(match);
		bool fatal = severity.Groups[1].ToString().Trim() == "Error";

		return new DiagnosticData(title.Groups[1].ToString(), id.Groups[1].ToString(), moduleName, hasLocation, fatal);
	}

	private static string GetDefinitionValue(Match match, int group)
	{
		return match.Groups[group].ToString().Trim();
	}

	private static string? GetId(Match match)
	{
		if(match.Groups.Count < 5)
		{
			return null;
		}

		string value = GetDefinitionValue(match, 4);

		if(string.IsNullOrWhiteSpace(value))
		{
			return null;
		}

		return value;
	}

	private static string[] GetDiagnosticFiles(string content)
	{
		MatchCollection? matches = GetMatches(content, _diagnosticFilesAttributeRegex);

		if(matches is null)
		{
			return Array.Empty<string>();
		}

		List<string> files = new(matches.Count);

		foreach(Match match in matches)
		{
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

	private static string[] GetIncludedDiagnostics(string content)
	{
		return GetAttributeArguments(content, _includedDiagnosticsAttributeRegex);
	}

	private static string[] GetIncludedTypes(string content)
	{
		return GetAttributeArguments(content, _includedTypesAttributeRegex);
	}

	private static MatchCollection? GetMatches(string content, Regex attributeRegex)
	{
		string attribute = attributeRegex.Match(content).Groups[1].ToString();

		if (string.IsNullOrWhiteSpace(attribute))
		{
			return null;
		}

		MatchCollection matches = _attributeValueRegex.Matches(attribute);

		if (matches.Count == 0)
		{
			return null;
		}

		return matches;
	}

	private static string[] GetAttributeArguments(string content, Regex attributeRegex)
	{
		MatchCollection? matches = GetMatches(content, attributeRegex);

		if (matches is null)
		{
			return Array.Empty<string>();
		}

		List<string> list = new(matches.Count);

		foreach (Match match in matches)
		{
			string value = match.Groups[1].ToString().Trim();

			if(string.IsNullOrWhiteSpace(value))
			{
				value = match.Groups[2].ToString().Trim();

				if(string.IsNullOrWhiteSpace(value))
				{
					continue;
				}
			}

			list.Add(value);
		}

		return list.ToArray();
	}

	private static StringBuilder InitializeStringBuilder()
	{
		StringBuilder builder = new();
		builder.Append(
$@"//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the GenerateModuleRepository project.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Durian.Info
{{
	/// <summary>
	/// Factory class of <see cref=""ModuleIdentity""/> for all available Durian modules.
	/// </summary>
	public static class ModuleRepository
	{{");

		return builder;
	}

	private static void EndClass(StringBuilder builder)
	{
		builder.AppendLine("\t}");
		builder.AppendLine("}");
	}

	private static void BeginIdentity(in Configuration configuration, StringBuilder builder)
	{
		builder.Append(
$@"		/// <summary>
		/// Returns a <see cref=""ModuleIdentity""/> for the <c>Durian.{configuration.ModuleName}</c> module.
		/// </summary>
		public static ModuleIdentity {configuration.ModuleName} => new(
			name: ""Durian.{configuration.ModuleName}"",
			version: ""{configuration.Version}"",
			module: DurianModule.{configuration.ModuleName},
			type: {configuration.ModuleType},
			id: {configuration.Id}");

		if(configuration.IncludedTypes.Length > 0 || configuration.ExternalDiagnostics.Length > 0 || configuration.DiagnosticFiles.Length > 0)
		{
			builder.Append(',');
		}

		builder.AppendLine();
	}

	private static void WriteDocPath(string moduleName, StringBuilder builder)
	{
		builder.AppendLine($"\t\t\tdocPath: @\"docs\\{moduleName}\",");
	}

	private static void WriteDiagnostics(DiagnosticData[] diagnostics, StringBuilder builder)
	{
		for (int i = 0; i < diagnostics.Length; i++)
		{
			ref readonly DiagnosticData data = ref diagnostics[i];
			BeginDiagnostic(in data, builder);
			builder.AppendLine("),");
		}
	}

	private static void FindAndWriteExternalDiagnostics(List<DiagnosticData> diagnostics, in Configuration configuration, StringBuilder builder)
	{
		if(configuration.ExternalDiagnostics.Length > 0)
		{
			builder.AppendLine();
			builder.AppendLine("\t\t\t\t// External diagnostics");
			builder.AppendLine();

			for (int i = 0; i < diagnostics.Count; i++)
			{
				DiagnosticData data = diagnostics[i];

				for (int j = 0; j < configuration.ExternalDiagnostics.Length; j++)
				{
					if (data.Id == configuration.ExternalDiagnostics[j])
					{
						BeginDiagnostic(in data, builder);
						builder.AppendLine($", {data.Module}),");
					}
				}
			}
		}

		builder.Append("\t\t\t}");

		if(configuration.IncludedTypes.Length > 0)
		{
			builder.Append(',');
		}

		builder.AppendLine();
	}

	private static void BeginDiagnostic(in DiagnosticData data, StringBuilder builder)
	{
		string id = data.Id[^2..];

		builder.Append($"\t\t\t\tnew DiagnosticData(\"{data.Title}\", {id}, \"{data.Id}.md\", {data.Fatal.ToString().ToLower()}, {data.HasLocation.ToString().ToLower()}");
	}

	private static void FindAndWriteIncludedTypes(string[] files, string[] types, StringBuilder builder)
	{
		if(types.Length == 0)
		{
			return;
		}

		builder.Append(
$@"			types: new TypeIdentity[]
			{{
");

		string[] fileNames = new string[files.Length];

		for (int i = 0; i < files.Length; i++)
		{
			fileNames[i] = Path.GetFileNameWithoutExtension(files[i]);
		}

		foreach (string type in types)
		{
			for (int i = 0; i < files.Length; i++)
			{
				if(type != fileNames[i])
				{
					continue;
				}

				IncludedType? data = GetTypeData(files[i], type);

				if(!data.HasValue)
				{
					continue;
				}

				IncludedType t = data.Value;

				builder.AppendLine($"\t\t\t\tnew TypeIdentity(\"{t.Name}\", \"{t.Namespace}\"),");
			}
		}

		builder.AppendLine("\t\t\t}");
	}

	private static void EndIdentity(StringBuilder builder)
	{
		builder.AppendLine("\t\t);");
	}

	private static IncludedType? GetTypeData(string file, string typeName)
	{
		string content = File.ReadAllText(file);
		Match match = Regex.Match(content, $@"(\w+)\s*{typeName}", RegexOptions.Singleline);
		string kind;

		if (match.Success)
		{
			kind = match.Groups[1].ToString();
		}
		else
		{
			return null;
		}

		string @namespace;
		match = _namespaceRegex.Match(content);

		if (match.Success)
		{
			@namespace = match.Groups[1].ToString();
		}
		else
		{
			@namespace = string.Empty;
		}

		return new IncludedType(typeName, @namespace, kind);
	}
}
