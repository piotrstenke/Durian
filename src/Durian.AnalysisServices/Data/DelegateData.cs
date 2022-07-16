// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.Analysis.Data
{
	/// <summary>
	/// Encapsulates data associated with a single <see cref="DelegateDeclarationSyntax"/>.
	/// </summary>
	public class DelegateData : MemberData
	{
		/// <summary>
		/// Target <see cref="DelegateDeclarationSyntax"/>.
		/// </summary>
		public new DelegateDeclarationSyntax Declaration => (base.Declaration as DelegateDeclarationSyntax)!;

		/// <summary>
		/// <see cref="INamedTypeSymbol"/> associated with the <see cref="Declaration"/>.
		/// </summary>
		public new INamedTypeSymbol Symbol => (base.Symbol as INamedTypeSymbol)!;

		/// <summary>
		/// Initializes a new instance of the <see cref="DelegateData"/> class.
		/// </summary>
		/// <param name="declaration"><see cref="DelegateDeclarationSyntax"/> this <see cref="DelegateData"/> represents.</param>
		/// <param name="compilation">Parent <see cref="ICompilationData"/> of this <see cref="DelegateData"/>.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="declaration"/> is <see langword="null"/>. -or- <paramref name="compilation"/> is <see langword="null"/>
		/// </exception>
		public DelegateData(DelegateDeclarationSyntax declaration, ICompilationData compilation) : base(declaration, compilation)
		{
		}

		internal DelegateData(INamedTypeSymbol symbol, ICompilationData compilation, MemberData.Properties? properties = default) : base(symbol, compilation, properties)
		{
		}
	}
}
