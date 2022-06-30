// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Durian.Analysis.Extensions;
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
			/// <inheritdoc cref="PropertyData.AutoPropertyKind"/>
			public AutoPropertyKind? AutoPropertyKind { get; set; }

			/// <summary>
			/// Initializes a new instance of the <see cref="Properties"/> class.
			/// </summary>
			public Properties()
			{
			}
		}

		/// <summary>
		/// Target <see cref="IndexerDeclarationSyntax"/>.
		/// </summary>
		public new IndexerDeclarationSyntax Declaration => (base.Declaration as IndexerDeclarationSyntax)!;

		/// <summary>
		/// <see cref="IPropertySymbol"/> associated with the <see cref="Declaration"/>.
		/// </summary>
		public new IPropertySymbol Symbol => (base.Symbol as IPropertySymbol)!;

		/// <summary>
		/// Determines whether this property is an auto-property.
		/// </summary>
		public bool IsAutoProperty => AutoPropertyKind != AutoPropertyKind.None;

		/// <summary>
		/// Kind of the auto-property.
		/// </summary>
		public AutoPropertyKind AutoPropertyKind { get; }

		BasePropertyDeclarationSyntax IPropertyData.Declaration => Declaration;

		ISymbolOrMember<IFieldSymbol, IFieldData>? IPropertyData.BackingField => null;

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
			AutoPropertyKind = properties?.AutoPropertyKind ?? Symbol.GetAutoPropertyKind();
		}

		internal IndexerData(IPropertySymbol symbol, ICompilationData compilation) : base(symbol, compilation)
		{
		}
	}
}
