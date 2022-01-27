// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.Analysis.Data
{
	/// <summary>
	/// Encapsulates data associated with a single <see cref="MethodDeclarationSyntax"/>.
	/// </summary>
	public class MethodData : MemberData
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
		/// Target <see cref="MethodDeclarationSyntax"/>.
		/// </summary>
		public new MethodDeclarationSyntax Declaration => (base.Declaration as MethodDeclarationSyntax)!;

		/// <summary>
		/// <see cref="IMethodSymbol"/> associated with the <see cref="Declaration"/>.
		/// </summary>
		public new IMethodSymbol Symbol => (base.Symbol as IMethodSymbol)!;

		/// <summary>
		/// Initializes a new instance of the <see cref="MethodData"/> class.
		/// </summary>
		/// <param name="declaration"><see cref="MethodDeclarationSyntax"/> this <see cref="MethodData"/> represents.</param>
		/// <param name="compilation">Parent <see cref="ICompilationData"/> of this <see cref="MethodData"/>.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="declaration"/> is <see langword="null"/>. -or- <paramref name="compilation"/> is <see langword="null"/>
		/// </exception>
		public MethodData(MethodDeclarationSyntax declaration, ICompilationData compilation) : base(declaration, compilation)
		{
		}

		internal MethodData(IMethodSymbol symbol, ICompilationData compilation) : base(symbol, compilation)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MethodData"/> class.
		/// </summary>
		/// <param name="declaration"><see cref="MethodDeclarationSyntax"/> this <see cref="MethodData"/> represents.</param>
		/// <param name="compilation">Parent <see cref="ICompilationData"/> of this <see cref="MethodData"/>.</param>
		/// <param name="symbol"><see cref="IMethodSymbol"/> this <see cref="MethodData"/> represents.</param>
		/// <param name="semanticModel"><see cref="SemanticModel"/> of the <paramref name="declaration"/>.</param>
		/// <param name="containingTypes">A collection of <see cref="ITypeData"/>s the <paramref name="symbol"/> is contained within.</param>
		/// <param name="containingNamespaces">A collection of <see cref="INamespaceSymbol"/>s the <paramref name="symbol"/> is contained within.</param>
		/// <param name="attributes">A collection of <see cref="AttributeData"/>s representing the <paramref name="symbol"/> attributes.</param>
		protected internal MethodData(
			MethodDeclarationSyntax declaration,
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