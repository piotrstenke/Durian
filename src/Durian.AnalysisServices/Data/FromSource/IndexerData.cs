using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Durian.Analysis.SymbolContainers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.Analysis.Data.FromSource;

/// <inheritdoc cref="IIndexerData"/>
public class IndexerData : MemberData, IIndexerData
{
	/// <summary>
	/// Contains optional data that can be passed to a <see cref="IndexerData"/>.
	/// </summary>
	public new class Properties : Properties<IPropertySymbol>
	{
		/// <inheritdoc cref="IndexerData.AutoPropertyKind"/>
		public AutoPropertyKind? AutoPropertyKind { get; set; }

		/// <inheritdoc cref="IndexerData.IsDefaultImplementation"/>
		public bool? IsDefaultImplementation { get; set; }

		/// <inheritdoc cref="IndexerData.ExplicitInterfaceImplementation"/>
		public DefaultedValue<ISymbolOrMember<IPropertySymbol, IIndexerData>> ExplicitInterfaceImplementation { get; set; }

		/// <inheritdoc cref="IndexerData.ImplicitInterfaceImplementations"/>
		public DefaultedValue<ISymbolContainer<IPropertySymbol, IIndexerData>> ImplicitInterfaceImplementations { get; set; }

		/// <inheritdoc cref="IndexerData.Parameters"/>
		public DefaultedValue<ISymbolContainer<IParameterSymbol, IParameterData>> Parameters { get; set; }

		/// <inheritdoc cref="IndexerData.OverriddenIndexer"/>
		public DefaultedValue<ISymbolOrMember<IPropertySymbol, IIndexerData>> OverriddenIndexer { get; set; }

		/// <inheritdoc cref="IndexerData.OverriddenIndexers"/>
		public DefaultedValue<ISymbolContainer<IPropertySymbol, IIndexerData>> OverriddenIndexers { get; set; }

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
			properties.Parameters = Parameters;
			properties.OverriddenIndexer = OverriddenIndexer;
			properties.OverriddenIndexers = OverriddenIndexers;
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
	private ISymbolContainer<IParameterSymbol, IParameterData>? _parameters;
	private DefaultedValue<ISymbolOrMember<IPropertySymbol, IIndexerData>> _overriddenIndexer;
	private ISymbolContainer<IPropertySymbol, IIndexerData>? _overriddenIndexers;
	private bool? _isDefaultImplementation;
	private DefaultedValue<ISymbolOrMember<IPropertySymbol, IIndexerData>> _explicitImplementation;
	private ISymbolContainer<IPropertySymbol, IIndexerData>? _implicitImplementations;

	/// <summary>
	/// Kind of the auto-property.
	/// </summary>
	public AutoPropertyKind AutoPropertyKind => _autoPropertyKind!.Value;

	/// <summary>
	/// Target <see cref="IndexerDeclarationSyntax"/>.
	/// </summary>
	public new IndexerDeclarationSyntax Declaration => (base.Declaration as IndexerDeclarationSyntax)!;

	/// <summary>
	/// Determines whether this property is an auto-property.
	/// </summary>
	public bool IsAutoProperty => AutoPropertyKind != AutoPropertyKind.None;

	/// <inheritdoc/>
	public bool IsDefaultImplementation => _isDefaultImplementation ??= Symbol.IsDefaultImplementation();

	/// <inheritdoc/>
	public ISymbolOrMember<IPropertySymbol, IIndexerData>? ExplicitInterfaceImplementation
	{
		get
		{
			if (_explicitImplementation.IsDefault)
			{
				_explicitImplementation = new(Symbol.ExplicitInterfaceImplementations.FirstOrDefault()?.ToData(ParentCompilation) as IIndexerData);
			}

			return _explicitImplementation.Value;
		}
	}

	/// <inheritdoc/>
	public ISymbolContainer<IPropertySymbol, IIndexerData> ImplicitInterfaceImplementations
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
				.Where(m => m.IsIndexer && m.Parameters.Length == Symbol.Parameters.Length))
			.ToContainer<IIndexerData>(ParentCompilation);
		}
	}

	/// <inheritdoc/>
	public ISymbolOrMember<IPropertySymbol, IIndexerData>? OverriddenIndexer
	{
		get
		{
			if (_overriddenIndexer.IsDefault)
			{
				if (Symbol.OverriddenProperty is null)
				{
					_overriddenIndexer = null;
				}
				else if (_overriddenIndexers is null)
				{
					_overriddenIndexer = new(Symbol.OverriddenProperty.ToDataOrSymbol<IIndexerData>(ParentCompilation));
				}
				else if (_overriddenIndexers.Count > 0)
				{
					_overriddenIndexer = new(_overriddenIndexers.First(ReturnOrder.ChildToParent));
				}
				else
				{
					_overriddenIndexer = null;
				}
			}

			return _overriddenIndexer.Value;
		}
	}

	/// <inheritdoc/>
	public ISymbolContainer<IPropertySymbol, IIndexerData> OverriddenIndexers
	{
		get
		{
			return _overriddenIndexers ??= Symbol.GetOverriddenSymbols().ToContainer<IIndexerData>(ParentCompilation);
		}
	}

	/// <summary>
	/// Container of <see cref="IParameterSymbol"/> of this indexer.
	/// </summary>
	public ISymbolContainer<IParameterSymbol, IParameterData> Parameters
	{
		get
		{
			return _parameters ??= Symbol.Parameters.ToContainer(ParentCompilation);
		}
	}

	/// <summary>
	/// <see cref="IPropertySymbol"/> associated with the <see cref="Declaration"/>.
	/// </summary>
	public new IPropertySymbol Symbol => (base.Symbol as IPropertySymbol)!;

	ISymbolOrMember<IPropertySymbol, IPropertyData>? IPropertyData.OverriddenProperty => OverriddenIndexer;
	ISymbolContainer<IPropertySymbol, IPropertyData> IPropertyData.OverriddenProperties => OverriddenIndexers;
	ISymbolOrMember<IFieldSymbol, IFieldData>? IPropertyData.BackingField => null;
	BasePropertyDeclarationSyntax IPropertyData.Declaration => Declaration;
	IPropertyData ISymbolOrMember<IPropertySymbol, IPropertyData>.Member => this;
	IIndexerData ISymbolOrMember<IPropertySymbol, IIndexerData>.Member => this;

	ISymbolOrMember<IPropertySymbol, IPropertyData>? IPropertyData.ExplicitInterfaceImplementation => ExplicitInterfaceImplementation;

	ISymbolContainer<IPropertySymbol, IPropertyData> IPropertyData.ImplicitInterfaceImplementations => ImplicitInterfaceImplementations;

	/// <summary>
	/// Initializes a new instance of the <see cref="IndexerData"/> class.
	/// </summary>
	/// <param name="declaration"><see cref="IndexerDeclarationSyntax"/> this <see cref="IndexerData"/> represents.</param>
	/// <param name="compilation">Parent <see cref="ICompilationData"/> of this <see cref="IndexerData"/>.</param>
	/// <param name="properties"><see cref="Properties"/> to use for the current instance.</param>
	/// <exception cref="ArgumentNullException">
	/// <paramref name="declaration"/> is <see langword="null"/>. -or- <paramref name="compilation"/> is <see langword="null"/>.
	/// </exception>
	public IndexerData(IndexerDeclarationSyntax declaration, ICompilationData compilation, Properties? properties = default) : base(declaration, compilation, properties)
	{
		SetAutoPropertyKind();
	}

	internal IndexerData(IPropertySymbol symbol, ICompilationData compilation, MemberData.Properties? properties = default) : base(symbol, compilation, properties)
	{
		SetAutoPropertyKind();
	}

	/// <inheritdoc cref="MemberData.Clone"/>
	public new IndexerData Clone()
	{
		return (CloneCore() as IndexerData)!;
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
		properties.Parameters = DataHelpers.ToDefaultedValue(_parameters);
		properties.OverriddenIndexer = _overriddenIndexer;
		properties.OverriddenIndexers = DataHelpers.ToDefaultedValue(_overriddenIndexers);
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
		return new IndexerData(Declaration, ParentCompilation, GetProperties());
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
			_autoPropertyKind = props.AutoPropertyKind;
			_parameters = DataHelpers.FromDefaultedOrEmpty(props.Parameters);
			_overriddenIndexer = props.OverriddenIndexer;
			_overriddenIndexers = DataHelpers.FromDefaultedOrEmpty(props.OverriddenIndexers);
			_isDefaultImplementation = props.IsDefaultImplementation;
			_explicitImplementation = props.ExplicitInterfaceImplementation;
			_implicitImplementations = DataHelpers.FromDefaultedOrEmpty(props.ImplicitInterfaceImplementations);
		}
	}

	IIndexerData IIndexerData.Clone()
	{
		return Clone();
	}

	IPropertyData IPropertyData.Clone()
	{
		return Clone();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void SetAutoPropertyKind()
	{
		if (!_autoPropertyKind.HasValue)
		{
			_autoPropertyKind = Symbol.GetAutoPropertyKind();
		}
	}
}
