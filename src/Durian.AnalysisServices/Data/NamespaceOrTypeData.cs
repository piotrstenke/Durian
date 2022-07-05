// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Durian.Analysis.Extensions;
using Durian.Analysis.SymbolContainers;
using Durian.Analysis.SymbolContainers.Specialized;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.Analysis.Data
{
	/// <summary>
	/// Encapsulates data associated with a single <see cref="BaseNamespaceDeclarationSyntax"/> or <see cref="BaseTypeDeclarationSyntax"/>.
	/// </summary>
	public class NamespaceOrTypeData : MemberData, INamespaceOrTypeData
	{
		/// <summary>
		/// Contains optional data that can be passed to a <see cref="NamespaceOrTypeData"/>.
		/// </summary>
		public new class Properties : Properties<INamespaceOrTypeSymbol>
		{
			/// <summary>
			/// Inner types of the current symbol.
			/// </summary>
			public ILeveledSymbolContainer<INamedTypeSymbol, ITypeData>? Types { get; set; }

			/// <summary>
			/// Initializes a new instance of the <see cref="Properties"/> class.
			/// </summary>
			public Properties()
			{
			}
		}

		private ILeveledSymbolContainer<INamedTypeSymbol, ITypeData>? _types;

		/// <summary>
		/// Returns the <see cref="MemberData.Declaration"/> as a <see cref="BaseNamespaceDeclarationSyntax"/>.
		/// </summary>
		public BaseNamespaceDeclarationSyntax? AsNamespace => (Declaration as BaseNamespaceDeclarationSyntax)!;

		/// <summary>
		/// Returns the <see cref="MemberData.Declaration"/> as a <see cref="BaseTypeDeclarationSyntax"/>.
		/// </summary>
		public BaseTypeDeclarationSyntax? AsType => (Declaration as BaseTypeDeclarationSyntax)!;

		/// <summary>
		/// Returns the <see cref="MemberData.Declaration"/> as a <see cref="DelegateDeclarationSyntax"/>.
		/// </summary>
		public DelegateDeclarationSyntax? AsDelegate => (Declaration as DelegateDeclarationSyntax)!;

		/// <summary>
		/// <see cref="INamespaceOrTypeSymbol"/> associated with the <see cref="MemberData.Declaration"/>.
		/// </summary>
		public new INamespaceOrTypeSymbol Symbol => (base.Symbol as INamespaceOrTypeSymbol)!;

		INamespaceOrTypeData ISymbolOrMember<INamespaceOrTypeSymbol, INamespaceOrTypeData>.Member => this;

		/// <summary>
		/// Initializes a new instance of the <see cref="NamespaceOrTypeData"/> class.
		/// </summary>
		/// <param name="declaration"><see cref="TypeDeclarationSyntax"/> this <see cref="NamespaceOrTypeData"/> represents.</param>
		/// <param name="compilation">Parent <see cref="ICompilationData"/> of this <see cref="NamespaceOrTypeData"/>.</param>
		/// <param name="properties"><see cref="Properties"/> to use for the current instance.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="declaration"/> is <see langword="null"/>. -or- <paramref name="compilation"/> is <see langword="null"/>.
		/// </exception>
		public NamespaceOrTypeData(TypeDeclarationSyntax declaration, ICompilationData compilation, Properties? properties = default) : base(declaration, compilation, properties)
		{
			SetProperties(properties);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="NamespaceOrTypeData"/> class.
		/// </summary>
		/// <param name="declaration"><see cref="BaseNamespaceDeclarationSyntax"/> this <see cref="NamespaceOrTypeData"/> represents.</param>
		/// <param name="compilation">Parent <see cref="ICompilationData"/> of this <see cref="NamespaceOrTypeData"/>.</param>
		/// <param name="properties"><see cref="Properties"/> to use for the current instance.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="declaration"/> is <see langword="null"/>. -or- <paramref name="compilation"/> is <see langword="null"/>.
		/// </exception>
		public NamespaceOrTypeData(BaseNamespaceDeclarationSyntax declaration, ICompilationData compilation, Properties? properties = default) : base(declaration, compilation, properties)
		{
			SetProperties(properties);
		}

		internal NamespaceOrTypeData(INamespaceOrTypeSymbol symbol, ICompilationData compilation) : base(symbol, compilation)
		{
		}

		/// <summary>
		/// Converts the current object to a <see cref="ITypeData"/>.
		/// </summary>
		/// <exception cref="InvalidOperationException">Symbol is not a type.</exception>
		public ITypeData ToType()
		{
			if(!Symbol.IsType)
			{
				throw new InvalidOperationException("Symbol is not a type");
			}

			return (Symbol as INamedTypeSymbol)!.ToData(ParentCompilation);
		}

		/// <summary>
		/// Converts the current object to a <see cref="ITypeData"/>.
		/// </summary>
		/// <exception cref="InvalidOperationException">Symbol is not a namespace.</exception>
		public INamespaceData ToNamespace()
		{
			if(!Symbol.IsNamespace)
			{
				throw new InvalidOperationException("Symbol is not a namespace");
			}

			return (Symbol as INamespaceSymbol)!.ToData(ParentCompilation);
		}

		/// <inheritdoc/>
		public ISymbolContainer<INamedTypeSymbol, ITypeData> GetTypes(IncludedMembers members)
		{
			if(_types is null)
			{
				_types = new NamespacesOrTypesContainer(this, ParentCompilation).GetTypes();
			}

			if(_types is IncludedMembersSymbolContainer<INamedTypeSymbol, ITypeData> t)
			{
				return t.ResolveLevel(members);
			}

			return _types.ResolveLevel((int)members);
		}

		private void SetProperties(Properties? properties)
		{
			if (properties is not null)
			{
				_types = properties.Types;
			}
		}
	}
}
