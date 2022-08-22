// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.ComponentModel;
using Durian.Analysis.CodeGeneration;
using Durian.Analysis.Extensions;
using Durian.Analysis.SymbolContainers;
using Durian.Analysis.SymbolContainers.Specialized;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.Analysis.Data.FromSource
{
	/// <inheritdoc cref="ILocalFunctionData"/>
	public class LocalFunctionData : MemberData, ILocalFunctionData
	{
		/// <summary>
		/// Contains optional data that can be passed to a <see cref="DestructorData"/>.
		/// </summary>
		public new class Properties : Properties<IMethodSymbol>
		{
			/// <inheritdoc cref="LambdaData.CapturedVariables"/>
			public DefaultedValue<ISymbolContainer<ISymbol, IMemberData>> CapturedVariables { get; set; }

			/// <inheritdoc cref="ConstructorData.IsParameterless"/>
			public bool? IsParameterless { get; set; }

			/// <summary>
			/// Local functions of this accessor.
			/// </summary>
			public DefaultedValue<ILeveledSymbolContainer<IMethodSymbol, ILocalFunctionData>> LocalFunctions { get; set; }

			/// <inheritdoc cref="ConstructorData.Parameters"/>
			public DefaultedValue<ISymbolContainer<IParameterSymbol, IParameterData>> Parameters { get; set; }

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
				properties.LocalFunctions = LocalFunctions;
				properties.CapturedVariables = CapturedVariables;
				properties.Parameters = Parameters;
				properties.IsParameterless = IsParameterless;
				properties.TypeArguments = TypeArguments;
				properties.TypeParameters = TypeParameters;
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

			/// <inheritdoc/>
			protected override void FillWithDefaultData()
			{
				HiddenSymbol = null;
				IsNew = false;
				IsPartial = false;
				Virtuality = Analysis.Virtuality.NotVirtual;
				IsUnsafe = false;
			}
		}

		private MethodStyle? _bodyType;
		private DefaultedValue<SyntaxNode> _body;
		private ISymbolContainer<IParameterSymbol, IParameterData>? _parameters;
		private ISymbolContainer<ISymbol, IMemberData>? _capturedVariables;
		private bool? _isParameterless;
		private ILeveledSymbolContainer<IMethodSymbol, ILocalFunctionData>? _localFunctions;
		private ISymbolContainer<ITypeSymbol, ITypeData>? _typeArguments;
		private ISymbolContainer<ITypeParameterSymbol, ITypeParameterData>? _typeParameters;

		/// <inheritdoc/>
		public SyntaxNode? Body
		{
			get
			{
				if (_body.IsDefault)
				{
					_body = Declaration.GetBody();
				}

				return _body.Value;
			}
		}

		/// <inheritdoc/>
		public MethodStyle BodyType => _bodyType ??= Declaration.GetBodyType();

		/// <inheritdoc/>
		public ISymbolContainer<ISymbol, IMemberData> CapturedVariables
		{
			get
			{
				return _capturedVariables ??= SemanticModel.GetCapturedVariables(Declaration).ToContainer(ParentCompilation);
			}
		}

		/// <inheritdoc/>
		public new LocalFunctionStatementSyntax Declaration => (base.Declaration as LocalFunctionStatementSyntax)!;

		/// <inheritdoc/>
		public bool IsParameterless => _isParameterless ??= Symbol.IsParameterless();

		/// <inheritdoc/>
		public ISymbolContainer<IParameterSymbol, IParameterData> Parameters
		{
			get
			{
				return _parameters ??= Symbol.Parameters.ToContainer(ParentCompilation);
			}
		}

		/// <inheritdoc/>
		public new IMethodSymbol Symbol => (base.Symbol as IMethodSymbol)!;

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

		string? IMethodData.CompilerCondition => null;

		ISymbolOrMember<IMethodSymbol, IMethodData>? IMethodData.ExplicitInterfaceImplementation => null;

		ISymbolContainer<IMethodSymbol, IMethodData> IMethodData.ImplicitInterfaceImplementations => SymbolContainerFactory.Empty<IMethodSymbol, IMethodData>();

		bool IMethodData.IsDefaultImplementation => false;

		bool IMethodData.IsModuleInitializer => false;

		ISymbolOrMember<IMethodSymbol, IMethodData>? IMethodData.OverriddenMethod => null;

		ISymbolContainer<IMethodSymbol, IMethodData> IMethodData.OverriddenMethods => SymbolContainerFactory.Empty<IMethodSymbol, IMethodData>();

		SyntaxNode IMethodData.SafeDeclaration => Declaration;

		BaseMethodDeclarationSyntax IMethodData.Declaration => throw new InvalidOperationException("A local function cannot be represented by a BaseMethodDeclarationSyntax");

		IMethodData ISymbolOrMember<IMethodSymbol, IMethodData>.Member => this;

		ILocalFunctionData ISymbolOrMember<IMethodSymbol, ILocalFunctionData>.Member => this;

		/// <summary>
		/// Initializes a new instance of the <see cref="LocalFunctionData"/> class.
		/// </summary>
		/// <param name="declaration"><see cref="LocalFunctionStatementSyntax"/> this <see cref="LocalFunctionData"/> represents.</param>
		/// <param name="compilation">Parent <see cref="ICompilationData"/> of this <see cref="LocalFunctionData"/>.</param>
		/// <param name="properties"><see cref="Properties"/> to use for the current instance.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="declaration"/> is <see langword="null"/>. -or- <paramref name="compilation"/> is <see langword="null"/>
		/// </exception>
		public LocalFunctionData(LocalFunctionStatementSyntax declaration, ICompilationData compilation, Properties? properties = default) : base(declaration, compilation, properties)
		{
		}

		internal LocalFunctionData(IMethodSymbol symbol, ICompilationData compilation, MemberData.Properties? properties = default) : base(symbol, compilation, properties)
		{
		}

		/// <inheritdoc cref="MemberData.Clone"/>
		public new LocalFunctionData Clone()
		{
			return (CloneCore() as LocalFunctionData)!;
		}

		/// <inheritdoc/>
		public ISymbolContainer<IMethodSymbol, ILocalFunctionData> GetLocalFunctions(IncludedMembers members)
		{
			_localFunctions ??= new LocalFunctionsContainer(this, ParentCompilation);

			if (_localFunctions is IMappedSymbolContainer<IMethodSymbol, ILocalFunctionData, IncludedMembers> mapped)
			{
				return mapped.ResolveLevel(members);
			}

			return _localFunctions.ResolveLevel((int)members);
		}

		/// <inheritdoc cref="MemberData.GetProperties"/>
		public new Properties GetProperties()
		{
			return (GetPropertiesCore() as Properties)!;
		}

		/// <inheritdoc cref="MemberData.Properties.Map(MemberData.Properties)"/>
		public virtual void Map(Properties properties)
		{
			base.Map(properties);
			properties.LocalFunctions = DataHelpers.ToDefaultedValue(_localFunctions);
			properties.CapturedVariables = DataHelpers.ToDefaultedValue(_capturedVariables);
			properties.IsParameterless = _isParameterless;
			properties.Parameters = DataHelpers.ToDefaultedValue(_parameters);
			properties.TypeParameters = DataHelpers.ToDefaultedValue(_typeParameters);
			properties.TypeArguments = DataHelpers.ToDefaultedValue(_typeArguments);
		}

		/// <inheritdoc/>
#pragma warning disable CS0809 // Obsolete member overrides non-obsolete member
		[Obsolete("Use Map(Properties) instead")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public sealed override void Map(MemberData.Properties properties)
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
		protected override MemberData CloneCore()
		{
			return new LocalFunctionData(Declaration, ParentCompilation, GetProperties());
		}

		/// <inheritdoc/>
		protected override MemberData.Properties? GetDefaultProperties()
		{
			return new Properties(true);
		}

		/// <inheritdoc/>
		protected override MemberData.Properties GetPropertiesCore()
		{
			Properties properties = new();
			Map(properties);
			return properties;
		}

		/// <inheritdoc/>
		protected override void SetProperties(MemberData.Properties properties)
		{
			base.SetProperties(properties);

			if (properties is Properties props)
			{
				_localFunctions = DataHelpers.FromDefaultedOrEmpty(props.LocalFunctions);
				_capturedVariables = DataHelpers.FromDefaultedOrEmpty(props.CapturedVariables);
				_parameters = DataHelpers.FromDefaultedOrEmpty(props.Parameters);
				_isParameterless = props.IsParameterless;
				_typeArguments = DataHelpers.FromDefaultedOrEmpty(props.TypeArguments);
				_typeParameters = DataHelpers.FromDefaultedOrEmpty(props.TypeParameters);
			}
		}

		ILocalFunctionData ILocalFunctionData.Clone()
		{
			return Clone();
		}

		ISymbolContainer<IMethodSymbol, IMethodData> IMethodData.GetOverloads(IncludedMembers members)
		{
			return SymbolContainerFactory.Empty<IMethodSymbol, IMethodData>();
		}

		IMethodData IMethodData.Clone()
		{
			return Clone();
		}
	}
}
