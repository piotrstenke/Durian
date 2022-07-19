// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using Durian.Analysis.Extensions;
using Durian.Analysis.SymbolContainers;
using Microsoft.CodeAnalysis;

namespace Durian.Analysis.Data
{
	/// <inheritdoc cref="IMemberData"/>
	[DebuggerDisplay("{Symbol}")]
	public class MemberData : IMemberData, ICloneable
	{
		/// <summary>
		/// Contains optional data that can be passed to a <see cref="MemberData"/>.
		/// </summary>
		public class Properties<TSymbol> : Properties where TSymbol : class, ISymbol
		{
			/// <inheritdoc cref="Properties.Symbol"/>
			public new TSymbol? Symbol
			{
				get => (_symbol as TSymbol)!;
				set => _symbol = value;
			}

			/// <summary>
			/// Initializes a new instance of the <see cref="Properties{TSymbol}"/> class.
			/// </summary>
			public Properties()
			{
			}

			/// <summary>
			/// Initializes a new instance of the <see cref="Properties{TSymbol}"/> class.
			/// </summary>
			/// <param name="fillWithDefault">Determines whether to fill the current properties with default data.</param>
			public Properties(bool fillWithDefault) : base(fillWithDefault)
			{
			}

			/// <inheritdoc cref="Properties.Clone"/>
			public new Properties<TSymbol> Clone()
			{
				return (CloneCore() as Properties<TSymbol>)!;
			}

			/// <inheritdoc cref="Properties.Map(Properties)"/>
			public virtual void Map(Properties<TSymbol> properties)
			{
				base.Map(properties);
			}

			/// <inheritdoc/>
#pragma warning disable CS0809 // Obsolete member overrides non-obsolete member
			[Obsolete("Use Map(Properties<TSymbol> instead")]
			[EditorBrowsable(EditorBrowsableState.Never)]
			public sealed override void Map(Properties properties)
#pragma warning restore CS0809 // Obsolete member overrides non-obsolete member
			{
				if (properties is Properties<TSymbol> child)
				{
					Map(child);
				}
				else
				{
					base.Map(properties);
				}
			}

			/// <inheritdoc/>
			protected override Properties CloneCore()
			{
				Properties<TSymbol> properties = new();
				Map(properties);
				return properties;
			}

			private protected override void ValidateSymbol(ISymbol? symbol)
			{
				if (symbol is null)
				{
					return;
				}

				if (symbol is not TSymbol)
				{
					throw new ArgumentException($"Symbol '{symbol}' is not of type '{typeof(TSymbol)}", nameof(symbol));
				}
			}
		}

		/// <summary>
		/// Contains optional data that can be passed to a <see cref="MemberData"/>.
		/// </summary>
		[DebuggerDisplay("{Symbol ?? string.Empty}")]
		public class Properties : ICloneable
		{
			private protected ISymbol? _symbol;

			/// <summary>
			/// All attributes if the current symbol.
			/// </summary>
			public ImmutableArray<AttributeData> Attributes { get; set; }

			/// <inheritdoc cref="IMemberData.ContainingMember"/>
			public DefaultedValue<ISymbolOrMember<ISymbol, IMemberData>> ContainingMember { get; set; }

			/// <inheritdoc cref="IMemberData.ContainingNamespace"/>
			public DefaultedValue<ISymbolOrMember<INamespaceSymbol, INamespaceData>> ContainingNamespace { get; set; }

			/// <summary>
			/// All containing namespaces of the current symbol.
			/// </summary>
			public DefaultedValue<IWritableSymbolContainer<INamespaceSymbol, INamespaceData>> ContainingNamespaces { get; set; }

			/// <inheritdoc cref="IMemberData.ContainingType"/>
			public DefaultedValue<ISymbolOrMember<INamedTypeSymbol, ITypeData>> ContainingType { get; set; }

			/// <summary>
			/// All containing types of the current symbol.
			/// </summary>
			public DefaultedValue<IWritableSymbolContainer<INamedTypeSymbol, ITypeData>> ContainingTypes { get; set; }

