// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Durian.Analysis.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Durian.Analysis.SyntaxVisitors
{
	/// <summary>
	/// Replaces <see cref="IdentifierNameSyntax"/> with <see cref="QualifiedNameSyntax"/>.
	/// </summary>
	public class IdentifierToQualifiedNameReplacer : CSharpSyntaxRewriter
	{
		/// <summary>
		/// Determines whether to delete using directives.
		/// </summary>
		public bool DeleteUsings { get; set; }

		/// <summary>
		/// <see cref="Microsoft.CodeAnalysis.SemanticModel"/> that is used to identify <see cref="ISymbol"/>s.
		/// </summary>
		[MaybeNull]
		public SemanticModel SemanticModel { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="IdentifierToQualifiedNameReplacer"/> class.
		/// </summary>
		public IdentifierToQualifiedNameReplacer()
		{
			SemanticModel = null!;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="IdentifierToQualifiedNameReplacer"/> class.
		/// </summary>
		/// <param name="semanticModel"><see cref="Microsoft.CodeAnalysis.SemanticModel"/> that is used to identify <see cref="ISymbol"/>s.</param>
		public IdentifierToQualifiedNameReplacer(SemanticModel semanticModel)
		{
			SemanticModel = semanticModel;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="IdentifierToQualifiedNameReplacer"/> class.
		/// </summary>
		/// <param name="semanticModel"><see cref="Microsoft.CodeAnalysis.SemanticModel"/> that is used to identify <see cref="ISymbol"/>s.</param>
		/// <param name="deleteUsings">Determines whether to delete using directives.</param>
		public IdentifierToQualifiedNameReplacer(SemanticModel semanticModel, bool deleteUsings)
		{
			SemanticModel = semanticModel;
			DeleteUsings = deleteUsings;
		}

		/// <inheritdoc/>
		public override SyntaxNode? VisitGenericName(GenericNameSyntax node)
		{
			if (node.Parent is QualifiedNameSyntax)
			{
				return base.VisitGenericName(node);
			}

			TypeInfo info = SemanticModel.GetTypeInfo(node);

			if (info.Type is not INamedTypeSymbol symbol)
			{
				return base.VisitGenericName(node);
			}

			string[] namespaces = symbol.GetContainingNamespacesAndTypes().Select(n => n.Name).ToArray();

			if (namespaces.Length == 0)
			{
				return base.VisitGenericName(node);
			}
			else
			{
				TypeArgumentListSyntax arguments = (TypeArgumentListSyntax)VisitTypeArgumentList(node.TypeArgumentList)!.WithoutTrivia();

				GenericNameSyntax name = node.WithTypeArgumentList(arguments).WithoutTrivia();

				if (namespaces.Length == 1)
				{
					return QualifiedName(IdentifierName(namespaces[0]), name).WithTriviaFrom(node);
				}
				else
				{
					return QualifiedName(AnalysisUtilities.JoinIntoQualifiedName(namespaces)!, name).WithTriviaFrom(node);
				}
			}
		}

		/// <inheritdoc/>
		public override SyntaxNode? VisitIdentifierName(IdentifierNameSyntax node)
		{
			if (node.Parent is QualifiedNameSyntax)
			{
				return base.VisitIdentifierName(node);
			}

			SymbolInfo info = SemanticModel.GetSymbolInfo(node);

			if (info.Symbol is not INamedTypeSymbol symbol)
			{
				return base.VisitIdentifierName(node);
			}

			string[] namespaces = symbol.GetContainingNamespacesAndTypes().Select(n => n.Name).ToArray();

			if (namespaces.Length == 0)
			{
				return base.VisitIdentifierName(node);
			}
			else if (namespaces.Length == 1)
			{
				return QualifiedName(IdentifierName(namespaces[0]), node.WithoutTrivia()).WithTriviaFrom(node);
			}
			else
			{
				return QualifiedName(AnalysisUtilities.JoinIntoQualifiedName(namespaces)!, node.WithoutTrivia()).WithTriviaFrom(node);
			}
		}

		/// <inheritdoc/>
		public override SyntaxNode? VisitNameMemberCref(NameMemberCrefSyntax node)
		{
			SymbolInfo info = SemanticModel.GetSymbolInfo(node);

			if (info.Symbol is not INamedTypeSymbol symbol)
			{
				return base.VisitNameMemberCref(node);
			}

			string[] namespaces = symbol.GetContainingNamespacesAndTypes().Select(n => n.Name).ToArray();

			if (namespaces.Length == 0)
			{
				return base.VisitNameMemberCref(node);
			}
			else if (namespaces.Length == 1)
			{
				return QualifiedCref(IdentifierName(namespaces[0]), node.WithoutTrivia()).WithTriviaFrom(node);
			}
			else
			{
				return QualifiedCref(AnalysisUtilities.JoinIntoQualifiedName(namespaces)!, node.WithoutTrivia()).WithTriviaFrom(node);
			}
		}

		/// <inheritdoc/>
		public override SyntaxNode? VisitQualifiedCref(QualifiedCrefSyntax node)
		{
			SymbolInfo info = SemanticModel.GetSymbolInfo(node);
			if (info.Symbol is not INamedTypeSymbol symbol)
			{
				return base.VisitQualifiedCref(node);
			}

			string[] namespaces = symbol.GetContainingNamespacesAndTypes().Select(n => n.Name).ToArray();

			if (namespaces.Length < 2)
			{
				return base.VisitQualifiedCref(node);
			}
			else
			{
				return QualifiedCref(AnalysisUtilities.JoinIntoQualifiedName(namespaces)!, node.Member.WithoutTrivia()).WithTriviaFrom(node);
			}
		}

		/// <inheritdoc/>
		public override SyntaxNode? VisitQualifiedName(QualifiedNameSyntax node)
		{
			TypeInfo info = SemanticModel.GetTypeInfo(node);

			if (info.Type is not INamedTypeSymbol symbol)
			{
				return base.VisitQualifiedName(node);
			}

			string[] namespaces = symbol.GetContainingNamespacesAndTypes().Select(n => n.Name).ToArray();

			if (namespaces.Length < 2)
			{
				return base.VisitQualifiedName(node);
			}
			else
			{
				return QualifiedName(AnalysisUtilities.JoinIntoQualifiedName(namespaces)!, node.Right.WithoutTrivia()).WithTriviaFrom(node);
			}
		}

		/// <inheritdoc/>
		public override SyntaxNode? VisitUsingDirective(UsingDirectiveSyntax node)
		{
			if (DeleteUsings)
			{
				return null;
			}

			return base.VisitUsingDirective(node);
		}
	}
}
