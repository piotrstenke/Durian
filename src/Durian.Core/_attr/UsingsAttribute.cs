// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Diagnostics;

namespace Durian
{
    /// <summary>
    /// Specifies that the following using statements should be applied when generating new syntax tree.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface, AllowMultiple = false, Inherited = false)]
    [Conditional("DEBUG")]
    public sealed class UsingsAttribute : Attribute
    {
        /// <summary>
        /// Usings to apply when generating new syntax tree.
        /// </summary>
        public string[]? Usings { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="UsingsAttribute"/> class.
        /// </summary>
        /// <param name="usings">Usings to apply when generating new syntax tree.</param>
        public UsingsAttribute(params string[]? usings)
        {
            Usings = usings;
        }
    }
}