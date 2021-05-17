using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Durian.Generator;

namespace Durian.Info
{
	/// <summary>
	/// Contains basic information about a Durian module.
	/// </summary>
	[DebuggerDisplay("Name = {ToString()}")]
	public sealed record ModuleIdentity
	{
		/// <summary>
		/// Link to documentation regarding this module. -or- empty <see cref="string"/> if the module has no documentation.
		/// </summary>
		public string Documentation { get; }

		/// <summary>
		/// Enum representation of this module.
		/// </summary>
		public DurianModule Module { get; }

		/// <summary>
		/// A two-digit number that precedes the id of a diagnostic.
		/// </summary>
		public IdSection AnalysisId { get; }

		/// <summary>
		/// A collection of types that are part of this module.
		/// </summary>
		public ImmutableArray<TypeIdentity> Types { get; }

		/// <summary>
		/// A collection of diagnostics that can be reported by this module.
		/// </summary>
		public ImmutableArray<DiagnosticData> Diagnostics { get; }

		/// <summary>
		/// A collection of packages that are part of this module.
		/// </summary>
		public ImmutableArray<PackageIdentity> Packages { get; }

		internal ModuleIdentity(
			DurianModule module,
			int id,
			PackageIdentity[]? packages,
			string? docPath,
			DiagnosticData[]? diagnostics,
			TypeIdentity[]? types
		)
		{
			Module = module;
			AnalysisId = (IdSection)id;
			Documentation = docPath is not null ? @$"{DurianInfo.Repository}\{docPath}" : string.Empty;

			if (packages is null)
			{
				Packages = ImmutableArray.Create<PackageIdentity>();
			}
			else
			{
				foreach (PackageIdentity package in packages)
				{
					package.SetModule(this);
				}

				Packages = packages.ToImmutableArray();
			}

			if (types is null)
			{
				Types = ImmutableArray.Create<TypeIdentity>();
			}
			else
			{
				foreach (TypeIdentity type in types)
				{
					type.SetModule(this);
				}

				Types = types.ToImmutableArray();
			}

			if (diagnostics is null)
			{
				Diagnostics = ImmutableArray.Create<DiagnosticData>();
			}
			else
			{
				foreach (DiagnosticData diag in diagnostics)
				{
					diag.SetModule(this);
				}

				Diagnostics = diagnostics.ToImmutableArray();
			}
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			return Module.ToString();
		}

		/// <inheritdoc/>
		public override int GetHashCode()
		{
			int hashCode = -726504116;
			hashCode = (hashCode * -1521134295) + Documentation.GetHashCode();
			hashCode = (hashCode * -1521134295) + AnalysisId.GetHashCode();
			hashCode = (hashCode * -1521134295) + Module.GetHashCode();
			hashCode = (hashCode * -1521134295) + Types.GetHashCode();
			hashCode = (hashCode * -1521134295) + Diagnostics.GetHashCode();
			return hashCode;
		}

		/// <summary>
		/// Checks if the specified <paramref name="module"/> is enabled for the calling <see cref="Assembly"/>.
		/// </summary>
		/// <param name="module"><see cref="ModuleIdentity"/> representing a Durian module to check.</param>
		/// <exception cref="ArgumentNullException"><paramref name="module"/> is <see langword="null"/>.</exception>
		public static bool IsEnabled(ModuleIdentity module)
		{
			return IsEnabled(module, Assembly.GetCallingAssembly());
		}

		/// <summary>
		/// Checks if the specified <paramref name="module"/> is enabled for the specified <paramref name="assembly"/>.
		/// </summary>
		/// <param name="module"><see cref="ModuleIdentity"/> representing a Durian module to check.</param>
		/// <param name="assembly"><see cref="Assembly"/> to check if the <paramref name="module"/> is enabled for.</param>
		/// <exception cref="ArgumentNullException"><paramref name="module"/> is <see langword="null"/>. -or- <paramref name="assembly"/> is <see langword="null"/>.</exception>
		public static bool IsEnabled(ModuleIdentity module, Assembly assembly)
		{
			if (module is null)
			{
				throw new ArgumentNullException(nameof(module));
			}

			return IsEnabled(module.Module, assembly);
		}

