using Durian.Analysis.SymbolContainers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.Analysis.Data;

/// <summary>
/// Encapsulates data associated with a single <see cref="IPropertySymbol"/> representing an indexer.
/// </summary>
public interface IIndexerData : IPropertyData, ISymbolOrMember<IPropertySymbol, IIndexerData>
{
	/// <summary>
	/// Target <see cref="BasePropertyDeclarationSyntax"/>.
	/// </summary>
	new IndexerDeclarationSyntax Declaration { get; }

	/// <summary>
	/// Interface methods this <see cref="IPropertySymbol"/> explicitly implements.
	/// </summary>
	new ISymbolOrMember<IPropertySymbol, IIndexerData>? ExplicitInterfaceImplementation { get; }

	/// <summary>
	/// Interface methods this <see cref="IPropertySymbol"/> implicitly implements
	/// </summary>
	new ISymbolContainer<IPropertySymbol, IIndexerData> ImplicitInterfaceImplementations { get; }

	/// <summary>
	/// Indexer overridden by this indexer.
	/// </summary>
	ISymbolOrMember<IPropertySymbol, IIndexerData>? OverriddenIndexer { get; }

	/// <summary>
	/// All indexers overridden by this indexer.
	/// </summary>
	ISymbolContainer<IPropertySymbol, IIndexerData> OverriddenIndexers { get; }

	/// <summary>
	/// Creates a shallow copy of the current data.
	/// </summary>
	new IIndexerData Clone();
}
