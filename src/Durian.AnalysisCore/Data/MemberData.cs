using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using Durian.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.Data
{
	/// <inheritdoc cref="IMemberData"/>
	[DebuggerDisplay("{Symbol}: {_location}")]
	public class MemberData : IMemberData
	{
		// Used is some extension methods.
		internal ITypeData[]? _containingTypes;
		internal INamespaceSymbol[]? _containingNamespaces;
		internal ImmutableArray<AttributeData> _attributes;

		private Location? _location;

		/// <inheritdoc/>
		public SemanticModel SemanticModel { get; }

		/// <inheritdoc/>
		public MemberDeclarationSyntax Declaration { get; }

		/// <summary>
		/// <see cref="Microsoft.CodeAnalysis.Location"/> of the <see cref="Declaration"/>.
		/// </summary>
		public Location Location => _location ??= Declaration.GetLocation();

		/// <inheritdoc/>
		public ISymbol Symbol { get; }

		/// <inheritdoc/>
		public ICompilationData ParentCompilation { get; }

		/// <summary>
		/// Name of the member.
		/// </summary>
		public string Name => Symbol.Name;

		/// <summary>
		/// Initializes a new instance of the <see cref="MemberData"/> class.
		/// </summary>
		/// <param name="declaration">Target <see cref="MemberDeclarationSyntax"/>.</param>
		/// <param name="compilation">Current <see cref="ICompilationData"/>.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="declaration"/> is <c>null</c>. -or- <paramref name="compilation"/> is <c>null</c>.
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

		/// <summary>
		/// Initializes a new instance of the <see cref="MemberData"/> class.
		/// </summary>
		/// <param name="declaration"></param>
		/// <param name="compilation"></param>
		/// <param name="symbol"></param>
		/// <param name="semanticModel"></param>
		/// <param name="containingTypes"></param>
		/// <param name="containingNamespaces"></param>
		/// <param name="attributes"></param>
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

		/// <inheritdoc/>
		public virtual IEnumerable<ITypeData> GetContainingTypes()
		{
			return _containingTypes ??= Symbol.GetContainingTypes(ParentCompilation).ToArray();
		}

		/// <inheritdoc/>
		public virtual IEnumerable<INamespaceSymbol> GetContainingNamespaces()
		{
			return _containingNamespaces ??= Symbol.GetContainingNamespaces().ToArray();
		}

		/// <inheritdoc/>
		public virtual bool Validate(in GeneratorExecutionContext context)
		{
			return true;
		}

		/// <inheritdoc/>
		public virtual ImmutableArray<AttributeData> GetAttributes()
		{
			return _attributes.IsDefault ? (_attributes = Symbol.GetAttributes()) : _attributes;
		}

		private protected static InvalidOperationException Exc_NoSyntaxReference(ISymbol symbol)
		{
			return new InvalidOperationException($"Symbol '{symbol}' doesn't define any syntax reference, thus can't be used in a {nameof(MemberData)}!");
		}
	}
}
