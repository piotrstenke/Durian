using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.Data
{
	/// <inheritdoc cref="ITypeData"/>
	/// <typeparam name="TDeclaration">Specific type of the target <see cref="TypeDeclarationSyntax"/>.</typeparam>
	public class TypeData<TDeclaration> : TypeData where TDeclaration : TypeDeclarationSyntax
	{
		/// <summary>
		/// Target <see cref="TypeDeclarationSyntax"/>.
		/// </summary>
		public new TDeclaration Declaration => (base.Declaration as TDeclaration)!;

		/// <summary>
		/// <see cref="INamedTypeSymbol"/> associated with the <see cref="Declaration"/>.
		/// </summary>
		public new INamedTypeSymbol Symbol => base.Symbol!;

		/// <summary>
		/// Initializes a new instance of the <see cref="TypeData{TDeclaration}"/> class.
		/// </summary>
		/// <param name="declaration"><see cref="TypeDeclarationSyntax"/> this <see cref="TypeData"/> represents.</param>
		/// <param name="compilation">Parent <see cref="ICompilationData"/> of this <see cref="TypeData"/>.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="declaration"/> is <see langword="null"/>. -or- <paramref name="compilation"/> is <see langword="null"/>
		/// </exception>
		public TypeData(TDeclaration declaration, ICompilationData compilation) : base(declaration, compilation)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="TypeData{TDeclaration}"/> class.
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
			TDeclaration declaration,
			ICompilationData compilation,
			INamedTypeSymbol symbol,
			SemanticModel semanticModel,
			IEnumerable<TDeclaration>? partialDeclarations,
			IEnumerable<SyntaxToken>? modifiers,
			IEnumerable<ITypeData>? containingTypes,
			IEnumerable<INamespaceSymbol>? containingNamespaces,
			IEnumerable<AttributeData>? attributes
		) : base(declaration, compilation, symbol, semanticModel, partialDeclarations, modifiers, containingTypes, containingNamespaces, attributes)
		{
		}

		internal TypeData(INamedTypeSymbol symbol, ICompilationData compilation) : base(symbol, compilation)
		{
		}

		/// <summary>
		/// If the type is partial, returns all declarations of the type (including <see cref="Declaration"/>), otherwise returns only <see cref="Declaration"/>.
		/// </summary>
		public new IEnumerable<TDeclaration> GetPartialDeclarations()
		{
			return base.GetPartialDeclarations().Cast<TDeclaration>();
		}
	}
}