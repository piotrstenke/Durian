// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

namespace Durian.Info
{
	public partial class TypeIdentity
	{
		/// <summary>
		/// Deallocates all cached instances of <see cref="TypeIdentity"/>.
		/// </summary>
		public static void Deallocate()
		{
			IdentityPool.Types.Clear();
		}

		/// <summary>
		/// Returns a collection of <see cref="TypeIdentity"/>s representing all <see cref="Type"/>s declared in the <c>Durian.Core</c> package that are part of any of the provided Durian <paramref name="modules"/>.
		/// </summary>
		/// <param name="modules"><see cref="ModuleContainer"/> that provides a collection of Durian modules to pick the <see cref="TypeIdentity"/>s from.</param>
		/// <exception cref="ArgumentNullException"><paramref name="modules"/> is <see langword="null"/>.</exception>
		public static IEnumerable<TypeIdentity> GetAllTypes(ModuleContainer modules)
		{
			if (modules is null)
			{
				throw new ArgumentNullException(nameof(modules));
			}

			if (modules.Count == 0)
			{
				return Array.Empty<TypeIdentity>();
			}

			ModuleIdentity[] identities = modules.AsIdentities();

			return GetAllTypes(identities);
		}

		/// <summary>
		/// Returns a collection of <see cref="TypeIdentity"/>s representing all <see cref="Type"/>s declared in the <c>Durian.Core</c> package that are part of any of the provided Durian modules.
		/// </summary>
		/// <param name="references">Array of <see cref="ModuleReference"/>s to pick the <see cref="TypeIdentity"/>s from.</param>
		public static IEnumerable<TypeIdentity> GetAllTypes(params ModuleReference[]? references)
		{
			if (references is null || references.Length == 0)
			{
				return Array.Empty<TypeIdentity>();
			}

			ModuleIdentity[] modules = ModuleConverter.ToIdentities(references.Where(r => r is not null));

			return GetAllTypes(modules);
		}

		/// <summary>
		/// Returns a collection of <see cref="TypeIdentity"/>s representing all <see cref="Type"/>s declared in the <c>Durian.Core</c> package that are part of any existing Durian module.
		/// </summary>
		public static IEnumerable<TypeIdentity> GetAllTypes()
		{
			ModuleIdentity[] identities = ModuleIdentity.GetAllModules().AsIdentities();

			return GetAllTypes(identities);
		}

		/// <summary>
		/// Returns a collection of <see cref="TypeIdentity"/>s representing all <see cref="Type"/>s declared in the <c>Durian.Core</c> package that are part of any of the provided Durian <paramref name="modules"/>.
		/// </summary>
		/// <param name="modules">Array of <see cref="DurianModule"/>s to pick the <see cref="TypeIdentity"/>s from.</param>
		/// <exception cref="InvalidOperationException">Unknown <see cref="DurianModule"/> value detected. -or- <see cref="DurianModule.None"/> is not a valid Durian module.</exception>
		public static IEnumerable<TypeIdentity> GetAllTypes(params DurianModule[]? modules)
		{
			if (modules is null || modules.Length == 0)
			{
				return Array.Empty<TypeIdentity>();
			}

			ModuleIdentity[] identities = ModuleConverter.ToIdentities(modules);

			return GetAllTypes(identities);
		}

		/// <summary>
		/// Returns a collection of <see cref="TypeIdentity"/>s representing all <see cref="Type"/>s declared in the <c>Durian.Core</c> package that are part of any of the provided Durian <paramref name="modules"/>.
		/// </summary>
		/// <param name="modules">Array of <see cref="ModuleIdentity"/>s to pick the <see cref="TypeIdentity"/>s from.</param>
		public static IEnumerable<TypeIdentity> GetAllTypes(params ModuleIdentity[]? modules)
		{
			if (modules is null || modules.Length == 0)
			{
				return Array.Empty<TypeIdentity>();
			}

			return Yield();

			IEnumerable<TypeIdentity> Yield()
			{
				HashSet<TypeIdentity> types = new(EqualityComparer<TypeIdentity>.Default);

				foreach (ModuleIdentity module in modules)
				{
					if (module is null)
					{
						continue;
					}

					foreach (TypeIdentity type in module.Types)
					{
						if (types.Add(type))
						{
							yield return type;
						}
					}
				}
			}
		}

