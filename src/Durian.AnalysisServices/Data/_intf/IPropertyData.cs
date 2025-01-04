using Durian.Analysis.SymbolContainers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.Analysis.Data;

/// <summary>
/// Encapsulates data associated with a single <see cref="IPropertySymbol"/>.
/// </summary>
public interface IPropertyData : IMemberData, ISymbolOrMember<IPropertySymbol, IPropertyData>
{
	/// <summary>
	/// Kind of the auto-property.
	/// </summary>
	AutoPropertyKind AutoPropertyKind { get; }

	/// <summary>
	/// Backing field of the property or <see langword="null"/> if not an auto-property.
	/// </summary>
	ISymbolOrMember<IFieldSymbol, IFieldData>? BackingField { get; }

	/// <summary>
	/// Target <see cref="BasePropertyDeclarationSyntax"/>.
	/// </summary>
	new BasePropertyDeclarationSyntax Declaration { get; }

	/// <summary>
	/// Interface methods this <see cref="IPropertySymbol"/> explicitly implements.
	/// </summary>
	ISymbolOrMember<IPropertySymbol, IPropertyData>? ExplicitInterfaceImplementation { get; }

	/// <summary>
	/// Interface methods this <see cref="IPropertySymbol"/> implicitly implements
	/// </summary>
	ISymbolContainer<IPropertySymbol, IPropertyData> ImplicitInterfaceImplementations { get; }

	/// <summary>
	/// Determines whether this property is a default interface implementation.
	/// </summary>
	bool IsDefaultImplementation { get; }

	/// <summary>
	/// Property overridden by this property.
	/// </summary>
	ISymbolOrMember<IPropertySymbol, IPropertyData>? OverriddenProperty { get; }

	/// <summary>
	/// All properties overridden by this property.
	/// </summary>
	ISymbolContainer<IPropertySymbol, IPropertyData> OverriddenProperties { get; }

	/// <summary>
	/// Parameters of this property.
	/// </summary>
	ISymbolContainer<IParameterSymbol, IParameterData> Parameters { get; }

	/// <summary>
	/// <see cref="ISymbol"/> associated with the <see cref="Declaration"/>.
	/// </summary>
	new IPropertySymbol Symbol { get; }

	/// <summary>
	/// Creates a shallow copy of the current data.
	/// </summary>
	new IPropertyData Clone();
}
