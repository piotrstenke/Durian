// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Durian.Analysis.Extensions;
using Durian.Analysis.SymbolContainers;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;

namespace Durian.Analysis.Data
{
	/// <summary>
	/// Encapsulates data associated with a single <see cref="InterfaceDeclarationSyntax"/>.
	/// </summary>
	public class InterfaceData : TypeData<InterfaceDeclarationSyntax>
	{
		/// <summary>
		/// Contains optional data that can be passed to a <see cref="InterfaceData"/>.
		/// </summary>
		public new class Properties : TypeData<InterfaceDeclarationSyntax>.Properties
		{
			/// <inheritdoc cref="InterfaceData.DefaultImplementations"/>
			public SymbolContainer<ISymbol>? DefaultImplementations { get; set; }

			/// <summary>
			/// Initializes a new instance of the <see cref="Properties"/> class.
			/// </summary>
			public Properties()
			{
			}
		}

		private SymbolContainer<ISymbol>? _defaultImplementations;

		/// <summary>
		/// <see cref="INamedTypeSymbol"/> associated with the <see cref="TypeData{TDeclaration}.Declaration"/>.
		/// </summary>
		public new INamedTypeSymbol Symbol => (base.Symbol as INamedTypeSymbol)!;

		/// <summary>
		/// Members of the interface that are default-implemented.
		/// </summary>
		public SymbolContainer<ISymbol> DefaultImplementations
		{
			get
			{
				return _defaultImplementations ??= Symbol.GetDefaultImplementations(true).ToContainer(ParentCompilation);
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="InterfaceData"/> class.
		/// </summary>
		/// <param name="declaration"><see cref="InterfaceDeclarationSyntax"/> this <see cref="InterfaceData"/> represents.</param>
		/// <param name="compilation">Parent <see cref="ICompilationData"/> of this <see cref="InterfaceData"/>.</param>
		/// <param name="properties"><see cref="Properties"/> to use for the current instance.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="declaration"/> is <see langword="null"/>. -or- <paramref name="compilation"/> is <see langword="null"/>
		/// </exception>
		public InterfaceData(InterfaceDeclarationSyntax declaration, ICompilationData compilation, Properties? properties = default) : base(declaration, compilation, properties)
		{
			if(properties is not null)
			{
				_defaultImplementations = properties.DefaultImplementations;
			}
		}

		internal InterfaceData(INamedTypeSymbol symbol, ICompilationData compilation) : base(symbol, compilation)
		{
		}
	}
}