			/// <inheritdoc cref="IMemberData.GenericName"/>
			public string? GenericName { get; set; }

			/// <inheritdoc cref="IMemberData.HiddenSymbol"/>
			public DefaultedValue<ISymbolOrMember<ISymbol, IMemberData>> HiddenSymbol { get; set; }

			/// <inheritdoc cref="IMemberData.IsNew"/>
			public bool? IsNew { get; set; }

			/// <inheritdoc cref="IMemberData.IsObsolete"/>
			public bool? IsObsolete { get; set; }

			/// <inheritdoc cref="IMemberData.IsPartial"/>
			public bool? IsPartial { get; set; }

			/// <inheritdoc cref="IMemberData.IsUnsafe"/>
			public bool? IsUnsafe { get; set; }

			/// <inheritdoc cref="IMemberData.Location"/>
			public Location? Location { get; set; }

			/// <summary>
			/// All modifiers of the current symbol.
			/// </summary>
			public ImmutableArray<string> Modifiers { get; set; }

			/// <inheritdoc cref="IMemberData.Name"/>
			public string? Name { get; set; }

			/// <inheritdoc cref="IMemberData.SemanticModel"/>
			public SemanticModel? SemanticModel { get; set; }

			/// <inheritdoc cref="IMemberData.SubstitutedName"/>
			public string? SubstitutedName { get; set; }

			/// <inheritdoc cref="IMemberData.Symbol"/>
			public ISymbol? Symbol
			{
				get => _symbol;
				set
				{
					ValidateSymbol(value);
					_symbol = value;
				}
			}

			/// <inheritdoc cref="IMemberData.Virtuality"/>
			public Virtuality? Virtuality { get; set; }

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
			public Properties(bool fillWithDefault)
			{
				if(fillWithDefault)
				{
					FillWithDefaultData();
				}
			}

			/// <summary>
			/// Creates a shallow copy the current object.
			/// </summary>
			public Properties Clone()
			{
				return CloneCore();
			}

			/// <summary>
			/// Maps the data of the current object to the specified <paramref name="properties"/>.
			/// </summary>
			/// <param name="properties"><see cref="Properties"/> to map to.</param>
			public virtual void Map(Properties properties)
			{
				properties.Attributes = Attributes;
				properties.ContainingMember = ContainingMember;
				properties.ContainingNamespace = ContainingNamespace;
				properties.ContainingNamespaces = ContainingNamespaces;
				properties.ContainingType = ContainingType;
				properties.ContainingTypes = ContainingTypes;
				properties.GenericName = GenericName;
				properties.HiddenSymbol = HiddenSymbol;
				properties.IsNew = IsNew;
				properties.IsObsolete = IsObsolete;
				properties.IsPartial = IsPartial;
				properties.IsUnsafe = IsUnsafe;
				properties.Location = Location;
				properties.Modifiers = Modifiers;
				properties.Name = Name;
				properties.SemanticModel = SemanticModel;
				properties.SubstitutedName = SubstitutedName;
				properties.Symbol = Symbol;
				properties.Virtuality = Virtuality;
			}

			/// <summary>
			/// Actually creates a shallow copy the current object.
			/// </summary>
			protected virtual Properties CloneCore()
			{
				Properties properties = new();
				Map(properties);
				return properties;
			}

			/// <summary>
			/// Fills the current properties will default data.
			/// </summary>
			protected virtual void FillWithDefaultData()
			{
				// Do nothing by default.
			}

			object ICloneable.Clone()
			{
				return Clone();
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			private protected void SetDefaultData()
			{
				IsPartial = false;
				IsObsolete = false;
				IsNew = false;
				IsUnsafe = false;
				Virtuality = Analysis.Virtuality.NotVirtual;
				HiddenSymbol = null;
			}

			private protected virtual void ValidateSymbol(ISymbol? symbol)
			{
				// Do nothing by default.
			}
		}

