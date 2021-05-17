using System;
using System.Collections.Generic;
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
		private readonly INamedTypeSymbol[] _allSymbols;
		private readonly ModuleIdentity[] _allModules;
		private readonly TypeIdentity[] _allTypes;
		private readonly bool[] _enabledOrDisabled;

		/// <inheritdoc cref="CompilationData.HasErrors"/>
		[MemberNotNullWhen(false, nameof(EnableModuleAttribute))]
		public override bool HasErrors => EnableModuleAttribute is null;

		/// <summary>
		/// Represents the <see cref="Generator.EnableModuleAttribute"/>.
		/// </summary>
		public INamedTypeSymbol? EnableModuleAttribute { get; private set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="CompilationWithImportedTypes"/>
		/// </summary>
		/// <param name="compilation"><see cref="CSharpCompilation"/> to get the <see cref="INamedTypeSymbol"/>s from.</param>
		public CompilationWithImportedTypes(CSharpCompilation compilation) : base(compilation)
		{
			SetAttributeSymbol();

			if (HasErrors)
			{
				_allModules = Array.Empty<ModuleIdentity>();
				_allTypes = Array.Empty<TypeIdentity>();
				_allSymbols = Array.Empty<INamedTypeSymbol>();
				_enabledOrDisabled = Array.Empty<bool>();
			}
			else
			{
				_allModules = ModuleIdentity.GetAllModules();
				ModuleIdentity[] enabledModules = ModuleUtilities.GetEnabledModules(compilation.Assembly, EnableModuleAttribute, _allModules);

				TypeIdentity[] typeArray = TypeIdentity.GetAllTypes();
				List<TypeIdentity> types = new(typeArray.Length);
				List<INamedTypeSymbol> symbols = new(typeArray.Length);

				foreach (TypeIdentity type in typeArray)
				{
					if (Compilation.GetTypeByMetadataName(type.FullyQualifiedName) is INamedTypeSymbol symbol)
					{
						types.Add(type);
						symbols.Add(symbol);
					}
				}

				if (typeArray.Length == types.Count)
				{
					_allTypes = typeArray;
				}
				else
				{
					_allTypes = types.ToArray();
				}

				_allSymbols = symbols.ToArray();
				_enabledOrDisabled = new bool[_allTypes.Length];

				for (int i = 0; i < _allTypes.Length; i++)
				{
					_enabledOrDisabled[i] = IsEnabled(_allTypes[i].Module.Module, enabledModules);
				}
			}

			static bool IsEnabled(DurianModule currentModule, ModuleIdentity[] enabledModules)
			{
				foreach (ModuleIdentity module in enabledModules)
				{
					if (module.Module == currentModule)
					{
						return true;
					}
				}

				return true;
			}
		}

		/// <summary>
		/// Resets <see cref="INamedTypeSymbol"/>s from the Durian.Generator namespace.
		/// </summary>
		public void Reset()
		{
			SetAttributeSymbol();
			INamedTypeSymbol[] newSymbols = ModuleUtilities.GetDurianTypes(Compilation, _allModules);
			int length = newSymbols.Length;

			for (int i = 0; i < length; i++)
			{
				_allSymbols[i] = newSymbols[i];
			}
		}

		/// <summary>
		/// Returns an array of <see cref="INamedTypeSymbol"/>s representing all types that are part of any Durian module.
		/// </summary>
		public INamedTypeSymbol[] GetAllSymbols()
		{
			return _allSymbols;
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
		/// Checks if the specified <paramref name="symbol"/> is part of any Durian module and whether that module is enabled.
		/// </summary>
		/// <param name="symbol"><see cref="INamedTypeSymbol"/> to check if is part of any enabled Durian module.</param>
		/// <exception cref="ArgumentNullException"><paramref name="symbol"/> is <see langword="null"/>.</exception>
		/// <returns>Two <see cref="bool"/> values indicating whether the <paramref name="symbol"/> is a Durian type (1) and if is enabled (2).</returns>
		public (bool isDurianType, bool isEnabled) IsEnabledDurianType(INamedTypeSymbol symbol)
		{
			return IsEnabledDurianType(symbol, out ModuleIdentity _);
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
			(bool isDurianType, bool isEnabled) enabled = IsEnabledDurianType(symbol, out ModuleIdentity? identity);
			module = enabled.isEnabled ? identity!.Module : DurianModule.None;
			return enabled;
		}

		/// <summary>
		/// Checks if the specified <paramref name="symbol"/> is part of any Durian module and whether that module is enabled.
		/// </summary>
		/// <param name="symbol"><see cref="INamedTypeSymbol"/> to check if is part of any enabled Durian module.</param>
		/// <param name="module"><see cref="ModuleIdentity"/> this <paramref name="symbol"/> is part of.</param>
		/// <exception cref="ArgumentNullException"><paramref name="symbol"/> is <see langword="null"/>.</exception>
		/// <returns>Two <see cref="bool"/> values indicating whether the <paramref name="symbol"/> is a Durian type (1) and if is enabled (2).</returns>
		public (bool isDurianType, bool isEnabled) IsEnabledDurianType(INamedTypeSymbol symbol, out ModuleIdentity? module)
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
					module = _allTypes[i].Module;
					return (true, _enabledOrDisabled[i]);
				}
			}

			module = null;
			return (false, false);
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
			bool enabled = IsEnabled(symbol, out ModuleIdentity? identity);
			module = enabled ? identity!.Module : DurianModule.None;
			return enabled;
		}

		/// <summary>
		/// Checks if the specified <paramref name="symbol"/> is part of any enabled Durian module.
		/// </summary>
		/// <param name="symbol"><see cref="INamedTypeSymbol"/> to check if is part of any enabled Durian module.</param>
		/// <param name="module"><see cref="ModuleIdentity"/> this <paramref name="symbol"/> is part of.</param>
		/// <exception cref="ArgumentNullException"><paramref name="symbol"/> is <see langword="null"/>.</exception>
		public bool IsEnabled(INamedTypeSymbol symbol, [NotNullWhen(true)] out ModuleIdentity? module)
		{
			if (symbol is null)
			{
				throw new ArgumentNullException(nameof(symbol));
			}

			if (HasErrors)
			{
				module = null;
				return false;
			}

			int length = _allSymbols.Length;

			for (int i = 0; i < length; i++)
			{
				if (SymbolEqualityComparer.Default.Equals(_allSymbols[i], symbol))
				{
					module = _allTypes[i].Module;
					return _enabledOrDisabled[i];
				}
			}

			module = null;
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
			return IsDisabledDurianType(symbol, out ModuleIdentity _);
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
			(bool isDurianType, bool isEnabled) enabled = IsDisabledDurianType(symbol, out ModuleIdentity? identity);
			module = enabled.isEnabled ? identity!.Module : DurianModule.None;
			return enabled;
		}

		/// <summary>
		/// Checks if the specified <paramref name="symbol"/> is part of any Durian module and whether that module is disabled.
		/// </summary>
		/// <param name="symbol"><see cref="INamedTypeSymbol"/> to check if is part of any enabled Durian module.</param>
		/// <param name="module"><see cref="ModuleIdentity"/> this <paramref name="symbol"/> is part of.</param>
		/// <exception cref="ArgumentNullException"><paramref name="symbol"/> is <see langword="null"/>.</exception>
		/// <returns>Two <see cref="bool"/> values indicating whether the <paramref name="symbol"/> is a Durian type (1) and if is enabled (2).</returns>
		public (bool isDurianType, bool isEnabled) IsDisabledDurianType(INamedTypeSymbol symbol, out ModuleIdentity? module)
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
					module = _allTypes[i].Module;
					return (true, !_enabledOrDisabled[i]);
				}
			}

			module = null;
			return (false, false);
		}

		/// <summary>
		/// Checks if the specified <paramref name="symbol"/> is part of any enabled Durian module.
		/// </summary>
		/// <param name="symbol"><see cref="INamedTypeSymbol"/> to check if is part of any enabled Durian module.</param>
		/// <exception cref="ArgumentNullException"><paramref name="symbol"/> is <see langword="null"/>.</exception>
		public bool IsDisabled(INamedTypeSymbol symbol)
		{
			return IsDisabled(symbol, out ModuleIdentity _);
		}

		/// <summary>
		/// Checks if the specified <paramref name="symbol"/> is part of any enabled Durian module.
		/// </summary>
		/// <param name="symbol"><see cref="INamedTypeSymbol"/> to check if is part of any enabled Durian module.</param>
		/// <param name="module"><see cref="DurianModule"/> this <paramref name="symbol"/> is part of.</param>
		/// <exception cref="ArgumentNullException"><paramref name="symbol"/> is <see langword="null"/>.</exception>
		public bool IsDisabled(INamedTypeSymbol symbol, out DurianModule module)
		{
			bool enabled = IsDisabled(symbol, out ModuleIdentity? identity);
			module = enabled ? identity!.Module : DurianModule.None;
			return enabled;
		}

		/// <summary>
		/// Checks if the specified <paramref name="symbol"/> is part of any disabled Durian module.
		/// </summary>
		/// <param name="symbol"><see cref="INamedTypeSymbol"/> to check if is part of any enabled Durian module.</param>
		/// <param name="module"><see cref="ModuleIdentity"/> this <paramref name="symbol"/> is part of.</param>
		/// <exception cref="ArgumentNullException"><paramref name="symbol"/> is <see langword="null"/>.</exception>
		public bool IsDisabled(INamedTypeSymbol symbol, [NotNullWhen(true)] out ModuleIdentity? module)
		{
			if (symbol is null)
			{
				throw new ArgumentNullException(nameof(symbol));
			}

			if (HasErrors)
			{
				module = null;
				return false;
			}

			int length = _allSymbols.Length;

			for (int i = 0; i < length; i++)
			{
				if (SymbolEqualityComparer.Default.Equals(_allSymbols[i], symbol))
				{
					module = _allTypes[i].Module;
					return !_enabledOrDisabled[i];
				}
			}

			module = null;
			return false;
		}

		private void SetAttributeSymbol()
		{
			EnableModuleAttribute = Compilation.GetTypeByMetadataName(_enableModuleAttributeName);
		}
	}
}
