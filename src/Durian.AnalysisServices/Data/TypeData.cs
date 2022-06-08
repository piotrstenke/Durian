// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Durian.Analysis.Extensions;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Durian.Analysis.SymbolContainers;

namespace Durian.Analysis.Data
{
	/// <inheritdoc cref="ITypeData"/>
	/// <typeparam name="TDeclaration">Specific type of the target <see cref="TypeDeclarationSyntax"/>.</typeparam>
	public abstract class TypeData<TDeclaration> : MemberData, ITypeData where TDeclaration : BaseTypeDeclarationSyntax
	{
		/// <summary>
		/// Contains optional data that can be passed to a <see cref="TypeData{TDeclaration}"/>.
		/// </summary>
		public new class Properties : Properties<ITypeSymbol>
		{
			/// <inheritdoc cref="ITypeData.PartialDeclarations"/>
			public ImmutableArray<TDeclaration> PartialDeclarations { get; set; }

			/// <inheritdoc cref="ITypeData.Members"/>
			public SymbolContainer<ISymbol>? Members { get; set; }

			/// <inheritdoc cref="ITypeData.AllMembers"/>
			public SymbolContainer<ISymbol>? AllMembers { get; set; }

			/// <inheritdoc cref="ITypeData.BaseTypes"/>
			public SymbolContainer<INamedTypeSymbol>? BaseTypes { get; set; }

			/// <inheritdoc cref="ITypeData.InnerTypes"/>
			public SymbolContainer<INamedTypeSymbol>? InnerTypes { get; set; }

			/// <inheritdoc cref="ITypeData.AllInnerTypes"/>
			public SymbolContainer<INamedTypeSymbol>? AllInnerTypes { get; set; }

			/// <inheritdoc cref="ITypeData.ParameterlessConstructor"/>
			public DefaultedValue<ISymbolOrMember<IMethodSymbol, IMethodData>> ParameterlessConstructor { get; set; }

			/// <inheritdoc cref="ITypeData.TypeParameters"/>
			public SymbolContainer<ITypeParameterSymbol>? TypeParameters { get; set; }

			/// <inheritdoc cref="ITypeData.TypeArguments"/>
			public SymbolContainer<ITypeSymbol>? TypeArguments { get; }

			/// <summary>
			/// Initializes a new instance of the <see cref="Properties"/> class.
			/// </summary>
			public Properties()
			{
			}
		}

		private ImmutableArray<TDeclaration> _partialDeclarations;
		private SymbolContainer<ISymbol>? _members;
		private SymbolContainer<ISymbol>? _allMembers;
		private SymbolContainer<INamedTypeSymbol>? _baseTypes;
		private SymbolContainer<INamedTypeSymbol>? _allInnerTypes;
		private SymbolContainer<INamedTypeSymbol>? _innerTypes;
		private SymbolContainer<ITypeParameterSymbol>? _typeParameters;
		private SymbolContainer<ITypeSymbol>? _typeArguments;
		private DefaultedValue<ISymbolOrMember<IMethodSymbol, IMethodData>> _parameterlessConstructor;

		/// <summary>
		/// Target <see cref="BaseTypeDeclarationSyntax"/>.
		/// </summary>
		public new TDeclaration Declaration => (base.Declaration as TDeclaration)!;

		/// <summary>
		/// <see cref="ITypeSymbol"/> associated with the <see cref="Declaration"/>.
		/// </summary>
		public new ITypeSymbol Symbol => (base.Symbol as ITypeSymbol)!;

		/// <inheritdoc/>
		public ISymbolOrMember<IMethodSymbol>? ParameterlessConstructor
		{
			get
			{
				if(_parameterlessConstructor.IsDefault)
				{
					_parameterlessConstructor = Symbol is INamedTypeSymbol type
						? type.GetSpecialConstructor(SpecialConstructor.Parameterless).ToDataOrSymbolInternal(ParentCompilation)
						: null;
				}

				return _parameterlessConstructor.Value;
			}
		}

		/// <inheritdoc/>
		public SymbolContainer<ITypeParameterSymbol> TypeParameters
		{
			get
			{
				return _typeParameters ??= Symbol is INamedTypeSymbol type && type.IsGenericType
					? type.TypeParameters.ToContainer(ParentCompilation)
					: SymbolContainerFactory.Empty<ITypeParameterSymbol>();
			}
		}

		/// <inheritdoc/>
		public SymbolContainer<ITypeSymbol> TypeArguments
		{
			get
			{
				return _typeArguments ??= Symbol is INamedTypeSymbol type && type.IsGenericType
					? type.TypeArguments.ToContainer(ParentCompilation)
					: SymbolContainerFactory.Empty<ITypeSymbol>();
			}
		}

		BaseTypeDeclarationSyntax ITypeData.Declaration => Declaration;

