using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using Durian.Generator.Extensions;
using Durian.Info;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using static Durian.Info.ModuleIdentity;

namespace Durian.Generator
{
	/// <summary>
	/// Utility class that contains static methods similar to those <see cref="ModuleIdentity"/>, but with <see cref="CSharpCompilation"/> or <see cref="IAssemblySymbol"/> as arguments instead of <see cref="Assembly"/>.
	/// </summary>
	public static class ModuleUtilities
	{
		/// <summary>
		/// Checks if the specified <paramref name="module"/> is enabled for the specified <paramref name="compilation"/>.
		/// </summary>
		/// <param name="module"><see cref="ModuleIdentity"/> representing a Durian module to check.</param>
		/// <param name="compilation"><see cref="Assembly"/> to check if the <paramref name="module"/> is enabled for.</param>
		/// <exception cref="ArgumentNullException"><paramref name="module"/> is <see langword="null"/>. -or- <paramref name="compilation"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentException"><paramref name="compilation"/> does not contain the <see cref="EnableModuleAttribute"/>.</exception>
		public static bool IsEnabled(ModuleIdentity module, CSharpCompilation compilation)
		{
			if (module is null)
			{
				throw new ArgumentNullException(nameof(module));
			}

			if (compilation is null)
			{
				throw new ArgumentNullException(nameof(compilation));
			}

			INamedTypeSymbol attr = GetEnableAttributeSymbol(compilation);
			return IsEnabled_Internal(module.Module, compilation.Assembly, attr);
		}

		/// <summary>
		/// Checks if the specified <paramref name="module"/> is enabled for the specified <paramref name="assembly"/>.
		/// </summary>
		/// <param name="module"><see cref="ModuleIdentity"/> representing a Durian module to check.</param>
		/// <param name="assembly"><see cref="IAssemblySymbol"/> to check if the <paramref name="module"/> is enabled for.</param>
		/// <param name="enableModuleAttribute"><see cref="INamedTypeSymbol"/> that represents the <see cref="EnableModuleAttribute"/>.</param>
		/// <exception cref="ArgumentNullException"><paramref name="module"/> is <see langword="null"/>. -or- <paramref name="assembly"/> is <see langword="null"/>. -or- <paramref name="enableModuleAttribute"/> is <see langword="null"/>.</exception>
		public static bool IsEnabled(ModuleIdentity module, IAssemblySymbol assembly, INamedTypeSymbol enableModuleAttribute)
		{
			if (module is null)
			{
				throw new ArgumentNullException(nameof(module));
			}

			return IsEnabled(module.Module, assembly, enableModuleAttribute);
		}

		/// <summary>
		/// Checks if the specified <paramref name="module"/> is enabled for the specified <paramref name="compilation"/>.
		/// </summary>
		/// <param name="module"><see cref="DurianModule"/> to check.</param>
		/// <param name="compilation"><see cref="CSharpCompilation"/> to check if the <paramref name="module"/> is enabled for.</param>
		/// <exception cref="ArgumentNullException"><paramref name="compilation"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentException"><paramref name="compilation"/> does not contain the <see cref="EnableModuleAttribute"/>.</exception>
		/// <exception cref="InvalidOperationException">Unknown <see cref="DurianModule"/> value detected. -or- <see cref="DurianModule.None"/> is not a valid Durian module.</exception>
		public static bool IsEnabled(DurianModule module, CSharpCompilation compilation)
		{
			if (compilation is null)
			{
				throw new ArgumentNullException(nameof(compilation));
			}

			CheckIsValidModuleEnum(module);
			INamedTypeSymbol symbol = GetEnableAttributeSymbol(compilation);
			return IsEnabled_Internal(module, compilation.Assembly, symbol);
		}

		/// <summary>
		/// Checks if the specified <paramref name="module"/> is enabled for the specified <paramref name="assembly"/>.
		/// </summary>
		/// <param name="module"><see cref="DurianModule"/> to check.</param>
		/// <param name="assembly"><see cref="IAssemblySymbol"/> to check if the <paramref name="module"/> is enabled for.</param>
		/// <param name="enableModuleAttribute"><see cref="INamedTypeSymbol"/> that represents the <see cref="EnableModuleAttribute"/>.</param>
		/// <exception cref="ArgumentNullException"><paramref name="assembly"/> is <see langword="null"/>. -or- <paramref name="enableModuleAttribute"/> is <see langword="null"/>.</exception>
		/// <exception cref="InvalidOperationException">Unknown <see cref="DurianModule"/> value detected. -or- <see cref="DurianModule.None"/> is not a valid Durian module.</exception>
		public static bool IsEnabled(DurianModule module, IAssemblySymbol assembly, INamedTypeSymbol enableModuleAttribute)
		{
			if (assembly is null)
			{
				throw new ArgumentNullException(nameof(assembly));
			}

			if (enableModuleAttribute is null)
			{
				throw new ArgumentNullException(nameof(enableModuleAttribute));
			}

			CheckIsValidModuleEnum(module);
			return IsEnabled_Internal(module, assembly, enableModuleAttribute);
		}

		/// <summary>
		/// Checks if the Durian module with the specified <paramref name="moduleName"/> is enabled for the specified <paramref name="compilation"/>.
		/// </summary>
		/// <param name="moduleName">Name of the module to check.</param>
		/// <param name="compilation"><see cref="CSharpCompilation"/> to check if the module is enabled for.</param>
		/// <exception cref="ArgumentNullException"><paramref name="moduleName"/> is <see langword="null"/>. -or- <paramref name="compilation"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentException">Unknown Durian module name: <paramref name="moduleName"/>. -or- <paramref name="compilation"/> does not contain the <see cref="EnableModuleAttribute"/>.</exception>
		public static bool IsEnabled(string moduleName, CSharpCompilation compilation)
		{
			if (moduleName is null)
			{
				throw new ArgumentNullException(nameof(moduleName));
			}

			if (compilation is null)
			{
				throw new ArgumentNullException(nameof(compilation));
			}

			DurianModule module = ParseModule(moduleName);
			INamedTypeSymbol symbol = GetEnableAttributeSymbol(compilation);
			return IsEnabled_Internal(module, compilation.Assembly, symbol);
		}

		/// <summary>
		/// Checks if the Durian module with the specified <paramref name="moduleName"/> is enabled for the specified <paramref name="assembly"/>.
		/// </summary>
		/// <param name="moduleName">Name of the module to check.</param>
		/// <param name="assembly"><see cref="IAssemblySymbol"/> to check if the module is enabled for.</param>
		/// <param name="enableModuleAttribute"><see cref="INamedTypeSymbol"/> that represents the <see cref="EnableModuleAttribute"/>.</param>
		/// <exception cref="ArgumentNullException"><paramref name="moduleName"/> is <see langword="null"/>. -or- <paramref name="assembly"/> is <see langword="null"/>. -or- <paramref name="enableModuleAttribute"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentException">Unknown Durian module name: <paramref name="moduleName"/>.</exception>
		public static bool IsEnabled(string moduleName, IAssemblySymbol assembly, INamedTypeSymbol enableModuleAttribute)
		{
			if (assembly is null)
			{
				throw new ArgumentNullException(nameof(assembly));
			}

			if (enableModuleAttribute is null)
			{
				throw new ArgumentNullException(nameof(enableModuleAttribute));
			}

			DurianModule module = ParseModule(moduleName);
			return IsEnabled_Internal(module, assembly, enableModuleAttribute);
		}

		/// <summary>
		/// Checks if the specified <paramref name="symbol"/> is part of any enabled Durian module.
		/// </summary>
		/// <param name="symbol"><see cref="INamedTypeSymbol"/> to check if is part of any enabled Durian module.</param>
		/// <param name="compilation"><see cref="CSharpCompilation"/> to check if the <paramref name="symbol"/> is enabled for.</param>
		/// <exception cref="ArgumentNullException"><paramref name="symbol"/> is <see langword="null"/>. -or- <paramref name="compilation"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentException"><paramref name="compilation"/> does not contain the <see cref="EnableModuleAttribute"/>.</exception>
		public static bool IsEnabled(INamedTypeSymbol symbol, CSharpCompilation compilation)
		{
			return IsEnabled(symbol, compilation, out ModuleIdentity _);
		}

