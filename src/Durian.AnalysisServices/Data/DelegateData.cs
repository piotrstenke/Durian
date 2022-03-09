// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;

namespace Durian.Analysis.Data
{
	/// <summary>
	/// Encapsulates data associated with a single <see cref="DelegateDeclarationSyntax"/>.
	/// </summary>
	public class DelegateData : MemberData
	{
		/// <summary>
		/// Target <see cref="DelegateDeclarationSyntax"/>.
		/// </summary>
		public new DelegateDeclarationSyntax Declaration => (base.Declaration as DelegateDeclarationSyntax)!;

		/// <summary>
		/// <see cref="INamedTypeSymbol"/> associated with the <see cref="Declaration"/>.
		/// </summary>
		public new INamedTypeSymbol Symbol => (base.Symbol as INamedTypeSymbol)!;

		/// <summary>
		/// Initializes a new instance of the <see cref="DelegateData"/> class.
		/// </summary>
		/// <param name="declaration"><see cref="DelegateDeclarationSyntax"/> this <see cref="DelegateData"/> represents.</param>
		/// <param name="compilation">Parent <see cref="ICompilationData"/> of this <see cref="DelegateData"/>.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="declaration"/> is <see langword="null"/>. -or- <paramref name="compilation"/> is <see langword="null"/>
		/// </exception>
		public DelegateData(DelegateDeclarationSyntax declaration, ICompilationData compilation) : base(declaration, compilation)
		{
		}

		internal DelegateData(INamedTypeSymbol symbol, ICompilationData compilation) : base(symbol, compilation)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DelegateData"/> class.
		/// </summary>
		/// <param name="declaration"><see cref="DelegateDeclarationSyntax"/> this <see cref="DelegateData"/> represents.</param>
		/// <param name="compilation">Parent <see cref="ICompilationData"/> of this <see cref="DelegateData"/>.</param>
		/// <param name="symbol"><see cref="INamedTypeSymbol"/> this <see cref="DelegateData"/> represents.</param>
		/// <param name="semanticModel"><see cref="SemanticModel"/> of the <paramref name="declaration"/>.</param>
		/// <param name="containingTypes">A collection of <see cref="ITypeData"/>s the <paramref name="symbol"/> is contained within.</param>
		/// <param name="containingNamespaces">A collection of <see cref="INamespaceSymbol"/>s the <paramref name="symbol"/> is contained within.</param>
		/// <param name="attributes">A collection of <see cref="AttributeData"/>s representing the <paramref name="symbol"/> attributes.</param>
		protected internal DelegateData(
			DelegateDeclarationSyntax declaration,
			ICompilationData compilation,
			INamedTypeSymbol symbol,
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