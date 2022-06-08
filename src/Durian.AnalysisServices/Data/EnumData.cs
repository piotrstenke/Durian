// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Durian.Analysis.Extensions;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.Analysis.Data
{
	/// <summary>
	/// Encapsulates data associated with a single <see cref="EnumDeclarationSyntax"/>.
	/// </summary>
	public class EnumData : TypeData<EnumDeclarationSyntax>
	{
		/// <summary>
		/// Contains optional data that can be passed to an <see cref="EnumData"/>.
		/// </summary>
		public new class Properties : TypeData<EnumDeclarationSyntax>.Properties
		{
			/// <inheritdoc cref="EnumData.IsFlags"/>
			public bool? IsFlags { get; set; }
		}

		private bool? _isFlags;

		/// <summary>
		/// <see cref="INamedTypeSymbol"/> associated with the <see cref="TypeData{TDeclaration}.Declaration"/>.
		/// </summary>
		public new INamedTypeSymbol Symbol => (base.Symbol as INamedTypeSymbol)!;

		/// <summary>
		/// Determines whether the enum has the <see cref="FlagsAttribute"/> applied.
		/// </summary>
		public bool IsFlags
		{
			get
			{
				return _isFlags ??= Symbol.IsFlagsEnum();
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="EnumData"/> class.
		/// </summary>
		/// <param name="declaration"><see cref="EnumDeclarationSyntax"/> this <see cref="EnumData"/> represents.</param>
		/// <param name="compilation">Parent <see cref="ICompilationData"/> of this <see cref="EnumData"/>.</param>
		/// <param name="properties"><see cref="TypeData{TDeclaration}.Properties"/> to use for the current instance.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="declaration"/> is <see langword="null"/>. -or- <paramref name="compilation"/> is <see langword="null"/>
		/// </exception>
		public EnumData(EnumDeclarationSyntax declaration, ICompilationData compilation, Properties? properties = default) : base(declaration, compilation, properties)
		{
			_isFlags = properties?.IsFlags;
		}

		internal EnumData(INamedTypeSymbol symbol, ICompilationData compilation) : base(symbol, compilation)
		{
		}
	}
}
