using System;
using System.Collections.Generic;
using System.Linq;
using Durian.Analysis.Data;
using Durian.Analysis.Extensions;
using Microsoft.CodeAnalysis;

namespace Durian.Analysis.SymbolContainers.Specialized
{
	/// <summary>
	/// <see cref="ILeveledSymbolContainer{TSymbol, TData}"/> that handles members of a type.
	/// </summary>
	public sealed class InnerMemberContainer : IncludedMemberContainerWithoutInner<ISymbol, IMemberData>
	{
		private GenericInnerMemberContainer<INamedTypeSymbol, ITypeData, ISymbol, IMemberData>? _innerTypes;
		private GenericInnerMemberContainer<IPropertySymbol, IPropertyData, ISymbol, IMemberData>? _properties;
		private GenericInnerMemberContainer<IFieldSymbol, IFieldData, ISymbol, IMemberData>? _fields;
		private GenericInnerMemberContainer<IMethodSymbol, IMethodData, ISymbol, IMemberData>? _methods;
		private GenericInnerMemberContainer<IEventSymbol, IEventData, ISymbol, IMemberData>? _events;

		/// <inheritdoc cref="LeveledSymbolContainer{TSymbol, TData}.Root"/>
		public new ISymbolOrMember<INamedTypeSymbol, ITypeData> Root => (base.Root as ISymbolOrMember<INamedTypeSymbol, ITypeData>)!;

		/// <inheritdoc cref="LeveledSymbolContainer{TSymbol, TData}.TargetRoot"/>
		[Obsolete("Use Root instead.")]
		public new ISymbolOrMember<INamedTypeSymbol, ITypeData> TargetRoot => Root;

		/// <summary>
		/// Initializes a new instance of the <see cref="InnerMemberContainer"/> class.
		/// </summary>
		/// <param name="root"><see cref="ISymbol"/> that is a root of all the underlaying containers.</param>
		/// <param name="parentCompilation"><see cref="ICompilationData"/> used to create <see cref="INamespaceOrTypeData"/>s.</param>
		/// <param name="nameResolver"><see cref="ISymbolNameResolver"/> used to resolve names of symbols when <see cref="ISymbolContainer.GetNames"/> is called.</param>
		/// <param name="includeRoot">Determines whether the <paramref name="root"/> should be included in the underlaying containers.</param>
		/// <exception cref="ArgumentNullException"><paramref name="root"/> is <see langword="null"/>.</exception>
		public InnerMemberContainer(
			ISymbolOrMember<INamedTypeSymbol, ITypeData> root,
			ICompilationData? parentCompilation = default,
			ISymbolNameResolver? nameResolver = default,
			bool includeRoot = false
		) : base(root, parentCompilation, nameResolver, includeRoot)
		{
		}

		/// <summary>
		/// Returns a <see cref="IMappedSymbolContainer{TSymbol, TData, TLevel}"/> created from types contained within this container.
		/// <para><b>Note:</b> this container and the returned <see cref="IMappedSymbolContainer{TSymbol, TData, TLevel}"/> use the same set of data -
		/// resolving a level for the <see cref="IMappedSymbolContainer{TSymbol, TData, TLevel}"/> will also resolve the same level for the current container and vice versa.</para>
		/// </summary>
		public IMappedSymbolContainer<INamedTypeSymbol, ITypeData, IncludedMembers> GetTypes()
		{
			return _innerTypes ??= new(this, Root);
		}

		/// <summary>
		/// Returns a <see cref="IMappedSymbolContainer{TSymbol, TData, TLevel}"/> created from methods contained within this container.
		/// <para><b>Note:</b> this container and the returned <see cref="IMappedSymbolContainer{TSymbol, TData, TLevel}"/> use the same set of data -
		/// resolving a level for the <see cref="IMappedSymbolContainer{TSymbol, TData, TLevel}"/> will also resolve the same level for the current container and vice versa.</para>
		/// </summary>
		public IMappedSymbolContainer<IMethodSymbol, IMethodData, IncludedMembers> GetMethods()
		{
			return _methods ??= new(this, Root);
		}

		/// <summary>
		/// Returns a <see cref="IMappedSymbolContainer{TSymbol, TData, TLevel}"/> created from fields contained within this container.
		/// <para><b>Note:</b> this container and the returned <see cref="IMappedSymbolContainer{TSymbol, TData, TLevel}"/> use the same set of data -
		/// resolving a level for the <see cref="IMappedSymbolContainer{TSymbol, TData, TLevel}"/> will also resolve the same level for the current container and vice versa.</para>
		/// </summary>
		public IMappedSymbolContainer<IFieldSymbol, IFieldData, IncludedMembers> GetFields()
		{
			return _fields ??= new(this, Root);
		}

		/// <summary>
		/// Returns a <see cref="IMappedSymbolContainer{TSymbol, TData, TLevel}"/> created from properties contained within this container.
		/// <para><b>Note:</b> this container and the returned <see cref="IMappedSymbolContainer{TSymbol, TData, TLevel}"/> use the same set of data -
		/// resolving a level for the <see cref="IMappedSymbolContainer{TSymbol, TData, TLevel}"/> will also resolve the same level for the current container and vice versa.</para>
		/// </summary>
		public IMappedSymbolContainer<IPropertySymbol, IPropertyData, IncludedMembers> GetProperties()
		{
			return _properties ??= new(this, Root);
		}

		/// <summary>
		/// Returns a <see cref="IMappedSymbolContainer{TSymbol, TData, TLevel}"/> created from events contained within this container.
		/// <para><b>Note:</b> this container and the returned <see cref="IMappedSymbolContainer{TSymbol, TData, TLevel}"/> use the same set of data -
		/// resolving a level for the <see cref="IMappedSymbolContainer{TSymbol, TData, TLevel}"/> will also resolve the same level for the current container and vice versa.</para>
		/// </summary>
		public IMappedSymbolContainer<IEventSymbol, IEventData, IncludedMembers> GetEvents()
		{
			return _events ??= new(this, Root);
		}

		/// <inheritdoc/>
		protected override IEnumerable<ISymbolOrMember<ISymbol, IMemberData>> All(ISymbolOrMember<ISymbol, IMemberData> member)
		{
			return (member.Symbol as INamedTypeSymbol)!.GetMembers().Select(m => m.ToDataOrSymbol(ParentCompilation));
		}

		/// <inheritdoc/>
		protected override IEnumerable<ISymbolOrMember<ISymbol, IMemberData>> ResolveRoot(ISymbolOrMember root)
		{
			return All((root as ISymbolOrMember<ISymbol, IMemberData>)!);
		}

		/// <inheritdoc/>
		protected override bool SkipMember(ISymbolOrMember<ISymbol, IMemberData> member, IncludedMembers level)
		{
			return member is not ISymbolOrMember<INamedTypeSymbol, ITypeData>;
		}
	}
}
