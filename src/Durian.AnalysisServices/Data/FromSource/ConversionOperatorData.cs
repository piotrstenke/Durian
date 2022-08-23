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
	/// <inheritdoc cref="IConversionOperatorData"/>
	public class ConversionOperatorData : MemberData, IConversionOperatorData
	{
		/// <summary>
		/// Contains optional data that can be passed to a <see cref="ConversionOperatorData"/>.
		/// </summary>
		public new class Properties : Properties<IMethodSymbol>
		{
			private bool? _isImplicit;
			private bool? _isExplicit;

			/// <summary>
			/// Determines whether this conversion operator is implicit.
			/// </summary>
			/// <remarks>Setting this will also set the <see cref="IsExplicit"/> property.</remarks>
			public bool? IsImplicit
			{
				get => _isImplicit;
				set
				{
					_isImplicit = value;

					if (value.HasValue)
					{
						_isExplicit = !value.Value;
					}
				}
			}

			/// <summary>
			/// Determines whether this conversion operator is explicit.
			/// </summary>
			/// <remarks>Setting this will also set the <see cref="IsImplicit"/> property.</remarks>
			public bool? IsExplicit
			{
				get => _isExplicit;
				set
				{
					_isExplicit = value;

					if (value.HasValue)
					{
						_isImplicit = !value.Value;
					}
				}
			}

			/// <summary>
			/// Local functions of this accessor.
			/// </summary>
			public DefaultedValue<ILeveledSymbolContainer<IMethodSymbol, ILocalFunctionData>> LocalFunctions { get; set; }

			/// <summary>
			/// The parameter to convert.
			/// </summary>
			public DefaultedValue<ISymbolOrMember<IParameterSymbol, IParameterData>> Parameter { get; set; }

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
				properties.IsImplicit = _isImplicit;
				properties.IsExplicit = _isExplicit;
				properties.Parameter = Parameter;
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
			}
		}

		private bool? _isExplicit;
		private bool? _isImplicit;
		private MethodStyle? _bodyType;
		private DefaultedValue<SyntaxNode> _body;
		private ILeveledSymbolContainer<IMethodSymbol, ILocalFunctionData>? _localFunctions;
		private DefaultedValue<ISymbolOrMember<IParameterSymbol, IParameterData>> _parameter;
		private ISymbolContainer<IParameterSymbol, IParameterData>? _parameters;

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
		public new ConversionOperatorDeclarationSyntax Declaration => (base.Declaration as ConversionOperatorDeclarationSyntax)!;

		/// <inheritdoc/>
		public bool IsExplicit => _isExplicit ??= Symbol.IsExplicitOperator();

		/// <inheritdoc/>
		public bool IsImplicit => _isImplicit ??= Symbol.IsImplicitOperator();

		/// <inheritdoc/>
		public ISymbolOrMember<IParameterSymbol, IParameterData>? Parameter
		{
			get
			{
				if (_parameter.IsDefault)
				{
					if (Symbol.Parameters.Length == 0)
					{
						_parameter = null;
					}
					else
					{
						_parameter = new(Symbol.Parameters[0].ToDataOrSymbol(ParentCompilation));
					}
				}

				return _parameter.Value;
			}
		}

		/// <inheritdoc/>
		public new IMethodSymbol Symbol => (base.Symbol as IMethodSymbol)!;

		string? IMethodData.CompilerCondition => null;

		ISymbolOrMember<IMethodSymbol, IMethodData>? IMethodData.ExplicitInterfaceImplementation => null;

		ISymbolContainer<IMethodSymbol, IMethodData> IMethodData.ImplicitInterfaceImplementations => SymbolContainerFactory.Empty<IMethodSymbol, IMethodData>();

		bool IMethodData.IsDefaultImplementation => false;

		bool IMethodData.IsParameterless => Parameter is null;

		bool IMethodData.IsModuleInitializer => false;

		ISymbolOrMember<IMethodSymbol, IMethodData>? IMethodData.OverriddenMethod => null;

		ISymbolContainer<IMethodSymbol, IMethodData> IMethodData.OverriddenMethods => SymbolContainerFactory.Empty<IMethodSymbol, IMethodData>();

		ISymbolContainer<IParameterSymbol, IParameterData> IMethodData.Parameters
		{
			get
			{
				if (_parameters is null)
				{
					ISymbolOrMember<IParameterSymbol, IParameterData>? parameter = Parameter;
					_parameters = parameter is null
						? SymbolContainerFactory.Empty<IParameterSymbol, IParameterData>()
						: SymbolContainerFactory.Single(parameter, ParentCompilation);
				}

				return _parameters;
			}
		}

		ISymbolContainer<ITypeSymbol, ITypeData> IGenericMemberData.TypeArguments => SymbolContainerFactory.Empty<ITypeSymbol, ITypeData>();

		ISymbolContainer<ITypeParameterSymbol, ITypeParameterData> IGenericMemberData.TypeParameters => SymbolContainerFactory.Empty<ITypeParameterSymbol, ITypeParameterData>();

		IMethodData ISymbolOrMember<IMethodSymbol, IMethodData>.Member => this;

		IConversionOperatorData ISymbolOrMember<IMethodSymbol, IConversionOperatorData>.Member => this;

		SyntaxNode IMethodData.SafeDeclaration => Declaration;

		BaseMethodDeclarationSyntax IMethodData.Declaration => Declaration;

		/// <summary>
		/// Initializes a new instance of the <see cref="ConversionOperatorData"/> class.
		/// </summary>
		/// <param name="declaration"><see cref="ConversionOperatorDeclarationSyntax"/> this <see cref="ConversionOperatorData"/> represents.</param>
		/// <param name="compilation">Parent <see cref="ICompilationData"/> of this <see cref="ConversionOperatorData"/>.</param>
		/// <param name="properties"><see cref="Properties"/> to use for the current instance.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="declaration"/> is <see langword="null"/>. -or- <paramref name="compilation"/> is <see langword="null"/>
		/// </exception>
		public ConversionOperatorData(ConversionOperatorDeclarationSyntax declaration, ICompilationData compilation, Properties? properties = default) : base(declaration, compilation, properties)
		{
		}

		internal ConversionOperatorData(IMethodSymbol symbol, ICompilationData compilation, MemberData.Properties? properties = default) : base(symbol, compilation, properties)
		{
		}

		/// <inheritdoc cref="MemberData.Clone"/>
		public new ConversionOperatorData Clone()
		{
			return (CloneCore() as ConversionOperatorData)!;
		}

		/// <inheritdoc/>
		public ISymbolContainer<IMethodSymbol, ILocalFunctionData> GetLocalFunctions(IncludedMembers members)
		{
			_localFunctions ??= new LocalFunctionContainer(this, ParentCompilation);

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
			properties.IsImplicit = _isImplicit;
			properties.IsExplicit = _isExplicit;
			properties.Parameter = _parameter;
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
			return new ConversionOperatorData(Declaration, ParentCompilation, GetProperties());
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
				_isImplicit = props.IsImplicit;
				_isExplicit = props.IsExplicit;
				_parameter = props.Parameter;
				_localFunctions = DataHelpers.FromDefaultedOrEmpty(props.LocalFunctions);
			}
		}

		IConversionOperatorData IConversionOperatorData.Clone()
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
