// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.ComponentModel;
using System.Linq;
using Durian.Analysis.Extensions;
using Durian.Analysis.SymbolContainers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.Analysis.Data.FromSource
{
	/// <inheritdoc cref="IInterfaceData"/>
	public class InterfaceData : TypeData<InterfaceDeclarationSyntax>, IInterfaceData
	{
		/// <summary>
		/// Contains optional data that can be passed to a <see cref="InterfaceData"/>.
		/// </summary>
		public new class Properties : TypeData<InterfaceDeclarationSyntax>.Properties
		{
			/// <inheritdoc cref="InterfaceData.DefaultImplementations"/>
			public DefaultedValue<ISymbolContainer<ISymbol, IMemberData>> DefaultImplementations { get; set; }

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
				properties.DefaultImplementations = DefaultImplementations;
			}

			/// <inheritdoc/>
#pragma warning disable CS0809 // Obsolete member overrides non-obsolete member
			[Obsolete("Use Map(Properties) instead")]
			[EditorBrowsable(EditorBrowsableState.Never)]
			public sealed override void Map(TypeData<InterfaceDeclarationSyntax>.Properties properties)
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
			protected override void FillWithDefaultData()
			{
				Virtuality = Analysis.Virtuality.Abstract;
				ParameterlessConstructor = null;
				CompilerCondition = null;
			}

			/// <inheritdoc/>
			protected override MemberData.Properties CloneCore()
			{
				Properties properties = new();
				Map(properties);
				return properties;
			}
		}

		IInterfaceData ISymbolOrMember<INamedTypeSymbol, IInterfaceData>.Member => this;

		private ISymbolContainer<ISymbol, IMemberData>? _defaultImplementations;

		/// <inheritdoc/>
		public ISymbolContainer<ISymbol, IMemberData> DefaultImplementations
		{
			get
			{
				return _defaultImplementations ??= GetMembers(IncludedMembers.Inherited)
					.AsEnumerable()
					.Where(m => m.Symbol.IsDefaultImplementation())
					.ToContainer(ParentCompilation);
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="InterfaceData"/> class.
		/// </summary>
		/// <param name="declaration"><see cref="InterfaceDeclarationSyntax"/> this <see cref="InterfaceData"/> represents.</param>
		/// <param name="compilation">Parent <see cref="ICompilationData"/> of this <see cref="InterfaceData"/>.</param>
		/// <param name="properties"><see cref="Properties"/> to use for the current instance.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="declaration"/> is <see langword="null"/>. -or- <paramref name="compilation"/> is <see langword="null"/>
		/// </exception>
		public InterfaceData(InterfaceDeclarationSyntax declaration, ICompilationData compilation, Properties? properties = default) : base(declaration, compilation, properties)
		{
		}

		internal InterfaceData(INamedTypeSymbol symbol, ICompilationData compilation, MemberData.Properties? properties = default) : base(symbol, compilation, properties)
		{
		}

		/// <inheritdoc cref="MemberData.Clone"/>
		public new InterfaceData Clone()
		{
			return (CloneCore() as InterfaceData)!;
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
			properties.DefaultImplementations = DataHelpers.ToDefaultedValue(_defaultImplementations);
		}

		/// <inheritdoc/>
#pragma warning disable CS0809 // Obsolete member overrides non-obsolete member
		[Obsolete("Use Map(Properties) instead")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public sealed override void Map(TypeData<InterfaceDeclarationSyntax>.Properties properties)
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
			return new InterfaceData(Declaration, ParentCompilation, GetProperties());
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
				_defaultImplementations = DataHelpers.FromDefaultedOrEmpty(props.DefaultImplementations);
			}
		}

		IInterfaceData IInterfaceData.Clone()
		{
			return Clone();
		}
	}
}
