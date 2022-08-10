// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.ComponentModel;
using System.Linq;
using Durian.Analysis.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.Analysis.Data.FromSource
{
	/// <inheritdoc cref="IRecordData"/>
	public class RecordData : TypeData<RecordDeclarationSyntax>, IRecordData
	{
		/// <summary>
		/// Contains optional data that can be passed to a <see cref="RecordData"/>.
		/// </summary>
		public new class Properties : TypeData<RecordDeclarationSyntax>.Properties
		{
			/// <inheritdoc cref="RecordData.CopyConstructor"/>
			public DefaultedValue<ISymbolOrMember<IMethodSymbol, IConstructorData>> CopyConstructor { get; set; }

			/// <inheritdoc cref="RecordData.ParameterList"/>
			public DefaultedValue<ParameterListSyntax> ParameterList { get; set; }

			/// <inheritdoc cref="RecordData.PrimaryConstructor"/>
			public DefaultedValue<ISymbolOrMember<IMethodSymbol, IConstructorData>> PrimaryConstructor { get; set; }

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
				properties.ParameterList = ParameterList;
				properties.PrimaryConstructor = PrimaryConstructor;
				properties.CopyConstructor = CopyConstructor;
			}

			/// <inheritdoc/>
#pragma warning disable CS0809 // Obsolete member overrides non-obsolete member
			[Obsolete("Use Map(Properties) instead")]
			[EditorBrowsable(EditorBrowsableState.Never)]
			public sealed override void Map(TypeData<RecordDeclarationSyntax>.Properties properties)
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

		IRecordData ISymbolOrMember<INamedTypeSymbol, IRecordData>.Member => this;

		private DefaultedValue<ISymbolOrMember<IMethodSymbol, IConstructorData>> _copyConstructor;
		private DefaultedValue<ParameterListSyntax> _parameterList;
		private DefaultedValue<ISymbolOrMember<IMethodSymbol, IConstructorData>> _primaryConstructor;

		/// <inheritdoc/>
		public ISymbolOrMember<IMethodSymbol, IConstructorData>? CopyConstructor
		{
			get
			{
				if (_copyConstructor.IsDefault)
				{
					_copyConstructor = new(Symbol.GetSpecialConstructor(SpecialConstructor.Copy)?.ToDataOrSymbol<IConstructorData>(ParentCompilation));
				}

				return _copyConstructor.Value;
			}
		}

		/// <inheritdoc/>
		public bool IsClass => Symbol.IsReferenceType;

		/// <inheritdoc/>
		public bool IsStruct => Symbol.IsValueType;

		/// <inheritdoc/>
		public ParameterListSyntax? ParameterList
		{
			get
			{
				if (_parameterList.IsDefault)
				{
					if (Declaration.ParameterList is not null)
					{
						_parameterList = Declaration.ParameterList;
					}
					else
					{
						_parameterList = PartialDeclarations.Select(d => d.ParameterList).FirstOrDefault(d => d is not null);
					}
				}

				return _parameterList.Value;
			}
		}

		/// <inheritdoc/>
		public ISymbolOrMember<IMethodSymbol, IConstructorData>? PrimaryConstructor
		{
			get
			{
				if (_primaryConstructor.IsDefault)
				{
					_primaryConstructor = new(Symbol.GetPrimaryConstructor()?.ToDataOrSymbol<IConstructorData>(ParentCompilation));
				}

				return _primaryConstructor.Value;
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="RecordData"/> class.
		/// </summary>
		/// <param name="declaration"><see cref="RecordDeclarationSyntax"/> this <see cref="RecordData"/> represents.</param>
		/// <param name="compilation">Parent <see cref="ICompilationData"/> of this <see cref="RecordData"/>.</param>
		/// <param name="properties"><see cref="Properties"/> to use for the current instance.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="declaration"/> is <see langword="null"/>. -or- <paramref name="compilation"/> is <see langword="null"/>
		/// </exception>
		public RecordData(RecordDeclarationSyntax declaration, ICompilationData compilation, Properties? properties = default) : base(declaration, compilation, properties)
		{
		}

		internal RecordData(INamedTypeSymbol symbol, ICompilationData compilation, MemberData.Properties? properties = default) : base(symbol, compilation, properties)
		{
		}

		/// <inheritdoc cref="MemberData.Clone"/>
		public new RecordData Clone()
		{
			return (CloneCore() as RecordData)!;
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
			properties.ParameterList = ParameterList;
			properties.CopyConstructor = _copyConstructor;
			properties.PrimaryConstructor = _primaryConstructor;
		}

		/// <inheritdoc/>
#pragma warning disable CS0809 // Obsolete member overrides non-obsolete member
		[Obsolete("Use Map(Properties) instead")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public sealed override void Map(TypeData<RecordDeclarationSyntax>.Properties properties)
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
			return new RecordData(Declaration, ParentCompilation, GetProperties());
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
				_copyConstructor = props.CopyConstructor;
				_primaryConstructor = props.PrimaryConstructor;
				_parameterList = props.ParameterList;
			}
		}

		IRecordData IRecordData.Clone()
		{
			return Clone();
		}
	}
}
