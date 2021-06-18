// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.Analysis.Data
{
	/// <summary>
	/// Encapsulates data associated with a single <see cref="RecordDeclarationSyntax"/>.
	/// </summary>
	public class RecordData : TypeData<RecordDeclarationSyntax>
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="RecordData"/> class.
		/// </summary>
		/// <param name="declaration"><see cref="RecordDeclarationSyntax"/> this <see cref="RecordData"/> represents.</param>
		/// <param name="compilation">Parent <see cref="ICompilationData"/> of this <see cref="RecordData"/>.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="declaration"/> is <see langword="null"/>. -or- <paramref name="compilation"/> is <see langword="null"/>
		/// </exception>
		public RecordData(RecordDeclarationSyntax declaration, ICompilationData compilation) : base(declaration, compilation)
		{
		}

		internal RecordData(INamedTypeSymbol symbol, ICompilationData compilation) : base(symbol, compilation)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="RecordData"/> class.
		/// </summary>
		/// <param name="declaration"><see cref="RecordDeclarationSyntax"/> this <see cref="RecordData"/> represents.</param>
		/// <param name="compilation">Parent <see cref="ICompilationData"/> of this <see cref="RecordData"/>.</param>
		/// <param name="symbol"><see cref="INamedTypeSymbol"/> this <see cref="RecordData"/> represents.</param>
		/// <param name="semanticModel"><see cref="SemanticModel"/> of the <paramref name="declaration"/>.</param>
		/// <param name="partialDeclarations">A collection of <see cref="RecordDeclarationSyntax"/> that represent the partial declarations of the target <paramref name="symbol"/>.</param>
		/// <param name="modifiers">A collection of all modifiers applied to the <paramref name="symbol"/>.</param>
		/// <param name="containingTypes">A collection of <see cref="ITypeData"/>s the <paramref name="symbol"/> is contained within.</param>
		/// <param name="containingNamespaces">A collection of <see cref="INamespaceSymbol"/>s the <paramref name="symbol"/> is contained within.</param>
		/// <param name="attributes">A collection of <see cref="AttributeData"/>s representing the <paramref name="symbol"/> attributes.</param>
		protected internal RecordData(
			RecordDeclarationSyntax declaration,
			ICompilationData compilation,
			INamedTypeSymbol symbol,
			SemanticModel semanticModel,
			IEnumerable<RecordDeclarationSyntax>? partialDeclarations,
			IEnumerable<SyntaxToken>? modifiers,
			IEnumerable<ITypeData>? containingTypes,
			IEnumerable<INamespaceSymbol>? containingNamespaces,
			IEnumerable<AttributeData>? attributes
		) : base(declaration, compilation, symbol, semanticModel, partialDeclarations, modifiers, containingTypes, containingNamespaces, attributes)
		{
		}
	}
}
