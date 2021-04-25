﻿using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Durian.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.DefaultParam
{
	internal static class DefaultParamUtilities
	{
		public static int GetIndent(SyntaxNode? node)
		{
			SyntaxNode? parent = node;
			int indent = 0;

			while ((parent = parent!.Parent) is not null)
			{
				if (parent is CompilationUnitSyntax)
				{
					continue;
				}

				indent++;
			}

			if (indent < 0)
			{
				indent = 0;
			}

			return indent;
		}

		public static int GetIndentWithoutMultipleNamespaces(SyntaxNode? node)
		{
			bool isNamespace = false;
			SyntaxNode? parent = node;
			int indent = 0;

			while ((parent = parent!.Parent) is not null)
			{
				if (parent is CompilationUnitSyntax)
				{
					continue;
				}

				if (parent is NamespaceDeclarationSyntax)
				{
					if (isNamespace)
					{
						continue;
					}
					else
					{
						isNamespace = true;
					}
				}

				indent++;
			}

			if (indent < 0)
			{
				indent = 0;
			}

			return indent;
		}

		public static string GetHintName(ISymbol symbol)
		{
			string? parentName = symbol.ContainingType?.Name;
			return $"{(parentName is not null ? $"{parentName}." : string.Empty)}{symbol.Name}";
		}

		public static IEnumerable<string> GetUsedNamespaces(IDefaultParamTarget target, in TypeParameterContainer parameters, CancellationToken cancellationToken = default)
		{
			int defaultParamCount = parameters.NumDefaultParam;
			List<string> namespaces = GetUsedNamespacesList(target, defaultParamCount, cancellationToken);

			for (int i = 0; i < defaultParamCount; i++)
			{
				ref readonly TypeParameterData data = ref parameters.GetDefaultParamAtIndex(i);

				if (data.TargetType is null || data.TargetType.IsPredefined())
				{
					continue;
				}

				string n = data.TargetType.JoinNamespaces();

				if (!namespaces.Contains(n))
				{
					namespaces.Add(n);
				}
			}

			return namespaces;
		}

		private static List<string> GetUsedNamespacesList(IDefaultParamTarget target, int defaultParamCount, CancellationToken cancellationToken)
		{
			INamespaceSymbol globalNamespace = target.ParentCompilation.Compilation.Assembly.GlobalNamespace;
			string[] namespaces = target.SemanticModel.GetUsedNamespacesWithoutDistinct(target.Declaration, globalNamespace, true, cancellationToken).ToArray();

			int count = 0;
			int length = namespaces.Length;

			for (int i = 0; i < length; i++)
			{
				if (namespaces[i] == "Durian")
				{
					count++;

					if (count > defaultParamCount)
					{
						return namespaces.ToList();
					}
				}
			}

			return namespaces.Distinct().Where(n => n != "Durian").ToList();
		}
	}
}
