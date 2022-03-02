// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Durian.Generator;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

namespace Durian.Info
{
    public static partial class AssemblyExtensions
    {
        /// <summary>
        /// Returns a collection of <see cref="TypeIdentity"/>s representing all disabled Durian <see cref="Type"/>s for the specified <paramref name="assembly"/>.
        /// </summary>
        /// <param name="assembly"><see cref="Assembly"/> to get the disabled Durian <see cref="Type"/>s of.</param>
        /// <exception cref="ArgumentNullException"><paramref name="assembly"/> is <see langword="null"/>.</exception>
        public static IEnumerable<TypeIdentity> GetDisabledTypes(this Assembly assembly)
        {
            if (assembly is null)
            {
                throw new ArgumentNullException(nameof(assembly));
            }

            ModuleReference[] modules = assembly.GetEnabledModules().AsReferences();

            return assembly.GetDisabledTypes(modules);
        }

        /// <summary>
        /// Returns a collection of <see cref="TypeIdentity"/>s representing all disabled Durian <see cref="Type"/>s for the specified <paramref name="assembly"/> that are part of any of the provided <paramref name="modules"/>.
        /// </summary>
        /// <param name="assembly"><see cref="Assembly"/> to get the disabled Durian <see cref="Type"/>s of.</param>
        /// <param name="modules"><see cref="ModuleContainer"/> that provides a collection of Durian modules to pick the <see cref="TypeIdentity"/>s from.</param>
        /// <exception cref="ArgumentNullException"><paramref name="assembly"/> is <see langword="null"/>. -or- <paramref name="modules"/> is <see langword="null"/>.</exception>
        public static IEnumerable<TypeIdentity> GetDisabledTypes(this Assembly assembly, ModuleContainer modules)
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
                return Array.Empty<TypeIdentity>();
            }

            ModuleReference[] references = modules.AsReferences();

