// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Durian.Generator;
using Durian.Info;
using Microsoft.CodeAnalysis;

namespace Durian.Analysis.Extensions
{
	/// <summary>
	/// Contains various <see cref="ModuleIdentity"/>-related extension methods for the <see cref="Compilation"/> class and <see cref="INamedTypeSymbol"/> interface.
	/// </summary>
	public static class ModuleUtilities
	{
		/// <summary>
		/// Returns a collection of <see cref="INamedTypeSymbol"/>s representing all disabled Durian <see cref="Type"/>s for the specified <paramref name="compilation"/>.
		/// </summary>
		/// <param name="compilation"><see cref="Compilation"/> to get the disabled Durian <see cref="Type"/>s of.</param>
		/// <exception cref="ArgumentNullException"><paramref name="compilation"/> is <see langword="null"/>.</exception>
		/// <exception cref="InvalidOperationException">Error while resolving the <see cref="EnableModuleAttribute"/> class.</exception>
		public static IEnumerable<INamedTypeSymbol> GetDisabledDurianTypes(this Compilation compilation)
		{
			if (compilation is null)
			{
				throw new ArgumentNullException(nameof(compilation));
			}

			INamedTypeSymbol symbol = GetEnableAttributeSymbol(compilation);
			return compilation.GetDisabledDurianTypes(symbol);
		}

		/// <summary>
		/// Returns a collection of <see cref="INamedTypeSymbol"/>s representing all disabled Durian <see cref="Type"/>s for the specified <paramref name="compilation"/>.
		/// </summary>
		/// <param name="compilation"><see cref="Compilation"/> to get the disabled Durian <see cref="Type"/>s of.</param>
		/// <param name="enableModuleAttribute"><see cref="INamedTypeSymbol"/> that represents the <see cref="EnableModuleAttribute"/> class.</param>
		/// <exception cref="ArgumentNullException"><paramref name="compilation"/> is <see langword="null"/>. -or- <paramref name="enableModuleAttribute"/> is <see langword="null"/>.</exception>
		public static IEnumerable<INamedTypeSymbol> GetDisabledDurianTypes(this Compilation compilation, INamedTypeSymbol enableModuleAttribute)
		{
			if (compilation is null)
			{
				throw new ArgumentNullException(nameof(compilation));
			}

			if (enableModuleAttribute is null)
			{
				throw new ArgumentNullException(nameof(compilation));
			}

			ModuleReference[] modules = compilation.GetDisabledModules(enableModuleAttribute).AsReferences();

			return compilation.GetDisabledDurianTypes(enableModuleAttribute, modules);
		}

		/// <summary>
		/// Returns a collection of <see cref="INamedTypeSymbol"/>s representing all disabled Durian <see cref="Type"/>s for the specified <paramref name="compilation"/> that are part of any of the provided <paramref name="modules"/>.
		/// </summary>
		/// <param name="compilation"><see cref="Compilation"/> to get the disabled Durian <see cref="Type"/>s of.</param>
		/// <param name="modules"><see cref="ModuleContainer"/> that provides a collection of Durian modules to pick the <see cref="INamedTypeSymbol"/>s from.</param>
		/// <exception cref="ArgumentNullException"><paramref name="compilation"/> is <see langword="null"/>. -or- <paramref name="modules"/> is <see langword="null"/>.</exception>
		/// <exception cref="InvalidOperationException">Error while resolving the <see cref="EnableModuleAttribute"/> class.</exception>
		public static IEnumerable<INamedTypeSymbol> GetDisabledDurianTypes(this Compilation compilation, ModuleContainer modules)
		{
			if (compilation is null)
			{
				throw new ArgumentNullException(nameof(compilation));
			}

			if (modules is null)
			{
				throw new ArgumentNullException(nameof(modules));
			}

			if (modules.Count == 0)
			{
				return Array.Empty<INamedTypeSymbol>();
			}

			ModuleReference[] references = modules.AsReferences();

			return compilation.GetDisabledDurianTypes(references);
		}

		/// <summary>
		/// Returns a collection of <see cref="INamedTypeSymbol"/>s representing all disabled Durian <see cref="Type"/>s for the specified <paramref name="compilation"/> that are part of any of the provided <paramref name="modules"/>.
		/// </summary>
		/// <param name="compilation"><see cref="Compilation"/> to get the disabled Durian <see cref="Type"/>s of.</param>
		/// <param name="enableModuleAttribute"><see cref="INamedTypeSymbol"/> that represents the <see cref="EnableModuleAttribute"/> class.</param>
		/// <param name="modules"><see cref="ModuleContainer"/> that provides a collection of Durian modules to pick the <see cref="INamedTypeSymbol"/>s from.</param>
		/// <exception cref="ArgumentNullException"><paramref name="compilation"/> is <see langword="null"/>. -or- <paramref name="enableModuleAttribute"/> is <see langword="null"/>. -or- <paramref name="modules"/> is <see langword="null"/>.</exception>
		public static IEnumerable<INamedTypeSymbol> GetDisabledDurianTypes(this Compilation compilation, INamedTypeSymbol enableModuleAttribute, ModuleContainer modules)
		{
			if (compilation is null)
			{
				throw new ArgumentNullException(nameof(compilation));
			}

			if (enableModuleAttribute is null)
			{
				throw new ArgumentNullException(nameof(enableModuleAttribute));
			}

			if (modules is null)
			{
				throw new ArgumentNullException(nameof(modules));
			}

			if (modules.Count == 0)
			{
				return Array.Empty<INamedTypeSymbol>();
			}

			ModuleReference[] references = modules.AsReferences();

			return compilation.GetDisabledDurianTypes(enableModuleAttribute, references);
		}

		/// <summary>
		/// Returns a collection of <see cref="INamedTypeSymbol"/>s representing all disabled Durian <see cref="Type"/>s for the specified <paramref name="compilation"/> picked from the provided array of <paramref name="types"/>.
		/// </summary>
		/// <param name="compilation"><see cref="Compilation"/> to get the disabled Durian <see cref="Type"/>s of.</param>
		/// <param name="types">Array of <see cref="TypeIdentity"/>s to pick the disabled ones from.</param>
		/// <exception cref="ArgumentNullException"><paramref name="compilation"/> is <see langword="null"/>.</exception>
		/// <exception cref="InvalidOperationException">Error while resolving the <see cref="EnableModuleAttribute"/> class.</exception>
		public static IEnumerable<INamedTypeSymbol> GetDisabledDurianTypes(this Compilation compilation, params TypeIdentity[]? types)
		{
			if (compilation is null)
			{
				throw new ArgumentNullException(nameof(compilation));
			}

			if (types is null)
			{
				return Array.Empty<INamedTypeSymbol>();
			}

			INamedTypeSymbol symbol = GetEnableAttributeSymbol(compilation);
			return compilation.GetDisabledDurianTypes(symbol, types);
		}

		/// <summary>
		/// Returns a collection of <see cref="INamedTypeSymbol"/>s representing all disabled Durian <see cref="Type"/>s for the specified <paramref name="compilation"/> picked from the provided array of <paramref name="types"/>.
		/// </summary>
		/// <param name="compilation"><see cref="Compilation"/> to get the disabled Durian <see cref="Type"/>s of.</param>
		/// <param name="enableModuleAttribute"><see cref="INamedTypeSymbol"/> that represents the <see cref="EnableModuleAttribute"/> class.</param>
		/// <param name="types">Array of <see cref="TypeIdentity"/>s to pick the disabled ones from.</param>
		/// <exception cref="ArgumentNullException"><paramref name="compilation"/> is <see langword="null"/>. -or- <paramref name="enableModuleAttribute"/> is <see langword="null"/>.</exception>
		public static IEnumerable<INamedTypeSymbol> GetDisabledDurianTypes(this Compilation compilation, INamedTypeSymbol enableModuleAttribute, params TypeIdentity[]? types)
		{
			if (compilation is null)
			{
				throw new ArgumentNullException(nameof(compilation));
			}

			if (enableModuleAttribute is null)
			{
				throw new ArgumentNullException(nameof(enableModuleAttribute));
			}

			if (types is null)
			{
				return Array.Empty<INamedTypeSymbol>();
			}

			AttributeData[] attributes = GetInstancesOfEnableAttribute(compilation, enableModuleAttribute);

			if (attributes.Length == 0)
			{
				return Array.Empty<INamedTypeSymbol>();
			}

			Dictionary<DurianModule, bool> modules = new();
			List<INamedTypeSymbol> list = new(types.Length);

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
							INamedTypeSymbol symbol = compilation.ToSymbol(type);
							list.Add(symbol);
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
		/// Returns a collection of <see cref="INamedTypeSymbol"/>s picked from the given array of <paramref name="types"/> that are disabled for the specified <paramref name="types"/>.
		/// </summary>
		/// <param name="compilation"><see cref="Compilation"/> to get the disabled Durian <see cref="Type"/>s of.</param>
		/// <param name="types">Array of <see cref="INamedTypeSymbol"/>s to pick the disabled ones from.</param>
		/// <exception cref="ArgumentNullException"><paramref name="compilation"/> is <see langword="null"/>.</exception>
		/// <exception cref="InvalidOperationException">Error while resolving the <see cref="EnableModuleAttribute"/> class. -or- Symbol is not a Durian type.</exception>
		public static IEnumerable<INamedTypeSymbol> GetDisabledDurianTypes(this Compilation compilation, params INamedTypeSymbol[]? types)
		{
			if (compilation is null)
			{
				throw new ArgumentNullException(nameof(compilation));
			}

			if (types is null)
			{
				return Array.Empty<INamedTypeSymbol>();
			}

			INamedTypeSymbol symbol = GetEnableAttributeSymbol(compilation);
			return compilation.GetDisabledDurianTypes(symbol, types);
		}

		/// <summary>
		/// Returns a collection of <see cref="INamedTypeSymbol"/>s picked from the given array of <paramref name="types"/> that are disabled for the specified <paramref name="types"/>.
		/// </summary>
		/// <param name="compilation"><see cref="Compilation"/> to get the disabled Durian <see cref="Type"/>s of.</param>
		/// <param name="enableModuleAttribute"><see cref="INamedTypeSymbol"/> that represents the <see cref="EnableModuleAttribute"/> class.</param>
		/// <param name="types">Array of <see cref="INamedTypeSymbol"/>s to pick the disabled ones from.</param>
		/// <exception cref="ArgumentNullException"><paramref name="compilation"/> is <see langword="null"/>. -or- <paramref name="enableModuleAttribute"/> is <see langword="null"/>.</exception>
		public static IEnumerable<INamedTypeSymbol> GetDisabledDurianTypes(this Compilation compilation, INamedTypeSymbol enableModuleAttribute, params INamedTypeSymbol[]? types)
		{
			if (compilation is null)
			{
				throw new ArgumentNullException(nameof(compilation));
			}

			if (enableModuleAttribute is null)
			{
				throw new ArgumentNullException(nameof(enableModuleAttribute));
			}

			if (types is null)
			{
				return Array.Empty<INamedTypeSymbol>();
			}

			AttributeData[] attributes = GetInstancesOfEnableAttribute(compilation, enableModuleAttribute);

			if (attributes.Length == 0)
			{
				return Array.Empty<INamedTypeSymbol>();
			}

			Dictionary<DurianModule, bool> modules = new();
			List<INamedTypeSymbol> list = new(types.Length);

			foreach (INamedTypeSymbol type in types)
			{
				if (type is null || !type.IsPartOfAnyModule())
				{
					continue;
				}

				TypeIdentity identity = type.GetIdentity();

				foreach (ModuleReference reference in identity.Modules)
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
		/// Returns a collection of <see cref="INamedTypeSymbol"/>s representing all disabled Durian <see cref="Type"/>s for the specified <paramref name="compilation"/> that are part of any of the provided <paramref name="modules"/>.
		/// </summary>
		/// <param name="compilation"><see cref="Compilation"/> to get the disabled Durian <see cref="Type"/>s of.</param>
		/// <param name="modules">Array of <see cref="ModuleIdentity"/>s to pick the <see cref="INamedTypeSymbol"/>s from.</param>
		/// <exception cref="ArgumentNullException"><paramref name="compilation"/> is <see langword="null"/>.</exception>
		/// <exception cref="InvalidOperationException">Error while resolving the <see cref="EnableModuleAttribute"/> class.</exception>
		public static IEnumerable<INamedTypeSymbol> GetDisabledDurianTypes(this Compilation compilation, params ModuleIdentity[]? modules)
		{
			if (compilation is null)
			{
				throw new ArgumentNullException(nameof(compilation));
			}

			if (modules is null || modules.Length == 0)
			{
				return Array.Empty<INamedTypeSymbol>();
			}

			INamedTypeSymbol symbol = GetEnableAttributeSymbol(compilation);
			return compilation.GetDisabledDurianTypes(symbol, modules);
		}

		/// <summary>
		/// Returns a collection of <see cref="INamedTypeSymbol"/>s representing all disabled Durian <see cref="Type"/>s for the specified <paramref name="compilation"/> that are part of any of the provided <paramref name="modules"/>.
		/// </summary>
		/// <param name="compilation"><see cref="Compilation"/> to get the disabled Durian <see cref="Type"/>s of.</param>
		/// <param name="enableModuleAttribute"><see cref="INamedTypeSymbol"/> that represents the <see cref="EnableModuleAttribute"/> class.</param>
		/// <param name="modules">Array of <see cref="ModuleIdentity"/>s to pick the <see cref="INamedTypeSymbol"/>s from.</param>
		/// <exception cref="ArgumentNullException"><paramref name="compilation"/> is <see langword="null"/>. -or- <paramref name="enableModuleAttribute"/> is <see langword="null"/>.</exception>
		public static IEnumerable<INamedTypeSymbol> GetDisabledDurianTypes(this Compilation compilation, INamedTypeSymbol enableModuleAttribute, params ModuleIdentity[]? modules)
		{
			if (compilation is null)
			{
				throw new ArgumentNullException(nameof(compilation));
			}

			if (enableModuleAttribute is null)
			{
				throw new ArgumentNullException(nameof(enableModuleAttribute));
			}

			if (modules is null || modules.Length == 0)
			{
				return Array.Empty<INamedTypeSymbol>();
			}

			AttributeData[] attributes = GetInstancesOfEnableAttribute(compilation, enableModuleAttribute);

			if (attributes.Length == 0)
			{
				return Array.Empty<INamedTypeSymbol>();
			}

			return Yield();

			IEnumerable<INamedTypeSymbol> Yield()
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
							yield return compilation.ToSymbol(t);
						}
					}
				}
			}
		}

		/// <summary>
		/// Returns a collection of <see cref="INamedTypeSymbol"/>s representing all disabled Durian <see cref="Type"/>s for the specified <paramref name="compilation"/> that are part of any of the provided modules.
		/// </summary>
		/// <param name="compilation"><see cref="Compilation"/> to get the disabled Durian <see cref="Type"/>s of.</param>
		/// <param name="references">Array of <see cref="ModuleReference"/>s to pick the <see cref="INamedTypeSymbol"/>s from.</param>
		/// <exception cref="ArgumentNullException"><paramref name="compilation"/> is <see langword="null"/>.</exception>
		/// <exception cref="InvalidOperationException">Error while resolving the <see cref="EnableModuleAttribute"/> class.</exception>
		public static IEnumerable<INamedTypeSymbol> GetDisabledDurianTypes(this Compilation compilation, params ModuleReference[]? references)
		{
			if (compilation is null)
			{
				throw new ArgumentNullException(nameof(compilation));
			}

			if (references is null || references.Length == 0)
			{
				return Array.Empty<INamedTypeSymbol>();
			}

			INamedTypeSymbol symbol = GetEnableAttributeSymbol(compilation);
			return compilation.GetDisabledDurianTypes(symbol, references);
		}

