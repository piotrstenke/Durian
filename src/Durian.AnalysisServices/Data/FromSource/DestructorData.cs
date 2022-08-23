// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

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
	/// <inheritdoc cref="IDestructorData"/>
	public class DestructorData : MemberData, IDestructorData
	{
		/// <summary>
		/// Contains optional data that can be passed to a <see cref="DestructorData"/>.
		/// </summary>
		public new class Properties : Properties<IMethodSymbol>
		{
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
				properties.LocalFunctions = LocalFunctions;
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
				Virtuality = Analysis.Virtuality.NotVirtual;
			}
		}

		private MethodStyle? _bodyType;
		private DefaultedValue<SyntaxNode> _body;
		private ILeveledSymbolContainer<IMethodSymbol, ILocalFunctionData>? _localFunctions;

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
		public new DestructorDeclarationSyntax Declaration => (base.Declaration as DestructorDeclarationSyntax)!;

		/// <inheritdoc/>
		public new IMethodSymbol Symbol => (base.Symbol as IMethodSymbol)!;

		string? IMethodData.CompilerCondition => null;

		ISymbolOrMember<IMethodSymbol, IMethodData>? IMethodData.ExplicitInterfaceImplementation => null;

		ISymbolContainer<IMethodSymbol, IMethodData> IMethodData.ImplicitInterfaceImplementations => SymbolContainerFactory.Empty<IMethodSymbol, IMethodData>();

		bool IMethodData.IsDefaultImplementation => false;

		bool IMethodData.IsParameterless => true;

		bool IMethodData.IsModuleInitializer => false;

		ISymbolOrMember<IMethodSymbol, IMethodData>? IMethodData.OverriddenMethod => null;

		ISymbolContainer<IMethodSymbol, IMethodData> IMethodData.OverriddenMethods => SymbolContainerFactory.Empty<IMethodSymbol, IMethodData>();

		ISymbolContainer<IParameterSymbol, IParameterData> IMethodData.Parameters => SymbolContainerFactory.Empty<IParameterSymbol, IParameterData>();

		ISymbolContainer<ITypeSymbol, ITypeData> IGenericMemberData.TypeArguments => SymbolContainerFactory.Empty<ITypeSymbol, ITypeData>();

		ISymbolContainer<ITypeParameterSymbol, ITypeParameterData> IGenericMemberData.TypeParameters => SymbolContainerFactory.Empty<ITypeParameterSymbol, ITypeParameterData>();

		SyntaxNode IMethodData.SafeDeclaration => Declaration;

		BaseMethodDeclarationSyntax IMethodData.Declaration => Declaration;

		IMethodData ISymbolOrMember<IMethodSymbol, IMethodData>.Member => this;

		IDestructorData ISymbolOrMember<IMethodSymbol, IDestructorData>.Member => this;

		/// <summary>
		/// Initializes a new instance of the <see cref="DestructorData"/> class.
		/// </summary>
		/// <param name="declaration"><see cref="DestructorDeclarationSyntax"/> this <see cref="DestructorData"/> represents.</param>
		/// <param name="compilation">Parent <see cref="ICompilationData"/> of this <see cref="DestructorData"/>.</param>
		/// <param name="properties"><see cref="Properties"/> to use for the current instance.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="declaration"/> is <see langword="null"/>. -or- <paramref name="compilation"/> is <see langword="null"/>
		/// </exception>
		public DestructorData(DestructorDeclarationSyntax declaration, ICompilationData compilation, Properties? properties = default) : base(declaration, compilation, properties)
		{
		}

		internal DestructorData(IMethodSymbol symbol, ICompilationData compilation, MemberData.Properties? properties = default) : base(symbol, compilation, properties)
		{
		}

		/// <inheritdoc cref="MemberData.Clone"/>
		public new DestructorData Clone()
		{
			return (CloneCore() as DestructorData)!;
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
			return new DestructorData(Declaration, ParentCompilation, GetProperties());
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
				_localFunctions = DataHelpers.FromDefaultedOrEmpty(props.LocalFunctions);
			}
		}

		IDestructorData IDestructorData.Clone()
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
