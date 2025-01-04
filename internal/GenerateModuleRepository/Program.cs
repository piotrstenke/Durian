using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

internal static class Program
{
#pragma warning disable SYSLIB1045 // Convert to 'GeneratedRegexAttribute'.
	private static readonly Regex _diagnosticRegex = new(@"public\s*static\s*readonly\s*DiagnosticDescriptor\s*\w+\s*=\s*new\s*\w*\s*\(.*?\)\s*;", RegexOptions.Singleline);
	private static readonly Regex _idRegex = new(@"id\s*:\s*""\s*(\w+)\s*""", RegexOptions.Singleline);
	private static readonly Regex _titleRegex = new(@"title\s*:\s*""\s*(.*?)\s*""", RegexOptions.Singleline);
	private static readonly Regex _severityRegex = new(@"defaultSeverity\s*:\s*DiagnosticSeverity\s*.\s*(\w+)", RegexOptions.Singleline);
	private static readonly Regex _locationRegex = new(@"\[\s*WithoutLocation\s*\]");
	private static readonly Regex _documentationRegex = new(@"helpLinkUri:\s*DocsPath\s*\+\s*\""\/(.*\.md)\""");
#pragma warning restore SYSLIB1045 // Convert to 'GeneratedRegexAttribute'.

	private static void Main(string[] args)
	{
		if (args.Length < 2)
		{
			return;
		}

		string dir = args[0];
		string output = Path.Combine(dir, args[1]);

		List<ModuleConfiguration> configurations = GetConfigurations(dir);
		WriteOutput(configurations, output);
	}

	private static List<ModuleConfiguration> GetConfigurations(string dir)
	{
		ModuleCollection modules = JsonConvert.DeserializeObject<ModuleCollection>(File.ReadAllText(Path.Combine(dir, "durian.json")))!;

		Dictionary<string, DiagnosticData> allDiagnostics = new();
		Dictionary<string, TypeData> allTypes = new();

		List<ModuleConfiguration> configurations = new(modules.Modules!.Length);

		foreach (ModuleData module in modules)
		{
			ModuleConfiguration config = new() { Module = module };

			SetDiagnostics(config, allDiagnostics, dir);
			SetIncludedTypes(config, allTypes);

			configurations.Add(config);
		}

		foreach (ModuleConfiguration config in configurations)
		{
			SetExternalDiagnostics(config, allDiagnostics);
		}

		return configurations;
	}

	private static void SetIncludedTypes(ModuleConfiguration module, Dictionary<string, TypeData> allTypes)
	{
		string[]? included = module.Module!.IncludedTypes;

		if (included is null || included.Length == 0)
		{
			return;
		}

		List<TypeData> includedTypes = new(included.Length);

		foreach (string t in included)
		{
			includedTypes.Add(GetTypeData(t, allTypes, module.Module.Name!));
		}

		module.IncludedTypes = includedTypes;
	}

	private static TypeData GetTypeData(string typeName, Dictionary<string, TypeData> types, string moduleName)
	{
		if (types.TryGetValue(typeName, out TypeData? data))
		{
			if (data.Modules!.Contains(moduleName))
			{
				Console.WriteLine($"Module '{moduleName}' already contains type '{typeName}'");
			}
			else
			{
				data.Modules.Add(moduleName);
			}
		}
		else
		{
			int lastPeriod = typeName.LastIndexOf('.');

			string @namespace = typeName[..lastPeriod];
			string name = typeName[(lastPeriod + 1)..];

			data = new TypeData()
			{
				Modules = new() { moduleName },
				Name = name,
				Namespace = @namespace
			};

			types.Add(typeName, data);
		}

		return data;
	}

	private static void SetDiagnostics(ModuleConfiguration module, Dictionary<string, DiagnosticData> allDiagnostics, string dir)
	{
		string[]? files = module.Module!.DiagnosticFiles;

		if (files is null || files.Length == 0)
		{
			return;
		}

		List<DiagnosticData> diagnostics = new(32);

		foreach (string f in files)
		{
			string file = Path.Combine(dir, f);

			MatchCollection matches = _diagnosticRegex.Matches(File.ReadAllText(file));

			if (matches.Count == 0)
			{
				Console.WriteLine($"File '{file}' does not contain any diagnostic definitions!");
				continue;
			}

			foreach (Match match in matches.Cast<Match>())
			{
				DiagnosticData data = GetDiagnosticData(match, module.Module.Name!, file);

				if (allDiagnostics.TryAdd(data.Id!, data))
				{
					diagnostics.Add(data);
				}
				else
				{
					Console.WriteLine($"Diagnostic '{data.Id}' is defined more than once! Last detection: '{data.File}'");
				}
			}
		}

		module.Diagnostics = diagnostics;
	}

	private static DiagnosticData GetDiagnosticData(Match match, string moduleName, string file)
	{
		string value = match.ToString();

		Match id = _idRegex.Match(value);
		Match title = _titleRegex.Match(value);
		Match severity = _severityRegex.Match(value);
		Match documentation = _documentationRegex.Match(value);
		bool hasLocation = !_locationRegex.IsMatch(value);
		bool fatal = severity.Groups[1].ToString().Trim() == "Error";

		return new DiagnosticData()
		{
			Title = title.Groups[1].ToString(),
			Id = id.Groups[1].ToString(),
			ModuleName = moduleName,
			HasLocation = hasLocation,
			Fatal = fatal,
			File = file,
			Documentation = documentation.Groups[1].ToString()
		};
	}

	private static void SetExternalDiagnostics(ModuleConfiguration module, Dictionary<string, DiagnosticData> allDiagnostics)
	{
		string[]? externalDiagnostics = module.Module!.ExternalDiagnostics;

		if (externalDiagnostics is null || externalDiagnostics.Length == 0)
		{
			return;
		}

		List<DiagnosticData> list = new(externalDiagnostics.Length);

		foreach (string diagnostic in externalDiagnostics)
		{
			if (allDiagnostics.TryGetValue(diagnostic, out DiagnosticData? data))
			{
				list.Add(data);
			}
			else
			{
				Console.WriteLine($"Diagnostic '{diagnostic}' not found!");
			}
		}

		module.ExternalDiagnostics = list;
	}

	private static void WriteOutput(List<ModuleConfiguration> configurations, string directory)
	{
		StringBuilder builder = new(4096);

		WritePackageRepository(builder, configurations);
		File.WriteAllText(Path.Combine(directory, ".generated", "PackageRepository.cs"), builder.ToString());
		builder.Clear();

		WriteTypeRepository(builder, configurations);
		File.WriteAllText(Path.Combine(directory, ".generated", "TypeRepository.cs"), builder.ToString());
		builder.Clear();

		WriteModuleRepository(builder, configurations);
		File.WriteAllText(Path.Combine(directory, ".generated", "ModuleRepository.cs"), builder.ToString());
	}

	private static void WritePackageRepository(StringBuilder builder, List<ModuleConfiguration> configurations)
	{
		builder.Append(GetAutoGeneratedNotice());
		builder.Append(
$@"namespace Durian.Info
{{
	/// <summary>
	/// Factory class of <see cref=""PackageIdentity""/>s for all available Durian packages.
	/// </summary>
	public static class PackageRepository
	{{");

		if (configurations.Count <= 0)
		{
			builder.AppendLine();
		}
		else
		{
			foreach (ModuleConfiguration config in configurations)
			{
				foreach (PackageData package in config.Module!.Packages!)
				{
					builder.AppendLine();

					builder.Append(
	$@"		/// <summary>
		/// Returns a <see cref=""PackageIdentity""/> for the <see cref=""DurianPackage.{package.Name}""/> package.
		/// </summary>
		public static PackageIdentity {package.Name}
		{{
			get
			{{
				if(!IdentityPool.Packages.TryGetValue(""{package.Name}"", out PackageIdentity package))
				{{
					package = new(
						enumValue: DurianPackage.{package.Name},
						version: ""{package.Version}"",
						type: ");

					if (package.Type is null || package.Type.Length == 0)
					{
						builder.AppendLine("null,");
					}
					else
					{
						builder.Append($"PackageType.{package.Type[0]}");

						for (int i = 1; i < package.Type.Length; i++)
						{
							builder.Append($" | PackageType.{package.Type[i]}");
						}

						builder.AppendLine(",");
					}

					builder.AppendLine(
@$"						modules: new DurianModule[]
						{{
							DurianModule.{config.Module.Name}
						}}
					);
				}}

				return package;
			}}
		}}");
				}
			}
		}

		builder.Append(
@"	}
}");
	}


	private static void WriteTypeRepository(StringBuilder builder, List<ModuleConfiguration> configurations)
	{
		builder.Append(GetAutoGeneratedNotice());
		builder.Append(
$@"using System;

namespace Durian.Info
{{
	/// <summary>
	/// Factory class of <see cref=""TypeIdentity""/>s for all available Durian <see cref=""Type""/>s.
	/// </summary>
	public static class TypeRepository
	{{");

		HashSet<string> types = new();

		foreach (ModuleConfiguration config in configurations)
		{
			if (config.IncludedTypes?.Count == default)
			{
				continue;
			}

			foreach (TypeData type in config.IncludedTypes!)
			{
				if (!types.Add(type.Name!))
				{
					continue;
				}

				builder.Append(
$@"
		/// <summary>
		/// Returns a <see cref=""TypeIdentity""/> for the <c>{type.Namespace}.{type.Name}</c> type.
		/// </summary>
		public static TypeIdentity {type.Name}
		{{
			get
			{{
				if(!IdentityPool.Types.TryGetValue(""{type.Name}"", out TypeIdentity type))
				{{
					type = new(
						name: ""{type.Name}"",
						@namespace: ""{type.Namespace}"",
						modules: new DurianModule[]
						{{");

				foreach (string m in type.Modules!)
				{
					builder.AppendLine();
					builder.Append("\t\t\t\t\t\t\tDurianModule.").Append(m).Append(',');
				}

				builder.AppendLine(
@"
						}
					);
				}

				return type;
			}
		}");
			}
		}

		builder.Append(
@"	}
}");
	}

	private static void WriteModuleRepository(StringBuilder builder, List<ModuleConfiguration> configurations)
	{
		builder.Append(GetAutoGeneratedNotice());
		builder.Append(
$@"namespace Durian.Info
{{
	/// <summary>
	/// Factory class of <see cref=""ModuleIdentity""/>s for all available Durian modules.
	/// </summary>
	public static class ModuleRepository
	{{");

		if (configurations.Count == 0)
		{
			builder.AppendLine();
		}
		else
		{
			foreach (ModuleConfiguration config in configurations)
			{
				builder.AppendLine();
				WriteModuleIdentity(builder, config);
			}
		}

		builder.AppendLine(
$@"	}}
}}");
	}

	private static void WriteModuleIdentity(StringBuilder builder, ModuleConfiguration config)
	{
		builder.Append(
$@"		/// <summary>
		/// Returns a <see cref=""ModuleIdentity""/> for the <see cref=""DurianModule.{config.Module!.Name}""/> module.
		/// </summary>
		public static ModuleIdentity {config.Module!.Name}
		{{
			get
			{{
				if(!IdentityPool.Modules.TryGetValue(""{config.Module!.Name}"", out ModuleIdentity module))
				{{
					module = new(
						module: DurianModule.{config.Module!.Name},
						id: {config.Module.DiagnosticIdPrefix ?? "default"},
						packages: ");

		if (config.Module!.Packages is null || config.Module.Packages.Length == 0)
		{
			builder.AppendLine("null,");
		}
		else
		{
			builder.AppendLine(
$@"new DurianPackage[]
						{{");

			foreach (PackageData package in config.Module.Packages)
			{
				builder.AppendLine($"\t\t\t\t\t\t\tDurianPackage.{package.Name},");
			}

			builder.AppendLine("\t\t\t\t\t\t},");
		}

		builder.Append("\t\t\t\t\t\tdocsPath: ");

		if (config.Diagnostics?.Count == default && config.ExternalDiagnostics?.Count == default)
		{
			builder.AppendLine("null,");
			builder.AppendLine("\t\t\t\t\t\tdiagnostics: null,");
		}
		else
		{
			builder.AppendLine($"\"{config.Module.Documentation}\",");
			builder.AppendLine("\t\t\t\t\t\tdiagnostics: new DiagnosticData[]");
			builder.Append("\t\t\t\t\t\t{");

			if (config.Diagnostics?.Count > 0)
			{
				int length = config.Diagnostics.Count;
				for (int i = 0; i < length; i++)
				{
					DiagnosticData data = config.Diagnostics.ToArray()[i];

					WriteDiagnosticData(in data, config.Module!, builder);
					builder.AppendLine();
					builder.AppendLine("\t\t\t\t\t\t\t),");
				}
			}

			if (config.ExternalDiagnostics?.Count > 0)
			{
				builder.AppendLine();
				builder.AppendLine(
$@"				//
				// External diagnostics
				//");
				builder.AppendLine();

				int length = config.ExternalDiagnostics.Count;
				for (int i = 0; i < length; i++)
				{
					DiagnosticData data = config.ExternalDiagnostics.ToArray()[i];

					WriteDiagnosticData(in data, config.Module, builder);
					builder.AppendLine(",");
					builder.AppendLine($"\t\t\t\t\t\t\t\toriginalModule: {data.ModuleName}");
					builder.AppendLine("\t\t\t\t\t\t\t),");
				}
			}

			builder.AppendLine("\t\t\t\t\t\t},");
		}

		builder.Append("\t\t\t\t\t\ttypes: ");

		if (config.IncludedTypes?.Count == default)
		{
			builder.AppendLine("null");
		}
		else
		{
			builder.AppendLine("new TypeIdentity[]");
			builder.AppendLine("\t\t\t\t\t\t{");

			List<TypeData> types = config.IncludedTypes!;

			for (int i = 0; i < config.IncludedTypes!.Count; i++)
			{
				TypeData type = types[i];

				builder
					.Append("\t\t\t\t\t\t\tTypeRepository.")
					.Append(type.Name)
					.AppendLine(",");
			}

			builder.AppendLine("\t\t\t\t\t\t}");
		}

		builder.AppendLine(
@"					);
				}

				return module;
			}
		}");
	}

	private static void WriteDiagnosticData(in DiagnosticData diag, ModuleData module, StringBuilder builder)
	{
		builder.AppendLine();
		builder.Append(
$@"							new DiagnosticData(
								title: ""{diag.Title}"",
								id: {diag.Id!.AsSpan(5, 2)},
								docsPath: ""{module.Documentation}/{diag.Id}.md"",
								fatal: {diag.Fatal.ToString().ToLower()},
								hasLocation: {diag.HasLocation.ToString().ToLower()}");
	}

	private static string GetAutoGeneratedNotice()
	{
		return
@"//------------------------------------------------------------------------------
// <auto-generated>
//	 This code was generated by the GenerateModuleRepository project.
//
//	 Changes to this file may cause incorrect behavior and will be lost if
//	 the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

";
	}
}