		/// <summary>
		/// Initializes a new instance of the <see cref="TypeData{TDeclaration}"/> class.
		/// </summary>
		/// <param name="declaration"><see cref="BaseTypeDeclarationSyntax"/> this <see cref="TypeData{TDeclaration}"/> represents.</param>
		/// <param name="compilation">Parent <see cref="ICompilationData"/> of this <see cref="TypeData{TDeclaration}"/>.</param>
		/// <param name="properties"><see cref="Properties"/> to use for the current instance.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="declaration"/> is <see langword="null"/>. -or- <paramref name="compilation"/> is <see langword="null"/>
		/// </exception>
		protected TypeData(TDeclaration declaration, ICompilationData compilation, Properties? properties = default) : base(declaration, compilation, properties)
		{
			if(properties is not null)
			{
				_members = properties.Members;
				_partialDeclarations = properties.PartialDeclarations;
				_allInnerTypes = properties.AllInnerTypes;
				_innerTypes = properties.InnerTypes;
				_allMembers = properties.AllMembers;
				_baseTypes = properties.BaseTypes;
				_parameterlessConstructor = properties.ParameterlessConstructor;
				_typeParameters = properties.TypeParameters;
				_typeArguments = properties.TypeArguments;
			}
		}

		internal TypeData(ITypeSymbol symbol, ICompilationData compilation) : base(symbol, compilation)
		{
		}

		/// <inheritdoc/>
		public SymbolContainer<ISymbol> Members
		{
			get
			{
				if(_members.IsDefault)
				{
					if(Symbol is INamedTypeSymbol)
					{
						_members = ImmutableArray.CreateRange<ISymbolOrMember>(Symbol
							.GetMembers()
							.Select(m => new SymbolOrMemberWrapper<ISymbol, IMemberData>(m, ParentCompilation)));
					}
					else
					{
						_members = ImmutableArray<ISymbolOrMember>.Empty;
					}
				}

				return _members;
			}
		}

		/// <inheritdoc/>
		public SymbolContainer<INamedTypeSymbol> InnerTypes
		{
			get
			{
				if (_innerTypes.IsDefault)
				{
					if(Symbol is INamedTypeSymbol type)
					{
						_innerTypes = ImmutableArray.CreateRange<ISymbolOrMember<INamedTypeSymbol, ITypeData>>(type
							.GetTypeMembers()
							.Select(m => new SymbolOrMemberWrapper<INamedTypeSymbol, ITypeData>(m, ParentCompilation)));
					}
					else
					{
						_innerTypes = ImmutableArray<ISymbolOrMember<INamedTypeSymbol, ITypeData>>.Empty;
					}
				}

				return _innerTypes;
			}
		}

		/// <inheritdoc/>
		public SymbolContainer<INamedTypeSymbol> AllInnerTypes
		{
			get
			{
				if (_allInnerTypes.IsDefault)
				{
					if (Symbol is INamedTypeSymbol type)
					{
						_allInnerTypes = ImmutableArray.CreateRange<ISymbolOrMember<INamedTypeSymbol, ITypeData>>(type
							.GetInnerTypes(false)
							.Select(m => new SymbolOrMemberWrapper<INamedTypeSymbol, ITypeData>(m, ParentCompilation)));
					}
					else
					{
						_allInnerTypes = ImmutableArray<ISymbolOrMember<INamedTypeSymbol, ITypeData>>.Empty;
					}
				}

				return _allInnerTypes;
			}
		}

		/// <inheritdoc/>
		public SymbolContainer<INamedTypeSymbol> BaseTypes
		{
			get
			{
				if (_baseTypes.IsDefault)
				{
					if (Symbol is INamedTypeSymbol type)
					{
						_baseTypes = ImmutableArray.CreateRange<ISymbolOrMember<INamedTypeSymbol, ITypeData>>(type
							.GetBaseTypes(false)
							.Select(m => new SymbolOrMemberWrapper<INamedTypeSymbol, ITypeData>(m, ParentCompilation)));
					}
					else
					{
						_baseTypes = ImmutableArray<ISymbolOrMember<INamedTypeSymbol, ITypeData>>.Empty;
					}
				}

				return _baseTypes;
			}
		}

		/// <inheritdoc/>
		public SymbolContainer<ISymbol> AllMembers
		{
			get
			{
				if(_allMembers.IsDefault)
				{
					if(Symbol is INamedTypeSymbol type)
					{
						_allMembers = ImmutableArray.CreateRange<ISymbolOrMember>(type
							.GetAllMembers()
							.Select(m => new SymbolOrMemberWrapper<ISymbol, IMemberData>(m, ParentCompilation)));
					}
					else
					{
						_allMembers = ImmutableArray<ISymbolOrMember>.Empty;
					}
				}

				return _allMembers;
			}
		}

		/// <inheritdoc cref="ITypeData.PartialDeclarations"/>
		public ImmutableArray<TDeclaration> PartialDeclarations
		{
			get
			{
				if (_partialDeclarations.IsDefault)
				{
					if (Symbol.TypeKind == TypeKind.Enum)
					{
						_partialDeclarations = ImmutableArray<TDeclaration>.Empty;
					}
					else
					{
						_partialDeclarations = Symbol.DeclaringSyntaxReferences
							.Select(e => e.GetSyntax())
							.OfType<TDeclaration>()
							.ToImmutableArray();
					}
				}

				return _partialDeclarations;
			}
		}

		ImmutableArray<BaseTypeDeclarationSyntax> ITypeData.PartialDeclarations => PartialDeclarations.CastArray<BaseTypeDeclarationSyntax>();
	}
}
