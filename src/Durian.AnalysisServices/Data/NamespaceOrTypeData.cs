// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Durian.Analysis.SymbolContainers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.Analysis.Data
{
	/// <summary>
	/// Encapsulates data associated with a single <see cref="BaseNamespaceDeclarationSyntax"/> or <see cref="BaseTypeDeclarationSyntax"/>.
	/// </summary>
	public class NamespaceOrTypeData : NamespaceData, ITypeData
	{
		private BaseTypeDeclarationSyntax[]? _partialDeclarations;

		/// <summary>
		/// Returns the <see cref="Declaration"/> as a <see cref="BaseNamespaceDeclarationSyntax"/>.
		/// </summary>
		public BaseNamespaceDeclarationSyntax? AsNamespace => (BaseDeclaration as BaseNamespaceDeclarationSyntax)!;

		/// <summary>
		/// Returns the <see cref="Declaration"/> as a <see cref="BaseTypeDeclarationSyntax"/>.
		/// </summary>
		public BaseTypeDeclarationSyntax? AsType => (BaseDeclaration as BaseTypeDeclarationSyntax)!;

		/// <summary>
		/// Target <see cref="MemberDeclarationSyntax"/>.
		/// </summary>
		public new MemberDeclarationSyntax Declaration => (BaseDeclaration as MemberDeclarationSyntax)!;

		/// <summary>
		/// <see cref="INamespaceOrTypeSymbol"/> associated with the <see cref="Declaration"/>.
		/// </summary>
		public new INamespaceOrTypeSymbol Symbol => (BaseSymbol as INamespaceOrTypeSymbol)!;

		BaseTypeDeclarationSyntax ITypeData.Declaration
		{
			get
			{
				if(!Symbol.IsType)
				{
					throw new InvalidOperationException("Current NamespaceOrTypeData does not represent a type");
				}

				return AsType!;
			}
		}

		ITypeSymbol ITypeData.Symbol
		{
			get
			{
				if(!Symbol.IsType)
				{
					throw new InvalidOperationException("Current NamespaceOrTypeData does not represent a type");
				}

				return (Symbol as INamedTypeSymbol)!;
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="NamespaceOrTypeData"/> class.
		/// </summary>
		/// <param name="declaration"><see cref="BaseNamespaceDeclarationSyntax"/> this <see cref="NamespaceOrTypeData"/> represents.</param>
		/// <param name="compilation">Parent <see cref="ICompilationData"/> of this <see cref="NamespaceOrTypeData"/>.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="declaration"/> is <see langword="null"/>. -or- <paramref name="compilation"/> is <see langword="null"/>
		/// </exception>
		public NamespaceOrTypeData(BaseNamespaceDeclarationSyntax declaration, ICompilationData compilation) : base(declaration, compilation)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="NamespaceOrTypeData"/> class.
		/// </summary>
		/// <param name="declaration"><see cref="BaseTypeDeclarationSyntax"/> this <see cref="NamespaceOrTypeData"/> represents.</param>
		/// <param name="compilation">Parent <see cref="ICompilationData"/> of this <see cref="NamespaceOrTypeData"/>.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="declaration"/> is <see langword="null"/>. -or- <paramref name="compilation"/> is <see langword="null"/>
		/// </exception>
		public NamespaceOrTypeData(BaseTypeDeclarationSyntax declaration, ICompilationData compilation) : base(declaration, compilation)
		{
		}

		internal NamespaceOrTypeData(INamespaceOrTypeSymbol symbol, ICompilationData compilation) : base(symbol, compilation)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="NamespaceOrTypeData"/> class.
		/// </summary>
		/// <param name="declaration"><see cref="BaseNamespaceDeclarationSyntax"/> this <see cref="NamespaceOrTypeData"/> represents.</param>
		/// <param name="compilation">Parent <see cref="ICompilationData"/> of this <see cref="NamespaceOrTypeData"/>.</param>
		/// <param name="symbol"><see cref="INamespaceSymbol"/> this <see cref="NamespaceOrTypeData"/> represents.</param>
		/// <param name="semanticModel"><see cref="SemanticModel"/> of the <paramref name="declaration"/>.</param>
		/// <param name="modifiers">A collection of all modifiers applied to the <paramref name="symbol"/>.</param>
		/// <param name="containingNamespaces">A collection of <see cref="INamespaceSymbol"/>s the <paramref name="symbol"/> is contained within.</param>
		/// <param name="attributes">A collection of <see cref="AttributeData"/>s representing the <paramref name="symbol"/> attributes.</param>
		protected internal NamespaceOrTypeData(
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
			containingNamespaces,
			attributes
		)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="NamespaceOrTypeData"/> class.
		/// </summary>
		/// <param name="declaration"><see cref="BaseTypeDeclarationSyntax"/> this <see cref="NamespaceOrTypeData"/> represents.</param>
		/// <param name="compilation">Parent <see cref="ICompilationData"/> of this <see cref="NamespaceOrTypeData"/>.</param>
		/// <param name="symbol"><see cref="ITypeSymbol"/> this <see cref="NamespaceOrTypeData"/> represents.</param>
		/// <param name="semanticModel"><see cref="SemanticModel"/> of the <paramref name="declaration"/>.</param>
		/// <param name="modifiers">A collection of all modifiers applied to the <paramref name="symbol"/>.</param>
		/// <param name="containingTypes">A collection of <see cref="ITypeData"/>s the <paramref name="symbol"/> is contained within.</param>
		/// <param name="containingNamespaces">A collection of <see cref="INamespaceSymbol"/>s the <paramref name="symbol"/> is contained within.</param>
		/// <param name="attributes">A collection of <see cref="AttributeData"/>s representing the <paramref name="symbol"/> attributes.</param>
		protected internal NamespaceOrTypeData(
			BaseTypeDeclarationSyntax declaration,
			ICompilationData compilation,
			ITypeSymbol symbol,
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
		public ImmutableArray<BaseTypeDeclarationSyntax> GetPartialDeclarations()
		{
			if(Symbol.IsNamespace)
			{
				return ImmutableArray<BaseTypeDeclarationSyntax>.Empty;
			}

			if (_partialDeclarations is null)
			{
				if (Symbol is ITypeSymbol t && t.TypeKind == TypeKind.Enum)
				{
					_partialDeclarations = Array.Empty<BaseTypeDeclarationSyntax>();
				}
				else
				{
					_partialDeclarations = Symbol.DeclaringSyntaxReferences.Select(e => e.GetSyntax()).OfType<BaseTypeDeclarationSyntax>().ToArray();
				}
			}

			return ImmutableArray.Create(_partialDeclarations);
		}
	}
}
