using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.Data
{
	/// <summary>
	/// Encapsulates data associated with a single <see cref="MethodDeclarationSyntax"/>.
	/// </summary>
	public class MethodData : MemberData
	{
		/// <summary>
		/// Target <see cref="MethodDeclarationSyntax"/>.
		/// </summary>
		public new MethodDeclarationSyntax Declaration => (base.Declaration as MethodDeclarationSyntax)!;

		/// <summary>
		/// <see cref="IMethodSymbol"/> associated with the <see cref="Declaration"/>.
		/// </summary>
		public new IMethodSymbol Symbol => (base.Symbol as IMethodSymbol)!;

		/// <summary>
		/// Body of the method.
		/// </summary>
		public SyntaxNode? Body
		{
			get
			{
				if (Declaration.ExpressionBody is not null)
				{
					return Declaration.ExpressionBody;
				}

				if (Declaration.Body is not null)
				{
					return Declaration.Body;
				}

				return null;
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MethodData"/> class.
		/// </summary>
		/// <param name="declaration">Target <see cref="MethodDeclarationSyntax"/>.</param>
		/// <param name="compilation">Current <see cref="ICompilationData"/>.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="declaration"/> is <c>null</c>. -or- <paramref name="compilation"/> is <c>null</c>
		/// </exception>
		public MethodData(MethodDeclarationSyntax declaration, ICompilationData compilation) : base(declaration, compilation)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MethodData"/> class.
		/// </summary>
		/// <param name="declaration"></param>
		/// <param name="compilation"></param>
		/// <param name="symbol"></param>
		/// <param name="semanticModel"></param>
		/// <param name="containingTypes"></param>
		/// <param name="containingNamespaces"></param>
		/// <param name="attributes"></param>
		protected internal MethodData(
			MethodDeclarationSyntax declaration,
			ICompilationData compilation,
			IMethodSymbol symbol,
			SemanticModel semanticModel,
			IEnumerable<ITypeData>? containingTypes,
			IEnumerable<INamespaceSymbol>? containingNamespaces,
			IEnumerable<AttributeData>? attributes
		) : base(declaration, compilation, symbol, semanticModel, containingTypes, containingNamespaces, attributes)
		{
		}

		internal MethodData(IMethodSymbol symbol, ICompilationData compilation) : base(symbol, compilation)
		{
		}
	}
}
