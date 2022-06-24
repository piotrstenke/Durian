// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.Analysis.Data
{
	/// <summary>
	/// Provides a <see cref="VariableDeclaratorSyntax"/> and an actual <see cref="Declaration"/> of a <see cref="ISymbol"/>.
	/// </summary>
	/// <typeparam name="TSyntax">Type of declaration.</typeparam>
	public interface IDeclarator<TSyntax> where TSyntax : CSharpSyntaxNode
	{
		/// <summary>
		/// Target <typeparamref name="TSyntax"/>.
		/// </summary>
		TSyntax Declaration { get; }

		/// <summary>
		/// Index of the <see cref="Variable"/> in the <see cref="Declaration"/>.
		/// </summary>
		int Index { get; }

		/// <summary>
		/// <see cref="VariableDeclaratorSyntax"/> of the member.
		/// </summary>
		VariableDeclaratorSyntax Variable { get; }
	}
}
