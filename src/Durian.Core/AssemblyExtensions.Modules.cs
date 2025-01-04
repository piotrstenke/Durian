using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Durian.Generator;

namespace Durian.Info
{
	public static partial class AssemblyExtensions
	{
		/// <summary>
		/// Returns a collection of all Durian modules that are disabled for the specified <paramref name="assembly"/>.
		/// </summary>
		/// <param name="assembly"><see cref="Assembly"/> to get all the disabled Durian modules of.</param>
		/// <returns>A new instance of <see cref="ModuleContainer"/> that contains the disabled Durian modules.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="assembly"/> is <see langword="null"/>.</exception>
		public static ModuleContainer GetDisabledModules(this Assembly assembly)
		{
			if (assembly is null)
			{
				throw new ArgumentNullException(nameof(assembly));
			}

			ModuleContainer all = ModuleIdentity.GetAllModules();

			return assembly.GetDisabledModules(all.AsEnums());
		}

		/// <summary>
		/// Returns a collection of all Durian modules present in the provided collection of <paramref name="modules"/> that are disabled for the specified <paramref name="assembly"/>.
		/// </summary>
		/// <param name="assembly"><see cref="Assembly"/> to get the disabled Durian modules from.</param>
		/// <param name="modules"><see cref="ModuleContainer"/> that provides a collection of Durian modules to pick from.</param>
		/// <returns>A new instance of <see cref="ModuleContainer"/> that contains the disabled Durian modules.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="assembly"/> is <see langword="null"/>. -or- <paramref name="modules"/> is <see langword="null"/>.</exception>
		public static ModuleContainer GetDisabledModules(this Assembly assembly, ModuleContainer modules)
		{
			if (assembly is null)
			{
				throw new ArgumentNullException(nameof(assembly));
			}

			if (modules is null)
			{
				throw new ArgumentNullException(nameof(modules));
			}

			if (modules.Count == 0)
			{
				return new ModuleContainer();
			}

			return assembly.GetDisabledModules(modules.AsEnums());
		}

		/// <summary>
		/// Returns a collection of all Durian modules present in the provided array of <paramref name="references"/> that are disabled for the specified <paramref name="assembly"/>.
		/// </summary>
		/// <param name="assembly"><see cref="Assembly"/> to get the disabled Durian modules of.</param>
		/// <param name="references">Array of <see cref="ModuleReference"/>s to pick from.</param>
		/// <returns>A new instance of <see cref="ModuleContainer"/> that contains the disabled Durian modules.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="assembly"/> is <see langword="null"/>.</exception>
		public static ModuleContainer GetDisabledModules(this Assembly assembly, params ModuleReference[]? references)
		{
			if (assembly is null)
			{
				throw new ArgumentNullException(nameof(assembly));
			}

			if (references is null || references.Length == 0)
			{
				return new ModuleContainer();
			}

			EnableModuleAttribute[] attributes = assembly.GetCustomAttributes<EnableModuleAttribute>().ToArray();

			if (attributes.Length == 0)
			{
				return new ModuleContainer();
			}

			ModuleContainer container = new(references.Length);

			foreach (ModuleReference reference in references)
			{
				if (reference is null)
				{
					continue;
				}

				if (!container.Contains(reference.EnumValue) && !IsEnabled_Internal(attributes, reference.EnumValue))
				{
					container.Include(reference);
				}
			}

			return container;
		}

		/// <summary>
		/// Returns a collection of all Durian modules present in the provided array of <paramref name="modules"/> that are disabled for the specified <paramref name="assembly"/>.
		/// </summary>
		/// <param name="assembly"><see cref="Assembly"/> to get the disabled Durian modules of.</param>
		/// <param name="modules">Array of <see cref="ModuleIdentity"/>s to pick from.</param>
		/// <returns>A new instance of <see cref="ModuleContainer"/> that contains the disabled Durian modules.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="assembly"/> is <see langword="null"/>.</exception>
		public static ModuleContainer GetDisabledModules(this Assembly assembly, params ModuleIdentity[]? modules)
		{
			if (assembly is null)
			{
				throw new ArgumentNullException(nameof(assembly));
			}

			if (modules is null || modules.Length == 0)
			{
				return new ModuleContainer();
			}

			EnableModuleAttribute[] attributes = assembly.GetCustomAttributes<EnableModuleAttribute>().ToArray();

			if (attributes.Length == 0)
			{
				return new ModuleContainer();
			}

			ModuleContainer container = new(modules.Length);

			foreach (ModuleIdentity module in modules)
			{
				if (module is null)
				{
					continue;
				}

				if (!container.Contains(module.Module) && !IsEnabled_Internal(attributes, module.Module))
				{
					container.Include(module);
				}
			}

			return container;
		}

