// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.Analysis.Data
{
	/// <summary>
	/// Encapsulates data associated with a single <see cref="ConstructorDeclarationSyntax"/>.
	/// </summary>
	public class ConstructorData : MethodData<ConstructorDeclarationSyntax>
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="ConstructorData"/> class.
		/// </summary>
		/// <param name="declaration"><see cref="ConstructorDeclarationSyntax"/> this <see cref="ConstructorData"/> represents.</param>
		/// <param name="compilation">Parent <see cref="ICompilationData"/> of this <see cref="ConstructorData"/>.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="declaration"/> is <see langword="null"/>. -or- <paramref name="compilation"/> is <see langword="null"/>
		/// </exception>
		public ConstructorData(ConstructorDeclarationSyntax declaration, ICompilationData compilation) : base(declaration, compilation)
		{
		}

		internal ConstructorData(IMethodSymbol symbol, ICompilationData compilation) : base(symbol, compilation)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ConstructorData"/> class.
		/// </summary>
		/// <param name="declaration"><see cref="ConstructorDeclarationSyntax"/> this <see cref="ConstructorData"/> represents.</param>
		/// <param name="compilation">Parent <see cref="ICompilationData"/> of this <see cref="ConstructorData"/>.</param>
		/// <param name="symbol"><see cref="IMethodSymbol"/> this <see cref="ConstructorData"/> represents.</param>
		/// <param name="semanticModel"><see cref="SemanticModel"/> of the <paramref name="declaration"/>.</param>
		/// <param name="containingTypes">A collection of <see cref="ITypeData"/>s the <paramref name="symbol"/> is contained within.</param>
		/// <param name="containingNamespaces">A collection of <see cref="INamespaceSymbol"/>s the <paramref name="symbol"/> is contained within.</param>
		/// <param name="attributes">A collection of <see cref="AttributeData"/>s representing the <paramref name="symbol"/> attributes.</param>
		protected internal ConstructorData(
			ConstructorDeclarationSyntax declaration,
			ICompilationData compilation,
			IMethodSymbol symbol,
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
