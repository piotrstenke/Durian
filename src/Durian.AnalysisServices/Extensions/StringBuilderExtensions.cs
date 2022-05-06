// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;

namespace Durian.Analysis.Extensions
{
	/// <summary>
	/// Contains extension methods for the <see cref="StringBuilder"/> class.
	/// </summary>
	public static class StringBuilderExtensions
	{
		/// <summary>
		/// Writes a <see cref="string"/> that contains all the parent types of the specified <paramref name="symbol"/> and the <paramref name="symbol"/>'s name separated by the dot ('.') character to the specified <paramref name="builder"/>.
		/// </summary>
		/// <remarks>If the <paramref name="symbol"/> is not contained within a type, an empty <see cref="string"/> is returned instead.</remarks>
		/// <param name="builder"><see cref="StringBuilder"/> to write to.</param>
		/// <param name="symbol"><see cref="ISymbol"/> to get the <see cref="string"/> of.</param>
		/// <param name="includeSelf">Determines whether to include the <paramref name="symbol"/> in the returned <see cref="string"/>.</param>
		/// <param name="includeParameters">If the value of <paramref name="symbol"/> is a <see cref="IMethodSymbol"/>, determines whether to include the method's parameters in the returned <see cref="string"/>.</param>
		public static StringBuilder WriteContainingTypes(this StringBuilder builder, ISymbol symbol, bool includeSelf = true, bool includeParameters = false)
		{
			foreach (INamedTypeSymbol type in symbol.GetContainingTypes())
			{
				builder.WriteGenericName(type);
				builder.Append('.');
			}

			if (includeSelf)
			{
				builder.WriteGenericName(symbol, includeParameters ? GenericSubstitution.ParameterList : GenericSubstitution.None);
			}
			else if (builder.Length > 0)
			{
				builder.Remove(builder.Length - 1, 1);
			}

			return builder;
		}

		/// <summary>
		/// Writes a <see cref="string"/> representing a fully qualified name of the specified <paramref name="symbol"/>.
		/// </summary>
		/// <param name="builder"><see cref="StringBuilder"/> to write to.</param>
		/// <param name="symbol"><see cref="ISymbol"/> to write the fully qualified name of.</param>
		public static StringBuilder WriteFullyQualifiedName(this StringBuilder builder, ISymbol symbol)
		{
			foreach (INamespaceSymbol @namespace in symbol.GetContainingNamespaces())
			{
				builder.Append(@namespace);
				builder.Append('.');
			}

			foreach (INamedTypeSymbol type in symbol.GetContainingTypes())
			{
				builder.WriteGenericName(type);
				builder.Append('.');
			}

			return builder.WriteGenericName(symbol);
		}

		/// <summary>
		/// Writes a <see cref="string"/> containing generic identifier of the specified <paramref name="symbol"/> or name of the <paramref name="symbol"/> if it is not an <see cref="IMethodSymbol"/> or <see cref="INamedTypeSymbol"/> to the specified <paramref name="builder"/>.
		/// </summary>
		/// <param name="builder"><see cref="StringBuilder"/> to write to.</param>
		/// <param name="symbol"><see cref="ISymbol"/> to get the generic name of.</param>
		/// <param name="substitution">Configures how generic type parameters are substituted.</param>
		public static StringBuilder WriteGenericName(this StringBuilder builder, ISymbol symbol, GenericSubstitution substitution = default)
		{
			if (symbol is INamedTypeSymbol t)
			{
				builder.WriteGenericName(t, substitution);
			}
			else if (symbol is IMethodSymbol m)
			{
				builder.WriteGenericName(m, substitution);
			}
			else
			{
				builder.Append(symbol.Name);
			}

			return builder;
		}

		/// <summary>
		/// Writes a <see cref="string"/> containing generic identifier of the specified <paramref name="method"/> to the specified <paramref name="builder"/>.
		/// </summary>
		/// <param name="builder"><see cref="StringBuilder"/> to write to.</param>
		/// <param name="method"><see cref="IMethodSymbol"/> to get the generic name of.</param>
		/// <param name="substitution">Configures how generic type parameters are substituted.</param>
		public static StringBuilder WriteGenericName(this StringBuilder builder, IMethodSymbol method, GenericSubstitution substitution = default)
		{
			if (substitution.HasFlag(GenericSubstitution.TypeArguments))
			{
				builder.WriteGenericName(method.TypeArguments, method.Name);
			}
			else
			{
				builder.WriteGenericName(method.TypeParameters, method.Name, substitution.HasFlag(GenericSubstitution.Variance));
			}

			if (substitution.HasFlag(GenericSubstitution.ParameterList))
			{
				builder.WriteParameterList(method, substitution);
			}

			return builder;
		}