		/// <summary>
		/// Checks if the specified <paramref name="symbol"/> is part of any enabled Durian module.
		/// </summary>
		/// <param name="symbol"><see cref="INamedTypeSymbol"/> to check if is part of any enabled Durian module.</param>
		/// <param name="compilation"><see cref="CSharpCompilation"/> to check if the <paramref name="symbol"/> is enabled for.</param>
		/// <param name="module"><see cref="DurianModule"/> this <paramref name="symbol"/> is part of.</param>
		/// <exception cref="ArgumentNullException"><paramref name="symbol"/> is <see langword="null"/>. -or- <paramref name="compilation"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentException"><paramref name="compilation"/> does not contain the <see cref="EnableModuleAttribute"/>.</exception>
		public static bool IsEnabled(INamedTypeSymbol symbol, CSharpCompilation compilation, out DurianModule module)
		{
			bool enabled = IsEnabled(symbol, compilation, out ModuleIdentity? identity);
			module = enabled ? identity!.Module : DurianModule.None;
			return enabled;
		}

		/// <summary>
		/// Checks if the specified <paramref name="symbol"/> is part of any enabled Durian module.
		/// </summary>
		/// <param name="symbol"><see cref="INamedTypeSymbol"/> to check if is part of any enabled Durian module.</param>
		/// <param name="compilation"><see cref="CSharpCompilation"/> to check if the <paramref name="symbol"/> is enabled for.</param>
		/// <param name="module"><see cref="ModuleIdentity"/> this <paramref name="symbol"/> is part of.</param>
		/// <exception cref="ArgumentNullException"><paramref name="symbol"/> is <see langword="null"/>. -or- <paramref name="compilation"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentException"><paramref name="compilation"/> does not contain the <see cref="EnableModuleAttribute"/>.</exception>
		public static bool IsEnabled(INamedTypeSymbol symbol, CSharpCompilation compilation, [NotNullWhen(true)] out ModuleIdentity? module)
		{
			if (symbol is null)
			{
				throw new ArgumentNullException(nameof(symbol));
			}

			if (compilation is null)
			{
				throw new ArgumentNullException(nameof(compilation));
			}

			INamedTypeSymbol attr = GetEnableAttributeSymbol(compilation);
			return IsEnabled_Internal(symbol, attr, compilation.Assembly.GetAttributes(), out module);
		}

		/// <summary>
		/// Checks if the specified <paramref name="symbol"/> is part of any enabled Durian module.
		/// </summary>
		/// <param name="symbol"><see cref="INamedTypeSymbol"/> to check if is part of any enabled Durian module.</param>
		/// <param name="assembly"><see cref="IAssemblySymbol"/> to check if the <paramref name="symbol"/> is enabled for.</param>
		/// <param name="enableModuleAttribute"><see cref="INamedTypeSymbol"/> that represents the <see cref="EnableModuleAttribute"/>.</param>
		/// <exception cref="ArgumentNullException"><paramref name="symbol"/> is <see langword="null"/>. -or- <paramref name="assembly"/> is <see langword="null"/>. -or- <paramref name="enableModuleAttribute"/> is <see langword="null"/>.</exception>
		public static bool IsEnabled(INamedTypeSymbol symbol, IAssemblySymbol assembly, INamedTypeSymbol enableModuleAttribute)
		{
			return IsEnabled(symbol, assembly, enableModuleAttribute, out ModuleIdentity _);
		}

		/// <summary>
		/// Checks if the specified <paramref name="symbol"/> is part of any enabled Durian module.
		/// </summary>
		/// <param name="symbol"><see cref="INamedTypeSymbol"/> to check if is part of any enabled Durian module.</param>
		/// <param name="assembly"><see cref="IAssemblySymbol"/> to check if the <paramref name="symbol"/> is enabled for.</param>
		/// <param name="enableModuleAttribute"><see cref="INamedTypeSymbol"/> that represents the <see cref="EnableModuleAttribute"/>.</param>
		/// <param name="module"><see cref="DurianModule"/> this <paramref name="symbol"/> is part of.</param>
		/// <exception cref="ArgumentNullException"><paramref name="symbol"/> is <see langword="null"/>. -or- <paramref name="assembly"/> is <see langword="null"/>. -or- <paramref name="enableModuleAttribute"/> is <see langword="null"/>.</exception>
		public static bool IsEnabled(INamedTypeSymbol symbol, IAssemblySymbol assembly, INamedTypeSymbol enableModuleAttribute, out DurianModule module)
		{
			bool enabled = IsEnabled(symbol, assembly, enableModuleAttribute, out ModuleIdentity? identity);
			module = enabled ? identity!.Module : DurianModule.None;
			return enabled;
		}

		/// <summary>
		/// Checks if the specified <paramref name="symbol"/> is part of any enabled Durian module.
		/// </summary>
		/// <param name="symbol"><see cref="INamedTypeSymbol"/> to check if is part of any enabled Durian module.</param>
		/// <param name="assembly"><see cref="IAssemblySymbol"/> to check if the <paramref name="symbol"/> is enabled for.</param>
		/// <param name="enableModuleAttribute"><see cref="INamedTypeSymbol"/> that represents the <see cref="EnableModuleAttribute"/>.</param>
		/// <param name="module"><see cref="ModuleIdentity"/> this <paramref name="symbol"/> is part of.</param>
		/// <exception cref="ArgumentNullException"><paramref name="symbol"/> is <see langword="null"/>. -or- <paramref name="assembly"/> is <see langword="null"/>. -or- <paramref name="enableModuleAttribute"/> is <see langword="null"/>.</exception>
		public static bool IsEnabled(INamedTypeSymbol symbol, IAssemblySymbol assembly, INamedTypeSymbol enableModuleAttribute, [NotNullWhen(true)] out ModuleIdentity? module)
		{
			if (symbol is null)
			{
				throw new ArgumentNullException(nameof(symbol));
			}

			if (assembly is null)
			{
				throw new ArgumentNullException(nameof(assembly));
			}

			if (enableModuleAttribute is null)
			{
				throw new ArgumentNullException(nameof(enableModuleAttribute));
			}

			return IsEnabled_Internal(symbol, enableModuleAttribute, assembly.GetAttributes(), out module);
		}

		/// <summary>
		/// Returns an array of <see cref="ModuleIdentity"/> representing Durian modules that are enabled for the specified <paramref name="compilation"/>.
		/// </summary>
		/// <param name="compilation"><see cref="CSharpCompilation"/> to check if the module is enabled for.</param>
		/// <exception cref="ArgumentNullException"><paramref name="compilation"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentException"><paramref name="compilation"/> does not contain the <see cref="EnableModuleAttribute"/>.</exception>
		public static ModuleIdentity[] GetEnabledModules(CSharpCompilation compilation)
		{
			if (compilation is null)
			{
				throw new ArgumentNullException(nameof(compilation));
			}

			INamedTypeSymbol attr = GetEnableAttributeSymbol(compilation);
			return GetEnabledModules(compilation.Assembly, attr);
		}

		/// <summary>
		/// Returns an array of <see cref="ModuleIdentity"/> representing Durian modules that are enabled for the specified <paramref name="assembly"/>.
		/// </summary>
		/// <param name="assembly"><see cref="IAssemblySymbol"/> to get the enabled Durian modules of.</param>
		/// <param name="enableModuleAttribute"><see cref="INamedTypeSymbol"/> that represents the <see cref="EnableModuleAttribute"/>.</param>
		/// <exception cref="ArgumentNullException"><paramref name="assembly"/> is <see langword="null"/>. -or- <paramref name="enableModuleAttribute"/> is <see langword="null"/></exception>
		public static ModuleIdentity[] GetEnabledModules(IAssemblySymbol assembly, INamedTypeSymbol enableModuleAttribute)
		{
			if (assembly is null)
			{
				throw new ArgumentNullException(nameof(assembly));
			}

			if (enableModuleAttribute is null)
			{
				throw new ArgumentNullException(nameof(enableModuleAttribute));
			}

			List<ModuleIdentity> modules = new(8);

			foreach (AttributeData attribute in assembly.GetAttributes())
			{
				if (SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, enableModuleAttribute) && attribute.TryGetConstructorArgumentValue(0, out int value))
				{
					DurianModule module = (DurianModule)value;
					modules.Add(GetModule(module));
				}
			}

