using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Durian.Generator.Data;
using Durian.Generator.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.Generator.DefaultParam
{
	/// <summary>
	/// Contains various utility methods related to the 'DefaultParam' module.
	/// </summary>
	internal static class DefaultParamUtilities
	{
		/// <summary>
		/// Converts an array of <see cref="ITypeData"/>s to an array of <see cref="INamedTypeSymbol"/>s.
		/// </summary>
		/// <param name="types">Array of <see cref="ITypeData"/>s to convert.</param>
		public static INamedTypeSymbol[] TypeDatasToTypeSymbols(ITypeData[] types)
		{
			int length = types.Length;
			INamedTypeSymbol[] symbols = new INamedTypeSymbol[length];

			for (int i = 0; i < length; i++)
			{
				symbols[i] = types[i].Symbol;
			}

			return symbols;
		}

		/// <summary>
		/// Returns an enum value of specified property of the <paramref name="configurationAttribute"/>.
		/// </summary>
		/// <param name="attributes">A collection of <see cref="AttributeData"/>s to get the value from.</param>
		/// <param name="configurationAttribute"><see cref="INamedTypeSymbol"/> of the configuration attribute.</param>
		/// <param name="propertyName">Name of property to get the value of.</param>
		/// <param name="value">Returned enum value as an <see cref="int"/>.</param>
		public static bool TryGetConfigurationPropertyName<T>(IEnumerable<AttributeData> attributes, INamedTypeSymbol configurationAttribute, string propertyName, out T? value)
		{
			AttributeData? attr = attributes.FirstOrDefault(attr => SymbolEqualityComparer.Default.Equals(attr.AttributeClass, configurationAttribute));

			if (attr is null)
			{
				value = default!;
				return false;
			}

			return attr.TryGetNamedArgumentValue(propertyName, out value);
		}

		/// <summary>
		/// Returns a new <see cref="IEnumerator{T}"/> for the specified <paramref name="filter"/>.
		/// </summary>
		/// <param name="filter"><see cref="IDefaultParamFilter"/> to get the <see cref="IEnumerator{T}"/> for.</param>
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

		/// <summary>
		/// Iterates through whole <paramref name="filter"/>.
		/// </summary>
		/// <typeparam name="T">Type of <see cref="IDefaultParamTarget"/> the <paramref name="filter"/> returns.</typeparam>
		/// <param name="filter"><see cref="IDefaultParamFilter"/> to iterate through.</param>
		/// <returns>An array of <see cref="IDefaultParamTarget"/>s that were returned by the <paramref name="filter"/>.</returns>
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

		/// <summary>
		/// Get the indent level of the <paramref name="node"/>.
		/// </summary>
		/// <param name="node"><see cref="CSharpSyntaxNode"/> to get the indent level of.</param>
		public static int GetIndent(CSharpSyntaxNode? node)
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

		/// <summary>
		/// Returns a collection of all namespaces used by the <paramref name="target"/> <see cref="IDefaultParamTarget"/>.
		/// </summary>
		/// <param name="target"><see cref="IDefaultParamTarget"/> to get the namespaces used by.</param>
		/// <param name="parameters"><see cref="TypeParameterContainer"/> that contains the type parameters of the <paramref name="target"/> member.</param>
		/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
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
						return namespaces.Distinct().ToList();
					}
				}
			}

			return namespaces.Distinct().Where(n => n != "Durian").ToList();
		}
	}
}
