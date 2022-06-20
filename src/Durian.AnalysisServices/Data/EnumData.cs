// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Durian.Analysis.Extensions;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Durian.Analysis.SymbolContainers;
using System.ComponentModel;

namespace Durian.Analysis.Data
{
	/// <summary>
	/// Encapsulates data associated with a single <see cref="EnumDeclarationSyntax"/>.
	/// </summary>
	public class EnumData : MemberData, ITypeData
	{
		/// <summary>
		/// Contains optional data that can be passed to an <see cref="EnumData"/>.
		/// </summary>
		public new class Properties : Properties<INamedTypeSymbol>
		{
			/// <inheritdoc cref="EnumData.IsFlags"/>
			public bool? IsFlags { get; set; }

			/// <inheritdoc cref="EnumData.BaseType"/>
			public INamedTypeSymbol? BaseType { get; set; }

			/// <inheritdoc cref="EnumData.Fields"/>
			public ISymbolContainer<IFieldSymbol>? Fields { get; set; }

			/// <summary>
			/// Initializes a new instance of the <see cref="Properties"/> class.
			/// </summary>
			public Properties()
			{
			}
		}

		private bool? _isFlags;
		private TypeKeyword? _typeKeyword;
		private INamedTypeSymbol? _baseType;
		private ISymbolContainer<IFieldSymbol>? _fields;

		/// <summary>
		/// Target <see cref="EnumDeclarationSyntax"/>.
		/// </summary>
		public new EnumDeclarationSyntax Declaration => (base.Declaration as EnumDeclarationSyntax)!;

		/// <summary>
		/// <see cref="INamedTypeSymbol"/> associated with the <see cref="TypeData{TDeclaration}.Declaration"/>.
		/// </summary>
		public new INamedTypeSymbol Symbol => (base.Symbol as INamedTypeSymbol)!;

		/// <summary>
		/// All fields of the
		/// </summary>
		public ISymbolContainer<IFieldSymbol> Fields
		{
			get
			{
				return _fields ??= Symbol.GetMembers().OfType<IFieldSymbol>().ToContainer(ParentCompilation);
			}
		}

		/// <summary>
		/// Determines whether the enum has the <see cref="FlagsAttribute"/> applied.
		/// </summary>
		public bool IsFlags
		{
			get
			{
				return _isFlags ??= Symbol.IsFlagsEnum();
			}
		}

		/// <summary>
		/// <see cref="INamedTypeSymbol"/> associated with the enum's underlaying type.
		/// </summary>
		public INamedTypeSymbol BaseType
		{
			get
			{
				return _baseType ??= Symbol.EnumUnderlyingType!;
			}
		}

		/// <summary>
		/// <see cref="TypeKeyword"/> associated with the enum's underlaying type.
		/// </summary>
		public TypeKeyword BaseTypeKeyword
		{
			get
			{
				return _typeKeyword ??= BaseType.GetEnumUnderlayingTypeKeyword();
			}
		}

		BaseTypeDeclarationSyntax ITypeData.Declaration => throw new NotImplementedException();

		ITypeSymbol ITypeData.Symbol => Symbol;

		ImmutableArray<BaseTypeDeclarationSyntax> ITypeData.PartialDeclarations => ImmutableArray<BaseTypeDeclarationSyntax>.Empty;

		ISymbolContainer<ISymbol> ITypeData.AllMembers => throw new NotImplementedException();

		ISymbolContainer<ISymbol> ITypeData.Members => throw new NotImplementedException();

		ISymbolContainer<INamedTypeSymbol> ITypeData.BaseTypes => throw new NotImplementedException();

		ISymbolContainer<INamedTypeSymbol> ITypeData.AllInnerTypes => throw new NotImplementedException();

		ISymbolContainer<INamedTypeSymbol> ITypeData.InnerTypes => throw new NotImplementedException();

		ISymbolOrMember<IMethodSymbol, IMethodData>? ITypeData.ParameterlessConstructor => throw new NotImplementedException();

		ISymbolContainer<ITypeParameterSymbol> ITypeData.TypeParameters => throw new NotImplementedException();

		ISymbolContainer<ITypeSymbol> ITypeData.TypeArguments => throw new NotImplementedException();

		/// <summary>
		/// Initializes a new instance of the <see cref="EnumData"/> class.
		/// </summary>
		/// <param name="declaration"><see cref="EnumDeclarationSyntax"/> this <see cref="EnumData"/> represents.</param>
		/// <param name="compilation">Parent <see cref="ICompilationData"/> of this <see cref="EnumData"/>.</param>
		/// <param name="properties"><see cref="MemberData.Properties"/> to use for the current instance.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="declaration"/> is <see langword="null"/>. -or- <paramref name="compilation"/> is <see langword="null"/>
		/// </exception>
		public EnumData(EnumDeclarationSyntax declaration, ICompilationData compilation, MemberData.Properties? properties = default) : base(declaration, compilation, properties)
		{
			if(properties is Properties props)
			{
				_isFlags = props.IsFlags;
				_fields = props.Fields;
				_baseType = props.BaseType;
			}
		}

		internal EnumData(INamedTypeSymbol symbol, ICompilationData compilation) : base(symbol, compilation, true)
		{
		}

		/// <inheritdoc/>
		protected override MemberData.Properties? GetDefaultProperties()
		{
			return base.GetDefaultProperties();
		}
	}
}
