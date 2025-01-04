using System;
using System.ComponentModel;
using Durian.Analysis.CodeGeneration;
using Durian.Analysis.Extensions;
using Durian.Analysis.SymbolContainers;
using Durian.Analysis.SymbolContainers.Specialized;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.Analysis.Data.FromSource;

/// <inheritdoc cref="IOperatorData"/>
public class OperatorData : MemberData, IOperatorData
{
	/// <summary>
	/// Contains optional data that can be passed to a <see cref="OperatorData"/>.
	/// </summary>
	public new class Properties : Properties<IMethodSymbol>
	{
		/// <summary>
		/// Local functions contained within this method.
		/// </summary>
		public DefaultedValue<ILeveledSymbolContainer<IMethodSymbol, ILocalFunctionData>> LocalFunctions { get; set; }

		/// <inheritdoc cref="OperatorData.OperatorKind"/>
		public OverloadableOperator? OperatorKind { get; set; }

		/// <inheritdoc cref="IMethodData.Parameters"/>
		public DefaultedValue<ISymbolContainer<IParameterSymbol, IParameterData>> Parameters { get; set; }

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
			properties.OperatorKind = OperatorKind;
			properties.LocalFunctions = LocalFunctions;
			properties.Parameters = Parameters;
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

	private MethodStyle? _bodyType;
	private DefaultedValue<SyntaxNode> _body;
	private ILeveledSymbolContainer<IMethodSymbol, ILocalFunctionData>? _localFunctions;
	private OverloadableOperator? _operatorKind;
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

	/// <summary>
	/// Target <see cref="OperatorDeclarationSyntax"/>.
	/// </summary>
	public new OperatorDeclarationSyntax Declaration => (base.Declaration as OperatorDeclarationSyntax)!;

	/// <inheritdoc/>
	public OverloadableOperator OperatorKind => _operatorKind ??= Symbol.GetOperatorKind();

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

	/// <summary>
	/// <see cref="IMethodSymbol"/> associated with the <see cref="Declaration"/>.
	/// </summary>
	public new IMethodSymbol Symbol => (base.Symbol as IMethodSymbol)!;

	string? IMethodData.CompilerCondition => null;

	ISymbolOrMember<IMethodSymbol, IMethodData>? IMethodData.ExplicitInterfaceImplementation => null;

	ISymbolContainer<IMethodSymbol, IMethodData> IMethodData.ImplicitInterfaceImplementations => SymbolContainerFactory.Empty<IMethodSymbol, IMethodData>();

	bool IMethodData.IsDefaultImplementation => false;

	bool IMethodData.IsParameterless => false;

	bool IMethodData.IsModuleInitializer => false;

	ISymbolOrMember<IMethodSymbol, IMethodData>? IMethodData.OverriddenMethod => null;

	ISymbolContainer<IMethodSymbol, IMethodData> IMethodData.OverriddenMethods => SymbolContainerFactory.Empty<IMethodSymbol, IMethodData>();

	ISymbolContainer<ITypeSymbol, ITypeData> IGenericMemberData.TypeArguments => SymbolContainerFactory.Empty<ITypeSymbol, ITypeData>();

	ISymbolContainer<ITypeParameterSymbol, ITypeParameterData> IGenericMemberData.TypeParameters => SymbolContainerFactory.Empty<ITypeParameterSymbol, ITypeParameterData>();

	SyntaxNode IMethodData.SafeDeclaration => Declaration;

	BaseMethodDeclarationSyntax IMethodData.Declaration => Declaration;

	IMethodData ISymbolOrMember<IMethodSymbol, IMethodData>.Member => this;

	IOperatorData ISymbolOrMember<IMethodSymbol, IOperatorData>.Member => this;

	/// <summary>
	/// Initializes a new instance of the <see cref="OperatorData"/> class.
	/// </summary>
	/// <param name="declaration"><see cref="OperatorDeclarationSyntax"/> this <see cref="OperatorData"/> represents.</param>
	/// <param name="compilation">Parent <see cref="ICompilationData"/> of this <see cref="OperatorData"/>.</param>
	/// <param name="properties"><see cref="Properties"/> to use for the current instance.</param>
	/// <exception cref="ArgumentNullException">
	/// <paramref name="declaration"/> is <see langword="null"/>. -or- <paramref name="compilation"/> is <see langword="null"/>
	/// </exception>
	public OperatorData(OperatorDeclarationSyntax declaration, ICompilationData compilation, Properties? properties = default) : base(declaration, compilation, properties)
	{
	}

	internal OperatorData(IMethodSymbol symbol, ICompilationData compilation, MemberData.Properties? properties = default) : base(symbol, compilation, properties)
	{
	}

	/// <inheritdoc cref="MemberData.Clone"/>
	public new OperatorData Clone()
	{
		return (CloneCore() as OperatorData)!;
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
		properties.OperatorKind = _operatorKind;
		properties.LocalFunctions = DataHelpers.ToDefaultedValue(_localFunctions);
		properties.Parameters = DataHelpers.ToDefaultedValue(_parameters);
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
		return new OperatorData(Declaration, ParentCompilation, GetProperties());
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
			_operatorKind = props.OperatorKind;
			_parameters = DataHelpers.FromDefaultedOrEmpty(props.Parameters);
			_localFunctions = DataHelpers.FromDefaultedOrEmpty(props.LocalFunctions);
		}
	}

	IOperatorData IOperatorData.Clone()
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
