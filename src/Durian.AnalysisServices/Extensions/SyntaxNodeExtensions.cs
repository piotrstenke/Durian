// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using Durian.Generator.Data;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.Generator.Extensions
{
	/// <summary>
	/// Contains various extension methods for <see cref="SyntaxNode"/>-derived classes.
	/// </summary>
	public static class SyntaxNodeExtensions
	{
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
				TypeDeclarationSyntax => new MethodData((MethodDeclarationSyntax)member, compilation),
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
