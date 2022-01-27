// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using Durian.Analysis.Data;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.Analysis.Extensions
{
	/// <summary>
	/// Contains various extension methods for <see cref="SyntaxNode"/>-derived classes.
	/// </summary>
	public static class SyntaxNodeExtensions
	{
		/// <summary>
		/// Returns attribute argument with the specified <paramref name="argumentName"/>
		/// or <see langword="null"/> if no argument with the <paramref name="argumentName"/> was found.
		/// </summary>
		/// <param name="attribute"><see cref="AttributeSyntax"/> to get the argument of.</param>
		/// <param name="argumentName">Name of argument to get.</param>
		/// <param name="includeParameters">Determines whether to include arguments with colons in the search.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="attribute"/> is <see langword="null"/>. -or-
		/// <paramref name="argumentName"/> is <see langword="null"/>.
		/// </exception>
		/// <exception cref="ArgumentException"><paramref name="argumentName"/> cannot be empty or white space only.</exception>
		public static AttributeArgumentSyntax? GetArgument(this AttributeSyntax attribute, string argumentName, bool includeParameters = false)
		{
			if (attribute is null)
			{
				throw new ArgumentNullException(nameof(attribute));
			}

			if (argumentName is null)
			{
				throw new ArgumentNullException(nameof(argumentName));
			}

			if (string.IsNullOrWhiteSpace(argumentName))
			{
				throw new ArgumentException("Property cannot be empty or white space only!", nameof(argumentName));
			}

			if (attribute.ArgumentList is null)
			{
				return null;
			}

			SeparatedSyntaxList<AttributeArgumentSyntax> arguments = attribute.ArgumentList.Arguments;

			if (!arguments.Any())
			{
				return null;
			}

			Func<AttributeArgumentSyntax, bool> func;

			if (includeParameters)
			{
				func = arg =>
				{
					if (arg.NameEquals is not null)
					{
						return arg.NameEquals.Name.Identifier.ValueText == argumentName;
					}

					return arg.NameColon is not null && arg.NameColon.Name.Identifier.ValueText == argumentName;
				};
			}
			else
			{
				func = arg => arg.NameEquals is not null && arg.NameEquals.Name.Identifier.ValueText == argumentName;
			}

			return arguments.FirstOrDefault(func);
		}

		/// <summary>
		/// Returns attribute argument at the specified <paramref name="position"/> and with the given <paramref name="argumentName"/>.
		/// <para>If <paramref name="argumentName"/> is <see langword="null"/>, only <paramref name="position"/> is included in the search.</para>
		/// <para>If no appropriate argument found, returns <see langword="null"/>.</para>
		/// </summary>
		/// <param name="attribute"><see cref="AttributeSyntax"/> to get the argument of.</param>
		/// <param name="position">Position of argument to get.</param>
		/// <param name="argumentName">Name of the argument to get the location of.</param>
		/// <exception cref="ArgumentNullException"><paramref name="attribute"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="position"/> cannot be less than <c>0</c>.</exception>
		public static AttributeArgumentSyntax? GetArgument(this AttributeSyntax attribute, int position, string? argumentName = null)
		{
			if (attribute is null)
			{
				throw new ArgumentNullException(nameof(attribute));
			}

			if (position < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(position), "Argument position cannot be less than 0!");
			}

			if (attribute.ArgumentList is null)
			{
				return null;
			}

			SeparatedSyntaxList<AttributeArgumentSyntax> arguments = attribute.ArgumentList.Arguments;

			if (!arguments.Any())
			{
				return null;
			}

			if (string.IsNullOrWhiteSpace(argumentName))
			{
				if (position >= arguments.Count)
				{
					return null;
				}

				return arguments[position];
			}

			if (position < arguments.Count)
			{
				AttributeArgumentSyntax arg = arguments[position];

				if (arg.GetName() == argumentName)
				{
					return arg;
				}
			}

			return arguments.FirstOrDefault(arg => arg.GetName() == argumentName);
		}

		/// <summary>
		/// Returns location of attribute argument with the specified <paramref name="argumentName"/>
		/// or location of the <paramref name="attribute"/> if no argument with the <paramref name="argumentName"/> was found.
		/// </summary>
		/// <param name="attribute"><see cref="AttributeSyntax"/> to get the location of argument of.</param>
		/// <param name="argumentName">Name of the argument to get the location of.</param>
		/// <param name="includeParameters">Determines whether to include arguments with colons in the search.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="attribute"/> is <see langword="null"/>. -or-
		/// <paramref name="argumentName"/> is <see langword="null"/>.
		/// </exception>
		/// <exception cref="ArgumentException"><paramref name="argumentName"/> cannot be empty or white space only.</exception>
		public static Location GetArgumentLocation(this AttributeSyntax attribute, string argumentName, bool includeParameters = false)
		{
			AttributeArgumentSyntax? arg = attribute.GetArgument(argumentName, includeParameters);

			if (arg is null)
			{
				return attribute.GetLocation();
			}

			return arg.GetLocation();
		}

		/// <summary>
		/// Returns location of attribute argument at the specified <paramref name="position"/> and with the given <paramref name="argumentName"/>
		/// or location of the <paramref name="attribute"/> is no appropriate argument was found.
		/// </summary>
		/// <param name="attribute"><see cref="AttributeSyntax"/> to get the location of argument of.</param>
		/// <param name="position">Position of argument to get.</param>
		/// <param name="argumentName">Name of the argument to get the location of.</param>
		/// <exception cref="ArgumentNullException"><paramref name="attribute"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="position"/> cannot be less than <c>0</c>.</exception>
		public static Location GetArgumentLocation(this AttributeSyntax attribute, int position, string? argumentName = null)
		{
			AttributeArgumentSyntax? arg = attribute.GetArgument(position, argumentName);

			if (arg is null)
			{
				return attribute.GetLocation();
			}

			return arg.GetLocation();
		}

		/// <summary>
		/// Returns the keyword that is used to declare the given <paramref name="type"/>.
		/// </summary>
		/// <param name="type"><see cref="BaseTypeDeclarationSyntax"/> to get the keyword of.</param>
		/// <exception cref="ArgumentException">Unknown type declaration format.</exception>
		/// <exception cref="ArgumentNullException"><paramref name="type"/> is <see langword="null"/>.</exception>
		public static string GetKeyword(this BaseTypeDeclarationSyntax type)
		{
			if (type is TypeDeclarationSyntax t)
			{
				return t.Keyword.ToString();
			}

			if (type is EnumDeclarationSyntax)
			{
				return "enum";
			}

			if (type is null)
			{
				throw new ArgumentNullException(nameof(type));
			}

			throw new ArgumentException($"Unknown type declaration format: {type}");
		}

		/// <summary>
		/// Returns new instance of <see cref="IMemberData"/> associated with the specified <paramref name="member"/>.
		/// </summary>
		/// <param name="member"><see cref="MemberDeclarationSyntax"/> to get the data of.</param>
		/// <param name="compilation">Current <see cref="ICompilationData"/>.</param>
		/// <exception cref="ArgumentNullException"><paramref name="member"/> is <see langword="null"/>. -or- <paramref name="compilation"/> is <see langword="null"/>.</exception>
		public static IMemberData GetMemberData(this MemberDeclarationSyntax member, ICompilationData compilation)
		{
			if (member is null)
			{
				throw new ArgumentNullException(nameof(member));
			}

			if (compilation is null)
			{
				throw new ArgumentNullException(nameof(compilation));
			}

			return member switch
			{
				ClassDeclarationSyntax => new ClassData((ClassDeclarationSyntax)member, compilation),
				StructDeclarationSyntax => new StructData((StructDeclarationSyntax)member, compilation),
				InterfaceDeclarationSyntax => new InterfaceData((InterfaceDeclarationSyntax)member, compilation),
				RecordDeclarationSyntax => new RecordData((RecordDeclarationSyntax)member, compilation),
				EnumDeclarationSyntax => new EnumData((EnumDeclarationSyntax)member, compilation),
				BaseTypeDeclarationSyntax => new TypeData((BaseTypeDeclarationSyntax)member, compilation),
				MethodDeclarationSyntax => new MethodData((MethodDeclarationSyntax)member, compilation),
				FieldDeclarationSyntax => new FieldData((FieldDeclarationSyntax)member, compilation),
				PropertyDeclarationSyntax => new PropertyData((PropertyDeclarationSyntax)member, compilation),
				EventDeclarationSyntax => new EventData((EventDeclarationSyntax)member, compilation),
				EventFieldDeclarationSyntax => new EventData((EventFieldDeclarationSyntax)member, compilation),
				DelegateDeclarationSyntax => new DelegateData((DelegateDeclarationSyntax)member, compilation),
				_ => new MemberData(member, compilation),
			};
		}

		/// <summary>
		/// Returns the name of attribute argument represented by the specified <paramref name="syntax"/>
		/// or <see langword="null"/> if the argument has neither <see cref="NameEqualsSyntax"/> or <see cref="NameColonSyntax"/>.
		/// </summary>
		/// <param name="syntax"><see cref="AttributeArgumentSyntax"/> to get the name of.</param>
		public static string? GetName(this AttributeArgumentSyntax syntax)
		{
			if (syntax.NameEquals is not null)
			{
				return syntax.NameEquals.GetName();
			}

			if (syntax.NameColon is not null)
			{
				return syntax.NameColon.GetName();
			}

			return null;
		}

		/// <summary>
		/// Returns the name of member represented by the specified <paramref name="syntax"/>.
		/// </summary>
		/// <param name="syntax"><see cref="NameColonSyntax"/> that contains the member name.</param>
		public static string GetName(this NameEqualsSyntax syntax)
		{
			return syntax.Name.Identifier.ValueText;
		}

		/// <summary>
		/// Returns the name of member represented by the specified <paramref name="syntax"/>.
		/// </summary>
		/// <param name="syntax"><see cref="NameColonSyntax"/> that contains the member name.</param>
		public static string GetName(this NameColonSyntax syntax)
		{
			return syntax.Name.Identifier.ValueText;
		}

		/// <summary>
		/// Returns parent namespaces of the specified <paramref name="node"/>.
		/// </summary>
		/// <param name="node"><see cref="SyntaxNode"/> to get the parent namespaces of.</param>
		/// <exception cref="ArgumentNullException"><paramref name="node"/> is <see langword="null"/>.</exception>
		public static IEnumerable<string> GetParentNamespaces(this SyntaxNode node)
		{
			if (node is null)
			{
				throw new ArgumentNullException(nameof(node));
			}

			return Yield().Reverse();

			IEnumerable<string> Yield()
			{
				SyntaxNode? current = node;

				while ((current = current!.Parent) is not null)
				{
					if (current is NamespaceDeclarationSyntax decl)
					{
						string[] split = decl.Name.ToString().Split('.');
						int length = split.Length;

						for (int i = length - 1; i > -1; i--)
						{
							yield return split[i];
						}
					}
				}
			}
		}

		/// <summary>
		/// Returns a <see cref="TypeParameterListSyntax"/> of the <paramref name="member"/> or <see langword="null"/> if the <paramref name="member"/> has no type parameters.
		/// </summary>
		/// <param name="member"><see cref="MemberDeclarationSyntax"/> to get the <see cref="TypeParameterListSyntax"/> of.</param>
		/// <exception cref="ArgumentNullException"><paramref name="member"/> is <see langword="null"/>.</exception>
		public static TypeParameterListSyntax? GetTypeParameterList(this MemberDeclarationSyntax member)
		{
			if (member is null)
			{
				throw new ArgumentNullException(nameof(member));
			}

			return member switch
			{
				TypeDeclarationSyntax t => t.TypeParameterList,
				MethodDeclarationSyntax m => m.TypeParameterList,
				DelegateDeclarationSyntax d => d.TypeParameterList,
				_ => null
			};
		}

		/// <summary>
		/// Checks if the target <paramref name="method"/> has a body, either block or expression.
		/// </summary>
		/// <param name="method"><see cref="MethodDeclarationSyntax"/> to check if has a body.</param>
		/// <exception cref="ArgumentNullException"><paramref name="method"/> is <see langword="null"/>.</exception>
		public static bool HasBody(this MethodDeclarationSyntax method)
		{
			if (method is null)
			{
				throw new ArgumentNullException(nameof(method));
			}

			return method.Body is not null || method.ExpressionBody is not null;
		}
	}
}