using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

namespace Durian.Info
{
	public partial class ModuleIdentity
	{
		private static DurianModule[]? _allModules;

		private static DurianModule[] AllModules
		{
			get => _allModules ??= Enum.GetValues(typeof(DurianModule))
				.Cast<DurianModule>()
				.Skip(1)
				.ToArray();
		}

		/// <summary>
		/// Deallocates all cached instances of <see cref="ModuleIdentity"/>.
		/// </summary>
		public static void Deallocate()
		{
			IdentityPool.Modules.Clear();
			_allModules = default;
		}

		/// <summary>
		/// Throws an <see cref="InvalidOperationException"/> if the specified <paramref name="module"/> is not a valid <see cref="DurianModule"/> value.
		/// </summary>
		/// <param name="module"><see cref="DurianModule"/> to ensure that is valid.</param>
		/// <exception cref="ArgumentException">Unknown <see cref="DurianModule"/> value detected. -or- <see cref="DurianModule.None"/> is not a valid Durian module.</exception>
		public static void EnsureIsValidModuleEnum(DurianModule module)
		{
			if (module == DurianModule.None)
			{
				throw new ArgumentException($"{nameof(DurianModule)}.{nameof(DurianModule.None)} is not a valid Durian module!");
			}

			if (!GlobalInfo.IsValidModuleValue(module))
			{
				throw new ArgumentException($"Unknown {nameof(DurianModule)} value: {module}!");
			}
		}

		/// <summary>
		/// Returns a collection of all existing Durian modules.
		/// </summary>
		/// <returns>A new instance of <see cref="ModuleContainer"/> that contains all the existing Durian modules.</returns>
		public static ModuleContainer GetAllModules()
		{
			List<DurianModule> modules = new(AllModules);

			return new ModuleContainer(modules);
		}

		/// <summary>
		/// Returns a collection of all Durian modules that are disabled for the calling <see cref="Assembly"/>.
		/// </summary>
		/// <returns>A new instance of <see cref="ModuleContainer"/> that contains the disabled Durian modules.</returns>
		public static ModuleContainer GetDisabledModules()
		{
			return Assembly.GetCallingAssembly().GetDisabledModules();
		}

		/// <summary>
		/// Returns a collection of all Durian modules present in the provided array of <paramref name="references"/> that are disabled for the calling <see cref="Assembly"/>.
		/// </summary>
		/// <param name="references">Array of <see cref="ModuleReference"/>s to pick from.</param>
		/// <returns>A new instance of <see cref="ModuleContainer"/> that contains the disabled Durian modules.</returns>
		public static ModuleContainer GetDisabledModules(params ModuleReference[]? references)
		{
			return Assembly.GetCallingAssembly().GetDisabledModules(references);
		}

		/// <summary>
		/// Returns a collection of all Durian modules present in the provided collection of <paramref name="modules"/> that are disabled for the calling <see cref="Assembly"/>.
		/// </summary>
		/// <param name="modules"><see cref="ModuleContainer"/> that provides a collection of Durian modules to pick from.</param>
		/// <returns>A new instance of <see cref="ModuleContainer"/> that contains the disabled Durian modules.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="modules"/> is <see langword="null"/>.</exception>
		public static ModuleContainer GetDisabledModules(ModuleContainer modules)
		{
			return Assembly.GetCallingAssembly().GetDisabledModules(modules);
		}

		/// <summary>
		/// Returns a collection of all Durian modules present in the provided array of <paramref name="modules"/> that are disabled for the calling <see cref="Assembly"/>.
		/// </summary>
		/// <param name="modules">Array of <see cref="ModuleIdentity"/>s to pick from.</param>
		/// <returns>A new instance of <see cref="ModuleContainer"/> that contains the disabled Durian modules.</returns>
		public static ModuleContainer GetDisabledModules(params ModuleIdentity[]? modules)
		{
			return Assembly.GetCallingAssembly().GetDisabledModules(modules);
		}

		/// <summary>
		/// Returns a collection of all Durian modules present in the provided array of <paramref name="modules"/> that are disabled for the calling <see cref="Assembly"/>.
		/// </summary>
		/// <param name="modules">Array of <see cref="DurianModule"/>s to pick the disabled modules from.</param>
		/// <returns>A new instance of <see cref="ModuleContainer"/> that contains the disabled Durian modules.</returns>
		/// <exception cref="InvalidOperationException">Unknown <see cref="DurianModule"/> value detected. -or- <see cref="DurianModule.None"/> is not a valid Durian module.</exception>
		public static ModuleContainer GetDisabledModules(params DurianModule[]? modules)
		{
			return Assembly.GetCallingAssembly().GetDisabledModules(modules);
		}

		/// <summary>
		/// Returns a collection of all Durian modules that are enabled for the calling <see cref="Assembly"/>.
		/// </summary>
		/// <returns>A new instance of <see cref="ModuleContainer"/> that contains the enabled Durian modules.</returns>
		public static ModuleContainer GetEnabledModules()
		{
			return Assembly.GetCallingAssembly().GetEnabledModules();
		}

