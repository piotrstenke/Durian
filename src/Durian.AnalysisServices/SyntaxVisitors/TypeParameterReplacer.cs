// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.Analysis.SyntaxVisitors
{
	/// <summary>
	/// Replaces a given identifier of a type parameter with a specified <see cref="CSharpSyntaxNode"/>.
	/// </summary>
	public class TypeParameterReplacer : IdentifierReplacer
	{
		private readonly SyntaxAnnotation _annotation = new("TypeParameterWithSameName");

		/// <inheritdoc cref="TypeParameterReplacer(string, string)"/>
		public TypeParameterReplacer()
		{
		}

		/// <inheritdoc cref="TypeParameterReplacer(string, string)"/>
		public TypeParameterReplacer(string identifier) : base(identifier)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="TypeParameterReplacer"/> class.
		/// </summary>
		/// <param name="identifier">Identifier to replace.</param>
		/// <param name="replacement">Identifier to replace with.</param>
		/// <exception cref="ArgumentException"><paramref name="identifier"/> cannot be <see langword="null"/> or empty. -or- <paramref name="replacement"/> cannot be <see langword="null"/> or empty.</exception>
		public TypeParameterReplacer(string identifier, string replacement) : base(identifier, replacement)
		{
		}

		/// <inheritdoc/>
		public override SyntaxNode? VisitIdentifierName(IdentifierNameSyntax node)
		{
			if (IsValidForReplace(node.Identifier, node))
			{
				node = node.WithIdentifier(GetReplacementToken(node.Identifier));
			}

			return base.VisitIdentifierName(node);
		}

		/// <inheritdoc/>
		public override SyntaxNode? VisitGenericName(GenericNameSyntax node)
		{
			if (IsValidForReplace(node.Identifier, node))
			{
				node = node.WithIdentifier(GetReplacementToken(node.Identifier));
			}

			return base.VisitGenericName(node);
		}

		/// <inheritdoc/>
		public override SyntaxNode? VisitMethodDeclaration(MethodDeclarationSyntax node)
		{
			if (ShouldAnnotate(node.TypeParameterList))
			{
				node = node.WithAdditionalAnnotations(_annotation);
			}

			return base.VisitMethodDeclaration(node);
		}

		/// <inheritdoc/>
		public override SyntaxNode? VisitDelegateDeclaration(DelegateDeclarationSyntax node)
		{
			if (ShouldAnnotate(node.TypeParameterList))
			{
				node = node.WithAdditionalAnnotations(_annotation);
			}

			return base.VisitDelegateDeclaration(node);
		}

		/// <inheritdoc/>
		public override SyntaxNode? VisitRecordDeclaration(RecordDeclarationSyntax node)
		{
			if (ShouldAnnotate(node.TypeParameterList))
			{
				node = node.WithAdditionalAnnotations(_annotation);
			}

			return base.VisitRecordDeclaration(node);
		}

		/// <inheritdoc/>
		public override SyntaxNode? VisitStructDeclaration(StructDeclarationSyntax node)
		{
			if (ShouldAnnotate(node.TypeParameterList))
			{
				node = node.WithAdditionalAnnotations(_annotation);
			}

			return base.VisitStructDeclaration(node);
		}

		/// <inheritdoc/>
		public override SyntaxNode? VisitClassDeclaration(ClassDeclarationSyntax node)
		{
			if (ShouldAnnotate(node.TypeParameterList))
			{
				node = node.WithAdditionalAnnotations(_annotation);
			}

			return base.VisitClassDeclaration(node);
		}

		/// <inheritdoc/>
		public override SyntaxNode? VisitInterfaceDeclaration(InterfaceDeclarationSyntax node)
		{
			if (ShouldAnnotate(node.TypeParameterList))
			{
				node = node.WithAdditionalAnnotations(_annotation);
			}

			return base.VisitInterfaceDeclaration(node);
		}

		private bool HasParameterWithIdentifier(MemberDeclarationSyntax member)
		{
			if (member is MethodDeclarationSyntax method)
			{
				return HasParameterWithIdentifier(method.TypeParameterList);
			}
			else if (member is TypeDeclarationSyntax type)
			{
				return HasParameterWithIdentifier(type.TypeParameterList);
			}

			return false;
		}

		private bool HasParameterWithIdentifier([NotNullWhen(true)] TypeParameterListSyntax? typeParameters)
		{
			return typeParameters is not null && typeParameters.Parameters.Any(p => ShouldReplace(p.Identifier));
		}

		private bool IsValidForReplace(in SyntaxToken identifier, SyntaxNode node)
		{
			return ShouldReplace(identifier) && node.FirstAncestorOrSelf<MemberDeclarationSyntax>(m => m.HasAnnotation(_annotation)) is null;
		}

		private bool ShouldAnnotate(TypeParameterListSyntax? typeParameters)
		{
			return
				HasParameterWithIdentifier(typeParameters) &&
				typeParameters.Parent?.Parent?.FirstAncestorOrSelf<MemberDeclarationSyntax>(HasParameterWithIdentifier) is not null;
		}
	}
}
