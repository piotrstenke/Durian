// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Durian.Analysis.Data;
using Durian.Analysis.Extensions;
using Microsoft.CodeAnalysis;

using BaseInnerContainer = Durian.Analysis.SymbolContainers.LeveledSymbolContainer<Microsoft.CodeAnalysis.INamespaceOrTypeSymbol, Durian.Analysis.Data.INamespaceOrTypeData>.InnerContainer;
using BaseLevelEntry = Durian.Analysis.SymbolContainers.LeveledSymbolContainer<Microsoft.CodeAnalysis.INamespaceOrTypeSymbol, Durian.Analysis.Data.INamespaceOrTypeData>.LevelEntry;
using BaseList = System.Collections.Generic.List<Durian.Analysis.Data.ISymbolOrMember<Microsoft.CodeAnalysis.INamespaceOrTypeSymbol, Durian.Analysis.Data.INamespaceOrTypeData>>;

namespace Durian.Analysis.SymbolContainers.Specialized
{
	/// <summary>
	/// <see cref="ILeveledSymbolContainer{TSymbol, TData}"/> that handles sub-namespaces and types.
	/// </summary>
	public sealed class NamespacesOrTypesContainer : IncludedMembersSymbolContainer<INamespaceOrTypeSymbol, INamespaceOrTypeData>
	{
		private sealed class PrivateSubNamespacesContainer : IncludedMembersSymbolContainerWithoutInner<INamespaceSymbol, INamespaceData>
		{
			public NamespacesOrTypesContainer ParentContainer { get; }

			public PrivateSubNamespacesContainer(NamespacesOrTypesContainer parentContainer, ISymbolOrMember<INamespaceSymbol, INamespaceData> root)
				: base(root, parentContainer.ParentCompilation, parentContainer.SymbolNameResolver, parentContainer.IncludeRoot)
			{
				ParentContainer = parentContainer;
			}

			/// <inheritdoc/>
			private protected override bool IsHandledExternally(int level)
			{
#pragma warning disable CS0618 // Type or member is obsolete
				BaseInnerContainer? container = (ParentContainer.ResolveLevel(level) as BaseInnerContainer)!;
#pragma warning restore CS0618 // Type or member is obsolete

				BaseLevelEntry levelEntry = ParentContainer._levels[level];
				BaseList baseList = ParentContainer._data;

				if(levelEntry.IsEmpty)
				{
					return true;
				}

				int start = levelEntry.StartIndex;
				int end = container.EndIndex;

				for (int i = start; i < end; i++)
				{
					if(baseList[i] is ISymbolOrMember<INamespaceSymbol, INamespaceData> @namespace)
					{
						_data.Add(@namespace);
					}
				}

				return true;
			}

			/// <inheritdoc/>
			protected override IEnumerable<ISymbolOrMember<INamespaceSymbol, INamespaceData>> All(ISymbolOrMember<INamespaceSymbol, INamespaceData> member)
			{
				return GetNamespaces(member);
			}

			/// <inheritdoc/>
			protected override IEnumerable<ISymbolOrMember<INamespaceSymbol, INamespaceData>> Direct(ISymbolOrMember<INamespaceSymbol, INamespaceData> member)
			{
				return GetNamespaces(member);
			}

			/// <inheritdoc/>
			protected override IEnumerable<ISymbolOrMember<INamespaceSymbol, INamespaceData>> ResolveRoot(ISymbolOrMember root)
			{
				return GetNamespaces((root as ISymbolOrMember<INamespaceSymbol, INamespaceData>)!);
			}

			private IEnumerable<ISymbolOrMember<INamespaceSymbol, INamespaceData>> GetNamespaces(ISymbolOrMember<INamespaceSymbol, INamespaceData> member)
			{
				return member.Symbol.GetNamespaceMembers().Select(s => s.ToDataOrSymbol(ParentCompilation));
			}
		}

		private sealed class PrivateInnerTypesContainer : IncludedMembersSymbolContainerWithoutInner<INamedTypeSymbol, ITypeData>
		{
			public NamespacesOrTypesContainer ParentContainer { get; }

			public PrivateInnerTypesContainer(NamespacesOrTypesContainer parentContainer, ISymbolOrMember<INamedTypeSymbol, ITypeData> root)
				: base(root, parentContainer.ParentCompilation, parentContainer.SymbolNameResolver, parentContainer.IncludeRoot)
			{
				ParentContainer = parentContainer;
			}

			public PrivateInnerTypesContainer(NamespacesOrTypesContainer parentContainer, ISymbolOrMember<INamespaceOrTypeSymbol, INamespaceOrTypeData> root)
				: base(root, parentContainer.ParentCompilation, parentContainer.SymbolNameResolver)
			{
				ParentContainer = parentContainer;
			}

