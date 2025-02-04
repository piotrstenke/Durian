using System;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using Durian.Analysis.SymbolContainers;
using Durian.Analysis.SymbolContainers.Specialized;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.Analysis.Data.FromSource;

/// <inheritdoc cref="ITypeData"/>
/// <typeparam name="TDeclaration">Specific type of the target <see cref="TypeDeclarationSyntax"/>.</typeparam>
public abstract class TypeData<TDeclaration> : MemberData, ITypeData where TDeclaration : TypeDeclarationSyntax
{
	/// <summary>
	/// Contains optional data that can be passed to a <see cref="TypeData{TDeclaration}"/>.
	/// </summary>
	public new class Properties : Properties<INamedTypeSymbol>
	{
		/// <inheritdoc cref="ITypeData.BaseTypes"/>
		public DefaultedValue<ISymbolContainer<INamedTypeSymbol, ITypeData>> BaseTypes { get; set; }

		/// <inheritdoc cref="ITypeData.CompilerCondition"/>
		public DefaultedValue<string> CompilerCondition { get; set; }

		/// <inheritdoc cref="ITypeData.IsAttribute"/>
		public bool? IsAttribute { get; set; }

		/// <inheritdoc cref="ITypeData.IsException"/>
		public bool? IsException { get; set; }

		/// <summary>
		/// Container of child <see cref="ISymbol"/>s of this type.
		/// </summary>
		public DefaultedValue<ILeveledSymbolContainer<ISymbol, IMemberData>> Members { get; set; }

		/// <inheritdoc cref="ITypeData.ParameterlessConstructor"/>
		public DefaultedValue<ISymbolOrMember<IMethodSymbol, IMethodData>> ParameterlessConstructor { get; set; }

		/// <inheritdoc cref="ITypeData.PartialDeclarations"/>
		public ImmutableArray<TDeclaration> PartialDeclarations { get; set; }

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
			properties.IsException = IsException;
			properties.IsAttribute = IsAttribute;
			properties.Members = Members;
			properties.ParameterlessConstructor = ParameterlessConstructor;
			properties.PartialDeclarations = PartialDeclarations;
			properties.TypeArguments = TypeArguments;
			properties.TypeParameters = TypeParameters;
			properties.BaseTypes = BaseTypes;
			properties.CompilerCondition = CompilerCondition;
		}

		/// <inheritdoc/>
#pragma warning disable CS0809 // Obsolete member overrides non-obsolete member

