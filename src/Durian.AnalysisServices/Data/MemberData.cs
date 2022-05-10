// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using Durian.Analysis.Extensions;
using Durian.Analysis.SymbolContainers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.Analysis.Data
{
	/// <inheritdoc cref="IMemberData"/>
	[DebuggerDisplay("{Symbol}")]
	public class MemberData : IMemberData
	{
		private ImmutableArray<AttributeData> _attributes;
		private NamespaceContainer? _containingNamespaces;
		private TypeContainer? _containingTypes;
		private string[]? _modifiers;

		private Location? _location;

		/// <inheritdoc/>
		public CSharpSyntaxNode Declaration { get; }

		/// <inheritdoc/>
		public Location Location => _location ??= Declaration.GetLocation();

		/// <inheritdoc/>
		public string Name { get; }

		/// <inheritdoc/>
		public ICompilationData ParentCompilation { get; }

		/// <inheritdoc/>
		public SemanticModel SemanticModel { get; }

		/// <inheritdoc/>
		public ISymbol Symbol { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="MemberData"/> class.
		/// </summary>
		/// <param name="declaration"><see cref="CSharpSyntaxNode"/> this <see cref="MemberData"/> represents.</param>
		/// <param name="compilation">Parent <see cref="ICompilationData"/> of this <see cref="MemberData"/>.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="declaration"/> is <see langword="null"/>. -or- <paramref name="compilation"/> is <see langword="null"/>.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Specified <paramref name="declaration"/> doesn't represent any symbols.
		/// </exception>
		public MemberData(CSharpSyntaxNode declaration, ICompilationData compilation)
		{
			(SemanticModel, Symbol) = AnalysisUtilities.GetSymbolAndSemanticModel(declaration, compilation);
			Declaration = declaration;
			ParentCompilation = compilation;
			Name = Symbol.GetVerbatimName();
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
			Name = Symbol.GetVerbatimName();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MemberData"/> class.
		/// </summary>
		/// <param name="declaration"><see cref="CSharpSyntaxNode"/> this <see cref="MemberData"/> represents.</param>
		/// <param name="compilation">Parent <see cref="ICompilationData"/> of this <see cref="MemberData"/>.</param>
		/// <param name="symbol"><see cref="ISymbol"/> this <see cref="MemberData"/> represents.</param>
		/// <param name="semanticModel"><see cref="SemanticModel"/> of the <paramref name="declaration"/>.</param>
		/// <param name="containingTypes">A collection of <see cref="ITypeData"/>s the <paramref name="symbol"/> is contained within.</param>
		/// <param name="containingNamespaces">A collection of <see cref="INamespaceSymbol"/>s the <paramref name="symbol"/> is contained within.</param>
		/// <param name="attributes">A collection of <see cref="AttributeData"/>s representing the <paramref name="symbol"/> attributes.</param>
		protected internal MemberData(
			CSharpSyntaxNode declaration,
			ICompilationData compilation,
			ISymbol symbol,
			SemanticModel semanticModel,
			IEnumerable<ITypeData>? containingTypes = null,
			IEnumerable<INamespaceSymbol>? containingNamespaces = null,
			IEnumerable<AttributeData>? attributes = null
		)
		{
			Declaration = declaration;
			ParentCompilation = compilation;
			Symbol = symbol;
			SemanticModel = semanticModel;

			if(containingTypes is not null)
			{
				_containingTypes = SymbolContainer.Types(containingTypes, true, ReturnOrder.Root);
			}

			if(containingNamespaces is not null)
			{
				_containingNamespaces = SymbolContainer.Namespaces(containingNamespaces, ReturnOrder.Root);
			}

			_containingNamespaces = containingNamespaces?.ToArray();

			if (attributes is not null)
			{
				_attributes = attributes.ToImmutableArray();
			}

			Name = Symbol.GetVerbatimName();
		}

		/// <inheritdoc/>
		public virtual ImmutableArray<AttributeData> GetAttributes()
		{
			return _attributes.IsDefault ? (_attributes = Symbol.GetAttributes()) : _attributes;
		}

		/// <inheritdoc/>
		public virtual NamespaceContainer GetContainingNamespaces()
		{

			return _containingNamespaces ??= Symbol.GetContainingNamespaces().ToArray();
		}

		/// <inheritdoc/>
		public virtual TypeContainer GetContainingTypes()
		{
			return _containingTypes ??= Symbol.GetContainingTypesAsData(ParentCompilation).ToArray();
		}

		/// <inheritdoc/>
		public virtual string[] GetModifiers()
		{
			return _modifiers ??= Symbol.GetModifiers();
		}

		private protected static InvalidOperationException Exc_NoSyntaxReference(ISymbol symbol)
		{
			return new InvalidOperationException($"Symbol '{symbol}' doesn't define any syntax reference, thus can't be used in a {nameof(MemberData)}!");
		}
	}
}
