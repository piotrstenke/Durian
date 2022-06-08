﻿// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.Analysis.Data
{
	/// <summary>
	/// Encapsulates data associated with a single <see cref="AnonymousFunctionExpressionSyntax"/>.
	/// </summary>
	public class LambdaData : MethodData<AnonymousFunctionExpressionSyntax>
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="LambdaData"/> class.
		/// </summary>
		/// <param name="declaration"><see cref="AnonymousFunctionExpressionSyntax"/> this <see cref="LambdaData"/> represents.</param>
		/// <param name="compilation">Parent <see cref="ICompilationData"/> of this <see cref="LambdaData"/>.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="declaration"/> is <see langword="null"/>. -or- <paramref name="compilation"/> is <see langword="null"/>
		/// </exception>
		public LambdaData(AnonymousFunctionExpressionSyntax declaration, ICompilationData compilation) : base(declaration, compilation)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="LambdaData"/> class.
		/// </summary>
		/// <param name="declaration"><see cref="AnonymousFunctionExpressionSyntax"/> this <see cref="LambdaData"/> represents.</param>
		/// <param name="compilation">Parent <see cref="ICompilationData"/> of this <see cref="LambdaData"/>.</param>
		/// <param name="symbol"><see cref="IMethodSymbol"/> this <see cref="LambdaData"/> represents.</param>
		/// <param name="semanticModel"><see cref="SemanticModel"/> of the <paramref name="declaration"/>.</param>
		/// <param name="modifiers">A collection of all modifiers applied to the <paramref name="symbol"/>.</param>
		/// <param name="containingTypes">A collection of <see cref="ITypeData"/>s the <paramref name="symbol"/> is contained within.</param>
		/// <param name="containingNamespaces">A collection of <see cref="INamespaceSymbol"/>s the <paramref name="symbol"/> is contained within.</param>
		/// <param name="attributes">A collection of <see cref="AttributeData"/>s representing the <paramref name="symbol"/> attributes.</param>
		protected internal LambdaData(
			AnonymousFunctionExpressionSyntax declaration,
			ICompilationData compilation,
			IMethodSymbol symbol,
			SemanticModel semanticModel,
			string[]? modifiers = null,
			IEnumerable<ITypeData>? containingTypes = null,
			IEnumerable<INamespaceSymbol>? containingNamespaces = null,
			IEnumerable<AttributeData>? attributes = null
		) : base(
			declaration,
			compilation,
			symbol,
			semanticModel,
			modifiers,
			containingTypes,
			containingNamespaces,
			attributes
		)
		{
		}

		internal LambdaData(IMethodSymbol symbol, ICompilationData compilation) : base(symbol, compilation)
		{
		}
	}
}
