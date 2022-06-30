﻿// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Durian.Analysis.CodeGeneration;
using Durian.Analysis.SymbolContainers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.Analysis.Data
{
	/// <summary>
	/// Encapsulates data associated with a single <see cref="IMethodSymbol"/>.
	/// </summary>
	public interface IMethodData : IGenericMemberData, ISymbolOrMember<IMethodSymbol, IMethodData>
	{
		/// <summary>
		/// Body of the method.
		/// </summary>
		CSharpSyntaxNode? Body { get; }

		/// <summary>
		/// Type of body of the method.
		/// </summary>
		MethodStyle BodyType { get; }

		/// <summary>
		/// <see cref="IMethodSymbol"/> associated with the <see cref="Declaration"/>.
		/// </summary>
		new IMethodSymbol Symbol { get; }

		/// <summary>
		/// Overloads of this method.
		/// </summary>
		ISymbolContainer<IMethodSymbol, IMethodData> Overloads { get; }

		/// <summary>
		/// Methods overridden by this method.
		/// </summary>
		ISymbolContainer<IMethodSymbol, IMethodData> OverridenMethods { get; }

		/// <summary>
		/// Parameters of this method.
		/// </summary>
		ISymbolContainer<IParameterSymbol, IParameterData> Parameters { get; }
	}
}
