// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.Analysis.SyntaxVisitors
{
	/// <summary>
	/// Replaces a given identifier with a specified <see cref="SyntaxNode"/>.
	/// </summary>
	public class IdentifierReplacer : NodeReplacer
	{
		private string? _identifier;
		private string? _replacement;

		/// <summary>
		/// Identifier to replace.
		/// </summary>
		/// <exception cref="ArgumentException"><paramref name="value"/> cannot be <see langword="null"/> or empty.</exception>
		public string Identifier
		{
			get => _identifier ??= string.Empty;
			set
			{
				if (string.IsNullOrWhiteSpace(value))
				{
					throw new ArgumentException("Identifier cannot be null or empty", nameof(Identifier));
				}

				_identifier = value;
			}
		}

		/// <summary>
		/// Identifier to replace with.
		/// </summary>
		/// <exception cref="ArgumentException"><paramref name="value"/> cannot be <see langword="null"/> or empty.</exception>
		public new string Replacement
		{
			get => _replacement ??= string.Empty;
			set
			{
				if (string.IsNullOrWhiteSpace(value))
				{
					throw new ArgumentException("Replacement cannot be null or empty", nameof(Replacement));
				}

				_replacement = value;
				base.Replacement = SyntaxFactory.IdentifierName(_replacement);
			}
		}

		/// <inheritdoc cref="IdentifierReplacer(string, string)"/>
		public IdentifierReplacer()
		{
		}

		/// <inheritdoc cref="IdentifierReplacer(string, string)"/>
		public IdentifierReplacer(string identifier) : this(identifier, identifier)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="IdentifierReplacer"/> class.
		/// </summary>
		/// <param name="identifier">Identifier to replace.</param>
		/// <param name="replacement">Identifier to replace with.</param>
		/// <exception cref="ArgumentException"><paramref name="identifier"/> cannot be <see langword="null"/> or empty. -or- <paramref name="replacement"/> cannot be <see langword="null"/> or empty.</exception>
		public IdentifierReplacer(string identifier, string replacement)
		{
			if (string.IsNullOrWhiteSpace(identifier))
			{
				throw new ArgumentException("Identifier cannot be null or empty", nameof(identifier));
			}

			if (string.IsNullOrWhiteSpace(replacement))
			{
				throw new ArgumentException("Identifier cannot be null or empty", nameof(replacement));
			}

			_identifier = identifier;
			_replacement = replacement;
		}

		/// <inheritdoc/>
		public override SyntaxNode? VisitGenericName(GenericNameSyntax node)
		{
			if (ShouldReplace(node.Identifier))
			{
				node = node.WithIdentifier(GetReplacementToken(node.Identifier));
			}

			return base.VisitGenericName(node);
		}

		/// <inheritdoc/>
		public override SyntaxNode? VisitIdentifierName(IdentifierNameSyntax node)
		{
			if (ShouldReplace(node.Identifier))
			{
				node = node.WithIdentifier(GetReplacementToken(node.Identifier));
			}

			return base.VisitIdentifierName(node);
		}

		/// <summary>
		/// Returns a <see cref="SyntaxToken"/> the current <see cref="SyntaxToken"/> should be replaced with.
		/// </summary>
		/// <param name="previous">Previous <see cref="SyntaxToken"/>.</param>
		protected SyntaxToken GetReplacementToken(in SyntaxToken previous)
		{
			return SyntaxFactory.Identifier(Replacement).WithTriviaFrom(previous);
		}

		/// <summary>
		/// Determines whether the specified <paramref name="identifier"/> should be replaced.
		/// </summary>
		/// <param name="identifier"><see cref="SyntaxToken"/> to determine whether should be replaced.</param>
		protected bool ShouldReplace(in SyntaxToken identifier)
		{
			return identifier.Text == Identifier;
		}
	}
}
