// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Durian.Analysis.SymbolContainers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.Analysis.Data
{
	/// <summary>
	/// Encapsulates data associated with a single <see cref="IMethodSymbol"/> representing an constructor.
	/// </summary>
	public interface IConstructorData : IMethodData, ISymbolOrMember<IMethodSymbol, IConstructorData>
	{
		/// <summary>
		/// Base constructor of this constructor.
		/// </summary>
		ISymbolOrMember<IMethodSymbol, IConstructorData>? BaseConstructor { get; }

		/// <summary>
		/// All base constructors of this constructor.
		/// </summary>
		ISymbolContainer<IMethodSymbol, IConstructorData> BaseConstructors { get; }

		/// <summary>
		/// Target <see cref="ConstructorDeclarationSyntax"/>.
		/// </summary>
		new ConstructorDeclarationSyntax Declaration { get; }

		/// <summary>
		/// Determines whether this constructor is a primary constructor.
		/// </summary>
		bool IsPrimary { get; }

		/// <summary>
		/// Kind of the constructor, or <see cref="SpecialConstructor.None"/> if its not special.
		/// </summary>
		SpecialConstructor SpecialKind { get; }

		/// <summary>
		/// Creates a shallow copy of the current data.
		/// </summary>
		new IConstructorData Clone();
	}
}