		/// <summary>
		/// Returns a collection of all Durian modules present in the provided collection of <paramref name="modules"/> that are enabled for the calling <see cref="Assembly"/>.
		/// </summary>
		/// <param name="modules"><see cref="ModuleContainer"/> that provides a collection of Durian modules to pick from.</param>
		/// <returns>A new instance of <see cref="ModuleContainer"/> that contains the enabled Durian modules.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="modules"/> is <see langword="null"/>.</exception>
		public static ModuleContainer GetEnabledModules(ModuleContainer modules)
		{
			return Assembly.GetCallingAssembly().GetEnabledModules(modules);
		}

		/// <summary>
		/// Returns a collection of all Durian modules present in the provided array of <paramref name="modules"/> that are enabled for the calling <see cref="Assembly"/>.
		/// </summary>
		/// <param name="modules">Array of <see cref="ModuleIdentity"/>s to pick from.</param>
		/// <returns>A new instance of <see cref="ModuleContainer"/> that contains the enabled Durian modules.</returns>
		public static ModuleContainer GetEnabledModules(params ModuleIdentity[]? modules)
		{
			return Assembly.GetCallingAssembly().GetEnabledModules(modules);
		}

		/// <summary>
		/// Returns a collection of all Durian modules present in the provided array of <paramref name="references"/> that are enabled for the calling <see cref="Assembly"/>.
		/// </summary>
		/// <param name="references">Array of <see cref="ModuleReference"/>s to pick from.</param>
		/// <returns>A new instance of <see cref="ModuleContainer"/> that contains the enabled Durian modules.</returns>
		public static ModuleContainer GetEnabledModules(params ModuleReference[]? references)
		{
			return Assembly.GetCallingAssembly().GetEnabledModules(references);
		}

		/// <summary>
		/// Returns a collection of all Durian modules present in the provided array of <paramref name="modules"/> that are enabled for the calling <see cref="Assembly"/>.
		/// </summary>
		/// <param name="modules">Array of <see cref="DurianModule"/>s to pick the enabled modules from.</param>
		/// <returns>A new instance of <see cref="ModuleContainer"/> that contains the enabled Durian modules.</returns>
		/// <exception cref="InvalidOperationException">Unknown <see cref="DurianModule"/> value detected. -or- <see cref="DurianModule.None"/> is not a valid Durian module.</exception>
		public static ModuleContainer GetEnabledModules(params DurianModule[]? modules)
		{
			return Assembly.GetCallingAssembly().GetEnabledModules(modules);
		}

		/// <summary>
		/// Returns a <see cref="ModuleIdentity"/> of Durian module with the specified <paramref name="moduleName"/>.
		/// </summary>
		/// <param name="moduleName">Name of the Durian module to get the <see cref="ModuleIdentity"/> of.</param>
		/// <exception cref="ArgumentException">
		/// <paramref name="moduleName"/> is <see langword="null"/> or empty. -or-
		/// Unknown Durian module name: <paramref name="moduleName"/>.
		/// </exception>
		public static ModuleIdentity GetModule(string moduleName)
		{
			DurianModule module = Parse(moduleName);
			return GetModule(module);
		}

		/// <summary>
		/// Returns a name of the specified <paramref name="module"/>.
		/// </summary>
		/// <param name="module"><see cref="DurianModule"/> to get the name of.</param>
		/// <exception cref="ArgumentException">Unknown <see cref="DurianModule"/> value: <paramref name="module"/>.</exception>
		public static string GetName(DurianModule module)
		{
			if (!TryGetName(module, out string? moduleName))
			{
				throw new ArgumentException($"Unknown {nameof(DurianModule)} value: {module}", nameof(module));
			}

			return moduleName;
		}

		/// <summary>
		/// Returns a new instance of <see cref="ModuleReference"/> that represents an in-direct reference to a <see cref="ModuleIdentity"/>.
		/// </summary>
		/// <param name="module"><see cref="DurianModule"/> to get a <see cref="ModuleReference"/> to.</param>
		/// <exception cref="ArgumentException">Unknown <see cref="DurianModule"/> value detected. -or- <see cref="DurianModule.None"/> is not a valid Durian module.</exception>
		public static ModuleReference GetReference(DurianModule module)
		{
			return new ModuleReference(module);
		}

		/// <summary>
		/// Returns a new instance of <see cref="ModuleReference"/> that represents an in-direct reference to a <see cref="ModuleIdentity"/>.
		/// </summary>
		/// <param name="moduleName">Name of the Durian module to get a <see cref="ModuleReference"/> to.</param>
		/// <exception cref="ArgumentException">
		/// <paramref name="moduleName"/> is <see langword="null"/> or empty. -or-
		/// Unknown Durian module name: <paramref name="moduleName"/>.
		/// </exception>
		public static ModuleReference GetReference(string moduleName)
		{
			DurianModule module = Parse(moduleName);
			return new ModuleReference(module);
		}

