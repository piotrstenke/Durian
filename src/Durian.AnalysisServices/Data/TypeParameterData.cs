// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.Analysis.Data
{
	/// <summary>
	/// Encapsulates data associated with a single <see cref="TypeParameterSyntax"/>.
	/// </summary>
	public class TypeParameterData : MemberData
	{
		/// <summary>
		/// Target <see cref="TypeParameterSyntax"/>.
		/// </summary>
		public new TypeParameterSyntax Declaration => (base.Declaration as TypeParameterSyntax)!;

		/// <summary>
		/// <see cref="IPropertySymbol"/> associated with the <see cref="Declaration"/>.
		/// </summary>
		public new ITypeParameterSymbol Symbol => (base.Symbol as ITypeParameterSymbol)!;

		/// <summary>
		/// Initializes a new instance of the <see cref="TypeParameterData"/> class.
		/// </summary>
		/// <param name="declaration"><see cref="TypeParameterSyntax"/> this <see cref="TypeParameterData"/> represents.</param>
		/// <param name="compilation">Parent <see cref="ICompilationData"/> of this <see cref="TypeParameterData"/>.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="declaration"/> is <see langword="null"/>. -or- <paramref name="compilation"/> is <see langword="null"/>
		/// </exception>
		public TypeParameterData(TypeParameterSyntax declaration, ICompilationData compilation) : base(declaration, compilation)
		{
		}

		internal TypeParameterData(ITypeParameterSymbol symbol, ICompilationData compilation) : base(symbol, compilation)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="TypeParameterData"/> class.
		/// </summary>
		/// <param name="declaration"><see cref="TypeParameterSyntax"/> this <see cref="TypeParameterData"/> represents.</param>
		/// <param name="compilation">Parent <see cref="ICompilationData"/> of this <see cref="TypeParameterData"/>.</param>
		/// <param name="symbol"><see cref="ITypeParameterSymbol"/> this <see cref="TypeParameterData"/> represents.</param>
		/// <param name="semanticModel"><see cref="SemanticModel"/> of the <paramref name="declaration"/>.</param>
		/// <param name="modifiers">A collection of all modifiers applied to the <paramref name="symbol"/>.</param>
		/// <param name="containingTypes">A collection of <see cref="ITypeData"/>s the <paramref name="symbol"/> is contained within.</param>
		/// <param name="containingNamespaces">A collection of <see cref="INamespaceSymbol"/>s the <paramref name="symbol"/> is contained within.</param>
		/// <param name="attributes">A collection of <see cref="AttributeData"/>s representing the <paramref name="symbol"/> attributes.</param>
		protected internal TypeParameterData(
			TypeParameterSyntax declaration,
			ICompilationData compilation,
			ITypeParameterSymbol symbol,
			SemanticModel semanticModel,
			string[]? modifiers = null,
			IEnumerable<ITypeData>? containingTypes = null,
			IEnumerable<INamespaceSymbol>? containingNamespaces = null,
			IEnumerable<AttributeData>? attributes = null
		) : base(
			declaration,
			compilation,
			symbol,
			semanticModel,
			modifiers,
			containingTypes,
			containingNamespaces,
			attributes
		)
		{
		}
	}
}
