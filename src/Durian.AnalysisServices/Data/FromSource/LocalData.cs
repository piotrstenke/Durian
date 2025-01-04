using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.Analysis.Data.FromSource;

/// <inheritdoc cref="ILocalData"/>
public class LocalData : MemberData, ILocalData, IVariableDeclarator
{
	/// <summary>
	/// Contains optional data that can be passed to a <see cref="LocalData"/>.
	/// </summary>
	public new class Properties : Properties<ILocalSymbol>, IVariableDeclaratorProperties
	{
		/// <inheritdoc cref="LocalData.Index"/>
		public int? Index { get; set; }

		/// <inheritdoc cref="LocalData.Variable"/>
		public VariableDeclaratorSyntax? Variable { get; set; }

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
			properties.Index = Index;
			properties.Variable = Variable;
		}

		/// <inheritdoc/>
#pragma warning disable CS0809 // Obsolete member overrides non-obsolete member
		[Obsolete("Use Map(Properties) instead")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public sealed override void Map(Properties<ILocalSymbol> properties)
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

		void IVariableDeclaratorProperties.FillWithDefaultData()
		{
			FillWithDefaultData();
		}

		/// <inheritdoc/>
		protected override void FillWithDefaultData()
		{
			SetDefaultData();

			Attributes = ImmutableArray<AttributeData>.Empty;
		}
	}

	private sealed class LocalDataWrapper : DataHelpers.VariableDataWrapper<ILocalSymbol, LocalData>
	{
		public LocalDataWrapper(LocalData parentLocal, int index) : base(parentLocal, index)
		{
		}

		protected override LocalData CreateData(IVariableDeclaratorProperties props)
		{
			return new LocalData(Parent.Declaration, Parent.ParentCompilation, props as Properties);
		}

		protected override VariableDeclarationSyntax GetVariableDeclaration()
		{
			return Parent.Declaration.Declaration;
		}
	}

	/// <summary>
	/// Target <see cref="LocalDeclarationStatementSyntax"/>.
	/// </summary>
	public new LocalDeclarationStatementSyntax Declaration => (base.Declaration as LocalDeclarationStatementSyntax)!;

	/// <summary>
	/// Index of this local in the <see cref="Declaration"/>.
	/// </summary>
	public int Index { get; private set; }

	/// <summary>
	/// <see cref="ILocalSymbol"/> associated with the <see cref="Declaration"/>.
	/// </summary>
	public new ILocalSymbol Symbol => (base.Symbol as ILocalSymbol)!;

	/// <summary>
	/// <see cref="VariableDeclaratorSyntax"/> used to declare this field. Equivalent to using <c>Declaration.Declaration.Variables[Index]</c>.
	/// </summary>
	public VariableDeclaratorSyntax Variable { get; private set; } = null!;

	ILocalData ISymbolOrMember<ILocalSymbol, ILocalData>.Member => this;

	/// <summary>
	/// Initializes a new instance of the <see cref="FieldData"/> class.
	/// </summary>
	/// <param name="declaration"><see cref="LocalDeclarationStatementSyntax"/> this <see cref="LocalData"/> represents.</param>
	/// <param name="compilation">Parent <see cref="ICompilationData"/> of this <see cref="LocalData"/>.</param>
	/// <param name="index">Index of this field in the <paramref name="declaration"/>.</param>
	/// <exception cref="ArgumentNullException">
	/// <paramref name="declaration"/> is <see langword="null"/>. -or- <paramref name="compilation"/> is <see langword="null"/>
	/// </exception>
	/// <exception cref="IndexOutOfRangeException">
	/// <paramref name="index"/> was out of range.
	/// </exception>
	public LocalData(LocalDeclarationStatementSyntax declaration, ICompilationData compilation, int index)
		: this(declaration, compilation, new Properties { Index = index })
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="LocalData"/> class.
	/// </summary>
	/// <param name="declaration"><see cref="LocalDeclarationStatementSyntax"/> this <see cref="LocalData"/> represents.</param>
	/// <param name="compilation">Parent <see cref="ICompilationData"/> of this <see cref="LocalData"/>.</param>
	/// <param name="properties"><see cref="Properties"/> to use for the current instance.</param>
	/// <exception cref="ArgumentNullException">
	/// <paramref name="declaration"/> is <see langword="null"/>. -or- <paramref name="compilation"/> is <see langword="null"/>
	/// </exception>
	/// <exception cref="IndexOutOfRangeException">
	/// <see cref="Properties.Index"/> was out of range.
	/// </exception>
	public LocalData(LocalDeclarationStatementSyntax declaration, ICompilationData compilation, Properties? properties)
		: base(declaration, compilation, DataHelpers.EnsureValidDeclaratorProperties<Properties>(declaration, compilation, properties))
	{
	}

	internal LocalData(ILocalSymbol symbol, ICompilationData compilation, MemberData.Properties? properties = default) : base(symbol, compilation, properties)
	{
	}

	/// <summary>
	/// Returns a collection of <see cref="ILocalSymbol"/>s of all variables defined in the <see cref="Declaration"/>.
	/// </summary>
	public IEnumerable<ISymbolOrMember<ILocalSymbol, LocalData>> GetUnderlayingLocals()
	{
		int index = Index;
		int length = Declaration.Declaration.Variables.Count;

		for (int i = 0; i < index; i++)
		{
			yield return new LocalDataWrapper(this, i);
		}

		for (int i = index + 1; i < length; i++)
		{
			yield return new LocalDataWrapper(this, i);
		}
	}

	/// <inheritdoc cref="MemberData.Map(MemberData.Properties)"/>
	public virtual void Map(Properties properties)
	{
		base.Map(properties);
		properties.Variable = Variable;
		properties.Index = Index;
	}

	/// <inheritdoc cref="MemberData.Clone"/>
	public new LocalData Clone()
	{
		return (CloneCore() as LocalData)!;
	}

	/// <inheritdoc cref="MemberData.GetProperties"/>
	public new Properties GetProperties()
	{
		return (GetPropertiesCore() as Properties)!;
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
		return new LocalData(Declaration, ParentCompilation, GetProperties());
	}

	/// <inheritdoc/>
#pragma warning disable CS0809 // Obsolete member overrides non-obsolete member
	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use GetDefaultPropertiesCore() instead")]
	protected sealed override MemberData.Properties? GetDefaultProperties()
#pragma warning restore CS0809 // Obsolete member overrides non-obsolete member
	{
		return GetDefaultPropertiesCore();
	}

	/// <inheritdoc cref="MemberData.GetDefaultProperties()"/>
	protected virtual Properties? GetDefaultPropertiesCore()
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
			Index = props.Index ?? default;
			Variable = props.Variable ?? Declaration.Declaration.Variables[Index];
		}
	}

	IVariableDeclaratorProperties IVariableDeclarator.GetProperties()
	{
		return GetProperties();
	}

	IEnumerable<ISymbolOrMember<ILocalSymbol, ILocalData>> ILocalData.GetUnderlayingLocals()
	{
		return GetUnderlayingLocals();
	}

	ILocalData ILocalData.Clone()
	{
		return Clone();
	}
}
