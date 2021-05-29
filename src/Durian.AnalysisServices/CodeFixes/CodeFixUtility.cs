using System;
using System.Collections.Generic;
using System.Threading;
using Durian.Generator.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.Generator.CodeFixes
{
	/// <summary>
	/// Contains some utility methods for dealing with code fixes.
	/// </summary>
	public static class CodeFixUtility
	{
		/// <summary>
		/// Returns a <see cref="NameSyntax"/> for the specified <see cref="INamedTypeSymbol"/> of an <see cref="Attribute"/> type based on the existing <paramref name="usings"/> and <paramref name="currentNamespace"/>.
		/// </summary>
		/// <param name="semanticModel"><see cref="SemanticModel"/> of the <paramref name="usings"/>.</param>
		/// <param name="usings">A collection of <see cref="UsingDirectiveSyntax"/>.</param>
		/// <param name="currentNamespace">Namespace where the created <see cref="NameSyntax"/> is to be located in.</param>
		/// <param name="targetType"><see cref="INamedTypeSymbol"/> of an <see cref="Attribute"/> to get the <see cref="NameSyntax"/> for.</param>
		/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
		public static NameSyntax GetNameSyntaxForAttribute(SemanticModel semanticModel, IEnumerable<UsingDirectiveSyntax> usings, INamespaceSymbol currentNamespace, INamedTypeSymbol targetType, CancellationToken cancellationToken = default)
		{
			if (targetType.ContainingNamespace is null ||
				targetType.ContainingNamespace.IsGlobalNamespace ||
				currentNamespace.IsGlobalNamespace ||
				targetType.IsPredefinedOrDynamic((CSharpCompilation)semanticModel.Compilation)
			)
			{
				return SyntaxFactory.IdentifierName(GetIdentifier(targetType.Name));
			}

			string @namespace = currentNamespace.ToString();
			string targetNamespace = targetType.ContainingNamespace.ToString();

			if (@namespace.StartsWith(targetNamespace))
			{
				return SyntaxFactory.IdentifierName(GetIdentifier(targetType.Name));
			}

			foreach (UsingDirectiveSyntax u in usings)
			{
				SymbolInfo info = semanticModel.GetSymbolInfo(u.Name, cancellationToken);

				if (info.Symbol is not INamespaceSymbol n)
				{
					continue;
				}

				if (n.ToString() == targetNamespace)
				{
					return SyntaxFactory.IdentifierName(GetIdentifier(targetType.Name));
				}
			}

			return SyntaxFactory.ParseName(GetIdentifier(targetType.ToString()));

			static string GetIdentifier(string str)
			{
				const string attribute = "Attribute";

				if (str.EndsWith(attribute))
				{
					return str.Substring(0, str.Length - attribute.Length);
				}

				return str;
			}
		}

		/// <summary>
		/// Returns a <see cref="NameSyntax"/> for the specified <paramref name="targetType"/> based on the existing <paramref name="usings"/> and <paramref name="currentNamespace"/>.
		/// </summary>
		/// <param name="semanticModel"><see cref="SemanticModel"/> of the <paramref name="usings"/>.</param>
		/// <param name="usings">A collection of <see cref="UsingDirectiveSyntax"/>.</param>
		/// <param name="currentNamespace">Namespace where the created <see cref="NameSyntax"/> is to be located in.</param>
		/// <param name="targetType"><see cref="ITypeSymbol"/> to get the <see cref="NameSyntax"/> for.</param>
		/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
		public static NameSyntax GetNameSyntax(SemanticModel semanticModel, IEnumerable<UsingDirectiveSyntax> usings, INamespaceSymbol currentNamespace, ITypeSymbol targetType, CancellationToken cancellationToken = default)
		{
			if (targetType.ContainingNamespace is null ||
				targetType.ContainingNamespace.IsGlobalNamespace ||
				currentNamespace.IsGlobalNamespace
			)
			{
				return SyntaxFactory.ParseName(targetType.GetGenericName(false));
			}

			if (targetType.IsPredefinedOrDynamic((CSharpCompilation)semanticModel.Compilation))
			{
				return SyntaxFactory.IdentifierName(targetType.Name);
			}

			string @namespace = currentNamespace.ToString();
			string targetNamespace = targetType.ContainingNamespace.ToString();

			if (@namespace.StartsWith(targetNamespace))
			{
				return SyntaxFactory.ParseName(targetType.GetGenericName(false));
			}

			foreach (UsingDirectiveSyntax u in usings)
			{
				SymbolInfo info = semanticModel.GetSymbolInfo(u.Name, cancellationToken);

				if (info.Symbol is not INamespaceSymbol n)
				{
					continue;
				}

				if (n.ToString() == targetNamespace)
				{
					return SyntaxFactory.ParseName(targetType.GetGenericName(false));
				}
			}

			return SyntaxFactory.ParseName(targetType.ToString());
		}

		/// <summary>
		/// Determines whether there is a <see cref="UsingDirectiveSyntax"/> specified for the given <paramref name="targetType"/>.
		/// </summary>
		/// <param name="semanticModel"><see cref="SemanticModel"/> of the <paramref name="usings"/>.</param>
		/// <param name="usings">A collection of <see cref="UsingDirectiveSyntax"/>.</param>
		/// <param name="currentNamespace">Current namespace.</param>
		/// <param name="targetType"><see cref="ITypeSymbol"/> to check.</param>
		/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
		public static bool HasUsingDirective(SemanticModel semanticModel, IEnumerable<UsingDirectiveSyntax> usings, INamespaceSymbol currentNamespace, ITypeSymbol targetType, CancellationToken cancellationToken = default)
		{
			if (targetType.ContainingNamespace is null ||
				targetType.ContainingNamespace.IsGlobalNamespace ||
				currentNamespace.IsGlobalNamespace ||
				targetType.IsPredefinedOrDynamic((CSharpCompilation)semanticModel.Compilation)
			)
			{
				return true;
			}

			string @namespace = currentNamespace.ToString();
			string targetNamespace = targetType.ContainingNamespace.ToString();

			if (@namespace.StartsWith(targetNamespace))
			{
				return true;
			}

			foreach (UsingDirectiveSyntax u in usings)
			{
				SymbolInfo info = semanticModel.GetSymbolInfo(u.Name, cancellationToken);

				if (info.Symbol is not INamespaceSymbol n)
				{
					continue;
				}

				if (n.ToString() == targetNamespace)
				{
					return true;
				}
			}

			return false;
		}
	}
}
