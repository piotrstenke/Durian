﻿using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.Analysis.CodeFixes;

/// <summary>
/// Contains some utility methods for dealing with code fixes.
/// </summary>
public static class CodeFixUtility
{
	/// <summary>
	/// Returns a <see cref="NameSyntax"/> for the specified <paramref name="targetType"/> based on the existing <paramref name="usings"/> and <paramref name="currentNamespace"/>.
	/// </summary>
	/// <param name="semanticModel"><see cref="SemanticModel"/> of the <paramref name="usings"/>.</param>
	/// <param name="usings">A collection of <see cref="UsingDirectiveSyntax"/>.</param>
	/// <param name="currentNamespace">Namespace where the created <see cref="NameSyntax"/> is to be located in.</param>
	/// <param name="targetType"><see cref="ITypeSymbol"/> to get the <see cref="NameSyntax"/> for.</param>
	/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
	public static NameSyntax GetNameSyntax(
		this SemanticModel semanticModel,
		IEnumerable<UsingDirectiveSyntax> usings,
		INamespaceSymbol currentNamespace,
		ITypeSymbol targetType,
		CancellationToken cancellationToken = default)
	{
		if (targetType.ContainingNamespace is null ||
			targetType.ContainingNamespace.IsGlobalNamespace
		)
		{
			return SyntaxFactory.ParseName(targetType.GetGenericName(true));
		}

		if (targetType.IsKeyword())
		{
			return SyntaxFactory.IdentifierName(targetType.Name);
		}

		string @namespace = currentNamespace.ToString();
		string targetNamespace = targetType.ContainingNamespace.ToString();

		if (@namespace.StartsWith(targetNamespace))
		{
			return SyntaxFactory.ParseName(targetType.GetGenericName(true));
		}

		foreach (UsingDirectiveSyntax u in usings)
		{
			SymbolInfo info = semanticModel.GetSymbolInfo(u.NamespaceOrType, cancellationToken);

			if (info.Symbol is not INamespaceSymbol n)
			{
				continue;
			}

			if (n.ToString() == targetNamespace)
			{
				return SyntaxFactory.ParseName(targetType.GetGenericName(true));
			}
		}

		return SyntaxFactory.ParseName(targetType.ToString());
	}

	/// <summary>
	/// Returns a <see cref="NameSyntax"/> for the specified <see cref="INamedTypeSymbol"/> of an <see cref="Attribute"/> type based on the existing <paramref name="usings"/> and <paramref name="currentNamespace"/>.
	/// </summary>
	/// <param name="semanticModel"><see cref="SemanticModel"/> of the <paramref name="usings"/>.</param>
	/// <param name="usings">A collection of <see cref="UsingDirectiveSyntax"/>.</param>
	/// <param name="currentNamespace">Namespace where the created <see cref="NameSyntax"/> is to be located in.</param>
	/// <param name="targetType"><see cref="INamedTypeSymbol"/> of an <see cref="Attribute"/> to get the <see cref="NameSyntax"/> for.</param>
	/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
	public static NameSyntax GetNameSyntaxForAttribute(
		this SemanticModel semanticModel,
		IEnumerable<UsingDirectiveSyntax> usings,
		INamespaceSymbol currentNamespace,
		INamedTypeSymbol targetType,
		CancellationToken cancellationToken = default)
	{
		if (targetType.ContainingNamespace is null ||
			targetType.ContainingNamespace.IsGlobalNamespace ||
			targetType.IsKeyword()
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
			SymbolInfo info = semanticModel.GetSymbolInfo(u.NamespaceOrType, cancellationToken);

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
			const string ATTRIBUTE = "Attribute";

			if (str.EndsWith(ATTRIBUTE))
			{
				return str.Substring(0, str.Length - ATTRIBUTE.Length);
			}

			return str;
		}
	}

	/// <summary>
	/// Determines whether there is a <see cref="UsingDirectiveSyntax"/> specified for the given <paramref name="targetType"/>.
	/// </summary>
	/// <param name="semanticModel"><see cref="SemanticModel"/> of the <paramref name="usings"/>.</param>
	/// <param name="usings">A collection of <see cref="UsingDirectiveSyntax"/>.</param>
	/// <param name="currentNamespace">Current namespace.</param>
	/// <param name="targetType"><see cref="ITypeSymbol"/> to check.</param>
	/// <param name="cancellationToken"><see cref="CancellationToken"/> that specifies if the operation should be canceled.</param>
	public static bool HasUsingDirective(
		this SemanticModel semanticModel,
		IEnumerable<UsingDirectiveSyntax> usings,
		INamespaceSymbol currentNamespace,
		ITypeSymbol targetType,
		CancellationToken cancellationToken = default)
	{
		if (targetType.ContainingNamespace is null ||
			targetType.ContainingNamespace.IsGlobalNamespace ||
			targetType.IsKeyword()
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
			SymbolInfo info = semanticModel.GetSymbolInfo(u.NamespaceOrType, cancellationToken);

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
