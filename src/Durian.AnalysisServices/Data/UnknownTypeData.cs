// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.Analysis.Data
{
	/// <summary>
	/// Encapsulates data associated with an unknown kind of type declaration.
	/// </summary>
	public class UnknownTypeData : TypeData<TypeDeclarationSyntax>
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="UnknownTypeData"/> class.
		/// </summary>
		/// <param name="declaration"><see cref="BaseTypeDeclarationSyntax"/> this <see cref="UnknownTypeData"/> represents.</param>
		/// <param name="compilation">Parent <see cref="ICompilationData"/> of this <see cref="UnknownTypeData"/>.</param>
		/// <param name="properties"><see cref="TypeData{TDeclaration}.Properties"/> to use for the current instance.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="declaration"/> is <see langword="null"/>. -or- <paramref name="compilation"/> is <see langword="null"/>
		/// </exception>
		public UnknownTypeData(TypeDeclarationSyntax declaration, ICompilationData compilation, Properties? properties = default) : base(declaration, compilation, properties)
		{
		}

		internal UnknownTypeData(ITypeSymbol symbol, ICompilationData compilation) : base(symbol, compilation)
		{
		}
	}
}