		/// <summary>
		/// Returns a collection of all Durian modules present in the provided array of <paramref name="modules"/> that are disabled for the specified <paramref name="assembly"/>.
		/// </summary>
		/// <param name="assembly"><see cref="Assembly"/> to get the disabled Durian modules of.</param>
		/// <param name="modules">Array of <see cref="DurianModule"/>s to pick the disabled modules from.</param>
		/// <returns>A new instance of <see cref="ModuleContainer"/> that contains the disabled Durian modules.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="assembly"/> is <see langword="null"/>.</exception>
		/// <exception cref="InvalidOperationException">Unknown <see cref="DurianModule"/> value detected. -or- <see cref="DurianModule.None"/> is not a valid Durian module.</exception>
		public static ModuleContainer GetDisabledModules(this Assembly assembly, params DurianModule[]? modules)
		{
			if (assembly is null)
			{
				throw new ArgumentNullException(nameof(assembly));
			}

			if (modules is null || modules.Length == 0)
			{
				return new ModuleContainer();
			}

			foreach (DurianModule module in modules)
			{
				ModuleIdentity.EnsureIsValidModuleEnum_InvOp(module);
			}

			EnableModuleAttribute[] attributes = assembly.GetCustomAttributes<EnableModuleAttribute>().ToArray();

			if (attributes.Length == 0)
			{
				return new ModuleContainer();
			}

			ModuleContainer container = new(modules.Length);

			foreach (DurianModule module in modules)
			{
				if (!container.Contains(module) && !IsEnabled_Internal(attributes, module))
				{
					container.Include(module);
				}
			}

			return container;
		}

		/// <summary>
		/// Returns a collection of all Durian modules present in the provided array of <paramref name="references"/> that are enabled for the specified <paramref name="assembly"/>.
		/// </summary>
		/// <param name="assembly"><see cref="Assembly"/> to get the enabled Durian modules of.</param>
		/// <param name="references">Array of <see cref="ModuleReference"/>s to pick from.</param>
		/// <returns>A new instance of <see cref="ModuleContainer"/> that contains the enabled Durian modules.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="assembly"/> is <see langword="null"/>.</exception>
		public static ModuleContainer GetEnabledModules(this Assembly assembly, params ModuleReference[]? references)
		{
			if (assembly is null)
			{
				throw new ArgumentNullException(nameof(assembly));
			}

			if (references is null || references.Length == 0)
			{
				return new ModuleContainer();
			}

			EnableModuleAttribute[] attributes = assembly.GetCustomAttributes<EnableModuleAttribute>().ToArray();

			if (attributes.Length == 0)
			{
				return new ModuleContainer();
			}

			ModuleContainer container = new(references.Length);

			foreach (ModuleReference reference in references)
			{
				if (reference is null)
				{
					continue;
				}

				if (!container.Contains(reference.EnumValue) && IsEnabled_Internal(attributes, reference.EnumValue))
				{
					container.Include(reference);
				}
			}

			return container;
		}