		/// <summary>
		/// Returns a <see cref="string"/> containing generic identifier of the specified <paramref name="type"/>.
		/// </summary>
		/// <param name="builder"><see cref="StringBuilder"/> to write to.</param>
		/// <param name="type"><see cref="INamedTypeSymbol"/> to get the generic name of.</param>
		/// <param name="substitution">Configures how generic type parameters are substituted.</param>
		public static StringBuilder WriteGenericName(this StringBuilder builder, INamedTypeSymbol type, GenericSubstitution substitution = default)
		{
			string typeName = type.GetTypeKeyword() ?? type.Name;

			if (substitution.HasFlag(GenericSubstitution.TypeArguments))
			{
				builder.WriteGenericName(type.TypeArguments, typeName);
			}
			else
			{
				builder.WriteGenericName(type.TypeParameters, typeName, substitution.HasFlag(GenericSubstitution.Variance));
			}

			if (substitution.HasFlag(GenericSubstitution.ParameterList) && type.DelegateInvokeMethod is not null)
			{
				builder.WriteParameterList(type.DelegateInvokeMethod, substitution);
			}

			return builder;
		}

		/// <summary>
		/// Writes a <see cref="string"/> containing the generic part of an identifier created from the collection of <paramref name="typeParameters"/> to the specified <paramref name="builder"/>.
		/// </summary>
		/// <param name="builder"><see cref="StringBuilder"/> to write to.</param>
		/// <param name="typeParameters">Type parameters.</param>
		/// <param name="includeVariance">Determines whether to include variance of the <paramref name="typeParameters"/>.</param>
		/// <exception cref="InvalidOperationException">Pointers can't be used as generic arguments.</exception>
		public static StringBuilder WriteGenericName(this StringBuilder builder, IEnumerable<ITypeParameterSymbol> typeParameters, bool includeVariance = false)
		{
			return builder.WriteGenericName(typeParameters, null, includeVariance);
		}

		/// <summary>
		/// Writes a <see cref="string"/> containing generic identifier combined of the specified <paramref name="name"/> and the collection of <paramref name="typeParameters"/> to the specified <paramref name="builder"/>.
		/// </summary>
		/// <param name="builder"><see cref="StringBuilder"/> to write to.</param>
		/// <param name="typeParameters">Type parameters to use to built the generic name.</param>
		/// <param name="name">Actual member identifier.</param>
		public static StringBuilder WriteGenericName(this StringBuilder builder, IEnumerable<string> typeParameters, string? name)
		{
			if (!string.IsNullOrWhiteSpace(name))
			{
				builder.Append(name);
			}

			return builder.WriteGenericName(typeParameters);
		}

		/// <summary>
		/// Writes a <see cref="string"/> containing the generic part of an identifier created from the collection of <paramref name="typeParameters"/> to the specified <paramref name="builder"/>.
		/// </summary>
		/// <param name="builder"><see cref="StringBuilder"/> to write to.</param>
		/// <param name="typeParameters">Type parameters to use to built the generic name.</param>
		public static StringBuilder WriteGenericName(this StringBuilder builder, IEnumerable<string> typeParameters)
		{
			string[] parameters = typeParameters.ToArray();
			int length = parameters.Length;

			if (length == 0)
			{
				return builder;
			}

			builder.Append('<');
			builder.Append(parameters[0]);

			for (int i = 1; i < length; i++)
			{
				builder.Append(", ").Append(parameters[i]);
			}

			builder.Append('>');

			return builder;
		}

