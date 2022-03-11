// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.Analysis.SyntaxVisitors
{
	/// <summary>
	/// Replaces a given identifier with a specified <see cref="CSharpSyntaxNode"/>.
	/// </summary>
	public class IdentifierReplacer : NodeReplacer
	{
		/// <summary>
		/// Identifier to replace.
		/// </summary>
		public string Identifier { get; }

		/// <inheritdoc cref="IdentifierReplacer(string, CSharpSyntaxNode)"/>
		public IdentifierReplacer(string identifier)
		{
			Identifier = identifier;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="IdentifierReplacer"/> class.
		/// </summary>
		/// <param name="identifier">Identifier to replace.</param>
		/// <param name="replacement"><see cref="CSharpSyntaxNode"/> that is the replacement.</param>
		/// <exception cref="ArgumentException"><paramref name="identifier"/> cannot be <see langword="null"/> or empty.</exception>
		public IdentifierReplacer(string identifier, CSharpSyntaxNode? replacement)
		{
			if(string.IsNullOrWhiteSpace(identifier))
			{
				throw new ArgumentException("Identifier cannot be null or empty", nameof(identifier));
			}

			Identifier = identifier;
			Replacement = replacement;
		}

		/// <inheritdoc/>
		public override SyntaxNode? VisitIdentifierName(IdentifierNameSyntax node)
		{
			if(ShouldReplace(node.Identifier))
			{
				node = node.WithIdentifier(GetIdentifierToken());
			}

			return node;
		}

		/// <inheritdoc/>
		public override SyntaxNode? VisitGenericName(GenericNameSyntax node)
		{
			if (ShouldReplace(node.Identifier))
			{
				node = node.WithIdentifier(GetIdentifierToken());
			}

			return base.VisitGenericName(node);
		}

		/// <summary>
		/// Determines whether the specified <paramref name="identifier"/> should be replaced.
		/// </summary>
		/// <param name="identifier"><see cref="SyntaxToken"/> to determine whether should be replaced.</param>
		protected bool ShouldReplace(SyntaxToken identifier)
		{
			return identifier.Text == Identifier;
		}

		/// <summary>
		/// Returns a <see cref="SyntaxToken"/> the current <see cref="SyntaxToken"/> should be replaced with.
		/// </summary>
		protected SyntaxToken GetIdentifierToken()
		{
			return SyntaxFactory.Identifier(Identifier!);
		}
	}
}
