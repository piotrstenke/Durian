// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;

namespace Durian.Info
{
    /// <summary>
    /// Compares the <see cref="ModuleIdentity.Module"/> enum value of two <see cref="ModuleIdentity"/> instances.
    /// </summary>
    public sealed class ModuleEnumEqualityComparer : IEqualityComparer<ModuleIdentity>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ModuleEnumEqualityComparer"/> class.
        /// </summary>
        public ModuleEnumEqualityComparer()
        {
        }

        /// <inheritdoc/>
        public bool Equals(ModuleIdentity? x, ModuleIdentity? y)
        {
            if (x is null)
            {
                return y is null;
            }

            if (y is null)
            {
                return false;
            }

            return x.Module == y.Module;
        }

        /// <inheritdoc/>
        public int GetHashCode(ModuleIdentity obj)
        {
            if (obj is null)
            {
                throw new ArgumentNullException(nameof(obj));
            }

            return obj.Module.GetHashCode();
        }
    }
}