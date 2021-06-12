// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using Durian.Generator;
using Durian.Generator.Core;
using Durian.Generator.DefaultParam;
using Durian.Tests;

internal static class Program
{
	private static HashSet<string> GetCurrentState(string filePath)
	{
		if (!File.Exists(filePath))
		{
			return new HashSet<string>();
		}

		using ReleaseReader reader = new(filePath);

		return reader.ReadEntries();
	}

	private static string? GetMemberName(MemberInfo member, string fullTypeName, out string? additionalName)
	{
		additionalName = null;

		if (member is ConstructorInfo c)
		{
			if (c.IsPrivate || c.IsAssembly || c.IsFamilyAndAssembly)
			{
				return null;
			}

			return GetMethodName(c.GetParameters(), 0, fullTypeName);
		}
		else if (member is FieldInfo f)
		{
			if (f.IsPrivate || f.IsAssembly || f.IsFamilyAndAssembly)
			{
				return null;
			}
		}
		else if (member is MethodInfo m)
		{
			if (m.Name.StartsWith("get_") || m.Name.StartsWith("set_") || m.IsPrivate || m.IsAssembly || m.IsFamilyAndAssembly)
			{
				return null;
			}

			return GetMethodName(m.GetParameters(), m.GetGenericArguments().Length, fullTypeName + "." + member.Name);
		}
		else if (member is PropertyInfo p)
		{
			ParameterInfo[] parameters = p.GetIndexParameters();

			if (parameters.Length > 0)
			{
				return GetMethodName(parameters, 0, fullTypeName + "." + "this");
			}

			string? getName = null;
			string? setName = null;

			if (p.GetGetMethod() is MethodInfo get && !(get.IsPrivate || get.IsAssembly || get.IsFamilyAndAssembly))
			{
				getName = fullTypeName + "." + get.Name;
			}

			if (p.GetSetMethod() is MethodInfo set && !(set.IsPrivate || set.IsAssembly || set.IsFamilyAndAssembly))
			{
				setName = fullTypeName + "." + set.Name;
			}

			if (getName is null)
			{
				return setName;
			}

			additionalName = setName;
			return getName;
		}
		else if (member is EventInfo e)
		{
			string? addName = null;
			string? remName = null;

			if (e.GetAddMethod() is MethodInfo add && !(add.IsPrivate || add.IsAssembly || add.IsFamilyAndAssembly))
			{
				addName = fullTypeName + "." + add.Name;
			}

			if (e.GetRemoveMethod() is MethodInfo rem && !(rem.IsPrivate || rem.IsAssembly || rem.IsFamilyAndAssembly))
			{
				remName = fullTypeName + "." + rem.Name;
			}

			if (addName is null)
			{
				return remName;
			}

			additionalName = remName;
			return addName;
		}

		return fullTypeName + "." + member.Name;
	}

	private static string GetMethodName(ParameterInfo[] parameters, int numTypeParameters, string fullName)
	{
		if (parameters.Length == 0)
		{
			return fullName + "()";
		}

		StringBuilder builder = new(256);
		builder.Append(fullName);

		if (numTypeParameters > 0)
		{
			builder.Append('\'').Append(numTypeParameters);
		}

		builder.Append('(');

		foreach (ParameterInfo parameter in parameters)
		{
			builder.Append(parameter.ToString()).Append(", ");
		}

		builder.Remove(builder.Length - 2, 2);
		builder.Append(')');
		return builder.ToString();
	}

	private static Assembly[] GetReferencedAssemblies()
	{
		return new Assembly[]
		{
			// Durian.Core
			typeof(EnableModuleAttribute).Assembly,

			// Durian.Core.Analyzer
			typeof(IsCSharpCompilationAnalyzer).Assembly,

			// Durian.AnalysisServices
			typeof(DurianGenerator).Assembly,

			// Durian.TestServices
			typeof(SingletonGeneratorTestResult).Assembly,

			// Durian.DefaultParam
			typeof(DefaultParamAnalyzer).Assembly
		};
	}

