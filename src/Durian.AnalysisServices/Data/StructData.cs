// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Durian.Analysis.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.Analysis.Data
{
	/// <summary>
	/// Encapsulates data associated with a single <see cref="StructDeclarationSyntax"/>.
	/// </summary>
	public class StructData : TypeData<StructDeclarationSyntax>
	{
		/// <summary>
		/// <see cref="INamedTypeSymbol"/> associated with the <see cref="TypeData{TDeclaration}.Declaration"/>.
		/// </summary>
		public new INamedTypeSymbol Symbol => (base.Symbol as INamedTypeSymbol)!;

		/// <summary>
		/// Initializes a new instance of the <see cref="StructData"/> class.
		/// </summary>
		/// <param name="declaration"><see cref="StructDeclarationSyntax"/> this <see cref="StructData"/> represents.</param>
		/// <param name="compilation">Parent <see cref="ICompilationData"/> of this <see cref="StructData"/>.</param>
		/// <param name="properties"><see cref="TypeData{TDeclaration}.Properties"/> to use for the current instance.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="declaration"/> is <see langword="null"/>. -or- <paramref name="compilation"/> is <see langword="null"/>
		/// </exception>
		public StructData(StructDeclarationSyntax declaration, ICompilationData compilation, Properties? properties = default) : base(declaration, compilation, properties)
		{
		}

		internal StructData(INamedTypeSymbol symbol, ICompilationData compilation) : base(symbol, compilation)
		{
		}
	}
}