		private ImmutableArray<AttributeData> _attributes;
		private DefaultedValue<ISymbolOrMember<INamespaceSymbol, INamespaceData>> _containingNamespace;
		private IWritableSymbolContainer<INamespaceSymbol, INamespaceData>? _containingNamespaces;
		private DefaultedValue<ISymbolOrMember<INamedTypeSymbol, ITypeData>> _containingType;
		private IWritableSymbolContainer<INamedTypeSymbol, ITypeData>? _containingTypes;
		private DefaultedValue<ISymbolOrMember<ISymbol, IMemberData>> _containingMember;
		private string? _genericName;
		private DefaultedValue<ISymbolOrMember<ISymbol, IMemberData>> _hiddenMember;
		private bool? _isNew;
		private bool? _isPartial;
		private bool? _isUnsafe;
		private bool? _isObsolete;
		private Location? _location;
		private ImmutableArray<string> _modifiers;
		private string? _substitutedName;

		/// <inheritdoc/>
		public ImmutableArray<AttributeData> Attributes
		{
			get
			{
				return _attributes.IsDefault ? (_attributes = Symbol.GetAttributes()) : _attributes;
			}
		}

		/// <inheritdoc/>
		public ISymbolOrMember<ISymbol, IMemberData>? ContainingMember
		{
			get
			{
				if(_containingMember.IsDefault)
				{
					switch (Symbol.ContainingSymbol)
					{
						case null:
							_containingMember = null;
							_containingType = null;
							_containingNamespace = null;
							break;

						case INamedTypeSymbol:
							_containingMember = new(ContainingType);
							break;

						case INamespaceSymbol:
							_containingMember = new(ContainingNamespace);
							break;

						default:
							_containingMember = new(Symbol.ContainingSymbol.ToDataOrSymbol(ParentCompilation));
							break;
					}
				}

				return _containingMember.Value;
			}
		}

		/// <inheritdoc/>
		public ISymbolOrMember<INamespaceSymbol, INamespaceData>? ContainingNamespace
		{
			get
			{
				if(_containingNamespace.IsDefault)
				{
					if(Symbol.ContainingNamespace is null)
					{
						_containingNamespace = null;
					}
					else if(_containingNamespaces is not null && !Symbol.ContainingNamespace.IsGlobalNamespace)
					{
						_containingNamespace = new(_containingNamespaces.First(ReturnOrder.ChildToParent));
					}
					else
					{
						_containingNamespace = new(Symbol.ContainingNamespace.ToDataOrSymbol(ParentCompilation));
					}
				}

				return _containingNamespace.Value;
			}
		}

		/// <inheritdoc/>
		public IWritableSymbolContainer<INamespaceSymbol, INamespaceData> ContainingNamespaces
		{
			get
			{
				return _containingNamespaces ??= Symbol.GetContainingNamespaces().ToWritableContainer(ParentCompilation);
			}
		}

		/// <inheritdoc/>
		public ISymbolOrMember<INamedTypeSymbol, ITypeData>? ContainingType
		{
			get
			{
				if (_containingType.IsDefault)
				{
					if(Symbol.ContainingType is null)
					{
						_containingType = null;
					}
					else if (_containingTypes is not null && Symbol.ContainingNamespace.IsGlobalNamespace)
					{
						_containingType = new(_containingTypes.First(ReturnOrder.ChildToParent));
					}
					else
					{
						_containingType = new(Symbol.ContainingType.ToDataOrSymbol(ParentCompilation));
					}
				}

				return _containingType.Value;
			}
		}

		/// <inheritdoc/>
		public IWritableSymbolContainer<INamedTypeSymbol, ITypeData> ContainingTypes
		{
			get
			{
				return _containingTypes ??= Symbol.GetContainingTypes().ToWritableContainer(ParentCompilation);
			}
		}

		/// <inheritdoc/>
		public SyntaxNode Declaration { get; }

		/// <inheritdoc/>
		public string GenericName
		{
			get
			{
				if(_genericName is null)
				{
					CodeBuilder builder = new(false);
					builder.Name(this, SymbolName.Generic);
					_genericName = builder.ToString();
				}

				return _genericName;
			}
		}

