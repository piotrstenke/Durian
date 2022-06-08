// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using Durian.Analysis.Extensions;
using Durian.Analysis.Filtration;
using Durian.Analysis.SymbolContainers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.Analysis.Data
{
	/// <inheritdoc cref="IMemberData"/>
	[DebuggerDisplay("{Symbol}")]
	public class MemberData : IMemberData
	{
		/// <summary>
		/// Contains optional data that can be passed to a <see cref="MemberData"/>.
		/// </summary>
		public class Properties<TSymbol> : Properties where TSymbol : class, ISymbol
		{
			/// <inheritdoc cref="Properties.Symbol"/>
			public new TSymbol? Symbol
			{
				get => (_symbol as TSymbol)!;
				set => _symbol = value;
			}

			/// <summary>
			/// Initializes a new instance of the <see cref="Properties{TSymbol}"/> class.
			/// </summary>
			public Properties()
			{
			}

			private protected override void ValidateSymbol(ISymbol? symbol)
			{
				if(symbol is null)
				{
					return;
				}

				if(symbol is not TSymbol)
				{
					throw new ArgumentException($"Symbol '{symbol}' is not of type '{typeof(TSymbol)}", nameof(symbol));
				}
			}
		}

		/// <summary>
		/// Contains optional data that can be passed to a <see cref="MemberData"/>.
		/// </summary>
		[DebuggerDisplay("{Symbol ?? string.Empty}")]
		public class Properties
		{
			private protected ISymbol? _symbol;

			/// <inheritdoc cref="IMemberData.Symbol"/>
			public ISymbol? Symbol
			{
				get => _symbol;
				set
				{
					ValidateSymbol(value);
					_symbol = value;
				}
			}

			/// <inheritdoc cref="IMemberData.SemanticModel"/>
			public SemanticModel? SemanticModel { get; set; }

			/// <inheritdoc cref="IMemberData.Location"/>
			public Location? Location { get; set; }

			/// <inheritdoc cref="IMemberData.IsNew"/>
			public bool? IsNew { get; set; }

			/// <inheritdoc cref="IMemberData.IsUnsafe"/>
			public bool? IsUnsafe { get; set; }

			/// <inheritdoc cref="IMemberData.IsPartial"/>
			public bool? IsPartial { get; set; }

			/// <inheritdoc cref="IMemberData.Name"/>
			public string? Name { get; set; }

			/// <inheritdoc cref="IMemberData.Virtuality"/>
			public Virtuality? Virtuality { get; set; }

			/// <inheritdoc cref="IMemberData.HiddenSymbol"/>
			public DefaultedValue<ISymbolOrMember> HiddenMember { get; set; }

			/// <summary>
			/// All modifiers of the current symbol.
			/// </summary>
			public string[]? Modifiers { get; set; }

			/// <summary>
			/// All containing types of the current symbol.
			/// </summary>
			public GenericSymbolContainer<INamedTypeSymbol>? ContainingTypes { get; set; }

			/// <summary>
			/// All containing namespaces of the current symbol.
			/// </summary>
			public WritableSymbolContainer<INamespaceSymbol>? ContainingNamespaces { get; set; }

			/// <summary>
			/// All attributes if the current symbol.
			/// </summary>
			public ImmutableArray<AttributeData> Attributes { get; set; }

			/// <summary>
			/// Initializes a new instance of the <see cref="Properties{TSymbol}"/> class.
			/// </summary>
			public Properties()
			{
			}

			private protected virtual void ValidateSymbol(ISymbol? symbol)
			{
				// Do nothing by default.
			}
		}

		private ImmutableArray<AttributeData> _attributes;
		private WritableSymbolContainer<INamespaceSymbol>? _containingNamespaces;
		private GenericSymbolContainer<INamedTypeSymbol>? _containingTypes;
		private bool? _isNew;
		private bool? _isPartial;
		private bool? _isUnsafe;
		private Location? _location;
		private DefaultedValue<ISymbolOrMember> _hiddenMember;
		private string[]? _modifiers;

		/// <inheritdoc/>
		public SyntaxNode Declaration { get; }

		/// <inheritdoc/>
		public bool IsNew => _isNew ??= Symbol.IsNew();

		/// <inheritdoc/>
		public bool IsPartial => _isPartial ??= Symbol.IsPartial();

		/// <inheritdoc/>
		public bool IsUnsafe => _isUnsafe ??= Symbol.IsUnsafe();

		/// <inheritdoc/>
		public Location Location => _location ??= Declaration.GetLocation();

		/// <inheritdoc/>
		public string Name { get; }

		/// <inheritdoc/>
		public Virtuality Virtuality { get; }

		/// <inheritdoc/>
		public ICompilationData ParentCompilation { get; }

		/// <inheritdoc/>
		public SemanticModel SemanticModel { get; }

		/// <inheritdoc/>
		public ISymbol Symbol { get; }

		IMemberData ISymbolOrMember.Member => this;
		bool ISymbolOrMember.HasMember => true;

		/// <summary>
		/// Initializes a new instance of the <see cref="MemberData"/> class.
		/// </summary>
		/// <param name="declaration"><see cref="SyntaxNode"/> this <see cref="MemberData"/> represents.</param>
		/// <param name="compilation">Parent <see cref="ICompilationData"/> of this <see cref="MemberData"/>.</param>
		/// <param name="properties"><see cref="Properties"/> to use for the current instance.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="declaration"/> is <see langword="null"/>. -or- <paramref name="compilation"/> is <see langword="null"/>.
		/// </exception>
		public MemberData(SyntaxNode declaration, ICompilationData compilation, Properties? properties = default)
		{
			if (declaration is null)
			{
				throw new ArgumentNullException(nameof(declaration));
			}

			if (compilation is null)
			{
				throw new ArgumentNullException(nameof(compilation));
			}

			if (properties is null)
			{
				SemanticModel = compilation.Compilation.GetSemanticModel(declaration, out ISymbol? symbol);
				Symbol = symbol;

				Declaration = declaration;
				ParentCompilation = compilation;
				Name = Symbol.GetVerbatimName();
				Virtuality = Symbol.GetVirtuality();
			}
			else
			{
				SemanticModel = properties.SemanticModel ?? compilation.Compilation.GetSemanticModel(declaration);
				Symbol = properties.Symbol ?? SemanticModel.GetSymbol(declaration);

				Declaration = declaration;
				ParentCompilation = compilation;
				Name = properties.Name ?? Symbol.GetVerbatimName();
				Virtuality = properties.Virtuality ?? Symbol.GetVirtuality();

				_attributes = properties.Attributes;
				_containingNamespaces = properties.ContainingNamespaces;
				_containingTypes = properties.ContainingTypes;
				_isNew = properties.IsNew;
				_isPartial = properties.IsPartial;
				_isUnsafe = properties.IsUnsafe;
				_location = properties.Location;
				_modifiers = properties.Modifiers;
				_hiddenMember = properties.HiddenMember;
			}
		}

		internal MemberData(ISymbol symbol, ICompilationData compilation)
		{
			if (symbol.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax() is not SyntaxNode decl)
			{
				throw Exc_NoSyntaxReference(symbol);
			}

			Symbol = symbol;
			Declaration = decl;
			SemanticModel = compilation.Compilation.GetSemanticModel(decl.SyntaxTree);
			ParentCompilation = compilation;
			Name = Symbol.GetVerbatimName();
		}

		/// <inheritdoc/>
		public virtual ImmutableArray<AttributeData> Attributes
		{
			get
			{
				return _attributes.IsDefault ? (_attributes = Symbol.GetAttributes()) : _attributes;
			}
		}

		/// <summary>
		/// Root namespace of the current member (excluding the <see langword="global"/> namespace).
		/// </summary>
		public ISymbolOrMember<INamespaceSymbol> RootNamespace => ContainingNamespaces.First();

		/// <inheritdoc/>
		public WritableSymbolContainer<INamespaceSymbol> ContainingNamespaces
		{
			get
			{
				return _containingNamespaces ??= Symbol.GetContainingNamespaces().ToWritableContainer(ParentCompilation);
			}
		}

		/// <inheritdoc/>
		public GenericSymbolContainer<INamedTypeSymbol> ContainingTypes
		{
			get
			{
				return _containingTypes ??= Symbol.GetContainingTypes().ToWritableContainer(ParentCompilation);
			}
		}

		/// <inheritdoc/>
		public ISymbolOrMember? HiddenSymbol
		{
			get
			{
				if(_hiddenMember.IsDefault)
				{
					_hiddenMember = Symbol.GetHiddenSymbol().ToDataOrSymbolInternal(ParentCompilation);
				}

				return _hiddenMember.Value;
			}
		}

		/// <inheritdoc/>
		public string[] Modifiers
		{
			get
			{
				return _modifiers ??= Symbol.GetModifiers();
			}
		}

		private protected static InvalidOperationException Exc_NoSyntaxReference(ISymbol symbol)
		{
			return new InvalidOperationException($"Symbol '{symbol}' doesn't define any syntax reference, thus can't be used in a {nameof(MemberData)}!");
		}
	}
}
