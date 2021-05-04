using System.Globalization;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Durian
{
	/// <summary>
	/// Provides methods for creating new, more specific <see cref="DiagnosticDescriptor"/>s from those that can be found in the <see cref="Descriptors"/> class.
	/// </summary>
	public static class DescriptorFactory
	{
		/// <summary>
		/// Creates a new instance of <see cref="DiagnosticDescriptor"/> of the DUR0010 rule with the specified <paramref name="attributeName"/> and <paramref name="modifier"/>.
		/// <para>See: <see cref="Descriptors.MemberWithAttributeMustHaveModifier"/></para>
		/// </summary>
		/// <param name="attributeName">Name of the attribute the rule applies to.</param>
		/// <param name="modifier">Modifier the rule applies to.</param>
		public static DiagnosticDescriptor MemberWithAttributeMustHaveModifier(string attributeName, string modifier)
		{
			return CopyDescriptor(
				Descriptors.MemberWithAttributeMustHaveModifier,
				("'memberType'", "Member"),
				(nameof(attributeName), attributeName),
				("'modifier'", modifier)
			);
		}

		/// <summary>
		/// Creates a new instance of <see cref="DiagnosticDescriptor"/> of the DUR0010 rule with the specified <paramref name="attributeName"/>, <paramref name="modifier"/> and <paramref name="memberType"/>.
		/// <para>See: <see cref="Descriptors.MemberWithAttributeMustHaveModifier"/></para>
		/// </summary>
		/// <param name="attributeName">Name of the attribute the rule applies to.</param>
		/// <param name="modifier">Modifier the rule applies to.</param>
		/// <param name="memberType">Type of the member the rule applies to.</param>
		public static DiagnosticDescriptor MemberWithAttributeMustHaveModifier(string attributeName, string modifier, string memberType)
		{
			return CopyDescriptor(
				Descriptors.MemberWithAttributeMustHaveModifier,
				("'memberType'", memberType),
				(nameof(attributeName), attributeName),
				("'modifier'", modifier)
			);
		}

		/// <summary>
		/// Creates a new instance of <see cref="DiagnosticDescriptor"/> of the DUR0011 rule with the specified <paramref name="attributeName"/> and <paramref name="modifier"/>.
		/// </summary>
		/// <para>See: <see cref="Descriptors.MemberWithAttributeCannotHaveModifier"/></para>
		/// <param name="attributeName">Name of the attribute the rule applies to.</param>
		/// <param name="modifier">Modifier the rule applies to.</param>
		public static DiagnosticDescriptor MemberWithAttributeCannotHaveModifier(string attributeName, string modifier)
		{
			return CopyDescriptor(
				Descriptors.MemberWithAttributeCannotHaveModifier,
				("'memberType'", "Member"),
				(nameof(attributeName), attributeName),
				("'modifier'", modifier)
			);
		}

		/// <summary>
		/// Creates a new instance of <see cref="DiagnosticDescriptor"/> of the DUR00011 rule with the specified <paramref name="attributeName"/>, <paramref name="modifier"/> and <paramref name="memberType"/>.
		/// <para>See: <see cref="Descriptors.MemberWithAttributeCannotHaveModifier"/></para>
		/// </summary>
		/// <param name="attributeName">Name of the attribute the rule applies to.</param>
		/// <param name="modifier">Modifier the rule applies to.</param>
		/// <param name="memberType">Type of the member the rule applies to.</param>
		public static DiagnosticDescriptor MemberWithAttributeCannotHaveModifier(string attributeName, string modifier, string memberType)
		{
			return CopyDescriptor(
				Descriptors.MemberWithAttributeCannotHaveModifier,
				("'memberType'", memberType),
				(nameof(attributeName), attributeName),
				("'modifier'", modifier)
			);
		}

		/// <summary>
		/// Creates a new instance of <see cref="DiagnosticDescriptor"/> of the DUR0012 rule with the specified <paramref name="attributeName"/>.
		/// <para>See: <see cref="Descriptors.MemberWithAttributeMustHaveImplementation"/></para>
		/// </summary>
		/// <param name="attributeName">Name of the attribute the rule applies to.</param>
		public static DiagnosticDescriptor MemberWithAttributeMustHaveImplementation(string attributeName)
		{
			return CopyDescriptor(
				Descriptors.MemberWithAttributeMustHaveImplementation,
				("'memberType'", "Member"),
				(nameof(attributeName), attributeName)
			);
		}

		/// <summary>
		/// Creates a new instance of <see cref="DiagnosticDescriptor"/> of the DUR0012 rule with the specified <paramref name="attributeName"/> and <paramref name="memberType"/>.
		/// <para>See: <see cref="Descriptors.MemberWithAttributeMustHaveImplementation"/></para>
		/// </summary>
		/// <param name="attributeName">Name of the attribute the rule applies to.</param>
		/// <param name="memberType">Type of the member the rule applies to.</param>
		public static DiagnosticDescriptor MemberWithAttributeMustHaveImplementation(string attributeName, string memberType)
		{
			return CopyDescriptor(
				Descriptors.MemberWithAttributeMustHaveImplementation,
				("'memberType'", memberType),
				(nameof(attributeName), attributeName)
			);
		}

		/// <summary>
		/// Creates a new instance of <see cref="DiagnosticDescriptor"/> of the DUR0013 rule with the specified <paramref name="attributeName"/>.
		/// <para>See: <see cref="Descriptors.MemberWithAttributeCannotHaveImplementation"/></para>
		/// </summary>
		/// <param name="attributeName">Name of the attribute the rule applies to.</param>
		public static DiagnosticDescriptor MemberWithAttributeCannotHaveImplementation(string attributeName)
		{
			return CopyDescriptor(
				Descriptors.MemberWithAttributeCannotHaveImplementation,
				("'memberType'", "Member"),
				(nameof(attributeName), attributeName)
			);
		}

		/// <summary>
		/// Creates a new instance of <see cref="DiagnosticDescriptor"/> of the DUR0013 rule with the specified <paramref name="attributeName"/> and <paramref name="memberType"/>.
		/// <para>See: <see cref="Descriptors.MemberWithAttributeCannotHaveImplementation"/></para>
		/// </summary>
		/// <param name="attributeName">Name of the attribute the rule applies to.</param>
		/// <param name="memberType">Type of the member the rule applies to.</param>
		public static DiagnosticDescriptor MemberWithAttributeCannotHaveImplementation(string attributeName, string memberType)
		{
			return CopyDescriptor(
				Descriptors.MemberWithAttributeCannotHaveImplementation,
				("'memberType'", memberType),
				(nameof(attributeName), attributeName)
			);
		}

		/// <summary>
		/// Creates a new instance of <see cref="DiagnosticDescriptor"/> of the DUR0014 rule with the specified <paramref name="attributeName"/>.
		/// <para>See: <see cref="Descriptors.ParentTypeOfMemberWithAttributeMustBePartial"/></para>
		/// </summary>
		/// <param name="attributeName">Name of the attribute the rule applies to.</param>
		public static DiagnosticDescriptor ParentTypeOfMemberWithAttributeMustBePartial(string attributeName)
		{
			return CopyDescriptor(
				Descriptors.ParentTypeOfMemberWithAttributeMustBePartial,
				(nameof(attributeName), attributeName)
			);
		}

		/// <summary>
		/// Creates a new instance of <see cref="DiagnosticDescriptor"/> of the DUR0015 rule with the specified <paramref name="attributeName"/> and <paramref name="memberType"/>.
		/// <para>See: <see cref="Descriptors.TargetOfAttributeMustBeOfSpecifiedMemberType"/></para>
		/// </summary>
		/// <param name="attributeName">Name of the attribute the rule applies to.</param>
		/// <param name="memberType">Type of the member the rule applies to.</param>
		public static DiagnosticDescriptor TargetOfAttributeMustBeOfSpecifiedMemberType(string attributeName, string memberType)
		{
			return CopyDescriptor(
				Descriptors.TargetOfAttributeMustBeOfSpecifiedMemberType,
				(nameof(attributeName), attributeName),
				("'memberType'", memberType)
			);
		}

		/// <summary>
		/// Creates a new instance of <see cref="DiagnosticDescriptor"/> of the DUR0016 rule with the specified <paramref name="attributeName"/> and <paramref name="memberType"/>.
		/// <para>See: <see cref="Descriptors.AttributeCannotBeAppliedToMembersOfType"/></para>
		/// </summary>
		/// <param name="attributeName">Name of the attribute the rule applies to.</param>
		/// <param name="memberType">Type of the member the rule applies to.</param>
		public static DiagnosticDescriptor AttributeCannotBeAppliedToMembersOfType(string attributeName, string memberType)
		{
			return CopyDescriptor(
				Descriptors.AttributeCannotBeAppliedToMembersOfType,
				(nameof(attributeName), attributeName),
				("'memberType'", memberType)
			);
		}

		/// <summary>
		/// Creates a new instance of <see cref="DiagnosticDescriptor"/> of the DUR0017 rule with the specified <paramref name="attributeName"/> and the 'attributeName2' left unspecified.
		/// <para>See: <see cref="Descriptors.AttributeCannotBeAppliedToMembersWithAttribute"/></para>
		/// </summary>
		/// <param name="attributeName">Name of the attribute.</param>
		public static DiagnosticDescriptor AttributeCannotBeAppliedToMembersWithAttribute(string attributeName)
		{
			return CopyDescriptor(
				Descriptors.AttributeCannotBeAppliedToMembersWithAttribute,
				("attributeName1", attributeName),
				("{2}", "{1}")
			);
		}

		/// <summary>
		/// Creates a new instance of <see cref="DiagnosticDescriptor"/> of the DUR0017 rule with the specified <paramref name="attributeName1"/> and <paramref name="attributeName2"/>.
		/// <para>See: <see cref="Descriptors.AttributeCannotBeAppliedToMembersWithAttribute"/></para>
		/// </summary>
		/// <param name="attributeName1">Name of the first attribute.</param>
		/// <param name="attributeName2">Name of the second attribute.</param>
		public static DiagnosticDescriptor AttributeCannotBeAppliedToMembersWithAttribute(string attributeName1, string attributeName2)
		{
			return CopyDescriptor(
				Descriptors.AttributeCannotBeAppliedToMembersWithAttribute,
				(nameof(attributeName1), attributeName1),
				(nameof(attributeName2), attributeName2)
			);
		}

		/// <summary>
		/// Creates a new instance of <see cref="DiagnosticDescriptor"/> of the DUR0018 rule with the specified <paramref name="attributeName"/>.
		/// <para>See: <see cref="Descriptors.TypeParameterWithAttributeMustBeLast"/></para>
		/// </summary>
		/// <param name="attributeName">Name of the attribute the rule applies to.</param>
		public static DiagnosticDescriptor TypeParameterWithAttributeMustBeLast(string attributeName)
		{
			return CopyDescriptor(
				Descriptors.TypeParameterWithAttributeMustBeLast,
				(nameof(attributeName), attributeName)
			);
		}

		/// <summary>
		/// Creates a new instance of <see cref="DiagnosticDescriptor"/> of the DUR0020 rule with the specified <paramref name="attributeName"/>.
		/// <para>See: <see cref="Descriptors.AttributeOfOverridenMemberShouldBeAddedForClarity"/></para>
		/// </summary>
		/// <param name="attributeName">Name of the attribute the rule applies to.</param>
		public static DiagnosticDescriptor AttributeOfOverridenMemberShouldBeAddedForClarity(string attributeName)
		{
			return CopyDescriptor(
				Descriptors.AttributeOfOverridenMemberShouldBeAddedForClarity,
				(nameof(attributeName), attributeName)
			);
		}

		/// <summary>
		/// Creates a new instance of <see cref="DiagnosticDescriptor"/> of the DUR0021 rule with the specified <paramref name="attributeName"/>.
		/// <para>See: <see cref="Descriptors.DoNotOverrideMembersGeneratedUsingSpecifiedAttribute"/></para>
		/// </summary>
		/// <param name="attributeName">Name of the attribute the rule applies to.</param>
		public static DiagnosticDescriptor DoNotOverrideMembersGeneratedUsingSpecifiedAttribute(string attributeName)
		{
			return CopyDescriptor(
				Descriptors.DoNotOverrideMembersGeneratedUsingSpecifiedAttribute,
				(nameof(attributeName), attributeName)
			);
		}

		/// <summary>
		/// Creates a new instance of <see cref="DiagnosticDescriptor"/> of the DUR0022 rule with the specified <paramref name="attributeName"/>.
		/// <para>See: <see cref="Descriptors.ValueOfAttributeMustBeTheSameAsValueOfTheOverridenMember"/></para>
		/// </summary>
		/// <param name="attributeName">Name of the attribute the rule applies to.</param>
		public static DiagnosticDescriptor ValueOfAttributeMustBeTheSameAsValueOfTheOverridenMember(string attributeName)
		{
			return CopyDescriptor(
				Descriptors.ValueOfAttributeMustBeTheSameAsValueOfTheOverridenMember,
				(nameof(attributeName), attributeName)
			);
		}

		/// <summary>
		/// Creates a new instance of <see cref="DiagnosticDescriptor"/> of the DUR0025 rule with the specified <paramref name="attributeName"/>.
		/// <para>See: <see cref="Descriptors.ValueOfAttributeMustBeTheSameAsValueOfTheOverridenMember"/></para>
		/// </summary>
		/// <param name="attributeName">Name of the attribute the rule applies to.</param>
		public static DiagnosticDescriptor DoNotAddAttributeOnVirtualTypeParameter(string attributeName)
		{
			return CopyDescriptor(
				Descriptors.DoNotAddDefaultParamAttributeOnOverriddenVirtualTypeParameter,
				(nameof(attributeName), attributeName)
			);
		}

		private static DiagnosticDescriptor CopyDescriptor(DiagnosticDescriptor descriptor, params (string original, string target)[] replacements)
		{
			string title = descriptor.Title.ToString(CultureInfo.InvariantCulture);
			string messageFormat = descriptor.MessageFormat.ToString(CultureInfo.InvariantCulture);

			if (replacements is not null)
			{
				string[] args = new string[replacements.Length + 1];
				args[0] = "{0}";

				for (int i = 0, argIndex = 1; i < replacements.Length; i++, argIndex++)
				{
					(string original, string target) = replacements[i];
					title = title.Replace(original, target);
					args[argIndex] = target;
				}

				messageFormat = string.Format(messageFormat, args);
			}

			return new(
				id: descriptor.Id,
				title: title,
				messageFormat: messageFormat,
				category: descriptor.Category,
				defaultSeverity: descriptor.DefaultSeverity,
				isEnabledByDefault: descriptor.IsEnabledByDefault,
				description: descriptor.Description.ToString(CultureInfo.InvariantCulture),
				helpLinkUri: descriptor.HelpLinkUri,
				customTags: descriptor.CustomTags.ToArray()
			);
		}
	}
}