			/// <inheritdoc/>
			private protected override bool IsHandledExternally(int level)
			{
#pragma warning disable CS0618 // Type or member is obsolete
				BaseInnerContainer container = (ParentContainer.ResolveLevel(level) as BaseInnerContainer)!;
#pragma warning restore CS0618 // Type or member is obsolete

				BaseLevelEntry levelEntry = ParentContainer._levels[level];
				BaseList baseList = ParentContainer._data;

				if (levelEntry.IsEmpty)
				{
					return true;
				}

				int start = levelEntry.StartIndex;
				int end = container.EndIndex;

				for (int i = start; i < end; i++)
				{
					if (baseList[i] is ISymbolOrMember<INamedTypeSymbol, ITypeData> type)
					{
						_data.Add(type);
					}
				}

				return true;
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

			private IEnumerable<ISymbolOrMember<INamedTypeSymbol, ITypeData>> GetTypes(ISymbolOrMember<INamespaceOrTypeSymbol, INamespaceOrTypeData> member)
			{
				return member.Symbol.GetTypeMembers().Select(s => s.ToDataOrSymbol(ParentCompilation));
			}
		}

		private readonly IMappedSymbolContainer<INamespaceSymbol, INamespaceData, IncludedMembers> _subNamespaces;
		private readonly PrivateInnerTypesContainer _innerTypes;

		/// <summary>
		/// Initializes a new instance of the <see cref="NamespacesOrTypesContainer"/> class.
		/// </summary>
		/// <param name="root"><see cref="ISymbol"/> that is a root of all the underlaying containers.</param>
		/// <param name="parentCompilation"><see cref="ICompilationData"/> used to create <see cref="INamespaceOrTypeData"/>s.</param>
		/// <param name="nameResolver"><see cref="ISymbolNameResolver"/> used to resolve names of symbols when <see cref="ISymbolContainer.GetNames"/> is called.</param>
		/// <param name="includeRoot">Determines whether the <paramref name="root"/> should be included in the underlaying containers.</param>
		/// <exception cref="ArgumentNullException"><paramref name="root"/> is <see langword="null"/>.</exception>
		public NamespacesOrTypesContainer(
			ISymbolOrMember<INamespaceOrTypeSymbol, INamespaceOrTypeData> root,
			ICompilationData? parentCompilation = default,
			ISymbolNameResolver? nameResolver = default,
			bool includeRoot = false
		) : base(root, parentCompilation, nameResolver, includeRoot)
		{
			if (root is ISymbolOrMember<INamespaceSymbol, INamespaceData> @namespace)
			{
				_subNamespaces = new PrivateSubNamespacesContainer(this, @namespace);
			}
			else
			{
				_subNamespaces = (SymbolContainerFactory.EmptyLeveled<INamespaceSymbol, INamespaceData>() as IMappedSymbolContainer<INamespaceSymbol, INamespaceData, IncludedMembers>)!;
			}

			_innerTypes = new PrivateInnerTypesContainer(this, root);
		}

		/// <summary>
		/// Returns a <see cref="IMappedSymbolContainer{TSymbol, TData, TLevel}"/> created from namespaces contained within this container.
		/// <para><b>Note:</b> this container and the returned <see cref="IMappedSymbolContainer{TSymbol, TData, TLevel}"/> use the same set of data -
		/// resolving a level for the <see cref="IMappedSymbolContainer{TSymbol, TData, TLevel}"/> will also resolve the same level for the current container and vice versa.</para>
		/// </summary>
		public IMappedSymbolContainer<INamespaceSymbol, INamespaceData, IncludedMembers> GetNamespaces()
		{
			return _subNamespaces;
		}

		/// <summary>
		/// Returns a <see cref="IMappedSymbolContainer{TSymbol, TData, TLevel}"/> created from data within this container.
		/// <para><b>Note:</b> this container and the returned <see cref="IMappedSymbolContainer{TSymbol, TData, TLevel}"/> use the same set of data -
		/// resolving a level for the <see cref="IMappedSymbolContainer{TSymbol, TData, TLevel}"/> will also resolve the same level for the current container and vice versa.</para>
		/// </summary>
		public IMappedSymbolContainer<INamedTypeSymbol, ITypeData, IncludedMembers> GetTypes()
		{
			return _innerTypes;
		}

		/// <inheritdoc/>
		protected override void OnLevelFilled(IncludedMembers level)
		{
			_innerTypes.ResolveLevel(level);

			if(_subNamespaces is IMappedSymbolContainer<IncludedMembers> mapped)
			{
				mapped.ResolveLevel(level);
			}
			else
			{
				_subNamespaces.ResolveLevel(MapLevel(level));
			}
		}

		/// <inheritdoc/>
		protected override void OnLevelCleared(IncludedMembers level)
		{
			_innerTypes.ClearLevel(level);

			if(_subNamespaces is IMappedSymbolContainer<IncludedMembers> mapped)
			{
				mapped.ClearLevel(level);
			}
			else
			{
				_subNamespaces.ClearLevel(MapLevel(level));
			}
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
		public new NamespacesOrTypesContainer Reverse()
		{
			return (base.Reverse() as NamespacesOrTypesContainer)!;
		}

		private IEnumerable<ISymbolOrMember<INamespaceOrTypeSymbol, INamespaceOrTypeData>> GetNamespacesOrTypes(ISymbolOrMember<INamespaceOrTypeSymbol, INamespaceOrTypeData> member)
		{
			INamespaceOrTypeSymbol symbol = member.Symbol;

			if(symbol.IsNamespace)
			{
				return (symbol as INamespaceSymbol)!.GetMembers().Select(s => s.ToDataOrSymbol(ParentCompilation));
			}

			return symbol.GetTypeMembers().Select(s => (s as INamespaceOrTypeSymbol).ToDataOrSymbol(ParentCompilation));
		}
	}
}
