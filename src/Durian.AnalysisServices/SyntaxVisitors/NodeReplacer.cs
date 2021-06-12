﻿// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Durian.Generator.SyntaxVisitors
{
	/// <summary>
	/// Replaces <see cref="CSharpSyntaxNode"/>s of the specified type with the provided <see cref="Replacement"/>.
	/// </summary>
	public abstract class NodeReplacer : CSharpSyntaxRewriter
	{
		/// <summary>
		/// <see cref="CSharpSyntaxNode"/> that is the replacement.
		/// </summary>
		[MaybeNull]
		public virtual CSharpSyntaxNode Replacement { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="NodeReplacer"/> class.
		/// </summary>
		protected NodeReplacer()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="NodeReplacer"/> class.
		/// </summary>
		/// <param name="replacement"><see cref="CSharpSyntaxNode"/> that is the replacement.</param>
		protected NodeReplacer(CSharpSyntaxNode replacement)
		{
			Replacement = replacement;
		}
	}
}
