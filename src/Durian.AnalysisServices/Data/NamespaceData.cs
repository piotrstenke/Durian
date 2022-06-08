// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using Durian.Analysis.CodeGeneration;
using Durian.Analysis.Extensions;
using Durian.Analysis.SymbolContainers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.Analysis.Data
{
	/// <summary>
	/// Encapsulates data associated with a single <see cref="BaseNamespaceDeclarationSyntax"/>.
	/// </summary>
	public class NamespaceData : MemberData
	{
		/// <summary>
		/// Target <see cref="BaseNamespaceDeclarationSyntax"/>.
		/// </summary>
		public new BaseNamespaceDeclarationSyntax Declaration => (base.Declaration as BaseNamespaceDeclarationSyntax)!;

		/// <summary>
		/// Style of this namespace declaration.
		/// </summary>
		public NamespaceStyle DeclarationStyle { get; }

		/// <summary>
		/// <see cref="IPropertySymbol"/> associated with the <see cref="Declaration"/>.
		/// </summary>
		public new INamespaceSymbol Symbol => (base.Symbol as INamespaceSymbol)!;

		internal SyntaxNode BaseDeclaration => base.Declaration;
		internal ISymbol BaseSymbol => base.Symbol;

		/// <summary>
		/// Initializes a new instance of the <see cref="PropertyData"/> class.
		/// </summary>
		/// <param name="declaration"><see cref="BaseNamespaceDeclarationSyntax"/> this <see cref="PropertyData"/> represents.</param>
		/// <param name="compilation">Parent <see cref="ICompilationData"/> of this <see cref="PropertyData"/>.</param>
		/// <param name="properties"><see cref="MemberData.Properties"/> to use for the current instance.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="declaration"/> is <see langword="null"/>. -or- <paramref name="compilation"/> is <see langword="null"/>.
		/// </exception>
		public NamespaceData(BaseNamespaceDeclarationSyntax declaration, ICompilationData compilation, Properties? properties = default) : base(declaration, compilation, properties)
		{
			DeclarationStyle = declaration.GetNamespaceStyle();
		}

		internal NamespaceData(MemberDeclarationSyntax declaration, ICompilationData compilation) : base(declaration, compilation)
		{
		}

		internal NamespaceData(ISymbol symbol, ICompilationData compilation) : base(symbol, compilation)
		{
		}

		/// <inheritdoc/>
		public override TypeContainer ContainingTypes => SymbolContainerFactory.Empty<TypeContainer>();
	}
}