		/// <summary>
		/// Returns a collection of <see cref="TypeIdentity"/>s representing all disabled Durian <see cref="Type"/>s for the calling <see cref="Assembly"/>.
		/// </summary>
		public static IEnumerable<TypeIdentity> GetDisabledTypes()
		{
			return Assembly.GetCallingAssembly().GetDisabledTypes();
		}

		/// <summary>
		/// Returns a collection of <see cref="TypeIdentity"/>s representing all disabled Durian <see cref="Type"/>s for the calling <see cref="Assembly"/> that are part of any of the provided <paramref name="modules"/>.
		/// </summary>
		/// <param name="modules">Array of <see cref="DurianModule"/>s to pick the <see cref="TypeIdentity"/>s from.</param>
		/// <exception cref="InvalidOperationException"> Unknown <see cref="DurianModule"/> value detected. -or- <see cref="DurianModule.None"/> is not a valid Durian module.</exception>
		public static IEnumerable<TypeIdentity> GetDisabledTypes(params DurianModule[]? modules)
		{
			return Assembly.GetCallingAssembly().GetDisabledTypes(modules);
		}

		/// <summary>
		/// Returns a collection of <see cref="TypeIdentity"/>s representing all disabled Durian <see cref="Type"/>s for the calling <see cref="Assembly"/> that are part of any of the provided <paramref name="modules"/>.
		/// </summary>
		/// <param name="modules">Array of <see cref="ModuleIdentity"/>s to pick the <see cref="TypeIdentity"/>s from.</param>
		public static IEnumerable<TypeIdentity> GetDisabledTypes(params ModuleIdentity[]? modules)
		{
			return Assembly.GetCallingAssembly().GetDisabledTypes(modules);
		}

		/// <summary>
		/// Returns a collection of <see cref="TypeIdentity"/>s representing all disabled Durian <see cref="Type"/>s for the calling <see cref="Assembly"/> that are part of any of the provided modules.
		/// </summary>
		/// <param name="references">Array of <see cref="ModuleReference"/>s to pick the <see cref="TypeIdentity"/>s from.</param>
		public static IEnumerable<TypeIdentity> GetDisabledTypes(params ModuleReference[]? references)
		{
			return Assembly.GetCallingAssembly().GetDisabledTypes(references);
		}

		/// <summary>
		/// Returns a collection of <see cref="TypeIdentity"/>s picked from the given array of <paramref name="types"/> that are disabled for the specified <paramref name="types"/>.
		/// </summary>
		/// <param name="types">Array of <see cref="TypeIdentity"/>s to pick the disabled ones from.</param>
		public static IEnumerable<TypeIdentity> GetDisabledTypes(params TypeIdentity[]? types)
		{
			return Assembly.GetCallingAssembly().GetDisabledTypes(types);
		}

		/// <summary>
		/// Returns a collection of <see cref="TypeIdentity"/>s representing all disabled Durian <see cref="Type"/>s for the calling <see cref="Assembly"/> that are part of any of the provided <paramref name="modules"/>.
		/// </summary>
		/// <param name="modules"><see cref="ModuleContainer"/> that provides a collection of Durian modules to pick the <see cref="TypeIdentity"/>s from.</param>
		/// <exception cref="ArgumentNullException"><paramref name="modules"/> is <see langword="null"/>.</exception>
		public static IEnumerable<TypeIdentity> GetDisabledTypes(ModuleContainer modules)
		{
			return Assembly.GetCallingAssembly().GetDisabledTypes(modules);
		}

		/// <summary>
		/// Returns a collection of <see cref="TypeIdentity"/>s representing all enabled Durian <see cref="Type"/>s for the calling <see cref="Assembly"/>.
		/// </summary>
		public static IEnumerable<TypeIdentity> GetEnabledTypes()
		{
			return Assembly.GetCallingAssembly().GetEnabledTypes();
		}

		/// <summary>
		/// Returns a collection of <see cref="TypeIdentity"/>s picked from the given array of <paramref name="types"/> that are enabled for the specified <paramref name="types"/>.
		/// </summary>
		/// <param name="types">Array of <see cref="TypeIdentity"/>s to pick the enabled ones from.</param>
		public static IEnumerable<TypeIdentity> GetEnabledTypes(params TypeIdentity[]? types)
		{
			return Assembly.GetCallingAssembly().GetEnabledTypes(types);
		}

