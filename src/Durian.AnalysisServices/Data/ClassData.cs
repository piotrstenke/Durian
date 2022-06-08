// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.Analysis.Data
{
	/// <summary>
	/// Encapsulates data associated with a single <see cref="ClassDeclarationSyntax"/>.
	/// </summary>
	public class ClassData : TypeData<ClassDeclarationSyntax>
	{
		/// <summary>
		/// <see cref="INamedTypeSymbol"/> associated with the <see cref="TypeData{TDeclaration}.Declaration"/>.
		/// </summary>
		public new INamedTypeSymbol Symbol => (base.Symbol as INamedTypeSymbol)!;

		/// <summary>
		/// Initializes a new instance of the <see cref="ClassData"/> class.
		/// </summary>
		/// <param name="declaration"><see cref="ClassDeclarationSyntax"/> this <see cref="ClassData"/> represents.</param>
		/// <param name="compilation">Parent <see cref="ICompilationData"/> of this <see cref="ClassData"/>.</param>
		/// <param name="properties"><see cref="TypeData{TDeclaration}.Properties"/> to use for the current instance.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="declaration"/> is <see langword="null"/>. -or- <paramref name="compilation"/> is <see langword="null"/>
		/// </exception>
		public ClassData(ClassDeclarationSyntax declaration, ICompilationData compilation, Properties? properties = default) : base(declaration, compilation, properties)
		{
		}

		internal ClassData(INamedTypeSymbol symbol, ICompilationData compilation) : base(symbol, compilation)
		{
		}
	}
}
