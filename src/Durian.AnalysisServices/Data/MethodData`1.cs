// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.ComponentModel;
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
			/// <inheritdoc cref="IMethodData.Parameters"/>
			public DefaultedValue<ISymbolContainer<IParameterSymbol, IParameterData>> Parameters { get; set; }

			/// <inheritdoc cref="IMethodData.IsModuleInitializer"/>
			public bool? IsModuleInitializer { get; set; }

			/// <inheritdoc cref="IMethodData.IsParameterless"/>
			public bool? IsParameterless { get; set; }

			/// <summary>
			/// Local functions contained within this method.
			/// </summary>
			public DefaultedValue<ILeveledSymbolContainer<IMethodSymbol, IMethodData>> LocalFunctions { get; set; }

			/// <inheritdoc cref="IMethodData.OverriddenMethod"/>
			public DefaultedValue<ISymbolOrMember<IMethodSymbol, IMethodData>> OverriddenMethod { get; set; }

			/// <inheritdoc cref="IMethodData.OverriddenMethods"/>
			public DefaultedValue<ISymbolContainer<IMethodSymbol, IMethodData>> OverriddenMethods { get; set; }

			/// <summary>
			/// All overloads of this method.
			/// </summary>
			public DefaultedValue<ILeveledSymbolContainer<IMethodSymbol, IMethodData>> Overloads { get; set; }

			/// <inheritdoc cref="IGenericMemberData.TypeArguments"/>
			public DefaultedValue<ISymbolContainer<ITypeSymbol, ITypeData>> TypeArguments { get; set; }

			/// <inheritdoc cref="IGenericMemberData.TypeParameters"/>
			public DefaultedValue<ISymbolContainer<ITypeParameterSymbol, ITypeParameterData>> TypeParameters { get; set; }

			/// <summary>
			/// Initializes a new instance of the <see cref="Properties"/> class.
			/// </summary>
			public Properties()
			{
			}

			/// <summary>
			/// Initializes a new instance of the <see cref="Properties"/> class.
			/// </summary>
			/// <param name="fillWithDefault">Determines whether to fill the current properties with default data.</param>
			public Properties(bool fillWithDefault) : base(fillWithDefault)
			{
			}

			/// <inheritdoc cref="MemberData.Properties.Clone"/>
			public new Properties Clone()
			{
				return (CloneCore() as Properties)!;
			}

			/// <inheritdoc cref="MemberData.Properties.Map(MemberData.Properties)"/>
			public virtual void Map(Properties properties)
			{
				base.Map(properties);
				properties.IsModuleInitializer = properties.IsModuleInitializer;
				properties.IsParameterless = properties.IsParameterless;
				properties.Parameters = Parameters;
				properties.LocalFunctions = LocalFunctions;
				properties.Overloads = Overloads;
				properties.TypeArguments = TypeArguments;
				properties.TypeParameters = TypeParameters;
				properties.OverriddenMethods = OverriddenMethods;
				properties.OverriddenMethod = OverriddenMethod;
			}

			/// <inheritdoc/>
#pragma warning disable CS0809 // Obsolete member overrides non-obsolete member

			[Obsolete("Use Map(Properties) instead")]
			[EditorBrowsable(EditorBrowsableState.Never)]
			public sealed override void Map(Properties<IMethodSymbol> properties)
#pragma warning restore CS0809 // Obsolete member overrides non-obsolete member
			{
				if (properties is Properties props)
				{
					Map(props);
				}
				else
				{
					base.Map(properties);
				}
			}

			/// <inheritdoc/>
			protected override MemberData.Properties CloneCore()
			{
				Properties properties = new();
				Map(properties);
				return properties;
			}
		}

		private MethodStyle? _bodyType;
		private ISymbolContainer<ITypeSymbol, ITypeData>? _typeArguments;
		private ISymbolContainer<ITypeParameterSymbol, ITypeParameterData>? _typeParameters;
		private ISymbolContainer<IMethodSymbol, IMethodData>? _localFunctions;
		private ISymbolContainer<IParameterSymbol, IParameterData>? _parameters;
		private ILeveledSymbolContainer<IMethodSymbol, IMethodData>? _overloads;
		private ISymbolContainer<IMethodSymbol, IMethodData>? _overriddenMethods;
		private DefaultedValue<ISymbolOrMember<IMethodSymbol, IMethodData>> _overriddenMethod;
		private bool? _isModuleInitializer;
		private bool? _isParameterless;

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
		public bool IsModuleInitializer
		{
			get
			{
				return _isModuleInitializer ??= Symbol.IsModuleInitializer();
			}
		}

		/// <inheritdoc/>
		public bool IsParameterless
		{
			get
			{
				return _isParameterless ??= Symbol.IsParameterless();
			}
		}

		/// <inheritdoc/>
		public ISymbolContainer<IMethodSymbol, IMethodData> OverriddenMethods
		{
			get
			{
				return _overriddenMethods ??= Symbol.GetOverriddenSymbols().ToContainer(ParentCompilation);
			}
		}

		/// <inheritdoc/>
		public ISymbolOrMember<IMethodSymbol, IMethodData>? OverriddenMethod
		{
			get
			{
				if (_overriddenMethod.IsDefault)
				{
					if (Symbol.OverriddenMethod is null)
					{
						_overriddenMethod = null;
					}
					else if (_overriddenMethods is null)
					{
						_overriddenMethod = new(Symbol.OverriddenMethod.ToDataOrSymbol(ParentCompilation));
					}
					else if (_overriddenMethods.Count > 0)
					{
						_overriddenMethod = new(_overriddenMethods.First(ReturnOrder.ChildToParent));
					}
					else
					{
						_overriddenMethod = null;
					}
				}

				return _overriddenMethod.Value;
			}
		}

		/// <summary>
		/// Container of <see cref="IParameterSymbol"/> of this indexer.
		/// </summary>
		public ISymbolContainer<IParameterSymbol, IParameterData> Parameters
		{
			get
			{
				return _parameters ??= Symbol.Parameters.ToContainer(ParentCompilation);
			}
		}

		/// <inheritdoc/>
		public ISymbolContainer<ITypeSymbol, ITypeData> TypeArguments
		{
			get
			{
				return _typeArguments ??= Symbol.IsGenericMethod
					? Symbol.TypeArguments.ToContainer(ParentCompilation)
					: SymbolContainerFactory.Empty<ITypeSymbol, ITypeData>();
			}
		}

		/// <inheritdoc/>
		public ISymbolContainer<ITypeParameterSymbol, ITypeParameterData> TypeParameters
		{
			get
			{
				return _typeParameters ??= Symbol.IsGenericMethod
					? Symbol.TypeParameters.ToContainer(ParentCompilation)
					: SymbolContainerFactory.Empty<ITypeParameterSymbol, ITypeParameterData>();
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
		}

		internal MethodData(IMethodSymbol symbol, ICompilationData compilation, MemberData.Properties? properties = default) : base(symbol, compilation, properties)
		{
		}
	}
}
