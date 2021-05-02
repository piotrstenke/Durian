using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Durian.Data;
using Durian.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.DefaultParam
{
	internal static class DefaultParamUtilities
	{
		public static IEnumerator<IMemberData> GetFilterEnumerator(IDefaultParamFilter filter)
		{
			return filter.Mode switch
			{
				FilterMode.None => new FilterEnumerator(filter),
				FilterMode.Diagnostics => new DiagnosticEnumerator(filter),
				FilterMode.Logs => new LoggableEnumerator(filter),
				FilterMode.Both => new LoggableDiagnosticEnumerator(filter),
				_ => new FilterEnumerator(filter)
			};
		}

		public static T[] IterateFilter<T>(IDefaultParamFilter filter) where T : IDefaultParamTarget
		{
			IEnumerable<IDefaultParamTarget> collection = filter.Mode switch
			{
				FilterMode.None => IterateFilter(new FilterEnumerator(filter)),
				FilterMode.Diagnostics => IterateFilter(new DiagnosticEnumerator(filter)),
				FilterMode.Logs => IterateFilter(new LoggableEnumerator(filter)),
				FilterMode.Both => IterateFilter(new LoggableDiagnosticEnumerator(filter)),
				_ => IterateFilter(new FilterEnumerator(filter))
			};

			return collection.Cast<T>().ToArray();
		}

		private static IEnumerable<IDefaultParamTarget> IterateFilter<T>(T iter) where T : IEnumerator<IDefaultParamTarget>
		{
			while (iter.MoveNext())
			{
				yield return iter.Current;
			}
		}

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
