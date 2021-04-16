using System;
using System.Text;
using Durian.Data;
using Microsoft.CodeAnalysis;

namespace Durian.Extensions
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
		/// <returns>If the <paramref name="type"/> has no type parameters, returns the name of the <paramref name="type"/> instead.</returns>
		public static string GetGenericName(this ITypeData type)
		{
			if (type is null)
			{
				throw new ArgumentNullException(nameof(type));
			}

			return type.Symbol.GetGenericName(true);
		}

		/// <summary>
		/// Returns the generic identifier of the specified <paramref name="member"/>.
		/// </summary>
		/// <param name="member"><see cref="IMemberData"/> to get the generic name of.</param>
		/// <param name="includeParameters">If the value of the <see cref="IMemberData.Symbol"/> property of the <paramref name="member"/> parameter is a <see cref="IMethodSymbol"/>, determines whether to include the method's parameters in the returned <see cref="string"/>.</param>
		/// <returns>If the <see cref="IMemberData.Symbol"/> is not of type <see cref="INamedTypeSymbol"/> or <see cref="IMethodSymbol"/> or the symbol has no type parameters, returns the name of the symbol instead.</returns>
		public static string GetGenericName(this IMemberData member, bool includeParameters = false)
		{
			if (member is null)
			{
				throw new ArgumentNullException(nameof(member));
			}

			if (member.Symbol is INamedTypeSymbol type)
			{
				return type.GetGenericName(true, includeParameters);
			}
			else if (member.Symbol is IMethodSymbol method)
			{
				return method.GetGenericName(true, includeParameters);
			}

			return member.Symbol.Name;
		}

		/// <summary>
		/// Returns a <see cref="string"/> that contains all the parent types of the specified <paramref name="member"/> and the <paramref name="member"/>'s name separated by the dot ('.') character.
		/// </summary>
		/// <remarks>If the <paramref name="member"/> is not contained within a type, an empty <see cref="string"/> is returned instead.</remarks>
		/// <param name="member"><see cref="IMemberData"/> to get the <see cref="string"/> of.</param>
		/// <param name="includeParameters">If the value of the <see cref="IMemberData.Symbol"/> property of the <paramref name="member"/> parameter is a <see cref="IMethodSymbol"/>, determines whether to include the method's parameters in the returned <see cref="string"/>.</param>
		/// <exception cref="ArgumentNullException"><paramref name="member"/> was <c>null</c>.</exception>
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
		/// Returns a <see cref="string"/> that contains all the parent types of the specified <paramref name="member"/> and the <paramref name="member"/>'s separated by the dot ('.') character. Can be used in XML documentation.
		/// </summary>
		/// <param name="member"><see cref="IMemberData"/> to get the <see cref="string"/> of.</param>
		/// <param name="includeParameters">If the value of the <see cref="IMemberData.Symbol"/> property of the <paramref name="member"/> parameter is a <see cref="IMethodSymbol"/>, determines whether to include the method's parameters in the returned <see cref="string"/>.</param>
		/// <exception cref="ArgumentNullException"><paramref name="member"/> was <c>null</c>.</exception>
		public static string GetXmlParentTypesString(this IMemberData member, bool includeParameters = false)
		{
			return AnalysisUtilities.ConvertFullyQualifiedNameToXml(GetParentTypesString(member, includeParameters));
		}

		/// <summary>
		/// Returns a <see cref="string"/> representing the fully qualified name of the <paramref name="member"/> that can be used in the XML documentation.
		/// </summary>
		/// <param name="member"><see cref="IMemberData"/> to get the fully qualified name of.</param>
		/// <exception cref="ArgumentNullException"><paramref name="member"/> was <c>null</c>.</exception>
		public static string GetXmlFullyQualifiedName(this IMemberData member)
		{
			if (member is null)
			{
				throw new ArgumentNullException(nameof(member));
			}

			return AnalysisUtilities.ConvertFullyQualifiedNameToXml(member.Symbol.ToString());
		}

		/// <summary>
		/// Returns full namespace of the target <paramref name="member"/>.
		/// </summary>
		/// <param name="member"><see cref="IMemberData"/> to get the parent namespace of.</param>
		/// <returns>The full namespace of the target <paramref name="member"/>. -or- <c>null</c> if the <paramref name="member"/> is not contained withing a namespace. -or- <paramref name="member"/> is contained within global namespace.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="member"/> was <c>null</c>.</exception>
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
