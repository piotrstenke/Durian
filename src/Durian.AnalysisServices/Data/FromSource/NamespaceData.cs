using System;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Durian.Analysis.CodeGeneration;
using Durian.Analysis.SymbolContainers;
using Durian.Analysis.SymbolContainers.Specialized;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.Analysis.Data.FromSource;

/// <inheritdoc cref="INamespaceData"/>
public class NamespaceData : MemberData, INamespaceData
{
	/// <summary>
	/// Contains optional data that can be passed to a <see cref="NamespaceData"/>.
	/// </summary>
	public new class Properties : Properties<INamespaceSymbol>
	{
		/// <summary>
		/// Container of child <see cref="INamespaceOrTypeSymbol"/>s of this namespace.
		/// </summary>
		public DefaultedValue<ILeveledSymbolContainer<INamespaceOrTypeSymbol, INamespaceOrTypeData>> Members { get; set; }

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
			properties.Members = Members;
		}

		/// <inheritdoc/>
#pragma warning disable CS0809 // Obsolete member overrides non-obsolete member
		[Obsolete("Use Map(Properties) instead")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public sealed override void Map(Properties<INamespaceSymbol> properties)
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
			SetDefaultData();

			Attributes = ImmutableArray<AttributeData>.Empty;
			ContainingTypes = new(null);
		}
	}

	private ILeveledSymbolContainer<INamespaceOrTypeSymbol, INamespaceOrTypeData>? _members;

	/// <summary>
	/// Target <see cref="BaseNamespaceDeclarationSyntax"/>.
	/// </summary>
	public new BaseNamespaceDeclarationSyntax Declaration => (base.Declaration as BaseNamespaceDeclarationSyntax)!;

	/// <summary>
	/// Style of this namespace declaration.
	/// </summary>
	public NamespaceStyle DeclarationStyle { get; }

	/// <summary>
	/// <see cref="IPropertySymbol"/> associated with the <see cref="Declaration"/>.
	/// </summary>
	public new INamespaceSymbol Symbol => (base.Symbol as INamespaceSymbol)!;

	INamespaceData ISymbolOrMember<INamespaceSymbol, INamespaceData>.Member => this;

	INamespaceOrTypeData ISymbolOrMember<INamespaceOrTypeSymbol, INamespaceOrTypeData>.Member => this;
	INamespaceOrTypeSymbol INamespaceOrTypeData.Symbol => Symbol;

	INamespaceOrTypeSymbol ISymbolOrMember<INamespaceOrTypeSymbol, INamespaceOrTypeData>.Symbol => Symbol;

	/// <summary>
	/// Initializes a new instance of the <see cref="PropertyData"/> class.
	/// </summary>
	/// <param name="declaration"><see cref="BaseNamespaceDeclarationSyntax"/> this <see cref="PropertyData"/> represents.</param>
	/// <param name="compilation">Parent <see cref="ICompilationData"/> of this <see cref="PropertyData"/>.</param>
	/// <param name="properties"><see cref="MemberData.Properties"/> to use for the current instance.</param>
	/// <exception cref="ArgumentNullException">
	/// <paramref name="declaration"/> is <see langword="null"/>. -or- <paramref name="compilation"/> is <see langword="null"/>.
	/// </exception>
	public NamespaceData(BaseNamespaceDeclarationSyntax declaration, ICompilationData compilation, Properties? properties = default) : base(declaration, compilation, properties)
	{
		DeclarationStyle = declaration.GetNamespaceStyle();
	}

	internal NamespaceData(ISymbol symbol, ICompilationData compilation, MemberData.Properties? properties = default) : base(symbol, compilation, properties)
	{
		DeclarationStyle = Declaration.GetNamespaceStyle();
	}

	/// <inheritdoc cref="MemberData.Clone"/>
	public new NamespaceData Clone()
	{
		return (CloneCore() as NamespaceData)!;
	}

	/// <inheritdoc/>
	public ISymbolContainer<INamespaceOrTypeSymbol, INamespaceOrTypeData> GetMembers(IncludedMembers members)
	{
		InitMembers();

		if (_members is IMappedSymbolContainer<INamespaceOrTypeSymbol, INamespaceOrTypeData, IncludedMembers> container)
		{
			return container.ResolveLevel(members);
		}

		return _members.ResolveLevel((int)members);
	}

	/// <inheritdoc/>
	public ISymbolContainer<INamespaceSymbol, INamespaceData> GetNamespaces(IncludedMembers members)
	{
		InitMembers();

		switch (_members)
		{
			case NamespaceOrTypeContainer typed:
				IMappedSymbolContainer<INamespaceSymbol, INamespaceData, IncludedMembers> namespaces = typed.GetNamespaces();
				return namespaces.ResolveLevel(members);

			case IMappedSymbolContainer<INamespaceSymbol, INamespaceData, IncludedMembers> mapped:
				return mapped.ResolveLevel(members);

			case ILeveledSymbolContainer<INamespaceSymbol, INamespaceData> leveled:
				return leveled.ResolveLevel((int)members);

			case IMappedSymbolContainer<INamespaceOrTypeSymbol, INamespaceOrTypeData, IncludedMembers> mappedUnknown:
				return mappedUnknown.ResolveLevel(members).OfType<ISymbolOrMember<INamespaceSymbol, INamespaceData>>().ToContainer(ParentCompilation);

			default:
				return _members.ResolveLevel((int)members).OfType<ISymbolOrMember<INamespaceSymbol, INamespaceData>>().ToContainer(ParentCompilation);
		}
	}

	/// <inheritdoc cref="MemberData.GetProperties"/>
	public new Properties GetProperties()
	{
		return (GetPropertiesCore() as Properties)!;
	}

	/// <inheritdoc/>
	public ISymbolContainer<INamedTypeSymbol, ITypeData> GetTypes(IncludedMembers members)
	{
		InitMembers();

		switch (_members)
		{
			case NamespaceOrTypeContainer typed:
				IMappedSymbolContainer<INamedTypeSymbol, ITypeData, IncludedMembers> types = typed.GetTypes();
				return types.ResolveLevel(members);

			case IMappedSymbolContainer<INamedTypeSymbol, ITypeData, IncludedMembers> mapped:
				return mapped.ResolveLevel(members);

			case ILeveledSymbolContainer<INamedTypeSymbol, ITypeData> leveled:
				return leveled.ResolveLevel((int)members);

			case IMappedSymbolContainer<INamespaceOrTypeSymbol, INamespaceOrTypeData, IncludedMembers> mappedUnknown:
				return mappedUnknown.ResolveLevel(members).OfType<ISymbolOrMember<INamedTypeSymbol, ITypeData>>().ToContainer(ParentCompilation);

			default:
				return _members.ResolveLevel((int)members).OfType<ISymbolOrMember<INamedTypeSymbol, ITypeData>>().ToContainer(ParentCompilation);
		}
	}

	/// <inheritdoc cref="MemberData.Map(MemberData.Properties)"/>
	public virtual void Map(Properties properties)
	{
		base.Map(properties);
		properties.Members = DataHelpers.ToDefaultedValue(_members);
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
		return new NamespaceData(Declaration, ParentCompilation, GetProperties());
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
			_members = DataHelpers.FromDefaultedOrEmpty(props.Members);
		}
	}

	INamespaceData INamespaceOrTypeData.ToNamespace()
	{
		return this;
	}

	ITypeData INamespaceOrTypeData.ToType()
	{
		throw new InvalidOperationException("Current symbol is not a type");
	}

	INamespaceData INamespaceData.Clone()
	{
		return Clone();
	}

	INamespaceOrTypeData INamespaceOrTypeData.Clone()
	{
		return Clone();
	}

	[MemberNotNull(nameof(_members))]
	private void InitMembers()
	{
		_members ??= new NamespaceOrTypeContainer(this, ParentCompilation);
	}
}
