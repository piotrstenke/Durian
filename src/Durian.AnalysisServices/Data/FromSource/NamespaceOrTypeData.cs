// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.ComponentModel;
using System.Linq;
using Durian.Analysis.Extensions;
using Durian.Analysis.SymbolContainers;
using Durian.Analysis.SymbolContainers.Specialized;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.Analysis.Data.FromSource
{
	/// <inheritdoc cref="INamespaceOrTypeData"/>
	public class NamespaceOrTypeData : MemberData, INamespaceOrTypeData
	{
		/// <summary>
		/// Contains optional data that can be passed to a <see cref="NamespaceOrTypeData"/>.
		/// </summary>
		public new class Properties : Properties<INamespaceOrTypeSymbol>
		{
			/// <summary>
			/// Inner types of the current symbol.
			/// </summary>
			public DefaultedValue<ILeveledSymbolContainer<INamedTypeSymbol, ITypeData>> Types { get; set; }

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
				properties.Types = Types;
			}

			/// <inheritdoc/>
#pragma warning disable CS0809 // Obsolete member overrides non-obsolete member
			[Obsolete("Use Map(Properties) instead")]
			[EditorBrowsable(EditorBrowsableState.Never)]
			public sealed override void Map(Properties<INamespaceOrTypeSymbol> properties)
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

		private ILeveledSymbolContainer<INamedTypeSymbol, ITypeData>? _types;

		/// <summary>
		/// Returns the <see cref="MemberData.Declaration"/> as a <see cref="DelegateDeclarationSyntax"/>.
		/// </summary>
		public DelegateDeclarationSyntax? AsDelegate => (Declaration as DelegateDeclarationSyntax)!;

		/// <summary>
		/// Returns the <see cref="MemberData.Declaration"/> as a <see cref="BaseNamespaceDeclarationSyntax"/>.
		/// </summary>
		public BaseNamespaceDeclarationSyntax? AsNamespace => (Declaration as BaseNamespaceDeclarationSyntax)!;

		/// <summary>
		/// Returns the <see cref="MemberData.Declaration"/> as a <see cref="BaseTypeDeclarationSyntax"/>.
		/// </summary>
		public BaseTypeDeclarationSyntax? AsType => (Declaration as BaseTypeDeclarationSyntax)!;

		/// <summary>
		/// <see cref="INamespaceOrTypeSymbol"/> associated with the <see cref="MemberData.Declaration"/>.
		/// </summary>
		public new INamespaceOrTypeSymbol Symbol => (base.Symbol as INamespaceOrTypeSymbol)!;

		INamespaceOrTypeData ISymbolOrMember<INamespaceOrTypeSymbol, INamespaceOrTypeData>.Member => this;

		/// <summary>
		/// Initializes a new instance of the <see cref="NamespaceOrTypeData"/> class.
		/// </summary>
		/// <param name="declaration"><see cref="BaseTypeDeclarationSyntax"/> this <see cref="NamespaceOrTypeData"/> represents.</param>
		/// <param name="compilation">Parent <see cref="ICompilationData"/> of this <see cref="NamespaceOrTypeData"/>.</param>
		/// <param name="properties"><see cref="Properties"/> to use for the current instance.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="declaration"/> is <see langword="null"/>. -or- <paramref name="compilation"/> is <see langword="null"/>.
		/// </exception>
		public NamespaceOrTypeData(BaseTypeDeclarationSyntax declaration, ICompilationData compilation, Properties? properties = default) : base(declaration, compilation, properties)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="NamespaceOrTypeData"/> class.
		/// </summary>
		/// <param name="declaration"><see cref="BaseNamespaceDeclarationSyntax"/> this <see cref="NamespaceOrTypeData"/> represents.</param>
		/// <param name="compilation">Parent <see cref="ICompilationData"/> of this <see cref="NamespaceOrTypeData"/>.</param>
		/// <param name="properties"><see cref="Properties"/> to use for the current instance.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="declaration"/> is <see langword="null"/>. -or- <paramref name="compilation"/> is <see langword="null"/>.
		/// </exception>
		public NamespaceOrTypeData(BaseNamespaceDeclarationSyntax declaration, ICompilationData compilation, Properties? properties = default) : base(declaration, compilation, properties)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="NamespaceOrTypeData"/> class.
		/// </summary>
		/// <param name="declaration"><see cref="DelegateDeclarationSyntax"/> this <see cref="NamespaceOrTypeData"/> represents.</param>
		/// <param name="compilation">Parent <see cref="ICompilationData"/> of this <see cref="NamespaceOrTypeData"/>.</param>
		/// <param name="properties"><see cref="Properties"/> to use for the current instance.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="declaration"/> is <see langword="null"/>. -or- <paramref name="compilation"/> is <see langword="null"/>.
		/// </exception>
		public NamespaceOrTypeData(DelegateDeclarationSyntax declaration, ICompilationData compilation, Properties? properties = default) : base(declaration, compilation, properties)
		{
		}

		internal NamespaceOrTypeData(INamespaceOrTypeSymbol symbol, ICompilationData compilation, MemberData.Properties? properties = default) : base(symbol, compilation, properties)
		{
		}

		/// <inheritdoc cref="MemberData.Clone"/>
		public new NamespaceOrTypeData Clone()
		{
			return (CloneCore() as NamespaceOrTypeData)!;
		}

		/// <inheritdoc cref="MemberData.GetProperties"/>
		public new Properties GetProperties()
		{
			return (GetPropertiesCore() as Properties)!;
		}

		/// <inheritdoc/>
		public ISymbolContainer<INamedTypeSymbol, ITypeData> GetTypes(IncludedMembers members)
		{
			_types ??= new NamespacesOrTypesContainer(this, ParentCompilation).GetTypes();

#pragma warning disable IDE0066 // Convert switch statement to expression
			switch (_types)
			{
				case IMappedSymbolContainer<INamedTypeSymbol, ITypeData, IncludedMembers> mapped:
					return mapped.ResolveLevel(members);

				case ILeveledSymbolContainer<INamedTypeSymbol, ITypeData> leveled:
					return leveled.ResolveLevel((int)members);

				default:
					return _types.ResolveLevel((int)members).OfType<ISymbolOrMember<INamedTypeSymbol, ITypeData>>().ToContainer(ParentCompilation);
			}
#pragma warning restore IDE0066 // Convert switch statement to expression
		}

		/// <inheritdoc cref="MemberData.Map(MemberData.Properties)"/>
		public virtual void Map(Properties properties)
		{
			base.Map(properties);
			properties.Types = DataHelpers.ToDefaultedValue(_types);
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

		/// <summary>
		/// Converts the current object to a <see cref="ITypeData"/>.
		/// </summary>
		/// <exception cref="InvalidOperationException">Symbol is not a namespace.</exception>
		public NamespaceData ToNamespace()
		{
			if (AsNamespace is null)
			{
				throw new InvalidOperationException("Symbol is not a namespace");
			}

			NamespaceData.Properties properties = new();
			base.Map(properties);
			return new(AsNamespace, ParentCompilation, properties);
		}

		/// <summary>
		/// Converts the current object to a <see cref="ITypeData"/>.
		/// </summary>
		/// <exception cref="InvalidOperationException">Symbol is not a type.</exception>
		public ITypeData ToType()
		{
			if (AsType is not null)
			{
				return (Symbol as INamedTypeSymbol)!.ToData(ParentCompilation);
			}
			else if (AsDelegate is not null)
			{
				DelegateData.Properties properties = new();
				base.Map(properties);
				return new DelegateData(AsDelegate, ParentCompilation, properties);
			}
			else
			{
				throw new InvalidOperationException("Symbol is not a type");
			}
		}

		/// <inheritdoc/>
		protected override MemberData CloneCore()
		{
			return Declaration switch
			{
				BaseTypeDeclarationSyntax type => new NamespaceOrTypeData(type, ParentCompilation, GetProperties()),
				BaseNamespaceDeclarationSyntax @namespace => new NamespaceOrTypeData(@namespace, ParentCompilation, GetProperties()),
				DelegateDeclarationSyntax @delegate => new NamespaceOrTypeData(@delegate, ParentCompilation, GetProperties()),
				_ => throw new InvalidOperationException($"Invalid declaration type: '{Declaration.GetType()}'")
			};
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
				_types = DataHelpers.FromDefaultedOrEmpty(props.Types);
			}
		}

		INamespaceData INamespaceOrTypeData.ToNamespace()
		{
			return ToNamespace();
		}

		INamespaceOrTypeData INamespaceOrTypeData.Clone()
		{
			return Clone();
		}
	}
}
