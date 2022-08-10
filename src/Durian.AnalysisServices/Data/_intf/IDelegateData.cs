// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.Analysis.Data
{
	/// <summary>
	/// Encapsulates data associated with a single <see cref="INamedTypeSymbol"/> representing a delegate.
	/// </summary>
	public interface IDelegateData : ITypeData, ISymbolOrMember<INamedTypeSymbol, IDelegateData>
	{
		/// <summary>
		/// Target <see cref="DelegateDeclarationSyntax"/>.
		/// </summary>
		new DelegateDeclarationSyntax Declaration { get; }

		/// <summary>
		/// Creates a shallow copy of the current data.
		/// </summary>
		new IDelegateData Clone();
	}
}