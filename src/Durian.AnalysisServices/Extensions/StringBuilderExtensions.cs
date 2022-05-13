// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Durian.Analysis.Data;
using Microsoft.CodeAnalysis;

namespace Durian.Analysis.Extensions
{
	/// <summary>
	/// Contains extension methods for the <see cref="StringBuilder"/> class.
	/// </summary>
	internal static class StringBuilderExtensions
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
		/// Writes a <see cref="string"/> that contains all the parent types of the specified <paramref name="member"/> and the <paramref name="member"/>'s name separated by the dot ('.') character to the specified <paramref name="builder"/>.
		/// </summary>
		/// <remarks>If the <paramref name="member"/> is not contained within a type, an empty <see cref="string"/> is returned instead.</remarks>
		/// <param name="builder"><see cref="StringBuilder"/> to write to.</param>
		/// <param name="member"><see cref="IMemberData"/> to get the <see cref="string"/> of.</param>
		/// <param name="includeSelf">Determines whether to include the <paramref name="member"/> in the returned <see cref="string"/>.</param>
		/// <param name="includeParameters">If the value of the <see cref="IMemberData.Symbol"/> property of the <paramref name="member"/> parameter is a <see cref="IMethodSymbol"/>, determines whether to include the method's parameters in the returned <see cref="string"/>.</param>
		/// <exception cref="ArgumentNullException"><paramref name="member"/> is <see langword="null"/>.</exception>
		public static StringBuilder WriteContainingTypes(this StringBuilder builder, IMemberData member, bool includeSelf = true, bool includeParameters = false)
		{
			foreach (INamedTypeSymbol type in member.GetContainingTypes())
			{
				builder.WriteGenericName(type);
				builder.Append('.');
			}

			if (includeSelf)
			{
				builder.WriteGenericName(member.Symbol, includeParameters ? GenericSubstitution.ParameterList : GenericSubstitution.None);
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
		/// Writes a <see cref="string"/> representing a dot-separated name to the specified <paramref name="builder"/>.
		/// </summary>
		/// <param name="builder"><see cref="StringBuilder"/> to write to.</param>
		/// <param name="parts">Parts of the name. Each part will be separated by a dot.</param>
		public static StringBuilder WriteName(this StringBuilder builder, params string[]? parts)
		{
			if (parts is null || parts.Length == 0)
			{
				return builder;
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

		/// <summary>
		/// Writes a <see cref="string"/> that contains all the parent types of the specified <paramref name="member"/> and the <paramref name="member"/>'s separated by the dot ('.') character to the specified <paramref name="builder"/>. Can be used in XML documentation.
		/// </summary>
		/// <param name="builder"><see cref="StringBuilder"/> to write to.</param>
		/// <param name="member"><see cref="IMemberData"/> to get the <see cref="string"/> of.</param>
		/// <param name="includeSelf">Determines whether to include the <paramref name="member"/> in the returned <see cref="string"/>.</param>
		/// <param name="includeParameters">If the value of the <see cref="IMemberData.Symbol"/> property of the <paramref name="member"/> parameter is a <see cref="IMethodSymbol"/>, determines whether to include the method's parameters in the returned <see cref="string"/>.</param>
		/// <exception cref="ArgumentNullException"><paramref name="member"/> is <see langword="null"/>.</exception>
		public static StringBuilder WriteXmlContainingTypes(this StringBuilder builder, IMemberData member, bool includeSelf = true, bool includeParameters = false)
		{
			foreach (ITypeData type in member.GetContainingTypes())
			{
				builder.Append(AnalysisUtilities.ToXmlCompatible(type.Symbol.GetGenericName())).Append('.');
			}

			if (includeSelf)
			{
				builder.Append(member.Symbol.GetXmlCompatibleName(includeParameters));
			}
			else if (builder.Length > 0)
			{
				builder.Remove(builder.Length - 1, 1);
			}

			return builder;
		}
	}
}
