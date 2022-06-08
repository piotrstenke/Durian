// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;
using System.Globalization;
using Durian.Analysis.Data;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.Analysis.Extensions
{
	/// <summary>
	/// Contains various extension methods for the <see cref="IMemberData"/> interface.
	/// </summary>
	public static class MemberDataExtensions
	{
		/// <summary>
		/// Returns an <see cref="AttributeData"/> associated with the <paramref name="syntax"/> defined on the specified <paramref name="member"/>.
		/// </summary>
		/// <param name="member">Target <see cref="ISymbol"/>.</param>
		/// <param name="syntax"><see cref="AttributeSyntax"/> to get the data of.</param>
		/// <returns>The <see cref="AttributeData"/> associated with the <paramref name="syntax"/>. -or- <see langword="null"/> if no such <see cref="AttributeData"/> found.</returns>
		public static AttributeData? GetAttribute(this IMemberData member, AttributeSyntax syntax)
		{
			foreach (AttributeData attr in member.Attributes)
			{
				SyntaxReference? reference = attr.ApplicationSyntaxReference;

				if (reference is null)
				{
					continue;
				}

				if (reference.Span == syntax.Span)
				{
					return attr;
				}
			}

			return null;
		}

		/// <summary>
		/// Returns a collection of <see cref="AttributeData"/>s associated with the <paramref name="attrSymbol"/> and defined on the specified <paramref name="member"/>.
		/// </summary>
		/// <param name="member">Target <see cref="ISymbol"/>.</param>
		/// <param name="attrSymbol">Type of attributes to look for.</param>
		public static IEnumerable<AttributeData> GetAttributes(this IMemberData member, INamedTypeSymbol attrSymbol)
		{
			foreach (AttributeData attr in member.Attributes)
			{
				if (SymbolEqualityComparer.Default.Equals(attr.AttributeClass, attrSymbol))
				{
					yield return attr;
				}
			}
		}

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
		/// Returns fully qualified name of the specified <paramref name="member"/>.
		/// </summary>
		/// <param name="member"><see cref="IMemberData"/> to get the fully qualified name of.</param>
		/// <param name="format">Determines format of the returned qualified name.</param>
		public static string GetFullyQualifiedName(this IMemberData member, QualifiedName format = default)
		{
			if (format == QualifiedName.Metadata)
			{
				return member.ToString();
			}

			CodeBuilder builder = new(false);
			builder.QualifiedName(member);
			string value = builder.ToString();

			if (format == QualifiedName.Xml)
			{
				return AnalysisUtilities.ToXmlCompatible(value);
			}

			return value;
		}
	}
}
