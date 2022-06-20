// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Durian.Analysis.CodeGeneration;
using Durian.Analysis.Extensions;
using Durian.Analysis.SymbolContainers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.Analysis.Data
{
	/// <summary>
	/// Encapsulates data associated with a single method declaration.
	/// </summary>
	/// <typeparam name="TDeclaration">Type of method declaration this class represents.</typeparam>
	public abstract class MethodData<TDeclaration> : MemberData, IMethodData where TDeclaration : CSharpSyntaxNode
	{
		/// <summary>
		/// Contains optional data that can be passed to a <see cref="MethodData{TDeclaration}"/>.
		/// </summary>
		public new class Properties : Properties<IMethodSymbol>
		{
			/// <inheritdoc cref="MethodData{TDeclaration}.Parameters"/>
			public ISymbolContainer<IParameterSymbol, ParameterData>? Parameters { get; set; }

			/// <inheritdoc cref="MethodData{TDeclaration}.TypeParameters"/>
			public ISymbolContainer<ITypeParameterSymbol, >? TypeParameters { get; set; }

			/// <inheritdoc cref="MethodData{TDeclaration}.OverridenMethods"/>
			public ISymbolContainer<IMethodSymbol>? OverridenMethods { get; set; }

			/// <inheritdoc cref="MethodData{TDeclaration}.Overloads"/>
			public ISymbolContainer<IMethodSymbol>? Overloads { get; set; }

			/// <summary>
			/// Initializes a new instance of the <see cref="Properties"/> class.
			/// </summary>
			public Properties()
			{
			}
		}

		private MethodStyle? _bodyType;
		private ISymbolContainer<IParameterSymbol>? _parameters;
		private ISymbolContainer<ITypeSymbol>? _typeArguments;
		private ISymbolContainer<ITypeParameterSymbol>? _typeParameters;
		private ISymbolContainer<IMethodSymbol>? _overridenMethods;
		private ISymbolContainer<IMethodSymbol>? _overloads;

		/// <inheritdoc/>
		public virtual CSharpSyntaxNode? Body
		{
			get
			{
				return BodyRaw ??= (Declaration as BaseMethodDeclarationSyntax)!.GetBody();
			}
		}

		/// <inheritdoc/>
		public MethodStyle BodyType
		{
			get
			{
				return _bodyType ??= Body switch
				{
					BlockSyntax => MethodStyle.Block,
					ArrowExpressionClauseSyntax => MethodStyle.Expression,
					_ => MethodStyle.None
				};
			}
		}

		/// <summary>
		/// Target <see cref="BaseMethodDeclarationSyntax"/>.
		/// </summary>
		public new TDeclaration Declaration => (base.Declaration as TDeclaration)!;

		/// <summary>
		/// <see cref="IMethodSymbol"/> associated with the <see cref="Declaration"/>.
		/// </summary>
		public new IMethodSymbol Symbol => (base.Symbol as IMethodSymbol)!;

		/// <summary>
		/// Returns the cached body of the method or <see langword="null"/> if there is no currently cached value.
		/// </summary>
		protected CSharpSyntaxNode? BodyRaw { get; set; }

		/// <inheritdoc/>
		public ISymbolContainer<ITypeParameterSymbol, TypeParameterData>? TypeParameters
		{
			get
			{
				return _typeParameters ??= Symbol.TypeParameters.ToContainer(ParentCompilation);
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MethodData{TDeclaration}"/> class.
		/// </summary>
		/// <param name="declaration"><see cref="BaseMethodDeclarationSyntax"/> this <see cref="MethodData{TDeclaration}"/> represents.</param>
		/// <param name="compilation">Parent <see cref="ICompilationData"/> of this <see cref="MethodData{TDeclaration}"/>.</param>
		/// <param name="properties"><see cref="Properties"/> to use for the current instance.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="declaration"/> is <see langword="null"/>. -or- <paramref name="compilation"/> is <see langword="null"/>
		/// </exception>
		protected MethodData(TDeclaration declaration, ICompilationData compilation, Properties? properties = default) : base(declaration, compilation, properties)
		{
			if(properties is not null)
			{

			}
		}

		internal MethodData(IMethodSymbol symbol, ICompilationData compilation) : base(symbol, compilation)
		{
		}
	}
}
