// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System.Globalization;
using Durian.Analysis.Data;

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
