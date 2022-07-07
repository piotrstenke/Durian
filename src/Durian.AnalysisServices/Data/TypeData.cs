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

			/// <summary>
			/// Container of child <see cref="ISymbol"/>s of this type.
			/// </summary>
			public ILeveledSymbolContainer<ISymbol, IMemberData>? Members { get; set; }

			/// <inheritdoc cref="ITypeData.BaseTypes"/>
			public ISymbolContainer<INamedTypeSymbol, ITypeData>? BaseTypes { get; set; }

			/// <summary>
			/// Container of inner types of this type.
			/// </summary>
			public ILeveledSymbolContainer<INamedTypeSymbol, ITypeData>? InnerTypes { get; set; }

			/// <inheritdoc cref="ITypeData.ParameterlessConstructor"/>
			public DefaultedValue<ISymbolOrMember<IMethodSymbol, IMethodData>> ParameterlessConstructor { get; set; }

			/// <inheritdoc cref="IGenericMemberData.TypeParameters"/>
			public ISymbolContainer<ITypeParameterSymbol, ITypeParameterData>? TypeParameters { get; set; }

			/// <inheritdoc cref="IGenericMemberData.TypeArguments"/>
			public ISymbolContainer<ITypeSymbol, ITypeData>? TypeArguments { get; }

			/// <summary>
			/// Initializes a new instance of the <see cref="Properties"/> class.
			/// </summary>
			public Properties()
			{
			}
		}

		private ImmutableArray<TDeclaration> _partialDeclarations;
		private ILeveledSymbolContainer<ISymbol, IMemberData>? _members;
		private ISymbolContainer<INamedTypeSymbol, ITypeData>? _baseTypes;
		private ILeveledSymbolContainer<INamedTypeSymbol, ITypeData>? _innerTypes;
		private ISymbolContainer<ITypeParameterSymbol, ITypeParameterData>? _typeParameters;
		private ISymbolContainer<ITypeSymbol, ITypeData>? _typeArguments;
		private DefaultedValue<ISymbolOrMember<IMethodSymbol, IMethodData>> _parameterlessConstructor;

		/// <summary>
		/// Target <see cref="TypeDeclarationSyntax"/>.
		/// </summary>
		public new TDeclaration Declaration => (base.Declaration as TDeclaration)!;

		/// <summary>
		/// <see cref="INamedTypeSymbol"/> associated with the <see cref="Declaration"/>.
		/// </summary>
		public new INamedTypeSymbol Symbol => (base.Symbol as INamedTypeSymbol)!;

		ITypeSymbol ITypeData.Symbol => Symbol;

		/// <inheritdoc/>
		public ISymbolOrMember<IMethodSymbol, IMethodData>? ParameterlessConstructor
		{
			get
			{
				if(_parameterlessConstructor.IsDefault)
				{
					_parameterlessConstructor = new(Symbol.GetSpecialConstructor(SpecialConstructor.Parameterless)?.ToDataOrSymbol(ParentCompilation));
				}

				return _parameterlessConstructor.Value;
			}
		}

		/// <inheritdoc/>
		public ISymbolContainer<ITypeParameterSymbol, ITypeParameterData> TypeParameters
		{
			get
			{
				return _typeParameters ??= Symbol.IsGenericType
					? Symbol.TypeParameters.ToContainer(ParentCompilation)
					: SymbolContainerFactory.Empty<ITypeParameterSymbol, ITypeParameterData>();
			}
		}

		/// <inheritdoc/>
		public ISymbolContainer<ITypeSymbol, ITypeData> TypeArguments
		{
			get
			{
				return _typeArguments ??= Symbol.IsGenericType
					? Symbol.TypeArguments.ToContainer(ParentCompilation)
					: SymbolContainerFactory.Empty<ITypeSymbol, ITypeData>();
			}
		}

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
		}

		internal TypeData(ITypeSymbol symbol, ICompilationData compilation, MemberData.Properties? properties = default) : base(symbol, compilation, properties)
		{
		}

		/// <inheritdoc cref="ITypeData.PartialDeclarations"/>
		public ImmutableArray<TDeclaration> PartialDeclarations
		{
			get
			{
				if (_partialDeclarations.IsDefault)
				{
					_partialDeclarations = Symbol.DeclaringSyntaxReferences
						.Select(e => e.GetSyntax())
						.OfType<TDeclaration>()
						.ToImmutableArray();
				}

				return _partialDeclarations;
			}
		}

		ImmutableArray<TypeDeclarationSyntax> ITypeData.PartialDeclarations => PartialDeclarations.CastArray<TypeDeclarationSyntax>();
	}
}
