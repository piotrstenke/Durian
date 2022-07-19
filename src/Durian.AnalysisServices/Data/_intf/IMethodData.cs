// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System.Diagnostics;
using Durian.Analysis.CodeGeneration;
using Durian.Analysis.SymbolContainers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

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
		/// Value applied to the <see cref="ConditionalAttribute"/> of this method.
		/// </summary>
		string? CompilerCondition { get; }

		/// <summary>
		/// Determines whether this method is parameterless.
		/// </summary>
		bool IsParameterless { get; }

		/// <summary>
		/// Determines whether this method is a module initializer.
		/// </summary>
		bool IsModuleInitializer { get; }

		/// <summary>
		/// Method overridden by this method.
		/// </summary>
		ISymbolOrMember<IMethodSymbol, IMethodData>? OverriddenMethod { get; }

		/// <summary>
		/// Methods overridden by this method.
		/// </summary>
		ISymbolContainer<IMethodSymbol, IMethodData> OverriddenMethods { get; }

		/// <summary>
		/// Parameters of this method.
		/// </summary>
		ISymbolContainer<IParameterSymbol, IParameterData> Parameters { get; }

		/// <summary>
		/// <see cref="IMethodSymbol"/> associated with the <see cref="IMemberData.Declaration"/>.
		/// </summary>
		new IMethodSymbol Symbol { get; }

		/// <summary>
		/// Returns all overloads of this method.
		/// </summary>
		/// <param name="members">Range of members to include.</param>
		ISymbolContainer<IMethodSymbol, IMethodData> GetOverloads(IncludedMembers members);

		/// <summary>
		/// Returns all local function declared inside this method.
		/// </summary>
		/// <param name="members">Range of members to include.</param>
		ISymbolContainer<IMethodSymbol, IMethodData> GetLocalFunctions(IncludedMembers members);
	}
}
