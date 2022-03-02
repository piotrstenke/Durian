// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Microsoft.CodeAnalysis;
using System;

namespace Durian.Analysis
{
    /// <summary>
    /// Specifies that the target <see cref="DiagnosticDescriptor"/> does not have a specific location.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public sealed class WithoutLocationAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WithoutLocationAttribute"/> class.
        /// </summary>
        public WithoutLocationAttribute()
        {
        }
    }
}