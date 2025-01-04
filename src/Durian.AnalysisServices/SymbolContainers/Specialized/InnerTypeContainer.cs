using System;
using System.Collections.Generic;
using System.Linq;
using Durian.Analysis.Data;
using Durian.Analysis.Extensions;
using Microsoft.CodeAnalysis;

namespace Durian.Analysis.SymbolContainers.Specialized;

/// <summary>
/// <see cref="ILeveledSymbolContainer{TSymbol, TData}"/> that handles inner types.
/// </summary>
public sealed class InnerTypeContainer : IncludedMemberContainerWithoutInner<INamedTypeSymbol, ITypeData>
{
	/// <inheritdoc cref="LeveledSymbolContainer{TSymbol, TData}.TargetRoot"/>
	public new ISymbolOrMember<INamespaceOrTypeSymbol, INamespaceOrTypeData> TargetRoot => (base.TargetRoot as ISymbolOrMember<INamespaceOrTypeSymbol, INamespaceOrTypeData>)!;

	/// <summary>
	/// Initializes a new instance of the <see cref="InnerTypeContainer"/> class.
	/// </summary>
	/// <param name="root"><see cref="ISymbol"/> that is a root of all the underlaying containers.</param>
	/// <param name="parentCompilation"><see cref="ICompilationData"/> used to create <see cref="ITypeData"/>s.</param>
	/// <param name="nameResolver"><see cref="ISymbolNameResolver"/> used to resolve names of symbols when <see cref="ISymbolContainer.GetNames"/> is called.</param>
	/// <param name="includeRoot">Determines whether the <paramref name="root"/> should be included in the underlaying containers.</param>
	/// <exception cref="ArgumentNullException"><paramref name="root"/> is <see langword="null"/>.</exception>
	public InnerTypeContainer(
		ISymbolOrMember<INamedTypeSymbol, ITypeData> root,
		ICompilationData? parentCompilation = default,
		ISymbolNameResolver? nameResolver = default,
		bool includeRoot = false
	) : base(root, parentCompilation, nameResolver, includeRoot)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="InnerTypeContainer"/> class.
	/// </summary>
	/// <param name="root"><see cref="ISymbol"/> that is a root of all the underlaying containers.</param>
	/// <param name="parentCompilation"><see cref="ICompilationData"/> used to create <see cref="ITypeData"/>s.</param>
	/// <param name="nameResolver"><see cref="ISymbolNameResolver"/> used to resolve names of symbols when <see cref="ISymbolContainer.GetNames"/> is called.</param>
	/// <exception cref="ArgumentNullException"><paramref name="root"/> is <see langword="null"/>.</exception>
	public InnerTypeContainer(
		ISymbolOrMember<INamespaceOrTypeSymbol, INamespaceOrTypeData> root,
		ICompilationData? parentCompilation = default,
		ISymbolNameResolver? nameResolver = default
	) : base(root, parentCompilation, nameResolver)
	{
	}

	/// <inheritdoc/>
	protected override IEnumerable<ISymbolOrMember<INamedTypeSymbol, ITypeData>> All(ISymbolOrMember<INamedTypeSymbol, ITypeData> member)
	{
		return GetTypes(member);
	}

	/// <inheritdoc/>
	protected override IEnumerable<ISymbolOrMember<INamedTypeSymbol, ITypeData>> Direct(ISymbolOrMember<INamedTypeSymbol, ITypeData> member)
	{
		return GetTypes(member);
	}

	/// <inheritdoc/>
	protected override IEnumerable<ISymbolOrMember<INamedTypeSymbol, ITypeData>> ResolveRoot(ISymbolOrMember root)
	{
		return GetTypes((root as ISymbolOrMember<INamespaceOrTypeSymbol, INamespaceOrTypeData>)!);
	}

	/// <inheritdoc cref="LeveledSymbolContainer{TSymbol, TData}.Reverse"/>
	public new InnerTypeContainer Reverse()
	{
		return (base.Reverse() as InnerTypeContainer)!;
	}

	private IEnumerable<ISymbolOrMember<INamedTypeSymbol, ITypeData>> GetTypes(ISymbolOrMember<INamespaceOrTypeSymbol, INamespaceOrTypeData> member)
	{
		return member.Symbol.GetTypeMembers().Select(s => s.ToDataOrSymbol(ParentCompilation));
	}
}
