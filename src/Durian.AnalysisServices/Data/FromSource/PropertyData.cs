using System;
using System.ComponentModel;
using System.Linq;
using Durian.Analysis.SymbolContainers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.Analysis.Data.FromSource;

/// <inheritdoc cref="IPropertyData"/>
public class PropertyData : MemberData, IPropertyData
{
	/// <summary>
	/// Contains optional data that can be passed to a <see cref="PropertyData"/>.
	/// </summary>
	public new class Properties : Properties<IPropertySymbol>
	{
		/// <inheritdoc cref="PropertyData.AutoPropertyKind"/>
		public AutoPropertyKind? AutoPropertyKind { get; set; }

		/// <inheritdoc cref="PropertyData.BackingField"/>
		public DefaultedValue<ISymbolOrMember<IFieldSymbol, IFieldData>> BackingField { get; set; }

		/// <inheritdoc cref="PropertyData.IsDefaultImplementation"/>
		public bool? IsDefaultImplementation { get; set; }

		/// <inheritdoc cref="PropertyData.ExplicitInterfaceImplementation"/>
		public DefaultedValue<ISymbolOrMember<IPropertySymbol, IPropertyData>> ExplicitInterfaceImplementation { get; set; }

		/// <inheritdoc cref="PropertyData.ImplicitInterfaceImplementations"/>
		public DefaultedValue<ISymbolContainer<IPropertySymbol, IPropertyData>> ImplicitInterfaceImplementations { get; set; }

		/// <inheritdoc cref="PropertyData.OverriddenProperty"/>
		public DefaultedValue<ISymbolOrMember<IPropertySymbol, IPropertyData>> OverriddenProperty { get; set; }

		/// <inheritdoc cref="PropertyData.OverriddenProperties"/>
		public DefaultedValue<ISymbolContainer<IPropertySymbol, IPropertyData>> OverriddenProperties { get; set; }

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
			properties.AutoPropertyKind = AutoPropertyKind;
			properties.BackingField = BackingField;
			properties.OverriddenProperty = OverriddenProperty;
			properties.OverriddenProperties = OverriddenProperties;
			properties.IsDefaultImplementation = IsDefaultImplementation;
			properties.ImplicitInterfaceImplementations = ImplicitInterfaceImplementations;
			properties.ExplicitInterfaceImplementation = ExplicitInterfaceImplementation;
		}