		/// <inheritdoc/>
		public ISymbolOrMember<ISymbol, IMemberData>? HiddenSymbol
		{
			get
			{
				if (_hiddenMember.IsDefault)
				{
					_hiddenMember = new(Symbol.GetHiddenSymbol()?.ToDataOrSymbol(ParentCompilation));
				}

				return _hiddenMember.Value;
			}
		}

		/// <inheritdoc/>
		public bool IsNew => _isNew ??= Symbol.IsNew();

		/// <inheritdoc/>
		public bool IsObsolete => _isObsolete ??= Attributes.Any(attr => attr.GetSpecialAttributeKind() == SpecialAttribute.Obsolete);

		/// <inheritdoc/>
		public bool IsPartial => _isPartial ??= Symbol.IsPartial();

		/// <inheritdoc/>
		public bool IsUnsafe => _isUnsafe ??= Symbol.IsUnsafe();

		/// <inheritdoc/>
		public Location Location => _location ??= Declaration.GetLocation();

		/// <inheritdoc/>
		public ImmutableArray<string> Modifiers
		{
			get
			{
				return _modifiers.IsDefault ? (_modifiers = Symbol.GetModifiers().ToImmutableArray()) : _modifiers;
			}
		}

		/// <inheritdoc/>
		public string Name { get; }

		/// <inheritdoc/>
		public ICompilationData ParentCompilation { get; }

		/// <summary>
		/// Root namespace of the current member (excluding the <see langword="global"/> namespace).
		/// </summary>
		public ISymbolOrMember<INamespaceSymbol, INamespaceData> RootNamespace => ContainingNamespaces.First();

		/// <inheritdoc/>
		public SemanticModel SemanticModel { get; }

		/// <inheritdoc/>
		public string SubstitutedName
		{
			get
			{
				if (_substitutedName is null)
				{
					CodeBuilder builder = new(false);
					builder.Name(this, SymbolName.Substituted);
					_substitutedName = builder.ToString();
				}

				return _substitutedName;
			}
		}

		/// <inheritdoc/>
		public ISymbol Symbol { get; }

		/// <inheritdoc/>
		public Virtuality Virtuality { get; }

		bool IMemberData.HasDeclaration => true;

		bool ISymbolOrMember.HasMember => true;
		IMemberData ISymbolOrMember.Member => this;

		/// <summary>
		/// Initializes a new instance of the <see cref="MemberData"/> class.
		/// </summary>
		/// <param name="declaration"><see cref="SyntaxNode"/> this <see cref="MemberData"/> represents.</param>
		/// <param name="compilation">Parent <see cref="ICompilationData"/> of this <see cref="MemberData"/>.</param>
		/// <param name="properties"><see cref="Properties"/> to use for the current instance.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="declaration"/> is <see langword="null"/>. -or- <paramref name="compilation"/> is <see langword="null"/>.
		/// </exception>
		public MemberData(SyntaxNode declaration, ICompilationData compilation, Properties? properties = default)
		{
			if (declaration is null)
			{
				throw new ArgumentNullException(nameof(declaration));
			}

			if (compilation is null)
			{
				throw new ArgumentNullException(nameof(compilation));
			}

			Declaration = declaration;
			ParentCompilation = compilation;

			Properties? props = properties ?? GetDefaultProperties();

			if (props is null)
			{
				SemanticModel = ParentCompilation.Compilation.GetSemanticModel(Declaration);
				Symbol = SemanticModel.GetSymbol(Declaration);

				Name = Symbol.GetVerbatimName();
				Virtuality = Symbol.GetVirtuality();
			}
			else
			{
				SemanticModel = props.SemanticModel ?? ParentCompilation.Compilation.GetSemanticModel(Declaration);
				Symbol = props.Symbol ?? SemanticModel.GetSymbol(Declaration);

				Name = props.Name ?? Symbol.GetVerbatimName();
				Virtuality = props.Virtuality ?? Symbol.GetVirtuality();

				SetProperties(props);
			}
		}

