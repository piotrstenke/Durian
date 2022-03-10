// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Microsoft.CodeAnalysis.CSharp;

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
		public string? Identifier { get; set; }

		/// <inheritdoc cref="IdentifierReplacer(string, CSharpSyntaxNode)"/>
		public IdentifierReplacer()
		{
		}

		/// <inheritdoc cref="IdentifierReplacer(string, CSharpSyntaxNode)"/>
		public IdentifierReplacer(string? identifier)
		{
			Identifier = identifier;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="IdentifierReplacer"/> class.
		/// </summary>
		/// <param name="identifier">Identifier to replace.</param>
		/// <param name="replacement"><see cref="CSharpSyntaxNode"/> that is the replacement.</param>
		public IdentifierReplacer(string? identifier, CSharpSyntaxNode? replacement)
		{
			Identifier = identifier;
			Replacement = replacement;
		}
	}
}