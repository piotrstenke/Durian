using System;
using System.ComponentModel;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.Analysis.Data.FromSource;

/// <inheritdoc cref="ITypeParameterData"/>
public class TypeParameterData : MemberData, ITypeParameterData
{
	/// <summary>
	/// Contains optional data that can be passed to a <see cref="Properties"/>.
	/// </summary>
	public new class Properties : Properties<ITypeParameterSymbol>
	{
		/// <inheritdoc cref="TypeParameterData.ConstraintClause"/>
		public DefaultedValue<TypeParameterConstraintClauseSyntax> ConstraintClause { get; set; }

		/// <inheritdoc cref="TypeParameterData.Constraints"/>
		public GenericConstraint? Constraints { get; set; }

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
			properties.ConstraintClause = ConstraintClause;
			properties.Constraints = Constraints;
		}

		/// <inheritdoc/>
#pragma warning disable CS0809 // Obsolete member overrides non-obsolete member
		[Obsolete("Use Map(Properties) instead")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public sealed override void Map(Properties<ITypeParameterSymbol> properties)
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
		}
	}

	private DefaultedValue<TypeParameterConstraintClauseSyntax> _constraintClause;
	private GenericConstraint? _constraints;

	/// <summary>
	/// <see cref="TypeParameterConstraintSyntax"/> associated with the <see cref="Declaration"/>.
	/// </summary>
	public TypeParameterConstraintClauseSyntax? ConstraintClause
	{
		get
		{
			if (_constraintClause.IsDefault)
			{
				_constraintClause = Declaration.GetConstraintClause();
			}

			return _constraintClause.Value;
		}
	}

	/// <summary>
	/// Generic constraints applied to this type parameter.
	/// </summary>
	public GenericConstraint Constraints
	{
		get
		{
			return _constraints ??= Symbol.GetConstraints(true);
		}
	}

	/// <summary>
	/// Target <see cref="TypeParameterSyntax"/>.
	/// </summary>
	public new TypeParameterSyntax Declaration => (base.Declaration as TypeParameterSyntax)!;

	/// <summary>
	/// <see cref="IPropertySymbol"/> associated with the <see cref="Declaration"/>.
	/// </summary>
	public new ITypeParameterSymbol Symbol => (base.Symbol as ITypeParameterSymbol)!;

	ITypeParameterData ISymbolOrMember<ITypeParameterSymbol, ITypeParameterData>.Member => this;

	/// <summary>
	/// Initializes a new instance of the <see cref="TypeParameterData"/> class.
	/// </summary>
	/// <param name="declaration"><see cref="TypeParameterSyntax"/> this <see cref="TypeParameterData"/> represents.</param>
	/// <param name="compilation">Parent <see cref="ICompilationData"/> of this <see cref="TypeParameterData"/>.</param>
	/// <param name="properties"><see cref="Properties"/> to use for the current instance.</param>
	/// <exception cref="ArgumentNullException">
	/// <paramref name="declaration"/> is <see langword="null"/>. -or- <paramref name="compilation"/> is <see langword="null"/>
	/// </exception>
	public TypeParameterData(TypeParameterSyntax declaration, ICompilationData compilation, Properties? properties = default) : base(declaration, compilation, properties)
	{
	}

	internal TypeParameterData(ITypeParameterSymbol symbol, ICompilationData compilation, MemberData.Properties? properties = default) : base(symbol, compilation, properties)
	{
	}

	/// <inheritdoc cref="MemberData.Clone"/>
	public new TypeParameterData Clone()
	{
		return (CloneCore() as TypeParameterData)!;
	}

	/// <inheritdoc cref="MemberData.GetProperties"/>
	public new Properties GetProperties()
	{
		return (GetPropertiesCore() as Properties)!;
	}

	/// <inheritdoc cref="MemberData.Map(MemberData.Properties)"/>
	public virtual void Map(Properties properties)
	{
		base.Map(properties);
		properties.ConstraintClause = _constraintClause;
		properties.Constraints = _constraints;
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
		return new TypeParameterData(Declaration, ParentCompilation, GetProperties());
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
			_constraintClause = props.ConstraintClause;
			_constraints = props.Constraints;
		}
	}

	ITypeParameterData ITypeParameterData.Clone()
	{
		return Clone();
	}
}
