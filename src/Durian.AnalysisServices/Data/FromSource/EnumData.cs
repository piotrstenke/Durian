// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Immutable;
using System.ComponentModel;
using Durian.Analysis.Extensions;
using Durian.Analysis.SymbolContainers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.Analysis.Data.FromSource
{
	/// <inheritdoc cref="IEnumData"/>
	public class EnumData : MemberData, IEnumData
	{
		/// <summary>
		/// Contains optional data that can be passed to an <see cref="EnumData"/>.
		/// </summary>
		public new class Properties : Properties<INamedTypeSymbol>
		{
			/// <inheritdoc cref="EnumData.BaseType"/>
			public ISymbolOrMember<INamedTypeSymbol, ITypeData>? BaseType { get; set; }

			/// <inheritdoc cref="EnumData.BaseTypeKeyword"/>
			public TypeKeyword? BaseTypeKeyword { get; set; }

			/// <inheritdoc cref="EnumData.Fields"/>
			public DefaultedValue<ISymbolContainer<IFieldSymbol, IFieldData>> Fields { get; set; }

			/// <inheritdoc cref="EnumData.IsFlags"/>
			public bool? IsFlags { get; set; }

			/// <summary>
			/// Initializes a new instance of the <see cref="Properties"/> class.
			/// </summary>
			public Properties()
			{
			}

			/// <summary>
			/// Initializes a new instance of the <see cref="Properties"/> class.
			/// </summary>
			/// <param name="fillWithDefault">Determines whether to fill the current properties with default data.</param>
			public Properties(bool fillWithDefault) : base(fillWithDefault)
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
				properties.IsFlags = IsFlags;
				properties.BaseType = BaseType;
				properties.BaseTypeKeyword = BaseTypeKeyword;
				properties.Fields = Fields;
			}

			/// <inheritdoc/>
#pragma warning disable CS0809 // Obsolete member overrides non-obsolete member
			[Obsolete("Use Map(Properties) instead")]
			[EditorBrowsable(EditorBrowsableState.Never)]
			public sealed override void Map(Properties<INamedTypeSymbol> properties)
#pragma warning restore CS0809 // Obsolete member overrides non-obsolete member
			{
				if (properties is Properties props)
				{
					Map(props);
				}
				else
				{
					base.Map(properties);
				}
			}

			/// <inheritdoc/>
			protected override MemberData.Properties CloneCore()
			{
				Properties properties = new();
				Map(properties);
				return properties;
			}

			/// <inheritdoc/>
			protected override void FillWithDefaultData()
			{
				Virtuality = Analysis.Virtuality.NotVirtual;
				IsPartial = false;
				IsUnsafe = false;
			}
		}

		private ISymbolOrMember<INamedTypeSymbol, ITypeData>? _baseType;
		private ISymbolContainer<INamedTypeSymbol, ITypeData>? _baseTypes;
		private ISymbolContainer<IFieldSymbol, IFieldData>? _fields;
		private bool? _isFlags;
		private TypeKeyword? _typeKeyword;

		/// <summary>
		/// <see cref="INamedTypeSymbol"/> associated with the enum's underlaying type.
		/// </summary>
		public ISymbolOrMember<INamedTypeSymbol, ITypeData> BaseType
		{
			get
			{
				return _baseType ??= Symbol.EnumUnderlyingType!.ToDataOrSymbol();
			}
		}

		/// <inheritdoc/>
		public TypeKeyword BaseTypeKeyword
		{
			get
			{
				return _typeKeyword ??= BaseType.Symbol.GetEnumUnderlayingTypeKeyword();
			}
		}

		/// <inheritdoc/>
		public new EnumDeclarationSyntax Declaration => (base.Declaration as EnumDeclarationSyntax)!;

		/// <inheritdoc/>
		public ISymbolContainer<IFieldSymbol, IFieldData> Fields
		{
			get
			{
				return _fields ??= Symbol.GetMembers().OfType<IFieldSymbol>().ToContainer(ParentCompilation);
			}
		}

		/// <inheritdoc/>
		public bool IsFlags
		{
			get
			{
				return _isFlags ??= Symbol.IsFlagsEnum();
			}
		}

		/// <summary>
		/// <see cref="INamedTypeSymbol"/> associated with the <see cref="TypeData{TDeclaration}.Declaration"/>.
		/// </summary>
		public new INamedTypeSymbol Symbol => (base.Symbol as INamedTypeSymbol)!;

		ISymbolContainer<INamedTypeSymbol, ITypeData> ITypeData.BaseTypes
		{
			get
			{
				return _baseTypes ??= SymbolContainerFactory.Single(BaseType, ParentCompilation);
			}
		}

		string? ITypeData.CompilerCondition => default;

		INamespaceOrTypeData ISymbolOrMember<INamespaceOrTypeSymbol, INamespaceOrTypeData>.Member => this;

		ITypeData ISymbolOrMember<INamedTypeSymbol, ITypeData>.Member => this;

		ISymbolOrMember<IMethodSymbol, IMethodData>? ITypeData.ParameterlessConstructor => default;

		ImmutableArray<TypeDeclarationSyntax> ITypeData.PartialDeclarations => ImmutableArray<TypeDeclarationSyntax>.Empty;

		INamespaceOrTypeSymbol INamespaceOrTypeData.Symbol => Symbol;

		INamespaceOrTypeSymbol ISymbolOrMember<INamespaceOrTypeSymbol, INamespaceOrTypeData>.Symbol => Symbol;

		ISymbolContainer<ITypeSymbol, ITypeData> IGenericMemberData.TypeArguments => SymbolContainerFactory.Empty<ITypeSymbol, ITypeData>();

		ISymbolContainer<ITypeParameterSymbol, ITypeParameterData> IGenericMemberData.TypeParameters => SymbolContainerFactory.Empty<ITypeParameterSymbol, ITypeParameterData>();

		bool ITypeData.IsAttribute => false;
		bool ITypeData.IsException => false;

		IEnumData ISymbolOrMember<INamedTypeSymbol, IEnumData>.Member => this;

		BaseTypeDeclarationSyntax ITypeData.Declaration => Declaration;

		SyntaxNode ITypeData.SafeDeclaration => Declaration;

		/// <summary>
		/// Initializes a new instance of the <see cref="EnumData"/> class.
		/// </summary>
		/// <param name="declaration"><see cref="EnumDeclarationSyntax"/> this <see cref="EnumData"/> represents.</param>
		/// <param name="compilation">Parent <see cref="ICompilationData"/> of this <see cref="EnumData"/>.</param>
		/// <param name="properties"><see cref="Properties"/> to use for the current instance.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="declaration"/> is <see langword="null"/>. -or- <paramref name="compilation"/> is <see langword="null"/>
		/// </exception>
		public EnumData(EnumDeclarationSyntax declaration, ICompilationData compilation, Properties? properties = default) : base(declaration, compilation, properties)
		{
		}

		internal EnumData(INamedTypeSymbol symbol, ICompilationData compilation, MemberData.Properties? properties = default) : base(symbol, compilation, properties)
		{
		}

		/// <inheritdoc cref="MemberData.Clone"/>
		public new EnumData Clone()
		{
			return (CloneCore() as EnumData)!;
		}

		/// <inheritdoc cref="MemberData.GetProperties"/>
		public new Properties GetProperties()
		{
			return (GetPropertiesCore() as Properties)!;
		}

		/// <inheritdoc cref="MemberData.Map(MemberData.Properties)"/>
		public virtual void Map(Properties properties)
		{
			base.Map(properties);
			properties.IsFlags = _isFlags;
			properties.BaseType = _baseType;
			properties.BaseTypeKeyword = _typeKeyword;
			properties.Fields = DataHelpers.ToDefaultedValue(_fields);
		}

		/// <inheritdoc/>
#pragma warning disable CS0809 // Obsolete member overrides non-obsolete member
		[Obsolete("Use Map(Properties) instead")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public sealed override void Map(MemberData.Properties properties)
#pragma warning restore CS0809 // Obsolete member overrides non-obsolete member
		{
			if (properties is Properties props)
			{
				Map(props);
			}
			else
			{
				base.Map(properties);
			}
		}

		/// <inheritdoc/>
		protected override MemberData CloneCore()
		{
			return new EnumData(Declaration, ParentCompilation, GetProperties());
		}

		/// <inheritdoc/>
		protected override MemberData.Properties? GetDefaultProperties()
		{
			return new Properties(true);
		}

		/// <inheritdoc/>
		protected override MemberData.Properties GetPropertiesCore()
		{
			Properties properties = new();
			Map(properties);
			return properties;
		}

		/// <inheritdoc/>
		protected override void SetProperties(MemberData.Properties properties)
		{
			base.SetProperties(properties);

			if (properties is Properties props)
			{
				_isFlags = props.IsFlags;
				_fields = DataHelpers.FromDefaultedOrEmpty(props.Fields);
				_baseType = props.BaseType;
				_typeKeyword = props.BaseTypeKeyword;
			}
		}

		ISymbolContainer<IEventSymbol, IEventData> ITypeData.GetEvents(IncludedMembers members)
		{
			return SymbolContainerFactory.Empty<IEventSymbol, IEventData>();
		}

		ISymbolContainer<IFieldSymbol, IFieldData> ITypeData.GetFields(IncludedMembers members)
		{
			if (members != IncludedMembers.Direct)
			{
				return SymbolContainerFactory.Empty<IFieldSymbol, IFieldData>();
			}

			return Fields;
		}

		ISymbolContainer<ISymbol, IMemberData> ITypeData.GetMembers(IncludedMembers members)
		{
			return Fields;
		}

		ISymbolContainer<IMethodSymbol, IMethodData> ITypeData.GetMethods(IncludedMembers members)
		{
			return SymbolContainerFactory.Empty<IMethodSymbol, IMethodData>();
		}

		ISymbolContainer<IPropertySymbol, IPropertyData> ITypeData.GetProperties(IncludedMembers members)
		{
			return SymbolContainerFactory.Empty<IPropertySymbol, IPropertyData>();
		}

		ISymbolContainer<INamedTypeSymbol, ITypeData> INamespaceOrTypeData.GetTypes(IncludedMembers members)
		{
			return SymbolContainerFactory.Empty<INamedTypeSymbol, ITypeData>();
		}

		INamespaceData INamespaceOrTypeData.ToNamespace()
		{
			throw new InvalidOperationException("Current symbol is not a namespace");
		}

		ITypeData INamespaceOrTypeData.ToType()
		{
			return this;
		}

		ITypeData ITypeData.Clone()
		{
			return Clone();
		}

		INamespaceOrTypeData INamespaceOrTypeData.Clone()
		{
			return Clone();
		}

		IEnumData IEnumData.Clone()
		{
			return Clone();
		}
	}
}
