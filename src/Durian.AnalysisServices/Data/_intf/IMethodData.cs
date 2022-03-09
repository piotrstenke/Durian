// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.Analysis.Data
{
	/// <summary>
	/// Encapsulates data associated with a single <see cref="BaseMethodDeclarationSyntax"/>.
	/// </summary>
	public interface IMethodData : IMemberData
	{
		/// <summary>
		/// Body of the method.
		/// </summary>
		public SyntaxNode? Body { get; }

		/// <summary>
		/// Target <see cref="BaseMethodDeclarationSyntax"/>.
		/// </summary>
		new BaseMethodDeclarationSyntax Declaration { get; }

		/// <summary>
		/// <see cref="IMethodSymbol"/> associated with the <see cref="Declaration"/>.
		/// </summary>
		new IMethodSymbol Symbol { get; }
	}
}