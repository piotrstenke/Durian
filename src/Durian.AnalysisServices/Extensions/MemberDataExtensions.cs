// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Durian.Analysis.Data;
using Microsoft.CodeAnalysis;
using System;
using System.Globalization;
using System.Text;

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

			StringBuilder sb = new();

			foreach (ITypeData type in member.GetContainingTypes())
			{
				sb.Append(type.Symbol.GetGenericName()).Append('.');
			}

			if (includeSelf)
			{
				sb.Append(member.Symbol.GetGenericName(includeParameters ? GenericSubstitution.ParameterList : GenericSubstitution.None));
			}
			else if (sb.Length > 0)
			{
				sb.Remove(sb.Length - 1, 1);
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

			return AnalysisUtilities.ToXmlCompatible(member.Symbol.ToString());
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
			if (member is null)
			{
				throw new ArgumentNullException(nameof(member));
			}

			StringBuilder sb = new();

			foreach (ITypeData type in member.GetContainingTypes())
			{
				sb.Append(AnalysisUtilities.ToXmlCompatible(type.Symbol.GetGenericName())).Append('.');
			}

			if (includeSelf)
			{
				sb.Append(member.Symbol.GetXmlCompatibleName(includeParameters));
			}
			else if (sb.Length > 0)
			{
				sb.Remove(sb.Length - 1, 1);
			}

			return sb.ToString();
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
