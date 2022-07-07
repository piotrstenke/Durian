// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Durian.Analysis.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.Analysis.Data
{
	/// <summary>
	/// Encapsulates data associated with a single <see cref="PropertyDeclarationSyntax"/>.
	/// </summary>
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
			protected override void FillWithDefaultData()
			{
				IsPartial = false;
			}

			/// <inheritdoc/>
			protected override MemberData.Properties CloneCore()
			{
				Properties properties = new();
				Map(properties);
				return properties;
			}
		}

		private AutoPropertyKind? _autoPropertyKind;
		private DefaultedValue<ISymbolOrMember<IFieldSymbol, IFieldData>> _backingField;

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

		/// <summary>
		/// Determines whether this property is an auto-property.
		/// </summary>
		public bool IsAutoProperty => AutoPropertyKind != AutoPropertyKind.None;

		/// <summary>
		/// <see cref="IPropertySymbol"/> associated with the <see cref="Declaration"/>.
		/// </summary>
		public new IPropertySymbol Symbol => (base.Symbol as IPropertySymbol)!;

		BasePropertyDeclarationSyntax IPropertyData.Declaration => Declaration;

		IPropertyData ISymbolOrMember<IPropertySymbol, IPropertyData>.Member => this;

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
			SetAutoPropertyKind();
		}

		internal PropertyData(IPropertySymbol symbol, ICompilationData compilation, MemberData.Properties? properties = default) : base(symbol, compilation, properties)
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
			properties.BackingField = _backingField;
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
