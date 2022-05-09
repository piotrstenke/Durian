// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Globalization;
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
		/// Gets the XML comment associated with the specified <paramref name="member"/>.
		/// </summary>
		/// <param name="member"><see cref="IMemberData"/> to get the documentation of.</param>
		/// <param name="preferredCulture">Preferred <see cref="CultureInfo"/>.</param>
		public static string? GetDocumentation(this IMemberData member, CultureInfo? preferredCulture = default)
		{
			return member.Symbol.GetDocumentationCommentXml(preferredCulture ?? CultureInfo.InvariantCulture);
		}

		/// <summary>
		/// Returns a <see cref="string"/> that contains all the parent types of the specified <paramref name="member"/> and the <paramref name="member"/>'s name separated by the dot ('.') character.
		/// </summary>
		/// <remarks>If the <paramref name="member"/> is not contained within a type, an empty <see cref="string"/> is returned instead.</remarks>
		/// <param name="member"><see cref="IMemberData"/> to get the <see cref="string"/> of.</param>
		/// <param name="includeSelf">Determines whether to include the <paramref name="member"/> in the returned <see cref="string"/>.</param>
		/// <param name="includeParameters">If the value of the <see cref="IMemberData.Symbol"/> property of the <paramref name="member"/> parameter is a <see cref="IMethodSymbol"/>, determines whether to include the method's parameters in the returned <see cref="string"/>.</param>
		public static string GetContainingTypesAsString(this IMemberData member, bool includeSelf = true, bool includeParameters = false)
		{
			StringBuilder builder = new();
			builder.WriteContainingTypes(member, includeSelf, includeParameters);
			return builder.ToString();
		}

		/// <summary>
		/// Returns a <see cref="string"/> representing the fully qualified name of the <paramref name="member"/> that can be used in the XML documentation.
		/// </summary>
		/// <param name="member"><see cref="IMemberData"/> to get the fully qualified name of.</param>
		public static string GetXmlFullyQualifiedName(this IMemberData member)
		{
			return AnalysisUtilities.ToXmlCompatible(member.Symbol.ToString());
		}

		/// <summary>
		/// Returns a <see cref="string"/> that contains all the parent types of the specified <paramref name="member"/> and the <paramref name="member"/>'s separated by the dot ('.') character. Can be used in XML documentation.
		/// </summary>
		/// <param name="member"><see cref="IMemberData"/> to get the <see cref="string"/> of.</param>
		/// <param name="includeSelf">Determines whether to include the <paramref name="member"/> in the returned <see cref="string"/>.</param>
		/// <param name="includeParameters">If the value of the <see cref="IMemberData.Symbol"/> property of the <paramref name="member"/> parameter is a <see cref="IMethodSymbol"/>, determines whether to include the method's parameters in the returned <see cref="string"/>.</param>
		public static string GetXmlContainingTypes(this IMemberData member, bool includeSelf = true, bool includeParameters = false)
		{
			StringBuilder builder = new();
			builder.WriteContainingTypes(member, includeSelf, includeParameters);
			return builder.ToString();
		}

		/// <summary>
		/// Returns full namespace of the target <paramref name="member"/>.
		/// </summary>
		/// <param name="member"><see cref="IMemberData"/> to get the parent namespace of.</param>
		/// <param name="includeSelf">Determines whether to include name of the <paramref name="member"/> if it represents a <see cref="INamespaceSymbol"/>.</param>
		public static string? JoinNamespaces(this IMemberData member, bool includeSelf = true)
		{
			StringBuilder builder = new();
			builder.WriteNamespacesOf(member, includeSelf);
			return builder.ToString();
		}
	}
}