		/// <summary>
		/// Writes a <see cref="string"/> containing generic identifier combined of the specified <paramref name="name"/> and the collection of <paramref name="typeParameters"/> to the specified <paramref name="builder"/>.
		/// </summary>
		/// <param name="builder"><see cref="StringBuilder"/> to write to.</param>
		/// <param name="typeParameters">Type parameters.</param>
		/// <param name="name">Actual member identifier.</param>
		/// <param name="includeVariance">Determines whether to include variance of the <paramref name="typeParameters"/>.</param>
		public static StringBuilder WriteGenericName(this StringBuilder builder, IEnumerable<ITypeParameterSymbol> typeParameters, string? name, bool includeVariance = false)
		{
			if (includeVariance)
			{
				return builder.WriteGenericName(typeParameters.Select(p =>
				{
					if (p.Variance == VarianceKind.Out || p.Variance == VarianceKind.In)
					{
						return $"{p.Variance.ToString().ToLower()} {p.Name}";
					}

					return p.Name;
				}),
				name);
			}

			return builder.WriteGenericName(typeParameters.Select(p => p.Name), name);
		}

		/// <summary>
		/// Writes a <see cref="string"/> containing the generic part of an identifier created from the collection of <paramref name="typeArguments"/> to the specified <paramref name="builder"/>.
		/// </summary>
		/// <param name="builder"><see cref="StringBuilder"/> to write to.</param>
		/// <param name="typeArguments">Type arguments.</param>
		/// <exception cref="InvalidOperationException">Pointers can't be used as generic arguments.</exception>
		public static StringBuilder WriteGenericName(this StringBuilder builder, IEnumerable<ITypeSymbol> typeArguments)
		{
			if (typeArguments is IEnumerable<ITypeParameterSymbol> parameters)
			{
				return builder.WriteGenericName(parameters);
			}

			ITypeSymbol[] symbols = typeArguments.ToArray();

			if (symbols.Length == 0)
			{
				return builder;
			}

			CodeBuilder cb = new(builder);

			builder.Append('<');

			foreach (ITypeSymbol argument in symbols)
			{
				if (argument is null)
				{
					continue;
				}

				if (argument is IPointerTypeSymbol or IFunctionPointerTypeSymbol)
				{
					throw new InvalidOperationException("Pointers can't be used as generic arguments!");
				}

				cb.Type(argument);

				builder.Append(", ");
			}

			builder.Remove(builder.Length - 2, 2);
			builder.Append('>');
			return builder;
		}

		/// <summary>
		/// Writes a <see cref="string"/> containing generic identifier combined of the specified <paramref name="name"/> and the collection of <paramref name="typeArguments"/> to the specified <paramref name="builder"/>.
		/// </summary>
		/// <param name="builder"><see cref="StringBuilder"/> to write to.</param>
		/// <param name="typeArguments">Type arguments.</param>
		/// <param name="name">Actual member identifier.</param>
		public static StringBuilder WriteGenericName(this StringBuilder builder, IEnumerable<ITypeSymbol> typeArguments, string? name)
		{
			if (typeArguments is IEnumerable<ITypeParameterSymbol> parameters)
			{
				return builder.WriteGenericName(parameters, name);
			}

			if (!string.IsNullOrWhiteSpace(name))
			{
				builder.Append(name);
			}

			return builder.WriteGenericName(typeArguments);
		}

		/// <summary>
		/// Writes a <see cref="string"/> representing a dot-separated name to the specified <paramref name="builder"/>.
		/// </summary>
		/// <param name="builder"><see cref="StringBuilder"/> to write to.</param>
		/// <param name="parts">Parts of the name. Each part will be separated by a dot.</param>
		public static void WriteName(this StringBuilder builder, params string[]? parts)
		{
			if (parts is null || parts.Length == 0)
			{
				return;
			}

			foreach (string part in parts)
			{
				if (string.IsNullOrWhiteSpace(part))
				{
					continue;
				}

				builder.Append(part).Append('.');
			}

			builder.Remove(builder.Length - 2, 1);
		}

		/// <summary>
		/// Writes a <see cref="string"/> that is created by joining the names of parent namespaces of the provided <paramref name="namespace"/> to the specified <paramref name="builder"/>.
		/// </summary>
		/// <param name="builder"><see cref="StringBuilder"/> to write to.</param>
		/// <param name="namespace">A collection of <see cref="INamespaceSymbol"/>s create the <see cref="string"/> from.</param>
		/// <param name="includeSelf">Determines whether to include name of the <paramref name="namespace"/>.</param>
		public static StringBuilder WriteNamespace(this StringBuilder builder, INamespaceSymbol @namespace, bool includeSelf = true)
		{
			builder.WriteNamespaces(@namespace.GetContainingNamespaces());

			if (includeSelf)
			{
				builder.Append('.').Append(@namespace.Name);
			}

			return builder;
		}