		/// <summary>
		/// Determines whether the <paramref name="module"/> is enabled for the calling <see cref="Assembly"/>.
		/// </summary>
		/// <param name="module"><see cref="ModuleReference"/> of Durian module to check for.</param>
		/// <exception cref="ArgumentNullException"><paramref name="module"/> is <see langword="null"/>.</exception>
		public static bool IsEnabled(ModuleReference module)
		{
			return Assembly.GetCallingAssembly().IsEnabled(module);
		}

		/// <summary>
		/// Determines whether the specified <paramref name="module"/> is enabled for the calling <see cref="Assembly"/>.
		/// </summary>
		/// <param name="module"><see cref="ModuleIdentity"/> representing a Durian module to check for.</param>
		/// <exception cref="ArgumentNullException"><paramref name="module"/> is <see langword="null"/>.</exception>
		public static bool IsEnabled(ModuleIdentity module)
		{
			return Assembly.GetCallingAssembly().IsEnabled(module);
		}

		/// <summary>
		/// Determines whether the <paramref name="module"/> is enabled for the calling <see cref="Assembly"/>.
		/// </summary>
		/// <param name="module"><see cref="DurianModule"/> representing a Durian module to check for.</param>
		/// <exception cref="InvalidOperationException">Unknown <see cref="DurianModule"/> value detected. -or- <see cref="DurianModule.None"/> is not a valid Durian module.</exception>
		public static bool IsEnabled(DurianModule module)
		{
			return Assembly.GetCallingAssembly().IsEnabled(module);
		}

		/// <summary>
		/// Determines whether the calling <see cref="Assembly"/> references a Durian module with the given <paramref name="moduleName"/>.
		/// </summary>
		/// <param name="moduleName">Name of the Durian module to check for.</param>
		/// <exception cref="ArgumentException">
		/// <paramref name="moduleName"/> is <see langword="null"/> or empty. -or-
		/// Unknown Durian module name: <paramref name="moduleName"/>.
		/// </exception>
		public static bool IsEnabled(string moduleName)
		{
			return Assembly.GetCallingAssembly().IsEnabled(moduleName);
		}

		/// <summary>
		/// Converts the specified <paramref name="moduleName"/> into a value of the <see cref="DurianModule"/> enum.
		/// </summary>
		/// <param name="moduleName"><see cref="string"/> to convert to a value of the <see cref="DurianModule"/> enum.</param>
		/// <exception cref="ArgumentException">
		/// <paramref name="moduleName"/> is <see langword="null"/> or empty. -or-
		/// Unknown Durian module name: <paramref name="moduleName"/>.
		/// </exception>
		public static DurianModule Parse(string moduleName)
		{
			if (!TryParse(moduleName, out DurianModule module))
			{
				if (string.IsNullOrWhiteSpace(moduleName))
				{
					throw new ArgumentException($"$'{nameof(moduleName)}' cannot be null or empty", nameof(moduleName));
				}

				throw new ArgumentException($"Unknown Durian module name: {moduleName}", nameof(moduleName));
			}

			return module;
		}

		/// <summary>
		/// Attempts to return a <see cref="ModuleIdentity"/> of Durian module with the specified <paramref name="moduleName"/>.
		/// </summary>
		/// <param name="moduleName">Name of the Durian module to get the <see cref="ModuleIdentity"/> of.</param>
		/// <param name="module"><see cref="ModuleIdentity"/> that was returned.</param>
		public static bool TryGetModule([NotNullWhen(true)] string? moduleName, [NotNullWhen(true)] out ModuleIdentity? module)
		{
			if (!TryParse(moduleName, out DurianModule m))
			{
				module = null;
				return false;
			}

			module = GetModule(m);
			return true;
		}

		/// <summary>
		/// Attempts to return a new instance of <see cref="ModuleReference"/> that represents an in-direct reference to a <see cref="ModuleIdentity"/>.
		/// </summary>
		/// <param name="moduleName">Name of the Durian module to get a <see cref="ModuleReference"/> to.</param>
		/// <param name="reference">Newly-created <see cref="ModuleReference"/>.</param>
		public static bool TryGetReference([NotNullWhen(true)] string? moduleName, [NotNullWhen(true)] out ModuleReference? reference)
		{
			if (!TryParse(moduleName, out DurianModule module))
			{
				reference = null;
				return false;
			}

			reference = new ModuleReference(module);
			return true;
		}

		internal static void EnsureIsValidModuleEnum_InvOp(DurianModule module)
		{
			if (module == DurianModule.None)
			{
				throw new InvalidOperationException($"{nameof(DurianModule)}.{nameof(DurianModule.None)} is not a valid Durian module");
			}

			if (!GlobalInfo.IsValidModuleValue(module))
			{
				throw new InvalidOperationException($"Unknown {nameof(DurianModule)} value: {module}!");
			}
		}
	}
}
