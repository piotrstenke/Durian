// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
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
		}

		private DefaultedValue<ISymbolOrMember<IFieldSymbol, IFieldData>> _backingField;

		/// <summary>
		/// Target <see cref="PropertyDeclarationSyntax"/>.
		/// </summary>
		public new PropertyDeclarationSyntax Declaration => (base.Declaration as PropertyDeclarationSyntax)!;

		/// <summary>
		/// <see cref="IPropertySymbol"/> associated with the <see cref="Declaration"/>.
		/// </summary>
		public new IPropertySymbol Symbol => (base.Symbol as IPropertySymbol)!;

		/// <summary>
		/// Determines whether this property is an auto-property.
		/// </summary>
		public bool IsAutoProperty => AutoPropertyKind != AutoPropertyKind.None;

		/// <summary>
		/// Backing field of the property or <see langword="null"/> if not an auto-property.
		/// </summary>
		public ISymbolOrMember<IFieldSymbol, IFieldData>? BackingField
		{
			get
			{
				if(_backingField.IsDefault)
				{
					_backingField = new(Symbol.GetBackingField()?.ToDataOrSymbol(ParentCompilation));
				}

				return _backingField.Value;
			}
		}

		/// <summary>
		/// Kind of the auto-property.
		/// </summary>
		public AutoPropertyKind AutoPropertyKind { get; }

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
			if(properties is not null)
			{
				_backingField = properties.BackingField;
				AutoPropertyKind = properties.AutoPropertyKind ?? Symbol.GetAutoPropertyKind();
			}
			else
			{
				AutoPropertyKind = Symbol.GetAutoPropertyKind();
			}
		}

		internal PropertyData(IPropertySymbol symbol, ICompilationData compilation) : base(symbol, compilation)
		{
		}
	}
}