		/// <summary>
		/// Returns a collection of all Durian modules present in the provided array of <paramref name="modules"/> that are enabled for the specified <paramref name="assembly"/>.
		/// </summary>
		/// <param name="assembly"><see cref="Assembly"/> to get the enabled Durian modules of.</param>
		/// <param name="modules">Array of <see cref="DurianModule"/>s to pick the enabled modules from.</param>
		/// <returns>A new instance of <see cref="ModuleContainer"/> that contains the enabled Durian modules.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="assembly"/> is <see langword="null"/>.</exception>
		/// <exception cref="InvalidOperationException">Unknown <see cref="DurianModule"/> value detected. -or- <see cref="DurianModule.None"/> is not a valid Durian module.</exception>
		public static ModuleContainer GetEnabledModules(this Assembly assembly, params DurianModule[]? modules)
		{
			if (assembly is null)
			{
				throw new ArgumentNullException(nameof(assembly));
			}

			if (modules is null || modules.Length == 0)
			{
				return new ModuleContainer();
			}

			foreach (DurianModule module in modules)
			{
				ModuleIdentity.EnsureIsValidModuleEnum_InvOp(module);
			}

			EnableModuleAttribute[] attributes = assembly.GetCustomAttributes<EnableModuleAttribute>().ToArray();

			if (attributes.Length == 0)
			{
				return new ModuleContainer();
			}

			ModuleContainer container = new(modules.Length);

			foreach (DurianModule module in modules)
			{
				if (!container.Contains(module) && IsEnabled_Internal(attributes, module))
				{
					container.Include(module);
				}
			}

			return container;
		}

		/// <summary>
		/// Returns a collection of all Durian modules that are enabled for the specified <paramref name="assembly"/>.
		/// </summary>
		/// <param name="assembly"><see cref="Assembly"/> to get all the enabled Durian modules of.</param>
		/// <returns>A new instance of <see cref="ModuleContainer"/> that contains the enabled Durian modules.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="assembly"/> is <see langword="null"/>.</exception>
		public static ModuleContainer GetEnabledModules(this Assembly assembly)
		{
			if (assembly is null)
			{
				throw new ArgumentNullException(nameof(assembly));
			}

			ModuleContainer all = ModuleIdentity.GetAllModules();

			return assembly.GetEnabledModules(all.AsEnums());
		}

		/// <summary>
		/// Returns a collection of all Durian modules present in the provided collection of <paramref name="modules"/> that are enabled for the specified <paramref name="assembly"/>.
		/// </summary>
		/// <param name="assembly"><see cref="Assembly"/> to get the enabled Durian modules from.</param>
		/// <param name="modules"><see cref="ModuleContainer"/> that provides a collection of Durian modules to pick from.</param>
		/// <returns>A new instance of <see cref="ModuleContainer"/> that contains the enabled Durian modules.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="assembly"/> is <see langword="null"/>. -or- <paramref name="modules"/> is <see langword="null"/>.</exception>
		public static ModuleContainer GetEnabledModules(this Assembly assembly, ModuleContainer modules)
		{
			if (assembly is null)
			{
				throw new ArgumentNullException(nameof(assembly));
			}

			if (modules is null)
			{
				throw new ArgumentNullException(nameof(modules));
			}

			if (modules.Count == 0)
			{
				return new ModuleContainer();
			}

			return assembly.GetEnabledModules(modules.AsEnums());
		}

		/// <summary>
		/// Returns a collection of all Durian modules present in the provided array of <paramref name="modules"/> that are enabled for the specified <paramref name="assembly"/>.
		/// </summary>
		/// <param name="assembly"><see cref="Assembly"/> to get the enabled Durian modules of.</param>
		/// <param name="modules">Array of <see cref="ModuleIdentity"/>s to pick from.</param>
		/// <returns>A new instance of <see cref="ModuleContainer"/> that contains the enabled Durian modules.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="assembly"/> is <see langword="null"/>.</exception>
		public static ModuleContainer GetEnabledModules(this Assembly assembly, params ModuleIdentity[]? modules)
		{
			if (assembly is null)
			{
				throw new ArgumentNullException(nameof(assembly));
			}

			if (modules is null || modules.Length == 0)
			{
				return new ModuleContainer();
			}

			EnableModuleAttribute[] attributes = assembly.GetCustomAttributes<EnableModuleAttribute>().ToArray();

			if (attributes.Length == 0)
			{
				return new ModuleContainer();
			}

			ModuleContainer container = new(modules.Length);

			foreach (ModuleIdentity module in modules)
			{
				if (module is null)
				{
					continue;
				}

				if (!container.Contains(module.Module) && IsEnabled_Internal(attributes, module.Module))
				{
					container.Include(module);
				}
			}

			return container;
		}

