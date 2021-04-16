using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.Data
{
	/// <summary>
	/// Encapsulates data associated with a single <see cref="DelegateDeclarationSyntax"/>.
	/// </summary>
	public class DelegateData : MemberData
	{
		/// <summary>
		/// <see cref="INamedTypeSymbol"/> associated with the <see cref="Declaration"/>.
		/// </summary>
		public new INamedTypeSymbol Symbol => (base.Symbol as INamedTypeSymbol)!;

		/// <summary>
		/// Target <see cref="DelegateDeclarationSyntax"/>.
		/// </summary>
		public new DelegateDeclarationSyntax Declaration => (base.Declaration as DelegateDeclarationSyntax)!;

		/// <summary>
		/// Initializes a new instance of the <see cref="DelegateData"/> class.
		/// </summary>
		/// <param name="declaration">Target <see cref="DelegateDeclarationSyntax"/>.</param>
		/// <param name="compilation">Current <see cref="ICompilationData"/>.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="declaration"/> was <c>null</c>. -or- <paramref name="compilation"/> was <c>null</c>
		/// </exception>
		public DelegateData(DelegateDeclarationSyntax declaration, ICompilationData compilation) : base(declaration, compilation)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DelegateData"/> class.
		/// </summary>
		/// <param name="declaration"></param>
		/// <param name="compilation"></param>
		/// <param name="symbol"></param>
		/// <param name="semanticModel"></param>
		/// <param name="containingTypes"></param>
		/// <param name="containingNamespaces"></param>
		/// <param name="attributes"></param>
		protected internal DelegateData(
			DelegateDeclarationSyntax declaration,
			ICompilationData compilation,
			INamedTypeSymbol symbol,
			SemanticModel semanticModel,
			IEnumerable<ITypeData>? containingTypes,
			IEnumerable<INamespaceSymbol>? containingNamespaces,
			IEnumerable<AttributeData>? attributes
		) : base(declaration, compilation, symbol, semanticModel, containingTypes, containingNamespaces, attributes)
		{
		}

		internal DelegateData(INamedTypeSymbol symbol, ICompilationData compilation) : base(symbol, compilation)
		{
		}
	}
}
