// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Immutable;
using System.ComponentModel;
using Durian.Analysis.SymbolContainers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.Analysis.Data.FromSource
{
	/// <inheritdoc cref="IDelegateData"/>
	public class DelegateData : MemberData, IDelegateData
	{
		/// <summary>
		/// Contains optional data that can be passed to a <see cref="DelegateData"/>.
		/// </summary>
		public new class Properties : Properties<INamedTypeSymbol>
		{
			/// <inheritdoc cref="IGenericMemberData.TypeArguments"/>
			public DefaultedValue<ISymbolContainer<ITypeSymbol, ITypeData>> TypeArguments { get; set; }

			/// <inheritdoc cref="IGenericMemberData.TypeParameters"/>
			public DefaultedValue<ISymbolContainer<ITypeParameterSymbol, ITypeParameterData>> TypeParameters { get; set; }

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
				properties.TypeArguments = TypeArguments;
				properties.TypeParameters = TypeParameters;
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
				IsPartial = false;
				Virtuality = Analysis.Virtuality.NotVirtual;
			}
		}

		private ISymbolContainer<ITypeSymbol, ITypeData>? _typeArguments;
		private ISymbolContainer<ITypeParameterSymbol, ITypeParameterData>? _typeParameters;

		/// <summary>
		/// Target <see cref="DelegateDeclarationSyntax"/>.
		/// </summary>
		public new DelegateDeclarationSyntax Declaration => (base.Declaration as DelegateDeclarationSyntax)!;

		/// <summary>
		/// <see cref="INamedTypeSymbol"/> associated with the <see cref="Declaration"/>.
		/// </summary>
		public new INamedTypeSymbol Symbol => (base.Symbol as INamedTypeSymbol)!;

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

		ISymbolContainer<INamedTypeSymbol, ITypeData> ITypeData.BaseTypes => SymbolContainerFactory.Empty<INamedTypeSymbol, ITypeData>();

		string? ITypeData.CompilerCondition => null;

		bool ITypeData.IsAttribute => false;

		bool ITypeData.IsException => false;

		INamespaceOrTypeData ISymbolOrMember<INamespaceOrTypeSymbol, INamespaceOrTypeData>.Member => this;

		ITypeData ISymbolOrMember<INamedTypeSymbol, ITypeData>.Member => this;

		ISymbolOrMember<IMethodSymbol, IMethodData>? ITypeData.ParameterlessConstructor => null;

		ImmutableArray<TypeDeclarationSyntax> ITypeData.PartialDeclarations => ImmutableArray<TypeDeclarationSyntax>.Empty;

		INamespaceOrTypeSymbol INamespaceOrTypeData.Symbol => Symbol;

		INamespaceOrTypeSymbol ISymbolOrMember<INamespaceOrTypeSymbol, INamespaceOrTypeData>.Symbol => Symbol;

		IDelegateData ISymbolOrMember<INamedTypeSymbol, IDelegateData>.Member => this;

		BaseTypeDeclarationSyntax ITypeData.Declaration => throw new InvalidOperationException("A delegate cannot be represented by a BaseTypeDeclarationSyntax");

		CSharpSyntaxNode ITypeData.SafeDeclaration => Declaration;

		/// <summary>
		/// Initializes a new instance of the <see cref="DelegateData"/> class.
		/// </summary>
		/// <param name="declaration"><see cref="DelegateDeclarationSyntax"/> this <see cref="DelegateData"/> represents.</param>
		/// <param name="compilation">Parent <see cref="ICompilationData"/> of this <see cref="DelegateData"/>.</param>
		/// <param name="properties"><see cref="Properties"/> to use for the current instance.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="declaration"/> is <see langword="null"/>. -or- <paramref name="compilation"/> is <see langword="null"/>
		/// </exception>
		public DelegateData(DelegateDeclarationSyntax declaration, ICompilationData compilation, Properties? properties = default) : base(declaration, compilation, properties)
		{
		}

		internal DelegateData(INamedTypeSymbol symbol, ICompilationData compilation, MemberData.Properties? properties = default) : base(symbol, compilation, properties)
		{
		}

		/// <inheritdoc cref="MemberData.Clone"/>
		public new DelegateData Clone()
		{
			return (CloneCore() as DelegateData)!;
		}

		/// <inheritdoc cref="MemberData.GetProperties"/>
		public new Properties GetProperties()
		{
			return (GetPropertiesCore() as Properties)!;
		}

		/// <inheritdoc cref="MemberData.Properties.Map(MemberData.Properties)"/>
		public virtual void Map(Properties properties)
		{
			base.Map(properties);
			properties.TypeParameters = DataHelpers.ToDefaultedValue(_typeParameters);
			properties.TypeArguments = DataHelpers.ToDefaultedValue(_typeArguments);
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
			return new DelegateData(Declaration, ParentCompilation, GetProperties());
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
				_typeArguments = DataHelpers.FromDefaultedOrEmpty(props.TypeArguments);
				_typeParameters = DataHelpers.FromDefaultedOrEmpty(props.TypeParameters);
			}
		}

		ISymbolContainer<IEventSymbol, IEventData> ITypeData.GetEvents(IncludedMembers members)
		{
			return SymbolContainerFactory.Empty<IEventSymbol, IEventData>();
		}

		ISymbolContainer<IFieldSymbol, IFieldData> ITypeData.GetFields(IncludedMembers members)
		{
			return SymbolContainerFactory.Empty<IFieldSymbol, IFieldData>();
		}

		ISymbolContainer<ISymbol, IMemberData> ITypeData.GetMembers(IncludedMembers members)
		{
			return SymbolContainerFactory.Empty<ISymbol, IMemberData>();
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

		IDelegateData IDelegateData.Clone()
		{
			return Clone();
		}

		ITypeData ITypeData.Clone()
		{
			return Clone();
		}

		INamespaceOrTypeData INamespaceOrTypeData.Clone()
		{
			return Clone();
		}
	}
}