			return modules.ToArray();
		}

		/// <summary>
		/// Returns an array of <see cref="ModuleIdentity"/> representing Durian modules that are enabled for the specified <paramref name="compilation"/>. Only <see cref="ModuleIdentity"/> that are present in the given array of <paramref name="modules"/> are included.
		/// </summary>
		/// <param name="compilation"><see cref="CSharpCompilation"/> to get the enabled Durian modules of.</param>
		/// <param name="modules">Array of <see cref="ModuleIdentity"/> to pick the enabled modules from.</param>
		/// <exception cref="ArgumentNullException"><paramref name="compilation"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentException"><paramref name="compilation"/> does not contain the <see cref="EnableModuleAttribute"/>.</exception>
		public static ModuleIdentity[] GetEnabledModules(CSharpCompilation compilation, ModuleIdentity[]? modules)
		{
			if (compilation is null)
			{
				throw new ArgumentNullException(nameof(compilation));
			}

			INamedTypeSymbol attr = GetEnableAttributeSymbol(compilation);
			return GetEnabledModules(compilation.Assembly, attr, modules);
		}

		/// <summary>
		/// Returns an array of <see cref="ModuleIdentity"/> representing Durian modules that are enabled for the specified <paramref name="assembly"/>. Only <see cref="ModuleIdentity"/> that are present in the given array of <paramref name="modules"/> are included.
		/// </summary>
		/// <param name="assembly"><see cref="Assembly"/> to get the enabled Durian modules of.</param>
		/// <param name="enableModuleAttribute"><see cref="INamedTypeSymbol"/> that represents the <see cref="EnableModuleAttribute"/>.</param>
		/// <param name="modules">Array of <see cref="ModuleIdentity"/> to pick the enabled modules from.</param>
		/// <exception cref="ArgumentNullException"><paramref name="assembly"/> is <see langword="null"/>. -or- <paramref name="enableModuleAttribute"/> is <see langword="null"/>.</exception>
		public static ModuleIdentity[] GetEnabledModules(IAssemblySymbol assembly, INamedTypeSymbol enableModuleAttribute, ModuleIdentity[]? modules)
		{
			if (assembly is null)
			{
				throw new ArgumentNullException(nameof(assembly));
			}

			if (enableModuleAttribute is null)
			{
				throw new ArgumentNullException(nameof(enableModuleAttribute));
			}

			if (modules is null || modules.Length == 0)
			{
				return Array.Empty<ModuleIdentity>();
			}

			HashSet<ModuleIdentity> set = new(ModuleEnumEqualityComparer.Instance);

			foreach (AttributeData attribute in assembly.GetAttributes())
			{
				if (SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, enableModuleAttribute) && attribute.TryGetConstructorArgumentValue(0, out int value))
				{
					DurianModule enumValue = (DurianModule)value;

					foreach (ModuleIdentity module in modules)
					{
						if (module.Module == enumValue)
						{
							set.Add(module);
						}
					}
				}
			}

