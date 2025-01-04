using System;
using System.Collections.Generic;
using System.Linq;
using Durian.Analysis.Data;
using Durian.Analysis.Extensions;
using Microsoft.CodeAnalysis;

namespace Durian.Analysis.SymbolContainers.Specialized;

/// <summary>
/// <see cref="ILeveledSymbolContainer{TSymbol, TData}"/> that handles method overloads.
/// </summary>
public sealed class MethodOverloadContainer : IncludedMemberContainerWithoutInner<IMethodSymbol, IMethodData>
{
	/// <summary>
	/// Initializes a new instance of the <see cref="MethodOverloadContainer"/> class.
	/// </summary>
	/// <param name="root"><see cref="ISymbol"/> that is a root of all the underlaying containers.</param>
	/// <param name="parentCompilation"><see cref="ICompilationData"/> used to create <see cref="INamespaceData"/>s.</param>
	/// <param name="nameResolver"><see cref="ISymbolNameResolver"/> used to resolve names of symbols when <see cref="ISymbolContainer.GetNames"/> is called.</param>
	/// <param name="includeRoot">Determines whether the <paramref name="root"/> should be included in the underlaying containers.</param>
	/// <exception cref="ArgumentNullException"><paramref name="root"/> is <see langword="null"/>.</exception>
	public MethodOverloadContainer(
		ISymbolOrMember<IMethodSymbol, IMethodData> root,
		ICompilationData? parentCompilation = default,
		ISymbolNameResolver? nameResolver = default,
		bool includeRoot = false
	) : base(root, parentCompilation, nameResolver, includeRoot)
	{
	}

	/// <inheritdoc/>
	protected override IEnumerable<ISymbolOrMember<IMethodSymbol, IMethodData>> All(ISymbolOrMember<IMethodSymbol, IMethodData> member)
	{
		return GetOverloads(member);
	}

	/// <inheritdoc/>
	protected override IEnumerable<ISymbolOrMember<IMethodSymbol, IMethodData>> Direct(ISymbolOrMember<IMethodSymbol, IMethodData> member)
	{
		return GetOverloads(member);
	}

	/// <inheritdoc/>
	protected override IEnumerable<ISymbolOrMember<IMethodSymbol, IMethodData>> ResolveRoot(ISymbolOrMember root)
	{
		return GetOverloads((root as SymbolOrMemberWrapper<IMethodSymbol, IMethodData>)!);
	}

	/// <inheritdoc cref="LeveledSymbolContainer{TSymbol, TData}.Reverse"/>
	public new MethodOverloadContainer Reverse()
	{
		return (base.Reverse() as MethodOverloadContainer)!;
	}

	private IEnumerable<ISymbolOrMember<IMethodSymbol, IMethodData>> GetOverloads(ISymbolOrMember<IMethodSymbol, IMethodData> member)
	{
		return member.Symbol.GetOverloads(false).Select(s => s.ToDataOrSymbol(ParentCompilation));
	}
}
