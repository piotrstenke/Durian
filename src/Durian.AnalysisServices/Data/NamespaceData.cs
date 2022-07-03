// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
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
		}

		private sealed class ContainerWrapper : SymbolContainer<INamespaceOrTypeSymbol, INamespaceOrTypeData>
		{
			private SubNamespacesContainer? _namespaces;
			private InnerTypesContainer? _innerTypes;

			public NamespaceData Root { get; }

			public ContainerWrapper(
				NamespaceData root,
				ICompilationData? parentCompilation,
				ISymbolNameResolver? nameResolver
			) : base(parentCompilation, nameResolver)
			{
				Root = root;
			}

			public SubNamespacesContainer GetNamespaces()
			{
				return _namespaces ??= new(Root, false, ParentCompilation, NameResolver);
			}

			public InnerTypesContainer GetTypes()
			{
				if(_innerTypes is null)
				{
					if (Root is not ISymbolOrMember<INamespaceOrTypeSymbol, ITypeData> type)
					{
						type = new NamespaceOrTypeData()
					}

					_innerTypes = new(type, false, ParentCompilation, NameResolver);
				}

				return _innerTypes;
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
			return _members.ResolveLevel((int)members);
		}

		/// <inheritdoc/>
		public ISymbolContainer<INamespaceSymbol, INamespaceData> GetNamespaces(IncludedMembers members)
		{
			InitMembers();

			if(_members is NamespacesOrTypesContainer container)
			{
				return container.GetNamespaces().ResolveLevel(members);
			}

			return (_members as ContainerWrapper)
		}

		/// <inheritdoc/>
		public ISymbolContainer<INamedTypeSymbol, ITypeData> GetTypes(IncludedMembers members)
		{

		}

		public NamespaceOrTypeData ToNamespaceOrType()
		{

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
				_members = new NamespacesOrTypesContainer(this, false, ParentCompilation);
			}
		}
	}
}
