// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Text;
using Durian.Analysis.Data;
using Microsoft.CodeAnalysis;

namespace Durian.Analysis.Extensions
{
	/// <summary>
	/// Contains various extension methods for the <see cref="IMemberData"/> interface.
	/// </summary>
	public static class MemberDataExtensions
	{
		/// <summary>
		/// Returns the generic identifier of the specified <paramref name="type"/>.
		/// </summary>
		/// <param name="type"><see cref="ITypeData"/> to get the generic name of.</param>
		/// <param name="includeVariance">Determines whether to include variance of the <paramref name="type"/>'s type parameters.</param>
		/// <returns>If the <paramref name="type"/> has no type parameters, returns the name of the <paramref name="type"/> instead.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="type"/> is <see langword="null"/>.</exception>
		public static string GetGenericName(this ITypeData type, bool includeVariance = false)
		{
			if (type is null)
			{
				throw new ArgumentNullException(nameof(type));
			}

			return type.Symbol.GetGenericName(true, includeVariance);
		}

		/// <summary>
		/// Returns the generic identifier of the specified <paramref name="member"/>.
		/// </summary>
		/// <param name="member"><see cref="IMemberData"/> to get the generic name of.</param>
		/// <param name="includeParameters">If the value of the <see cref="IMemberData.Symbol"/> property of the <paramref name="member"/> parameter is a <see cref="IMethodSymbol"/>, determines whether to include the method's parameters in the returned <see cref="string"/>.</param>
		/// <returns>If the <see cref="IMemberData.Symbol"/> is not of type <see cref="INamedTypeSymbol"/> or <see cref="IMethodSymbol"/> or the symbol has no type parameters, returns the name of the symbol instead.</returns>
		/// <param name="includeVariance">Determines whether to include variance of the <paramref name="member"/>'s type parameters.</param>
		/// <exception cref="ArgumentNullException"><paramref name="member"/> is <see langword="null"/>.</exception>
		public static string GetGenericName(this IMemberData member, bool includeParameters = false, bool includeVariance = false)
		{
			if (member is null)
			{
				throw new ArgumentNullException(nameof(member));
			}

			if (member.Symbol is INamedTypeSymbol type)
			{
				return type.GetGenericName(true, includeParameters, includeVariance);
			}
			else if (member.Symbol is IMethodSymbol method)
			{
				return method.GetGenericName(true, includeParameters);
			}

			return member.Symbol.Name;
		}

		/// <summary>
		/// Creates an <c>&lt;inheritdoc/&gt;</c> tag from the specified <paramref name="member"/>.
		/// </summary>
		/// <param name="member"><see cref="IMemberData"/> to get the <c>&lt;inheritdoc/&gt;</c> tag from.</param>
		/// <returns>A <see cref="string"/> containing the created <c>&lt;inheritdoc/&gt;</c> tag -or- <see langword="null"/> if <paramref name="member"/> has no documentation comment.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="member"/> is <see langword="null"/>.</exception>
		public static string? GetInheritdocIfHasDocumentation(this IMemberData member)
		{
			if (member is null)
			{
				throw new ArgumentNullException(nameof(member));
			}

			if (string.IsNullOrEmpty(member.Symbol.GetDocumentationCommentXml()))
			{
				return null;
			}
			else
			{
				return AutoGenerated.GetInheritdoc(member.GetXmlParentTypesString(true));
			}
		}

		/// <summary>
		/// Returns a <see cref="string"/> that contains all the parent types of the specified <paramref name="member"/> and the <paramref name="member"/>'s name separated by the dot ('.') character.
		/// </summary>
		/// <remarks>If the <paramref name="member"/> is not contained within a type, an empty <see cref="string"/> is returned instead.</remarks>
		/// <param name="member"><see cref="IMemberData"/> to get the <see cref="string"/> of.</param>
		/// <param name="includeParameters">If the value of the <see cref="IMemberData.Symbol"/> property of the <paramref name="member"/> parameter is a <see cref="IMethodSymbol"/>, determines whether to include the method's parameters in the returned <see cref="string"/>.</param>
		/// <exception cref="ArgumentNullException"><paramref name="member"/> is <see langword="null"/>.</exception>
		public static string GetParentTypesString(this IMemberData member, bool includeParameters = false)
		{
			if (member is null)
			{
				throw new ArgumentNullException(nameof(member));
			}

			StringBuilder sb = new();

			foreach (ITypeData type in member.GetContainingTypes())
			{
				sb.Append(GetGenericName(type)).Append('.');
			}

			sb.Append(GetGenericName(member));

			if (includeParameters && member.Symbol is IMethodSymbol m)
			{
				sb.Append(m.GetParameterSignature());
			}

			return sb.ToString();
		}

		/// <summary>
		/// Returns a <see cref="string"/> representing the fully qualified name of the <paramref name="member"/> that can be used in the XML documentation.
		/// </summary>
		/// <param name="member"><see cref="IMemberData"/> to get the fully qualified name of.</param>
		/// <exception cref="ArgumentNullException"><paramref name="member"/> is <see langword="null"/>.</exception>
		public static string GetXmlFullyQualifiedName(this IMemberData member)
		{
			if (member is null)
			{
				throw new ArgumentNullException(nameof(member));
			}

			return AnalysisUtilities.ConvertFullyQualifiedNameToXml(member.Symbol.ToString());
		}

		/// <summary>
		/// Returns a <see cref="string"/> that contains all the parent types of the specified <paramref name="member"/> and the <paramref name="member"/>'s separated by the dot ('.') character. Can be used in XML documentation.
		/// </summary>
		/// <param name="member"><see cref="IMemberData"/> to get the <see cref="string"/> of.</param>
		/// <param name="includeParameters">If the value of the <see cref="IMemberData.Symbol"/> property of the <paramref name="member"/> parameter is a <see cref="IMethodSymbol"/>, determines whether to include the method's parameters in the returned <see cref="string"/>.</param>
		/// <exception cref="ArgumentNullException"><paramref name="member"/> is <see langword="null"/>.</exception>
		public static string GetXmlParentTypesString(this IMemberData member, bool includeParameters = false)
		{
			string parentString = GetParentTypesString(member, includeParameters);

			return AnalysisUtilities.ConvertFullyQualifiedNameToXml(parentString);
		}

		/// <summary>
		/// Returns full namespace of the target <paramref name="member"/>.
		/// </summary>
		/// <param name="member"><see cref="IMemberData"/> to get the parent namespace of.</param>
		/// <returns>The full namespace of the target <paramref name="member"/>. -or- <see langword="null"/> if the <paramref name="member"/> is not contained withing a namespace. -or- <paramref name="member"/> is contained within global namespace.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="member"/> is <see langword="null"/>.</exception>
		public static string? JoinNamespaces(this IMemberData member)
		{
			if (member is null)
			{
				throw new ArgumentNullException(nameof(member));
			}

			return member.GetContainingNamespaces().JoinNamespaces();
		}
	}
}