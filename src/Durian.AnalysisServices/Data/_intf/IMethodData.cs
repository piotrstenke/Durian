// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Durian.Analysis.CodeGeneration;
using Durian.Analysis.SymbolContainers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
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
		CSharpSyntaxNode? Body { get; }

		/// <summary>
		/// Type of body of the method.
		/// </summary>
		MethodStyle BodyType { get; }

		/// <summary>
		/// <see cref="IMethodSymbol"/> associated with the <see cref="Declaration"/>.
		/// </summary>
		new IMethodSymbol Symbol { get; }

		/// <summary>
		/// Type parameters of this method.
		/// </summary>
		ISymbolContainer<ITypeParameterSymbol> TypeParameters { get; }

		/// <summary>
		/// Overloads of this method.
		/// </summary>
		ISymbolContainer<IMethodSymbol> Overloads { get; }

		/// <summary>
		/// Methods overridden by this method.
		/// </summary>
		ISymbolContainer<IMethodSymbol> OverridenMethods { get; }

		/// <summary>
		/// Parameters of this method.
		/// </summary>
		ISymbolContainer<IParameterSymbol> Parameters { get; }
	}
}
