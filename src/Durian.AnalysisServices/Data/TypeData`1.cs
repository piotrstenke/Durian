// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using Durian.Analysis.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.Analysis.Data
{
	/// <inheritdoc cref="ITypeData"/>
	/// <typeparam name="TDeclaration">Specific type of the target <see cref="TypeDeclarationSyntax"/>.</typeparam>
	public abstract class TypeData<TDeclaration> : MemberData, ITypeData where TDeclaration : BaseTypeDeclarationSyntax
	{
		internal SyntaxToken[]? _modifiers;

		internal TDeclaration[]? _partialDeclarations;

		/// <summary>
		/// Target <see cref="BaseTypeDeclarationSyntax"/>.
		/// </summary>
		public new TDeclaration Declaration => (base.Declaration as TDeclaration)!;

		BaseTypeDeclarationSyntax ITypeData.Declaration => Declaration;

		/// <inheritdoc/>
		public SyntaxToken[] Modifiers => _modifiers ??= GetPartialDeclarations().GetModifiers().ToArray();

		/// <summary>
		/// <see cref="ITypeSymbol"/> associated with the <see cref="Declaration"/>.
		/// </summary>
		public new ITypeSymbol Symbol => (base.Symbol as ITypeSymbol)!;

		INamedTypeSymbol ITypeData.Symbol => (Symbol as INamedTypeSymbol)!;

		internal TypeData(ITypeSymbol symbol, ICompilationData compilation) : base(symbol, compilation)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="TypeData{TDeclaration}"/> class.
		/// </summary>
		/// <param name="declaration"><see cref="BaseTypeDeclarationSyntax"/> this <see cref="TypeData{TDeclaration}"/> represents.</param>
		/// <param name="compilation">Parent <see cref="ICompilationData"/> of this <see cref="TypeData{TDeclaration}"/>.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="declaration"/> is <see langword="null"/>. -or- <paramref name="compilation"/> is <see langword="null"/>
		/// </exception>
		protected TypeData(TDeclaration declaration, ICompilationData compilation) : base(declaration, compilation)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="TypeData{TDeclaration}"/> class.
		/// </summary>
		/// <param name="declaration"><see cref="BaseTypeDeclarationSyntax"/> this <see cref="TypeData{TDeclaration}"/> represents.</param>
		/// <param name="compilation">Parent <see cref="ICompilationData"/> of this <see cref="TypeData{TDeclaration}"/>.</param>
		/// <param name="symbol"><see cref="ITypeSymbol"/> this <see cref="TypeData{TDeclaration}"/> represents.</param>
		/// <param name="semanticModel"><see cref="SemanticModel"/> of the <paramref name="declaration"/>.</param>
		/// <param name="partialDeclarations">A collection of <see cref="BaseTypeDeclarationSyntax"/> that represent the partial declarations of the target <paramref name="symbol"/>.</param>
		/// <param name="modifiers">A collection of all modifiers applied to the <paramref name="symbol"/>.</param>
		/// <param name="containingTypes">A collection of <see cref="ITypeData"/>s the <paramref name="symbol"/> is contained within.</param>
		/// <param name="containingNamespaces">A collection of <see cref="INamespaceSymbol"/>s the <paramref name="symbol"/> is contained within.</param>
		/// <param name="attributes">A collection of <see cref="AttributeData"/>s representing the <paramref name="symbol"/> attributes.</param>
		protected TypeData(
			TDeclaration declaration,
			ICompilationData compilation,
			ITypeSymbol symbol,
			SemanticModel semanticModel,
			IEnumerable<TDeclaration>? partialDeclarations = null,
			IEnumerable<SyntaxToken>? modifiers = null,
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
			_partialDeclarations = partialDeclarations?.OfType<TDeclaration>().ToArray();

			if (modifiers is not null)
			{
				_modifiers = modifiers.ToArray();
			}
		}

		/// <summary>
		/// <see cref="INamedTypeSymbol"/> associated with the <see cref="Declaration"/>.
		/// </summary>
		public IEnumerable<ITypeData> GetContainingTypes(bool includeSelf)
		{
			foreach (ITypeData parent in GetContainingTypes())
			{
				yield return parent;
			}

			if (includeSelf)
			{
				yield return this;
			}
		}

		/// <summary>
		/// If the type is partial, returns all declarations of the type (including <see cref="Declaration"/>), otherwise returns only <see cref="Declaration"/>.
		/// </summary>
		public virtual IEnumerable<TDeclaration> GetPartialDeclarations()
		{
			if (_partialDeclarations is null)
			{
				if (Symbol.TypeKind == TypeKind.Enum)
				{
					_partialDeclarations = Array.Empty<TDeclaration>();
				}
				else
				{
					_partialDeclarations = Symbol.DeclaringSyntaxReferences.Select(e => e.GetSyntax()).OfType<TDeclaration>().ToArray();
				}
			}

			return _partialDeclarations;
		}

		IEnumerable<TypeDeclarationSyntax> ITypeData.GetPartialDeclarations()
		{
			return GetPartialDeclarations().Cast<TypeDeclarationSyntax>();
		}
	}
}