		/// <summary>
		/// Returns a collection of <see cref="INamedTypeSymbol"/>s representing all disabled Durian <see cref="Type"/>s for the specified <paramref name="compilation"/> that are part of any of the provided modules.
		/// </summary>
		/// <param name="compilation"><see cref="Compilation"/> to get the disabled Durian <see cref="Type"/>s of.</param>
		/// <param name="enableModuleAttribute"><see cref="INamedTypeSymbol"/> that represents the <see cref="EnableModuleAttribute"/> class.</param>
		/// <param name="references">Array of <see cref="ModuleReference"/>s to pick the <see cref="INamedTypeSymbol"/>s from.</param>
		/// <exception cref="ArgumentNullException"><paramref name="compilation"/> is <see langword="null"/>. -or- <paramref name="enableModuleAttribute"/> is <see langword="null"/>.</exception>
		public static IEnumerable<INamedTypeSymbol> GetDisabledDurianTypes(this Compilation compilation, INamedTypeSymbol enableModuleAttribute, params ModuleReference[]? references)
		{
			if (compilation is null)
			{
				throw new ArgumentNullException(nameof(compilation));
			}

			if (enableModuleAttribute is null)
			{
				throw new ArgumentNullException(nameof(enableModuleAttribute));
			}

			if (references is null || references.Length == 0)
			{
				return Array.Empty<INamedTypeSymbol>();
			}

			AttributeData[] attributes = GetInstancesOfEnableAttribute(compilation, enableModuleAttribute);

			if (attributes.Length == 0)
			{
				return Array.Empty<INamedTypeSymbol>();
			}

			return Yield();

			IEnumerable<INamedTypeSymbol> Yield()
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
							yield return compilation.ToSymbol(t);
						}
					}
				}
			}
		}

		/// <summary>
		/// Returns a collection of <see cref="INamedTypeSymbol"/>s representing all disabled Durian <see cref="Type"/>s for the specified <paramref name="compilation"/> that are part of any of the provided <paramref name="modules"/>.
		/// </summary>
		/// <param name="compilation"><see cref="Compilation"/> to get the disabled Durian <see cref="Type"/>s of.</param>
		/// <param name="modules">Array of <see cref="DurianModule"/>s to pick the <see cref="INamedTypeSymbol"/>s from.</param>
		/// <exception cref="ArgumentNullException"><paramref name="compilation"/> is <see langword="null"/>.</exception>
		/// <exception cref="InvalidOperationException"> Unknown <see cref="DurianModule"/> value detected. -or- <see cref="DurianModule.None"/> is not a valid Durian module. -or- Error while resolving the <see cref="EnableModuleAttribute"/> class.</exception>
		public static IEnumerable<INamedTypeSymbol> GetDisabledDurianTypes(this Compilation compilation, params DurianModule[]? modules)
		{
			if (compilation is null)
			{
				throw new ArgumentNullException(nameof(compilation));
			}

			if (modules is null || modules.Length == 0)
			{
				return Array.Empty<INamedTypeSymbol>();
			}

			INamedTypeSymbol symbol = GetEnableAttributeSymbol(compilation);
			return compilation.GetDisabledDurianTypes(symbol, modules);
		}

		/// <summary>
		/// Returns a collection of <see cref="INamedTypeSymbol"/>s representing all disabled Durian <see cref="Type"/>s for the specified <paramref name="compilation"/> that are part of any of the provided <paramref name="modules"/>.
		/// </summary>
		/// <param name="compilation"><see cref="Compilation"/> to get the disabled Durian <see cref="Type"/>s of.</param>
		/// <param name="enableModuleAttribute"><see cref="INamedTypeSymbol"/> that represents the <see cref="EnableModuleAttribute"/> class.</param>
		/// <param name="modules">Array of <see cref="DurianModule"/>s to pick the <see cref="INamedTypeSymbol"/>s from.</param>
		/// <exception cref="ArgumentNullException"><paramref name="compilation"/> is <see langword="null"/>. -or- <paramref name="enableModuleAttribute"/> is <see langword="null"/>.</exception>
		/// <exception cref="InvalidOperationException"> Unknown <see cref="DurianModule"/> value detected. -or- <see cref="DurianModule.None"/> is not a valid Durian module.</exception>
		public static IEnumerable<INamedTypeSymbol> GetDisabledDurianTypes(this Compilation compilation, INamedTypeSymbol enableModuleAttribute, params DurianModule[]? modules)
		{
			if (compilation is null)
			{
				throw new ArgumentNullException(nameof(compilation));
			}

			if (enableModuleAttribute is null)
			{
				throw new ArgumentNullException(nameof(enableModuleAttribute));
			}

			if (modules is null || modules.Length == 0)
			{
				return Array.Empty<INamedTypeSymbol>();
			}

			foreach (DurianModule module in modules)
			{
				ModuleIdentity.EnsureIsValidModuleEnum_InvOp(module);
			}

			AttributeData[] attributes = GetInstancesOfEnableAttribute(compilation, enableModuleAttribute);

			if (attributes.Length == 0)
			{
				return Array.Empty<INamedTypeSymbol>();
			}

			return Yield();

			IEnumerable<INamedTypeSymbol> Yield()
			{
				foreach (DurianModule module in modules)
				{
					if (!IsEnabled_Internal(attributes, module))
					{
						ModuleIdentity identity = ModuleIdentity.GetModule(module);

						foreach (TypeIdentity t in identity.Types)
						{
							yield return compilation.ToSymbol(t);
						}
					}
				}
			}
		}

		/// <summary>
		/// Returns a collection of all Durian modules that are disabled for the specified <paramref name="compilation"/>.
		/// </summary>
		/// <param name="compilation"><see cref="Compilation"/> to get all the disabled Durian modules of.</param>
		/// <returns>A new instance of <see cref="ModuleContainer"/> that contains the disabled Durian modules.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="compilation"/> is <see langword="null"/>.</exception>
		/// <exception cref="InvalidOperationException">Error while resolving the <see cref="EnableModuleAttribute"/> class.</exception>
		public static ModuleContainer GetDisabledModules(this Compilation compilation)
		{
			if (compilation is null)
			{
				throw new ArgumentNullException(nameof(compilation));
			}

			INamedTypeSymbol symbol = GetEnableAttributeSymbol(compilation);

			ModuleContainer all = ModuleIdentity.GetAllModules();

			return compilation.GetDisabledModules(symbol, all.AsEnums());
		}

		/// <summary>
		/// Returns a collection of all Durian modules that are disabled for the specified <paramref name="compilation"/>.
		/// </summary>
		/// <param name="compilation"><see cref="Compilation"/> to get all the disabled Durian modules of.</param>
		/// <param name="enableModuleAttribute"><see cref="INamedTypeSymbol"/> that represents the <see cref="EnableModuleAttribute"/> class.</param>
		/// <returns>A new instance of <see cref="ModuleContainer"/> that contains the disabled Durian modules.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="compilation"/> is <see langword="null"/>. -or- <paramref name="enableModuleAttribute"/> is <see langword="null"/>.</exception>
		public static ModuleContainer GetDisabledModules(this Compilation compilation, INamedTypeSymbol enableModuleAttribute)
		{
			if (compilation is null)
			{
				throw new ArgumentNullException(nameof(compilation));
			}

			if (enableModuleAttribute is null)
			{
				throw new ArgumentNullException(nameof(enableModuleAttribute));
			}

			ModuleContainer all = ModuleIdentity.GetAllModules();

			return compilation.GetDisabledModules(enableModuleAttribute, all.AsEnums());
		}

		/// <summary>
		/// Returns a collection of all Durian modules present in the provided collection of <paramref name="modules"/> that are disabled for the specified <paramref name="compilation"/>.
		/// </summary>
		/// <param name="compilation"><see cref="Compilation"/> to get the disabled Durian modules from.</param>
		/// <param name="modules"><see cref="ModuleContainer"/> that provides a collection of Durian modules to pick from.</param>
		/// <returns>A new instance of <see cref="ModuleContainer"/> that contains the disabled Durian modules.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="compilation"/> is <see langword="null"/>. -or- <paramref name="modules"/> is <see langword="null"/>.</exception>
		/// <exception cref="InvalidOperationException">Error while resolving the <see cref="EnableModuleAttribute"/> class.</exception>
		public static ModuleContainer GetDisabledModules(this Compilation compilation, ModuleContainer modules)
		{
			if (compilation is null)
			{
				throw new ArgumentNullException(nameof(compilation));
			}

			if (modules is null)
			{
				throw new ArgumentNullException(nameof(modules));
			}

			if (modules.Count == 0)
			{
				return new ModuleContainer();
			}

			INamedTypeSymbol symbol = GetEnableAttributeSymbol(compilation);

			return compilation.GetDisabledModules(symbol, modules.AsEnums());
		}

		/// <summary>
		/// Returns a collection of all Durian modules present in the provided collection of <paramref name="modules"/> that are disabled for the specified <paramref name="compilation"/>.
		/// </summary>
		/// <param name="compilation"><see cref="Compilation"/> to get the disabled Durian modules from.</param>
		/// <param name="enableModuleAttribute"><see cref="INamedTypeSymbol"/> that represents the <see cref="EnableModuleAttribute"/> class.</param>
		/// <param name="modules"><see cref="ModuleContainer"/> that provides a collection of Durian modules to pick from.</param>
		/// <returns>A new instance of <see cref="ModuleContainer"/> that contains the disabled Durian modules.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="compilation"/> is <see langword="null"/>. -or- <paramref name="enableModuleAttribute"/> is <see langword="null"/>. -or- <paramref name="modules"/> is <see langword="null"/>.</exception>
		public static ModuleContainer GetDisabledModules(this Compilation compilation, INamedTypeSymbol enableModuleAttribute, ModuleContainer modules)
		{
			if (compilation is null)
			{
				throw new ArgumentNullException(nameof(compilation));
			}

			if (enableModuleAttribute is null)
			{
				throw new ArgumentNullException(nameof(enableModuleAttribute));
			}

			if (modules is null)
			{
				throw new ArgumentNullException(nameof(modules));
			}

			if (modules.Count == 0)
			{
				return new ModuleContainer();
			}

			return compilation.GetDisabledModules(enableModuleAttribute, modules.AsEnums());
		}

		/// <summary>
		/// Returns a collection of all Durian modules present in the provided array of <paramref name="references"/> that are disabled for the specified <paramref name="compilation"/>.
		/// </summary>
		/// <param name="compilation"><see cref="Compilation"/> to get the disabled Durian modules of.</param>
		/// <param name="references">Array of <see cref="ModuleReference"/>s to pick from.</param>
		/// <returns>A new instance of <see cref="ModuleContainer"/> that contains the disabled Durian modules.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="compilation"/> is <see langword="null"/>.</exception>
		/// <exception cref="InvalidOperationException">Error while resolving the <see cref="EnableModuleAttribute"/> class.</exception>
		public static ModuleContainer GetDisabledModules(this Compilation compilation, params ModuleReference[]? references)
		{
			if (compilation is null)
			{
				throw new ArgumentNullException(nameof(compilation));
			}

			if (references is null || references.Length == 0)
			{
				return new ModuleContainer();
			}

			INamedTypeSymbol symbol = GetEnableAttributeSymbol(compilation);
			return compilation.GetDisabledModules(symbol, references);
		}

		/// <summary>
		/// Returns a collection of all Durian modules present in the provided array of <paramref name="references"/> that are disabled for the specified <paramref name="compilation"/>.
		/// </summary>
		/// <param name="compilation"><see cref="Compilation"/> to get the disabled Durian modules of.</param>
		/// <param name="enableModuleAttribute"><see cref="INamedTypeSymbol"/> that represents the <see cref="EnableModuleAttribute"/> class.</param>
		/// <param name="references">Array of <see cref="ModuleReference"/>s to pick from.</param>
		/// <returns>A new instance of <see cref="ModuleContainer"/> that contains the disabled Durian modules.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="compilation"/> is <see langword="null"/>. -or- <paramref name="enableModuleAttribute"/> is <see langword="null"/>.</exception>
		public static ModuleContainer GetDisabledModules(this Compilation compilation, INamedTypeSymbol enableModuleAttribute, params ModuleReference[]? references)
		{
			if (compilation is null)
			{
				throw new ArgumentNullException(nameof(compilation));
			}

			if (enableModuleAttribute is null)
			{
				throw new ArgumentNullException(nameof(enableModuleAttribute));
			}

			if (references is null || references.Length == 0)
			{
				return new ModuleContainer();
			}

			AttributeData[] attributes = GetInstancesOfEnableAttribute(compilation, enableModuleAttribute);

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
		/// Returns a collection of all Durian modules present in the provided array of <paramref name="modules"/> that are disabled for the specified <paramref name="compilation"/>.
		/// </summary>
		/// <param name="compilation"><see cref="Compilation"/> to get the disabled Durian modules of.</param>
		/// <param name="modules">Array of <see cref="ModuleIdentity"/>s to pick from.</param>
		/// <returns>A new instance of <see cref="ModuleContainer"/> that contains the disabled Durian modules.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="compilation"/> is <see langword="null"/>.</exception>
		/// <exception cref="InvalidOperationException">Error while resolving the <see cref="EnableModuleAttribute"/> class.</exception>
		public static ModuleContainer GetDisabledModules(this Compilation compilation, params ModuleIdentity[]? modules)
		{
			if (compilation is null)
			{
				throw new ArgumentNullException(nameof(compilation));
			}

			if (modules is null || modules.Length == 0)
			{
				return new ModuleContainer();
			}

			INamedTypeSymbol symbol = GetEnableAttributeSymbol(compilation);
			return compilation.GetDisabledModules(symbol, modules);
		}

		/// <summary>
		/// Returns a collection of all Durian modules present in the provided array of <paramref name="modules"/> that are disabled for the specified <paramref name="compilation"/>.
		/// </summary>
		/// <param name="compilation"><see cref="Compilation"/> to get the disabled Durian modules of.</param>
		/// <param name="enableModuleAttribute"><see cref="INamedTypeSymbol"/> that represents the <see cref="EnableModuleAttribute"/> class.</param>
		/// <param name="modules">Array of <see cref="ModuleIdentity"/>s to pick from.</param>
		/// <returns>A new instance of <see cref="ModuleContainer"/> that contains the disabled Durian modules.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="compilation"/> is <see langword="null"/>. -or- <paramref name="enableModuleAttribute"/> is <see langword="null"/>.</exception>
		public static ModuleContainer GetDisabledModules(this Compilation compilation, INamedTypeSymbol enableModuleAttribute, params ModuleIdentity[]? modules)
		{
			if (compilation is null)
			{
				throw new ArgumentNullException(nameof(compilation));
			}

			if (enableModuleAttribute is null)
			{
				throw new ArgumentNullException(nameof(enableModuleAttribute));
			}

			if (modules is null || modules.Length == 0)
			{
				return new ModuleContainer();
			}

			AttributeData[] attributes = GetInstancesOfEnableAttribute(compilation, enableModuleAttribute);

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
		/// Returns a collection of all Durian modules present in the provided array of <paramref name="modules"/> that are disabled for the specified <paramref name="compilation"/>.
		/// </summary>
		/// <param name="compilation"><see cref="Compilation"/> to get the disabled Durian modules of.</param>
		/// <param name="modules">Array of <see cref="DurianModule"/>s to pick the disabled modules from.</param>
		/// <returns>A new instance of <see cref="ModuleContainer"/> that contains the disabled Durian modules.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="compilation"/> is <see langword="null"/>.</exception>
		/// <exception cref="InvalidOperationException">Unknown <see cref="DurianModule"/> value detected. -or- <see cref="DurianModule.None"/> is not a valid Durian module. -or-
		/// Error while resolving the <see cref="EnableModuleAttribute"/> class.</exception>
		public static ModuleContainer GetDisabledModules(this Compilation compilation, params DurianModule[]? modules)
		{
			if (compilation is null)
			{
				throw new ArgumentNullException(nameof(compilation));
			}

			if (modules is null || modules.Length == 0)
			{
				return new ModuleContainer();
			}

			INamedTypeSymbol symbol = GetEnableAttributeSymbol(compilation);
			return compilation.GetDisabledModules(symbol, modules);
		}

		/// <summary>
		/// Returns a collection of all Durian modules present in the provided array of <paramref name="modules"/> that are disabled for the specified <paramref name="compilation"/>.
		/// </summary>
		/// <param name="compilation"><see cref="Compilation"/> to get the disabled Durian modules of.</param>
		/// <param name="enableModuleAttribute"><see cref="INamedTypeSymbol"/> that represents the <see cref="EnableModuleAttribute"/> class.</param>
		/// <param name="modules">Array of <see cref="DurianModule"/>s to pick the disabled modules from.</param>
		/// <returns>A new instance of <see cref="ModuleContainer"/> that contains the disabled Durian modules.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="compilation"/> is <see langword="null"/>. -or- <paramref name="enableModuleAttribute"/> is <see langword="null"/>.</exception>
		/// <exception cref="InvalidOperationException">Unknown <see cref="DurianModule"/> value detected. -or- <see cref="DurianModule.None"/> is not a valid Durian module.</exception>
		public static ModuleContainer GetDisabledModules(this Compilation compilation, INamedTypeSymbol enableModuleAttribute, params DurianModule[]? modules)
		{
			if (compilation is null)
			{
				throw new ArgumentNullException(nameof(compilation));
			}

			if (enableModuleAttribute is null)
			{
				throw new ArgumentNullException(nameof(enableModuleAttribute));
			}

			if (modules is null || modules.Length == 0)
			{
				return new ModuleContainer();
			}

			foreach (DurianModule module in modules)
			{
				ModuleIdentity.EnsureIsValidModuleEnum_InvOp(module);
			}

			AttributeData[] attributes = GetInstancesOfEnableAttribute(compilation, enableModuleAttribute);

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
		/// Returns a collection of <see cref="INamedTypeSymbol"/>s representing all available Durian types.
		/// </summary>
		/// <param name="compilation"><see cref="Compilation"/> to get the <see cref="INamedTypeSymbol"/>s from.</param>
		/// <exception cref="ArgumentNullException"><paramref name="compilation"/> is <see langword="null"/>.</exception>
		/// <exception cref="InvalidOperationException">Target type could not be resolved.</exception>
		public static IEnumerable<INamedTypeSymbol> GetDurianTypes(this Compilation compilation)
		{
			if (compilation is null)
			{
				throw new ArgumentNullException(nameof(compilation));
			}

			return Yield();

			IEnumerable<INamedTypeSymbol> Yield()
			{
				foreach (TypeIdentity type in TypeIdentity.GetAllTypes())
				{
					yield return compilation.ToSymbol(type);
				}
			}
		}

		/// <summary>
		/// Returns a collection of <see cref="INamedTypeSymbol"/>s representing all Durian types that can be found in the provided array of <paramref name="types"/>.
		/// </summary>
		/// <param name="compilation"><see cref="Compilation"/> to get the <see cref="INamedTypeSymbol"/>s from.</param>
		/// <param name="types">Array of <see cref="INamedTypeSymbol"/>s to pick Durian types from.</param>
		/// <exception cref="ArgumentNullException"><paramref name="compilation"/> is <see langword="null"/>.</exception>
		/// <exception cref="InvalidOperationException">Symbol is not a Durian type.</exception>
		public static IEnumerable<INamedTypeSymbol> GetDurianTypes(this Compilation compilation, params INamedTypeSymbol[]? types)
		{
			if (compilation is null)
			{
				throw new ArgumentNullException(nameof(compilation));
			}

			if (types is null || types.Length == 0)
			{
				return Array.Empty<INamedTypeSymbol>();
			}

			return Yield();

			IEnumerable<INamedTypeSymbol> Yield()
			{
				foreach (INamedTypeSymbol type in types)
				{
					if (type is null)
					{
						continue;
					}

					if (IsPartOfAnyModule(type))
					{
						yield return type;
					}
				}
			}
		}

		/// <summary>
		/// Returns a collection of <see cref="INamedTypeSymbol"/>s representing all Durian types that are part of the specified <paramref name="module"/>.
		/// </summary>
		/// <param name="compilation"><see cref="Compilation"/> to get the <see cref="INamedTypeSymbol"/>s from.</param>
		/// <param name="module"><see cref="ModuleIdentity"/> to get the <see cref="INamedTypeSymbol"/> of.</param>
		/// <exception cref="ArgumentNullException"><paramref name="compilation"/> is <see langword="null"/>. -or- <paramref name="module"/> is <see langword="null"/>.</exception>
		/// <exception cref="InvalidOperationException">Target type could not be resolved.</exception>
		public static IEnumerable<INamedTypeSymbol> GetDurianTypes(this Compilation compilation, ModuleIdentity module)
		{
			if (compilation is null)
			{
				throw new ArgumentNullException(nameof(compilation));
			}

			if (module is null)
			{
				throw new ArgumentNullException(nameof(module));
			}

			return Yield();

			IEnumerable<INamedTypeSymbol> Yield()
			{
				foreach (TypeIdentity type in module.Types)
				{
					yield return compilation.ToSymbol(type);
				}
			}
		}

		/// <summary>
		/// Returns a collection of <see cref="INamedTypeSymbol"/>s representing all Durian types that are part of the specified module.
		/// </summary>
		/// <param name="compilation"><see cref="Compilation"/> to get the <see cref="INamedTypeSymbol"/>s from.</param>
		/// <param name="reference"><see cref="ModuleReference"/> pointing to a <see cref="ModuleIdentity"/> to get the <see cref="INamedTypeSymbol"/> of.</param>
		/// <exception cref="ArgumentNullException"><paramref name="compilation"/> is <see langword="null"/>. -or- <paramref name="reference"/> is <see langword="null"/>.</exception>
		/// <exception cref="InvalidOperationException">Target type could not be resolved.</exception>
		public static IEnumerable<INamedTypeSymbol> GetDurianTypes(this Compilation compilation, ModuleReference reference)
		{
			if (compilation is null)
			{
				throw new ArgumentNullException(nameof(compilation));
			}

			if (reference is null)
			{
				throw new ArgumentNullException(nameof(reference));
			}

			ModuleIdentity identity = reference.GetModule();
			return compilation.GetDurianTypes(identity);
		}

		/// <summary>
		/// Returns a collection of <see cref="INamedTypeSymbol"/>s representing all Durian types that are part of the specified <paramref name="module"/>.
		/// </summary>
		/// <param name="compilation"><see cref="Compilation"/> to get the <see cref="INamedTypeSymbol"/>s from.</param>
		/// <param name="module"><see cref="DurianModule"/> to get the <see cref="INamedTypeSymbol"/> of.</param>
		/// <exception cref="ArgumentNullException"><paramref name="compilation"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentException">Unknown <see cref="DurianModule"/> value detected. -or- <see cref="DurianModule.None"/> is not a valid Durian module. -or- Target type could not be resolved.</exception>
		public static IEnumerable<INamedTypeSymbol> GetDurianTypes(this Compilation compilation, DurianModule module)
		{
			if (compilation is null)
			{
				throw new ArgumentNullException(nameof(compilation));
			}

			ModuleIdentity identity = ModuleIdentity.GetModule(module);

			return compilation.GetDurianTypes(identity);
		}

		/// <summary>
		/// Returns a collection of <see cref="INamedTypeSymbol"/>s representing all enabled Durian <see cref="Type"/>s for the specified <paramref name="compilation"/> picked from the provided array of <paramref name="types"/>.
		/// </summary>
		/// <param name="compilation"><see cref="Compilation"/> to get the enabled Durian <see cref="Type"/>s of.</param>
		/// <param name="types">Array of <see cref="TypeIdentity"/>s to pick the enabled ones from.</param>
		/// <exception cref="ArgumentNullException"><paramref name="compilation"/> is <see langword="null"/>.</exception>
		/// <exception cref="InvalidOperationException">Error while resolving the <see cref="EnableModuleAttribute"/> class.</exception>
		public static IEnumerable<INamedTypeSymbol> GetEnabledDurianTypes(this Compilation compilation, params TypeIdentity[]? types)
		{
			if (compilation is null)
			{
				throw new ArgumentNullException(nameof(compilation));
			}

			if (types is null || types.Length == 0)
			{
				return Array.Empty<INamedTypeSymbol>();
			}

			INamedTypeSymbol symbol = GetEnableAttributeSymbol(compilation);
			return compilation.GetEnabledDurianTypes(symbol, types);
		}

		/// <summary>
		/// Returns a collection of <see cref="INamedTypeSymbol"/>s representing all enabled Durian <see cref="Type"/>s for the specified <paramref name="compilation"/> picked from the provided array of <paramref name="types"/>.
		/// </summary>
		/// <param name="compilation"><see cref="Compilation"/> to get the enabled Durian <see cref="Type"/>s of.</param>
		/// <param name="enableModuleAttribute"><see cref="INamedTypeSymbol"/> that represents the <see cref="EnableModuleAttribute"/> class.</param>
		/// <param name="types">Array of <see cref="TypeIdentity"/>s to pick the enabled ones from.</param>
		/// <exception cref="ArgumentNullException"><paramref name="compilation"/> is <see langword="null"/>. -or- <paramref name="enableModuleAttribute"/> is <see langword="null"/>.</exception>
		public static IEnumerable<INamedTypeSymbol> GetEnabledDurianTypes(this Compilation compilation, INamedTypeSymbol enableModuleAttribute, params TypeIdentity[]? types)
		{
			if (compilation is null)
			{
				throw new ArgumentNullException(nameof(compilation));
			}

			if (enableModuleAttribute is null)
			{
				throw new ArgumentNullException(nameof(enableModuleAttribute));
			}

			if (types is null || types.Length == 0)
			{
				return Array.Empty<INamedTypeSymbol>();
			}

			AttributeData[] attributes = GetInstancesOfEnableAttribute(compilation, enableModuleAttribute);

			if (attributes.Length == 0)
			{
				return Array.Empty<INamedTypeSymbol>();
			}

			Dictionary<DurianModule, bool> modules = new();
			List<INamedTypeSymbol> list = new(types.Length);

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
							INamedTypeSymbol symbol = compilation.ToSymbol(type);
							list.Add(symbol);
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
		/// Returns a collection of <see cref="INamedTypeSymbol"/>s representing all enabled Durian <see cref="Type"/>s for the specified <paramref name="compilation"/>.
		/// </summary>
		/// <param name="compilation"><see cref="Compilation"/> to get the enabled Durian <see cref="Type"/>s of.</param>
		/// <exception cref="ArgumentNullException"><paramref name="compilation"/> is <see langword="null"/>.</exception>
		/// <exception cref="InvalidOperationException">Error while resolving the <see cref="EnableModuleAttribute"/> class.</exception>
		public static IEnumerable<INamedTypeSymbol> GetEnabledDurianTypes(this Compilation compilation)
		{
			if (compilation is null)
			{
				throw new ArgumentNullException(nameof(compilation));
			}

			INamedTypeSymbol symbol = GetEnableAttributeSymbol(compilation);
			ModuleReference[] modules = compilation.GetEnabledModules().AsReferences();

			return compilation.GetEnabledDurianTypes(symbol, modules);
		}

		/// <summary>
		/// Returns a collection of <see cref="INamedTypeSymbol"/>s representing all enabled Durian <see cref="Type"/>s for the specified <paramref name="compilation"/>.
		/// </summary>
		/// <param name="compilation"><see cref="Compilation"/> to get the enabled Durian <see cref="Type"/>s of.</param>
		/// <param name="enableModuleAttribute"><see cref="INamedTypeSymbol"/> that represents the <see cref="EnableModuleAttribute"/> class.</param>
		/// <exception cref="ArgumentNullException"><paramref name="compilation"/> is <see langword="null"/>. -or- <paramref name="enableModuleAttribute"/> is <see langword="null"/>.</exception>
		public static IEnumerable<INamedTypeSymbol> GetEnabledDurianTypes(this Compilation compilation, INamedTypeSymbol enableModuleAttribute)
		{
			if (compilation is null)
			{
				throw new ArgumentNullException(nameof(compilation));
			}

			if (enableModuleAttribute is null)
			{
				throw new ArgumentNullException(nameof(enableModuleAttribute));
			}

			ModuleReference[] modules = compilation.GetEnabledModules().AsReferences();

			return compilation.GetEnabledDurianTypes(enableModuleAttribute, modules);
		}

		/// <summary>
		/// Returns a collection of <see cref="INamedTypeSymbol"/>s representing all enabled Durian <see cref="Type"/>s for the specified <paramref name="compilation"/> that are part of any of the provided <paramref name="modules"/>.
		/// </summary>
		/// <param name="compilation"><see cref="Compilation"/> to get the enabled Durian <see cref="Type"/>s of.</param>
		/// <param name="modules"><see cref="ModuleContainer"/> that provides a collection of Durian modules to pick the <see cref="INamedTypeSymbol"/>s from.</param>
		/// <exception cref="ArgumentNullException"><paramref name="compilation"/> is <see langword="null"/>. -or- <paramref name="modules"/> is <see langword="null"/>.</exception>
		/// <exception cref="InvalidOperationException">Error while resolving the <see cref="EnableModuleAttribute"/> class.</exception>
		public static IEnumerable<INamedTypeSymbol> GetEnabledDurianTypes(this Compilation compilation, ModuleContainer modules)
		{
			if (compilation is null)
			{
				throw new ArgumentNullException(nameof(compilation));
			}

			if (modules is null)
			{
				throw new ArgumentNullException(nameof(modules));
			}

			if (modules.Count == 0)
			{
				return Array.Empty<INamedTypeSymbol>();
			}

			INamedTypeSymbol symbol = GetEnableAttributeSymbol(compilation);

			ModuleReference[] references = modules.AsReferences();

			return compilation.GetEnabledDurianTypes(symbol, references);
		}

		/// <summary>
		/// Returns a collection of <see cref="INamedTypeSymbol"/>s representing all enabled Durian <see cref="Type"/>s for the specified <paramref name="compilation"/> that are part of any of the provided <paramref name="modules"/>.
		/// </summary>
		/// <param name="compilation"><see cref="Compilation"/> to get the enabled Durian <see cref="Type"/>s of.</param>
		/// <param name="enableModuleAttribute"><see cref="INamedTypeSymbol"/> that represents the <see cref="EnableModuleAttribute"/> class.</param>
		/// <param name="modules"><see cref="ModuleContainer"/> that provides a collection of Durian modules to pick the <see cref="INamedTypeSymbol"/>s from.</param>
		/// <exception cref="ArgumentNullException"><paramref name="compilation"/> is <see langword="null"/>. -or- <paramref name="enableModuleAttribute"/> is <see langword="null"/>. -or- <paramref name="modules"/> is <see langword="null"/>.</exception>
		public static IEnumerable<INamedTypeSymbol> GetEnabledDurianTypes(this Compilation compilation, INamedTypeSymbol enableModuleAttribute, ModuleContainer modules)
		{
			if (compilation is null)
			{
				throw new ArgumentNullException(nameof(compilation));
			}

			if (enableModuleAttribute is null)
			{
				throw new ArgumentNullException(nameof(enableModuleAttribute));
			}

			if (modules is null)
			{
				throw new ArgumentNullException(nameof(modules));
			}

			if (modules.Count == 0)
			{
				return Array.Empty<INamedTypeSymbol>();
			}

			ModuleReference[] references = modules.AsReferences();

			return compilation.GetEnabledDurianTypes(enableModuleAttribute, references);
		}

		/// <summary>
		/// Returns a collection of <see cref="INamedTypeSymbol"/>s picked from the given array of <paramref name="types"/> that are enabled for the specified <paramref name="types"/>.
		/// </summary>
		/// <param name="compilation"><see cref="Compilation"/> to get the enabled Durian <see cref="Type"/>s of.</param>
		/// <param name="types">Array of <see cref="INamedTypeSymbol"/>s to pick the enabled ones from.</param>
		/// <exception cref="ArgumentNullException"><paramref name="compilation"/> is <see langword="null"/>.</exception>
		/// <exception cref="InvalidOperationException">Error while resolving the <see cref="EnableModuleAttribute"/> class. -or- Symbol is not a Durian type.</exception>
		public static IEnumerable<INamedTypeSymbol> GetEnabledDurianTypes(this Compilation compilation, params INamedTypeSymbol[]? types)
		{
			if (compilation is null)
			{
				throw new ArgumentNullException(nameof(compilation));
			}

			if (types is null || types.Length == 0)
			{
				return Array.Empty<INamedTypeSymbol>();
			}

			INamedTypeSymbol symbol = GetEnableAttributeSymbol(compilation);
			return compilation.GetEnabledDurianTypes(symbol, types);
		}

		/// <summary>
		/// Returns a collection of <see cref="INamedTypeSymbol"/>s picked from the given array of <paramref name="types"/> that are enabled for the specified <paramref name="types"/>.
		/// </summary>
		/// <param name="compilation"><see cref="Compilation"/> to get the enabled Durian <see cref="Type"/>s of.</param>
		/// <param name="enableModuleAttribute"><see cref="INamedTypeSymbol"/> that represents the <see cref="EnableModuleAttribute"/> class.</param>
		/// <param name="types">Array of <see cref="INamedTypeSymbol"/>s to pick the enabled ones from.</param>
		/// <exception cref="ArgumentNullException"><paramref name="compilation"/> is <see langword="null"/>. -or- <paramref name="enableModuleAttribute"/> is <see langword="null"/>.</exception>
		/// <exception cref="InvalidOperationException"> Symbol is not a Durian type.</exception>
		public static IEnumerable<INamedTypeSymbol> GetEnabledDurianTypes(this Compilation compilation, INamedTypeSymbol enableModuleAttribute, params INamedTypeSymbol[]? types)
		{
			if (compilation is null)
			{
				throw new ArgumentNullException(nameof(compilation));
			}

			if (enableModuleAttribute is null)
			{
				throw new ArgumentNullException(nameof(enableModuleAttribute));
			}

			if (types is null || types.Length == 0)
			{
				return Array.Empty<INamedTypeSymbol>();
			}

			AttributeData[] attributes = GetInstancesOfEnableAttribute(compilation, enableModuleAttribute);

			if (attributes.Length == 0)
			{
				return Array.Empty<INamedTypeSymbol>();
			}

			Dictionary<DurianModule, bool> modules = new();
			List<INamedTypeSymbol> list = new(types.Length);

			foreach (INamedTypeSymbol type in types)
			{
				if (type is null)
				{
					continue;
				}

				TypeIdentity identity = type.GetIdentity();

				foreach (ModuleReference reference in identity.Modules)
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
		/// Returns a collection of <see cref="INamedTypeSymbol"/>s representing all enabled Durian <see cref="Type"/>s for the specified <paramref name="compilation"/> that are part of any of the provided <paramref name="modules"/>.
		/// </summary>
		/// <param name="compilation"><see cref="Compilation"/> to get the enabled Durian <see cref="Type"/>s of.</param>
		/// <param name="modules">Array of <see cref="ModuleIdentity"/>s to pick the <see cref="INamedTypeSymbol"/>s from.</param>
		/// <exception cref="ArgumentNullException"><paramref name="compilation"/> is <see langword="null"/>.</exception>
		/// <exception cref="InvalidOperationException">Error while resolving the <see cref="EnableModuleAttribute"/> class.</exception>
		public static IEnumerable<INamedTypeSymbol> GetEnabledDurianTypes(this Compilation compilation, params ModuleIdentity[]? modules)
		{
			if (compilation is null)
			{
				throw new ArgumentNullException(nameof(compilation));
			}

			if (modules is null || modules.Length == 0)
			{
				return Array.Empty<INamedTypeSymbol>();
			}

			INamedTypeSymbol symbol = GetEnableAttributeSymbol(compilation);
			return compilation.GetEnabledDurianTypes(symbol, modules);
		}

		/// <summary>
		/// Returns a collection of <see cref="INamedTypeSymbol"/>s representing all enabled Durian <see cref="Type"/>s for the specified <paramref name="compilation"/> that are part of any of the provided <paramref name="modules"/>.
		/// </summary>
		/// <param name="compilation"><see cref="Compilation"/> to get the enabled Durian <see cref="Type"/>s of.</param>
		/// <param name="enableModuleAttribute"><see cref="INamedTypeSymbol"/> that represents the <see cref="EnableModuleAttribute"/> class.</param>
		/// <param name="modules">Array of <see cref="ModuleIdentity"/>s to pick the <see cref="INamedTypeSymbol"/>s from.</param>
		/// <exception cref="ArgumentNullException"><paramref name="compilation"/> is <see langword="null"/>. -or- <paramref name="enableModuleAttribute"/> is <see langword="null"/>.</exception>
		public static IEnumerable<INamedTypeSymbol> GetEnabledDurianTypes(this Compilation compilation, INamedTypeSymbol enableModuleAttribute, params ModuleIdentity[]? modules)
		{
			if (compilation is null)
			{
				throw new ArgumentNullException(nameof(compilation));
			}

			if (enableModuleAttribute is null)
			{
				throw new ArgumentNullException(nameof(enableModuleAttribute));
			}

			if (modules is null || modules.Length == 0)
			{
				return Array.Empty<INamedTypeSymbol>();
			}

			AttributeData[] attributes = GetInstancesOfEnableAttribute(compilation, enableModuleAttribute);

			if (attributes.Length == 0)
			{
				return Array.Empty<INamedTypeSymbol>();
			}

			return Yield();

			IEnumerable<INamedTypeSymbol> Yield()
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
							yield return compilation.ToSymbol(t);
						}
					}
				}
			}
		}

		/// <summary>
		/// Returns a collection of <see cref="INamedTypeSymbol"/>s representing all enabled Durian <see cref="Type"/>s for the specified <paramref name="compilation"/> that are part of any of the provided modules.
		/// </summary>
		/// <param name="compilation"><see cref="Compilation"/> to get the enabled Durian <see cref="Type"/>s of.</param>
		/// <param name="references">Array of <see cref="ModuleReference"/>s to pick the <see cref="INamedTypeSymbol"/>s from.</param>
		/// <exception cref="ArgumentNullException"><paramref name="compilation"/> is <see langword="null"/>.</exception>
		/// <exception cref="InvalidOperationException">Error while resolving the <see cref="EnableModuleAttribute"/> class.</exception>
		public static IEnumerable<INamedTypeSymbol> GetEnabledDurianTypes(this Compilation compilation, params ModuleReference[]? references)
		{
			if (compilation is null)
			{
				throw new ArgumentNullException(nameof(compilation));
			}

			if (references is null || references.Length == 0)
			{
				return Array.Empty<INamedTypeSymbol>();
			}

			INamedTypeSymbol symbol = GetEnableAttributeSymbol(compilation);
			return compilation.GetEnabledDurianTypes(symbol, references);
		}

		/// <summary>
		/// Returns a collection of <see cref="INamedTypeSymbol"/>s representing all enabled Durian <see cref="Type"/>s for the specified <paramref name="compilation"/> that are part of any of the provided modules.
		/// </summary>
		/// <param name="compilation"><see cref="Compilation"/> to get the enabled Durian <see cref="Type"/>s of.</param>
		/// <param name="enableModuleAttribute"><see cref="INamedTypeSymbol"/> that represents the <see cref="EnableModuleAttribute"/> class.</param>
		/// <param name="references">Array of <see cref="ModuleReference"/>s to pick the <see cref="INamedTypeSymbol"/>s from.</param>
		/// <exception cref="ArgumentNullException"><paramref name="compilation"/> is <see langword="null"/>. -or- <paramref name="enableModuleAttribute"/> is <see langword="null"/>.</exception>
		public static IEnumerable<INamedTypeSymbol> GetEnabledDurianTypes(this Compilation compilation, INamedTypeSymbol enableModuleAttribute, params ModuleReference[]? references)
		{
			if (compilation is null)
			{
				throw new ArgumentNullException(nameof(compilation));
			}

			if (enableModuleAttribute is null)
			{
				throw new ArgumentNullException(nameof(enableModuleAttribute));
			}

			if (references is null || references.Length == 0)
			{
				return Array.Empty<INamedTypeSymbol>();
			}

			AttributeData[] attributes = GetInstancesOfEnableAttribute(compilation, enableModuleAttribute);

			if (attributes.Length == 0)
			{
				return Array.Empty<INamedTypeSymbol>();
			}

			return Yield();

			IEnumerable<INamedTypeSymbol> Yield()
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
							yield return compilation.ToSymbol(t);
						}
					}
				}
			}
		}

		/// <summary>
		/// Returns a collection of <see cref="INamedTypeSymbol"/>s representing all enabled Durian <see cref="Type"/>s for the specified <paramref name="compilation"/> that are part of any of the provided <paramref name="modules"/>.
		/// </summary>
		/// <param name="compilation"><see cref="Compilation"/> to get the enabled Durian <see cref="Type"/>s of.</param>
		/// <param name="modules">Array of <see cref="DurianModule"/>s to pick the <see cref="INamedTypeSymbol"/>s from.</param>
		/// <exception cref="ArgumentNullException"><paramref name="compilation"/> is <see langword="null"/>.</exception>
		/// <exception cref="InvalidOperationException">Unknown <see cref="DurianModule"/> value detected. -or- <see cref="DurianModule.None"/> is not a valid Durian module. -or- Error while resolving the <see cref="EnableModuleAttribute"/> class.</exception>
		public static IEnumerable<INamedTypeSymbol> GetEnabledDurianTypes(this Compilation compilation, params DurianModule[]? modules)
		{
			if (compilation is null)
			{
				throw new ArgumentNullException(nameof(compilation));
			}

			if (modules is null || modules.Length == 0)
			{
				return Array.Empty<INamedTypeSymbol>();
			}

			INamedTypeSymbol symbol = GetEnableAttributeSymbol(compilation);
			return compilation.GetEnabledDurianTypes(symbol, modules);
		}

		/// <summary>
		/// Returns a collection of <see cref="INamedTypeSymbol"/>s representing all enabled Durian <see cref="Type"/>s for the specified <paramref name="compilation"/> that are part of any of the provided <paramref name="modules"/>.
		/// </summary>
		/// <param name="compilation"><see cref="Compilation"/> to get the enabled Durian <see cref="Type"/>s of.</param>
		/// <param name="enableModuleAttribute"><see cref="INamedTypeSymbol"/> that represents the <see cref="EnableModuleAttribute"/> class.</param>
		/// <param name="modules">Array of <see cref="DurianModule"/>s to pick the <see cref="INamedTypeSymbol"/>s from.</param>
		/// <exception cref="ArgumentNullException"><paramref name="compilation"/> is <see langword="null"/>. -or- <paramref name="enableModuleAttribute"/> is <see langword="null"/>.</exception>
		/// <exception cref="InvalidOperationException">Unknown <see cref="DurianModule"/> value detected. -or- <see cref="DurianModule.None"/> is not a valid Durian module.</exception>
		public static IEnumerable<INamedTypeSymbol> GetEnabledDurianTypes(this Compilation compilation, INamedTypeSymbol enableModuleAttribute, params DurianModule[]? modules)
		{
			if (compilation is null)
			{
				throw new ArgumentNullException(nameof(compilation));
			}

			if (enableModuleAttribute is null)
			{
				throw new ArgumentNullException(nameof(enableModuleAttribute));
			}

			if (modules is null || modules.Length == 0)
			{
				return Array.Empty<INamedTypeSymbol>();
			}

			foreach (DurianModule module in modules)
			{
				ModuleIdentity.EnsureIsValidModuleEnum_InvOp(module);
			}

			AttributeData[] attributes = GetInstancesOfEnableAttribute(compilation, enableModuleAttribute);

			if (attributes.Length == 0)
			{
				return Array.Empty<INamedTypeSymbol>();
			}

			return Yield();

			IEnumerable<INamedTypeSymbol> Yield()
			{
				foreach (DurianModule module in modules)
				{
					if (IsEnabled_Internal(attributes, module))
					{
						ModuleIdentity identity = ModuleIdentity.GetModule(module);

						foreach (TypeIdentity t in identity.Types)
						{
							yield return compilation.ToSymbol(t);
						}
					}
				}
			}
		}

		/// <summary>
		/// Returns a collection of all Durian modules present in the provided array of <paramref name="references"/> that are enabled for the specified <paramref name="compilation"/>.
		/// </summary>
		/// <param name="compilation"><see cref="Compilation"/> to get the enabled Durian modules of.</param>
		/// <param name="references">Array of <see cref="ModuleReference"/>s to pick from.</param>
		/// <returns>A new instance of <see cref="ModuleContainer"/> that contains the enabled Durian modules.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="compilation"/> is <see langword="null"/>.</exception>
		/// <exception cref="InvalidOperationException">Error while resolving the <see cref="EnableModuleAttribute"/> class.</exception>>
		public static ModuleContainer GetEnabledModules(this Compilation compilation, params ModuleReference[]? references)
		{
			if (compilation is null)
			{
				throw new ArgumentNullException(nameof(compilation));
			}

			if (references is null || references.Length == 0)
			{
				return new ModuleContainer();
			}

			INamedTypeSymbol symbol = GetEnableAttributeSymbol(compilation);
			return compilation.GetEnabledModules(symbol, references);
		}

		/// <summary>
		/// Returns a collection of all Durian modules present in the provided array of <paramref name="references"/> that are enabled for the specified <paramref name="compilation"/>.
		/// </summary>
		/// <param name="compilation"><see cref="Compilation"/> to get the enabled Durian modules of.</param>
		/// <param name="enableModuleAttribute"><see cref="INamedTypeSymbol"/> that represents the <see cref="EnableModuleAttribute"/> class.</param>
		/// <param name="references">Array of <see cref="ModuleReference"/>s to pick from.</param>
		/// <returns>A new instance of <see cref="ModuleContainer"/> that contains the enabled Durian modules.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="compilation"/> is <see langword="null"/>. -or- <paramref name="enableModuleAttribute"/> is <see langword="null"/>.</exception>
		public static ModuleContainer GetEnabledModules(this Compilation compilation, INamedTypeSymbol enableModuleAttribute, params ModuleReference[]? references)
		{
			if (compilation is null)
			{
				throw new ArgumentNullException(nameof(compilation));
			}

			if (enableModuleAttribute is null)
			{
				throw new ArgumentNullException(nameof(enableModuleAttribute));
			}

			if (references is null || references.Length == 0)
			{
				return new ModuleContainer();
			}

			AttributeData[] attributes = GetInstancesOfEnableAttribute(compilation, enableModuleAttribute);

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
		/// Returns a collection of all Durian modules present in the provided array of <paramref name="modules"/> that are enabled for the specified <paramref name="compilation"/>.
		/// </summary>
		/// <param name="compilation"><see cref="Compilation"/> to get the enabled Durian modules of.</param>
		/// <param name="modules">Array of <see cref="DurianModule"/>s to pick the enabled modules from.</param>
		/// <returns>A new instance of <see cref="ModuleContainer"/> that contains the enabled Durian modules.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="compilation"/> is <see langword="null"/>.</exception>
		/// <exception cref="InvalidOperationException">Unknown <see cref="DurianModule"/> value detected. -or- <see cref="DurianModule.None"/> is not a valid Durian module. -or- Error while resolving the <see cref="EnableModuleAttribute"/> class.</exception>
		public static ModuleContainer GetEnabledModules(this Compilation compilation, params DurianModule[]? modules)
		{
			if (compilation is null)
			{
				throw new ArgumentNullException(nameof(compilation));
			}

			if (modules is null || modules.Length == 0)
			{
				return new ModuleContainer();
			}

			INamedTypeSymbol symbol = GetEnableAttributeSymbol(compilation);
			return compilation.GetEnabledModules(symbol, modules);
		}

		/// <summary>
		/// Returns a collection of all Durian modules present in the provided array of <paramref name="modules"/> that are enabled for the specified <paramref name="compilation"/>.
		/// </summary>
		/// <param name="compilation"><see cref="Compilation"/> to get the enabled Durian modules of.</param>
		/// <param name="enableModuleAttribute"><see cref="INamedTypeSymbol"/> that represents the <see cref="EnableModuleAttribute"/> class.</param>
		/// <param name="modules">Array of <see cref="DurianModule"/>s to pick the enabled modules from.</param>
		/// <returns>A new instance of <see cref="ModuleContainer"/> that contains the enabled Durian modules.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="compilation"/> is <see langword="null"/>. -or- <paramref name="enableModuleAttribute"/> is <see langword="null"/>.</exception>
		/// <exception cref="InvalidOperationException">Unknown <see cref="DurianModule"/> value detected. -or- <see cref="DurianModule.None"/> is not a valid Durian module.</exception>
		public static ModuleContainer GetEnabledModules(this Compilation compilation, INamedTypeSymbol enableModuleAttribute, params DurianModule[]? modules)
		{
			if (compilation is null)
			{
				throw new ArgumentNullException(nameof(compilation));
			}

			if (enableModuleAttribute is null)
			{
				throw new ArgumentNullException(nameof(enableModuleAttribute));
			}

			if (modules is null || modules.Length == 0)
			{
				return new ModuleContainer();
			}

			AttributeData[] attributes = GetInstancesOfEnableAttribute(compilation, enableModuleAttribute);

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
		/// Returns a collection of all Durian modules that are enabled for the specified <paramref name="compilation"/>.
		/// </summary>
		/// <param name="compilation"><see cref="Compilation"/> to get all the enabled Durian modules of.</param>
		/// <returns>A new instance of <see cref="ModuleContainer"/> that contains the enabled Durian modules.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="compilation"/> is <see langword="null"/>.</exception>
		/// <exception cref="InvalidOperationException">Error while resolving the <see cref="EnableModuleAttribute"/> class.</exception>
		public static ModuleContainer GetEnabledModules(this Compilation compilation)
		{
			if (compilation is null)
			{
				throw new ArgumentNullException(nameof(compilation));
			}

			INamedTypeSymbol symbol = GetEnableAttributeSymbol(compilation);
			ModuleContainer all = ModuleIdentity.GetAllModules();

			return compilation.GetEnabledModules(symbol, all.AsEnums());
		}

		/// <summary>
		/// Returns a collection of all Durian modules that are enabled for the specified <paramref name="compilation"/>.
		/// </summary>
		/// <param name="compilation"><see cref="Compilation"/> to get all the enabled Durian modules of.</param>
		/// <param name="enableModuleAttribute"><see cref="INamedTypeSymbol"/> that represents the <see cref="EnableModuleAttribute"/> class.</param>
		/// <returns>A new instance of <see cref="ModuleContainer"/> that contains the enabled Durian modules.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="compilation"/> is <see langword="null"/>. -or- <paramref name="enableModuleAttribute"/> is <see langword="null"/>.</exception>
		public static ModuleContainer GetEnabledModules(this Compilation compilation, INamedTypeSymbol enableModuleAttribute)
		{
			if (compilation is null)
			{
				throw new ArgumentNullException(nameof(compilation));
			}

			if (enableModuleAttribute is null)
			{
				throw new ArgumentNullException(nameof(enableModuleAttribute));
			}

			ModuleContainer all = ModuleIdentity.GetAllModules();

			return compilation.GetEnabledModules(enableModuleAttribute, all.AsEnums());
		}

		/// <summary>
		/// Returns a collection of all Durian modules present in the provided collection of <paramref name="modules"/> that are enabled for the specified <paramref name="compilation"/>.
		/// </summary>
		/// <param name="compilation"><see cref="Compilation"/> to get the enabled Durian modules from.</param>
		/// <param name="modules"><see cref="ModuleContainer"/> that provides a collection of Durian modules to pick from.</param>
		/// <returns>A new instance of <see cref="ModuleContainer"/> that contains the enabled Durian modules.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="compilation"/> is <see langword="null"/>. -or- <paramref name="modules"/> is <see langword="null"/>.</exception>
		/// <exception cref="InvalidOperationException">Error while resolving the <see cref="EnableModuleAttribute"/> class.</exception>
		public static ModuleContainer GetEnabledModules(this Compilation compilation, ModuleContainer modules)
		{
			if (compilation is null)
			{
				throw new ArgumentNullException(nameof(compilation));
			}

			if (modules is null)
			{
				throw new ArgumentNullException(nameof(modules));
			}

			if (modules.Count == 0)
			{
				return new ModuleContainer();
			}

			INamedTypeSymbol symbol = GetEnableAttributeSymbol(compilation);

			return compilation.GetEnabledModules(symbol, modules.AsEnums());
		}

		/// <summary>
		/// Returns a collection of all Durian modules present in the provided collection of <paramref name="modules"/> that are enabled for the specified <paramref name="compilation"/>.
		/// </summary>
		/// <param name="compilation"><see cref="Compilation"/> to get the enabled Durian modules from.</param>
		/// <param name="enableModuleAttribute"><see cref="INamedTypeSymbol"/> that represents the <see cref="EnableModuleAttribute"/> class.</param>
		/// <param name="modules"><see cref="ModuleContainer"/> that provides a collection of Durian modules to pick from.</param>
		/// <returns>A new instance of <see cref="ModuleContainer"/> that contains the enabled Durian modules.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="compilation"/> is <see langword="null"/>. -or- <paramref name="enableModuleAttribute"/> is <see langword="null"/>. -or- <paramref name="modules"/> is <see langword="null"/>.</exception>
		public static ModuleContainer GetEnabledModules(this Compilation compilation, INamedTypeSymbol enableModuleAttribute, ModuleContainer modules)
		{
			if (compilation is null)
			{
				throw new ArgumentNullException(nameof(compilation));
			}

			if (enableModuleAttribute is null)
			{
				throw new ArgumentNullException(nameof(enableModuleAttribute));
			}

			if (modules is null)
			{
				throw new ArgumentNullException(nameof(modules));
			}

			if (modules.Count == 0)
			{
				return new ModuleContainer();
			}

			return compilation.GetEnabledModules(enableModuleAttribute, modules.AsEnums());
		}

		/// <summary>
		/// Returns a collection of all Durian modules present in the provided array of <paramref name="modules"/> that are enabled for the specified <paramref name="compilation"/>.
		/// </summary>
		/// <param name="compilation"><see cref="Compilation"/> to get the enabled Durian modules of.</param>
		/// <param name="modules">Array of <see cref="ModuleIdentity"/>s to pick from.</param>
		/// <returns>A new instance of <see cref="ModuleContainer"/> that contains the enabled Durian modules.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="compilation"/> is <see langword="null"/>.</exception>
		/// <exception cref="InvalidOperationException">Error while resolving the <see cref="EnableModuleAttribute"/> class.</exception>
		public static ModuleContainer GetEnabledModules(this Compilation compilation, params ModuleIdentity[]? modules)
		{
			if (compilation is null)
			{
				throw new ArgumentNullException(nameof(compilation));
			}

			if (modules is null || modules.Length == 0)
			{
				return new ModuleContainer();
			}

			INamedTypeSymbol symbol = GetEnableAttributeSymbol(compilation);
			return compilation.GetEnabledModules(symbol, modules);
		}

		/// <summary>
		/// Returns a collection of all Durian modules present in the provided array of <paramref name="modules"/> that are enabled for the specified <paramref name="compilation"/>.
		/// </summary>
		/// <param name="compilation"><see cref="Compilation"/> to get the enabled Durian modules of.</param>
		/// <param name="enableModuleAttribute"><see cref="INamedTypeSymbol"/> that represents the <see cref="EnableModuleAttribute"/> class.</param>
		/// <param name="modules">Array of <see cref="ModuleIdentity"/>s to pick from.</param>
		/// <returns>A new instance of <see cref="ModuleContainer"/> that contains the enabled Durian modules.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="compilation"/> is <see langword="null"/>. -or- <paramref name="enableModuleAttribute"/> is <see langword="null"/>.</exception>
		public static ModuleContainer GetEnabledModules(this Compilation compilation, INamedTypeSymbol enableModuleAttribute, params ModuleIdentity[]? modules)
		{
			if (compilation is null)
			{
				throw new ArgumentNullException(nameof(compilation));
			}

			if (enableModuleAttribute is null)
			{
				throw new ArgumentNullException(nameof(enableModuleAttribute));
			}

			if (modules is null || modules.Length == 0)
			{
				return new ModuleContainer();
			}

			AttributeData[] attributes = GetInstancesOfEnableAttribute(compilation, enableModuleAttribute);

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
		/// Converts the specified <paramref name="symbol"/> into a <see cref="TypeIdentity"/>.
		/// </summary>
		/// <param name="symbol"><see cref="INamedTypeSymbol"/> to convert into a <see cref="TypeIdentity"/>.</param>
		/// <exception cref="ArgumentNullException"><paramref name="symbol"/> is <see langword="null"/>.</exception>
		/// <exception cref="InvalidOperationException"><paramref name="symbol"/> is not a Durian type.</exception>
		public static TypeIdentity GetIdentity(this INamedTypeSymbol symbol)
		{
			if (symbol is null)
			{
				throw new ArgumentNullException(nameof(symbol));
			}

			if (!symbol.TryGetIdentity(out TypeIdentity? identity))
			{
				throw new InvalidOperationException($"Type '{symbol}' is not a Durian type!");
			}

			return identity;
		}

		/// <summary>
		/// Returns an array of <see cref="ModuleReference"/>s to all Durian modules the specified <paramref name="symbol"/> is part of.
		/// </summary>
		/// <param name="symbol"><see cref="INamedTypeSymbol"/> to get all the modules it is part of.</param>
		/// <exception cref="ArgumentNullException"><paramref name="symbol"/> is <see langword="null"/>.</exception>
		/// <exception cref="InvalidOperationException"><paramref name="symbol"/> is not part of any Durian module.</exception>
		public static ModuleReference[] GetModules(this INamedTypeSymbol symbol)
		{
			if (!symbol.TryGetModules(out ModuleReference[]? modules))
			{
				throw new InvalidOperationException($"Type '{symbol}' is not a part of any Durian module!");
			}

			return modules;
		}

		/// <summary>
		/// Determines whether the specified <paramref name="module"/> is enabled for the given <paramref name="compilation"/>.
		/// </summary>
		/// <param name="compilation"><see cref="Compilation"/> to check if the <paramref name="module"/> is enabled for.</param>
		/// <param name="module"><see cref="ModuleReference"/> of Durian module to check for.</param>
		/// <exception cref="ArgumentNullException"><paramref name="module"/> is <see langword="null"/>. -or- <paramref name="compilation"/> is <see langword="null"/>.</exception>
		/// <exception cref="InvalidOperationException">Error while resolving the <see cref="EnableModuleAttribute"/> class.</exception>
		public static bool IsEnabled(this Compilation compilation, ModuleReference module)
		{
			if (compilation is null)
			{
				throw new ArgumentNullException(nameof(compilation));
			}

			if (module is null)
			{
				throw new ArgumentNullException(nameof(module));
			}

			return IsEnabled_Internal(compilation, module.EnumValue);
		}

		/// <summary>
		/// Determines whether the specified <paramref name="module"/> is enabled for the given <paramref name="compilation"/>.
		/// </summary>
		/// <param name="compilation"><see cref="Compilation"/> to check if the <paramref name="module"/> is enabled for.</param>
		/// <param name="module"><see cref="ModuleReference"/> of Durian module to check for.</param>
		/// <param name="enableModuleAttribute"><see cref="INamedTypeSymbol"/> that represents the <see cref="EnableModuleAttribute"/> class.</param>
		/// <exception cref="ArgumentNullException"><paramref name="module"/> is <see langword="null"/>. -or- <paramref name="compilation"/> is <see langword="null"/>. -or- <paramref name="enableModuleAttribute"/> is <see langword="null"/>.</exception>
		public static bool IsEnabled(this Compilation compilation, ModuleReference module, INamedTypeSymbol enableModuleAttribute)
		{
			if (compilation is null)
			{
				throw new ArgumentNullException(nameof(compilation));
			}

			if (module is null)
			{
				throw new ArgumentNullException(nameof(module));
			}

			if (enableModuleAttribute is null)
			{
				throw new ArgumentNullException(nameof(enableModuleAttribute));
			}

			return IsEnabled_Internal(compilation, module.EnumValue, enableModuleAttribute);
		}

		/// <summary>
		/// Determines whether the specified <paramref name="module"/> is enabled for the given <paramref name="compilation"/>.
		/// </summary>
		/// <param name="compilation"><see cref="Compilation"/> to check if the <paramref name="module"/> is enabled for.</param>
		/// <param name="module"><see cref="ModuleIdentity"/> representing a Durian module to check for.</param>
		/// <exception cref="ArgumentNullException"><paramref name="module"/> is <see langword="null"/>. -or- <paramref name="compilation"/> is <see langword="null"/>.</exception>
		/// <exception cref="InvalidOperationException">Error while resolving the <see cref="EnableModuleAttribute"/> class.</exception>
		public static bool IsEnabled(this Compilation compilation, ModuleIdentity module)
		{
			if (compilation is null)
			{
				throw new ArgumentNullException(nameof(compilation));
			}

			if (module is null)
			{
				throw new ArgumentNullException(nameof(module));
			}

			return IsEnabled_Internal(compilation, module.Module);
		}

		/// <summary>
		/// Determines whether the specified <paramref name="module"/> is enabled for the given <paramref name="compilation"/>.
		/// </summary>
		/// <param name="compilation"><see cref="Compilation"/> to check if the <paramref name="module"/> is enabled for.</param>
		/// <param name="module"><see cref="ModuleIdentity"/> representing a Durian module to check for.</param>
		/// <param name="enableModuleAttribute"><see cref="INamedTypeSymbol"/> that represents the <see cref="EnableModuleAttribute"/> class.</param>
		/// <exception cref="ArgumentNullException"><paramref name="module"/> is <see langword="null"/>. -or- <paramref name="compilation"/> is <see langword="null"/>. -or- <paramref name="enableModuleAttribute"/> is <see langword="null"/>.</exception>
		public static bool IsEnabled(this Compilation compilation, ModuleIdentity module, INamedTypeSymbol enableModuleAttribute)
		{
			if (compilation is null)
			{
				throw new ArgumentNullException(nameof(compilation));
			}

			if (module is null)
			{
				throw new ArgumentNullException(nameof(module));
			}

			if (enableModuleAttribute is null)
			{
				throw new ArgumentNullException(nameof(enableModuleAttribute));
			}

			return IsEnabled_Internal(compilation, module.Module, enableModuleAttribute);
		}

		/// <summary>
		/// Determines whether the specified <paramref name="module"/> is enabled for the given <paramref name="compilation"/>.
		/// </summary>
		/// <param name="compilation"><see cref="Compilation"/> to check if the <paramref name="module"/> is enabled for.</param>
		/// <param name="module"><see cref="DurianModule"/> representing a Durian module to check for.</param>
		/// <exception cref="ArgumentNullException"><paramref name="compilation"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentException">Unknown <see cref="DurianModule"/> value detected. -or- <see cref="DurianModule.None"/> is not a valid Durian module. -or- Error while resolving the <see cref="EnableModuleAttribute"/> class.</exception>
		public static bool IsEnabled(this Compilation compilation, DurianModule module)
		{
			if (compilation is null)
			{
				throw new ArgumentNullException(nameof(compilation));
			}

			ModuleIdentity.EnsureIsValidModuleEnum(module);
			return IsEnabled_Internal(compilation, module);
		}

		/// <summary>
		/// Determines whether the specified <paramref name="module"/> is enabled for the given <paramref name="compilation"/>.
		/// </summary>
		/// <param name="compilation"><see cref="Compilation"/> to check if the <paramref name="module"/> is enabled for.</param>
		/// <param name="module"><see cref="DurianModule"/> representing a Durian module to check for.</param>
		/// <param name="enableModuleAttribute"><see cref="INamedTypeSymbol"/> that represents the <see cref="EnableModuleAttribute"/> class.</param>
		/// <exception cref="ArgumentNullException"><paramref name="compilation"/> is <see langword="null"/>. -or- <paramref name="enableModuleAttribute"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentException">Unknown <see cref="DurianModule"/> value detected. -or- <see cref="DurianModule.None"/> is not a valid Durian module.</exception>
		public static bool IsEnabled(this Compilation compilation, DurianModule module, INamedTypeSymbol enableModuleAttribute)
		{
			if (compilation is null)
			{
				throw new ArgumentNullException(nameof(compilation));
			}

			if (enableModuleAttribute is null)
			{
				throw new ArgumentNullException(nameof(enableModuleAttribute));
			}

			ModuleIdentity.EnsureIsValidModuleEnum(module);
			return IsEnabled_Internal(compilation, module, enableModuleAttribute);
		}

		/// <summary>
		/// Determines whether the specified <paramref name="compilation"/> references a Durian module with the given <paramref name="moduleName"/>.
		/// </summary>
		/// <param name="compilation"><see cref="Compilation"/> to check if contains the reference.</param>
		/// <param name="moduleName">Name of the Durian module to check for.</param>
		/// <exception cref="ArgumentNullException"><paramref name="moduleName"/> is <see langword="null"/>. -or- <paramref name="compilation"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentException">Unknown Durian module name: <paramref name="moduleName"/>.</exception>
		/// <exception cref="InvalidOperationException">Error while resolving the <see cref="EnableModuleAttribute"/> class.</exception>
		public static bool IsEnabled(this Compilation compilation, string moduleName)
		{
			if (compilation is null)
			{
				throw new ArgumentNullException(nameof(compilation));
			}

			DurianModule module = ModuleIdentity.ParseModule(moduleName);
			return IsEnabled_Internal(compilation, module);
		}

		/// <summary>
		/// Determines whether the specified <paramref name="compilation"/> references a Durian module with the given <paramref name="moduleName"/>.
		/// </summary>
		/// <param name="compilation"><see cref="Compilation"/> to check if contains the reference.</param>
		/// <param name="moduleName">Name of the Durian module to check for.</param>
		/// <param name="enableModuleAttribute"><see cref="INamedTypeSymbol"/> that represents the <see cref="EnableModuleAttribute"/> class.</param>
		/// <exception cref="ArgumentNullException"><paramref name="moduleName"/> is <see langword="null"/>. -or- <paramref name="compilation"/> is <see langword="null"/>. -or- <paramref name="enableModuleAttribute"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentException">Unknown Durian module name: <paramref name="moduleName"/>.</exception>
		public static bool IsEnabled(this Compilation compilation, string moduleName, INamedTypeSymbol enableModuleAttribute)
		{
			if (compilation is null)
			{
				throw new ArgumentNullException(nameof(compilation));
			}

			if (enableModuleAttribute is null)
			{
				throw new ArgumentNullException(nameof(enableModuleAttribute));
			}

			DurianModule module = ModuleIdentity.ParseModule(moduleName);
			return IsEnabled_Internal(compilation, module, enableModuleAttribute);
		}

		/// <summary>
		/// Determines whether the specified <paramref name="type"/> is enabled for the given <paramref name="compilation"/>.
		/// </summary>
		/// <param name="compilation"><see cref="Compilation"/> to check if the <paramref name="type"/> is enabled for.</param>
		/// <param name="type"><see cref="INamedTypeSymbol"/> to check if is enabled.</param>
		/// <exception cref="ArgumentNullException"><paramref name="compilation"/> is <see langword="null"/>. -or- <paramref name="type"/> is <see langword="null"/>.</exception>
		/// <exception cref="InvalidOperationException">Error while resolving the <see cref="EnableModuleAttribute"/> class. -or- <paramref name="type"/> is not a Durian type.</exception>
		public static bool IsEnabled(this Compilation compilation, INamedTypeSymbol type)
		{
			if (compilation is null)
			{
				throw new ArgumentNullException(nameof(compilation));
			}

			if (type is null)
			{
				throw new ArgumentNullException(nameof(type));
			}

			TypeIdentity identity = type.GetIdentity();

			DurianModule[] all = ModuleIdentity.GetAllModules().AsEnums();
			return IsEnabled_Internal(compilation, identity, all);
		}

		/// <summary>
		/// Determines whether the specified <paramref name="type"/> is enabled for the given <paramref name="compilation"/>.
		/// </summary>
		/// <param name="compilation"><see cref="Compilation"/> to check if the <paramref name="type"/> is enabled for.</param>
		/// <param name="type"><see cref="INamedTypeSymbol"/> to check if is enabled.</param>
		/// <param name="enableModuleAttribute"><see cref="INamedTypeSymbol"/> that represents the <see cref="EnableModuleAttribute"/> class.</param>
		/// <exception cref="ArgumentNullException"><paramref name="compilation"/> is <see langword="null"/>. -or- <paramref name="type"/> is <see langword="null"/>. -or- <paramref name="enableModuleAttribute"/> is <see langword="null"/>.</exception>
		/// <exception cref="InvalidOperationException"><paramref name="type"/> is not a Durian type.</exception>
		public static bool IsEnabled(this Compilation compilation, INamedTypeSymbol type, INamedTypeSymbol enableModuleAttribute)
		{
			if (compilation is null)
			{
				throw new ArgumentNullException(nameof(compilation));
			}

			if (type is null)
			{
				throw new ArgumentNullException(nameof(type));
			}

			if (enableModuleAttribute is null)
			{
				throw new ArgumentNullException(nameof(enableModuleAttribute));
			}

			TypeIdentity identity = type.GetIdentity();

			DurianModule[] all = ModuleIdentity.GetAllModules().AsEnums();
			return IsEnabled_Internal(compilation, identity, all, enableModuleAttribute);
		}

		/// <summary>
		/// Determines whether the specified <paramref name="type"/> is enabled for the given <paramref name="compilation"/>.
		/// </summary>
		/// <param name="compilation"><see cref="Compilation"/> to check if the <paramref name="type"/> is enabled for.</param>
		/// <param name="type"><see cref="INamedTypeSymbol"/> to check if is enabled.</param>
		/// <param name="modules"><see cref="ModuleContainer"/> that provides a collection of Durian modules to pick the <see cref="INamedTypeSymbol"/>s from.</param>
		/// <exception cref="ArgumentNullException"><paramref name="compilation"/> is <see langword="null"/>. -or- <paramref name="type"/> is <see langword="null"/>. -or- <paramref name="modules"/> is <see langword="null"/>.</exception>
		/// <exception cref="InvalidOperationException">Error while resolving the <see cref="EnableModuleAttribute"/> class. -or- <paramref name="type"/> is not a Durian type.</exception>
		public static bool IsEnabled(this Compilation compilation, INamedTypeSymbol type, ModuleContainer modules)
		{
			if (compilation is null)
			{
				throw new ArgumentNullException(nameof(compilation));
			}

			if (type is null)
			{
				throw new ArgumentNullException(nameof(type));
			}

			if (modules is null)
			{
				throw new ArgumentNullException(nameof(modules));
			}

			if (modules.Count == 0)
			{
				return false;
			}

			TypeIdentity identity = type.GetIdentity();

			DurianModule[] array = modules.AsEnums();
			return IsEnabled_Internal(compilation, identity, array);
		}

		/// <summary>
		/// Determines whether the specified <paramref name="type"/> is enabled for the given <paramref name="compilation"/>.
		/// </summary>
		/// <param name="compilation"><see cref="Compilation"/> to check if the <paramref name="type"/> is enabled for.</param>
		/// <param name="type"><see cref="INamedTypeSymbol"/> to check if is enabled.</param>
		/// <param name="enableModuleAttribute"><see cref="INamedTypeSymbol"/> that represents the <see cref="EnableModuleAttribute"/> class.</param>
		/// <param name="modules"><see cref="ModuleContainer"/> that provides a collection of Durian modules to pick the <see cref="INamedTypeSymbol"/>s from.</param>
		/// <exception cref="ArgumentNullException"><paramref name="compilation"/> is <see langword="null"/>. -or- <paramref name="type"/> is <see langword="null"/>. -or- <paramref name="enableModuleAttribute"/> is <see langword="null"/>. -or- <paramref name="modules"/> is <see langword="null"/>.</exception>
		/// <exception cref="InvalidOperationException"><paramref name="type"/> is not a Durian type.</exception>
		public static bool IsEnabled(this Compilation compilation, INamedTypeSymbol type, INamedTypeSymbol enableModuleAttribute, ModuleContainer modules)
		{
			if (compilation is null)
			{
				throw new ArgumentNullException(nameof(compilation));
			}

			if (type is null)
			{
				throw new ArgumentNullException(nameof(type));
			}

			if (enableModuleAttribute is null)
			{
				throw new ArgumentNullException(nameof(enableModuleAttribute));
			}

			if (modules is null)
			{
				throw new ArgumentNullException(nameof(modules));
			}

			if (modules.Count == 0)
			{
				return false;
			}

			TypeIdentity identity = type.GetIdentity();

			DurianModule[] array = modules.AsEnums();
			return IsEnabled_Internal(compilation, identity, array, enableModuleAttribute);
		}

		/// <summary>
		/// Determines whether the specified <paramref name="type"/> is enabled for the given <paramref name="compilation"/>.
		/// </summary>
		/// <param name="compilation"><see cref="Compilation"/> to check if the <paramref name="type"/> is enabled for.</param>
		/// <param name="type"><see cref="INamedTypeSymbol"/> to check if is enabled.</param>
		/// <param name="modules">Array of <see cref="DurianModule"/>s to pick the <see cref="INamedTypeSymbol"/>s from.</param>
		/// <exception cref="ArgumentNullException"><paramref name="compilation"/> is <see langword="null"/>. -or- <paramref name="type"/> is <see langword="null"/>.</exception>
		/// <exception cref="InvalidOperationException">Unknown <see cref="DurianModule"/> value detected. -or- <see cref="DurianModule.None"/> is not a valid Durian module. -or-
		/// Error while resolving the <see cref="EnableModuleAttribute"/> class. -or- <paramref name="type"/> is not a Durian type.</exception>
		public static bool IsEnabled(this Compilation compilation, INamedTypeSymbol type, [NotNullWhen(true)] params DurianModule[]? modules)
		{
			if (compilation is null)
			{
				throw new ArgumentNullException(nameof(compilation));
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
				ModuleIdentity.EnsureIsValidModuleEnum_InvOp(module);
			}

			TypeIdentity identity = type.GetIdentity();
			return IsEnabled_Internal(compilation, identity, modules);
		}

		/// <summary>
		/// Determines whether the specified <paramref name="type"/> is enabled for the given <paramref name="compilation"/>.
		/// </summary>
		/// <param name="compilation"><see cref="Compilation"/> to check if the <paramref name="type"/> is enabled for.</param>
		/// <param name="type"><see cref="INamedTypeSymbol"/> to check if is enabled.</param>
		/// <param name="enableModuleAttribute"><see cref="INamedTypeSymbol"/> that represents the <see cref="EnableModuleAttribute"/> class.</param>
		/// <param name="modules">Array of <see cref="DurianModule"/>s to pick the <see cref="INamedTypeSymbol"/>s from.</param>
		/// <exception cref="ArgumentNullException"><paramref name="compilation"/> is <see langword="null"/>. -or- <paramref name="type"/> is <see langword="null"/>. -or- <paramref name="enableModuleAttribute"/> is <see langword="null"/>.</exception>
		/// <exception cref="InvalidOperationException">Unknown <see cref="DurianModule"/> value detected. -or- <see cref="DurianModule.None"/> is not a valid Durian module. -or-
		///<paramref name="type"/> is not a Durian type.</exception>
		public static bool IsEnabled(this Compilation compilation, INamedTypeSymbol type, INamedTypeSymbol enableModuleAttribute, [NotNullWhen(true)] params DurianModule[]? modules)
		{
			if (compilation is null)
			{
				throw new ArgumentNullException(nameof(compilation));
			}

			if (type is null)
			{
				throw new ArgumentNullException(nameof(type));
			}

			if (enableModuleAttribute is null)
			{
				throw new ArgumentNullException(nameof(enableModuleAttribute));
			}

			if (modules is null || modules.Length == 0)
			{
				return false;
			}

			foreach (DurianModule module in modules)
			{
				ModuleIdentity.EnsureIsValidModuleEnum_InvOp(module);
			}

			TypeIdentity identity = type.GetIdentity();
			return IsEnabled_Internal(compilation, identity, modules, enableModuleAttribute);
		}

		/// <summary>
		/// Determines whether the specified <paramref name="type"/> is enabled for the given <paramref name="compilation"/>.
		/// </summary>
		/// <param name="compilation"><see cref="Compilation"/> to check if the <paramref name="type"/> is enabled for.</param>
		/// <param name="type"><see cref="INamedTypeSymbol"/> to check if is enabled.</param>
		/// <param name="modules">Array of <see cref="ModuleIdentity"/>s to pick the <see cref="INamedTypeSymbol"/>s from.</param>
		/// <exception cref="ArgumentNullException"><paramref name="compilation"/> is <see langword="null"/>. -or- <paramref name="type"/> is <see langword="null"/>.</exception>
		/// <exception cref="InvalidOperationException">Error while resolving the <see cref="EnableModuleAttribute"/> class. -or- <paramref name="type"/> is not a Durian type.</exception>
		public static bool IsEnabled(this Compilation compilation, INamedTypeSymbol type, [NotNullWhen(true)] params ModuleIdentity[]? modules)
		{
			if (compilation is null)
			{
				throw new ArgumentNullException(nameof(compilation));
			}

			if (type is null)
			{
				throw new ArgumentNullException(nameof(type));
			}

			if (modules is null || modules.Length == 0)
			{
				return false;
			}

			TypeIdentity identity = type.GetIdentity();

			DurianModule[] array = ModuleConverter.ToEnums(modules.Where(m => m is not null));
			return IsEnabled_Internal(compilation, identity, array);
		}

		/// <summary>
		/// Determines whether the specified <paramref name="type"/> is enabled for the given <paramref name="compilation"/>.
		/// </summary>
		/// <param name="compilation"><see cref="Compilation"/> to check if the <paramref name="type"/> is enabled for.</param>
		/// <param name="type"><see cref="INamedTypeSymbol"/> to check if is enabled.</param>
		/// <param name="enableModuleAttribute"><see cref="INamedTypeSymbol"/> that represents the <see cref="EnableModuleAttribute"/> class.</param>
		/// <param name="modules">Array of <see cref="ModuleIdentity"/>s to pick the <see cref="INamedTypeSymbol"/>s from.</param>
		/// <exception cref="ArgumentNullException"><paramref name="compilation"/> is <see langword="null"/>. -or- <paramref name="type"/> is <see langword="null"/>. -or- <paramref name="enableModuleAttribute"/> is <see langword="null"/>.</exception>
		/// <exception cref="InvalidOperationException">\<paramref name="type"/> is not a Durian type.</exception>
		public static bool IsEnabled(this Compilation compilation, INamedTypeSymbol type, INamedTypeSymbol enableModuleAttribute, [NotNullWhen(true)] params ModuleIdentity[]? modules)
		{
			if (compilation is null)
			{
				throw new ArgumentNullException(nameof(compilation));
			}

			if (type is null)
			{
				throw new ArgumentNullException(nameof(type));
			}

			if (enableModuleAttribute is null)
			{
				throw new ArgumentNullException(nameof(enableModuleAttribute));
			}

			if (modules is null || modules.Length == 0)
			{
				return false;
			}

			TypeIdentity identity = type.GetIdentity();

			DurianModule[] array = ModuleConverter.ToEnums(modules.Where(m => m is not null));
			return IsEnabled_Internal(compilation, identity, array, enableModuleAttribute);
		}

		/// <summary>
		/// Determines whether the specified <paramref name="type"/> is enabled for the given <paramref name="compilation"/>.
		/// </summary>
		/// <param name="compilation"><see cref="Compilation"/> to check if the <paramref name="type"/> is enabled for.</param>
		/// <param name="type"><see cref="INamedTypeSymbol"/> to check if is enabled.</param>
		/// <param name="references">Array of <see cref="ModuleReference"/>s to pick the <see cref="INamedTypeSymbol"/>s from.</param>
		/// <exception cref="ArgumentNullException"><paramref name="compilation"/> is <see langword="null"/>. -or- <paramref name="type"/> is <see langword="null"/>.</exception>
		/// <exception cref="InvalidOperationException">Error while resolving the <see cref="EnableModuleAttribute"/> class. -or- <paramref name="type"/> is not a Durian type.</exception>
		public static bool IsEnabled(this Compilation compilation, INamedTypeSymbol type, [NotNullWhen(true)] params ModuleReference[]? references)
		{
			if (compilation is null)
			{
				throw new ArgumentNullException(nameof(compilation));
			}

			if (type is null)
			{
				throw new ArgumentNullException(nameof(type));
			}

			if (references is null || references.Length == 0)
			{
				return false;
			}

			TypeIdentity identity = type.GetIdentity();

			DurianModule[] array = ModuleConverter.ToEnums(references.Where(m => m is not null));
			return IsEnabled_Internal(compilation, identity, array);
		}

		/// <summary>
		/// Determines whether the specified <paramref name="type"/> is enabled for the given <paramref name="compilation"/>.
		/// </summary>
		/// <param name="compilation"><see cref="Compilation"/> to check if the <paramref name="type"/> is enabled for.</param>
		/// <param name="type"><see cref="INamedTypeSymbol"/> to check if is enabled.</param>
		/// <param name="enableModuleAttribute"><see cref="INamedTypeSymbol"/> that represents the <see cref="EnableModuleAttribute"/> class.</param>
		/// <param name="references">Array of <see cref="ModuleReference"/>s to pick the <see cref="INamedTypeSymbol"/>s from.</param>
		/// <exception cref="ArgumentNullException"><paramref name="compilation"/> is <see langword="null"/>. -or- <paramref name="type"/> is <see langword="null"/>. -or- <paramref name="enableModuleAttribute"/> is <see langword="null"/>.</exception>
		/// <exception cref="InvalidOperationException"><paramref name="type"/> is not a Durian type.</exception>
		public static bool IsEnabled(this Compilation compilation, INamedTypeSymbol type, INamedTypeSymbol enableModuleAttribute, [NotNullWhen(true)] params ModuleReference[]? references)
		{
			if (compilation is null)
			{
				throw new ArgumentNullException(nameof(compilation));
			}

			if (type is null)
			{
				throw new ArgumentNullException(nameof(type));
			}

			if (enableModuleAttribute is null)
			{
				throw new ArgumentNullException(nameof(enableModuleAttribute));
			}

			if (references is null || references.Length == 0)
			{
				return false;
			}

			TypeIdentity identity = type.GetIdentity();

			DurianModule[] array = ModuleConverter.ToEnums(references.Where(m => m is not null));
			return IsEnabled_Internal(compilation, identity, array, enableModuleAttribute);
		}

		/// <summary>
		/// Determines whether the specified <paramref name="type"/> is enabled for the given <paramref name="compilation"/>.
		/// </summary>
		/// <param name="compilation"><see cref="Compilation"/> to check if the <paramref name="type"/> is enabled for.</param>
		/// <param name="type"><see cref="TypeIdentity"/> to check if is enabled.</param>
		/// <exception cref="ArgumentNullException"><paramref name="compilation"/> is <see langword="null"/>. -or- <paramref name="type"/> is <see langword="null"/>.</exception>
		/// <exception cref="InvalidOperationException">Error while resolving the <see cref="EnableModuleAttribute"/> class.</exception>
		public static bool IsEnabled(this Compilation compilation, TypeIdentity type)
		{
			if (compilation is null)
			{
				throw new ArgumentNullException(nameof(compilation));
			}

			if (type is null)
			{
				throw new ArgumentNullException(nameof(type));
			}

			DurianModule[] all = ModuleIdentity.GetAllModules().AsEnums();
			return IsEnabled_Internal(compilation, type, all);
		}

		/// <summary>
		/// Determines whether the specified <paramref name="type"/> is enabled for the given <paramref name="compilation"/>.
		/// </summary>
		/// <param name="compilation"><see cref="Compilation"/> to check if the <paramref name="type"/> is enabled for.</param>
		/// <param name="type"><see cref="TypeIdentity"/> to check if is enabled.</param>
		/// <param name="enableModuleAttribute"><see cref="INamedTypeSymbol"/> that represents the <see cref="EnableModuleAttribute"/> class.</param>
		/// <exception cref="ArgumentNullException"><paramref name="compilation"/> is <see langword="null"/>. -or- <paramref name="type"/> is <see langword="null"/>. -or- <paramref name="enableModuleAttribute"/> is <see langword="null"/>.</exception>
		public static bool IsEnabled(this Compilation compilation, TypeIdentity type, INamedTypeSymbol enableModuleAttribute)
		{
			if (compilation is null)
			{
				throw new ArgumentNullException(nameof(compilation));
			}

			if (type is null)
			{
				throw new ArgumentNullException(nameof(type));
			}

			if (enableModuleAttribute is null)
			{
				throw new ArgumentNullException(nameof(enableModuleAttribute));
			}

			DurianModule[] all = ModuleIdentity.GetAllModules().AsEnums();
			return IsEnabled_Internal(compilation, type, all, enableModuleAttribute);
		}

		/// <summary>
		/// Determines whether the specified <paramref name="type"/> is enabled for the given <paramref name="compilation"/>.
		/// </summary>
		/// <param name="compilation"><see cref="Compilation"/> to check if the <paramref name="type"/> is enabled for.</param>
		/// <param name="type"><see cref="TypeIdentity"/> to check if is enabled.</param>
		/// <param name="modules"><see cref="ModuleContainer"/> that provides a collection of Durian modules to pick the <see cref="TypeIdentity"/>s from.</param>
		/// <exception cref="ArgumentNullException"><paramref name="compilation"/> is <see langword="null"/>. -or- <paramref name="type"/> is <see langword="null"/>. -or- <paramref name="modules"/> is <see langword="null"/>.</exception>
		/// <exception cref="InvalidOperationException">Error while resolving the <see cref="EnableModuleAttribute"/> class.</exception>
		public static bool IsEnabled(this Compilation compilation, TypeIdentity type, ModuleContainer modules)
		{
			if (compilation is null)
			{
				throw new ArgumentNullException(nameof(compilation));
			}

			if (type is null)
			{
				throw new ArgumentNullException(nameof(type));
			}

			if (modules is null)
			{
				throw new ArgumentNullException(nameof(modules));
			}

			if (modules.Count == 0)
			{
				return false;
			}

			DurianModule[] array = modules.AsEnums();
			return IsEnabled_Internal(compilation, type, array);
		}

		/// <summary>
		/// Determines whether the specified <paramref name="type"/> is enabled for the given <paramref name="compilation"/>.
		/// </summary>
		/// <param name="compilation"><see cref="Compilation"/> to check if the <paramref name="type"/> is enabled for.</param>
		/// <param name="type"><see cref="TypeIdentity"/> to check if is enabled.</param>
		/// <param name="enableModuleAttribute"><see cref="INamedTypeSymbol"/> that represents the <see cref="EnableModuleAttribute"/> class.</param>
		/// <param name="modules"><see cref="ModuleContainer"/> that provides a collection of Durian modules to pick the <see cref="TypeIdentity"/>s from.</param>
		/// <exception cref="ArgumentNullException"><paramref name="compilation"/> is <see langword="null"/>. -or- <paramref name="type"/> is <see langword="null"/>. -or- <paramref name="enableModuleAttribute"/> is <see langword="null"/>. -or- <paramref name="modules"/> is <see langword="null"/>.</exception>
		public static bool IsEnabled(this Compilation compilation, TypeIdentity type, INamedTypeSymbol enableModuleAttribute, ModuleContainer modules)
		{
			if (compilation is null)
			{
				throw new ArgumentNullException(nameof(compilation));
			}

			if (type is null)
			{
				throw new ArgumentNullException(nameof(type));
			}

			if (enableModuleAttribute is null)
			{
				throw new ArgumentNullException(nameof(enableModuleAttribute));
			}

			if (modules is null)
			{
				throw new ArgumentNullException(nameof(modules));
			}

			if (modules.Count == 0)
			{
				return false;
			}

			DurianModule[] array = modules.AsEnums();
			return IsEnabled_Internal(compilation, type, array, enableModuleAttribute);
		}

		/// <summary>
		/// Determines whether the specified <paramref name="type"/> is enabled for the given <paramref name="compilation"/>.
		/// </summary>
		/// <param name="compilation"><see cref="Compilation"/> to check if the <paramref name="type"/> is enabled for.</param>
		/// <param name="type"><see cref="TypeIdentity"/> to check if is enabled.</param>
		/// <param name="modules">Array of <see cref="DurianModule"/>s to pick the <see cref="TypeIdentity"/>s from.</param>
		/// <exception cref="ArgumentNullException"><paramref name="compilation"/> is <see langword="null"/>. -or- <paramref name="type"/> is <see langword="null"/>.</exception>
		/// <exception cref="InvalidOperationException">Unknown <see cref="DurianModule"/> value detected. -or- <see cref="DurianModule.None"/> is not a valid Durian module. -or-
		/// Error while resolving the <see cref="EnableModuleAttribute"/> class.</exception>
		public static bool IsEnabled(this Compilation compilation, TypeIdentity type, [NotNullWhen(true)] params DurianModule[]? modules)
		{
			if (compilation is null)
			{
				throw new ArgumentNullException(nameof(compilation));
			}

			if (type is null)
			{
				throw new ArgumentNullException(nameof(type));
			}

			if (modules is null || modules.Length == 0)
			{
				return false;
			}

			return IsEnabled_Internal(compilation, type, modules);
		}

		/// <summary>
		/// Determines whether the specified <paramref name="type"/> is enabled for the given <paramref name="compilation"/>.
		/// </summary>
		/// <param name="compilation"><see cref="Compilation"/> to check if the <paramref name="type"/> is enabled for.</param>
		/// <param name="type"><see cref="TypeIdentity"/> to check if is enabled.</param>
		/// <param name="enableModuleAttribute"><see cref="INamedTypeSymbol"/> that represents the <see cref="EnableModuleAttribute"/> class.</param>
		/// <param name="modules">Array of <see cref="DurianModule"/>s to pick the <see cref="TypeIdentity"/>s from.</param>
		/// <exception cref="ArgumentNullException"><paramref name="compilation"/> is <see langword="null"/>. -or- <paramref name="type"/> is <see langword="null"/>. -or- <paramref name="enableModuleAttribute"/> is <see langword="null"/>.</exception>
		/// <exception cref="InvalidOperationException">Unknown <see cref="DurianModule"/> value detected. -or- <see cref="DurianModule.None"/> is not a valid Durian module.</exception>
		public static bool IsEnabled(this Compilation compilation, TypeIdentity type, INamedTypeSymbol enableModuleAttribute, [NotNullWhen(true)] params DurianModule[]? modules)
		{
			if (compilation is null)
			{
				throw new ArgumentNullException(nameof(compilation));
			}

			if (type is null)
			{
				throw new ArgumentNullException(nameof(type));
			}

			if (enableModuleAttribute is null)
			{
				throw new ArgumentNullException(nameof(enableModuleAttribute));
			}

			if (modules is null || modules.Length == 0)
			{
				return false;
			}

			return IsEnabled_Internal(compilation, type, modules, enableModuleAttribute);
		}

		/// <summary>
		/// Determines whether the specified <paramref name="type"/> is enabled for the given <paramref name="compilation"/>.
		/// </summary>
		/// <param name="compilation"><see cref="Compilation"/> to check if the <paramref name="type"/> is enabled for.</param>
		/// <param name="type"><see cref="TypeIdentity"/> to check if is enabled.</param>
		/// <param name="modules">Array of <see cref="ModuleIdentity"/>s to pick the <see cref="TypeIdentity"/>s from.</param>
		/// <exception cref="ArgumentNullException"><paramref name="compilation"/> is <see langword="null"/>. -or- <paramref name="type"/> is <see langword="null"/>.</exception>
		/// <exception cref="InvalidOperationException">Error while resolving the <see cref="EnableModuleAttribute"/> class.</exception>
		public static bool IsEnabled(this Compilation compilation, TypeIdentity type, [NotNullWhen(true)] params ModuleIdentity[]? modules)
		{
			if (compilation is null)
			{
				throw new ArgumentNullException(nameof(compilation));
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
			return IsEnabled_Internal(compilation, type, array);
		}

		/// <summary>
		/// Determines whether the specified <paramref name="type"/> is enabled for the given <paramref name="compilation"/>.
		/// </summary>
		/// <param name="compilation"><see cref="Compilation"/> to check if the <paramref name="type"/> is enabled for.</param>
		/// <param name="type"><see cref="TypeIdentity"/> to check if is enabled.</param>
		/// <param name="enableModuleAttribute"><see cref="INamedTypeSymbol"/> that represents the <see cref="EnableModuleAttribute"/> class.</param>
		/// <param name="modules">Array of <see cref="ModuleIdentity"/>s to pick the <see cref="TypeIdentity"/>s from.</param>
		/// <exception cref="ArgumentNullException"><paramref name="compilation"/> is <see langword="null"/>. -or- <paramref name="type"/> is <see langword="null"/>. -or- <paramref name="enableModuleAttribute"/> is <see langword="null"/>.</exception>
		public static bool IsEnabled(this Compilation compilation, TypeIdentity type, INamedTypeSymbol enableModuleAttribute, [NotNullWhen(true)] params ModuleIdentity[]? modules)
		{
			if (compilation is null)
			{
				throw new ArgumentNullException(nameof(compilation));
			}

			if (type is null)
			{
				throw new ArgumentNullException(nameof(type));
			}

			if (enableModuleAttribute is null)
			{
				throw new ArgumentNullException(nameof(enableModuleAttribute));
			}

			if (modules is null || modules.Length == 0)
			{
				return false;
			}

			DurianModule[] array = ModuleConverter.ToEnums(modules.Where(m => m is not null));
			return IsEnabled_Internal(compilation, type, array, enableModuleAttribute);
		}

		/// <summary>
		/// Determines whether the specified <paramref name="type"/> is enabled for the given <paramref name="compilation"/>.
		/// </summary>
		/// <param name="compilation"><see cref="Compilation"/> to check if the <paramref name="type"/> is enabled for.</param>
		/// <param name="type"><see cref="TypeIdentity"/> to check if is enabled.</param>
		/// <param name="references">Array of <see cref="ModuleReference"/>s to pick the <see cref="TypeIdentity"/>s from.</param>
		/// <exception cref="ArgumentNullException"><paramref name="compilation"/> is <see langword="null"/>. -or- <paramref name="type"/> is <see langword="null"/>.</exception>
		/// <exception cref="InvalidOperationException">Error while resolving the <see cref="EnableModuleAttribute"/> class.</exception>
		public static bool IsEnabled(this Compilation compilation, TypeIdentity type, [NotNullWhen(true)] params ModuleReference[]? references)
		{
			if (compilation is null)
			{
				throw new ArgumentNullException(nameof(compilation));
			}

			if (type is null)
			{
				throw new ArgumentNullException(nameof(type));
			}

			if (references is null || references.Length == 0)
			{
				return false;
			}

			DurianModule[] array = ModuleConverter.ToEnums(references.Where(m => m is not null));
			return IsEnabled_Internal(compilation, type, array);
		}

		/// <summary>
		/// Determines whether the specified <paramref name="type"/> is enabled for the given <paramref name="compilation"/>.
		/// </summary>
		/// <param name="compilation"><see cref="Compilation"/> to check if the <paramref name="type"/> is enabled for.</param>
		/// <param name="type"><see cref="TypeIdentity"/> to check if is enabled.</param>
		/// <param name="enableModuleAttribute"><see cref="INamedTypeSymbol"/> that represents the <see cref="EnableModuleAttribute"/> class.</param>
		/// <param name="references">Array of <see cref="ModuleReference"/>s to pick the <see cref="TypeIdentity"/>s from.</param>
		/// <exception cref="ArgumentNullException"><paramref name="compilation"/> is <see langword="null"/>. -or- <paramref name="type"/> is <see langword="null"/>. -or- <paramref name="enableModuleAttribute"/> is <see langword="null"/>.</exception>
		public static bool IsEnabled(this Compilation compilation, TypeIdentity type, INamedTypeSymbol enableModuleAttribute, [NotNullWhen(true)] params ModuleReference[]? references)
		{
			if (compilation is null)
			{
				throw new ArgumentNullException(nameof(compilation));
			}

			if (type is null)
			{
				throw new ArgumentNullException(nameof(type));
			}

			if (enableModuleAttribute is null)
			{
				throw new ArgumentNullException(nameof(enableModuleAttribute));
			}

			if (references is null || references.Length == 0)
			{
				return false;
			}

			DurianModule[] array = ModuleConverter.ToEnums(references.Where(m => m is not null));
			return IsEnabled_Internal(compilation, type, array, enableModuleAttribute);
		}

		/// <summary>
		/// Determines whether the specified <paramref name="symbol"/> is a part of any Durian module.
		/// </summary>
		/// <param name="symbol"><see cref="INamedTypeSymbol"/> to check if is a part of any Durian module.</param>
		/// <exception cref="ArgumentNullException"><paramref name="symbol"/> is <see langword="null"/>.</exception>
		public static bool IsPartOfAnyModule(this INamedTypeSymbol symbol)
		{
			if (symbol is null)
			{
				throw new ArgumentNullException(nameof(symbol));
			}

			ModuleIdentity[] identities = ModuleIdentity.GetAllModules().AsIdentities();

			return symbol.IsPartOfAnyModule(identities);
		}

		/// <summary>
		/// Determines whether the specified <paramref name="symbol"/> is a part of any of the specified <paramref name="modules"/>.
		/// </summary>
		/// <param name="symbol"><see cref="INamedTypeSymbol"/> to check if is a part of any of the specified <paramref name="modules"/>.</param>
		/// <param name="modules">Array of <see cref="DurianModule"/>s to check.</param>
		/// <exception cref="ArgumentNullException"><paramref name="symbol"/> is <see langword="null"/>.</exception>
		/// <exception cref="InvalidOperationException">Unknown <see cref="DurianModule"/> value detected. -or- <see cref="DurianModule.None"/> is not a valid Durian module.</exception>
		public static bool IsPartOfAnyModule(this INamedTypeSymbol symbol, [NotNullWhen(true)] params DurianModule[]? modules)
		{
			if (symbol is null)
			{
				throw new ArgumentNullException(nameof(symbol));
			}

			if (modules is null || modules.Length == 0)
			{
				return false;
			}

			ModuleIdentity[] identities = ModuleConverter.ToIdentities(modules);

			return symbol.IsPartOfAnyModule(identities);
		}

		/// <summary>
		/// Determines whether the specified <paramref name="symbol"/> is a part of any of the specified modules.
		/// </summary>
		/// <param name="symbol"><see cref="INamedTypeSymbol"/> to check if is a part of any of the specified modules.</param>
		/// <param name="references">Array of <see cref="ModuleReference"/>s to check.</param>
		/// <exception cref="ArgumentNullException"><paramref name="symbol"/> is <see langword="null"/>.</exception>
		public static bool IsPartOfAnyModule(this INamedTypeSymbol symbol, [NotNullWhen(true)] params ModuleReference[]? references)
		{
			if (symbol is null)
			{
				throw new ArgumentNullException(nameof(symbol));
			}

			if (references is null || references.Length == 0)
			{
				return false;
			}

			ModuleIdentity[] identities = ModuleConverter.ToIdentities(references.Where(r => r is not null));

			return symbol.IsPartOfAnyModule(identities);
		}

		/// <summary>
		/// Determines whether the specified <paramref name="symbol"/> is a part of any of the specified <paramref name="modules"/>.
		/// </summary>
		/// <param name="symbol"><see cref="INamedTypeSymbol"/> to check if is a part of any of the specified <paramref name="modules"/>.</param>
		/// <param name="modules">Array of <see cref="ModuleIdentity"/>s to check.</param>
		/// <exception cref="ArgumentNullException"><paramref name="symbol"/> is <see langword="null"/>.</exception>
		public static bool IsPartOfAnyModule(this INamedTypeSymbol symbol, [NotNullWhen(true)] params ModuleIdentity[]? modules)
		{
			if (symbol is null)
			{
				throw new ArgumentNullException(nameof(symbol));
			}

			if (modules is null || modules.Length == 0)
			{
				return false;
			}

			string fullname = symbol.ToString();

			foreach (ModuleIdentity module in modules)
			{
				if (module is null)
				{
					continue;
				}

				foreach (TypeIdentity type in module.Types)
				{
					if (type.FullyQualifiedName == fullname)
					{
						return true;
					}
				}
			}

			return false;
		}

		/// <summary>
		/// Determines whether the specified <paramref name="symbol"/> is a part of any of the specified <paramref name="modules"/>.
		/// </summary>
		/// <param name="symbol"><see cref="INamedTypeSymbol"/> to check if is a part of any of the specified <paramref name="modules"/>.</param>
		/// <param name="modules"><see cref="ModuleContainer"/> that provides <see cref="ModuleIdentity"/>s to check if the <paramref name="symbol"/> is a part of.</param>
		/// <exception cref="ArgumentNullException"><paramref name="symbol"/> is <see langword="null"/>. -or- <paramref name="modules"/> is <see langword="null"/>.</exception>
		public static bool IsPartOfAnyModule(this INamedTypeSymbol symbol, ModuleContainer modules)
		{
			if (symbol is null)
			{
				throw new ArgumentNullException(nameof(symbol));
			}

			if (modules is null)
			{
				throw new ArgumentNullException(nameof(modules));
			}

			ModuleIdentity[] identities = modules.AsIdentities();

			return symbol.IsPartOfAnyModule(identities);
		}

		/// <summary>
		/// Converts the specified <paramref name="identity"/> into a <see cref="INamedTypeSymbol"/>.
		/// </summary>
		/// <param name="compilation"><see cref="Compilation"/> that is used to get the <see cref="INamedTypeSymbol"/>.</param>
		/// <param name="identity"><see cref="TypeIdentity"/> to convert into a <see cref="INamedTypeSymbol"/>.</param>
		/// <exception cref="ArgumentNullException"><paramref name="compilation"/> is <see langword="null"/>. -or- <paramref name="identity"/> is <see langword="null"/>.</exception>
		/// <exception cref="InvalidOperationException">Target type could not be resolved.</exception>
		public static INamedTypeSymbol ToSymbol(this Compilation compilation, TypeIdentity identity)
		{
			if (compilation is null)
			{
				throw new ArgumentNullException(nameof(compilation));
			}

			if (identity is null)
			{
				throw new ArgumentNullException(nameof(identity));
			}

			INamedTypeSymbol? symbol = compilation.GetTypeByMetadataName(identity.FullyQualifiedName);

			if (symbol is null)
			{
				throw new InvalidOperationException($"Type '{identity.FullyQualifiedName}' could not be resolved!");
			}

			return symbol;
		}

		/// <summary>
		/// Attempts to convert the specified <paramref name="symbol"/> into a <see cref="TypeIdentity"/>.
		/// </summary>
		/// <param name="symbol"><see cref="INamedTypeSymbol"/> to convert into a <see cref="TypeIdentity"/>.</param>
		/// <param name="identity"><see cref="TypeIdentity"/> that was created based on the <paramref name="symbol"/>.</param>
		/// <exception cref="ArgumentNullException"><paramref name="symbol"/> is <see langword="null"/>.</exception>
		public static bool TryGetIdentity(this INamedTypeSymbol symbol, [NotNullWhen(true)] out TypeIdentity? identity)
		{
			if (symbol is null)
			{
				throw new ArgumentNullException(nameof(symbol));
			}

			TypeIdentity type = TypeIdentity.GetIdentity(symbol.Name);

			if (type.FullyQualifiedName == symbol.ToString())
			{
				identity = type;
				return true;
			}

			identity = null;
			return false;
		}

		/// <summary>
		/// Attempts to return an array of <see cref="ModuleReference"/>s to all Durian modules the specified <paramref name="symbol"/> is part of.
		/// </summary>
		/// <param name="symbol"><see cref="INamedTypeSymbol"/> to get all the modules it is part of.</param>
		/// <param name="modules">Array of <see cref="ModuleReference"/>s to all Durian modules the specified <paramref name="symbol"/> is part of.</param>
		/// <returns><see langword="true"/> if the <paramref name="symbol"/> is part of any Durian module, <see langword="false"/> otherwise.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="symbol"/> is <see langword="null"/>.</exception>
		public static bool TryGetModules(this INamedTypeSymbol symbol, [NotNullWhen(true)] out ModuleReference[]? modules)
		{
			if (!symbol.TryGetIdentity(out TypeIdentity? identity))
			{
				modules = null;
				return false;
			}

			ImmutableArray<ModuleReference> immutable = identity.Modules;
			modules = new ModuleReference[immutable.Length];
			immutable.CopyTo(modules);

			return true;
		}

		internal static INamedTypeSymbol GetEnableAttributeSymbol(Compilation compilation)
		{
			INamedTypeSymbol? symbol = compilation.GetTypeByMetadataName(typeof(EnableModuleAttribute).ToString());

			if (symbol is null)
			{
				throw new InvalidOperationException($"Error while resolving the {nameof(EnableModuleAttribute)} class!");
			}

			return symbol;
		}

		internal static AttributeData[] GetInstancesOfEnableAttribute(Compilation compilation)
		{
			INamedTypeSymbol symbol = GetEnableAttributeSymbol(compilation);
			return GetInstancesOfEnableAttribute(compilation, symbol);
		}

		internal static AttributeData[] GetInstancesOfEnableAttribute(Compilation compilation, INamedTypeSymbol enableModuleAttribute)
		{
			return compilation.Assembly.GetAttributes(enableModuleAttribute).ToArray();
		}

		internal static bool IsEnabled_Internal(Compilation compilation, DurianModule module)
		{
			INamedTypeSymbol symbol = GetEnableAttributeSymbol(compilation);
			IEnumerable<AttributeData> attributes = compilation.Assembly.GetAttributes(symbol);

			return IsEnabled_Internal(attributes, module);
		}

		internal static bool IsEnabled_Internal(Compilation compilation, DurianModule module, INamedTypeSymbol enableModuleAttribute)
		{
			IEnumerable<AttributeData> attributes = compilation.Assembly.GetAttributes(enableModuleAttribute);

			return IsEnabled_Internal(attributes, module);
		}

		internal static bool IsEnabled_Internal(IEnumerable<AttributeData> attributes, DurianModule module)
		{
			foreach (AttributeData attribute in attributes)
			{
				if (attribute.TryGetConstructorArgumentValue(0, out int value) && (DurianModule)value == module)
				{
					return true;
				}
			}

			return false;
		}

		private static bool IsEnabled_Internal(Compilation compilation, TypeIdentity type, DurianModule[] modules)
		{
			INamedTypeSymbol symbol = GetEnableAttributeSymbol(compilation);
			return IsEnabled_Internal(compilation, type, modules, symbol);
		}

		private static bool IsEnabled_Internal(Compilation compilation, TypeIdentity type, DurianModule[] modules, INamedTypeSymbol enableModuleAttribute)
		{
			AttributeData[] attributes = GetInstancesOfEnableAttribute(compilation, enableModuleAttribute);

			if (attributes.Length == 0)
			{
				return false;
			}

			foreach (ModuleReference reference in type.Modules)
			{
				if (!IsEnabled_Internal(attributes, reference.EnumValue))
				{
					continue;
				}

				foreach (DurianModule m in modules)
				{
					if (reference.EnumValue == m)
					{
						return true;
					}
				}
			}

			return false;
		}
	}
}
