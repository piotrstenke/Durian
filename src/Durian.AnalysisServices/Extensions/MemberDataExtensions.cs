// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Durian.Analysis.Data;
using Microsoft.CodeAnalysis;
using System;
using System.Globalization;

namespace Durian.Analysis.Extensions
{
	/// <summary>
	/// Contains various extension methods for the <see cref="IMemberData"/> interface.
	/// </summary>
	public static class MemberDataExtensions
	{
		/// <summary>
		/// Gets the XML comment associated with the specified <paramref name="member"/>.
		/// </summary>
		/// <param name="member"><see cref="IMemberData"/> to get the documentation of.</param>
		/// <param name="preferredCulture">Preferred <see cref="CultureInfo"/>.</param>
		/// <exception cref="ArgumentNullException"><paramref name="member"/> is <see langword="null"/>.</exception>
		public static string? GetDocumentation(this IMemberData member, CultureInfo? preferredCulture = default)
		{
			if (member is null)
			{
				throw new ArgumentNullException(nameof(member));
			}

			return member.Symbol.GetDocumentationCommentXml(preferredCulture ?? CultureInfo.InvariantCulture);
		}

		/// <summary>
		/// Returns the generic identifier of the specified <paramref name="type"/>.
		/// </summary>
		/// <param name="type"><see cref="ITypeData"/> to get the generic name of.</param>
		/// <param name="substitution">Configures how generic type parameters are substituted.</param>
		/// <returns>If the <paramref name="type"/> has no type parameters, returns the name of the <paramref name="type"/> instead.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="type"/> is <see langword="null"/>.</exception>
		public static string GetGenericName(this ITypeData type, GenericSubstitution substitution = default)
		{
			if (type is null)
			{
				throw new ArgumentNullException(nameof(type));
			}

			return type.Symbol.GetGenericName(substitution);
		}

		/// <summary>
		/// Returns the generic identifier of the specified <paramref name="method"/>.
		/// </summary>
		/// <param name="method"><see cref="IMethodData"/> to get the generic name of.</param>
		/// <param name="substitution">Configures how generic type parameters are substituted.</param>
		/// <returns>If the <paramref name="method"/> has no type parameters, returns the name of the <paramref name="method"/> instead.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="method"/> is <see langword="null"/>.</exception>
		public static string GetGenericName(this IMethodData method, GenericSubstitution substitution = default)
		{
			if (method is null)
			{
				throw new ArgumentNullException(nameof(method));
			}

			return method.Symbol.GetGenericName(substitution);
		}

		/// <summary>
		/// Returns the generic identifier of the specified <paramref name="member"/>.
		/// </summary>
		/// <param name="member"><see cref="IMemberData"/> to get the generic name of.</param>
		/// <param name="substitution">Configures how generic type parameters are substituted.</param>
		/// <exception cref="ArgumentNullException"><paramref name="member"/> is <see langword="null"/>.</exception>
		public static string GetGenericName(this IMemberData member, GenericSubstitution substitution = default)
		{
			if (member is null)
			{
				throw new ArgumentNullException(nameof(member));
			}

			return member.Symbol.GetGenericName(substitution);
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

			return member.Symbol.GetInheritdocIfHasDocumentation();
		}

		/// <summary>
		/// Returns a <see cref="string"/> that contains all the parent types of the specified <paramref name="member"/> and the <paramref name="member"/>'s name separated by the dot ('.') character.
		/// </summary>
		/// <remarks>If the <paramref name="member"/> is not contained within a type, an empty <see cref="string"/> is returned instead.</remarks>
		/// <param name="member"><see cref="IMemberData"/> to get the <see cref="string"/> of.</param>
		/// <param name="includeSelf">Determines whether to include the <paramref name="member"/> in the returned <see cref="string"/>.</param>
		/// <param name="includeParameters">If the value of the <see cref="IMemberData.Symbol"/> property of the <paramref name="member"/> parameter is a <see cref="IMethodSymbol"/>, determines whether to include the method's parameters in the returned <see cref="string"/>.</param>
		/// <exception cref="ArgumentNullException"><paramref name="member"/> is <see langword="null"/>.</exception>
		public static string GetParentTypesString(this IMemberData member, bool includeSelf = true, bool includeParameters = false)
		{
			if (member is null)
			{
				throw new ArgumentNullException(nameof(member));
			}

			return member.Symbol.GetParentTypesString(includeSelf, includeParameters);
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
		/// <param name="includeSelf">Determines whether to include the <paramref name="member"/> in the returned <see cref="string"/>.</param>
		/// <param name="includeParameters">If the value of the <see cref="IMemberData.Symbol"/> property of the <paramref name="member"/> parameter is a <see cref="IMethodSymbol"/>, determines whether to include the method's parameters in the returned <see cref="string"/>.</param>
		/// <exception cref="ArgumentNullException"><paramref name="member"/> is <see langword="null"/>.</exception>
		public static string GetXmlParentTypesString(this IMemberData member, bool includeSelf = true, bool includeParameters = false)
		{
			string parentString = GetParentTypesString(member, includeSelf, includeParameters);

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
