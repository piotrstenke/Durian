// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Durian.Analysis.Extensions;
using Durian.Analysis.SymbolContainers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.Analysis.Data
{
	/// <summary>
	/// Encapsulates data associated with a single <see cref="IndexerDeclarationSyntax"/>.
	/// </summary>
	public class IndexerData : MemberData, IPropertyData
	{
		/// <summary>
		/// Contains optional data that can be passed to a <see cref="IndexerData"/>.
		/// </summary>
		public new class Properties : Properties<IPropertySymbol>
		{
			/// <inheritdoc cref="IndexerData.AutoPropertyKind"/>
			public AutoPropertyKind? AutoPropertyKind { get; set; }

			/// <inheritdoc cref="IndexerData.Parameters"/>
			public DefaultedValue<ISymbolContainer<IParameterSymbol, IParameterData>> Parameters { get; set; }

			/// <inheritdoc cref="IndexerData.OverriddenIndexer"/>
			public DefaultedValue<ISymbolOrMember<IPropertySymbol, IPropertyData>> OverriddenIndexer { get; set; }

			/// <inheritdoc cref="IndexerData.OverriddenIndexers"/>
			public DefaultedValue<ISymbolContainer<IPropertySymbol, IPropertyData>> OverriddenIndexers { get; set; }

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
		private DefaultedValue<ISymbolOrMember<IPropertySymbol, IPropertyData>> _overriddenIndexer;
		private ISymbolContainer<IPropertySymbol, IPropertyData>? _overriddenIndexers;

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

		/// <summary>
		/// Indexer overridden by this indexer.
		/// </summary>
		public ISymbolOrMember<IPropertySymbol, IPropertyData>? OverriddenIndexer
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
						_overriddenIndexer = new(Symbol.OverriddenProperty.ToDataOrSymbol(ParentCompilation));
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

		/// <summary>
		/// All indexers overridden by this indexer.
		/// </summary>
		public ISymbolContainer<IPropertySymbol, IPropertyData> OverriddenIndexers
		{
			get
			{
				return _overriddenIndexers ??= Symbol.GetOverriddenSymbols().ToContainer(ParentCompilation);
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
			properties.Parameters = DataHelpers.ToDefaultedValue(_parameters);
			properties.OverriddenIndexer = _overriddenIndexer;
			properties.OverriddenIndexers = DataHelpers.ToDefaultedValue(_overriddenIndexers);
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
			}
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
}
