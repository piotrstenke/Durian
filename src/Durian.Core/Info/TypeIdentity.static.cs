using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Durian.Info
{
	public partial class TypeIdentity
	{
		/// <summary>
		/// Returns an array of all existing <see cref="TypeIdentity"/>.
		/// </summary>
		public static TypeIdentity[] GetAllTypes()
		{
			return GetAllTypes(ModuleIdentity.GetAllModules());
		}

		/// <summary>
		/// Returns an array of all <see cref="TypeIdentity"/> present in the specified <paramref name="modules"/>.
		/// </summary>
		/// <param name="modules">An array of <see cref="DurianModule"/> to get the <see cref="TypeIdentity"/> from.</param>
		/// <exception cref="InvalidOperationException">Unknown <see cref="DurianModule"/> value detected. -or- <see cref="DurianModule.None"/> is not a valid Durian module.</exception>
		public static TypeIdentity[] GetAllTypes(DurianModule[]? modules)
		{
			if (modules is null || modules.Length == 0)
			{
				return Array.Empty<TypeIdentity>();
			}

			int length = modules.Length;
			ModuleIdentity[] identities = new ModuleIdentity[length];

			for (int i = 0; i < length; i++)
			{
				identities[i] = ModuleIdentity.GetModule(modules[i]);
			}

			return GetAllTypes(identities);
		}

		/// <summary>
		/// Returns an array of all <see cref="TypeIdentity"/> present in the specified <paramref name="modules"/>.
		/// </summary>
		/// <param name="modules">An array of <see cref="ModuleIdentity"/> to get the <see cref="TypeIdentity"/> from.</param>
		public static TypeIdentity[] GetAllTypes(ModuleIdentity[]? modules)
		{
			if (modules is null || modules.Length == 0)
			{
				return Array.Empty<TypeIdentity>();
			}

			HashSet<TypeIdentity> types = new(EqualityComparer<TypeIdentity>.Default);

			foreach (ModuleIdentity module in modules)
			{
				foreach (TypeIdentity type in module.Types)
				{
					types.Add(type);
				}
			}

			TypeIdentity[] array = new TypeIdentity[types.Count];
			types.CopyTo(array);
			return array;
		}

		/// <summary>
		/// Returns an array of <see cref="TypeIdentity"/> representing all enabled Durian types for the calling <see cref="Assembly"/>.
		/// </summary>
		public static TypeIdentity[] GetEnabledTypes()
		{
			return GetEnabledTypes(Assembly.GetCallingAssembly());
		}

		/// <summary>
		/// Returns an array of <see cref="TypeIdentity"/> representing all enabled Durian types for the calling <see cref="Assembly"/>.
		/// </summary>
		/// <param name="assembly"><see cref="Assembly"/> to check the enabled <see cref="TypeIdentity"/> for.</param>
		/// <exception cref="ArgumentNullException"><paramref name="assembly"/> is <see langword="null"/>.</exception>
		public static TypeIdentity[] GetEnabledTypes(Assembly assembly)
		{
			ModuleIdentity[] identities = ModuleIdentity.GetEnabledModules(assembly);
			return GetAllTypes(identities);
		}

		/// <summary>
		/// Returns an array of <see cref="TypeIdentity"/> representing all enabled Durian types for the calling <see cref="Assembly"/>. Only <see cref="TypeIdentity"/> that are present in the given array of <paramref name="modules"/> are included.
		/// </summary>
		/// <param name="modules">Array of <see cref="DurianModule"/> to pick the enabled types from.</param>
		/// <exception cref="InvalidOperationException">Unknown <see cref="DurianModule"/> value detected. -or- <see cref="DurianModule.None"/> is not a valid Durian module.</exception>
		public static TypeIdentity[] GetEnabledTypes(DurianModule[]? modules)
		{
			return GetEnabledTypes(Assembly.GetCallingAssembly(), modules);
		}

		/// <summary>
		/// Returns an array of <see cref="TypeIdentity"/> representing all enabled Durian types for the specified <paramref name="assembly"/>. Only <see cref="TypeIdentity"/> that are present in the given array of <paramref name="modules"/> are included.
		/// </summary>
		/// <param name="assembly"><see cref="Assembly"/> to check the enabled <see cref="TypeIdentity"/> for.</param>
		/// <param name="modules">Array of <see cref="DurianModule"/> to pick the enabled types from.</param>
		/// <exception cref="ArgumentNullException"><paramref name="assembly"/> is <see langword="null"/>.</exception>
		/// <exception cref="InvalidOperationException">Unknown <see cref="DurianModule"/> value detected. -or- <see cref="DurianModule.None"/> is not a valid Durian module.</exception>
		public static TypeIdentity[] GetEnabledTypes(Assembly assembly, DurianModule[]? modules)
		{
			ModuleIdentity[] identities = ModuleIdentity.GetEnabledModules(assembly, modules);
			return GetAllTypes(identities);
		}

		/// <summary>
		/// Returns an array of <see cref="TypeIdentity"/> representing all enabled Durian types for the calling <see cref="Assembly"/>. Only <see cref="TypeIdentity"/> that are present in the given array of <paramref name="modules"/> are included.
		/// </summary>
		/// <param name="modules">Array of <see cref="ModuleIdentity"/> to pick the enabled types from.</param>
		public static TypeIdentity[] GetEnabledTypes(ModuleIdentity[]? modules)
		{
			return GetEnabledTypes(Assembly.GetCallingAssembly(), modules);
		}

		/// <summary>
		/// Returns an array of <see cref="TypeIdentity"/> representing all enabled Durian types for the specified <paramref name="assembly"/>. Only <see cref="TypeIdentity"/> that are present in the given array of <paramref name="modules"/> are included.
		/// </summary>
		/// <param name="assembly"><see cref="Assembly"/> to check the enabled <see cref="TypeIdentity"/> for.</param>
		/// <param name="modules">Array of <see cref="ModuleIdentity"/> to pick the enabled types from.</param>
		/// <exception cref="ArgumentNullException"><paramref name="assembly"/> is <see langword="null"/>.</exception>
		public static TypeIdentity[] GetEnabledTypes(Assembly assembly, ModuleIdentity[]? modules)
		{
			ModuleIdentity[] identities = ModuleIdentity.GetEnabledModules(assembly, modules);
			return GetAllTypes(identities);
		}

		/// <summary>
		/// Returns an array of <see cref="TypeIdentity"/> representing all disabled Durian types for the calling <see cref="Assembly"/>.
		/// </summary>
		public static TypeIdentity[] GetDisabledTypes()
		{
			return GetDisabledTypes(Assembly.GetCallingAssembly());
		}

		/// <summary>
		/// Returns an array of <see cref="TypeIdentity"/> representing all disabled Durian types for the calling <see cref="Assembly"/>.
		/// </summary>
		/// <param name="assembly"><see cref="Assembly"/> to check the enabled <see cref="TypeIdentity"/> for.</param>
		/// <exception cref="ArgumentNullException"><paramref name="assembly"/> is <see langword="null"/>.</exception>
		public static TypeIdentity[] GetDisabledTypes(Assembly assembly)
		{
			ModuleIdentity[] identities = ModuleIdentity.GetDisabledModules(assembly);
			return GetAllTypes(identities);
		}

		/// <summary>
		/// Returns an array of <see cref="TypeIdentity"/> representing all disabled Durian types for the calling <see cref="Assembly"/>. Only <see cref="TypeIdentity"/> that are present in the given array of <paramref name="modules"/> are included.
		/// </summary>
		/// <param name="modules">Array of <see cref="DurianModule"/> to pick the enabled types from.</param>
		/// <exception cref="InvalidOperationException">Unknown <see cref="DurianModule"/> value detected. -or- <see cref="DurianModule.None"/> is not a valid Durian module.</exception>
		public static TypeIdentity[] GetDisabledTypes(DurianModule[]? modules)
		{
			return GetDisabledTypes(Assembly.GetCallingAssembly(), modules);
		}

		/// <summary>
		/// Returns an array of <see cref="TypeIdentity"/> representing all disabled Durian types for the specified <paramref name="assembly"/>. Only <see cref="TypeIdentity"/> that are present in the given array of <paramref name="modules"/> are included.
		/// </summary>
		/// <param name="assembly"><see cref="Assembly"/> to check the enabled <see cref="TypeIdentity"/> for.</param>
		/// <param name="modules">Array of <see cref="DurianModule"/> to pick the enabled types from.</param>
		/// <exception cref="ArgumentNullException"><paramref name="assembly"/> is <see langword="null"/>.</exception>
		/// <exception cref="InvalidOperationException">Unknown <see cref="DurianModule"/> value detected. -or- <see cref="DurianModule.None"/> is not a valid Durian module.</exception>
		public static TypeIdentity[] GetDisabledTypes(Assembly assembly, DurianModule[]? modules)
		{
			ModuleIdentity[] identities = ModuleIdentity.GetDisabledModules(assembly, modules);
			return GetAllTypes(identities);
		}

		/// <summary>
		/// Returns an array of <see cref="TypeIdentity"/> representing all disabled Durian types for the calling <see cref="Assembly"/>. Only <see cref="TypeIdentity"/> that are present in the given array of <paramref name="modules"/> are included.
		/// </summary>
		/// <param name="modules">Array of <see cref="ModuleIdentity"/> to pick the enabled types from.</param>
		/// <exception cref="InvalidOperationException">Unknown <see cref="DurianModule"/> value detected. -or- <see cref="DurianModule.None"/> is not a valid Durian module.</exception>
		public static TypeIdentity[] GetDisabledTypes(ModuleIdentity[]? modules)
		{
			return GetDisabledTypes(Assembly.GetCallingAssembly(), modules);
		}

		/// <summary>
		/// Returns an array of <see cref="TypeIdentity"/> representing all disabled Durian types for the specified <paramref name="assembly"/>. Only <see cref="TypeIdentity"/> that are present in the given array of <paramref name="modules"/> are included.
		/// </summary>
		/// <param name="assembly"><see cref="Assembly"/> to check the enabled <see cref="TypeIdentity"/> for.</param>
		/// <param name="modules">Array of <see cref="ModuleIdentity"/> to pick the enabled types from.</param>
		/// <exception cref="ArgumentNullException"><paramref name="assembly"/> is <see langword="null"/>.</exception>
		public static TypeIdentity[] GetDisabledTypes(Assembly assembly, ModuleIdentity[]? modules)
		{
			ModuleIdentity[] identities = ModuleIdentity.GetEnabledModules(assembly, modules);
			return GetAllTypes(identities);
		}

		/// <summary>
		/// Determines whether the specified <paramref name="type"/> is enabled for the calling <see cref="Assembly"/>.
		/// </summary>
		/// <param name="type"><see cref="TypeIdentity"/> to check if is enabled.</param>
		/// <exception cref="ArgumentNullException"><paramref name="type"/> is <see langword="null"/>.</exception>
		public static bool IsEnabled(TypeIdentity type)
		{
			if (type is null)
			{
				throw new ArgumentNullException(nameof(type));
			}

			return IsEnabled_Internal(type, ModuleIdentity.GetAllModules(), Assembly.GetCallingAssembly());
		}

		/// <summary>
		/// Determines whether the specified <paramref name="type"/> is enabled for the specified <paramref name="assembly"/>.
		/// </summary>
		/// <param name="type"><see cref="TypeIdentity"/> to check if is enabled.</param>
		/// <param name="assembly"><see cref="Assembly"/> to check the <paramref name="type"/> for.</param>
		/// <exception cref="ArgumentNullException"><paramref name="type"/> is <see langword="null"/>. -or <paramref name="assembly"/> is <see langword="null"/>.</exception>
		public static bool IsEnabled(TypeIdentity type, Assembly assembly)
		{
			if (type is null)
			{
				throw new ArgumentNullException(nameof(type));
			}

			if (assembly is null)
			{
				throw new ArgumentNullException(nameof(assembly));
			}

			return IsEnabled_Internal(type, ModuleIdentity.GetAllModules(), assembly);
		}

		/// <summary>
		/// Determines whether the specified <paramref name="type"/> is enabled for the calling <see cref="Assembly"/>. Only <see cref="TypeIdentity"/> that are present in the given array of <paramref name="modules"/> are included.
		/// </summary>
		/// <param name="type"><see cref="TypeIdentity"/> to check if is enabled.</param>
		/// <param name="modules">Array of <see cref="ModuleIdentity"/> to pick the enabled modules from.</param>
		/// <exception cref="ArgumentNullException"><paramref name="type"/> is <see langword="null"/>.</exception>
		public static bool IsEnabled(TypeIdentity type, ModuleIdentity[]? modules)
		{
			if (type is null)
			{
				throw new ArgumentNullException(nameof(type));
			}

			if (modules is null || modules.Length == 0)
			{
				return false;
			}

			return IsEnabled_Internal(type, modules, Assembly.GetCallingAssembly());
		}

		/// <summary>
		/// Determines whether the specified <paramref name="type"/> is enabled for the specified <paramref name="assembly"/>. Only <see cref="TypeIdentity"/> that are present in the given array of <paramref name="modules"/> are included.
		/// </summary>
		/// <param name="type"><see cref="TypeIdentity"/> to check if is enabled.</param>
		/// <param name="assembly"><see cref="Assembly"/> to check the <paramref name="type"/> for.</param>
		/// <param name="modules">Array of <see cref="ModuleIdentity"/> to pick the enabled modules from.</param>
		/// <exception cref="ArgumentNullException"><paramref name="type"/> is <see langword="null"/>. -or <paramref name="assembly"/> is <see langword="null"/>.</exception>
		public static bool IsEnabled(TypeIdentity type, Assembly assembly, ModuleIdentity[]? modules)
		{
			if (type is null)
			{
				throw new ArgumentNullException(nameof(type));
			}

			if (assembly is null)
			{
				throw new ArgumentNullException(nameof(assembly));
			}

			if (modules is null || modules.Length == 0)
			{
				return false;
			}

			return IsEnabled_Internal(type, modules, assembly);
		}

		/// <summary>
		/// Determines whether the specified <paramref name="type"/> is enabled for the calling <see cref="Assembly"/>. Only <see cref="TypeIdentity"/> that are present in the given array of <paramref name="modules"/> are included.
		/// </summary>
		/// <param name="type"><see cref="TypeIdentity"/> to check if is enabled.</param>
		/// <param name="modules">Array of <see cref="DurianModule"/> to pick the enabled modules from.</param>
		/// <exception cref="ArgumentNullException"><paramref name="type"/> is <see langword="null"/>.</exception>
		/// <exception cref="InvalidOperationException">Unknown <see cref="DurianModule"/> value detected. -or- <see cref="DurianModule.None"/> is not a valid Durian module.</exception>
		public static bool IsEnabled(TypeIdentity type, DurianModule[]? modules)
		{
			if (type is null)
			{
				throw new ArgumentNullException(nameof(type));
			}

			if (modules is null || modules.Length == 0)
			{
				return false;
			}

			ModuleIdentity[] identities = new ModuleIdentity[modules.Length];

			for (int i = 0; i < modules.Length; i++)
			{
				identities[i] = ModuleIdentity.GetModule(modules[i]);
			}

			return IsEnabled_Internal(type, identities, Assembly.GetCallingAssembly());
		}

		/// <summary>
		/// Determines whether the specified <paramref name="type"/> is enabled for the specified <paramref name="assembly"/>. Only <see cref="TypeIdentity"/> that are present in the given array of <paramref name="modules"/> are included.
		/// </summary>
		/// <param name="type"><see cref="TypeIdentity"/> to check if is enabled.</param>
		/// <param name="assembly"><see cref="Assembly"/> to check the <paramref name="type"/> for.</param>
		/// <param name="modules">Array of <see cref="DurianModule"/> to pick the enabled modules from.</param>
		/// <exception cref="ArgumentNullException"><paramref name="type"/> is <see langword="null"/>. -or <paramref name="assembly"/> is <see langword="null"/>.</exception>
		/// <exception cref="InvalidOperationException">Unknown <see cref="DurianModule"/> value detected. -or- <see cref="DurianModule.None"/> is not a valid Durian module.</exception>
		public static bool IsEnabled(TypeIdentity type, Assembly assembly, DurianModule[]? modules)
		{
			if (type is null)
			{
				throw new ArgumentNullException(nameof(type));
			}

			if (assembly is null)
			{
				throw new ArgumentNullException(nameof(assembly));
			}

			if (modules is null || modules.Length == 0)
			{
				return false;
			}

			ModuleIdentity[] identities = new ModuleIdentity[modules.Length];

			for (int i = 0; i < modules.Length; i++)
			{
				identities[i] = ModuleIdentity.GetModule(modules[i]);
			}

			return IsEnabled_Internal(type, identities, assembly);
		}

		private static bool IsEnabled_Internal(TypeIdentity type, ModuleIdentity[] modules, Assembly assembly)
		{
			DurianModule[] enabled = ModuleIdentity.GetEnabledModulesAsEnums(assembly, modules);

			foreach (ModuleIdentity module in modules)
			{
				foreach (TypeIdentity t in module.Types)
				{
					if (t == type)
					{
						DurianModule m = module.Module;

						foreach (DurianModule e in enabled)
						{
							if (e == m)
							{
								return true;
							}
						}

						return false;
					}
				}
			}

			return false;
		}
	}
}
