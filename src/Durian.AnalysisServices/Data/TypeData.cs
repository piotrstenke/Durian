using System;
using System.Collections.Generic;
using System.Linq;
using Durian.Generator.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.Generator.Data
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
		/// <param name="declaration"><see cref="TypeDeclarationSyntax"/> this <see cref="TypeData"/> represents.</param>
		/// <param name="compilation">Parent <see cref="ICompilationData"/> of this <see cref="TypeData"/>.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="declaration"/> is <see langword="null"/>. -or- <paramref name="compilation"/> is <see langword="null"/>
		/// </exception>
		public TypeData(TypeDeclarationSyntax declaration, ICompilationData compilation) : base(declaration, compilation)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="TypeData"/> class.
		/// </summary>
		/// <param name="declaration"><see cref="TypeDeclarationSyntax"/> this <see cref="TypeData"/> represents.</param>
		/// <param name="compilation">Parent <see cref="ICompilationData"/> of this <see cref="TypeData"/>.</param>
		/// <param name="symbol"><see cref="INamedTypeSymbol"/> this <see cref="TypeData"/> represents.</param>
		/// <param name="semanticModel"><see cref="SemanticModel"/> of the <paramref name="declaration"/>.</param>
		/// <param name="partialDeclarations">A collection of <see cref="TypeDeclarationSyntax"/> that represent the partial declarations of the target <paramref name="symbol"/>.</param>
		/// <param name="modifiers">A collection of all modifiers applied to the <paramref name="symbol"/>.</param>
		/// <param name="containingTypes">A collection of <see cref="ITypeData"/>s the <paramref name="symbol"/> is contained within.</param>
		/// <param name="containingNamespaces">A collection of <see cref="INamespaceSymbol"/>s the <paramref name="symbol"/> is contained within.</param>
		/// <param name="attributes">A collection of <see cref="AttributeData"/>s representing the <paramref name="symbol"/> attributes.</param>
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