		/// <summary>
		/// Determines whether the specified <paramref name="module"/> is enabled for the given <paramref name="assembly"/>.
		/// </summary>
		/// <param name="assembly"><see cref="Assembly"/> to check if the <paramref name="module"/> is enabled for.</param>
		/// <param name="module"><see cref="ModuleReference"/> of Durian module to check for.</param>
		/// <exception cref="ArgumentNullException"><paramref name="module"/> is <see langword="null"/>. -or- <paramref name="assembly"/> is <see langword="null"/>.</exception>
		public static bool IsEnabled(this Assembly assembly, ModuleReference module)
		{
			if (assembly is null)
			{
				throw new ArgumentNullException(nameof(assembly));
			}

			if (module is null)
			{
				throw new ArgumentNullException(nameof(module));
			}

			return IsEnabled_Internal(assembly, module.EnumValue);
		}

		/// <summary>
		/// Determines whether the specified <paramref name="module"/> is enabled for the given <paramref name="assembly"/>.
		/// </summary>
		/// <param name="assembly"><see cref="Assembly"/> to check if the <paramref name="module"/> is enabled for.</param>
		/// <param name="module"><see cref="ModuleIdentity"/> representing a Durian module to check for.</param>
		/// <exception cref="ArgumentNullException"><paramref name="module"/> is <see langword="null"/>. -or- <paramref name="assembly"/> is <see langword="null"/>.</exception>
		public static bool IsEnabled(this Assembly assembly, ModuleIdentity module)
		{
			if (assembly is null)
			{
				throw new ArgumentNullException(nameof(assembly));
			}

			if (module is null)
			{
				throw new ArgumentNullException(nameof(module));
			}

			return IsEnabled_Internal(assembly, module.Module);
		}

		/// <summary>
		/// Determines whether the specified <paramref name="module"/> is enabled for the given <paramref name="assembly"/>.
		/// </summary>
		/// <param name="assembly"><see cref="Assembly"/> to check if the <paramref name="module"/> is enabled for.</param>
		/// <param name="module"><see cref="DurianModule"/> representing a Durian module to check for.</param>
		/// <exception cref="ArgumentNullException"><paramref name="assembly"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentException">Unknown <see cref="DurianModule"/> value detected. -or- <see cref="DurianModule.None"/> is not a valid Durian module.</exception>
		public static bool IsEnabled(this Assembly assembly, DurianModule module)
		{
			if (assembly is null)
			{
				throw new ArgumentNullException(nameof(assembly));
			}

			ModuleIdentity.EnsureIsValidModuleEnum(module);
			return IsEnabled_Internal(assembly, module);
		}

		/// <summary>
		/// Determines whether the specified <paramref name="assembly"/> references a Durian module with the given <paramref name="moduleName"/>.
		/// </summary>
		/// <param name="assembly"><see cref="Assembly"/> to check if contains the reference.</param>
		/// <param name="moduleName">Name of the Durian module to check for.</param>
		/// <exception cref="ArgumentNullException"><paramref name="moduleName"/> is <see langword="null"/>. -or- <paramref name="assembly"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentException">Unknown Durian module name: <paramref name="moduleName"/>.</exception>
		public static bool IsEnabled(this Assembly assembly, string moduleName)
		{
			if (assembly is null)
			{
				throw new ArgumentNullException(nameof(assembly));
			}

			DurianModule module = ModuleIdentity.Parse(moduleName);
			return IsEnabled_Internal(assembly, module);
		}

		private static bool IsEnabled_Internal(Assembly assembly, DurianModule module)
		{
			return IsEnabled_Internal(assembly.GetCustomAttributes<EnableModuleAttribute>(), module);
		}

		private static bool IsEnabled_Internal(IEnumerable<EnableModuleAttribute> attributes, DurianModule module)
		{
			foreach (EnableModuleAttribute attribute in attributes)
			{
				if (attribute.Module == module)
				{
					return true;
				}
			}

			return false;
		}
	}
}
