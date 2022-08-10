// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Durian.Analysis.Extensions;
using Durian.Analysis.SymbolContainers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.Analysis.Data.FromSource
{
	/// <inheritdoc cref="IEventData"/>
	public class EventData : MemberData, IEventData, IVariableDeclarator
	{
		/// <summary>
		/// Contains optional data that can be passed to a <see cref="FieldData"/>.
		/// </summary>
		public new class Properties : Properties<ILocalSymbol>, IVariableDeclaratorProperties
		{
			/// <inheritdoc cref="EventData.BackingField"/>
			public DefaultedValue<ISymbolOrMember<IFieldSymbol, IFieldData>> BackingField { get; set; }

			/// <inheritdoc cref="EventData.Index"/>
			public int? Index { get; set; }

			/// <inheritdoc cref="EventData.IsDefaultImplementation"/>
			public bool? IsDefaultImplementation { get; set; }

			/// <inheritdoc cref="EventData.ExplicitInterfaceImplementation"/>
			public DefaultedValue<ISymbolOrMember<IEventSymbol, IEventData>> ExplicitInterfaceImplementation { get; set; }

			/// <inheritdoc cref="EventData.ImplicitInterfaceImplementations"/>
			public DefaultedValue<ISymbolContainer<IEventSymbol, IEventData>> ImplicitInterfaceImplementations { get; set; }

			/// <inheritdoc cref="EventData.OverriddenEvent"/>
			public DefaultedValue<ISymbolOrMember<IEventSymbol, IEventData>> OverriddenEvent { get; set; }

			/// <inheritdoc cref="EventData.OverriddenEvents"/>
			public DefaultedValue<ISymbolContainer<IEventSymbol, IEventData>> OverriddenEvents { get; set; }

			/// <inheritdoc cref="LocalData.Variable"/>
			public DefaultedValue<VariableDeclaratorSyntax> Variable { get; set; }

			VariableDeclaratorSyntax? IVariableDeclaratorProperties.Variable
			{
				get => Variable.Value;
				set => Variable = value;
			}

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
				properties.BackingField = BackingField;
				properties.OverriddenEvent = OverriddenEvent;
				properties.OverriddenEvents = OverriddenEvents;
				properties.IsDefaultImplementation = IsDefaultImplementation;
				properties.ImplicitInterfaceImplementations = ImplicitInterfaceImplementations;
				properties.ExplicitInterfaceImplementation = ExplicitInterfaceImplementation;
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

			void IVariableDeclaratorProperties.FillWithDefaultData()
			{
				FillWithDefaultData();
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
				IsPartial = false;
			}
		}

		private sealed class EventDataWrapper : DataHelpers.VariableDataWrapper<IEventSymbol, EventData>
		{
			public EventDataWrapper(EventData parentEvent, int index) : base(parentEvent, index)
			{
			}

			protected override EventData CreateData(IVariableDeclaratorProperties props)
			{
				if (Parent.AsField is not null)
				{
					return new EventData(Parent.AsField, Parent.ParentCompilation, props as Properties);
				}

				return new EventData(Parent.AsProperty!, Parent.ParentCompilation, props as Properties);
			}

			protected override VariableDeclarationSyntax GetVariableDeclaration()
			{
				return Parent.AsField!.Declaration;
			}
		}

		private DefaultedValue<ISymbolOrMember<IFieldSymbol, IFieldData>> _backingField;
		private DefaultedValue<ISymbolOrMember<IEventSymbol, IEventData>> _overriddenEvent;
		private ISymbolContainer<IEventSymbol, IEventData>? _overriddenEvents;
		private bool? _isDefaultImplementation;
		private DefaultedValue<ISymbolOrMember<IEventSymbol, IEventData>> _explicitImplementation;
		private ISymbolContainer<IEventSymbol, IEventData>? _implicitImplementations;

		/// <summary>
		/// Returns the <see cref="Declaration"/> as a <see cref="EventFieldDeclarationSyntax"/>.
		/// </summary>
		public EventFieldDeclarationSyntax? AsField => Declaration as EventFieldDeclarationSyntax;

		/// <summary>
		/// Returns the <see cref="Declaration"/> as a <see cref="EventDeclarationSyntax"/>.
		/// </summary>
		public EventDeclarationSyntax? AsProperty => Declaration as EventDeclarationSyntax;

		/// <inheritdoc/>
		public ISymbolOrMember<IFieldSymbol, IFieldData>? BackingField
		{
			get
			{
				if (_backingField.IsDefault)
				{
					_backingField = AsField is null ? new(null) : new(Symbol.GetBackingField()?.ToDataOrSymbol(ParentCompilation));
				}

				return _backingField.Value;
			}
		}

		/// <summary>
		/// Target <see cref="MemberDeclarationSyntax"/>.
		/// </summary>
		public new MemberDeclarationSyntax Declaration => (base.Declaration as MemberDeclarationSyntax)!;

		/// <inheritdoc/>
		public ISymbolOrMember<IEventSymbol, IEventData>? ExplicitInterfaceImplementation
		{
			get
			{
				if (_explicitImplementation.IsDefault)
				{
					_explicitImplementation = new(Symbol.ExplicitInterfaceImplementations.FirstOrDefault()?.ToDataOrSymbol(ParentCompilation));
				}

				return _explicitImplementation.Value;
			}
		}

		/// <inheritdoc/>
		public ISymbolContainer<IEventSymbol, IEventData> ImplicitInterfaceImplementations
		{
			get
			{
				return _implicitImplementations ??= SymbolExtensions.GetImplicitImplementations_Internal(Symbol, intf =>
				{
					return intf
						.ToDataOrSymbol(ParentCompilation)
						.Member
						.GetMembers(IncludedMembers.Direct)
						.AsEnumerable()
						.Select(s => s.Symbol)
						.OfType<IEventSymbol>()
						.Where(m => m.Name == Symbol.Name);
				})
				.ToContainer(ParentCompilation);
			}
		}

		/// <summary>
		/// Index of this field in the <see cref="Declaration"/>.
		/// </summary>
		public int Index { get; private set; }

		/// <inheritdoc/>
		public bool IsDefaultImplementation => _isDefaultImplementation ??= Symbol.IsDefaultImplementation();

		/// <inheritdoc/>
		public ISymbolOrMember<IEventSymbol, IEventData>? OverriddenEvent
		{
			get
			{
				if (_overriddenEvent.IsDefault)
				{
					if (Symbol.OverriddenEvent is null)
					{
						_overriddenEvent = null;
					}
					else if (_overriddenEvents is null)
					{
						_overriddenEvent = new(Symbol.OverriddenEvent.ToDataOrSymbol(ParentCompilation));
					}
					else if (_overriddenEvents.Count > 0)
					{
						_overriddenEvent = new(_overriddenEvents.First(ReturnOrder.ChildToParent));
					}
					else
					{
						_overriddenEvent = null;
					}
				}

				return _overriddenEvent.Value;
			}
		}

		/// <inheritdoc/>
		public ISymbolContainer<IEventSymbol, IEventData> OverriddenEvents
		{
			get
			{
				return _overriddenEvents ??= Symbol.GetOverriddenSymbols().ToContainer(ParentCompilation);
			}
		}

		/// <summary>
		/// <see cref="IEventSymbol"/> associated with the <see cref="EventFieldDeclarationSyntax"/> or <see cref="EventDeclarationSyntax"/>.
		/// </summary>
		public new IEventSymbol Symbol => (base.Symbol as IEventSymbol)!;

		/// <summary>
		/// <see cref="VariableDeclaratorSyntax"/> used to declare this event field. Equivalent to using <c>AsField.Declaration.Variables[Index]</c>.
		/// </summary>
		public VariableDeclaratorSyntax? Variable { get; private set; }

		EventFieldDeclarationSyntax IVariableDeclarator<EventFieldDeclarationSyntax>.Declaration
		{
			get
			{
				if (AsField is null)
				{
					throw new InvalidOperationException("Current instance does not represent a field-line event");
				}

				return AsField;
			}
		}

		IEventData ISymbolOrMember<IEventSymbol, IEventData>.Member => this;

		VariableDeclaratorSyntax IVariableDeclarator<EventFieldDeclarationSyntax>.Variable
		{
			get
			{
				if (Variable is null)
				{
					throw new InvalidOperationException("Current instance does not represent a field-line event");
				}

				return Variable;
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="EventData"/> class.
		/// </summary>
		/// <param name="declaration"><see cref="EventDeclarationSyntax"/> this <see cref="EventData"/> represents.</param>
		/// <param name="compilation">Parent <see cref="ICompilationData"/> of this <see cref="EventData"/>.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="declaration"/> is <see langword="null"/>. -or- <paramref name="compilation"/> is <see langword="null"/>
		/// </exception>
		public EventData(EventDeclarationSyntax declaration, ICompilationData compilation) : base(declaration, compilation)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="EventData"/> class.
		/// </summary>
		/// <param name="declaration"><see cref="EventDeclarationSyntax"/> this <see cref="EventData"/> represents.</param>
		/// <param name="compilation">Parent <see cref="ICompilationData"/> of this <see cref="EventData"/>.</param>
		/// <param name="properties"><see cref="Properties"/> to use for the current instance.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="declaration"/> is <see langword="null"/>. -or- <paramref name="compilation"/> is <see langword="null"/>
		/// </exception>
		public EventData(EventDeclarationSyntax declaration, ICompilationData compilation, Properties? properties) : base(declaration, compilation, properties)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="EventData"/> class.
		/// </summary>
		/// <param name="declaration"><see cref="EventFieldDeclarationSyntax"/> this <see cref="EventData"/> represents.</param>
		/// <param name="compilation">Parent <see cref="ICompilationData"/> of this <see cref="EventData"/>.</param>
		/// <param name="index">Index of this field in the <paramref name="declaration"/>.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="declaration"/> is <see langword="null"/>. -or- <paramref name="compilation"/> is <see langword="null"/>
		/// </exception>
		/// <exception cref="IndexOutOfRangeException">
		/// <paramref name="index"/> was out of range.
		/// </exception>
		public EventData(EventFieldDeclarationSyntax declaration, ICompilationData compilation, int index)
			: this(declaration, compilation, new Properties { Index = index })
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="EventData"/> class.
		/// </summary>
		/// <param name="declaration"><see cref="EventFieldDeclarationSyntax"/> this <see cref="EventData"/> represents.</param>
		/// <param name="compilation">Parent <see cref="ICompilationData"/> of this <see cref="EventData"/>.</param>
		/// <param name="properties"><see cref="Properties"/> to use for the current instance.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="declaration"/> is <see langword="null"/>. -or- <paramref name="compilation"/> is <see langword="null"/>
		/// </exception>
		/// <exception cref="IndexOutOfRangeException">
		/// <see cref="Properties.Index"/> was out of range.
		/// </exception>
		public EventData(EventFieldDeclarationSyntax declaration, ICompilationData compilation, Properties? properties)
			: base(declaration, compilation, DataHelpers.EnsureValidDeclaratorProperties<Properties>(declaration, compilation, properties))
		{
		}

		internal EventData(IEventSymbol symbol, ICompilationData compilation, MemberData.Properties? properties = default) : base(symbol, compilation, properties)
		{
		}

		/// <inheritdoc cref="MemberData.Clone"/>
		public new EventData Clone()
		{
			return (CloneCore() as EventData)!;
		}

		/// <inheritdoc cref="MemberData.GetProperties"/>
		public new Properties GetProperties()
		{
			return (GetPropertiesCore() as Properties)!;
		}

		/// <summary>
		/// Returns a collection of <see cref="IEventSymbol"/>s of all variables defined in the <see cref="Declaration"/>.
		/// </summary>
		public IEnumerable<ISymbolOrMember<IEventSymbol, IEventData>> GetUnderlayingEvents()
		{
			if (AsField is null)
			{
				return Array.Empty<ISymbolOrMember<IEventSymbol, IEventData>>();
			}

			return Yield();

			IEnumerable<ISymbolOrMember<IEventSymbol, IEventData>> Yield()
			{
				int index = Index;
				int length = AsField.Declaration.Variables.Count;

				for (int i = 0; i < index; i++)
				{
					yield return new EventDataWrapper(this, i);
				}

				for (int i = index + 1; i < length; i++)
				{
					yield return new EventDataWrapper(this, i);
				}
			}
		}

		/// <inheritdoc cref="MemberData.Map(MemberData.Properties)"/>
		public virtual void Map(Properties properties)
		{
			base.Map(properties);
			properties.Variable = Variable;
			properties.Index = Index;
			properties.BackingField = _backingField;
			properties.OverriddenEvent = _overriddenEvent;
			properties.OverriddenEvents = DataHelpers.ToDefaultedValue(_overriddenEvents);
			properties.ImplicitInterfaceImplementations = DataHelpers.ToDefaultedValue(_implicitImplementations);
			properties.ExplicitInterfaceImplementation = _explicitImplementation;
			properties.IsDefaultImplementation = _isDefaultImplementation;
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

		IVariableDeclaratorProperties IVariableDeclarator.GetProperties()
		{
			return GetProperties();
		}

		IEnumerable<ISymbolOrMember<IEventSymbol, IEventData>> IEventData.GetUnderlayingEvents()
		{
			return GetUnderlayingEvents();
		}

		/// <inheritdoc/>
		protected override MemberData CloneCore()
		{
			if (AsField is not null)
			{
				return new EventData(AsField, ParentCompilation, GetProperties());
			}

			return new EventData(AsProperty!, ParentCompilation, GetProperties());
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
				_backingField = props.BackingField;
				_overriddenEvent = props.OverriddenEvent;
				_overriddenEvents = DataHelpers.FromDefaultedOrEmpty(props.OverriddenEvents);
				_isDefaultImplementation = props.IsDefaultImplementation;
				_explicitImplementation = props.ExplicitInterfaceImplementation;
				_implicitImplementations = DataHelpers.FromDefaultedOrEmpty(props.ImplicitInterfaceImplementations);

				if (props.Variable.IsDefault && AsField is not null)
				{
					Variable = AsField.GetVariable(Index);
				}
				else
				{
					Variable = props.Variable.Value;
				}
			}
		}

		IEventData IEventData.Clone()
		{
			return Clone();
		}
	}
}
