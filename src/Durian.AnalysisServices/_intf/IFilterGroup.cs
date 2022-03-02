// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;

namespace Durian.Analysis
{
    /// <summary>
    /// A collection of <see cref="ISyntaxFilter"/>s.
    /// </summary>
    public interface IFilterGroup
    {
        /// <summary>
        /// Number of <see cref="ISyntaxFilter"/>s in this <see cref="IFilterGroup"/>.
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Determines whether this <see cref="IFilterGroup"/> is sealed by calling the <see cref="Seal"/> method.
        /// </summary>
        bool IsSealed { get; }

        /// <summary>
        /// Name of this <see cref="IFilterGroup"/>.
        /// </summary>
        /// <exception cref="ArgumentException"><see cref="Name"/> cannot be empty or white space only.</exception>
        string? Name { get; set; }

        /// <summary>
        /// Adds the specified <paramref name="filter"/> to this <see cref="IFilterGroup"/>.
        /// </summary>
        /// <param name="filter"><see cref="ISyntaxFilter"/> to add to the <see cref="IFilterGroup"/>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="filter"/> is <see langword="null"/>.</exception>
        /// <exception cref="InvalidOperationException"><see cref="FilterGroup{TFilter}"/> is sealed.</exception>
        void AddFilter(ISyntaxFilter filter);

        /// <summary>
        /// Adds the collection of <paramref name="filters"/> to this <see cref="IFilterGroup"/>.
        /// </summary>
        /// <param name="filters">A collection of <see cref="ISyntaxFilter"/>s to add to the <see cref="IFilterGroup"/>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="filters"/> is <see langword="null"/>.</exception>
        /// <exception cref="InvalidOperationException"><see cref="IFilterGroup"/> is sealed.</exception>
        void AddFilters(IEnumerable<ISyntaxFilter> filters);

        /// <summary>
        /// Removes all <see cref="ISyntaxFilter"/>s from the <see cref="IFilterGroup"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException"><see cref="IFilterGroup"/> is sealed.</exception>
        void Clear();

        /// <summary>
        /// Checks if the <see cref="IFilterGroup"/> contains the specified <paramref name="filter"/>.
        /// </summary>
        /// <param name="filter"><see cref="ISyntaxFilter"/> to add to the <see cref="IFilterGroup"/>.</param>
        bool ContainsFilter(ISyntaxFilter filter);

        /// <summary>
        /// Prohibits this <see cref="IFilterGroup"/> from further modifications until <see cref="Unseal"/> is called.
        /// </summary>
        void Seal();

        /// <summary>
        /// Converts this <see cref="IFilterGroup"/> into an array of <see cref="ISyntaxFilter"/>s.
        /// </summary>
        ISyntaxFilter[] ToArray();

        /// <summary>
        /// Allows this <see cref="IFilterGroup"/> to be modified after the <see cref="Seal"/> method was called.
        /// </summary>
        void Unseal();
    }
}