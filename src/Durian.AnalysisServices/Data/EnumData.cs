// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;

namespace Durian.Analysis.Data
{
	/// <summary>
	/// Encapsulates data associated with a single <see cref="EnumDeclarationSyntax"/>.
	/// </summary>
	public class EnumData : TypeData<EnumDeclarationSyntax>
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="EnumData"/> class.
		/// </summary>
		/// <param name="declaration"><see cref="EnumDeclarationSyntax"/> this <see cref="EnumData"/> represents.</param>
		/// <param name="compilation">Parent <see cref="ICompilationData"/> of this <see cref="EnumData"/>.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="declaration"/> is <see langword="null"/>. -or- <paramref name="compilation"/> is <see langword="null"/>
		/// </exception>
		public EnumData(EnumDeclarationSyntax declaration, ICompilationData compilation) : base(declaration, compilation)
		{
		}

		internal EnumData(INamedTypeSymbol symbol, ICompilationData compilation) : base(symbol, compilation)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="EnumData"/> class.
		/// </summary>
		/// <param name="declaration"><see cref="EnumDeclarationSyntax"/> this <see cref="EnumData"/> represents.</param>
		/// <param name="compilation">Parent <see cref="ICompilationData"/> of this <see cref="EnumData"/>.</param>
		/// <param name="symbol"><see cref="INamedTypeSymbol"/> this <see cref="EnumData"/> represents.</param>
		/// <param name="semanticModel"><see cref="SemanticModel"/> of the <paramref name="declaration"/>.</param>
		/// <param name="modifiers">A collection of all modifiers applied to the <paramref name="symbol"/>.</param>
		/// <param name="containingTypes">A collection of <see cref="ITypeData"/>s the <paramref name="symbol"/> is contained within.</param>
		/// <param name="containingNamespaces">A collection of <see cref="INamespaceSymbol"/>s the <paramref name="symbol"/> is contained within.</param>
		/// <param name="attributes">A collection of <see cref="AttributeData"/>s representing the <paramref name="symbol"/> attributes.</param>
		protected internal EnumData(
			EnumDeclarationSyntax declaration,
			ICompilationData compilation,
			INamedTypeSymbol symbol,
			SemanticModel semanticModel,
			IEnumerable<SyntaxToken>? modifiers = null,
			IEnumerable<ITypeData>? containingTypes = null,
			IEnumerable<INamespaceSymbol>? containingNamespaces = null,
			IEnumerable<AttributeData>? attributes = null
		) : base(
			declaration,
			compilation,
			symbol,
			semanticModel,
			null,
			modifiers,
			containingTypes,
			containingNamespaces,
			attributes
		)
		{
		}
	}
}