using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.Analysis.Data.FromSource;

/// <inheritdoc cref="IFieldData"/>
public class FieldData : MemberData, IFieldData, IVariableDeclarator
{
	/// <summary>
	/// Contains optional data that can be passed to a <see cref="FieldData"/>.
	/// </summary>
	public new class Properties : Properties<IFieldSymbol>, IVariableDeclaratorProperties
	{
		/// <inheritdoc cref="FieldData.BackingFieldKind"/>
		public BackingFieldKind? BackingFieldKind { get; set; }

		/// <inheritdoc cref="FieldData.CustomOffset"/>
		public int? CustomOffset { get; set; }

		/// <inheritdoc cref="FieldData.IsThreadStatic"/>
		public bool? IsThreadStatic { get; set; }

		/// <inheritdoc cref="FieldData.Index"/>
		public int? Index { get; set; }

		/// <inheritdoc cref="FieldData.Variable"/>
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
			properties.BackingFieldKind = BackingFieldKind;
			properties.IsThreadStatic = IsThreadStatic;
			properties.CustomOffset = CustomOffset;
		}

		/// <inheritdoc/>
#pragma warning disable CS0809 // Obsolete member overrides non-obsolete member
		[Obsolete("Use Map(Properties) instead")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public sealed override void Map(Properties<IFieldSymbol> properties)
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
			IsPartial = false;
			Virtuality = Analysis.Virtuality.NotVirtual;
		}
	}

	private sealed class FieldDataWrapper : DataHelpers.VariableDataWrapper<IFieldSymbol, FieldData>
	{
		public FieldDataWrapper(FieldData parentField, int index) : base(parentField, index)
		{
		}

		protected override FieldData CreateData(IVariableDeclaratorProperties props)
		{
			return new FieldData(Parent.Declaration, Parent.ParentCompilation, props as Properties);
		}

		protected override VariableDeclarationSyntax GetVariableDeclaration()
		{
			return Parent.Declaration.Declaration;
		}
	}

	private bool? _isThreadStatic;
	private int? _customOffset;

	/// <inheritdoc cref="IFieldData.BackingFieldKind"/>
	public BackingFieldKind BackingFieldKind { get; private set; }

	/// <inheritdoc/>
	public int CustomOffset
	{
		get
		{
			if (!_customOffset.HasValue)
			{
				AttributeData? attribute = this.GetSpecialAttribute(SpecialAttribute.FieldOffset);

				if (attribute is null)
				{
					_customOffset = -1;
				}
				else
				{
					_customOffset = attribute.TryGetConstructorArgumentValue(0, out int value) ? value : -1;
				}
			}

			return _customOffset.Value;
		}
	}

	/// <summary>
	/// Target <see cref="FieldDeclarationSyntax"/>.
	/// </summary>
	public new FieldDeclarationSyntax Declaration => (base.Declaration as FieldDeclarationSyntax)!;

	/// <summary>
	/// Index of this field in the <see cref="Declaration"/>.
	/// </summary>
	public int Index { get; private set; }

	/// <inheritdoc/>
	public bool IsThreadStatic
	{
		get
		{
			return _isThreadStatic ??= Attributes.Any(attr => attr.GetSpecialAttributeKind() == SpecialAttribute.ThreadStatic);
		}
	}

	/// <summary>
	/// <see cref="IFieldSymbol"/> associated with the <see cref="Declaration"/>.
	/// </summary>
	public new IFieldSymbol Symbol => (base.Symbol as IFieldSymbol)!;

	/// <summary>
	/// <see cref="VariableDeclaratorSyntax"/> used to declare this field. Equivalent to using <c>Declaration.Declaration.Variables[Index]</c>.
	/// </summary>
	public VariableDeclaratorSyntax Variable { get; private set; } = null!;

	IFieldData ISymbolOrMember<IFieldSymbol, IFieldData>.Member => this;

	/// <summary>
	/// Initializes a new instance of the <see cref="FieldData"/> class.
	/// </summary>
	/// <param name="declaration"><see cref="FieldDeclarationSyntax"/> this <see cref="FieldData"/> represents.</param>
	/// <param name="compilation">Parent <see cref="ICompilationData"/> of this <see cref="FieldData"/>.</param>
	/// <param name="index">Index of this field in the <paramref name="declaration"/>.</param>
	/// <exception cref="ArgumentNullException">
	/// <paramref name="declaration"/> is <see langword="null"/>. -or- <paramref name="compilation"/> is <see langword="null"/>
	/// </exception>
	/// <exception cref="IndexOutOfRangeException">
	/// <paramref name="index"/> was out of range.
	/// </exception>
	public FieldData(FieldDeclarationSyntax declaration, ICompilationData compilation, int index)
		: this(declaration, compilation, new Properties { Index = index })
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="PropertyData"/> class.
	/// </summary>
	/// <param name="declaration"><see cref="FieldDeclarationSyntax"/> this <see cref="FieldData"/> represents.</param>
	/// <param name="compilation">Parent <see cref="ICompilationData"/> of this <see cref="FieldData"/>.</param>
	/// <param name="properties"><see cref="Properties"/> to use for the current instance.</param>
	/// <exception cref="ArgumentNullException">
	/// <paramref name="declaration"/> is <see langword="null"/>. -or- <paramref name="compilation"/> is <see langword="null"/>
	/// </exception>
	/// <exception cref="IndexOutOfRangeException">
	/// <see cref="Properties.Index"/> was out of range.
	/// </exception>
	public FieldData(FieldDeclarationSyntax declaration, ICompilationData compilation, Properties? properties)
		: base(declaration, compilation, DataHelpers.EnsureValidDeclaratorProperties<Properties>(declaration, compilation, properties))
	{
	}

	internal FieldData(IFieldSymbol symbol, ICompilationData compilation, MemberData.Properties? properties = default) : base(symbol, compilation, properties)
	{
	}

	/// <inheritdoc cref="MemberData.Clone"/>
	public new FieldData Clone()
	{
		return (CloneCore() as FieldData)!;
	}

	/// <inheritdoc cref="MemberData.GetProperties"/>
	public new Properties GetProperties()
	{
		return (GetPropertiesCore() as Properties)!;
	}

	/// <summary>
	/// Returns a collection of <see cref="IFieldSymbol"/>s of all variables defined in the <see cref="Declaration"/>.
	/// </summary>
	public IEnumerable<ISymbolOrMember<IFieldSymbol, FieldData>> GetUnderlayingFields()
	{
		int index = Index;
		int length = Declaration.Declaration.Variables.Count;

		for (int i = 0; i < index; i++)
		{
			yield return new FieldDataWrapper(this, i);
		}

		for (int i = index + 1; i < length; i++)
		{
			yield return new FieldDataWrapper(this, i);
		}
	}

	/// <inheritdoc cref="MemberData.Map(MemberData.Properties)"/>
	public virtual void Map(Properties properties)
	{
		base.Map(properties);
		properties.Variable = Variable;
		properties.Index = Index;
		properties.CustomOffset = _customOffset;
		properties.BackingFieldKind = BackingFieldKind;
		properties.IsThreadStatic = _isThreadStatic;
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
		return new FieldData(Declaration, ParentCompilation, GetProperties());
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
			BackingFieldKind = props.BackingFieldKind ?? Symbol.GetBackingFieldKind();
			_isThreadStatic = props.IsThreadStatic;
			_customOffset = props.CustomOffset;
		}
	}

	IVariableDeclaratorProperties IVariableDeclarator.GetProperties()
	{
		return GetProperties();
	}

	IEnumerable<ISymbolOrMember<IFieldSymbol, IFieldData>> IFieldData.GetUnderlayingFields()
	{
		return GetUnderlayingFields();
	}

	IFieldData IFieldData.Clone()
	{
		return Clone();
	}
}
