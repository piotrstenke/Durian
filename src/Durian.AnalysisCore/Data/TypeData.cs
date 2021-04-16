using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Durian.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.Data
{
	/// <inheritdoc cref="ITypeData"/>
	public class TypeData : MemberData, ITypeData
	{
		private SyntaxToken[]? _modifiers;
		private TypeDeclarationSyntax[]? _partialDeclarations;

		/// <summary>
		/// Target <see cref="TypeDeclarationSyntax"/>.
		/// </summary>
		public new TypeDeclarationSyntax Declaration => (base.Declaration as TypeDeclarationSyntax)!;

		/// <summary>
		/// <see cref="INamedTypeSymbol"/> associated with the <see cref="Declaration"/>.
		/// </summary>
		public new INamedTypeSymbol Symbol => (base.Symbol as INamedTypeSymbol)!;

		/// <inheritdoc/>
		public SyntaxToken[] Modifiers => _modifiers ??= GetPartialDeclarations().GetModifiers().ToArray();

		/// <summary>
		/// Initializes a new instance of the <see cref="TypeData"/> class.
		/// </summary>
		/// <param name="declaration">Target <see cref="TypeDeclarationSyntax"/>.</param>
		/// <param name="compilation">Current <see cref="ICompilationData"/>.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="declaration"/> was <c>null</c>. -or- <paramref name="compilation"/> was <c>null</c>
		/// </exception>
		public TypeData(TypeDeclarationSyntax declaration, ICompilationData compilation) : base(declaration, compilation)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="TypeData"/> class.
		/// </summary>
		/// <param name="declaration"></param>
		/// <param name="compilation"></param>
		/// <param name="symbol"></param>
		/// <param name="semanticModel"></param>
		/// <param name="partialDeclarations"></param>
		/// <param name="modifiers"></param>
		/// <param name="containingTypes"></param>
		/// <param name="containingNamespaces"></param>
		/// <param name="attributes"></param>
		protected internal TypeData(
			TypeDeclarationSyntax declaration,
			ICompilationData compilation,
			INamedTypeSymbol symbol,
			SemanticModel semanticModel,
			IEnumerable<TypeDeclarationSyntax>? partialDeclarations,
			IEnumerable<SyntaxToken>? modifiers,
			IEnumerable<ITypeData>? containingTypes,
			IEnumerable<INamespaceSymbol>? containingNamespaces,
			IEnumerable<AttributeData>? attributes
		) : base(declaration, compilation, symbol, semanticModel, containingTypes, containingNamespaces, attributes)
		{
			_partialDeclarations = partialDeclarations?.ToArray();

			if (modifiers is not null)
			{
				_modifiers = modifiers.ToArray();
			}
		}

		internal TypeData(INamedTypeSymbol symbol, ICompilationData compilation) : base(symbol, compilation)
		{
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
		public virtual IEnumerable<TypeDeclarationSyntax> GetPartialDeclarations()
		{
			return _partialDeclarations ??= Symbol.GetPartialDeclarations<TypeDeclarationSyntax>().ToArray();
		}
	}
}
