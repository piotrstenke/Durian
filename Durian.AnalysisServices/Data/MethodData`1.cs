// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.Analysis.Data
{
	/// <summary>
	/// Encapsulates data associated with a single <see cref="BaseMethodDeclarationSyntax"/>.
	/// </summary>
	public abstract class MethodData<TDeclaration> : MemberData where TDeclaration : BaseMethodDeclarationSyntax
	{
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
		/// Target <see cref="BaseMethodDeclarationSyntax"/>.
		/// </summary>
		public new TDeclaration Declaration => (base.Declaration as TDeclaration)!;

		/// <summary>
		/// <see cref="IMethodSymbol"/> associated with the <see cref="Declaration"/>.
		/// </summary>
		public new IMethodSymbol Symbol => (base.Symbol as IMethodSymbol)!;

		/// <summary>
		/// Initializes a new instance of the <see cref="MethodData{TDeclaration}"/> class.
		/// </summary>
		/// <param name="declaration"><see cref="BaseMethodDeclarationSyntax"/> this <see cref="MethodData{TDeclaration}"/> represents.</param>
		/// <param name="compilation">Parent <see cref="ICompilationData"/> of this <see cref="MethodData{TDeclaration}"/>.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="declaration"/> is <see langword="null"/>. -or- <paramref name="compilation"/> is <see langword="null"/>
		/// </exception>
		protected MethodData(TDeclaration declaration, ICompilationData compilation) : base(declaration, compilation)
		{
		}

		private protected MethodData(IMethodSymbol symbol, ICompilationData compilation) : base(symbol, compilation)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MethodData{TDeclaration}"/> class.
		/// </summary>
		/// <param name="declaration"><see cref="BaseMethodDeclarationSyntax"/> this <see cref="MethodData{TDeclaration}"/> represents.</param>
		/// <param name="compilation">Parent <see cref="ICompilationData"/> of this <see cref="MethodData{TDeclaration}"/>.</param>
		/// <param name="symbol"><see cref="IMethodSymbol"/> this <see cref="MethodData{TDeclaration}"/> represents.</param>
		/// <param name="semanticModel"><see cref="SemanticModel"/> of the <paramref name="declaration"/>.</param>
		/// <param name="containingTypes">A collection of <see cref="ITypeData"/>s the <paramref name="symbol"/> is contained within.</param>
		/// <param name="containingNamespaces">A collection of <see cref="INamespaceSymbol"/>s the <paramref name="symbol"/> is contained within.</param>
		/// <param name="attributes">A collection of <see cref="AttributeData"/>s representing the <paramref name="symbol"/> attributes.</param>
		protected MethodData(
			TDeclaration declaration,
			ICompilationData compilation,
			IMethodSymbol symbol,
			SemanticModel semanticModel,
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
		}
	}
}