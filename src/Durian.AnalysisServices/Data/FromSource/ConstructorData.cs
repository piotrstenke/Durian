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
	/// <inheritdoc cref="IConstructorData"/>
	public class ConstructorData : MemberData, IConstructorData
	{
		/// <summary>
		/// Contains optional data that can be passed to a <see cref="ConstructorData"/>.
		/// </summary>
		public new class Properties : Properties<IMethodSymbol>
		{
			/// <inheritdoc cref="ConstructorData.BaseConstructor"/>
			public DefaultedValue<ISymbolOrMember<IMethodSymbol, IConstructorData>> BaseConstructor { get; set; }

			/// <inheritdoc cref="ConstructorData.BaseConstructors"/>
			public DefaultedValue<ISymbolContainer<IMethodSymbol, IConstructorData>> BaseConstructors { get; set; }

			/// <summary>
			/// Local functions of this accessor.
			/// </summary>
			public DefaultedValue<ILeveledSymbolContainer<IMethodSymbol, ILocalFunctionData>> LocalFunctions { get; set; }

			/// <inheritdoc cref="ConstructorData.IsPrimary"/>
			public bool? IsPrimary { get; set; }

			/// <inheritdoc cref="ConstructorData.IsParameterless"/>
			public bool? IsParameterless { get; set; }

			/// <inheritdoc cref="ConstructorData.Parameters"/>
			public DefaultedValue<ISymbolContainer<IParameterSymbol, IParameterData>> Parameters { get; set; }

			/// <inheritdoc cref="ConstructorData.SpecialKind"/>
			public SpecialConstructor? SpecialKind { get; set; }

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
				properties.SpecialKind = SpecialKind;
				properties.IsPrimary = IsPrimary;
				properties.BaseConstructor = BaseConstructor;
				properties.BaseConstructors = BaseConstructors;
				properties.IsParameterless = IsParameterless;
				properties.Parameters = Parameters;
				properties.LocalFunctions = LocalFunctions;
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
			}
		}

		private MethodStyle? _bodyType;
		private DefaultedValue<SyntaxNode> _body;
		private bool? _isParameterless;
		private bool? _isPrimary;
		private SpecialConstructor? _specialKind;
		private ISymbolContainer<IParameterSymbol, IParameterData>? _parameters;
		private DefaultedValue<ISymbolOrMember<IMethodSymbol, IConstructorData>> _baseCtor;
		private ISymbolContainer<IMethodSymbol, IConstructorData>? _baseCtors;
		private ILeveledSymbolContainer<IMethodSymbol, ILocalFunctionData>? _localFunctions;

		/// <inheritdoc/>
		public ISymbolOrMember<IMethodSymbol, IConstructorData>? BaseConstructor
		{
			get
			{
				if (_baseCtor.IsDefault)
				{
					if (SemanticModel.GetBaseConstructor(Declaration, Symbol.ContainingType) is not IMethodSymbol ctor)
					{
						_baseCtor = null;
					}
					else if (_baseCtors is null)
					{
						_baseCtor = new(ctor.ToDataOrSymbol<IConstructorData>(ParentCompilation));
					}
					else if (_baseCtors.Count > 0)
					{
						_baseCtor = new(_baseCtors.First(ReturnOrder.ChildToParent));
					}
					else
					{
						_baseCtor = null;
					}
				}

				return _baseCtor.Value;
			}
		}

		/// <inheritdoc/>
		public ISymbolContainer<IMethodSymbol, IConstructorData> BaseConstructors => _baseCtors ??= SemanticModel.GetBaseConstructors(Declaration).ToContainer<IConstructorData>(ParentCompilation);

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
		public new ConstructorDeclarationSyntax Declaration => (base.Declaration as ConstructorDeclarationSyntax)!;

		/// <inheritdoc/>
		public bool IsParameterless => _isParameterless ??= Symbol.IsParameterless();

		/// <inheritdoc/>
		public bool IsPrimary => _isPrimary ??= Symbol.IsPrimaryConstructor();

		/// <inheritdoc/>
		public ISymbolContainer<IParameterSymbol, IParameterData> Parameters
		{
			get
			{
				return _parameters ??= Symbol.Parameters.ToContainer(ParentCompilation);
			}
		}

		/// <inheritdoc/>
		public SpecialConstructor SpecialKind => _specialKind ??= Symbol.GetConstructorKind();

		/// <inheritdoc/>
		public new IMethodSymbol Symbol => (base.Symbol as IMethodSymbol)!;

		string? IMethodData.CompilerCondition => null;

		ISymbolOrMember<IMethodSymbol, IMethodData>? IMethodData.ExplicitInterfaceImplementation => null;

		ISymbolContainer<IMethodSymbol, IMethodData> IMethodData.ImplicitInterfaceImplementations => SymbolContainerFactory.Empty<IMethodSymbol, IMethodData>();

		bool IMethodData.IsDefaultImplementation => false;

		bool IMethodData.IsModuleInitializer => false;

		ISymbolOrMember<IMethodSymbol, IMethodData>? IMethodData.OverriddenMethod => null;

		ISymbolContainer<IMethodSymbol, IMethodData> IMethodData.OverriddenMethods => SymbolContainerFactory.Empty<IMethodSymbol, IMethodData>();

		ISymbolContainer<ITypeSymbol, ITypeData> IGenericMemberData.TypeArguments => SymbolContainerFactory.Empty<ITypeSymbol, ITypeData>();

		ISymbolContainer<ITypeParameterSymbol, ITypeParameterData> IGenericMemberData.TypeParameters => SymbolContainerFactory.Empty<ITypeParameterSymbol, ITypeParameterData>();

		IMethodData ISymbolOrMember<IMethodSymbol, IMethodData>.Member => this;

		IConstructorData ISymbolOrMember<IMethodSymbol, IConstructorData>.Member => this;

		SyntaxNode IMethodData.SafeDeclaration => Declaration;

		BaseMethodDeclarationSyntax IMethodData.Declaration => Declaration;

		/// <summary>
		/// Initializes a new instance of the <see cref="ConstructorData"/> class.
		/// </summary>
		/// <param name="declaration"><see cref="ConstructorDeclarationSyntax"/> this <see cref="ConstructorData"/> represents.</param>
		/// <param name="compilation">Parent <see cref="ICompilationData"/> of this <see cref="ConstructorData"/>.</param>
		/// <param name="properties"><see cref="MemberData.Properties"/> to use for the current instance.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="declaration"/> is <see langword="null"/>. -or- <paramref name="compilation"/> is <see langword="null"/>
		/// </exception>
		public ConstructorData(ConstructorDeclarationSyntax declaration, ICompilationData compilation, Properties? properties = default) : base(declaration, compilation, properties)
		{
		}

		internal ConstructorData(IMethodSymbol symbol, ICompilationData compilation, MemberData.Properties? properties = default) : base(symbol, compilation, properties)
		{
		}

		/// <inheritdoc cref="MemberData.Clone"/>
		public new ConstructorData Clone()
		{
			return (CloneCore() as ConstructorData)!;
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
			properties.IsPrimary = _isPrimary;
			properties.BaseConstructor = _baseCtor;
			properties.SpecialKind = _specialKind;
			properties.BaseConstructors = DataHelpers.ToDefaultedValue(_baseCtors);
			properties.IsParameterless = _isParameterless;
			properties.Parameters = DataHelpers.ToDefaultedValue(_parameters);
			properties.LocalFunctions = DataHelpers.ToDefaultedValue(_localFunctions);
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
			return new ConstructorData(Declaration, ParentCompilation, GetProperties());
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
				_baseCtor = props.BaseConstructor;
				_baseCtors = DataHelpers.FromDefaultedOrEmpty(props.BaseConstructors);
				_isPrimary = props.IsPrimary;
				_isParameterless = props.IsParameterless;
				_parameters = DataHelpers.FromDefaultedOrEmpty(props.Parameters);
				_specialKind = props.SpecialKind;
				_localFunctions = DataHelpers.FromDefaultedOrEmpty(props.LocalFunctions);
			}
		}

		IConstructorData IConstructorData.Clone()
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
