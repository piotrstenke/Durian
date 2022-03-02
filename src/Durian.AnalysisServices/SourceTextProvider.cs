// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

namespace Durian.Analysis
{
    /// <inheritdoc cref="ISourceTextProvider"/>
    public abstract class SourceTextProvider : ISourceTextProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SourceTextProvider"/> class.
        /// </summary>
        protected SourceTextProvider()
        {
        }

        /// <inheritdoc/>
        public virtual string GetFullName()
        {
            return $"{GetNamespace()}.{GetTypeName()}";
        }

        /// <inheritdoc/>
        public virtual string GetHintName()
        {
            return GetFullName();
        }

        /// <inheritdoc/>
        public abstract string GetNamespace();

        /// <inheritdoc/>
        public abstract string GetText();

        /// <inheritdoc/>
        public abstract string GetTypeName();

        /// <inheritdoc/>
        public override string ToString()
        {
            return GetHintName();
        }
    }
}