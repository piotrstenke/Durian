using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Durian.Analysis.Data;
using Durian.Analysis.Extensions;
using Microsoft.CodeAnalysis;

namespace Durian.Analysis.SymbolContainers.Specialized
{
	/// <summary>
	/// <see cref="ILeveledSymbolContainer{TSymbol, TData}"/> that handles sub-namespaces and types.
	/// </summary>
	public sealed class NamespaceOrTypeContainer : IncludedMemberContainer<INamespaceOrTypeSymbol, INamespaceOrTypeData>
	{
		private IMappedSymbolContainer<INamespaceSymbol, INamespaceData, IncludedMembers>? _subNamespaces;
		private GenericInnerMemberContainer<INamedTypeSymbol, ITypeData, INamespaceOrTypeSymbol, INamespaceOrTypeData>? _innerTypes;

		/// <summary>
		/// Initializes a new instance of the <see cref="NamespaceOrTypeContainer"/> class.
		/// </summary>
		/// <param name="root"><see cref="ISymbol"/> that is a root of all the underlaying containers.</param>
		/// <param name="parentCompilation"><see cref="ICompilationData"/> used to create <see cref="INamespaceOrTypeData"/>s.</param>
		/// <param name="nameResolver"><see cref="ISymbolNameResolver"/> used to resolve names of symbols when <see cref="ISymbolContainer.GetNames"/> is called.</param>
		/// <param name="includeRoot">Determines whether the <paramref name="root"/> should be included in the underlaying containers.</param>
		/// <exception cref="ArgumentNullException"><paramref name="root"/> is <see langword="null"/>.</exception>
		public NamespaceOrTypeContainer(
			ISymbolOrMember<INamespaceOrTypeSymbol, INamespaceOrTypeData> root,
			ICompilationData? parentCompilation = default,
			ISymbolNameResolver? nameResolver = default,
			bool includeRoot = false
		) : base(root, parentCompilation, nameResolver, includeRoot)
		{
		}

		/// <summary>
		/// Returns a <see cref="IMappedSymbolContainer{TSymbol, TData, TLevel}"/> created from namespaces contained within this container.
		/// <para><b>Note:</b> this container and the returned <see cref="IMappedSymbolContainer{TSymbol, TData, TLevel}"/> use the same set of data -
		/// resolving a level for the <see cref="IMappedSymbolContainer{TSymbol, TData, TLevel}"/> will also resolve the same level for the current container and vice versa.</para>
		/// </summary>
		public IMappedSymbolContainer<INamespaceSymbol, INamespaceData, IncludedMembers> GetNamespaces()
		{
			return _subNamespaces ??= CreateSubNamespacesContainer();
		}

		/// <summary>
		/// Returns a <see cref="IMappedSymbolContainer{TSymbol, TData, TLevel}"/> created from types contained within this container.
		/// <para><b>Note:</b> this container and the returned <see cref="IMappedSymbolContainer{TSymbol, TData, TLevel}"/> use the same set of data -
		/// resolving a level for the <see cref="IMappedSymbolContainer{TSymbol, TData, TLevel}"/> will also resolve the same level for the current container and vice versa.</para>
		/// </summary>
		public IMappedSymbolContainer<INamedTypeSymbol, ITypeData, IncludedMembers> GetTypes()
		{
			return _innerTypes ??= new GenericInnerMemberContainer<INamedTypeSymbol, ITypeData, INamespaceOrTypeSymbol, INamespaceOrTypeData>(this, TargetRoot);
		}

		/// <inheritdoc/>
		protected override IEnumerable<ISymbolOrMember<INamespaceOrTypeSymbol, INamespaceOrTypeData>> All(ISymbolOrMember<INamespaceOrTypeSymbol, INamespaceOrTypeData> member)
		{
			return GetNamespacesOrTypes(member);
		}

		/// <inheritdoc/>
		protected override IEnumerable<ISymbolOrMember<INamespaceOrTypeSymbol, INamespaceOrTypeData>> Direct(ISymbolOrMember<INamespaceOrTypeSymbol, INamespaceOrTypeData> member)
		{
			return GetNamespacesOrTypes(member);
		}

		/// <inheritdoc/>
		protected override IEnumerable<ISymbolOrMember<INamespaceOrTypeSymbol, INamespaceOrTypeData>> Inner(ISymbolOrMember<INamespaceOrTypeSymbol, INamespaceOrTypeData> member)
		{
			return GetNamespacesOrTypes(member);
		}

		/// <inheritdoc/>
		protected override IEnumerable<ISymbolOrMember<INamespaceOrTypeSymbol, INamespaceOrTypeData>> ResolveRoot(ISymbolOrMember root)
		{
			return GetNamespacesOrTypes((root as ISymbolOrMember<INamespaceOrTypeSymbol, INamespaceOrTypeData>)!);
		}

		/// <inheritdoc cref="LeveledSymbolContainer{TSymbol, TData}.Reverse"/>
		public new NamespaceOrTypeContainer Reverse()
		{
			return (base.Reverse() as NamespaceOrTypeContainer)!;
		}

		private IEnumerable<ISymbolOrMember<INamespaceOrTypeSymbol, INamespaceOrTypeData>> GetNamespacesOrTypes(ISymbolOrMember<INamespaceOrTypeSymbol, INamespaceOrTypeData> member)
		{
			INamespaceOrTypeSymbol symbol = member.Symbol;

			if (symbol.IsNamespace)
			{
				return (symbol as INamespaceSymbol)!.GetMembers().Select(s => s.ToDataOrSymbol(ParentCompilation));
			}

			return symbol.GetTypeMembers().Select(s => (s as INamespaceOrTypeSymbol).ToDataOrSymbol(ParentCompilation));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private IMappedSymbolContainer<INamespaceSymbol, INamespaceData, IncludedMembers> CreateSubNamespacesContainer()
		{
			if (Root is ISymbolOrMember<INamespaceSymbol, INamespaceData> @namespace)
			{
				return new GenericInnerMemberContainer<INamespaceSymbol, INamespaceData, INamespaceOrTypeSymbol, INamespaceOrTypeData>(this, @namespace);
			}

			return (SymbolContainerFactory.EmptyLeveled<INamespaceSymbol, INamespaceData>() as IMappedSymbolContainer<INamespaceSymbol, INamespaceData, IncludedMembers>)!;
		}
	}
}