		[Obsolete("Use Map(Properties) instead")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public sealed override void Map(Properties<INamedTypeSymbol> properties)
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

	private ISymbolContainer<INamedTypeSymbol, ITypeData>? _baseTypes;
	private DefaultedValue<string> _compilerCondition;
	private bool? _isAttribute;
	private bool? _isException;
	private ILeveledSymbolContainer<ISymbol, IMemberData>? _members;
	private DefaultedValue<ISymbolOrMember<IMethodSymbol, IMethodData>> _parameterlessConstructor;
	private ImmutableArray<TDeclaration> _partialDeclarations;
	private ISymbolContainer<ITypeSymbol, ITypeData>? _typeArguments;
	private ISymbolContainer<ITypeParameterSymbol, ITypeParameterData>? _typeParameters;

	/// <inheritdoc/>
	public ISymbolContainer<INamedTypeSymbol, ITypeData> BaseTypes
	{
		get
		{
			return _baseTypes ??= Symbol.GetBaseTypes().ToContainer(ParentCompilation);
		}
	}

	/// <inheritdoc/>
	public string? CompilerCondition
	{
		get
		{
			if (_compilerCondition.IsDefault)
			{
				AttributeData? attribute = this.GetSpecialAttribute(SpecialAttribute.Conditional);
				_compilerCondition = attribute?.GetConstructorArgumentValue<string>(0);
			}

			return _compilerCondition;
		}
	}

	/// <summary>
	/// Target <see cref="TypeDeclarationSyntax"/>.
	/// </summary>
	public new TDeclaration Declaration => (base.Declaration as TDeclaration)!;

	/// <inheritdoc/>
	public bool IsAttribute => _isAttribute ??= Symbol.IsAttribute();

	/// <inheritdoc/>
	public bool IsException => _isException ??= Symbol.IsException();

	/// <inheritdoc/>
	public ISymbolOrMember<IMethodSymbol, IMethodData>? ParameterlessConstructor
	{
		get
		{
			if (_parameterlessConstructor.IsDefault)
			{
				_parameterlessConstructor = new(Symbol.GetSpecialConstructor(SpecialConstructor.Parameterless)?.ToDataOrSymbol(ParentCompilation));
			}

			return _parameterlessConstructor.Value;
		}
	}

	/// <inheritdoc cref="ITypeData.PartialDeclarations"/>
	public ImmutableArray<TDeclaration> PartialDeclarations
	{
		get
		{
			if (_partialDeclarations.IsDefault)
			{
				_partialDeclarations = Symbol.DeclaringSyntaxReferences
					.Select(e => e.GetSyntax())
					.OfType<TDeclaration>()
					.ToImmutableArray();
			}

			return _partialDeclarations;
		}
	}

	/// <summary>
	/// <see cref="INamedTypeSymbol"/> associated with the <see cref="Declaration"/>.
	/// </summary>
	public new INamedTypeSymbol Symbol => (base.Symbol as INamedTypeSymbol)!;

	/// <inheritdoc/>
	public ISymbolContainer<ITypeSymbol, ITypeData> TypeArguments
	{
		get
		{
			return _typeArguments ??= Symbol.IsGenericType
				? Symbol.TypeArguments.ToContainer(ParentCompilation)
				: SymbolContainerFactory.Empty<ITypeSymbol, ITypeData>();
		}
	}

	/// <inheritdoc/>
	public ISymbolContainer<ITypeParameterSymbol, ITypeParameterData> TypeParameters
	{
		get
		{
			return _typeParameters ??= Symbol.IsGenericType
				? Symbol.TypeParameters.ToContainer(ParentCompilation)
				: SymbolContainerFactory.Empty<ITypeParameterSymbol, ITypeParameterData>();
		}
	}

	ITypeData ISymbolOrMember<INamedTypeSymbol, ITypeData>.Member => this;
	INamespaceOrTypeData ISymbolOrMember<INamespaceOrTypeSymbol, INamespaceOrTypeData>.Member => this;
	ImmutableArray<TypeDeclarationSyntax> ITypeData.PartialDeclarations => PartialDeclarations.CastArray<TypeDeclarationSyntax>();
	INamespaceOrTypeSymbol INamespaceOrTypeData.Symbol => Symbol;
	INamespaceOrTypeSymbol ISymbolOrMember<INamespaceOrTypeSymbol, INamespaceOrTypeData>.Symbol => Symbol;

	BaseTypeDeclarationSyntax ITypeData.Declaration => Declaration;

	SyntaxNode ITypeData.SafeDeclaration => Declaration;

	/// <summary>
	/// Initializes a new instance of the <see cref="TypeData{TDeclaration}"/> class.
	/// </summary>
	/// <param name="declaration"><see cref="BaseTypeDeclarationSyntax"/> this <see cref="TypeData{TDeclaration}"/> represents.</param>
	/// <param name="compilation">Parent <see cref="ICompilationData"/> of this <see cref="TypeData{TDeclaration}"/>.</param>
	/// <param name="properties"><see cref="MemberData.Properties"/> to use for the current instance.</param>
	/// <exception cref="ArgumentNullException">
	/// <paramref name="declaration"/> is <see langword="null"/>. -or- <paramref name="compilation"/> is <see langword="null"/>
	/// </exception>
	protected TypeData(TDeclaration declaration, ICompilationData compilation, MemberData.Properties? properties = default) : base(declaration, compilation, properties)
	{
	}

	internal TypeData(ITypeSymbol symbol, ICompilationData compilation, MemberData.Properties? properties = default) : base(symbol, compilation, properties)
	{
	}

	/// <inheritdoc cref="MemberData.Clone"/>
	public new TypeData<TDeclaration> Clone()
	{
		return (CloneCore() as TypeData<TDeclaration>)!;
	}

	/// <inheritdoc/>
	public ISymbolContainer<IEventSymbol, IEventData> GetEvents(IncludedMembers members)
	{
		InitMembers();

		switch (_members)
		{
			case InnerMemberContainer typed:
				IMappedSymbolContainer<IEventSymbol, IEventData, IncludedMembers> events = typed.GetEvents();
				return events.ResolveLevel(members);

			case IMappedSymbolContainer<IEventSymbol, IEventData, IncludedMembers> mapped:
				return mapped.ResolveLevel(members);

			case ILeveledSymbolContainer<IEventSymbol, IEventData> leveled:
				return leveled.ResolveLevel((int)members);

			case IMappedSymbolContainer<ISymbol, IMemberData, IncludedMembers> mappedUnknown:
				return mappedUnknown.ResolveLevel(members).OfType<ISymbolOrMember<IEventSymbol, IEventData>>().ToContainer(ParentCompilation);

			default:
				return _members.ResolveLevel((int)members).OfType<ISymbolOrMember<IEventSymbol, IEventData>>().ToContainer(ParentCompilation);
		}
	}

	/// <inheritdoc/>
	public ISymbolContainer<IFieldSymbol, IFieldData> GetFields(IncludedMembers members)
	{
		InitMembers();

		switch (_members)
		{
			case InnerMemberContainer typed:
				IMappedSymbolContainer<IFieldSymbol, IFieldData, IncludedMembers> fields = typed.GetFields();
				return fields.ResolveLevel(members);

			case IMappedSymbolContainer<IFieldSymbol, IFieldData, IncludedMembers> mapped:
				return mapped.ResolveLevel(members);

			case ILeveledSymbolContainer<IFieldSymbol, IFieldData> leveled:
				return leveled.ResolveLevel((int)members);

			case IMappedSymbolContainer<ISymbol, IMemberData, IncludedMembers> mappedUnknown:
				return mappedUnknown.ResolveLevel(members).OfType<ISymbolOrMember<IFieldSymbol, IFieldData>>().ToContainer(ParentCompilation);

			default:
				return _members.ResolveLevel((int)members).OfType<ISymbolOrMember<IFieldSymbol, IFieldData>>().ToContainer(ParentCompilation);
		}
	}

	/// <inheritdoc/>
	public ISymbolContainer<ISymbol, IMemberData> GetMembers(IncludedMembers members)
	{
		InitMembers();

		if (_members is IMappedSymbolContainer<ISymbol, IMemberData, IncludedMembers> container)
		{
			return container.ResolveLevel(members);
		}

		return _members.ResolveLevel((int)members);
	}

	/// <inheritdoc/>
	public ISymbolContainer<IMethodSymbol, IMethodData> GetMethods(IncludedMembers members)
	{
		InitMembers();

		switch (_members)
		{
			case InnerMemberContainer typed:
				IMappedSymbolContainer<IMethodSymbol, IMethodData, IncludedMembers> methods = typed.GetMethods();
				return methods.ResolveLevel(members);

			case IMappedSymbolContainer<IMethodSymbol, IMethodData, IncludedMembers> mapped:
				return mapped.ResolveLevel(members);

			case ILeveledSymbolContainer<IMethodSymbol, IMethodData> leveled:
				return leveled.ResolveLevel((int)members);

			case IMappedSymbolContainer<ISymbol, IMemberData, IncludedMembers> mappedUnknown:
				return mappedUnknown.ResolveLevel(members).OfType<ISymbolOrMember<IMethodSymbol, IMethodData>>().ToContainer(ParentCompilation);

			default:
				return _members.ResolveLevel((int)members).OfType<ISymbolOrMember<IMethodSymbol, IMethodData>>().ToContainer(ParentCompilation);
		}
	}

	/// <inheritdoc cref="MemberData.GetProperties"/>
	public new Properties GetProperties()
	{
		return (GetPropertiesCore() as Properties)!;
	}

	/// <inheritdoc/>
	public ISymbolContainer<IPropertySymbol, IPropertyData> GetProperties(IncludedMembers members)
	{
		InitMembers();

		switch (_members)
		{
			case InnerMemberContainer typed:
				IMappedSymbolContainer<IPropertySymbol, IPropertyData, IncludedMembers> properties = typed.GetProperties();
				return properties.ResolveLevel(members);

			case IMappedSymbolContainer<IPropertySymbol, IPropertyData, IncludedMembers> mapped:
				return mapped.ResolveLevel(members);

			case ILeveledSymbolContainer<IPropertySymbol, IPropertyData> leveled:
				return leveled.ResolveLevel((int)members);

			case IMappedSymbolContainer<ISymbol, IMemberData, IncludedMembers> mappedUnknown:
				return mappedUnknown.ResolveLevel(members).OfType<ISymbolOrMember<IPropertySymbol, IPropertyData>>().ToContainer(ParentCompilation);

			default:
				return _members.ResolveLevel((int)members).OfType<ISymbolOrMember<IPropertySymbol, IPropertyData>>().ToContainer(ParentCompilation);
		}
	}

	/// <inheritdoc/>
	public ISymbolContainer<INamedTypeSymbol, ITypeData> GetTypes(IncludedMembers members)
	{
		InitMembers();

		switch (_members)
		{
			case InnerMemberContainer typed:
				IMappedSymbolContainer<INamedTypeSymbol, ITypeData, IncludedMembers> types = typed.GetTypes();
				return types.ResolveLevel(members);

			case IMappedSymbolContainer<INamedTypeSymbol, ITypeData, IncludedMembers> mapped:
				return mapped.ResolveLevel(members);

			case ILeveledSymbolContainer<INamedTypeSymbol, ITypeData> leveled:
				return leveled.ResolveLevel((int)members);

			case IMappedSymbolContainer<ISymbol, ITypeData, IncludedMembers> mappedUnknown:
				return mappedUnknown.ResolveLevel(members).OfType<ISymbolOrMember<INamedTypeSymbol, ITypeData>>().ToContainer(ParentCompilation);

			default:
				return _members.ResolveLevel((int)members).OfType<ISymbolOrMember<INamedTypeSymbol, ITypeData>>().ToContainer(ParentCompilation);
		}
	}

	/// <inheritdoc cref="MemberData.Map(MemberData.Properties)"/>
	public virtual void Map(Properties properties)
	{
		base.Map(properties);
		properties.IsAttribute = _isAttribute;
		properties.IsException = _isException;
		properties.Members = DataHelpers.ToDefaultedValue(_members);
		properties.TypeParameters = DataHelpers.ToDefaultedValue(_typeParameters);
		properties.TypeArguments = DataHelpers.ToDefaultedValue(_typeArguments);
		properties.PartialDeclarations = _partialDeclarations;
		properties.ParameterlessConstructor = _parameterlessConstructor;
		properties.BaseTypes = DataHelpers.ToDefaultedValue(_baseTypes);
		properties.CompilerCondition = _compilerCondition;
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
	protected abstract override MemberData CloneCore();

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
			_isAttribute = props.IsAttribute;
			_isException = props.IsException;
			_members = DataHelpers.FromDefaultedOrEmpty(props.Members);
			_baseTypes = DataHelpers.FromDefaultedOrEmpty(props.BaseTypes);
			_parameterlessConstructor = props.ParameterlessConstructor;
			_partialDeclarations = props.PartialDeclarations;
			_typeArguments = DataHelpers.FromDefaultedOrEmpty(props.TypeArguments);
			_typeParameters = DataHelpers.FromDefaultedOrEmpty(props.TypeParameters);
			_compilerCondition = props.CompilerCondition;
		}
	}

	INamespaceData INamespaceOrTypeData.ToNamespace()
	{
		throw new InvalidOperationException("Current symbol is not a namespace");
	}

	ITypeData INamespaceOrTypeData.ToType()
	{
		return this;
	}

	ITypeData ITypeData.Clone()
	{
		return Clone();
	}

	INamespaceOrTypeData INamespaceOrTypeData.Clone()
	{
		return Clone();
	}

	[MemberNotNull(nameof(_members))]
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void InitMembers()
	{
		_members ??= new InnerMemberContainer(this, ParentCompilation);
	}
}