		/// <summary>
		/// Writes a <see cref="string"/> that is created by joining the names of the provided <paramref name="namespaces"/> to the specified <paramref name="builder"/>.
		/// </summary>
		/// <param name="builder"><see cref="StringBuilder"/> to write to.</param>
		/// <param name="namespaces">A collection of <see cref="INamespaceSymbol"/>s create the <see cref="string"/> from.</param>
		public static StringBuilder WriteNamespaces(this StringBuilder builder, IEnumerable<INamespaceSymbol> namespaces)
		{
			bool any = false;

			foreach (INamespaceSymbol n in namespaces)
			{
				if (n is null)
				{
					continue;
				}

				any = true;

				builder.Append(n.Name).Append('.');
			}

			if (any)
			{
				builder.Remove(builder.Length - 1, 1);
			}

			return builder;
		}

		/// <summary>
		/// Writes a <see cref="string"/> that is created by joining the names of the namespaces the provided <paramref name="symbol"/> is contained in to the specified <paramref name="builder"/>.
		/// </summary>
		/// <param name="builder"><see cref="StringBuilder"/> to write to.</param>
		/// <param name="symbol"><see cref="ISymbol"/> to get the containing namespaces of.</param>
		/// <param name="includeSelf">Determines whether to include name of the <paramref name="symbol"/> if its a <see cref="INamespaceSymbol"/>.</param>
		public static StringBuilder WriteNamespacesOf(this StringBuilder builder, ISymbol symbol, bool includeSelf = true)
		{
			if (symbol is INamespaceSymbol @namespace)
			{
				return builder.WriteNamespace(@namespace, includeSelf);
			}

			if (symbol.ContainingNamespace is null || symbol.ContainingNamespace.IsGlobalNamespace)
			{
				return builder;
			}

			return builder.WriteNamespaces(symbol.GetContainingNamespaces());
		}

		/// <summary>
		/// Writes a <see cref="string"/> that represents the parameter signature of the <paramref name="method"/> to the specified <paramref name="builder"/>.
		/// </summary>
		/// <param name="builder"><see cref="StringBuilder"/> to write to.</param>
		/// <param name="method"><see cref="IMethodSymbol"/> to get the signature of.</param>
		/// <param name="substitution">Configures how generic type parameters are substituted.</param>
		public static StringBuilder WriteParameterList(this StringBuilder builder, IMethodSymbol method, GenericSubstitution substitution = default)
		{
			ImmutableArray<IParameterSymbol> parameters = substitution.HasFlag(GenericSubstitution.TypeArguments) || method.ConstructedFrom is null
				? method.Parameters
				: method.ConstructedFrom.Parameters;

			CodeBuilder cd = new(builder);

			builder.Append('(');

			if (parameters.Length > 0)
			{
				cd.Parameter(parameters[0]);

				for (int i = 1; i < parameters.Length; i++)
				{
					builder.Append(", ");
					cd.Parameter(parameters[i]);
				}
			}

			builder.Append(')');
			return builder;
		}

		/// <summary>
		/// Writes a <see cref="string"/> that contains all the parent types of the specified <paramref name="symbol"/> and the <paramref name="symbol"/>'s separated by the dot ('.') character to the specified <paramref name="builder"/>. Can be used in XML documentation.
		/// </summary>
		/// <param name="builder"><see cref="StringBuilder"/> to write to.</param>
		/// <param name="symbol"><see cref="ISymbol"/> to get the <see cref="string"/> of.</param>
		/// <param name="includeSelf">Determines whether to include the <paramref name="symbol"/> in the returned <see cref="string"/>.</param>
		/// <param name="includeParameters">If the value of <paramref name="symbol"/> is a <see cref="IMethodSymbol"/>, determines whether to include the method's parameters in the returned <see cref="string"/>.</param>
		public static StringBuilder WriteXmlContainingTypes(this StringBuilder builder, ISymbol symbol, bool includeSelf = true, bool includeParameters = false)
		{
			foreach (INamedTypeSymbol type in symbol.GetContainingTypes())
			{
				builder.Append(AnalysisUtilities.ToXmlCompatible(type.GetGenericName())).Append('.');
			}

			if (includeSelf)
			{
				builder.Append(symbol.GetXmlCompatibleName(includeParameters));
			}
			else if (builder.Length > 0)
			{
				builder.Remove(builder.Length - 1, 1);
			}

			return builder;
		}
	}
}