	private static void HandleChangeLog(Assembly assembly, string directory, string release)
	{
		string filePath = directory + "/CHANGELOG.md";
		HashSet<string> entries = GetCurrentState(filePath);
		List<string> add = new();
		List<string> move = new();

		foreach (Type type in assembly.GetTypes())
		{
			if (type.IsNotPublic)
			{
				continue;
			}

			HandleType(type, entries, add, move);
		}

		string[] remove = new string[entries.Count];
		entries.CopyTo(remove);

		WriteOutput(filePath, release, add.ToArray(), move.ToArray(), remove);
	}

	private static bool HandleMember(MemberInfo member, HashSet<string> entries, List<string> add, List<string> move, string name)
	{
		if (!ValidateMember(member, name))
		{
			return false;
		}

		if (member.GetCustomAttribute<MovedFromAttribute>() is MovedFromAttribute attr)
		{
			if (entries.Remove(attr.Source))
			{
				if (entries.Contains(name))
				{
					Console.WriteLine($"Tried moving member '{attr.Source!}' to '{name}', but '{name}' already exists!");
					return false;
				}

				move.Add($"{attr.Source} -> {name}");
			}
		}
		else if (!entries.Remove(name))
		{
			add.Add(name);
		}

		return true;
	}

	private static void HandleType(Type type, HashSet<string> entries, List<string> add, List<string> move)
	{
		if (!ValidateMember(type, type.Name) || !HandleMember(type, entries, add, move, type.FullName!))
		{
			return;
		}

		MemberInfo[] members;

		if (type.IsEnum)
		{
			members = type.GetFields().Where(f => f.Name != "value__").ToArray();
		}
		else
		{
			members = type.GetMembers(BindingFlags.Public | BindingFlags.DeclaredOnly | BindingFlags.Static | BindingFlags.Instance | BindingFlags.NonPublic);
		}

		foreach (MemberInfo member in members)
		{
			if (member is TypeInfo t)
			{
				continue;
			}
			else
			{
				string? name = GetMemberName(member, type.FullName!, out string? additionalName);

				if (name is null)
				{
					continue;
				}

				name = name.Replace('`', '\'');
				HandleMember(member, entries, add, move, name);

				if (additionalName is not null)
				{
					additionalName = additionalName.Replace('`', '\'');
					HandleMember(member, entries, add, move, additionalName);
				}
			}
		}
	}

	private static void Main()
	{
		foreach (Assembly assembly in GetReferencedAssemblies())
		{
			string? release = assembly.GetCustomAttribute<PackageDefinitionAttribute>()?.Version;

			if (release is null)
			{
				continue;
			}

			string directory = $"../../../src/{assembly.GetName().Name}";
			HandleChangeLog(assembly, directory, release);
		}
	}

	private static bool ValidateMember(MemberInfo member, string name)
	{
		if (name.StartsWith('<') || name.StartsWith('+') || member.GetCustomAttribute<CompilerGeneratedAttribute>() is not null)
		{
			return false;
		}

		return true;
	}

	private static void WriteOutput(string filePath, string release, string[] added, string[] moved, string[] removed)
	{
		StringBuilder builder = new(1024);
		builder.Append("## Release ").AppendLine(release);
		bool hasValue = WriteSection("Added", added);
		hasValue |= WriteSection("Moved", moved);
		hasValue |= WriteSection("Removed", removed);

		if (hasValue)
		{
			builder.AppendLine();
			File.AppendAllText(filePath, builder.ToString(), Encoding.UTF8);
		}

		bool WriteSection(string section, string[] members)
		{
			if (members.Length > 0)
			{
				builder.AppendLine();
				builder.Append("### ").AppendLine(section);
				builder.AppendLine();

				foreach (string member in members)
				{
					builder.Append(" - ").AppendLine(member);
				}

				return true;
			}

			return false;
		}
	}
}
