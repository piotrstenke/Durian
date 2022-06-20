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
	public abstract class TypeData<TDeclaration> : MemberData, ITypeData where TDeclaration : TypeDeclarationSyntax
	{
		/// <summary>
		/// Contains optional data that can be passed to a <see cref="TypeData{TDeclaration}"/>.
		/// </summary>
		public new class Properties : Properties<ITypeSymbol>
		{
			/// <inheritdoc cref="ITypeData.PartialDeclarations"/>
			public ImmutableArray<TDeclaration> PartialDeclarations { get; set; }

			/// <inheritdoc cref="ITypeData.Members"/>
			public ISymbolContainer<ISymbol>? Members { get; set; }

			/// <inheritdoc cref="ITypeData.AllMembers"/>
			public ISymbolContainer<ISymbol>? AllMembers { get; set; }

			/// <inheritdoc cref="ITypeData.BaseTypes"/>
			public ISymbolContainer<INamedTypeSymbol>? BaseTypes { get; set; }

			/// <inheritdoc cref="ITypeData.InnerTypes"/>
			public ISymbolContainer<INamedTypeSymbol>? InnerTypes { get; set; }

			/// <inheritdoc cref="ITypeData.AllInnerTypes"/>
			public ISymbolContainer<INamedTypeSymbol>? AllInnerTypes { get; set; }

			/// <inheritdoc cref="ITypeData.ParameterlessConstructor"/>
			public DefaultedValue<ISymbolOrMember<IMethodSymbol, ConstructorData>> ParameterlessConstructor { get; set; }

			/// <inheritdoc cref="ITypeData.TypeParameters"/>
			public ISymbolContainer<ITypeParameterSymbol>? TypeParameters { get; set; }

			/// <inheritdoc cref="ITypeData.TypeArguments"/>
			public ISymbolContainer<ITypeSymbol>? TypeArguments { get; }

			/// <summary>
			/// Initializes a new instance of the <see cref="Properties"/> class.
			/// </summary>
			public Properties()
			{
			}
		}

		private ImmutableArray<TDeclaration> _partialDeclarations;
		private ISymbolContainer<ISymbol>? _members;
		private ISymbolContainer<ISymbol>? _allMembers;
		private ISymbolContainer<INamedTypeSymbol>? _baseTypes;
		private protected ISymbolContainer<INamedTypeSymbol>? _allInnerTypes;
		private protected ISymbolContainer<INamedTypeSymbol>? _innerTypes;
		private ISymbolContainer<ITypeParameterSymbol>? _typeParameters;
		private ISymbolContainer<ITypeSymbol>? _typeArguments;
		private DefaultedValue<ISymbolOrMember<IMethodSymbol, ConstructorData>> _parameterlessConstructor;

		/// <summary>
		/// Target <see cref="TypeDeclarationSyntax"/>.
		/// </summary>
		public new TDeclaration Declaration => (base.Declaration as TDeclaration)!;

		/// <summary>
		/// <see cref="ITypeSymbol"/> associated with the <see cref="Declaration"/>.
		/// </summary>
		public new ITypeSymbol Symbol => (base.Symbol as ITypeSymbol)!;

		ISymbolOrMember<IMethodSymbol, IMethodData>? ITypeData.ParameterlessConstructor => ParameterlessConstructor;

		/// <inheritdoc/>
		public ISymbolOrMember<IMethodSymbol, ConstructorData>? ParameterlessConstructor
		{
			get
			{
				if(_parameterlessConstructor.IsDefault)
				{
					_parameterlessConstructor = Symbol is INamedTypeSymbol type
						? type.GetSpecialConstructor(SpecialConstructor.Parameterless).ToDataOrSymbolInternal<ConstructorData>(ParentCompilation)
						: null;
				}

				return _parameterlessConstructor.Value;
			}
		}

		/// <inheritdoc/>
		public ISymbolContainer<ITypeParameterSymbol> TypeParameters
		{
			get
			{
				return _typeParameters ??= Symbol is INamedTypeSymbol type && type.IsGenericType
					? type.TypeParameters.ToContainer(ParentCompilation)
					: SymbolContainerFactory.Empty<ITypeParameterSymbol>();
			}
		}

		/// <inheritdoc/>
		public ISymbolContainer<ITypeSymbol> TypeArguments
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
		/// <param name="properties"><see cref="MemberData.Properties"/> to use for the current instance.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="declaration"/> is <see langword="null"/>. -or- <paramref name="compilation"/> is <see langword="null"/>
		/// </exception>
		protected TypeData(TDeclaration declaration, ICompilationData compilation, MemberData.Properties? properties = default) : base(declaration, compilation, properties)
		{
			if(properties is Properties props)
			{
				_members = props.Members;
				_partialDeclarations = props.PartialDeclarations;
				_allInnerTypes = props.AllInnerTypes;
				_innerTypes = props.InnerTypes;
				_allMembers = props.AllMembers;
				_baseTypes = props.BaseTypes;
				_parameterlessConstructor = props.ParameterlessConstructor;
				_typeParameters = props.TypeParameters;
				_typeArguments = props.TypeArguments;
			}
		}

		internal TypeData(ITypeSymbol symbol, ICompilationData compilation, bool initDefaultProperties = false) : base(symbol, compilation, initDefaultProperties)
		{
		}

		/// <inheritdoc/>
		protected override MemberData.Properties? GetDefaultProperties()
		{
			return new MemberData.Properties()
			{
				Virtuality = Virtuality.NotVirtual
			};
		}

		/// <inheritdoc/>
		public ISymbolContainer<ISymbol> Members
		{
			get
			{
				return _members ??= Symbol is INamedTypeSymbol type
					? type.GetMembers().ToContainer(ParentCompilation)
					: SymbolContainerFactory.Empty<ISymbol>();
			}
		}

		/// <inheritdoc/>
		public ISymbolContainer<INamedTypeSymbol> InnerTypes
		{
			get
			{
				return _innerTypes ??= Symbol is INamedTypeSymbol type
					? type.GetTypeMembers().ToContainer(ParentCompilation)
					: SymbolContainerFactory.Empty<INamedTypeSymbol>();
			}
		}

		/// <inheritdoc/>
		public ISymbolContainer<INamedTypeSymbol> AllInnerTypes
		{
			get
			{
				return _allInnerTypes ??= Symbol is INamedTypeSymbol type
					? type.GetInnerTypes(false).ToContainer(ParentCompilation)
					: SymbolContainerFactory.Empty<INamedTypeSymbol>();
			}
		}

		/// <inheritdoc/>
		public ISymbolContainer<INamedTypeSymbol> BaseTypes
		{
			get
			{
				return _baseTypes ??= Symbol is INamedTypeSymbol type
					? type.GetBaseTypes(false).ToContainer(ParentCompilation)
					: SymbolContainerFactory.Empty<INamedTypeSymbol>();
			}
		}

		/// <inheritdoc/>
		public ISymbolContainer<ISymbol> AllMembers
		{
			get
			{
				return _allMembers ??= Symbol is INamedTypeSymbol type
					? type.GetAllMembers().ToContainer(ParentCompilation)
					: SymbolContainerFactory.Empty<ISymbol>();
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
