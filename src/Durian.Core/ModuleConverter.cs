// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Durian.Info
{
    /// <summary>
    /// Contains conversion methods for collections of <see cref="PackageReference"/>s, <see cref="PackageIdentity"/>s and <see cref="DurianPackage"/>s.
    /// </summary>
    public static class ModuleConverter
    {
        /// <summary>
        /// Converts a collection of <see cref="ModuleReference"/>s into an array of <see cref="DurianModule"/>s.
        /// </summary>
        /// <param name="references">A collection of <see cref="ModuleReference"/>s to convert.</param>
        /// <exception cref="ArgumentNullException"><paramref name="references"/> is <see langword="null"/>.</exception>
        /// <exception cref="InvalidOperationException">Null <see cref="ModuleReference"/> detected.</exception>
        public static DurianModule[] ToEnums(IEnumerable<ModuleReference> references)
        {
            if (references is null)
            {
                throw new ArgumentNullException(nameof(references));
            }

            ModuleReference[] array = references.ToArray();

            if (array.Length == 0)
            {
                return Array.Empty<DurianModule>();
            }

            DurianModule[] enums = new DurianModule[array.Length];

            for (int i = 0; i < enums.Length; i++)
            {
                ModuleReference? reference = array[i];

                if (reference is null)
                {
                    throw new InvalidOperationException($"Null module reference at index: {i}");
                }

                enums[i] = reference.EnumValue;
            }

            return enums;
        }

        /// <summary>
        /// Converts a collection of <see cref="ModuleIdentity"/>s into an array of <see cref="DurianModule"/>s.
        /// </summary>
        /// <param name="modules">A collection of <see cref="ModuleIdentity"/>s to convert.</param>
        /// <exception cref="ArgumentNullException"><paramref name="modules"/> is <see langword="null"/>.</exception>
        /// <exception cref="InvalidOperationException">Null <see cref="ModuleIdentity"/> detected.</exception>
        public static DurianModule[] ToEnums(IEnumerable<ModuleIdentity> modules)
        {
            if (modules is null)
            {
                throw new ArgumentNullException(nameof(modules));
            }

            ModuleIdentity[] array = modules.ToArray();

            if (array.Length == 0)
            {
                return Array.Empty<DurianModule>();
            }

            DurianModule[] enums = new DurianModule[array.Length];

            for (int i = 0; i < enums.Length; i++)
            {
                ModuleIdentity? identity = array[i];

                if (identity is null)
                {
                    throw new InvalidOperationException($"Null module identity at index: {i}");
                }

                enums[i] = identity.Module;
            }

            return enums;
        }

        /// <summary>
        /// Converts a collection of <see cref="DurianModule"/>s into an array of <see cref="ModuleIdentity"/>s.
        /// </summary>
        /// <param name="modules">A collection of <see cref="DurianModule"/>s to convert.</param>
        /// <exception cref="ArgumentNullException"><paramref name="modules"/> is <see langword="null"/>.</exception>
        /// <exception cref="InvalidOperationException">Unknown <see cref="DurianModule"/> value detected.</exception>
        public static ModuleIdentity[] ToIdentities(IEnumerable<DurianModule> modules)
        {
            if (modules is null)
            {
                throw new ArgumentNullException(nameof(modules));
            }

            DurianModule[] array = modules.ToArray();

            if (array.Length == 0)
            {
                return Array.Empty<ModuleIdentity>();
            }

            ModuleIdentity[] identities = new ModuleIdentity[array.Length];

            for (int i = 0; i < identities.Length; i++)
            {
                identities[i] = ModuleIdentity.GetModule(array[i]);
            }

            return identities;
        }

        /// <summary>
        /// Converts a collection of <see cref="ModuleReference"/>s into an array of <see cref="ModuleIdentity"/>s.
        /// </summary>
        /// <param name="references">A collection of <see cref="ModuleReference"/>s to convert.</param>
        /// <exception cref="ArgumentNullException"><paramref name="references"/> is <see langword="null"/>.</exception>
        /// <exception cref="InvalidOperationException">Null <see cref="ModuleReference"/> detected.</exception>
        public static ModuleIdentity[] ToIdentities(IEnumerable<ModuleReference> references)
        {
            if (references is null)
            {
                throw new ArgumentNullException(nameof(references));
            }

            ModuleReference[] array = references.ToArray();

            if (array.Length == 0)
            {
                return Array.Empty<ModuleIdentity>();
            }

            ModuleIdentity[] identities = new ModuleIdentity[array.Length];

            for (int i = 0; i < identities.Length; i++)
            {
                ModuleReference? reference = array[i];

                if (reference is null)
                {
                    throw new InvalidOperationException($"Null module reference at index: {i}");
                }

                identities[i] = reference.GetModule();
            }

            return identities;
        }

        /// <summary>
        /// Converts a collection of <see cref="ModuleIdentity"/>s into an array of <see cref="ModuleReference"/>s.
        /// </summary>
        /// <param name="modules">A collection of <see cref="ModuleIdentity"/>s to convert.</param>
        /// <exception cref="ArgumentNullException"><paramref name="modules"/> is <see langword="null"/>.</exception>
        /// <exception cref="InvalidOperationException">Null <see cref="ModuleIdentity"/> detected.</exception>
        public static ModuleReference[] ToReferences(IEnumerable<ModuleIdentity> modules)
        {
            if (modules is null)
            {
                throw new ArgumentNullException(nameof(modules));
            }

            ModuleIdentity[] array = modules.ToArray();

            if (array.Length == 0)
            {
                return Array.Empty<ModuleReference>();
            }

            ModuleReference[] references = new ModuleReference[array.Length];

            for (int i = 0; i < references.Length; i++)
            {
                ModuleIdentity? identity = array[i];

                if (identity is null)
                {
                    throw new InvalidOperationException($"Null module identity at index: {i}");
                }

                references[i] = new ModuleReference(identity);
            }

            return references;
        }

        /// <summary>
        /// Converts a collection of <see cref="DurianModule"/>s into an array of <see cref="ModuleReference"/>s.
        /// </summary>
        /// <param name="modules">A collection of <see cref="DurianModule"/>s to convert.</param>
        /// <exception cref="ArgumentNullException"><paramref name="modules"/> is <see langword="null"/>.</exception>
        /// <exception cref="InvalidOperationException">Unknown <see cref="DurianModule"/> value detected.</exception>
        public static ModuleReference[] ToReferences(IEnumerable<DurianModule> modules)
        {
            if (modules is null)
            {
                throw new ArgumentNullException(nameof(modules));
            }

            DurianModule[] array = modules.ToArray();

            if (array.Length == 0)
            {
                return Array.Empty<ModuleReference>();
            }

            ModuleReference[] references = new ModuleReference[array.Length];

            for (int i = 0; i < references.Length; i++)
            {
                references[i] = new ModuleReference(array[i]);
            }

            return references;
        }
    }
}