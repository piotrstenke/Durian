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
	private static readonly Regex _diagnosticRegex = new(@"public\s*static\s*readonly\s*DiagnosticDescriptor\s*\w+\s*=\s*new\s*\w*\s*\(.*?\)\s*;", RegexOptions.Singleline);
	private static readonly Regex _idRegex = new(@"id\s*:\s*""\s*(\w+)\s*""", RegexOptions.Singleline);
	private static readonly Regex _titleRegex = new(@"title\s*:\s*""\s*(.*?)\s*""", RegexOptions.Singleline);
	private static readonly Regex _severityRegex = new(@"defaultSeverity\s*:\s*DiagnosticSeverity\s*.\s*(\w+)", RegexOptions.Singleline);
	private static readonly Regex _locationRegex = new(@"\[\s*WithoutLocation\s*\]");
	private static readonly Regex _definitionAttributeRegex = new(@"\[\s*assembly\s*:\s*PackageDefinition\s*\(\s*DurianPackage\s*\.\s*(\w+)\s*,\s*(.*?),\s*""(.*?)""\s*(?:,\s*DurianModule\s*\.\s*(.*?))?\)\s*\]", RegexOptions.Singleline);
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

		Configuration[] configurations = HandleConfigFiles(dir);
		ModuleData[] modules = GetModules(configurations);
		WriteOutput(configurations, modules, output);
	}

	private static Configuration[] HandleConfigFiles(string directory)
	{
		string[] coreFilePaths = GetFilesInDurianCore(directory);
		string[] coreFileNames = GetNamesOfFilesInDurianCore(coreFilePaths);

		string[] directories = Directory.GetDirectories(directory);

		HashSet<DiagnosticData> allDiagnostics = new();
		List<Configuration> configurations = new(directories.Length);
		List<IncludedType> includedTypes = new(32);

		foreach (string dir in directories)
		{
			string configFile = dir + @"\" + "_Configuration.cs";

			if (!File.Exists(configFile))
			{
				Console.WriteLine($"Directory '{dir}' does not contain a _Configuration.cs file!");
				continue;
			}

			string content = File.ReadAllText(configFile);
			PackageDefinition? definition = GetPackageDefinition(content);

			if (!definition.HasValue)
			{
				Console.WriteLine($"File '{configFile}' contains invalid package definition!");
				continue;
			}

			PackageDefinition def = definition.Value;

			if (def.DiagnosticFiles.Length > 0 && def.Modules.Length == 1 && def.Modules[0] == "None")
			{
				Console.WriteLine($"Package '{def.PackageName}' contains diagnostic definitions, but is not part of any Durian module!");
				continue;
			}

			DiagnosticData[] diagnostics = GetDiagnostics(in def, allDiagnostics, dir);
			IncludedType[] types = GetIncludedTypes(coreFileNames, coreFilePaths, def.IncludedTypes, def.Modules, includedTypes);

			configurations.Add(new Configuration(in def, diagnostics, types));
		}

		SetExternalDiagnostics(configurations, allDiagnostics);
		return configurations.ToArray();
	}

	private static DiagnosticData[] GetDiagnostics(in PackageDefinition definition, HashSet<DiagnosticData> allDiagnostics, string currentDirectory)
	{
		if (definition.DiagnosticFiles.Length == 0)
		{
			return Array.Empty<DiagnosticData>();
		}

		List<DiagnosticData> diagnostics = new(32);

		foreach (string file in definition.DiagnosticFiles)
		{
			MatchCollection matches = _diagnosticRegex.Matches(File.ReadAllText(currentDirectory + @"\" + file));

			if (matches.Count == 0)
			{
				Console.WriteLine($"File '{file}' does not contain any diagnostic definitions!");
				continue;
			}

			foreach (Match match in matches)
			{
				DiagnosticData data = GetDiagnosticData(match, definition.Modules![0], file);

				if (allDiagnostics.Add(data))
				{
					diagnostics.Add(data);
				}
				else
				{
					Console.WriteLine($"Diagnostic '{data.Id}' is defined more than once! Last detection: '{data.File}'");
				}
			}
		}

		return diagnostics.ToArray();
	}

	private static string[] GetFilesInDurianCore(string directory)
	{
		string dir = directory + @"\" + "Durian.Core";
		return Directory.GetFiles(dir, "*.cs", SearchOption.AllDirectories);
	}

	private static string[] GetNamesOfFilesInDurianCore(string[] files)
	{
		string[] fileNames = new string[files.Length];

		for (int i = 0; i < files.Length; i++)
		{
			fileNames[i] = Path.GetFileNameWithoutExtension(files[i]);
		}

		return fileNames;
	}

	private static PackageDefinition? GetPackageDefinition(string content)
	{
		Match match = _definitionAttributeRegex.Match(content);

		if (!match.Success || match.Groups.Count < 5)
		{
			return null;
		}

		string packageName = GetDefinitionValue(match, 1);
		string packageType = GetDefinitionValue(match, 2);
		string version = GetDefinitionValue(match, 3);
		string[] modules = GetParentModulesOfPackage(match);
		string[] files = GetDiagnosticFiles(content);
		string[] includedTypes = GetAttributeArguments(content, _includedTypesAttributeRegex);
		string[] includedDiagnostics = GetAttributeArguments(content, _includedDiagnosticsAttributeRegex);

		if (modules.Length == 0)
		{
			modules = null!;
		}

		return new PackageDefinition(packageName, packageType, version, modules, includedDiagnostics, includedTypes, files);
	}

	private static IncludedType[] GetIncludedTypes(string[] fileNames, string[] filePaths, string[] types, string[] modules, List<IncludedType> existing)
	{
		if (types.Length == 0)
		{
			return Array.Empty<IncludedType>();
		}

		List<IncludedType> list = new(types.Length);
		int numFiles = fileNames.Length;

		foreach (string type in types)
		{
			for (int i = 0; i < numFiles; i++)
			{
				if (fileNames[i] == type)
				{
					if (TryGetIncludedType(filePaths[i], type, existing, out IncludedType? t))
					{
						t!.TryAddModules(modules);
						list.Add(t);
					}
					else
					{
						Console.WriteLine($"Type '{type}' could not be parsed! Change name of the type to match the name of the file.");
						break;
					}
				}
			}
		}

		return list.ToArray();
	}

	private static bool TryGetIncludedType(string file, string typeName, List<IncludedType> existing, out IncludedType? type)
	{
		if (!File.Exists(file))
		{
			type = null;
			return false;
		}

		string content = File.ReadAllText(file);
		Match match = _namespaceRegex.Match(content);

		if (match.Success)
		{
			string @namespace = match.Groups[1].ToString();

			foreach (IncludedType t in existing)
			{
				if (t.Name == typeName && t.Namespace == @namespace)
				{
					type = t;
					return true;
				}
			}

			type = new IncludedType(typeName, @namespace);
			existing.Add(type);
			return true;
		}

		type = null;
		return false;
	}

	private static DiagnosticData GetDiagnosticData(Match match, string moduleName, string file)
	{
		string value = match.ToString();

		Match id = _idRegex.Match(value);
		Match title = _titleRegex.Match(value);
		Match severity = _severityRegex.Match(value);
		bool hasLocation = !_locationRegex.IsMatch(value);
		bool fatal = severity.Groups[1].ToString().Trim() == "Error";

		return new DiagnosticData(title.Groups[1].ToString(), id.Groups[1].ToString(), moduleName, hasLocation, fatal, file);
	}

	private static string GetDefinitionValue(Match match, int group)
	{
		return match.Groups[group].ToString().Trim();
	}

	private static string[] GetParentModulesOfPackage(Match match)
	{
		string value = match.Groups[4].ToString();

		if (string.IsNullOrWhiteSpace(value))
		{
			return new string[] { "None" };
		}

		string[] modules = value
			.Replace("DurianModule.", "")
			.Split(',');

		for (int i = 0; i < modules.Length; i++)
		{
			modules[i] = modules[i].Trim();
		}

		if (modules.Length == 0)
		{
			return new string[] { "None" };
		}

		return modules;
	}

	private static string[] GetDiagnosticFiles(string content)
	{
		MatchCollection? matches = GetMatches(content, _diagnosticFilesAttributeRegex);

		if (matches is null)
		{
			return Array.Empty<string>();
		}

		List<string> files = new(matches.Count);

		foreach (Match match in matches)
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

	private static void SetExternalDiagnostics(List<Configuration> configurations, HashSet<DiagnosticData> diagnostics)
	{
		int length = diagnostics.Count;

		if (length == 0)
		{
			return;
		}

		DiagnosticData[] allDiagnostics = new DiagnosticData[length];
		diagnostics.CopyTo(allDiagnostics);
		Dictionary<string, DiagnosticData> dict = new(allDiagnostics.Select(d => new KeyValuePair<string, DiagnosticData>(d.Id, d)));
		List<DiagnosticData> list = new();

		foreach (Configuration config in configurations)
		{
			string[] externalDiagnostics = config.Definition.ExternalDiagnostics;

			if (externalDiagnostics.Length == 0)
			{
				continue;
			}

			list.Capacity = externalDiagnostics.Length;

			foreach (string diagnostic in externalDiagnostics)
			{
				if (dict.TryGetValue(diagnostic, out DiagnosticData data))
				{
					list.Add(data);
				}
				else
				{
					Console.WriteLine($"Diagnostic '{diagnostic}' not found!");
				}
			}

			config.ExternalDiagnostics = list.ToArray();
			list.Clear();
		}
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

			if (string.IsNullOrWhiteSpace(value))
			{
				value = match.Groups[2].ToString().Trim();

				if (string.IsNullOrWhiteSpace(value))
				{
					continue;
				}
			}

			list.Add(value);
		}

		return list.ToArray();
	}

	private static ModuleData[] GetModules(Configuration[] configurations)
	{
		HashSet<string> includedTypes = new();
		HashSet<string> packages = new();

		(string moduleName, Configuration[] configurations)[] sorted = SortConfigurations(configurations);
		ModuleData[] modules = new ModuleData[sorted.Length];

		for (int i = 0; i < sorted.Length; i++)
		{
			ModuleData data = new(sorted[i].moduleName);

			foreach (Configuration config in sorted[i].configurations)
			{
				for (int j = 0; j < config.IncludedTypes.Length; j++)
				{
					IncludedType type = config.IncludedTypes[j];

					if (includedTypes.Contains(type.Name))
					{
						Console.WriteLine($"Type '{type.Name}' is included by the '{sorted[i].moduleName}' module in multiple places!");
					}
					else
					{
						data.IncludedTypes.Add(type);
					}
				}

				if (packages.Contains(config.PackageName))
				{
					Console.WriteLine($"Package '{config.PackageName}' is part of multiple modules! Last encounter: '{sorted[i].moduleName}'");
				}
				else
				{
					data.Packages.Add(config.PackageName);
				}

				data.Diagnostics.AddRange(config.Diagnostics);
			}

			modules[i] = data;

			includedTypes.Clear();
		}

		return modules;
	}

	private static (string moduleName, Configuration[] configurations)[] SortConfigurations(Configuration[] configurations)
	{
		List<List<Configuration>> list = new(configurations.Length);
		List<string> moduleNames = new(configurations.Length);
		Dictionary<string, int> dict = new();

		foreach (Configuration configuration in configurations)
		{
			foreach (string module in configuration.Modules)
			{
				if (module == "None")
				{
					continue;
				}

				if (dict.TryGetValue(module, out int index))
				{
					if (module == configuration.Modules[0])
					{
						list[index].Add(configuration);
					}
					else
					{
						list[index].Add(MoveDiagnosticsToExternal(configuration));
					}
				}
				else
				{
					List<Configuration> current = new(4) { configuration };
					dict.Add(module, list.Count);
					moduleNames.Add(module);
					list.Add(current);
				}
			}
		}

		(string, Configuration[])[] sorted = new (string, Configuration[])[list.Count];

		for (int i = 0; i < list.Count; i++)
		{
			sorted[i] = (moduleNames[i], list[i].ToArray());
		}

		return sorted;
	}

	private static Configuration MoveDiagnosticsToExternal(Configuration configuration)
	{
		int length = configuration.Diagnostics.Length;

		if (length == 0)
		{
			return configuration;
		}

		List<DiagnosticData> external = new(configuration.ExternalDiagnostics ?? Array.Empty<DiagnosticData>());

		for (int i = 0; i < length; i++)
		{
			external.Add(configuration.Diagnostics[i]);
		}

		return new Configuration(in configuration._def, Array.Empty<DiagnosticData>(), configuration.IncludedTypes);
	}

	private static void WriteOutput(Configuration[] configurations, ModuleData[] modules, string directory)
	{
		StringBuilder builder = new(4096);

		WritePackageRepository(builder, configurations);
		File.WriteAllText(Path.Combine(directory, ".generated", "PackageRepository.cs"), builder.ToString());
		builder.Clear();

		WriteModuleRepository(builder, modules);
		File.WriteAllText(Path.Combine(directory, ".generated", "ModuleRepository.cs"), builder.ToString());
	}

	private static void WritePackageRepository(StringBuilder builder, Configuration[] configurations)
	{
		builder.Append(GetAutoGeneratedNotice());
		builder.Append(
$@"using System.Runtime.CompilerServices;

namespace Durian.Info
{{
	/// <summary>
	/// Factory class of <see cref=""PackageIdentity""/> for all available Durian packages.
	/// </summary>
	public static class PackageRepository
	{{");

		if (configurations.Length <= 0)
		{
			builder.AppendLine();
		}
		else
		{
			foreach (Configuration config in configurations)
			{
				builder.AppendLine();
				builder.Append(
	$@"		/// <summary>
		/// Creates a new instance of <see cref=""PackageIdentity""/> for the <see cref=""DurianPackage.{config.PackageName}""/> package.
		/// </summary>
		public static PackageIdentity {config.PackageName} => Initialize(
			package: DurianPackage.{config.PackageName},
			version: ""{config.Version}"",
			type: {config.PackageType},
			modules: ");

				if (config.Modules is null)
				{
					builder.AppendLine("null");
				}
				else
				{
					builder.AppendLine("new DurianModule[]");
					builder.AppendLine("\t\t\t{");

					foreach (string module in config.Modules)
					{
						builder.Append("\t\t\t\tDurianModule.").Append(module).AppendLine(",");
					}

					builder.AppendLine("\t\t\t}");
				}

				builder.AppendLine("\t\t);");
			}
		}

		builder.AppendLine();
		builder.AppendLine(
@"		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static PackageIdentity Initialize(DurianPackage package, string version, PackageType type, DurianModule[] modules)
		{
			PackageIdentity p = new PackageIdentity(package, version, type);
			p.Initialize(modules);
			return p;
		}
	}
}");
	}

	private static void WriteModuleRepository(StringBuilder builder, ModuleData[] modules)
	{
		builder.Append(GetAutoGeneratedNotice());
		builder.Append(
$@"namespace Durian.Info
{{
	/// <summary>
	/// Factory class of <see cref=""ModuleIdentity""/> for all available Durian modules.
	/// </summary>
	public static class ModuleRepository
	{{");

		if (modules.Length == 0)
		{
			builder.AppendLine();
		}
		else
		{
			foreach (ModuleData module in modules)
			{
				builder.AppendLine();
				WriteModuleIdentity(module, builder);
			}
		}

		builder.AppendLine(
$@"	}}
}}");
	}

	private static void WriteModuleIdentity(ModuleData module, StringBuilder builder)
	{
		builder.Append(
$@"		/// <summary>
		/// Creates a new instance of <see cref=""ModuleIdentity""/> for the <see cref=""DurianModule.{module.Name}""/> module.
		/// </summary>
		public static ModuleIdentity {module.Name} => new(
			module: DurianModule.{module.Name},
			id: {module.GetId()},
			packages: ");

		if (module.Packages.Count == 0)
		{
			builder.AppendLine("null,");
		}
		else
		{
			builder.AppendLine(
$@"new PackageIdentity[]
			{{");

			foreach (string packge in module.Packages)
			{
				builder.AppendLine($"\t\t\t\tPackageRepository.{packge},");
			}

			builder.AppendLine("\t\t\t},");
		}

		builder.Append("\t\t\tdocsPath: ");

		if (module.Diagnostics.Count == 0 && module.ExternalDiagnostics.Count == 0)
		{
			builder.AppendLine("null,");
			builder.AppendLine("\t\t\tdiagnostics: null,");
		}
		else
		{
			builder.AppendLine($"\"{module.Documentation}\",");
			builder.AppendLine("\t\t\tdiagnostics: new DiagnosticData[]");
			builder.Append("\t\t\t{");

			int length = module.Diagnostics.Count;
			for (int i = 0; i < length; i++)
			{
				ref readonly DiagnosticData data = ref module.Diagnostics.ToArray()[i];

				WriteDiagnosticData(in data, module, builder);
				builder.AppendLine();
				builder.AppendLine("\t\t\t\t),");
			}

			if (length > 0 && module.ExternalDiagnostics.Count > 0)
			{
				builder.AppendLine();
				builder.AppendLine(
$@"				//
				// External diagnostics
				//");
				builder.AppendLine();
			}

			length = module.ExternalDiagnostics.Count;
			for (int i = 0; i < length; i++)
			{
				ref readonly DiagnosticData data = ref module.ExternalDiagnostics.ToArray()[i];

				WriteDiagnosticData(in data, module, builder);
				builder.AppendLine(",");
				builder.AppendLine($"\t\t\t\t\toriginalModule: {data.Module}");
				builder.AppendLine("\t\t\t\t),");
			}

			builder.AppendLine("\t\t\t},");
		}

		builder.Append("\t\t\ttypes: ");

		if (module.IncludedTypes.Count == 0)
		{
			builder.AppendLine("null");
		}
		else
		{
			builder.AppendLine("new TypeIdentity[]");
			builder.AppendLine("\t\t\t{");

			int length = module.IncludedTypes.Count;

			IncludedType[] types = module.IncludedTypes.ToArray();
			for (int i = 0; i < length; i++)
			{
				IncludedType type = types[i];

				builder.AppendLine(
@$"				new TypeIdentity(
					name: ""{type.Name}"",
					@namespace: ""{type.Namespace}"",
					modules: new DurianModule[]
					{{");

				foreach (string m in type.ModuleNames)
				{
					builder.Append("\t\t\t\t\t\tDurianModule.").Append(m).AppendLine(",");
				}

				builder.AppendLine(
@"					}
				),");
			}

			builder.AppendLine("\t\t\t}");
		}

		builder.AppendLine("\t\t);");
	}

	private static void WriteDiagnosticData(in DiagnosticData diag, ModuleData module, StringBuilder builder)
	{
		builder.AppendLine();
		builder.Append(
$@"				new DiagnosticData(
					title: ""{diag.Title}"",
					id: {diag.Id.Substring(5, 2)},
					docsPath: ""{module.Documentation}/{diag.Id}.md"",
					fatal: {diag.Fatal.ToString().ToLower()},
					hasLocation: {diag.HasLocation.ToString().ToLower()}");
	}

	private static string GetAutoGeneratedNotice()
	{
		return
@"//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the GenerateModuleRepository project.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

";
	}
}
