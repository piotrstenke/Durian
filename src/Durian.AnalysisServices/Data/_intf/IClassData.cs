﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.Analysis.Data;

/// <summary>
/// Encapsulates data associated with a single <see cref="INamedTypeSymbol"/> representing a class.
/// </summary>
public interface IClassData : ITypeData, ISymbolOrMember<INamedTypeSymbol, IClassData>
{
	/// <summary>
	/// Target <see cref="ClassDeclarationSyntax"/>.
	/// </summary>
	new ClassDeclarationSyntax Declaration { get; }

	/// <summary>
	/// Creates a shallow copy of the current data.
	/// </summary>
	new IClassData Clone();
}