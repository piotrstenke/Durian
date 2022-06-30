// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.Analysis.Data
{
	/// <summary>
	/// Encapsulates data associated with a single <see cref="ParameterSyntax"/>.
	/// </summary>
	public class ParameterData : MemberData, IParameterData
	{
		/// <summary>
		/// Target <see cref="ParameterSyntax"/>.
		/// </summary>
		public new ParameterSyntax Declaration => (base.Declaration as ParameterSyntax)!;

		/// <summary>
		/// <see cref="IPropertySymbol"/> associated with the <see cref="Declaration"/>.
		/// </summary>
		public new IParameterSymbol Symbol => (base.Symbol as IParameterSymbol)!;

		BaseParameterSyntax IParameterData.Declaration => Declaration;

		IParameterData ISymbolOrMember<IParameterSymbol, IParameterData>.Member => this;

		/// <summary>
		/// Initializes a new instance of the <see cref="ParameterData"/> class.
		/// </summary>
		/// <param name="declaration"><see cref="ParameterSyntax"/> this <see cref="ParameterData"/> represents.</param>
		/// <param name="compilation">Parent <see cref="ICompilationData"/> of this <see cref="ParameterData"/>.</param>
		/// <param name="properties"><see cref="MemberData.Properties"/> to use for the current instance.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="declaration"/> is <see langword="null"/>. -or- <paramref name="compilation"/> is <see langword="null"/>
		/// </exception>
		public ParameterData(ParameterSyntax declaration, ICompilationData compilation, Properties<IParameterSymbol>? properties = default) : base(declaration, compilation, properties)
		{
		}

		internal ParameterData(IParameterSymbol symbol, ICompilationData compilation) : base(symbol, compilation)
		{
		}
	}
}