		/// <summary>
		/// Checks if the specified <paramref name="module"/> is enabled for the calling <see cref="Assembly"/>.
		/// </summary>
		/// <param name="module"><see cref="DurianModule"/> to check.</param>
		/// <exception cref="InvalidOperationException">Unknown <see cref="DurianModule"/> value detected. -or- <see cref="DurianModule.None"/> is not a valid Durian module.</exception>
		public static bool IsEnabled(DurianModule module)
		{
			return IsEnabled(module, Assembly.GetCallingAssembly());
		}

		/// <summary>
		/// Checks if the specified <paramref name="module"/> is enabled for the specified <paramref name="assembly"/>.
		/// </summary>
		/// <param name="module"><see cref="DurianModule"/> to check.</param>
		/// <param name="assembly"><see cref="Assembly"/> to check if the <paramref name="module"/> is enabled for.</param>
		/// <exception cref="ArgumentNullException"><paramref name="assembly"/> is <see langword="null"/>.</exception>
		/// <exception cref="InvalidOperationException">Unknown <see cref="DurianModule"/> value detected. -or- <see cref="DurianModule.None"/> is not a valid Durian module.</exception>
		public static bool IsEnabled(DurianModule module, Assembly assembly)
		{
			if (assembly is null)
			{
				throw new ArgumentNullException(nameof(assembly));
			}

			CheckIsValidModuleEnum(module);

			foreach (EnableModuleAttribute attribute in assembly.GetCustomAttributes<EnableModuleAttribute>())
			{
				if (attribute.Module == module)
				{
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Checks if the Durian module with the specified <paramref name="moduleName"/> is enabled for the calling <see cref="Assembly"/>.
		/// </summary>
		/// <param name="moduleName">Name of the module to check.</param>
		/// <exception cref="ArgumentNullException"><paramref name="moduleName"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentException">Unknown Durian module name: <paramref name="moduleName"/>.</exception>
		public static bool IsEnabled(string moduleName)
		{
			return IsEnabled(moduleName, Assembly.GetCallingAssembly());
		}

		/// <summary>
		/// Checks if the Durian module with the specified <paramref name="moduleName"/> is enabled for the specified <paramref name="assembly"/>.
		/// </summary>
		/// <param name="moduleName">Name of the module to check.</param>
		/// <param name="assembly"><see cref="Assembly"/> to check if the module is enabled for.</param>
		/// <exception cref="ArgumentNullException"><paramref name="moduleName"/> is <see langword="null"/>. -or- <paramref name="assembly"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentException">Unknown Durian module name: <paramref name="moduleName"/>.</exception>
		public static bool IsEnabled(string moduleName, Assembly assembly)
		{
			if (moduleName is null)
			{
				throw new ArgumentNullException(nameof(moduleName));
			}

			if (assembly is null)
			{
				throw new ArgumentNullException(nameof(assembly));
			}

			DurianModule module = ParseModule(moduleName);
			return IsEnabled(module, assembly);
		}

		/// <summary>
		/// Returns an array of <see cref="ModuleIdentity"/> representing Durian modules that are enabled for the calling <see cref="Assembly"/>.
		/// </summary>
		public static ModuleIdentity[] GetEnabledModules()
		{
			return GetEnabledModules(Assembly.GetCallingAssembly());
		}

		/// <summary>
		/// Returns an array of <see cref="ModuleIdentity"/> representing Durian modules that are enabled for the specified <paramref name="assembly"/>.
		/// </summary>
		/// <param name="assembly"><see cref="Assembly"/> to get the enabled Durian modules of.</param>
		/// <exception cref="ArgumentNullException"><paramref name="assembly"/> is <see langword="null"/>.</exception>
		public static ModuleIdentity[] GetEnabledModules(Assembly assembly)
		{
			if (assembly is null)
			{
				throw new ArgumentNullException(nameof(assembly));
			}

			EnableModuleAttribute[] attrs = assembly.GetCustomAttributes<EnableModuleAttribute>().ToArray();
			int length = attrs.Length;
			ModuleIdentity[] modules = new ModuleIdentity[length];

			for (int i = 0; i < length; i++)
			{
				modules[i] = GetModule(attrs[i].Module);
			}

			return modules;
		}

		/// <summary>
		/// Returns an array of <see cref="ModuleIdentity"/> representing Durian modules that are enabled for the calling <see cref="Assembly"/>. Only <see cref="ModuleIdentity"/> that are present in the given array of <paramref name="modules"/> are included.
		/// </summary>
		/// <param name="modules">Array of <see cref="ModuleIdentity"/> to pick the enabled modules from.</param>
		public static ModuleIdentity[] GetEnabledModules(ModuleIdentity[] modules)
		{
			return GetEnabledModules(Assembly.GetCallingAssembly(), modules);
		}

		/// <summary>
		/// Returns an array of <see cref="ModuleIdentity"/> representing Durian modules that are enabled for the specified <paramref name="assembly"/>. Only <see cref="ModuleIdentity"/> that are present in the given array of <paramref name="modules"/> are included.
		/// </summary>
		/// <param name="assembly"><see cref="Assembly"/> to get the enabled Durian modules of.</param>
		/// <param name="modules">Array of <see cref="ModuleIdentity"/> to pick the enabled modules from.</param>
		/// <exception cref="ArgumentNullException"><paramref name="assembly"/> is <see langword="null"/>.</exception>
		public static ModuleIdentity[] GetEnabledModules(Assembly assembly, ModuleIdentity[]? modules)
		{
			if (assembly is null)
			{
				throw new ArgumentNullException(nameof(assembly));
			}

			if (modules is null || modules.Length == 0)
			{
				return Array.Empty<ModuleIdentity>();
			}

			HashSet<ModuleIdentity> set = new(ModuleEnumEqualityComparer.Instance);

			foreach (EnableModuleAttribute attribute in assembly.GetCustomAttributes<EnableModuleAttribute>())
			{
				foreach (ModuleIdentity module in modules)
				{
					if (module.Module == attribute.Module)
					{
						set.Add(module);
					}
				}
			}

			ModuleIdentity[] array = new ModuleIdentity[set.Count];
			set.CopyTo(array);
			return array;
		}

		/// <summary>
		/// Returns an array of <see cref="ModuleIdentity"/> representing Durian modules that are enabled for the calling <see cref="Assembly"/>. Only <see cref="DurianModule"/>s that are present in the given array of <paramref name="modules"/> are included.
		/// </summary>
		/// <param name="modules">Array of <see cref="DurianModule"/>s to pick the enabled modules from.</param>
		/// <exception cref="InvalidOperationException">Unknown <see cref="DurianModule"/> value detected. -or- <see cref="DurianModule.None"/> is not a valid Durian module.</exception>
		public static ModuleIdentity[] GetEnabledModules(DurianModule[]? modules)
		{
			return GetEnabledModules(Assembly.GetCallingAssembly(), modules);
		}

		/// <summary>
		/// Returns an array of <see cref="ModuleIdentity"/> representing Durian modules that are enabled for the specified <paramref name="assembly"/>. Only <see cref="DurianModule"/>s that are present in the given array of <paramref name="modules"/> are included.
		/// </summary>
		/// <param name="assembly"><see cref="Assembly"/> to get the enabled Durian modules of.</param>
		/// <param name="modules">Array of <see cref="DurianModule"/>s to pick the enabled modules from.</param>
		/// <exception cref="ArgumentNullException"><paramref name="assembly"/> is <see langword="null"/>.</exception>
		/// <exception cref="InvalidOperationException">Unknown <see cref="DurianModule"/> value detected. -or- <see cref="DurianModule.None"/> is not a valid Durian module.</exception>
		public static ModuleIdentity[] GetEnabledModules(Assembly assembly, DurianModule[]? modules)
		{
			if (assembly is null)
			{
				throw new ArgumentNullException(nameof(assembly));
			}

			if (modules is null || modules.Length == 0)
			{
				return Array.Empty<ModuleIdentity>();
			}

			HashSet<DurianModule> set = new();
			EnableModuleAttribute[] attributes = assembly.GetCustomAttributes<EnableModuleAttribute>().ToArray();
			List<ModuleIdentity> list = new(attributes.Length);

			foreach (EnableModuleAttribute attribute in attributes)
			{
				foreach (DurianModule module in modules)
				{
					if (module == attribute.Module && set.Add(module))
					{
						list.Add(GetModule(module));
					}
				}
			}

			return list.ToArray();
		}

		/// <summary>
		/// Returns an array of <see cref="DurianModule"/>s representing Durian modules that are enabled for the calling <see cref="Assembly"/>.
		/// </summary>
		public static DurianModule[] GetEnabledModulesAsEnums()
		{
			return GetEnabledModulesAsEnums(Assembly.GetCallingAssembly());
		}

		/// <summary>
		/// Returns an array of <see cref="DurianModule"/>s representing Durian modules that are enabled for the specified <paramref name="assembly"/>.
		/// </summary>
		/// <param name="assembly"><see cref="Assembly"/> to get the enabled Durian modules of.</param>
		/// <exception cref="ArgumentNullException"><paramref name="assembly"/> is <see langword="null"/>.</exception>
		public static DurianModule[] GetEnabledModulesAsEnums(Assembly assembly)
		{
			if (assembly is null)
			{
				throw new ArgumentNullException(nameof(assembly));
			}

			EnableModuleAttribute[] attrs = assembly.GetCustomAttributes<EnableModuleAttribute>().ToArray();
			int length = attrs.Length;
			DurianModule[] modules = new DurianModule[length];

			for (int i = 0; i < length; i++)
			{
				modules[i] = attrs[i].Module;
			}

			return modules;
		}

		/// <summary>
		/// Returns an array of <see cref="DurianModule"/>s representing Durian modules that are enabled for the calling <see cref="Assembly"/>. Only <see cref="ModuleIdentity"/> that are present in the given array of <paramref name="modules"/> are included.
		/// </summary>
		/// <param name="modules">Array of <see cref="ModuleIdentity"/> to pick the enabled modules from.</param>
		public static DurianModule[] GetEnabledModulesAsEnums(ModuleIdentity[]? modules)
		{
			return GetEnabledModulesAsEnums(Assembly.GetCallingAssembly(), modules);
		}

		/// <summary>
		/// Returns an array of <see cref="DurianModule"/>s representing Durian modules that are enabled for the specified <paramref name="assembly"/>. Only <see cref="ModuleIdentity"/> that are present in the given array of <paramref name="modules"/> are included.
		/// </summary>
		/// <param name="assembly"><see cref="Assembly"/> to get the enabled Durian modules of.</param>
		/// <param name="modules">Array of <see cref="ModuleIdentity"/> to pick the enabled modules from.</param>
		/// <exception cref="ArgumentNullException"><paramref name="assembly"/> is <see langword="null"/>.</exception>
		public static DurianModule[] GetEnabledModulesAsEnums(Assembly assembly, ModuleIdentity[]? modules)
		{
			if (assembly is null)
			{
				throw new ArgumentNullException(nameof(assembly));
			}

			if (modules is null || modules.Length == 0)
			{
				return Array.Empty<DurianModule>();
			}

			HashSet<DurianModule> set = new();

			foreach (EnableModuleAttribute attribute in assembly.GetCustomAttributes<EnableModuleAttribute>())
			{
				foreach (ModuleIdentity module in modules)
				{
					if (module.Module == attribute.Module)
					{
						set.Add(module.Module);
					}
				}
			}

			DurianModule[] array = new DurianModule[set.Count];
			set.CopyTo(array);
			return array;
		}

		/// <summary>
		/// Returns an array of <see cref="DurianModule"/>s representing Durian modules that are enabled for the calling <see cref="Assembly"/>. Only <see cref="DurianModule"/>s that are present in the given array of <paramref name="modules"/> are included.
		/// </summary>
		/// <param name="modules">Array of <see cref="DurianModule"/>s to pick the enabled modules from.</param>
		/// <exception cref="InvalidOperationException">Unknown <see cref="DurianModule"/> value detected. -or- <see cref="DurianModule.None"/> is not a valid Durian module.</exception>
		public static DurianModule[] GetEnabledModulesAsEnums(DurianModule[]? modules)
		{
			return GetEnabledModulesAsEnums(Assembly.GetCallingAssembly(), modules);
		}

		/// <summary>
		/// Returns an array of <see cref="DurianModule"/>s representing Durian modules that are enabled for the specified <paramref name="assembly"/>. Only <see cref="DurianModule"/>s that are present in the given array of <paramref name="modules"/> are included.
		/// </summary>
		/// <param name="assembly"><see cref="Assembly"/> to get the enabled Durian modules of.</param>
		/// <param name="modules">Array of <see cref="DurianModule"/>s to pick the enabled modules from.</param>
		/// <exception cref="ArgumentNullException"><paramref name="assembly"/> is <see langword="null"/>.</exception>
		/// <exception cref="InvalidOperationException">Unknown <see cref="DurianModule"/> value detected. -or- <see cref="DurianModule.None"/> is not a valid Durian module.</exception>
		public static DurianModule[] GetEnabledModulesAsEnums(Assembly assembly, DurianModule[]? modules)
		{
			if (assembly is null)
			{
				throw new ArgumentNullException(nameof(assembly));
			}

			if (modules is null || modules.Length == 0)
			{
				return Array.Empty<DurianModule>();
			}

			HashSet<DurianModule> set = new();

			foreach (EnableModuleAttribute attribute in assembly.GetCustomAttributes<EnableModuleAttribute>().ToArray())
			{
				foreach (DurianModule module in modules)
				{
					if (module == attribute.Module)
					{
						set.Add(module);
					}
				}
			}

			DurianModule[] array = new DurianModule[set.Count];
			set.CopyTo(array);
			return array;
		}

		/// <summary>
		/// Returns an array of <see cref="ModuleIdentity"/> representing Durian modules that are disabled for the calling <see cref="Assembly"/>.
		/// </summary>
		public static ModuleIdentity[] GetDisabledModules()
		{
			return GetDisabledModules(Assembly.GetCallingAssembly());
		}

		/// <summary>
		/// Returns an array of <see cref="ModuleIdentity"/> representing Durian modules that are disabled for the specified <paramref name="assembly"/>.
		/// </summary>
		/// <param name="assembly"><see cref="Assembly"/> to get the disabled Durian modules of.</param>
		/// <exception cref="ArgumentNullException"><paramref name="assembly"/> is <see langword="null"/>.</exception>
		public static ModuleIdentity[] GetDisabledModules(Assembly assembly)
		{
			return GetDisabledModules(assembly, GetAllModules());
		}

		/// <summary>
		/// Returns an array of <see cref="ModuleIdentity"/> representing Durian modules that are enabled for the calling <see cref="Assembly"/>. Only <see cref="ModuleIdentity"/> that are present in the given array of <paramref name="modules"/> are included.
		/// </summary>
		/// <param name="modules">Array of <see cref="ModuleIdentity"/> to pick the enabled modules from.</param>
		public static ModuleIdentity[] GetDisabledModules(ModuleIdentity[]? modules)
		{
			return GetDisabledModules(Assembly.GetCallingAssembly(), modules);
		}

		/// <summary>
		/// Returns an array of <see cref="ModuleIdentity"/> representing Durian modules that are disabled for the specified <paramref name="assembly"/>. Only <see cref="ModuleIdentity"/> that are present in the given array of <paramref name="modules"/> are included.
		/// </summary>
		/// <param name="assembly"><see cref="Assembly"/> to get the disabled Durian modules of.</param>
		/// <param name="modules">Array of <see cref="ModuleIdentity"/> to pick the disabled modules from.</param>
		/// <exception cref="ArgumentNullException"><paramref name="assembly"/> is <see langword="null"/>.</exception>
		public static ModuleIdentity[] GetDisabledModules(Assembly assembly, ModuleIdentity[]? modules)
		{
			if (assembly is null)
			{
				throw new ArgumentNullException(nameof(assembly));
			}

			if (modules is null || modules.Length == 0)
			{
				return Array.Empty<ModuleIdentity>();
			}

			ModuleIdentity[] enabledModules = GetEnabledModules(assembly, modules);

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
		/// Returns an array of <see cref="ModuleIdentity"/> representing Durian modules that are disabled for the calling <see cref="Assembly"/>. Only <see cref="DurianModule"/>s that are present in the given array of <paramref name="modules"/> are included.
		/// </summary>
		/// <param name="modules">Array of <see cref="DurianModule"/>s to pick the disabled modules from.</param>
		/// <exception cref="InvalidOperationException">Unknown <see cref="DurianModule"/> value detected. -or- <see cref="DurianModule.None"/> is not a valid Durian module.</exception>
		public static ModuleIdentity[] GetDisabledModules(DurianModule[]? modules)
		{
			return GetDisabledModules(Assembly.GetCallingAssembly(), modules);
		}

		/// <summary>
		/// Returns an array of <see cref="ModuleIdentity"/> representing Durian modules that are disabled for the specified <paramref name="assembly"/>. Only <see cref="DurianModule"/>s that are present in the given array of <paramref name="modules"/> are included.
		/// </summary>
		/// <param name="assembly"><see cref="Assembly"/> to get the disabled Durian modules of.</param>
		/// <param name="modules">Array of <see cref="DurianModule"/>s to pick the disabled modules from.</param>
		/// <exception cref="ArgumentNullException"><paramref name="assembly"/> is <see langword="null"/>.</exception>
		/// <exception cref="InvalidOperationException">Unknown <see cref="DurianModule"/> value detected. -or- <see cref="DurianModule.None"/> is not a valid Durian module.</exception>
		public static ModuleIdentity[] GetDisabledModules(Assembly assembly, DurianModule[]? modules)
		{
			if (assembly is null)
			{
				throw new ArgumentNullException(nameof(assembly));
			}

			if (modules is null || modules.Length == 0)
			{
				return Array.Empty<ModuleIdentity>();
			}

			ModuleIdentity[] allModules = EnumArrayToIdentityArray(modules);
			ModuleIdentity[] enabledModules = GetEnabledModules(assembly, allModules);

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
		/// Returns an array of <see cref="DurianModule"/>s representing Durian modules that are disabled for the calling <see cref="Assembly"/>.
		/// </summary>
		public static DurianModule[] GetDisabledModulesAsEnums()
		{
			return GetDisabledModulesAsEnums(Assembly.GetCallingAssembly());
		}

		/// <summary>
		/// Returns an array of <see cref="DurianModule"/>s representing Durian modules that are disabled for the specified <paramref name="assembly"/>.
		/// </summary>
		/// <param name="assembly"><see cref="Assembly"/> to get the disabled Durian modules of.</param>
		/// <exception cref="ArgumentNullException"><paramref name="assembly"/> is <see langword="null"/>.</exception>
		public static DurianModule[] GetDisabledModulesAsEnums(Assembly assembly)
		{
			return GetDisabledModulesAsEnums(assembly, GetAllModules());
		}

		/// <summary>
		/// Returns an array of <see cref="DurianModule"/>s representing Durian modules that are enabled for the calling <see cref="Assembly"/>. Only <see cref="ModuleIdentity"/> that are present in the given array of <paramref name="modules"/> are included.
		/// </summary>
		/// <param name="modules">Array of <see cref="ModuleIdentity"/> to pick the enabled modules from.</param>
		public static DurianModule[] GetDisabledModulesAsEnums(ModuleIdentity[]? modules)
		{
			return GetDisabledModulesAsEnums(Assembly.GetCallingAssembly(), modules);
		}

		/// <summary>
		/// Returns an array of <see cref="DurianModule"/>s representing Durian modules that are disabled for the specified <paramref name="assembly"/>. Only <see cref="ModuleIdentity"/> that are present in the given array of <paramref name="modules"/> are included.
		/// </summary>
		/// <param name="assembly"><see cref="Assembly"/> to get the disabled Durian modules of.</param>
		/// <param name="modules">Array of <see cref="ModuleIdentity"/> to pick the disabled modules from.</param>
		/// <exception cref="ArgumentNullException"><paramref name="assembly"/> is <see langword="null"/>.</exception>
		public static DurianModule[] GetDisabledModulesAsEnums(Assembly assembly, ModuleIdentity[]? modules)
		{
			if (assembly is null)
			{
				throw new ArgumentNullException(nameof(assembly));
			}

			if (modules is null || modules.Length == 0)
			{
				return Array.Empty<DurianModule>();
			}

			ModuleIdentity[] enabledModules = GetEnabledModules(assembly, modules);

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
		/// Returns an array of <see cref="DurianModule"/>s representing Durian modules that are disabled for the calling <see cref="Assembly"/>. Only <see cref="DurianModule"/>s that are present in the given array of <paramref name="modules"/> are included.
		/// </summary>
		/// <param name="modules">Array of <see cref="DurianModule"/>s to pick the disabled modules from.</param>
		/// <exception cref="InvalidOperationException">Unknown <see cref="DurianModule"/> value detected. -or- <see cref="DurianModule.None"/> is not a valid Durian module.</exception>
		public static DurianModule[] GetDisabledModulesAsEnums(DurianModule[]? modules)
		{
			return GetDisabledModulesAsEnums(Assembly.GetCallingAssembly(), modules);
		}

		/// <summary>
		/// Returns an array of <see cref="DurianModule"/>s representing Durian modules that are disabled for the specified <paramref name="assembly"/>. Only <see cref="DurianModule"/>s that are present in the given array of <paramref name="modules"/> are included.
		/// </summary>
		/// <param name="assembly"><see cref="Assembly"/> to get the disabled Durian modules of.</param>
		/// <param name="modules">Array of <see cref="DurianModule"/>s to pick the disabled modules from.</param>
		/// <exception cref="ArgumentNullException"><paramref name="assembly"/> is <see langword="null"/>.</exception>
		/// <exception cref="InvalidOperationException">Unknown <see cref="DurianModule"/> value detected. -or- <see cref="DurianModule.None"/> is not a valid Durian module.</exception>
		public static DurianModule[] GetDisabledModulesAsEnums(Assembly assembly, DurianModule[]? modules)
		{
			if (assembly is null)
			{
				throw new ArgumentNullException(nameof(assembly));
			}

			if (modules is null || modules.Length == 0)
			{
				return Array.Empty<DurianModule>();
			}

			ModuleIdentity[] allModules = EnumArrayToIdentityArray(modules);
			ModuleIdentity[] enabledModules = GetEnabledModules(assembly, allModules);

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
		/// Returns an array of <see cref="ModuleIdentity"/> of all existing Durian modules.
		/// </summary>
		public static ModuleIdentity[] GetAllModules()
		{
			return new ModuleIdentity[]
			{
				ModuleRepository.Core,
				ModuleRepository.DefaultParam,
			};
		}

		/// <summary>
		/// Returns an array of <see cref="DurianModule"/> values representing all existing Durian modules.
		/// </summary>
		public static DurianModule[] GetAllModulesAsEnums()
		{
			return new DurianModule[]
			{
				DurianModule.Core,
				DurianModule.DefaultParam,
			};
		}

		/// <summary>
		/// Returns a new instance of <see cref="ModuleIdentity"/> corresponding with the specified <see cref="DurianModule"/>.
		/// </summary>
		/// <param name="module"><see cref="DurianModule"/> to get <see cref="ModuleIdentity"/> for.</param>
		/// <exception cref="InvalidOperationException">Unknown <see cref="DurianModule"/> value detected. -or- <see cref="DurianModule.None"/> is not a valid Durian module.</exception>
		public static ModuleIdentity GetModule(DurianModule module)
		{
			return module switch
			{
				DurianModule.Core => ModuleRepository.Core,
				DurianModule.DefaultParam => ModuleRepository.DefaultParam,
				DurianModule.None => throw new InvalidOperationException($"{nameof(DurianModule)}.{nameof(DurianModule.None)} is not a valid Durian module!"),
				_ => throw new InvalidOperationException($"Unknown {nameof(DurianModule)} value: {module}!")
			};
		}

		/// <summary>
		/// Returns a new instance of <see cref="ModuleIdentity"/> of Durian module with the specified <paramref name="moduleName"/>.
		/// </summary>
		/// <param name="moduleName">Name of the Durian module to get the <see cref="ModuleIdentity"/> of.</param>
		/// <exception cref="ArgumentNullException"><paramref name="moduleName"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentException"><paramref name="moduleName"/> cannot be empty or white space only. -or- Unknown Durian module name: <paramref name="moduleName"/>.</exception>
		public static ModuleIdentity GetModule(string moduleName)
		{
			DurianModule module = ParseModule(moduleName);
			return GetModule(module);
		}

		internal static ModuleIdentity[] EnumArrayToIdentityArray(DurianModule[] modules)
		{
			int length = modules.Length;
			ModuleIdentity[] identities = new ModuleIdentity[length];

			for (int i = 0; i < length; i++)
			{
				identities[i] = ModuleIdentity.GetModule(modules[i]);
			}

			return identities;
		}

		internal static void CheckIsValidModuleEnum(DurianModule module)
		{
			if (module == DurianModule.None)
			{
				throw new InvalidOperationException($"{nameof(DurianModule)}.{nameof(DurianModule.None)} is not a valid Durian module!");
			}

			if (module < DurianModule.Core || module > DurianModule.DefaultParam)
			{
				throw new InvalidOperationException($"Unknown {nameof(DurianModule)} value: {module}!");
			}
		}

		internal static DurianModule ParseModule(string moduleName)
		{
			if (moduleName is null)
			{
				throw new ArgumentNullException(nameof(moduleName));
			}

			string name = moduleName.Replace("Durian.", "").Replace(".", "");

			try
			{
				return (DurianModule)Enum.Parse(typeof(DurianModule), name);
			}
			catch
			{
				throw new ArgumentException($"Unknown Durian module name: {moduleName}", nameof(moduleName));
			}
		}
	}
}