		/// <summary>
		/// Returns a collection of <see cref="TypeIdentity"/>s representing all enabled Durian <see cref="Type"/>s for the calling <see cref="Assembly"/> that are part of any of the provided <paramref name="modules"/>.
		/// </summary>
		/// <param name="modules"><see cref="ModuleContainer"/> that provides a collection of Durian modules to pick the <see cref="TypeIdentity"/>s from.</param>
		/// <exception cref="ArgumentNullException"><paramref name="modules"/> is <see langword="null"/>.</exception>
		public static IEnumerable<TypeIdentity> GetEnabledTypes(ModuleContainer modules)
		{
			return Assembly.GetCallingAssembly().GetEnabledTypes(modules);
		}

		/// <summary>
		/// Returns a collection of <see cref="TypeIdentity"/>s representing all enabled Durian <see cref="Type"/>s for the calling <see cref="Assembly"/> that are part of any of the provided modules.
		/// </summary>
		/// <param name="references">Array of <see cref="ModuleReference"/>s to pick the <see cref="TypeIdentity"/>s from.</param>
		public static IEnumerable<TypeIdentity> GetEnabledTypes(params ModuleReference[]? references)
		{
			return Assembly.GetCallingAssembly().GetEnabledTypes(references);
		}

		/// <summary>
		/// Returns a collection of <see cref="TypeIdentity"/>s representing all enabled Durian <see cref="Type"/>s for the calling <see cref="Assembly"/> that are part of any of the provided <paramref name="modules"/>.
		/// </summary>
		/// <param name="modules">Array of <see cref="DurianModule"/>s to pick the <see cref="TypeIdentity"/>s from.</param>
		/// <exception cref = "InvalidOperationException" > Unknown <see cref="DurianModule"/> value detected. -or- <see cref = "DurianModule.None" /> is not a valid Durian module.</exception>
		public static IEnumerable<TypeIdentity> GetEnabledTypes(params DurianModule[]? modules)
		{
			return Assembly.GetCallingAssembly().GetEnabledTypes(modules);
		}

		/// <summary>
		/// Returns a collection of <see cref="TypeIdentity"/>s representing all enabled Durian <see cref="Type"/>s for the calling <see cref="Assembly"/> that are part of any of the provided <paramref name="modules"/>.
		/// </summary>
		/// <param name="modules">Array of <see cref="ModuleIdentity"/>s to pick the <see cref="TypeIdentity"/>s from.</param>
		public static IEnumerable<TypeIdentity> GetEnabledTypes(params ModuleIdentity[]? modules)
		{
			return Assembly.GetCallingAssembly().GetEnabledTypes(modules);
		}

		/// <summary>
		/// Returns a collection of all <see cref="TypeIdentity"/>s that represent <see cref="Type"/>s in the specified <paramref name="namespace"/>.
		/// </summary>
		/// <param name="namespace">Namespace to get the <see cref="TypeIdentity"/>s from.</param>
		/// <exception cref="ArgumentNullException"><paramref name="namespace"/> is <see langword="null"/>.</exception>
		public static IEnumerable<TypeIdentity> GetIdentitiesInNamespace(string @namespace)
		{
			if (@namespace is null)
			{
				throw new ArgumentNullException(nameof(@namespace));
			}

			if (string.IsNullOrWhiteSpace(@namespace))
			{
				return Array.Empty<TypeIdentity>();
			}

			return Yield();

			IEnumerable<TypeIdentity> Yield()
			{
				foreach (TypeIdentity type in GetAllTypes())
				{
					if (type.Namespace == @namespace)
					{
						yield return type;
					}
				}
			}
		}

		/// <summary>
		/// Returns a <see cref="TypeIdentity"/> with the specified <paramref name="name"/>.
		/// </summary>
		/// <param name="name">Name of the <see cref="TypeIdentity"/> to return.</param>
		/// <exception cref="ArgumentNullException"><paramref name="name"/> is <see langword="null"/>.</exception>
		/// <exception cref="InvalidOperationException"><paramref name="name"/> is not a valid <see cref="TypeIdentity"/> name.</exception>
		public static TypeIdentity GetIdentity(string name)
		{
			if (!TryGetIdentity(name, out TypeIdentity? identity))
			{
				throw new InvalidOperationException($"Name '{name}' is not a valid {nameof(TypeIdentity)} name!");
			}

			return identity;
		}

