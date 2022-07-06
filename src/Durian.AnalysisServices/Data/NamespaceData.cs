// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Durian.Analysis.CodeGeneration;
using Durian.Analysis.Extensions;
using Durian.Analysis.SymbolContainers;
using Durian.Analysis.SymbolContainers.Specialized;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.Analysis.Data
{
	/// <summary>
	/// Encapsulates data associated with a single <see cref="BaseNamespaceDeclarationSyntax"/>.
	/// </summary>
	public class NamespaceData : MemberData, INamespaceData
	{
		/// <summary>
		/// Contains optional data that can be passed to a <see cref="NamespaceData"/>.
		/// </summary>
		public new class Properties : Properties<INamespaceSymbol>
		{
			/// <summary>
			/// Container of child <see cref="INamespaceOrTypeSymbol"/>s of this namespace.
			/// </summary>
			public ILeveledSymbolContainer<INamespaceOrTypeSymbol, INamespaceOrTypeData>? Members { get; set; }

			/// <summary>
			/// Initializes a new instance of the <see cref="Properties"/> class.
			/// </summary>
			public Properties()
			{
			}

			/// <inheritdoc cref="MemberData.Properties.Clone"/>
			public new Properties Clone()
			{
				return (CloneCore() as Properties)!;
			}

			/// <inheritdoc cref="MemberData.Properties.Map(MemberData.Properties)"/>
			public virtual void Map(Properties properties)
			{
				base.Map(properties);
				properties.Members = Members;
			}

			/// <inheritdoc/>
			[Obsolete("Use Map(Properties) instead")]
			[EditorBrowsable(EditorBrowsableState.Never)]
#pragma warning disable CS0809 // Obsolete member overrides non-obsolete member
			public override void Map(Properties<INamespaceSymbol> properties)
#pragma warning restore CS0809 // Obsolete member overrides non-obsolete member
			{
				base.Map(properties);
			}

			/// <inheritdoc/>
			protected override MemberData.Properties CloneCore()
			{
				Properties properties = new();
				Map(properties);
				return properties;
			}
		}

		private ILeveledSymbolContainer<INamespaceOrTypeSymbol, INamespaceOrTypeData>? _members;

		/// <summary>
		/// Target <see cref="BaseNamespaceDeclarationSyntax"/>.
		/// </summary>
		public new BaseNamespaceDeclarationSyntax Declaration => (base.Declaration as BaseNamespaceDeclarationSyntax)!;

		/// <summary>
		/// Style of this namespace declaration.
		/// </summary>
		public NamespaceStyle DeclarationStyle { get; }

		/// <summary>
		/// <see cref="IPropertySymbol"/> associated with the <see cref="Declaration"/>.
		/// </summary>
		public new INamespaceSymbol Symbol => (base.Symbol as INamespaceSymbol)!;

		INamespaceData ISymbolOrMember<INamespaceSymbol, INamespaceData>.Member => this;

		INamespaceOrTypeSymbol INamespaceOrTypeData.Symbol => Symbol;

		INamespaceOrTypeSymbol ISymbolOrMember<INamespaceOrTypeSymbol, INamespaceOrTypeData>.Symbol => Symbol;

		INamespaceOrTypeData ISymbolOrMember<INamespaceOrTypeSymbol, INamespaceOrTypeData>.Member => this;

		/// <summary>
		/// Initializes a new instance of the <see cref="PropertyData"/> class.
		/// </summary>
		/// <param name="declaration"><see cref="BaseNamespaceDeclarationSyntax"/> this <see cref="PropertyData"/> represents.</param>
		/// <param name="compilation">Parent <see cref="ICompilationData"/> of this <see cref="PropertyData"/>.</param>
		/// <param name="properties"><see cref="MemberData.Properties"/> to use for the current instance.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="declaration"/> is <see langword="null"/>. -or- <paramref name="compilation"/> is <see langword="null"/>.
		/// </exception>
		public NamespaceData(BaseNamespaceDeclarationSyntax declaration, ICompilationData compilation, Properties? properties = default) : base(declaration, compilation, properties)
		{
			DeclarationStyle = declaration.GetNamespaceStyle();

			if(properties is not null)
			{
				_members = properties.Members;
			}
		}

		internal NamespaceData(ISymbol symbol, ICompilationData compilation) : base(symbol, compilation)
		{
		}

		/// <inheritdoc/>
		public ISymbolContainer<INamespaceOrTypeSymbol, INamespaceOrTypeData> GetMembers(IncludedMembers members)
		{
			InitMembers();

			if(_members is IMappedSymbolContainer<INamespaceOrTypeSymbol, INamespaceOrTypeData, IncludedMembers> container)
			{
				return container.ResolveLevel(members);
			}

			return _members.ResolveLevel((int)members);
		}

		/// <inheritdoc/>
		public ISymbolContainer<INamespaceSymbol, INamespaceData> GetNamespaces(IncludedMembers members)
		{
			InitMembers();

			switch (_members)
			{
				case NamespacesOrTypesContainer typed:
					IMappedSymbolContainer<INamespaceSymbol, INamespaceData, IncludedMembers> namespaces = typed.GetNamespaces();
					return namespaces.ResolveLevel(members);

				case IMappedSymbolContainer<INamespaceSymbol, INamespaceData, IncludedMembers> mapped:
					return mapped.ResolveLevel(members);

				case ILeveledSymbolContainer<INamespaceSymbol, INamespaceData> leveled:
					return leveled.ResolveLevel((int)members);

				case IMappedSymbolContainer<INamespaceOrTypeSymbol, INamespaceOrTypeData, IncludedMembers> mappedUnknown:
					return mappedUnknown.ResolveLevel(members).OfType<ISymbolOrMember<INamespaceSymbol, INamespaceData>>().ToContainer(ParentCompilation);

				default:
					return _members.ResolveLevel((int)members).OfType<ISymbolOrMember<INamespaceSymbol, INamespaceData>>().ToContainer(ParentCompilation);
			}
		}

		/// <inheritdoc/>
		public ISymbolContainer<INamedTypeSymbol, ITypeData> GetTypes(IncludedMembers members)
		{
			InitMembers();

			switch (_members)
			{
				case NamespacesOrTypesContainer typed:
					IMappedSymbolContainer<INamedTypeSymbol, ITypeData, IncludedMembers> namespaces = typed.GetTypes();
					return namespaces.ResolveLevel(members);

				case IMappedSymbolContainer<INamedTypeSymbol, ITypeData, IncludedMembers> mapped:
					return mapped.ResolveLevel(members);

				case ILeveledSymbolContainer<INamedTypeSymbol, ITypeData> leveled:
					return leveled.ResolveLevel((int)members);

				case IMappedSymbolContainer<INamespaceOrTypeSymbol, INamespaceOrTypeData, IncludedMembers> mappedUnknown:
					return mappedUnknown.ResolveLevel(members).OfType<ISymbolOrMember<INamedTypeSymbol, ITypeData>>().ToContainer(ParentCompilation);

				default:
					return _members.ResolveLevel((int)members).OfType<ISymbolOrMember<INamedTypeSymbol, ITypeData>>().ToContainer(ParentCompilation);
			}
		}

		public NamespaceOrTypeData ToNamespaceOrType()
		{
			return new NamespaceOrTypeData(Declaration, ParentCompilation, new()
			{
				Attributes = Attributes,
				ContainingNamespaces = ContainingNamespaces,
				ContainingTypes = ContainingTypes,
				GenericName = GenericName,
				HiddenMember = HiddenSymbol,
			})
		}

		ITypeData INamespaceOrTypeData.ToType()
		{
			throw new InvalidOperationException("Current symbol is not a type");
		}

		INamespaceData INamespaceOrTypeData.ToNamespace()
		{
			return this;
		}

		[MemberNotNull(nameof(_members))]
		private void InitMembers()
		{
			if (_members is null)
			{
				_members = new NamespacesOrTypesContainer(this, ParentCompilation);
			}
		}
	}
}
