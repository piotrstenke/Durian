// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Durian.Analysis.CodeGeneration;
using Durian.Analysis.SymbolContainers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.Analysis.Data
{
	/// <summary>
	/// Encapsulates data associated with a single <see cref="BaseNamespaceDeclarationSyntax"/>.
	/// </summary>
	public class NamespaceData : MemberData
	{
		/// <summary>
		/// Target <see cref="BaseNamespaceDeclarationSyntax"/>.
		/// </summary>
		public new BaseNamespaceDeclarationSyntax Declaration => (base.Declaration as BaseNamespaceDeclarationSyntax)!;

		/// <summary>
		/// <see cref="IPropertySymbol"/> associated with the <see cref="Declaration"/>.
		/// </summary>
		public new INamespaceSymbol Symbol => (base.Symbol as INamespaceSymbol)!;

		/// <summary>
		/// Type of this namespace declaration.
		/// </summary>
		public NamespaceStyle DeclarationType
		{
			get
			{
				if(Declaration is FileScopedNamespaceDeclarationSyntax)
				{
					return NamespaceStyle.File;
				}

				return NamespaceStyle.Default;
			}
		}

		internal CSharpSyntaxNode BaseDeclaration => base.Declaration;
		internal ISymbol BaseSymbol => base.Symbol;

		/// <summary>
		/// Initializes a new instance of the <see cref="PropertyData"/> class.
		/// </summary>
		/// <param name="declaration"><see cref="BaseNamespaceDeclarationSyntax"/> this <see cref="PropertyData"/> represents.</param>
		/// <param name="compilation">Parent <see cref="ICompilationData"/> of this <see cref="PropertyData"/>.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="declaration"/> is <see langword="null"/>. -or- <paramref name="compilation"/> is <see langword="null"/>
		/// </exception>
		public NamespaceData(BaseNamespaceDeclarationSyntax declaration, ICompilationData compilation) : base(declaration, compilation)
		{
		}

		internal NamespaceData(MemberDeclarationSyntax declaration, ICompilationData compilation) : base(declaration, compilation)
		{
		}

		internal NamespaceData(ISymbol symbol, ICompilationData compilation) : base(symbol, compilation)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="PropertyData"/> class.
		/// </summary>
		/// <param name="declaration"><see cref="BaseNamespaceDeclarationSyntax"/> this <see cref="PropertyData"/> represents.</param>
		/// <param name="compilation">Parent <see cref="ICompilationData"/> of this <see cref="PropertyData"/>.</param>
		/// <param name="symbol"><see cref="INamespaceSymbol"/> this <see cref="PropertyData"/> represents.</param>
		/// <param name="semanticModel"><see cref="SemanticModel"/> of the <paramref name="declaration"/>.</param>
		/// <param name="modifiers">A collection of all modifiers applied to the <paramref name="symbol"/>.</param>
		/// <param name="containingNamespaces">A collection of <see cref="INamespaceSymbol"/>s the <paramref name="symbol"/> is contained within.</param>
		/// <param name="attributes">A collection of <see cref="AttributeData"/>s representing the <paramref name="symbol"/> attributes.</param>
		protected internal NamespaceData(
			BaseNamespaceDeclarationSyntax declaration,
			ICompilationData compilation,
			INamespaceSymbol symbol,
			SemanticModel semanticModel,
			string[]? modifiers = null,
			IEnumerable<INamespaceSymbol>? containingNamespaces = null,
			IEnumerable<AttributeData>? attributes = null
		) : base(
			declaration,
			compilation,
			symbol,
			semanticModel,
			modifiers,
			default,
			containingNamespaces,
			attributes
		)
		{
		}

		private protected NamespaceData(
			MemberDeclarationSyntax declaration,
			ICompilationData compilation,
			INamespaceOrTypeSymbol symbol,
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

		/// <inheritdoc/>
		public override TypeContainer GetContainingTypes()
		{
			return SymbolContainerFactory.Empty<TypeContainer>();
		}
	}
}