		/// <summary>
		/// Determines whether the specified <paramref name="type"/> is enabled for the calling <see cref="Assembly"/>.
		/// </summary>
		/// <param name="type"><see cref="TypeIdentity"/> to check if is enabled.</param>
		/// <exception cref="ArgumentNullException"><paramref name="type"/> is <see langword="null"/>.</exception>
		public static bool IsEnabled(TypeIdentity type)
		{
			return Assembly.GetCallingAssembly().IsEnabled(type);
		}

		/// <summary>
		/// Determines whether the specified <paramref name="type"/> is enabled for the calling <see cref="Assembly"/>.
		/// </summary>
		/// <param name="type"><see cref="TypeIdentity"/> to check if is enabled.</param>
		/// <param name="references">Array of <see cref="ModuleReference"/>s to pick the <see cref="TypeIdentity"/>s from.</param>
		/// <exception cref="ArgumentNullException"><paramref name="type"/> is <see langword="null"/>.</exception>
		public static bool IsEnabled(TypeIdentity type, [NotNullWhen(true)] params ModuleReference[]? references)
		{
			return Assembly.GetCallingAssembly().IsEnabled(type, references);
		}

		/// <summary>
		/// Determines whether the specified <paramref name="type"/> is enabled for the calling <see cref="Assembly"/>.
		/// </summary>
		/// <param name="type"><see cref="TypeIdentity"/> to check if is enabled.</param>
		/// <param name="modules"><see cref="ModuleContainer"/> that provides a collection of Durian modules to pick the <see cref="TypeIdentity"/>s from.</param>
		/// <exception cref="ArgumentNullException"><paramref name="type"/> is <see langword="null"/>. -or- <paramref name="modules"/> is <see langword="null"/>.</exception>
		public static bool IsEnabled(TypeIdentity type, ModuleContainer modules)
		{
			return Assembly.GetCallingAssembly().IsEnabled(type, modules);
		}

		/// <summary>
		/// Determines whether the specified <paramref name="type"/> is enabled for the calling <see cref="Assembly"/>.
		/// </summary>
		/// <param name="type"><see cref="TypeIdentity"/> to check if is enabled.</param>
		/// <param name="modules">Array of <see cref="ModuleIdentity"/>s to pick the <see cref="TypeIdentity"/>s from.</param>
		/// <exception cref="ArgumentNullException"><paramref name="type"/> is <see langword="null"/>.</exception>
		public static bool IsEnabled(TypeIdentity type, [NotNullWhen(true)] params ModuleIdentity[]? modules)
		{
			return Assembly.GetCallingAssembly().IsEnabled(type, modules);
		}

		/// <summary>
		/// Determines whether the specified <paramref name="type"/> is enabled for the calling <see cref="Assembly"/>.
		/// </summary>
		/// <param name="type"><see cref="TypeIdentity"/> to check if is enabled.</param>
		/// <param name="modules">Array of <see cref="DurianModule"/>s to pick the <see cref="TypeIdentity"/>s from.</param>
		/// <exception cref="ArgumentNullException"><paramref name="type"/> is <see langword="null"/>.</exception>
		/// <exception cref="InvalidOperationException">Unknown <see cref="DurianModule"/> value detected. -or- <see cref="DurianModule.None"/> is not a valid Durian module.</exception>
		public static bool IsEnabled(TypeIdentity type, [NotNullWhen(true)] params DurianModule[]? modules)
		{
			return Assembly.GetCallingAssembly().IsEnabled(type, modules);
		}

		/// <summary>
		/// Attempts to return a <see cref="TypeIdentity"/> with the specified <paramref name="name"/>.
		/// </summary>
		/// <param name="name">Name of the <see cref="TypeIdentity"/> to return.</param>
		/// <param name="identity"><see cref="TypeIdentity"/> that was found.</param>
		/// <returns><see langword="true"/> if a <see cref="TypeIdentity"/> with the specified <paramref name="name"/> was found, <see langword="false"/> otherwise.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="name"/> is <see langword="null"/>.</exception>
		public static bool TryGetIdentity(string name, [NotNullWhen(true)] out TypeIdentity? identity)
		{
			if (name is null)
			{
				throw new ArgumentNullException(nameof(name));
			}

			PropertyInfo? property = typeof(TypeRepository).GetProperty(name, BindingFlags.Static | BindingFlags.Public);

			if (property is null)
			{
				identity = null;
				return false;
			}

			identity = (TypeIdentity)property.GetValue(null);
			return true;
		}
	}
}
