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
	/// <inheritdoc cref="IAccessorData"/>
	public class AccessorData : MemberData, IAccessorData
	{
		/// <summary>
		/// Contains optional data that can be passed to a <see cref="AccessorData"/>.
		/// </summary>
		public new class Properties : Properties<IMethodSymbol>
		{
			/// <summary>
			/// Kind of the accessor.
			/// </summary>
			public AccessorKind? Kind { get; set; }

			/// <summary>
			/// The <see langword="value"/> parameter of the accessor.
			/// </summary>
			public DefaultedValue<ISymbolOrMember<IParameterSymbol, IParameterData>> Parameter { get; set; }

			/// <summary>
			/// Local functions of this accessor.
			/// </summary>
			public DefaultedValue<ILeveledSymbolContainer<IMethodSymbol, ILocalFunctionData>> LocalFunctions { get; set; }

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
				properties.Kind = Kind;
				properties.LocalFunctions = LocalFunctions;
				properties.Parameter = properties.Parameter;
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
				Virtuality = Analysis.Virtuality.NotVirtual;
				HiddenSymbol = null;
				IsNew = false;
				IsPartial = false;
			}
		}

		private MethodStyle? _bodyType;
		private DefaultedValue<SyntaxNode> _body;
		private ILeveledSymbolContainer<IMethodSymbol, ILocalFunctionData>? _localFunctions;
		private DefaultedValue<ISymbolOrMember<IParameterSymbol, IParameterData>> _parameter;
		private ISymbolContainer<IParameterSymbol, IParameterData>? _parameters;

		/// <summary>
		/// Kind of this accessor.
		/// </summary>
		public AccessorKind AccessorKind { get; private set; }

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
		public new AccessorDeclarationSyntax Declaration => (base.Declaration as AccessorDeclarationSyntax)!;

		/// <summary>
		/// The <see langword="value"/> parameter of the accessor.
		/// </summary>
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

		IAccessorData ISymbolOrMember<IMethodSymbol, IAccessorData>.Member => this;

		SyntaxNode IMethodData.SafeDeclaration => Declaration;

		BaseMethodDeclarationSyntax IMethodData.Declaration => throw new InvalidOperationException("An accessor cannot be represented by a BaseMethodDeclarationSyntax");

		/// <summary>
		/// Initializes a new instance of the <see cref="AccessorData"/> class.
		/// </summary>
		/// <param name="declaration"><see cref="AccessorDeclarationSyntax"/> this <see cref="AccessorData"/> represents.</param>
		/// <param name="compilation">Parent <see cref="ICompilationData"/> of this <see cref="AccessorData"/>.</param>
		/// <param name="properties"><see cref="Properties"/> to use for the current instance.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="declaration"/> is <see langword="null"/>. -or- <paramref name="compilation"/> is <see langword="null"/>
		/// </exception>
		public AccessorData(AccessorDeclarationSyntax declaration, ICompilationData compilation, Properties? properties = default) : base(declaration, compilation, properties)
		{
		}

		internal AccessorData(IMethodSymbol symbol, ICompilationData compilation, MemberData.Properties? properties = default) : base(symbol, compilation, properties)
		{
		}

		/// <inheritdoc cref="MemberData.Clone"/>
		public new AccessorData Clone()
		{
			return (CloneCore() as AccessorData)!;
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
			properties.Kind = AccessorKind;
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
			return new AccessorData(Declaration, ParentCompilation, GetProperties());
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
				AccessorKind = props.Kind ?? Symbol.GetAccessorKind();
				_parameter = props.Parameter;
				_localFunctions = DataHelpers.FromDefaultedOrEmpty(props.LocalFunctions);
			}
		}

		IAccessorData IAccessorData.Clone()
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
