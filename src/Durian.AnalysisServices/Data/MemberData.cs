// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using Durian.Analysis.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.Analysis.Data
{
	/// <inheritdoc cref="IMemberData"/>
	[DebuggerDisplay("{Symbol}")]
	public class MemberData : IMemberData
	{
		internal ImmutableArray<AttributeData> _attributes;

		internal INamespaceSymbol[]? _containingNamespaces;

		// Used is some extension methods.
		internal ITypeData[]? _containingTypes;

		private Location? _location;

		/// <inheritdoc/>
		public MemberDeclarationSyntax Declaration { get; }

		/// <summary>
		/// <see cref="Microsoft.CodeAnalysis.Location"/> of the <see cref="Declaration"/>.
		/// </summary>
		public Location Location => _location ??= Declaration.GetLocation();

		/// <inheritdoc/>
		public ICompilationData ParentCompilation { get; }

		/// <inheritdoc/>
		public SemanticModel SemanticModel { get; }

		/// <inheritdoc/>
		public ISymbol Symbol { get; }

		/// <summary>
		/// Name of the member.
		/// </summary>
		public string Name => Symbol.Name;

		/// <summary>
		/// Initializes a new instance of the <see cref="MemberData"/> class.
		/// </summary>
		/// <param name="declaration"><see cref="MemberDeclarationSyntax"/> this <see cref="MemberData"/> represents.</param>
		/// <param name="compilation">Parent <see cref="ICompilationData"/> of this <see cref="MemberData"/>.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="declaration"/> is <see langword="null"/>. -or- <paramref name="compilation"/> is <see langword="null"/>.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Specified <paramref name="declaration"/> doesn't represent any symbols.
		/// </exception>
		public MemberData(MemberDeclarationSyntax declaration, ICompilationData compilation)
		{
			(SemanticModel, Symbol) = AnalysisUtilities.GetSymbolAndSemanticModel(declaration, compilation);
			Declaration = declaration;
			ParentCompilation = compilation;
		}

		internal MemberData(ISymbol symbol, ICompilationData compilation)
		{
			if (symbol.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax() is not MemberDeclarationSyntax decl)
			{
				throw Exc_NoSyntaxReference(symbol);
			}

			Symbol = symbol;
			Declaration = decl;
			SemanticModel = compilation.Compilation.GetSemanticModel(decl.SyntaxTree);
			ParentCompilation = compilation;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MemberData"/> class.
		/// </summary>
		/// <param name="declaration"><see cref="MemberDeclarationSyntax"/> this <see cref="MemberData"/> represents.</param>
		/// <param name="compilation">Parent <see cref="ICompilationData"/> of this <see cref="MemberData"/>.</param>
		/// <param name="symbol"><see cref="ISymbol"/> this <see cref="MemberData"/> represents.</param>
		/// <param name="semanticModel"><see cref="SemanticModel"/> of the <paramref name="declaration"/>.</param>
		/// <param name="containingTypes">A collection of <see cref="ITypeData"/>s the <paramref name="symbol"/> is contained within.</param>
		/// <param name="containingNamespaces">A collection of <see cref="INamespaceSymbol"/>s the <paramref name="symbol"/> is contained within.</param>
		/// <param name="attributes">A collection of <see cref="AttributeData"/>s representing the <paramref name="symbol"/> attributes.</param>
		protected internal MemberData(
			MemberDeclarationSyntax declaration,
			ICompilationData compilation,
			ISymbol symbol,
			SemanticModel semanticModel,
			IEnumerable<ITypeData>? containingTypes,
			IEnumerable<INamespaceSymbol>? containingNamespaces,
			IEnumerable<AttributeData>? attributes
		)
		{
			Declaration = declaration;
			ParentCompilation = compilation;
			Symbol = symbol;
			SemanticModel = semanticModel;
			_containingTypes = containingTypes?.ToArray();
			_containingNamespaces = containingNamespaces?.ToArray();

			if (attributes is not null)
			{
				_attributes = attributes.ToImmutableArray();
			}
		}

		/// <inheritdoc/>
		public virtual ImmutableArray<AttributeData> GetAttributes()
		{
			return _attributes.IsDefault ? (_attributes = Symbol.GetAttributes()) : _attributes;
		}

		/// <inheritdoc/>
		public virtual IEnumerable<INamespaceSymbol> GetContainingNamespaces()
		{
			return _containingNamespaces ??= Symbol.GetContainingNamespaces().ToArray();
		}

		/// <inheritdoc/>
		public virtual IEnumerable<ITypeData> GetContainingTypes()
		{
			return _containingTypes ??= Symbol.GetContainingTypes(ParentCompilation).ToArray();
		}

		private protected static InvalidOperationException Exc_NoSyntaxReference(ISymbol symbol)
		{
			return new InvalidOperationException($"Symbol '{symbol}' doesn't define any syntax reference, thus can't be used in a {nameof(MemberData)}!");
		}
	}
}
