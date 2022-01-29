// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Durian.Analysis.Data;
using Durian.Analysis.Extensions;
using Durian.Generator;
using Durian.Info;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Durian.Analysis
{
	/// <summary>
	/// A <see cref="CompilationData"/> with <see cref="INamedTypeSymbol"/>s for all types in the <c>Durian.Core</c> package.
	/// </summary>
	public sealed class CompilationWithImportedTypes : CompilationData
	{
		private sealed class Entry
		{
			public TypeIdentity Identity { get; }

			public bool? IsEnabled { get; set; }

			public INamedTypeSymbol Symbol { get; }

			public Entry(TypeIdentity identity, INamedTypeSymbol symbol)
			{
				Identity = identity;
				Symbol = symbol;
			}
		}

		private readonly TypeIdentity[] _allTypes;

		private readonly string _enableModuleAttributeName = typeof(RegisterDurianModuleAttribute).ToString();

		private readonly List<Entry> _entries;

		private DurianModule[] _enabledModules;

		/// <summary>
		/// Represents the <see cref="Generator.RegisterDurianModuleAttribute"/>. class.
		/// </summary>
		public INamedTypeSymbol? EnableModuleAttribute { get; private set; }

		/// <inheritdoc cref="CompilationData.HasErrors"/>
		[MemberNotNullWhen(false, nameof(EnableModuleAttribute))]
		public override bool HasErrors => EnableModuleAttribute is null;

		/// <summary>
		/// Initializes a new instance of the <see cref="CompilationWithImportedTypes"/>
		/// </summary>
		/// <param name="compilation"><see cref="CSharpCompilation"/> to get the <see cref="INamedTypeSymbol"/>s from.</param>
		/// <exception cref="ArgumentNullException"><paramref name="compilation"/> is <see langword="null"/>.</exception>
		public CompilationWithImportedTypes(CSharpCompilation compilation) : base(compilation)
		{
			_allTypes = TypeIdentity.GetAllTypes().ToArray();
			_entries = new(_allTypes.Length);

			Reset();
		}

		/// <summary>
		/// Returns an array of <see cref="INamedTypeSymbol"/>s representing all types that are part of any Durian module.
		/// </summary>
		public INamedTypeSymbol[] GetAllSymbols()
		{
			INamedTypeSymbol[] symbols = new INamedTypeSymbol[_entries.Count];

			for (int i = 0; i < _entries.Count; i++)
			{
				symbols[i] = _entries[i].Symbol;
			}

			return symbols;
		}

		/// <summary>
		/// Returns an array of <see cref="INamedTypeSymbol"/>s representing all types that are part of any disabled Durian module.
		/// </summary>
		public INamedTypeSymbol[] GetDisabledSymbols()
		{
			if (HasErrors)
			{
				return Array.Empty<INamedTypeSymbol>();
			}

			List<INamedTypeSymbol> symbols = new(_entries.Count);

			foreach (Entry e in _entries)
			{
				if (!IsEnabled(e))
				{
					symbols.Add(e.Symbol);
				}
			}

			return symbols.ToArray();
		}

		/// <summary>
		/// Returns an array of <see cref="INamedTypeSymbol"/>s representing all types that are part of any enabled Durian module.
		/// </summary>
		public INamedTypeSymbol[] GetEnabledSymbols()
		{
			if (HasErrors)
			{
				return GetAllSymbols();
			}

			List<INamedTypeSymbol> symbols = new(_entries.Count);

			foreach (Entry e in _entries)
			{
				if (IsEnabled(e))
				{
					symbols.Add(e.Symbol);
				}
			}

			return symbols.ToArray();
		}

		/// <summary>
		/// Checks if the specified <paramref name="symbol"/> is part of any disabled Durian module.
		/// </summary>
		/// <param name="symbol"><see cref="INamedTypeSymbol"/> to check if is part of any disabled Durian module.</param>
		/// <exception cref="ArgumentNullException"><paramref name="symbol"/> is <see langword="null"/>.</exception>
		public bool IsDisabled(INamedTypeSymbol symbol)
		{
			if (symbol is null)
			{
				throw new ArgumentNullException(nameof(symbol));
			}

			if (!ValidateSymbol(symbol))
			{
				return false;
			}

			foreach (Entry e in _entries)
			{
				if (SymbolEqualityComparer.Default.Equals(e.Symbol, symbol))
				{
					return !IsEnabled(e);
				}
			}

			return false;
		}

		/// <summary>
		/// Checks if the specified <paramref name="symbol"/> is part of any disabled Durian module.
		/// </summary>
		/// <param name="symbol"><see cref="INamedTypeSymbol"/> to check if is part of any disabled Durian module.</param>
		/// <param name="modules">Array of <see cref="DurianModule"/>s representing the modules the <paramref name="symbol"/> is part of.</param>
		/// <exception cref="ArgumentNullException"><paramref name="symbol"/> is <see langword="null"/>.</exception>
		public bool IsDisabled(INamedTypeSymbol symbol, [NotNullWhen(true)] out DurianModule[]? modules)
		{
			if (symbol is null)
			{
				throw new ArgumentNullException(nameof(symbol));
			}

			if (symbol is null)
			{
				throw new ArgumentNullException(nameof(symbol));
			}

			if (!ValidateSymbol(symbol))
			{
				modules = null;
				return false;
			}

			foreach (Entry e in _entries)
			{
				if (SymbolEqualityComparer.Default.Equals(e.Symbol, symbol))
				{
					modules = ModuleConverter.ToEnums(e.Identity.Modules);
					return !IsEnabled(e);
				}
			}

			modules = null;
			return false;
		}

		/// <summary>
		/// Checks if the specified <paramref name="symbol"/> is part of any enabled Durian module.
		/// </summary>
		/// <param name="symbol"><see cref="INamedTypeSymbol"/> to check if is part of any enabled Durian module.</param>
		/// <exception cref="ArgumentNullException"><paramref name="symbol"/> is <see langword="null"/>.</exception>
		public bool IsEnabled(INamedTypeSymbol symbol)
		{
			if (symbol is null)
			{
				throw new ArgumentNullException(nameof(symbol));
			}

			if (!ValidateSymbol(symbol))
			{
				return false;
			}

			foreach (Entry e in _entries)
			{
				if (SymbolEqualityComparer.Default.Equals(e.Symbol, symbol))
				{
					return IsEnabled(e);
				}
			}

			return false;
		}

		/// <summary>
		/// Checks if the specified <paramref name="symbol"/> is part of any enabled Durian module.
		/// </summary>
		/// <param name="symbol"><see cref="INamedTypeSymbol"/> to check if is part of any enabled Durian module.</param>
		/// <param name="modules">Array of <see cref="DurianModule"/>s representing the modules the <paramref name="symbol"/> is part of.</param>
		/// <exception cref="ArgumentNullException"><paramref name="symbol"/> is <see langword="null"/>.</exception>
		public bool IsEnabled(INamedTypeSymbol symbol, [NotNullWhen(true)] out DurianModule[]? modules)
		{
			if (symbol is null)
			{
				throw new ArgumentNullException(nameof(symbol));
			}

			if (!ValidateSymbol(symbol))
			{
				modules = null;
				return false;
			}

			foreach (Entry e in _entries)
			{
				if (SymbolEqualityComparer.Default.Equals(e.Symbol, symbol))
				{
					modules = ModuleConverter.ToEnums(e.Identity.Modules);
					return IsEnabled(e);
				}
			}

			modules = null;
			return false;
		}

		/// <summary>
		/// Determines whether the specified <paramref name="symbol"/> is part of any Durian module.
		/// </summary>
		/// <param name="symbol"><see cref="INamedTypeSymbol"/> to check if is part of any Durian module.</param>
		/// <exception cref="ArgumentNullException"><paramref name="symbol"/> is <see langword="null"/>.</exception>
		public bool IsPartOfAnyModule(INamedTypeSymbol symbol)
		{
			if (symbol is null)
			{
				throw new ArgumentNullException(nameof(symbol));
			}

			if (!ValidateSymbol(symbol))
			{
				return false;
			}

			foreach (Entry e in _entries)
			{
				if (SymbolEqualityComparer.Default.Equals(e.Symbol, symbol))
				{
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Determines whether the specified <paramref name="symbol"/> is part of any Durian module.
		/// </summary>
		/// <param name="symbol"><see cref="INamedTypeSymbol"/> to check if is part of any Durian module.</param>
		/// <param name="modules">Array of <see cref="DurianModule"/>s representing the modules the <paramref name="symbol"/> is part of.</param>
		/// <exception cref="ArgumentNullException"><paramref name="symbol"/> is <see langword="null"/>.</exception>
		public bool IsPartOfAnyModule(INamedTypeSymbol symbol, [NotNullWhen(true)] out DurianModule[]? modules)
		{
			if (symbol is null)
			{
				throw new ArgumentNullException(nameof(symbol));
			}

			if (!ValidateSymbol(symbol))
			{
				modules = null;
				return false;
			}

			foreach (Entry e in _entries)
			{
				if (SymbolEqualityComparer.Default.Equals(e.Symbol, symbol))
				{
					modules = ModuleConverter.ToEnums(e.Identity.Modules);
					return true;
				}
			}

			modules = null;
			return false;
		}

		/// <summary>
		/// Checks if the specified <paramref name="symbol"/> is part of any Durian module. If so, also checks if the module is disabled.
		/// </summary>
		/// <param name="symbol"><see cref="INamedTypeSymbol"/> to check if is part of any Durian module.</param>
		/// <exception cref="ArgumentNullException"><paramref name="symbol"/> is <see langword="null"/>.</exception>
		public (bool isPartOfAnyModule, bool isDisabled) IsPartOfAnyModuleAndDisabled(INamedTypeSymbol symbol)
		{
			if (symbol is null)
			{
				throw new ArgumentNullException(nameof(symbol));
			}

			if (!ValidateSymbol(symbol))
			{
				return (false, false);
			}

			foreach (Entry e in _entries)
			{
				if (SymbolEqualityComparer.Default.Equals(e.Symbol, symbol))
				{
					return (true, !IsEnabled(e));
				}
			}

			return (false, false);
		}

		/// <summary>
		/// Checks if the specified <paramref name="symbol"/> is part of any Durian module. If so, also checks if the module is enabled.
		/// </summary>
		/// <param name="symbol"><see cref="INamedTypeSymbol"/> to check if is part of any Durian module.</param>
		/// <exception cref="ArgumentNullException"><paramref name="symbol"/> is <see langword="null"/>.</exception>
		public (bool isPartOfAnyModule, bool isEnabled) IsPartOfAnyModuleAndEnabled(INamedTypeSymbol symbol)
		{
			if (symbol is null)
			{
				throw new ArgumentNullException(nameof(symbol));
			}

			if (!ValidateSymbol(symbol))
			{
				return (false, false);
			}

			foreach (Entry e in _entries)
			{
				if (SymbolEqualityComparer.Default.Equals(e.Symbol, symbol))
				{
					return (true, IsEnabled(e));
				}
			}

			return (false, false);
		}

		/// <summary>
		/// Resets <see cref="INamedTypeSymbol"/>s from the Durian.Generator namespace.
		/// </summary>
		[MemberNotNull(nameof(_enabledModules))]
		public void Reset()
		{
			EnableModuleAttribute = Compilation.GetTypeByMetadataName(_enableModuleAttributeName);

			if (EnableModuleAttribute is null)
			{
				if (_enabledModules is null || _enabledModules.Length > 0)
				{
					_enabledModules = Array.Empty<DurianModule>();
				}

				return;
			}

			FillWithEntries();
			_enabledModules = Compilation.GetEnabledModules(EnableModuleAttribute).AsEnums();
		}

		private static bool ValidateSymbol(INamedTypeSymbol symbol)
		{
			return symbol.ContainingNamespace is not null && symbol.JoinNamespaces().StartsWith("Durian");
		}

		private void FillWithEntries()
		{
			_entries.Clear();

			foreach (TypeIdentity type in _allTypes)
			{
				if (Compilation.GetTypeByMetadataName(type.FullyQualifiedName) is not INamedTypeSymbol symbol)
				{
					continue;
				}

				Entry entry = new(type, symbol);
				_entries.Add(entry);
			}
		}

		private bool IsEnabled(Entry e)
		{
			if (e.IsEnabled.HasValue)
			{
				return e.IsEnabled.Value;
			}

			foreach (DurianModule module in _enabledModules)
			{
				foreach (ModuleReference reference in e.Identity.Modules)
				{
					if (reference.EnumValue == module)
					{
						e.IsEnabled = true;
						return true;
					}
				}
			}

			e.IsEnabled = false;
			return false;
		}
	}
}