		internal MemberData(ISymbol symbol, ICompilationData compilation, Properties? properties = default)
		{
			if (symbol.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax() is not SyntaxNode decl)
			{
				throw new InvalidOperationException($"Symbol '{symbol}' doesn't define any syntax reference, thus can't be used in a {nameof(MemberData)}!");
			}

			Symbol = symbol;
			Declaration = decl;
			ParentCompilation = compilation;

			properties ??= GetDefaultProperties();

			if (properties is not null)
			{
				SemanticModel = properties.SemanticModel ?? compilation.Compilation.GetSemanticModel(decl.SyntaxTree);
				Name = properties.Name ?? Symbol.GetVerbatimName();
				Virtuality = properties.Virtuality ?? Symbol.GetVirtuality();

				SetProperties(properties);
			}
			else
			{
				SemanticModel = compilation.Compilation.GetSemanticModel(decl.SyntaxTree);
				Name = Symbol.GetVerbatimName();
				Virtuality = Symbol.GetVirtuality();
			}
		}

		/// <summary>
		/// Creates a shallow copy the current object.
		/// </summary>
		public MemberData Clone()
		{
			return CloneCore();
		}

		/// <summary>
		/// Returns new <see cref="Properties"/> build from the data contained within the current object.
		/// </summary>
		public Properties GetProperties()
		{
			return GetPropertiesCore();
		}

		/// <summary>
		/// Maps the data of the current object to the specified <paramref name="properties"/>.
		/// </summary>
		/// <param name="properties"><see cref="Properties"/> to map to.</param>
		public virtual void Map(Properties properties)
		{
			properties.Attributes = _attributes;
			properties.ContainingMember = _containingMember;
			properties.ContainingNamespace = _containingNamespace;
			properties.ContainingNamespaces = DataHelpers.ToDefaultedValue(_containingNamespaces);
			properties.ContainingType = _containingType;
			properties.ContainingTypes = DataHelpers.ToDefaultedValue(_containingTypes);
			properties.GenericName = _genericName;
			properties.HiddenSymbol = _hiddenMember;
			properties.IsNew = _isNew;
			properties.IsObsolete = _isObsolete;
			properties.IsPartial = _isPartial;
			properties.IsUnsafe = _isUnsafe;
			properties.Location = _location;
			properties.Modifiers = _modifiers;
			properties.Name = Name;
			properties.SemanticModel = SemanticModel;
			properties.SubstitutedName = _substitutedName;
			properties.Symbol = Symbol;
			properties.Virtuality = Virtuality;
		}

		/// <summary>
		/// Actually creates a shallow copy the current object.
		/// </summary>
		protected virtual MemberData CloneCore()
		{
			return new(Declaration, ParentCompilation, GetPropertiesCore());
		}

		/// <summary>
		/// Provides default <see cref="Properties"/> when no other were specified by the user.
		/// </summary>
		protected virtual Properties? GetDefaultProperties()
		{
			// Do nothing by default.
			return null;
		}

		/// <summary>
		/// Actually returns new <see cref="Properties"/> build from the data contained within the current object.
		/// </summary>
		protected virtual Properties GetPropertiesCore()
		{
			Properties properties = new();
			Map(properties);
			return properties;
		}

		/// <summary>
		/// Actually sets the properties of the current member.
		/// </summary>
		/// <param name="properties"><see cref="Properties"/> that contain the data to set.</param>
		protected virtual void SetProperties(Properties properties)
		{
			_attributes = properties.Attributes;
			_containingMember = properties.ContainingMember;
			_containingNamespace = properties.ContainingNamespace;
			_containingNamespaces = DataHelpers.FromDefaultedOrEmpty(properties.ContainingNamespaces);
			_containingType = properties.ContainingType;
			_containingTypes = DataHelpers.FromDefaultedOrEmpty(properties.ContainingTypes);
			_isNew = properties.IsNew;
			_isObsolete = properties.IsObsolete;
			_isPartial = properties.IsPartial;
			_isUnsafe = properties.IsUnsafe;
			_location = properties.Location;
			_modifiers = properties.Modifiers;
			_hiddenMember = properties.HiddenSymbol;
			_substitutedName = properties.SubstitutedName;
			_genericName = properties.GenericName;
		}

		object ICloneable.Clone()
		{
			return CloneCore();
		}
	}
}