            return assembly.GetDisabledTypes(references);
        }

        /// <summary>
        /// Returns a collection of <see cref="TypeIdentity"/>s representing all disabled Durian <see cref="Type"/>s for the specified <paramref name="assembly"/> that are part of any of the provided <paramref name="modules"/>.
        /// </summary>
        /// <param name="assembly"><see cref="Assembly"/> to get the disabled Durian <see cref="Type"/>s of.</param>
        /// <param name="modules">Array of <see cref="ModuleIdentity"/>s to pick the <see cref="TypeIdentity"/>s from.</param>
        /// <exception cref="ArgumentNullException"><paramref name="assembly"/> is <see langword="null"/>.</exception>
        public static IEnumerable<TypeIdentity> GetDisabledTypes(this Assembly assembly, params ModuleIdentity[]? modules)
        {
            if (assembly is null)
            {
                throw new ArgumentNullException(nameof(assembly));
            }

            if (modules is null || modules.Length == 0)
            {
                return Array.Empty<TypeIdentity>();
            }

            EnableModuleAttribute[] attributes = assembly.GetCustomAttributes<EnableModuleAttribute>().ToArray();

            if (attributes.Length == 0)
            {
                return Array.Empty<TypeIdentity>();
            }

            return Yield();

            IEnumerable<TypeIdentity> Yield()
            {
                foreach (ModuleIdentity module in modules)
                {
                    if (module is null)
                    {
                        continue;
                    }

                    if (!IsEnabled_Internal(attributes, module.Module))
                    {
                        foreach (TypeIdentity t in module.Types)
                        {
                            yield return t;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Returns a collection of <see cref="TypeIdentity"/>s representing all disabled Durian <see cref="Type"/>s for the specified <paramref name="assembly"/> that are part of any of the provided <paramref name="modules"/>.
        /// </summary>
        /// <param name="assembly"><see cref="Assembly"/> to get the disabled Durian <see cref="Type"/>s of.</param>
        /// <param name="modules">Array of <see cref="DurianModule"/>s to pick the <see cref="TypeIdentity"/>s from.</param>
        /// <exception cref="ArgumentNullException"><paramref name="assembly"/> is <see langword="null"/>.</exception>
        /// <exception cref="InvalidOperationException"> Unknown <see cref="DurianModule"/> value detected. -or- <see cref="DurianModule.None"/> is not a valid Durian module.</exception>
        public static IEnumerable<TypeIdentity> GetDisabledTypes(this Assembly assembly, params DurianModule[]? modules)
        {
            if (assembly is null)
            {
                throw new ArgumentNullException(nameof(assembly));
            }

            if (modules is null || modules.Length == 0)
            {
                return Array.Empty<TypeIdentity>();
            }

            foreach (DurianModule module in modules)
            {
                ModuleIdentity.EnsureIsValidModuleEnum(module);
            }

            EnableModuleAttribute[] attributes = assembly.GetCustomAttributes<EnableModuleAttribute>().ToArray();

            if (attributes.Length == 0)
            {
                return Array.Empty<TypeIdentity>();
            }

            return Yield();

            IEnumerable<TypeIdentity> Yield()
            {
                foreach (DurianModule module in modules)
                {
                    if (!IsEnabled_Internal(attributes, module))
                    {
                        ModuleIdentity identity = ModuleIdentity.GetModule(module);

                        foreach (TypeIdentity t in identity.Types)
                        {
                            yield return t;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Returns a collection of <see cref="TypeIdentity"/>s representing all disabled Durian <see cref="Type"/>s for the specified <paramref name="assembly"/> that are part of any of the provided modules.
        /// </summary>
        /// <param name="assembly"><see cref="Assembly"/> to get the disabled Durian <see cref="Type"/>s of.</param>
        /// <param name="references">Array of <see cref="ModuleReference"/>s to pick the <see cref="TypeIdentity"/>s from.</param>
        /// <exception cref="ArgumentNullException"><paramref name="assembly"/> is <see langword="null"/>.</exception>
        public static IEnumerable<TypeIdentity> GetDisabledTypes(this Assembly assembly, params ModuleReference[]? references)
        {
            if (assembly is null)
            {
                throw new ArgumentNullException(nameof(assembly));
            }

            if (references is null || references.Length == 0)
            {
                return Array.Empty<TypeIdentity>();
            }

            EnableModuleAttribute[] attributes = assembly.GetCustomAttributes<EnableModuleAttribute>().ToArray();

            if (attributes.Length == 0)
            {
                return Array.Empty<TypeIdentity>();
            }

            return Yield();

            IEnumerable<TypeIdentity> Yield()
            {
                foreach (ModuleReference reference in references)
                {
                    if (reference is null)
                    {
                        continue;
                    }

                    if (!IsEnabled_Internal(attributes, reference.EnumValue))
                    {
                        ModuleIdentity module = reference.GetModule();

                        foreach (TypeIdentity t in module.Types)
                        {
                            yield return t;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Returns a collection of <see cref="TypeIdentity"/>s picked from the given array of <paramref name="types"/> that are disabled for the specified <paramref name="types"/>.
        /// </summary>
        /// <param name="assembly"><see cref="Assembly"/> to get the enabled Durian <see cref="Type"/>s of.</param>
        /// <param name="types">Array of <see cref="TypeIdentity"/>s to pick the disabled ones from.</param>
        /// <exception cref="ArgumentNullException"><paramref name="assembly"/> is <see langword="null"/>.</exception>
        public static IEnumerable<TypeIdentity> GetDisabledTypes(this Assembly assembly, params TypeIdentity[]? types)
        {
            if (assembly is null)
            {
                throw new ArgumentNullException(nameof(assembly));
            }

            if (types is null)
            {
                return Array.Empty<TypeIdentity>();
            }

            EnableModuleAttribute[] attributes = assembly.GetCustomAttributes<EnableModuleAttribute>().ToArray();

            if (attributes.Length == 0)
            {
                return Array.Empty<TypeIdentity>();
            }

            Dictionary<DurianModule, bool> modules = new();
            List<TypeIdentity> list = new(types.Length);

            foreach (TypeIdentity type in types)
            {
                if (type is null)
                {
                    continue;
                }

                foreach (ModuleReference reference in type.Modules)
                {
                    if (modules.TryGetValue(reference.EnumValue, out bool value))
                    {
                        if (!value)
                        {
                            list.Add(type);
                        }
                    }
                    else
                    {
                        value = IsEnabled_Internal(attributes, reference.EnumValue);

                        modules.Add(reference.EnumValue, value);
                    }
                }
            }

            return list;
        }

        /// <summary>
        /// Returns a collection of <see cref="TypeIdentity"/>s representing all enabled Durian <see cref="Type"/>s for the specified <paramref name="assembly"/>.
        /// </summary>
        /// <param name="assembly"><see cref="Assembly"/> to get the enabled Durian <see cref="Type"/>s of.</param>
        /// <exception cref="ArgumentNullException"><paramref name="assembly"/> is <see langword="null"/>.</exception>
        public static IEnumerable<TypeIdentity> GetEnabledTypes(this Assembly assembly)
        {
            if (assembly is null)
            {
                throw new ArgumentNullException(nameof(assembly));
            }

            ModuleReference[] modules = assembly.GetEnabledModules().AsReferences();

            return assembly.GetEnabledTypes(modules);
        }

        /// <summary>
        /// Returns a collection of <see cref="TypeIdentity"/>s representing all enabled Durian <see cref="Type"/>s for the specified <paramref name="assembly"/> that are part of any of the provided <paramref name="modules"/>.
        /// </summary>
        /// <param name="assembly"><see cref="Assembly"/> to get the enabled Durian <see cref="Type"/>s of.</param>
        /// <param name="modules"><see cref="ModuleContainer"/> that provides a collection of Durian modules to pick the <see cref="TypeIdentity"/>s from.</param>
        /// <exception cref="ArgumentNullException"><paramref name="assembly"/> is <see langword="null"/>. -or- <paramref name="modules"/> is <see langword="null"/>.</exception>
        public static IEnumerable<TypeIdentity> GetEnabledTypes(this Assembly assembly, ModuleContainer modules)
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
                return Array.Empty<TypeIdentity>();
            }

            ModuleReference[] references = modules.AsReferences();

            return assembly.GetEnabledTypes(references);
        }

        /// <summary>
        /// Returns a collection of <see cref="TypeIdentity"/>s representing all enabled Durian <see cref="Type"/>s for the specified <paramref name="assembly"/> that are part of any of the provided <paramref name="modules"/>.
        /// </summary>
        /// <param name="assembly"><see cref="Assembly"/> to get the enabled Durian <see cref="Type"/>s of.</param>
        /// <param name="modules">Array of <see cref="ModuleIdentity"/>s to pick the <see cref="TypeIdentity"/>s from.</param>
        /// <exception cref="ArgumentNullException"><paramref name="assembly"/> is <see langword="null"/>.</exception>
        public static IEnumerable<TypeIdentity> GetEnabledTypes(this Assembly assembly, params ModuleIdentity[]? modules)
        {
            if (assembly is null)
            {
                throw new ArgumentNullException(nameof(assembly));
            }

            if (modules is null || modules.Length == 0)
            {
                return Array.Empty<TypeIdentity>();
            }

            EnableModuleAttribute[] attributes = assembly.GetCustomAttributes<EnableModuleAttribute>().ToArray();

            if (attributes.Length == 0)
            {
                return Array.Empty<TypeIdentity>();
            }

            return Yield();

            IEnumerable<TypeIdentity> Yield()
            {
                foreach (ModuleIdentity module in modules)
                {
                    if (module is null)
                    {
                        continue;
                    }

                    if (IsEnabled_Internal(attributes, module.Module))
                    {
                        foreach (TypeIdentity t in module.Types)
                        {
                            yield return t;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Returns a collection of <see cref="TypeIdentity"/>s representing all enabled Durian <see cref="Type"/>s for the specified <paramref name="assembly"/> that are part of any of the provided <paramref name="modules"/>.
        /// </summary>
        /// <param name="assembly"><see cref="Assembly"/> to get the enabled Durian <see cref="Type"/>s of.</param>
        /// <param name="modules">Array of <see cref="DurianModule"/>s to pick the <see cref="TypeIdentity"/>s from.</param>
        /// <exception cref="ArgumentNullException"><paramref name="assembly"/> is <see langword="null"/>.</exception>
        /// <exception cref = "InvalidOperationException" > Unknown <see cref="DurianModule"/> value detected. -or- <see cref = "DurianModule.None" /> is not a valid Durian module.</exception>
        public static IEnumerable<TypeIdentity> GetEnabledTypes(this Assembly assembly, params DurianModule[]? modules)
        {
            if (assembly is null)
            {
                throw new ArgumentNullException(nameof(assembly));
            }

            if (modules is null || modules.Length == 0)
            {
                return Array.Empty<TypeIdentity>();
            }

            foreach (DurianModule module in modules)
            {
                ModuleIdentity.EnsureIsValidModuleEnum(module);
            }

            EnableModuleAttribute[] attributes = assembly.GetCustomAttributes<EnableModuleAttribute>().ToArray();

            if (attributes.Length == 0)
            {
                return Array.Empty<TypeIdentity>();
            }

            return Yield();

            IEnumerable<TypeIdentity> Yield()
            {
                foreach (DurianModule module in modules)
                {
                    if (IsEnabled_Internal(attributes, module))
                    {
                        ModuleIdentity identity = ModuleIdentity.GetModule(module);

                        foreach (TypeIdentity t in identity.Types)
                        {
                            yield return t;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Returns a collection of <see cref="TypeIdentity"/>s representing all enabled Durian <see cref="Type"/>s for the specified <paramref name="assembly"/> that are part of any of the provided modules.
        /// </summary>
        /// <param name="assembly"><see cref="Assembly"/> to get the enabled Durian <see cref="Type"/>s of.</param>
        /// <param name="references">Array of <see cref="ModuleReference"/>s to pick the <see cref="TypeIdentity"/>s from.</param>
        /// <exception cref="ArgumentNullException"><paramref name="assembly"/> is <see langword="null"/>.</exception>
        public static IEnumerable<TypeIdentity> GetEnabledTypes(this Assembly assembly, params ModuleReference[]? references)
        {
            if (assembly is null)
            {
                throw new ArgumentNullException(nameof(assembly));
            }

            if (references is null || references.Length == 0)
            {
                return Array.Empty<TypeIdentity>();
            }

            EnableModuleAttribute[] attributes = assembly.GetCustomAttributes<EnableModuleAttribute>().ToArray();

            if (attributes.Length == 0)
            {
                return Array.Empty<TypeIdentity>();
            }

            return Yield();

            IEnumerable<TypeIdentity> Yield()
            {
                foreach (ModuleReference reference in references)
                {
                    if (reference is null)
                    {
                        continue;
                    }

                    if (IsEnabled_Internal(attributes, reference.EnumValue))
                    {
                        ModuleIdentity module = reference.GetModule();

                        foreach (TypeIdentity t in module.Types)
                        {
                            yield return t;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Returns a collection of <see cref="TypeIdentity"/>s picked from the given array of <paramref name="types"/> that are enabled for the specified <paramref name="types"/>.
        /// </summary>
        /// <param name="assembly"><see cref="Assembly"/> to get the enabled Durian <see cref="Type"/>s of.</param>
        /// <param name="types">Array of <see cref="TypeIdentity"/>s to pick the enabled ones from.</param>
        /// <exception cref="ArgumentNullException"><paramref name="assembly"/> is <see langword="null"/>.</exception>
        public static IEnumerable<TypeIdentity> GetEnabledTypes(this Assembly assembly, params TypeIdentity[]? types)
        {
            if (assembly is null)
            {
                throw new ArgumentNullException(nameof(assembly));
            }

            if (types is null || types.Length == 0)
            {
                return Array.Empty<TypeIdentity>();
            }

            EnableModuleAttribute[] attributes = assembly.GetCustomAttributes<EnableModuleAttribute>().ToArray();

            if (attributes.Length == 0)
            {
                return Array.Empty<TypeIdentity>();
            }

            Dictionary<DurianModule, bool> modules = new();
            List<TypeIdentity> list = new(types.Length);

            foreach (TypeIdentity type in types)
            {
                if (type is null)
                {
                    continue;
                }

                foreach (ModuleReference reference in type.Modules)
                {
                    if (modules.TryGetValue(reference.EnumValue, out bool value))
                    {
                        if (value)
                        {
                            list.Add(type);
                        }
                    }
                    else
                    {
                        value = IsEnabled_Internal(attributes, reference.EnumValue);

                        modules.Add(reference.EnumValue, value);
                    }
                }
            }

            return list;
        }

        /// <summary>
        /// Determines whether the specified <paramref name="type"/> is enabled for the given <paramref name="assembly"/>.
        /// </summary>
        /// <param name="assembly"><see cref="Assembly"/> to check if the <paramref name="type"/> is enabled for.</param>
        /// <param name="type"><see cref="TypeIdentity"/> to check if is enabled.</param>
        /// <exception cref="ArgumentNullException"><paramref name="assembly"/> is <see langword="null"/>. -or- <paramref name="type"/> is <see langword="null"/>.</exception>
        public static bool IsEnabled(this Assembly assembly, TypeIdentity type)
        {
            if (assembly is null)
            {
                throw new ArgumentNullException(nameof(assembly));
            }

            if (type is null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            DurianModule[] modules = ModuleIdentity.GetAllModules().AsEnums();

            return IsEnabled_Internal(assembly, type, modules);
        }

        /// <summary>
        /// Determines whether the specified <paramref name="type"/> is enabled for the given <paramref name="assembly"/>.
        /// </summary>
        /// <param name="assembly"><see cref="Assembly"/> to check if the <paramref name="type"/> is enabled for.</param>
        /// <param name="type"><see cref="TypeIdentity"/> to check if is enabled.</param>
        /// <param name="modules"><see cref="ModuleContainer"/> that provides a collection of Durian modules to pick the <see cref="TypeIdentity"/>s from.</param>
        /// <exception cref="ArgumentNullException"><paramref name="assembly"/> is <see langword="null"/>. -or- <paramref name="type"/> is <see langword="null"/>. -or- <paramref name="modules"/> is <see langword="null"/>.</exception>
        public static bool IsEnabled(this Assembly assembly, TypeIdentity type, ModuleContainer modules)
        {
            if (assembly is null)
            {
                throw new ArgumentNullException(nameof(assembly));
            }

            if (type is null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (modules is null)
            {
                throw new ArgumentNullException(nameof(modules));
            }

            if (modules.IsEmpty)
            {
                return false;
            }

            DurianModule[] array = modules.AsEnums();

            return IsEnabled_Internal(assembly, type, array);
        }

        /// <summary>
        /// Determines whether the specified <paramref name="type"/> is enabled for the given <paramref name="assembly"/>.
        /// </summary>
        /// <param name="assembly"><see cref="Assembly"/> to check if the <paramref name="type"/> is enabled for.</param>
        /// <param name="type"><see cref="TypeIdentity"/> to check if is enabled.</param>
        /// <param name="modules">Array of <see cref="ModuleIdentity"/>s to pick the <see cref="TypeIdentity"/>s from.</param>
        /// <exception cref="ArgumentNullException"><paramref name="assembly"/> is <see langword="null"/>. -or- <paramref name="type"/> is <see langword="null"/>.</exception>
        public static bool IsEnabled(this Assembly assembly, TypeIdentity type, [NotNullWhen(true)] params ModuleIdentity[]? modules)
        {
            if (assembly is null)
            {
                throw new ArgumentNullException(nameof(assembly));
            }

            if (type is null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (modules is null || modules.Length == 0)
            {
                return false;
            }

            DurianModule[] array = ModuleConverter.ToEnums(modules.Where(m => m is not null));

            return IsEnabled_Internal(assembly, type, array);
        }

        /// <summary>
        /// Determines whether the specified <paramref name="type"/> is enabled for the given <paramref name="assembly"/>.
        /// </summary>
        /// <param name="assembly"><see cref="Assembly"/> to check if the <paramref name="type"/> is enabled for.</param>
        /// <param name="type"><see cref="TypeIdentity"/> to check if is enabled.</param>
        /// <param name="references">Array of <see cref="ModuleReference"/>s to pick the <see cref="TypeIdentity"/>s from.</param>
        /// <exception cref="ArgumentNullException"><paramref name="assembly"/> is <see langword="null"/>. -or- <paramref name="type"/> is <see langword="null"/>.</exception>
        public static bool IsEnabled(this Assembly assembly, TypeIdentity type, [NotNullWhen(true)] params ModuleReference[]? references)
        {
            if (assembly is null)
            {
                throw new ArgumentNullException(nameof(assembly));
            }

            if (type is null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (references is null || references.Length == 0)
            {
                return false;
            }

            DurianModule[] modules = ModuleConverter.ToEnums(references.Where(r => r is not null));

            return IsEnabled_Internal(assembly, type, modules);
        }

        /// <summary>
        /// Determines whether the specified <paramref name="type"/> is enabled for the given <paramref name="assembly"/>.
        /// </summary>
        /// <param name="assembly"><see cref="Assembly"/> to check if the <paramref name="type"/> is enabled for.</param>
        /// <param name="type"><see cref="TypeIdentity"/> to check if is enabled.</param>
        /// <param name="modules">Array of <see cref="DurianModule"/>s to pick the <see cref="TypeIdentity"/>s from.</param>
        /// <exception cref="ArgumentNullException"><paramref name="assembly"/> is <see langword="null"/>. -or- <paramref name="type"/> is <see langword="null"/>.</exception>
        /// <exception cref="InvalidOperationException">Unknown <see cref="DurianModule"/> value detected. -or- <see cref="DurianModule.None"/> is not a valid Durian module.</exception>
        public static bool IsEnabled(this Assembly assembly, TypeIdentity type, [NotNullWhen(true)] params DurianModule[]? modules)
        {
            if (assembly is null)
            {
                throw new ArgumentNullException(nameof(assembly));
            }

            if (type is null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (modules is null || modules.Length == 0)
            {
                return false;
            }

            foreach (DurianModule module in modules)
            {
                ModuleIdentity.EnsureIsValidModuleEnum(module);
            }

            return IsEnabled_Internal(assembly, type, modules);
        }

        private static bool IsEnabled_Internal(Assembly assembly, TypeIdentity type, DurianModule[] modules)
        {
            EnableModuleAttribute[] attributes = assembly.GetCustomAttributes<EnableModuleAttribute>().ToArray();

            if (attributes.Length == 0)
            {
                return false;
            }

            foreach (ModuleReference m in type.Modules)
            {
                if (!IsEnabled_Internal(attributes, m.EnumValue))
                {
                    continue;
                }

                foreach (DurianModule e in modules)
                {
                    if (m.EnumValue == e)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}