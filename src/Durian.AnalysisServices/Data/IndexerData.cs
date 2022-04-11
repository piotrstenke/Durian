﻿// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.Analysis.Data
{
	/// <summary>
	/// Encapsulates data associated with a single <see cref="IndexerDeclarationSyntax"/>.
	/// </summary>
	public class IndexerData : MemberData
	{
		/// <summary>
		/// Target <see cref="IndexerDeclarationSyntax"/>.
		/// </summary>
		public new IndexerDeclarationSyntax Declaration => (base.Declaration as IndexerDeclarationSyntax)!;

		/// <summary>
		/// <see cref="IPropertySymbol"/> associated with the <see cref="Declaration"/>.
		/// </summary>
		public new IPropertySymbol Symbol => (base.Symbol as IPropertySymbol)!;

		/// <summary>
		/// Initializes a new instance of the <see cref="IndexerData"/> class.
		/// </summary>
		/// <param name="declaration"><see cref="IndexerDeclarationSyntax"/> this <see cref="IndexerData"/> represents.</param>
		/// <param name="compilation">Parent <see cref="ICompilationData"/> of this <see cref="IndexerData"/>.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="declaration"/> is <see langword="null"/>. -or- <paramref name="compilation"/> is <see langword="null"/>
		/// </exception>
		public IndexerData(IndexerDeclarationSyntax declaration, ICompilationData compilation) : base(declaration, compilation)
		{
		}

		internal IndexerData(IPropertySymbol symbol, ICompilationData compilation) : base(symbol, compilation)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="IndexerData"/> class.
		/// </summary>
		/// <param name="declaration"><see cref="IndexerDeclarationSyntax"/> this <see cref="IndexerData"/> represents.</param>
		/// <param name="compilation">Parent <see cref="ICompilationData"/> of this <see cref="IndexerData"/>.</param>
		/// <param name="symbol"><see cref="IPropertySymbol"/> this <see cref="IndexerData"/> represents.</param>
		/// <param name="semanticModel"><see cref="SemanticModel"/> of the <paramref name="declaration"/>.</param>
		/// <param name="containingTypes">A collection of <see cref="ITypeData"/>s the <paramref name="symbol"/> is contained within.</param>
		/// <param name="containingNamespaces">A collection of <see cref="INamespaceSymbol"/>s the <paramref name="symbol"/> is contained within.</param>
		/// <param name="attributes">A collection of <see cref="AttributeData"/>s representing the <paramref name="symbol"/> attributes.</param>
		protected internal IndexerData(
			IndexerDeclarationSyntax declaration,
			ICompilationData compilation,
			IPropertySymbol symbol,
			SemanticModel semanticModel,
			IEnumerable<ITypeData>? containingTypes = null,
			IEnumerable<INamespaceSymbol>? containingNamespaces = null,
			IEnumerable<AttributeData>? attributes = null
		) : base(
			declaration,
			compilation,
			symbol,
			semanticModel,
			containingTypes,
			containingNamespaces,
			attributes
		)
		{
		}
	}
}
