using System;
using Durian.Analysis.SymbolContainers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.Analysis.Data;

/// <summary>
/// Encapsulates data associated with a single <see cref="INamedTypeSymbol"/> representing an enum.
/// </summary>
public interface IEnumData : ITypeData, ISymbolOrMember<INamedTypeSymbol, IEnumData>
{
	/// <summary>
	/// <see cref="INamedTypeSymbol"/> associated with the enum's underlaying type.
	/// </summary>
	ISymbolOrMember<INamedTypeSymbol, ITypeData> BaseType { get; }

	/// <summary>
	/// <see cref="TypeKeyword"/> associated with the enum's underlaying type.
	/// </summary>
	TypeKeyword BaseTypeKeyword { get; }

	/// <summary>
	/// Target <see cref="EnumDeclarationSyntax"/>.
	/// </summary>
	new EnumDeclarationSyntax Declaration { get; }

	/// <summary>
	/// All fields of the of the enum.
	/// </summary>
	ISymbolContainer<IFieldSymbol, IFieldData> Fields { get; }

	/// <summary>
	/// Determines whether the enum has the <see cref="FlagsAttribute"/> applied.
	/// </summary>
	bool IsFlags { get; }

	/// <summary>
	/// Creates a shallow copy of the current data.
	/// </summary>
	new IEnumData Clone();
}