		/// <inheritdoc/>
#pragma warning disable CS0809 // Obsolete member overrides non-obsolete member
		[Obsolete("Use Map(Properties) instead")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public sealed override void Map(Properties<IPropertySymbol> properties)
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
		}
	}

	private AutoPropertyKind? _autoPropertyKind;
	private DefaultedValue<ISymbolOrMember<IFieldSymbol, IFieldData>> _backingField;
	private DefaultedValue<ISymbolOrMember<IPropertySymbol, IPropertyData>> _overriddenProperty;
	private ISymbolContainer<IPropertySymbol, IPropertyData>? _overriddenProperties;
	private bool? _isDefaultImplementation;
	private DefaultedValue<ISymbolOrMember<IPropertySymbol, IPropertyData>> _explicitImplementation;
	private ISymbolContainer<IPropertySymbol, IPropertyData>? _implicitImplementations;

	/// <summary>
	/// Kind of the auto-property.
	/// </summary>
	public AutoPropertyKind AutoPropertyKind => _autoPropertyKind!.Value;

	/// <summary>
	/// Backing field of the property or <see langword="null"/> if not an auto-property.
	/// </summary>
	public ISymbolOrMember<IFieldSymbol, IFieldData>? BackingField
	{
		get
		{
			if (_backingField.IsDefault)
			{
				_backingField = new(Symbol.GetBackingField()?.ToDataOrSymbol(ParentCompilation));
			}

			return _backingField.Value;
		}
	}

	/// <summary>
	/// Target <see cref="PropertyDeclarationSyntax"/>.
	/// </summary>
	public new PropertyDeclarationSyntax Declaration => (base.Declaration as PropertyDeclarationSyntax)!;

	/// <inheritdoc/>
	public ISymbolOrMember<IPropertySymbol, IPropertyData>? ExplicitInterfaceImplementation
	{
		get
		{
			if (_explicitImplementation.IsDefault)
			{
				_explicitImplementation = new(Symbol.ExplicitInterfaceImplementations.FirstOrDefault()?.ToDataOrSymbol(ParentCompilation));
			}

			return _explicitImplementation.Value;
		}
	}

	/// <inheritdoc/>
	public ISymbolContainer<IPropertySymbol, IPropertyData> ImplicitInterfaceImplementations
	{
		get
		{
			return _implicitImplementations ??= SymbolExtensions.GetImplicitImplementations_Internal(Symbol, intf => intf
				.ToDataOrSymbol(ParentCompilation)
				.Member
				.GetMembers(IncludedMembers.Direct)
				.AsEnumerable()
				.Select(s => s.Symbol)
				.OfType<IPropertySymbol>()
				.Where(m => m.Name == Symbol.Name))
			.ToContainer(ParentCompilation);
		}
	}

	/// <summary>
	/// Determines whether this property is an auto-property.
	/// </summary>
	public bool IsAutoProperty => AutoPropertyKind != AutoPropertyKind.None;

	/// <inheritdoc/>
	public bool IsDefaultImplementation => _isDefaultImplementation ??= Symbol.IsDefaultImplementation();

	/// <inheritdoc/>
	public ISymbolOrMember<IPropertySymbol, IPropertyData>? OverriddenProperty
	{
		get
		{
			if (_overriddenProperty.IsDefault)
			{
				if (Symbol.OverriddenProperty is null)
				{
					_overriddenProperty = null;
				}
				else if (_overriddenProperties is null)
				{
					_overriddenProperty = new(Symbol.OverriddenProperty.ToDataOrSymbol(ParentCompilation));
				}
				else if (_overriddenProperties.Count > 0)
				{
					_overriddenProperty = new(_overriddenProperties.First(ReturnOrder.ChildToParent));
				}
				else
				{
					_overriddenProperty = null;
				}
			}

			return _overriddenProperty.Value;
		}
	}

	/// <inheritdoc/>
	public ISymbolContainer<IPropertySymbol, IPropertyData> OverriddenProperties
	{
		get
		{
			return _overriddenProperties ??= Symbol.GetOverriddenSymbols().ToContainer(ParentCompilation);
		}
	}

	/// <summary>
	/// <see cref="IPropertySymbol"/> associated with the <see cref="Declaration"/>.
	/// </summary>
	public new IPropertySymbol Symbol => (base.Symbol as IPropertySymbol)!;

	BasePropertyDeclarationSyntax IPropertyData.Declaration => Declaration;

	IPropertyData ISymbolOrMember<IPropertySymbol, IPropertyData>.Member => this;

	ISymbolContainer<IParameterSymbol, IParameterData> IPropertyData.Parameters => SymbolContainerFactory.Empty<IParameterSymbol, IParameterData>();

	/// <summary>
	/// Initializes a new instance of the <see cref="PropertyData"/> class.
	/// </summary>
	/// <param name="declaration"><see cref="PropertyDeclarationSyntax"/> this <see cref="PropertyData"/> represents.</param>
	/// <param name="compilation">Parent <see cref="ICompilationData"/> of this <see cref="PropertyData"/>.</param>
	/// <param name="properties"><see cref="Properties"/> to use for the current instance.</param>
	/// <exception cref="ArgumentNullException">
	/// <paramref name="declaration"/> is <see langword="null"/>. -or- <paramref name="compilation"/> is <see langword="null"/>.
	/// </exception>
	public PropertyData(PropertyDeclarationSyntax declaration, ICompilationData compilation, Properties? properties = default) : base(declaration, compilation, properties)
	{
	}

	internal PropertyData(IPropertySymbol symbol, ICompilationData compilation, MemberData.Properties? properties = default) : base(symbol, compilation, properties)
	{
	}

	/// <inheritdoc cref="MemberData.Clone"/>
	public new PropertyData Clone()
	{
		return (CloneCore() as PropertyData)!;
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
		properties.AutoPropertyKind = _autoPropertyKind;
		properties.BackingField = _backingField;
		properties.OverriddenProperty = _overriddenProperty;
		properties.OverriddenProperties = DataHelpers.ToDefaultedValue(_overriddenProperties);
		properties.ImplicitInterfaceImplementations = DataHelpers.ToDefaultedValue(_implicitImplementations);
		properties.ExplicitInterfaceImplementation = _explicitImplementation;
		properties.IsDefaultImplementation = _isDefaultImplementation;
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
		return new PropertyData(Declaration, ParentCompilation, GetProperties());
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
			_backingField = props.BackingField;
			_autoPropertyKind = props.AutoPropertyKind ?? Symbol.GetAutoPropertyKind();
			_isDefaultImplementation = props.IsDefaultImplementation;
			_explicitImplementation = props.ExplicitInterfaceImplementation;
			_implicitImplementations = DataHelpers.FromDefaultedOrEmpty(props.ImplicitInterfaceImplementations);
			_overriddenProperties = DataHelpers.FromDefaultedOrEmpty(props.OverriddenProperties);
			_overriddenProperty = props.OverriddenProperty;
		}

		_autoPropertyKind = Symbol.GetAutoPropertyKind();
	}

	IPropertyData IPropertyData.Clone()
	{
		return Clone();
	}
}
