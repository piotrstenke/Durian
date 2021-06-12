// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Durian.Generator.Data;
using Durian.Info;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Durian.Generator
{
	/// <summary>
	/// A <see cref="CompilationData"/> with <see cref="INamedTypeSymbol"/>s for all types in the Durian.Generator namespace.
	/// </summary>
	public sealed class CompilationWithImportedTypes : CompilationData
	{
		private readonly string _enableModuleAttributeName = typeof(EnableModuleAttribute).ToString();
		private INamedTypeSymbol[] _allSymbols;
		private bool[] _enabledOrDisabled;
		private DurianModule[][] _symbolModules;

		/// <summary>
		/// Represents the <see cref="Generator.EnableModuleAttribute"/>.
		/// </summary>
		public INamedTypeSymbol? EnableModuleAttribute { get; private set; }

		/// <inheritdoc cref="CompilationData.HasErrors"/>
		[MemberNotNullWhen(false, nameof(EnableModuleAttribute))]
		public override bool HasErrors => EnableModuleAttribute is null;

		/// <summary>
		/// Initializes a new instance of the <see cref="CompilationWithImportedTypes"/>
		/// </summary>
		/// <param name="compilation"><see cref="CSharpCompilation"/> to get the <see cref="INamedTypeSymbol"/>s from.</param>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

		public CompilationWithImportedTypes(CSharpCompilation compilation) : base(compilation)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
		{
			Reset();
		}

		/// <summary>
		/// Returns an array of <see cref="INamedTypeSymbol"/>s representing all types that are part of any Durian module.
		/// </summary>
		public INamedTypeSymbol[] GetAllSymbols()
		{
			return _allSymbols;
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

			int length = _allSymbols.Length;
			List<INamedTypeSymbol> list = new(length);

			for (int i = 0; i < length; i++)
			{
				if (!_enabledOrDisabled[i])
				{
					list.Add(_allSymbols[i]);
				}
			}

			return list.ToArray();
		}

		/// <summary>
		/// Returns an array of <see cref="INamedTypeSymbol"/>s representing all types that are part of any enabled Durian module.
		/// </summary>
		public INamedTypeSymbol[] GetEnabledSymbols()
		{
			if (HasErrors)
			{
				return Array.Empty<INamedTypeSymbol>();
			}

			int length = _allSymbols.Length;
			List<INamedTypeSymbol> list = new(length);

			for (int i = 0; i < length; i++)
			{
				if (_enabledOrDisabled[i])
				{
					list.Add(_allSymbols[i]);
				}
			}

			return list.ToArray();
		}

		/// <summary>
		/// Checks if the specified <paramref name="symbol"/> is part of any enabled Durian module.
		/// </summary>
		/// <param name="symbol"><see cref="INamedTypeSymbol"/> to check if is part of any enabled Durian module.</param>
		/// <exception cref="ArgumentNullException"><paramref name="symbol"/> is <see langword="null"/>.</exception>
		public bool IsDisabled(INamedTypeSymbol symbol)
		{
			return IsDisabled(symbol, out _);
		}

		/// <summary>
		/// Checks if the specified <paramref name="symbol"/> is part of any enabled Durian module.
		/// </summary>
		/// <param name="symbol"><see cref="INamedTypeSymbol"/> to check if is part of any enabled Durian module.</param>
		/// <param name="module"><see cref="DurianModule"/> this <paramref name="symbol"/> is part of.</param>
		/// <exception cref="ArgumentNullException"><paramref name="symbol"/> is <see langword="null"/>.</exception>
		public bool IsDisabled(INamedTypeSymbol symbol, out DurianModule module)
		{
			if (symbol is null)
			{
				throw new ArgumentNullException(nameof(symbol));
			}

			if (HasErrors)
			{
				module = DurianModule.None;
				return false;
			}

			int length = _allSymbols.Length;

			for (int i = 0; i < length; i++)
			{
				if (SymbolEqualityComparer.Default.Equals(_allSymbols[i], symbol))
				{
					module = _symbolModules[i][0];
					return !_enabledOrDisabled[i];
				}
			}

			module = DurianModule.None;
			return false;
		}

		/// <summary>
		/// Checks if the specified <paramref name="symbol"/> is part of any Durian module and whether that module is disabled.
		/// </summary>
		/// <param name="symbol"><see cref="INamedTypeSymbol"/> to check if is part of any enabled Durian module.</param>
		/// <exception cref="ArgumentNullException"><paramref name="symbol"/> is <see langword="null"/>.</exception>
		/// <returns>Two <see cref="bool"/> values indicating whether the <paramref name="symbol"/> is a Durian type (1) and if is enabled (2).</returns>
		public (bool isDurianType, bool isEnabled) IsDisabledDurianType(INamedTypeSymbol symbol)
		{
			return IsDisabledDurianType(symbol, out _);
		}

		/// <summary>
		/// Checks if the specified <paramref name="symbol"/> is part of any Durian module and whether that module is disabled.
		/// </summary>
		/// <param name="symbol"><see cref="INamedTypeSymbol"/> to check if is part of any enabled Durian module.</param>
		/// <param name="module"><see cref="DurianModule"/> this <paramref name="symbol"/> is part of.</param>
		/// <exception cref="ArgumentNullException"><paramref name="symbol"/> is <see langword="null"/>.</exception>
		/// <returns>Two <see cref="bool"/> values indicating whether the <paramref name="symbol"/> is a Durian type (1) and if is enabled (2).</returns>
		public (bool isDurianType, bool isEnabled) IsDisabledDurianType(INamedTypeSymbol symbol, out DurianModule module)
		{
			if (symbol is null)
			{
				throw new ArgumentNullException(nameof(symbol));
			}

			int length = _allSymbols.Length;

			for (int i = 0; i < length; i++)
			{
				if (SymbolEqualityComparer.Default.Equals(symbol, _allSymbols[i]))
				{
					module = _symbolModules[i][0];
					return (true, !_enabledOrDisabled[i]);
				}
			}

			module = DurianModule.None;
			return (false, false);
		}

		/// <summary>
		/// Determines whether the specified <paramref name="symbol"/> is part of any Durian module.
		/// </summary>
		/// <param name="symbol"><see cref="INamedTypeSymbol"/> to check if is part of any Durian module.</param>
		/// <exception cref="ArgumentNullException"><paramref name="symbol"/> is <see langword="null"/>.</exception>
		public bool IsDurianType(INamedTypeSymbol symbol)
		{
			if (symbol is null)
			{
				throw new ArgumentNullException(nameof(symbol));
			}

			if (symbol.ContainingNamespace is not null && !symbol.ContainingNamespace.Name.StartsWith("Durian"))
			{
				return false;
			}

			foreach (INamedTypeSymbol type in _allSymbols)
			{
				if (SymbolEqualityComparer.Default.Equals(type, symbol))
				{
					return true;
				}
			}

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

			if (HasErrors)
			{
				return false;
			}

			int length = _allSymbols.Length;

			for (int i = 0; i < length; i++)
			{
				if (SymbolEqualityComparer.Default.Equals(_allSymbols[i], symbol))
				{
					return _enabledOrDisabled[i];
				}
			}

			return false;
		}

		/// <summary>
		/// Checks if the specified <paramref name="symbol"/> is part of any enabled Durian module.
		/// </summary>
		/// <param name="symbol"><see cref="INamedTypeSymbol"/> to check if is part of any enabled Durian module.</param>
		/// <param name="module"><see cref="DurianModule"/> this <paramref name="symbol"/> is part of.</param>
		/// <exception cref="ArgumentNullException"><paramref name="symbol"/> is <see langword="null"/>.</exception>
		public bool IsEnabled(INamedTypeSymbol symbol, out DurianModule module)
		{
			if (symbol is null)
			{
				throw new ArgumentNullException(nameof(symbol));
			}

			if (HasErrors)
			{
				module = DurianModule.None;
				return false;
			}

			int length = _allSymbols.Length;

			for (int i = 0; i < length; i++)
			{
				if (SymbolEqualityComparer.Default.Equals(_allSymbols[i], symbol))
				{
					module = _symbolModules[i][0];
					return _enabledOrDisabled[i];
				}
			}

			module = DurianModule.None;
			return false;
		}

		/// <summary>
		/// Checks if the specified <paramref name="symbol"/> is part of any Durian module and whether that module is enabled.
		/// </summary>
		/// <param name="symbol"><see cref="INamedTypeSymbol"/> to check if is part of any enabled Durian module.</param>
		/// <exception cref="ArgumentNullException"><paramref name="symbol"/> is <see langword="null"/>.</exception>
		/// <returns>Two <see cref="bool"/> values indicating whether the <paramref name="symbol"/> is a Durian type (1) and if is enabled (2).</returns>
		public (bool isDurianType, bool isEnabled) IsEnabledDurianType(INamedTypeSymbol symbol)
		{
			return IsEnabledDurianType(symbol, out _);
		}

		/// <summary>
		/// Checks if the specified <paramref name="symbol"/> is part of any Durian module and whether that module is enabled.
		/// </summary>
		/// <param name="symbol"><see cref="INamedTypeSymbol"/> to check if is part of any enabled Durian module.</param>
		/// <param name="module"><see cref="DurianModule"/> this <paramref name="symbol"/> is part of.</param>
		/// <exception cref="ArgumentNullException"><paramref name="symbol"/> is <see langword="null"/>.</exception>
		/// <returns>Two <see cref="bool"/> values indicating whether the <paramref name="symbol"/> is a Durian type (1) and if is enabled (2).</returns>
		public (bool isDurianType, bool isEnabled) IsEnabledDurianType(INamedTypeSymbol symbol, out DurianModule module)
		{
			if (symbol is null)
			{
				throw new ArgumentNullException(nameof(symbol));
			}

			int length = _allSymbols.Length;

			for (int i = 0; i < length; i++)
			{
				if (SymbolEqualityComparer.Default.Equals(symbol, _allSymbols[i]))
				{
					module = _symbolModules[i][0];
					return (true, _enabledOrDisabled[i]);
				}
			}

			module = DurianModule.None;
			return (false, false);
		}

		/// <summary>
		/// Resets <see cref="INamedTypeSymbol"/>s from the Durian.Generator namespace.
		/// </summary>
		public void Reset()
		{
			SetAttributeSymbol();

			if (HasErrors)
			{
				_allSymbols = Array.Empty<INamedTypeSymbol>();
				_enabledOrDisabled = Array.Empty<bool>();
				_symbolModules = Array.Empty<DurianModule[]>();
				return;
			}

			ModuleIdentity[] allModules = ModuleIdentity.GetAllModules();
			ModuleIdentity[] enabledModules = ModuleUtilities.GetEnabledModules(Compilation.Assembly, EnableModuleAttribute, allModules);

			SetTypes(allModules, enabledModules);
		}

		private void SetAttributeSymbol()
		{
			EnableModuleAttribute = Compilation.GetTypeByMetadataName(_enableModuleAttributeName);
		}

		private void SetTypes(ModuleIdentity[] allModules, ModuleIdentity[] enabledModules)
		{
			TypeIdentity[] allTypes = TypeIdentity.GetAllTypes(allModules);
			List<TypeIdentity> typeList = new(allTypes.Length);
			List<INamedTypeSymbol> symbolList = new(allTypes.Length);

			foreach (TypeIdentity type in allTypes)
			{
				if (Compilation.GetTypeByMetadataName(type.FullyQualifiedName) is INamedTypeSymbol symbol)
				{
					typeList.Add(type);
					symbolList.Add(symbol);
				}
			}

			int numTypes = typeList.Count;
			DurianModule[][] modules = new DurianModule[numTypes][];
			_enabledOrDisabled = new bool[numTypes];

			for (int i = 0; i < numTypes; i++)
			{
				ImmutableArray<ModuleReference> references = typeList[i].Modules;

				int numReferences = references.Length;
				DurianModule[] current = new DurianModule[numReferences];

				for (int j = 0; j < numReferences; j++)
				{
					current[j] = references[j].EnumValue;
				}

				modules[i] = current;
				_enabledOrDisabled[i] = IsEnabled(references, enabledModules);
			}

			_symbolModules = modules;
			_allSymbols = symbolList.ToArray();

			static bool IsEnabled(ImmutableArray<ModuleReference> references, ModuleIdentity[] enabledModules)
			{
				foreach (ModuleIdentity module in enabledModules)
				{
					foreach (ModuleReference reference in references)
					{
						if (reference.EnumValue == module.Module)
						{
							return true;
						}
					}
				}

				return false;
			}
		}
	}
}