			ModuleIdentity[] array = new ModuleIdentity[set.Count];
			set.CopyTo(array);
			return array;
		}

		/// <summary>
		/// Returns an array of <see cref="ModuleIdentity"/> representing Durian modules that are enabled for the specified <paramref name="compilation"/>. Only <see cref="DurianModule"/>s that are present in the given array of <paramref name="modules"/> are included.
		/// </summary>
		/// <param name="compilation"><see cref="CSharpCompilation"/> to get the enabled Durian modules of.</param>
		/// <param name="modules">Array of <see cref="DurianModule"/>s to pick the enabled modules from.</param>
		/// <exception cref="ArgumentNullException"><paramref name="compilation"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentException"><paramref name="compilation"/> does not contain the <see cref="EnableModuleAttribute"/>.</exception>
		/// <exception cref="InvalidOperationException">Unknown <see cref="DurianModule"/> value detected. -or- <see cref="DurianModule.None"/> is not a valid Durian module.</exception>
		public static ModuleIdentity[] GetEnabledModules(CSharpCompilation compilation, DurianModule[]? modules)
		{
			if (compilation is null)
			{
				throw new ArgumentNullException(nameof(compilation));
			}

			INamedTypeSymbol attr = GetEnableAttributeSymbol(compilation);
			return GetEnabledModules(compilation.Assembly, attr, modules);
		}

		/// <summary>
		/// Returns an array of <see cref="ModuleIdentity"/> representing Durian modules that are enabled for the specified <paramref name="assembly"/>. Only <see cref="DurianModule"/>s that are present in the given array of <paramref name="modules"/> are included.
		/// </summary>
		/// <param name="assembly"><see cref="IAssemblySymbol"/> to get the enabled Durian modules of.</param>
		/// <param name="enableModuleAttribute"><see cref="INamedTypeSymbol"/> that represents the <see cref="EnableModuleAttribute"/>.</param>
		/// <param name="modules">Array of <see cref="DurianModule"/>s to pick the enabled modules from.</param>
		/// <exception cref="ArgumentNullException"><paramref name="assembly"/> is <see langword="null"/>. -or- <paramref name="enableModuleAttribute"/> is <see langword="null"/>.</exception>
		/// <exception cref="InvalidOperationException">Unknown <see cref="DurianModule"/> value detected. -or- <see cref="DurianModule.None"/> is not a valid Durian module.</exception>
		public static ModuleIdentity[] GetEnabledModules(IAssemblySymbol assembly, INamedTypeSymbol enableModuleAttribute, DurianModule[]? modules)
		{
			if (assembly is null)
			{
				throw new ArgumentNullException(nameof(assembly));
			}

			if (enableModuleAttribute is null)
			{
				throw new ArgumentNullException(nameof(enableModuleAttribute));
			}

			if (modules is null || modules.Length == 0)
			{
				return Array.Empty<ModuleIdentity>();
			}

			HashSet<DurianModule> set = new();
			List<ModuleIdentity> list = new(16);

			foreach (AttributeData attribute in assembly.GetAttributes())
			{
				if (SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, enableModuleAttribute) && attribute.TryGetConstructorArgumentValue(0, out int value))
				{
					DurianModule enumValue = (DurianModule)value;

					foreach (DurianModule module in modules)
					{
						if (module == enumValue && set.Add(module))
						{
							list.Add(GetModule(module));
						}
					}
				}
			}

			return list.ToArray();
		}

		/// <summary>
		/// Returns an array of <see cref="DurianModule"/>s representing Durian modules that are enabled for the specified <paramref name="compilation"/>.
		/// </summary>
		/// <param name="compilation"><see cref="CSharpCompilation"/> to get the enabled Durian modules of.</param>
		/// <exception cref="ArgumentNullException"><paramref name="compilation"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentException"><paramref name="compilation"/> does not contain the <see cref="EnableModuleAttribute"/>.</exception>
		public static DurianModule[] GetEnabledModulesAsEnums(CSharpCompilation compilation)
		{
			if (compilation is null)
			{
				throw new ArgumentNullException(nameof(compilation));
			}

			INamedTypeSymbol attr = GetEnableAttributeSymbol(compilation);
			return GetEnabledModulesAsEnums(compilation.Assembly, attr);
		}

		/// <summary>
		/// Returns an array of <see cref="DurianModule"/>s representing Durian modules that are enabled for the specified <paramref name="assembly"/>.
		/// </summary>
		/// <param name="assembly"><see cref="IAssemblySymbol"/> to get the enabled Durian modules of.</param>
		/// <param name="enableModuleAttribute"><see cref="INamedTypeSymbol"/> that represents the <see cref="EnableModuleAttribute"/>.</param>
		/// <exception cref="ArgumentNullException"><paramref name="assembly"/> is <see langword="null"/>. -or- <paramref name="enableModuleAttribute"/> is <see langword="null"/>.</exception>
		public static DurianModule[] GetEnabledModulesAsEnums(IAssemblySymbol assembly, INamedTypeSymbol enableModuleAttribute)
		{
			if (assembly is null)
			{
				throw new ArgumentNullException(nameof(assembly));
			}

			if (enableModuleAttribute is null)
			{
				throw new ArgumentNullException(nameof(enableModuleAttribute));
			}

			List<DurianModule> modules = new(8);

			foreach (AttributeData attribute in assembly.GetAttributes())
			{
				if (SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, enableModuleAttribute) && attribute.TryGetConstructorArgumentValue(0, out int value))
				{
					DurianModule module = (DurianModule)value;
					modules.Add(module);
				}
			}

			return modules.ToArray();
		}

		/// <summary>
		/// Returns an array of <see cref="DurianModule"/>s representing Durian modules that are enabled for the specified <paramref name="compilation"/>. Only <see cref="ModuleIdentity"/> that are present in the given array of <paramref name="modules"/> are included.
		/// </summary>
		/// <param name="compilation"><see cref="CSharpCompilation"/> to get the enabled Durian modules of.</param>
		/// <param name="modules">Array of <see cref="ModuleIdentity"/> to pick the enabled modules from.</param>
		/// <exception cref="ArgumentNullException"><paramref name="compilation"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentException"><paramref name="compilation"/> does not contain the <see cref="EnableModuleAttribute"/>.</exception>
		public static DurianModule[] GetEnabledModulesAsEnums(CSharpCompilation compilation, ModuleIdentity[]? modules)
		{
			if (compilation is null)
			{
				throw new ArgumentNullException(nameof(compilation));
			}

			INamedTypeSymbol attr = GetEnableAttributeSymbol(compilation);
			return GetEnabledModulesAsEnums(compilation.Assembly, attr, modules);
		}

		/// <summary>
		/// Returns an array of <see cref="DurianModule"/>s representing Durian modules that are enabled for the specified <paramref name="assembly"/>. Only <see cref="ModuleIdentity"/> that are present in the given array of <paramref name="modules"/> are included.
		/// </summary>
		/// <param name="assembly"><see cref="Assembly"/> to get the enabled Durian modules of.</param>
		/// <param name="enableModuleAttribute"><see cref="INamedTypeSymbol"/> that represents the <see cref="EnableModuleAttribute"/>.</param>
		/// <param name="modules">Array of <see cref="ModuleIdentity"/> to pick the enabled modules from.</param>
		/// <exception cref="ArgumentNullException"><paramref name="assembly"/> is <see langword="null"/>. -or- <paramref name="enableModuleAttribute"/> is <see langword="null"/>.</exception>
		public static DurianModule[] GetEnabledModulesAsEnums(IAssemblySymbol assembly, INamedTypeSymbol enableModuleAttribute, ModuleIdentity[]? modules)
		{
			if (assembly is null)
			{
				throw new ArgumentNullException(nameof(assembly));
			}

			if (enableModuleAttribute is null)
			{
				throw new ArgumentNullException(nameof(enableModuleAttribute));
			}

			if (modules is null || modules.Length == 0)
			{
				return Array.Empty<DurianModule>();
			}

			HashSet<DurianModule> set = new();

			foreach (AttributeData attribute in assembly.GetAttributes())
			{
				if (SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, enableModuleAttribute) && attribute.TryGetConstructorArgumentValue(0, out int value))
				{
					DurianModule enumValue = (DurianModule)value;

					foreach (ModuleIdentity module in modules)
					{
						if (module.Module == enumValue)
						{
							set.Add(enumValue);
						}
					}
				}
			}

			DurianModule[] array = new DurianModule[set.Count];
			set.CopyTo(array);
			return array;
		}

		/// <summary>
		/// Returns an array of <see cref="DurianModule"/>s representing Durian modules that are enabled for the specified <paramref name="compilation"/>. Only <see cref="DurianModule"/>s that are present in the given array of <paramref name="modules"/> are included.
		/// </summary>
		/// <param name="compilation"><see cref="CSharpCompilation"/> to get the enabled Durian modules of.</param>
		/// <param name="modules">Array of <see cref="DurianModule"/>s to pick the enabled modules from.</param>
		/// <exception cref="ArgumentNullException"><paramref name="compilation"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentException"><paramref name="compilation"/> does not contain the <see cref="EnableModuleAttribute"/>.</exception>
		/// <exception cref="InvalidOperationException">Unknown <see cref="DurianModule"/> value detected. -or- <see cref="DurianModule.None"/> is not a valid Durian module.</exception>
		public static DurianModule[] GetEnabledModulesAsEnums(CSharpCompilation compilation, DurianModule[]? modules)
		{
			if (compilation is null)
			{
				throw new ArgumentNullException(nameof(compilation));
			}

			INamedTypeSymbol attr = GetEnableAttributeSymbol(compilation);
			return GetEnabledModulesAsEnums(compilation.Assembly, attr, modules);
		}

		/// <summary>
		/// Returns an array of <see cref="DurianModule"/>s representing Durian modules that are enabled for the specified <paramref name="assembly"/>. Only <see cref="DurianModule"/>s that are present in the given array of <paramref name="modules"/> are included.
		/// </summary>
		/// <param name="assembly"><see cref="IAssemblySymbol"/> to get the enabled Durian modules of.</param>
		/// <param name="enableModuleAttribute"><see cref="INamedTypeSymbol"/> that represents the <see cref="EnableModuleAttribute"/>.</param>
		/// <param name="modules">Array of <see cref="DurianModule"/>s to pick the enabled modules from.</param>
		/// <exception cref="ArgumentNullException"><paramref name="assembly"/> is <see langword="null"/>. -or- <paramref name="enableModuleAttribute"/> is <see langword="null"/>.</exception>
		/// <exception cref="InvalidOperationException">Unknown <see cref="DurianModule"/> value detected. -or- <see cref="DurianModule.None"/> is not a valid Durian module.</exception>
		public static DurianModule[] GetEnabledModulesAsEnums(IAssemblySymbol assembly, INamedTypeSymbol enableModuleAttribute, DurianModule[]? modules)
		{
			if (assembly is null)
			{
				throw new ArgumentNullException(nameof(assembly));
			}

			if (enableModuleAttribute is null)
			{
				throw new ArgumentNullException(nameof(enableModuleAttribute));
			}

			if (modules is null || modules.Length == 0)
			{
				return Array.Empty<DurianModule>();
			}

			HashSet<DurianModule> set = new();

			foreach (AttributeData attribute in assembly.GetAttributes())
			{
				if (SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, enableModuleAttribute) && attribute.TryGetConstructorArgumentValue(0, out int value))
				{
					DurianModule enumValue = (DurianModule)value;

					foreach (DurianModule module in modules)
					{
						if (module == enumValue)
						{
							set.Add(module);
						}
					}
				}
			}

			DurianModule[] array = new DurianModule[set.Count];
			set.CopyTo(array);
			return array;
		}

		/// <summary>
		/// Returns an array of <see cref="ModuleIdentity"/> representing Durian modules that are disabled for the specified <paramref name="compilation"/>.
		/// </summary>
		/// <param name="compilation"><see cref="CSharpCompilation"/> to get the disabled Durian modules of.</param>
		/// <exception cref="ArgumentNullException"><paramref name="compilation"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentException"><paramref name="compilation"/> does not contain the <see cref="EnableModuleAttribute"/>.</exception>
		public static ModuleIdentity[] GetDisabledModules(CSharpCompilation compilation)
		{
			if (compilation is null)
			{
				throw new ArgumentNullException(nameof(compilation));
			}

			INamedTypeSymbol attr = GetEnableAttributeSymbol(compilation);
			return GetDisabledModules(compilation.Assembly, attr);
		}

		/// <summary>
		/// Returns an array of <see cref="ModuleIdentity"/> representing Durian modules that are disabled for the specified <paramref name="assembly"/>.
		/// </summary>
		/// <param name="assembly"><see cref="IAssemblySymbol"/> to get the disabled Durian modules of.</param>
		/// <param name="enableModuleAttribute"><see cref="INamedTypeSymbol"/> that represents the <see cref="EnableModuleAttribute"/>.</param>
		/// <exception cref="ArgumentNullException"><paramref name="assembly"/> is <see langword="null"/>. -or- <paramref name="enableModuleAttribute"/> is <see langword="null"/>.</exception>
		public static ModuleIdentity[] GetDisabledModules(IAssemblySymbol assembly, INamedTypeSymbol enableModuleAttribute)
		{
			return GetDisabledModules(assembly, enableModuleAttribute, GetAllModules());
		}

		/// <summary>
		/// Returns an array of <see cref="ModuleIdentity"/> representing Durian modules that are disabled for the specified <paramref name="compilation"/>. Only <see cref="ModuleIdentity"/> that are present in the given array of <paramref name="modules"/> are included.
		/// </summary>
		/// <param name="compilation"><see cref="CSharpCompilation"/> to get the disabled Durian modules of.</param>
		/// <param name="modules">Array of <see cref="ModuleIdentity"/> to pick the disabled modules from.</param>
		/// <exception cref="ArgumentNullException"><paramref name="compilation"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentException"><paramref name="compilation"/> does not contain the <see cref="EnableModuleAttribute"/>.</exception>
		public static ModuleIdentity[] GetDisabledModules(CSharpCompilation compilation, ModuleIdentity[]? modules)
		{
			if (compilation is null)
			{
				throw new ArgumentNullException(nameof(compilation));
			}

			INamedTypeSymbol attr = GetEnableAttributeSymbol(compilation);
			return GetDisabledModules(compilation.Assembly, attr, modules);
		}

		/// <summary>
		/// Returns an array of <see cref="ModuleIdentity"/> representing Durian modules that are disabled for the specified <paramref name="assembly"/>. Only <see cref="ModuleIdentity"/> that are present in the given array of <paramref name="modules"/> are included.
		/// </summary>
		/// <param name="assembly"><see cref="IAssemblySymbol"/> to get the disabled Durian modules of.</param>
		/// <param name="enableModuleAttribute"><see cref="INamedTypeSymbol"/> that represents the <see cref="EnableModuleAttribute"/>.</param>
		/// <param name="modules">Array of <see cref="ModuleIdentity"/> to pick the disabled modules from.</param>
		/// <exception cref="ArgumentNullException"><paramref name="assembly"/> is <see langword="null"/>. -or- <paramref name="enableModuleAttribute"/> is <see langword="null"/>.</exception>
		public static ModuleIdentity[] GetDisabledModules(IAssemblySymbol assembly, INamedTypeSymbol enableModuleAttribute, ModuleIdentity[]? modules)
		{
			if (assembly is null)
			{
				throw new ArgumentNullException(nameof(assembly));
			}

			if (enableModuleAttribute is null)
			{
				throw new ArgumentNullException(nameof(enableModuleAttribute));
			}

			if (modules is null || modules.Length == 0)
			{
				return Array.Empty<ModuleIdentity>();
			}

			ModuleIdentity[] enabledModules = GetEnabledModules(assembly, enableModuleAttribute, modules);

			if (modules.Length == enabledModules.Length)
			{
				return Array.Empty<ModuleIdentity>();
			}

			ModuleEnumEqualityComparer comparer = ModuleEnumEqualityComparer.Instance;

			if (enabledModules.Length == 0)
			{
				return modules.Distinct(comparer).ToArray();
			}

			return modules
				.Except(enabledModules, comparer)
				.Distinct(comparer)
				.ToArray();
		}

		/// <summary>
		/// Returns an array of <see cref="ModuleIdentity"/> representing Durian modules that are disabled for the specified <paramref name="compilation"/>. Only <see cref="DurianModule"/> that are present in the given array of <paramref name="modules"/> are included.
		/// </summary>
		/// <param name="compilation"><see cref="CSharpCompilation"/> to get the disabled Durian modules of.</param>
		/// <param name="modules">Array of <see cref="DurianModule"/> to pick the disabled modules from.</param>
		/// <exception cref="ArgumentNullException"><paramref name="compilation"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentException"><paramref name="compilation"/> does not contain the <see cref="EnableModuleAttribute"/>.</exception>
		public static ModuleIdentity[] GetDisabledModules(CSharpCompilation compilation, DurianModule[]? modules)
		{
			if (compilation is null)
			{
				throw new ArgumentNullException(nameof(compilation));
			}

			INamedTypeSymbol attr = GetEnableAttributeSymbol(compilation);
			return GetDisabledModules(compilation.Assembly, attr, modules);
		}

		/// <summary>
		/// Returns an array of <see cref="ModuleIdentity"/> representing Durian modules that are disabled for the specified <paramref name="assembly"/>. Only <see cref="DurianModule"/>s that are present in the given array of <paramref name="modules"/> are included.
		/// </summary>
		/// <param name="assembly"><see cref="IAssemblySymbol"/> to get the disabled Durian modules of.</param>
		/// <param name="enableModuleAttribute"><see cref="INamedTypeSymbol"/> that represents the <see cref="EnableModuleAttribute"/>.</param>
		/// <param name="modules">Array of <see cref="DurianModule"/>s to pick the disabled modules from.</param>
		/// <exception cref="ArgumentNullException"><paramref name="assembly"/> is <see langword="null"/>. -or- <paramref name="enableModuleAttribute"/> is <see langword="null"/>.</exception>
		/// <exception cref="InvalidOperationException">Unknown <see cref="DurianModule"/> value detected. -or- <see cref="DurianModule.None"/> is not a valid Durian module.</exception>
		public static ModuleIdentity[] GetDisabledModules(IAssemblySymbol assembly, INamedTypeSymbol enableModuleAttribute, DurianModule[]? modules)
		{
			if (assembly is null)
			{
				throw new ArgumentNullException(nameof(assembly));
			}

			if (enableModuleAttribute is null)
			{
				throw new ArgumentNullException(nameof(enableModuleAttribute));
			}

			if (modules is null || modules.Length == 0)
			{
				return Array.Empty<ModuleIdentity>();
			}

			int length = modules.Length;
			ModuleIdentity[] allModules = new ModuleIdentity[length];

			for (int i = 0; i < length; i++)
			{
				allModules[i] = GetModule(modules[i]);
			}

			ModuleIdentity[] enabledModules = GetEnabledModules(assembly, enableModuleAttribute, allModules);

			if (modules.Length == enabledModules.Length)
			{
				return Array.Empty<ModuleIdentity>();
			}

			ModuleEnumEqualityComparer comparer = ModuleEnumEqualityComparer.Instance;

			if (enabledModules.Length == 0)
			{
				return allModules.Distinct(comparer).ToArray();
			}

			return allModules
				.Except(enabledModules, comparer)
				.Distinct(comparer)
				.ToArray();
		}

		/// <summary>
		/// Returns an array of <see cref="DurianModule"/>s representing Durian modules that are disabled for the specified <paramref name="compilation"/>.
		/// </summary>
		/// <param name="compilation"><see cref="CSharpCompilation"/> to get the disabled Durian modules of.</param>
		/// <exception cref="ArgumentNullException"><paramref name="compilation"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentException"><paramref name="compilation"/> does not contain the <see cref="EnableModuleAttribute"/>.</exception>
		public static DurianModule[] GetDisabledModulesAsEnums(CSharpCompilation compilation)
		{
			if (compilation is null)
			{
				throw new ArgumentNullException(nameof(compilation));
			}

			INamedTypeSymbol attr = GetEnableAttributeSymbol(compilation);
			return GetDisabledModulesAsEnums(compilation.Assembly, attr, GetAllModules());
		}

		/// <summary>
		/// Returns an array of <see cref="DurianModule"/>s representing Durian modules that are disabled for the specified <paramref name="assembly"/>.
		/// </summary>
		/// <param name="assembly"><see cref="IAssemblySymbol"/> to get the disabled Durian modules of.</param>
		/// <param name="enableModuleAttribute"><see cref="INamedTypeSymbol"/> that represents the <see cref="EnableModuleAttribute"/>.</param>
		/// <exception cref="ArgumentNullException"><paramref name="assembly"/> is <see langword="null"/>. -or- <paramref name="enableModuleAttribute"/> is <see langword="null"/>.</exception>
		public static DurianModule[] GetDisabledModulesAsEnums(IAssemblySymbol assembly, INamedTypeSymbol enableModuleAttribute)
		{
			return GetDisabledModulesAsEnums(assembly, enableModuleAttribute, GetAllModules());
		}

		/// <summary>
		/// Returns an array of <see cref="DurianModule"/>s representing Durian modules that are disabled for the specified <paramref name="compilation"/>. Only <see cref="ModuleIdentity"/> that are present in the given array of <paramref name="modules"/> are included.
		/// </summary>
		/// <param name="compilation"><see cref="CSharpCompilation"/> to get the disabled Durian modules of.</param>
		/// <param name="modules">Array of <see cref="ModuleIdentity"/> to pick the disabled modules from.</param>
		/// <exception cref="ArgumentNullException"><paramref name="compilation"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentException"><paramref name="compilation"/> does not contain the <see cref="EnableModuleAttribute"/>.</exception>
		public static DurianModule[] GetDisabledModulesAsEnums(CSharpCompilation compilation, ModuleIdentity[]? modules)
		{
			if (compilation is null)
			{
				throw new ArgumentNullException(nameof(compilation));
			}

			INamedTypeSymbol attr = GetEnableAttributeSymbol(compilation);
			return GetDisabledModulesAsEnums(compilation.Assembly, attr, modules);
		}

		/// <summary>
		/// Returns an array of <see cref="DurianModule"/>s representing Durian modules that are disabled for the specified <paramref name="assembly"/>. Only <see cref="ModuleIdentity"/> that are present in the given array of <paramref name="modules"/> are included.
		/// </summary>
		/// <param name="assembly"><see cref="IAssemblySymbol"/> to get the disabled Durian modules of.</param>
		/// <param name="enableModuleAttribute"><see cref="INamedTypeSymbol"/> that represents the <see cref="EnableModuleAttribute"/>.</param>
		/// <param name="modules">Array of <see cref="ModuleIdentity"/> to pick the disabled modules from.</param>
		/// <exception cref="ArgumentNullException"><paramref name="assembly"/> is <see langword="null"/>. -or- <paramref name="enableModuleAttribute"/> is <see langword="null"/>.</exception>
		public static DurianModule[] GetDisabledModulesAsEnums(IAssemblySymbol assembly, INamedTypeSymbol enableModuleAttribute, ModuleIdentity[]? modules)
		{
			if (assembly is null)
			{
				throw new ArgumentNullException(nameof(assembly));
			}

			if (enableModuleAttribute is null)
			{
				throw new ArgumentNullException(nameof(enableModuleAttribute));
			}

			if (modules is null || modules.Length == 0)
			{
				return Array.Empty<DurianModule>();
			}

			ModuleIdentity[] enabledModules = GetEnabledModules(assembly, enableModuleAttribute, modules);

			if (modules.Length == enabledModules.Length)
			{
				return Array.Empty<DurianModule>();
			}

			if (enabledModules.Length == 0)
			{
				return modules.Select(m => m.Module).ToArray();
			}

			ModuleEnumEqualityComparer comparer = ModuleEnumEqualityComparer.Instance;

			return modules
				.Except(enabledModules, comparer)
				.Distinct(comparer)
				.Select(m => m.Module)
				.ToArray();
		}

		/// <summary>
		/// Returns an array of <see cref="DurianModule"/>s representing Durian modules that are disabled for the specified <paramref name="compilation"/>. Only <see cref="DurianModule"/>s that are present in the given array of <paramref name="modules"/> are included.
		/// </summary>
		/// <param name="compilation"><see cref="CSharpCompilation"/> to get the disabled Durian modules of.</param>
		/// <param name="modules">Array of <see cref="ModuleIdentity"/> to pick the disabled modules from.</param>
		/// <exception cref="ArgumentNullException"><paramref name="compilation"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentException"><paramref name="compilation"/> does not contain the <see cref="EnableModuleAttribute"/>.</exception>
		public static DurianModule[] GetDisabledModulesAsEnums(CSharpCompilation compilation, DurianModule[]? modules)
		{
			if (compilation is null)
			{
				throw new ArgumentNullException(nameof(compilation));
			}

			INamedTypeSymbol attr = GetEnableAttributeSymbol(compilation);
			return GetDisabledModulesAsEnums(compilation.Assembly, attr, modules);
		}

		/// <summary>
		/// Returns an array of <see cref="DurianModule"/>s representing Durian modules that are disabled for the specified <paramref name="assembly"/>. Only <see cref="DurianModule"/>s that are present in the given array of <paramref name="modules"/> are included.
		/// </summary>
		/// <param name="assembly"><see cref="IAssemblySymbol"/> to get the disabled Durian modules of.</param>
		/// <param name="enableModuleAttribute"><see cref="INamedTypeSymbol"/> that represents the <see cref="EnableModuleAttribute"/>.</param>
		/// <param name="modules">Array of <see cref="DurianModule"/>s to pick the disabled modules from.</param>
		/// <exception cref="ArgumentNullException"><paramref name="assembly"/> is <see langword="null"/>. -or- <paramref name="enableModuleAttribute"/> is <see langword="null"/>.</exception>
		/// <exception cref="InvalidOperationException">Unknown <see cref="DurianModule"/> value detected. -or- <see cref="DurianModule.None"/> is not a valid Durian module.</exception>
		public static DurianModule[] GetDisabledModulesAsEnums(IAssemblySymbol assembly, INamedTypeSymbol enableModuleAttribute, DurianModule[]? modules)
		{
			if (assembly is null)
			{
				throw new ArgumentNullException(nameof(assembly));
			}

			if (enableModuleAttribute is null)
			{
				throw new ArgumentNullException(nameof(enableModuleAttribute));
			}

			if (modules is null || modules.Length == 0)
			{
				return Array.Empty<DurianModule>();
			}

			int length = modules.Length;
			ModuleIdentity[] allModules = new ModuleIdentity[length];

			for (int i = 0; i < length; i++)
			{
				allModules[i] = GetModule(modules[i]);
			}

			ModuleIdentity[] enabledModules = GetEnabledModules(assembly, enableModuleAttribute, allModules);

			if (modules.Length == enabledModules.Length)
			{
				return Array.Empty<DurianModule>();
			}

			ModuleEnumEqualityComparer comparer = ModuleEnumEqualityComparer.Instance;

			if (enabledModules.Length == 0)
			{
				return allModules
					.Distinct(comparer)
					.Select(m => m.Module)
					.ToArray();
			}

			return allModules
				.Except(enabledModules, comparer)
				.Distinct(comparer)
				.Select(m => m.Module)
				.ToArray();
		}

		/// <summary>
		/// Returns an array of <see cref="INamedTypeSymbol"/>s representing all types that are part of any Durian module.
		/// </summary>
		/// <param name="compilation"><see cref="CSharpCompilation"/> to get the <see cref="INamedTypeSymbol"/>s from.</param>
		/// <exception cref="ArgumentNullException"><paramref name="compilation"/> is <see langword="null"/>.</exception>
		public static INamedTypeSymbol[] GetDurianTypes(CSharpCompilation compilation)
		{
			if (compilation is null)
			{
				throw new ArgumentNullException(nameof(compilation));
			}

			ModuleIdentity[] modules = GetAllModules();
			List<INamedTypeSymbol> symbols = new(modules.Length * 4);

			foreach (ModuleIdentity module in modules)
			{
				foreach (TypeIdentity type in module.Types)
				{
					INamedTypeSymbol? symbol = compilation.GetTypeByMetadataName(type.FullyQualifiedName);

					if (symbol is not null)
					{
						symbols.Add(symbol);
					}
				}
			}

			return symbols.ToArray();
		}

		/// <summary>
		/// Returns an array of <see cref="INamedTypeSymbol"/>s representing all types that are part of any of the specified Durian <paramref name="modules"/>.
		/// </summary>
		/// <param name="compilation"><see cref="CSharpCompilation"/> to get the <see cref="INamedTypeSymbol"/>s from.</param>
		/// <param name="modules">An array of <see cref="ModuleIdentity"/> representing Durian modules to get the types of.</param>
		/// <exception cref="ArgumentNullException"><paramref name="compilation"/> is <see langword="null"/>.</exception>
		public static INamedTypeSymbol[] GetDurianTypes(CSharpCompilation compilation, ModuleIdentity[]? modules)
		{
			if (compilation is null)
			{
				throw new ArgumentNullException(nameof(compilation));
			}

			if (modules is null || modules.Length == 0)
			{
				return Array.Empty<INamedTypeSymbol>();
			}

			return GetDurianTypes_Internal(compilation, modules);
		}

		/// <summary>
		/// Returns an array of <see cref="INamedTypeSymbol"/>s representing all types that are part of any of the specified Durian <paramref name="modules"/>.
		/// </summary>
		/// <param name="compilation"><see cref="CSharpCompilation"/> to get the <see cref="INamedTypeSymbol"/>s from.</param>
		/// <param name="modules">An array of <see cref="DurianModule"/>s representing Durian modules to get the types of.</param>
		/// <exception cref="ArgumentNullException"><paramref name="compilation"/> is <see langword="null"/>.</exception>
		/// <exception cref="InvalidOperationException">Unknown <see cref="DurianModule"/> value detected. -or- <see cref="DurianModule.None"/> is not a valid Durian module.</exception>
		public static INamedTypeSymbol[] GetDurianTypes(CSharpCompilation compilation, DurianModule[]? modules)
		{
			if (compilation is null)
			{
				throw new ArgumentNullException(nameof(compilation));
			}

			if (modules is null || modules.Length == 0)
			{
				return Array.Empty<INamedTypeSymbol>();
			}

			int length = modules.Length;
			ModuleIdentity[] identities = new ModuleIdentity[length];

			for (int i = 0; i < length; i++)
			{
				identities[i] = GetModule(modules[i]);
			}

			return GetDurianTypes_Internal(compilation, identities);
		}

		/// <summary>
		/// Returns an array of <see cref="INamedTypeSymbol"/>s representing all types that are part of any enabled Durian module.
		/// </summary>
		/// <param name="compilation"><see cref="CSharpCompilation"/> to get the <see cref="INamedTypeSymbol"/>s from.</param>
		/// <exception cref="ArgumentNullException"><paramref name="compilation"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentException"><paramref name="compilation"/> does not contain the <see cref="EnableModuleAttribute"/>.</exception>
		public static INamedTypeSymbol[] GetEnabledDurianTypes(CSharpCompilation compilation)
		{
			if (compilation is null)
			{
				throw new ArgumentNullException(nameof(compilation));
			}

			return GetEnabledDurianTypes(compilation, GetAllModules());
		}

		/// <summary>
		/// Returns an array of <see cref="INamedTypeSymbol"/>s representing all types that are part of only those of the specified Durian <paramref name="modules"/> that are enabled for the current <paramref name="compilation"/>.
		/// </summary>
		/// <param name="compilation"><see cref="CSharpCompilation"/> to get the <see cref="INamedTypeSymbol"/>s from.</param>
		/// <param name="modules">An array of <see cref="ModuleIdentity"/> representing Durian modules to get the types of.</param>
		/// <exception cref="ArgumentNullException"><paramref name="compilation"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentException"><paramref name="compilation"/> does not contain the <see cref="EnableModuleAttribute"/>.</exception>
		public static INamedTypeSymbol[] GetEnabledDurianTypes(CSharpCompilation compilation, ModuleIdentity[]? modules)
		{
			if (compilation is null)
			{
				throw new ArgumentNullException(nameof(compilation));
			}

			if (modules is null || modules.Length == 0)
			{
				return Array.Empty<INamedTypeSymbol>();
			}

			return GetEnabledDurianTypes_Internal(compilation, modules);
		}

		/// <summary>
		/// Returns an array of <see cref="INamedTypeSymbol"/>s representing all types that are part of only those of the specified Durian <paramref name="modules"/> that are enabled for the current <paramref name="compilation"/>.
		/// </summary>
		/// <param name="compilation"><see cref="CSharpCompilation"/> to get the <see cref="INamedTypeSymbol"/>s from.</param>
		/// <param name="modules">An array of <see cref="DurianModule"/>s representing Durian modules to get the types of.</param>
		/// <exception cref="ArgumentNullException"><paramref name="compilation"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentException"><paramref name="compilation"/> does not contain the <see cref="EnableModuleAttribute"/>.</exception>
		/// <exception cref="InvalidOperationException">Unknown <see cref="DurianModule"/> value detected. -or- <see cref="DurianModule.None"/> is not a valid Durian module.</exception>
		public static INamedTypeSymbol[] GetEnabledDurianTypes(CSharpCompilation compilation, DurianModule[]? modules)
		{
			if (compilation is null)
			{
				throw new ArgumentNullException(nameof(compilation));
			}

			if (modules is null || modules.Length == 0)
			{
				return Array.Empty<INamedTypeSymbol>();
			}

			int length = modules.Length;
			ModuleIdentity[] identities = new ModuleIdentity[length];

			for (int i = 0; i < length; i++)
			{
				identities[i] = GetModule(modules[i]);
			}

			return GetEnabledDurianTypes_Internal(compilation, identities);
		}

		/// <summary>
		/// Returns an array of <see cref="INamedTypeSymbol"/>s representing all types that are part of any disabled Durian module.
		/// </summary>
		/// <param name="compilation"><see cref="CSharpCompilation"/> to get the <see cref="INamedTypeSymbol"/>s from.</param>
		/// <exception cref="ArgumentNullException"><paramref name="compilation"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentException"><paramref name="compilation"/> does not contain the <see cref="EnableModuleAttribute"/>.</exception>
		public static INamedTypeSymbol[] GetDisabledDurianTypes(CSharpCompilation compilation)
		{
			if (compilation is null)
			{
				throw new ArgumentNullException(nameof(compilation));
			}

			return GetDisabledDurianTypes(compilation, GetAllModules());
		}

		/// <summary>
		/// Returns an array of <see cref="INamedTypeSymbol"/>s representing all types that are part of only those of the specified Durian <paramref name="modules"/> that are disabled for the current <paramref name="compilation"/>.
		/// </summary>
		/// <param name="compilation"><see cref="CSharpCompilation"/> to get the <see cref="INamedTypeSymbol"/>s from.</param>
		/// <param name="modules">An array of <see cref="ModuleIdentity"/> representing Durian modules to get the types of.</param>
		/// <exception cref="ArgumentNullException"><paramref name="compilation"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentException"><paramref name="compilation"/> does not contain the <see cref="EnableModuleAttribute"/>.</exception>
		public static INamedTypeSymbol[] GetDisabledDurianTypes(CSharpCompilation compilation, ModuleIdentity[]? modules)
		{
			if (compilation is null)
			{
				throw new ArgumentNullException(nameof(compilation));
			}

			if (modules is null || modules.Length == 0)
			{
				return Array.Empty<INamedTypeSymbol>();
			}

			return GetDisabledDurianTypes_Internal(compilation, modules);
		}

		/// <summary>
		/// Returns an array of <see cref="INamedTypeSymbol"/>s representing all types that are part of only those of the specified Durian <paramref name="modules"/> that are disabled for the current <paramref name="compilation"/>.
		/// </summary>
		/// <param name="compilation"><see cref="CSharpCompilation"/> to get the <see cref="INamedTypeSymbol"/>s from.</param>
		/// <param name="modules">An array of <see cref="DurianModule"/>s representing Durian modules to get the types of.</param>
		/// <exception cref="ArgumentNullException"><paramref name="compilation"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentException"><paramref name="compilation"/> does not contain the <see cref="EnableModuleAttribute"/>.</exception>
		/// <exception cref="InvalidOperationException">Unknown <see cref="DurianModule"/> value detected. -or- <see cref="DurianModule.None"/> is not a valid Durian module.</exception>
		public static INamedTypeSymbol[] GetDisabledDurianTypes(CSharpCompilation compilation, DurianModule[]? modules)
		{
			if (compilation is null)
			{
				throw new ArgumentNullException(nameof(compilation));
			}

			if (modules is null || modules.Length == 0)
			{
				return Array.Empty<INamedTypeSymbol>();
			}

			int length = modules.Length;
			ModuleIdentity[] identities = new ModuleIdentity[length];

			for (int i = 0; i < length; i++)
			{
				identities[i] = GetModule(modules[i]);
			}

			return GetDisabledDurianTypes_Internal(compilation, identities);
		}

		/// <summary>
		/// Determines whether the specified <paramref name="symbol"/> is part of any Durian module.
		/// </summary>
		/// <param name="symbol"><see cref="INamedTypeSymbol"/> to check if is part of any Durian module.</param>
		/// <exception cref="ArgumentNullException"><paramref name="symbol"/> is <see langword="null"/>.</exception>
		public static bool IsDurianType(INamedTypeSymbol symbol)
		{
			return IsDurianType(symbol, GetAllModules());
		}

		/// <summary>
		/// Determines whether the specified <paramref name="symbol"/> is part of any Durian module.
		/// </summary>
		/// <param name="symbol"><see cref="INamedTypeSymbol"/> to check if is part of any Durian module.</param>
		/// <param name="modules">An array of <see cref="ModuleIdentity"/> representing Durian modules to get the types of.</param>
		/// <exception cref="ArgumentNullException"><paramref name="symbol"/> is <see langword="null"/>.</exception>
		public static bool IsDurianType(INamedTypeSymbol symbol, ModuleIdentity[]? modules)
		{
			if (symbol is null)
			{
				throw new ArgumentNullException(nameof(symbol));
			}

			if (modules is null || modules.Length == 0)
			{
				return false;
			}

			if (symbol.ContainingNamespace is not null && !symbol.ContainingNamespace.Name.StartsWith("Durian"))
			{
				return false;
			}

			string fullName = symbol.ToString();

			foreach (ModuleIdentity module in modules)
			{
				foreach (TypeIdentity type in module.Types)
				{
					if (type.FullyQualifiedName == fullName)
					{
						return true;
					}
				}
			}

			return false;
		}

		/// <summary>
		/// Determines whether the specified <paramref name="symbol"/> is part of any Durian module.
		/// </summary>
		/// <param name="symbol"><see cref="INamedTypeSymbol"/> to check if is part of any Durian module.</param>
		/// <param name="modules">An array of <see cref="DurianModule"/> representing Durian modules to get the types of.</param>
		/// <exception cref="ArgumentNullException"><paramref name="symbol"/> is <see langword="null"/>.</exception>
		public static bool IsDurianType(INamedTypeSymbol symbol, DurianModule[]? modules)
		{
			if (symbol is null)
			{
				throw new ArgumentNullException(nameof(symbol));
			}

			if (modules is null || modules.Length == 0)
			{
				return false;
			}

			if (symbol.ContainingType is not null && !symbol.ContainingNamespace.Name.StartsWith("Durian"))
			{
				return false;
			}

			string fullName = symbol.ToString();

			foreach (DurianModule enumModule in modules)
			{
				ModuleIdentity module = GetModule(enumModule);

				foreach (TypeIdentity type in module.Types)
				{
					if (type.FullyQualifiedName == fullName)
					{
						return true;
					}
				}
			}

			return false;
		}

		private static INamedTypeSymbol[] GetDurianTypes_Internal(CSharpCompilation compilation, ModuleIdentity[] modules)
		{
#pragma warning disable RS1024 // Compare symbols correctly
			HashSet<INamedTypeSymbol> symbols = new(SymbolEqualityComparer.Default);
#pragma warning restore RS1024 // Compare symbols correctly

			foreach (ModuleIdentity module in modules)
			{
				foreach (TypeIdentity type in module.Types)
				{
					INamedTypeSymbol? symbol = compilation.GetTypeByMetadataName(type.FullyQualifiedName);

					if (symbol is not null)
					{
						symbols.Add(symbol);
					}
				}
			}

			INamedTypeSymbol[] array = new INamedTypeSymbol[symbols.Count];
			symbols.CopyTo(array);
			return array;
		}

		private static INamedTypeSymbol[] GetEnabledDurianTypes_Internal(CSharpCompilation compilation, ModuleIdentity[] modules)
		{
#pragma warning disable RS1024 // Compare symbols correctly
			HashSet<INamedTypeSymbol> symbols = new(SymbolEqualityComparer.Default);
#pragma warning restore RS1024 // Compare symbols correctly

			DurianModule[] enabledModules = GetEnabledModulesAsEnums(compilation, modules);

			foreach (ModuleIdentity module in modules)
			{
				if (!IsEnabled_Internal(module, enabledModules))
				{
					continue;
				}

				foreach (TypeIdentity type in module.Types)
				{
					INamedTypeSymbol? symbol = compilation.GetTypeByMetadataName(type.FullyQualifiedName);

					if (symbol is not null)
					{
						symbols.Add(symbol);
					}
				}
			}

			INamedTypeSymbol[] array = new INamedTypeSymbol[symbols.Count];
			symbols.CopyTo(array);
			return array;
		}

		private static INamedTypeSymbol[] GetDisabledDurianTypes_Internal(CSharpCompilation compilation, ModuleIdentity[] modules)
		{
#pragma warning disable RS1024 // Compare symbols correctly
			HashSet<INamedTypeSymbol> symbols = new(SymbolEqualityComparer.Default);
#pragma warning restore RS1024 // Compare symbols correctly

			DurianModule[] enabledModules = GetEnabledModulesAsEnums(compilation, modules);

			foreach (ModuleIdentity module in modules)
			{
				if (IsEnabled_Internal(module, enabledModules))
				{
					continue;
				}

				foreach (TypeIdentity type in module.Types)
				{
					INamedTypeSymbol? symbol = compilation.GetTypeByMetadataName(type.FullyQualifiedName);

					if (symbol is not null)
					{
						symbols.Add(symbol);
					}
				}
			}

			INamedTypeSymbol[] array = new INamedTypeSymbol[symbols.Count];
			symbols.CopyTo(array);
			return array;
		}

		private static bool IsEnabled_Internal(INamedTypeSymbol symbol, INamedTypeSymbol enableModuleAttribute, IEnumerable<AttributeData> attributes, out ModuleIdentity? module)
		{
			string fullName = symbol.ToString();

			foreach (AttributeData attribute in attributes)
			{
				if (SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, enableModuleAttribute) && attribute.TryGetConstructorArgumentValue(0, out int value))
				{
					DurianModule enumValue = (DurianModule)value;
					ModuleIdentity identity = GetModule(enumValue);

					foreach (TypeIdentity type in identity.Types)
					{
						if (type.FullyQualifiedName == fullName)
						{
							module = identity;
							return true;
						}
					}
				}
			}

			module = null;
			return false;
		}

		private static bool IsEnabled_Internal(ModuleIdentity module, DurianModule[] enabledModules)
		{
			DurianModule value = module.Module;

			foreach (DurianModule enabledModule in enabledModules)
			{
				if (value == enabledModule)
				{
					return true;
				}
			}

			return false;
		}

		private static bool IsEnabled_Internal(DurianModule module, IAssemblySymbol assembly, INamedTypeSymbol enableModuleAttribute)
		{
			foreach (AttributeData attribute in assembly.GetAttributes())
			{
				if (SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, enableModuleAttribute) && attribute.TryGetConstructorArgumentValue(0, out int value))
				{
					DurianModule argumentValue = (DurianModule)value;

					if (argumentValue == module)
					{
						return true;
					}
				}
			}

			return false;
		}

		private static INamedTypeSymbol GetEnableAttributeSymbol(CSharpCompilation compilation)
		{
			if (compilation.GetTypeByMetadataName(typeof(EnableModuleAttribute).ToString()) is not INamedTypeSymbol symbol)
			{
				throw new ArgumentException($"{nameof(compilation)} does not contain the {nameof(EnableModuleAttribute)}!");
			}

			return symbol;
		}
	}
}
