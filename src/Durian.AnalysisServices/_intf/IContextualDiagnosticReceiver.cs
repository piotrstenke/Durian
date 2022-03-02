// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

namespace Durian.Analysis
{
    /// <summary>
    /// Provides a mechanism for reporting diagnostic messages to a context.
    /// </summary>
    /// <typeparam name="T">Type of context this <see cref="IContextualDiagnosticReceiver{T}"/> is compliant with.</typeparam>
    public interface IContextualDiagnosticReceiver<T> : IDirectDiagnosticReceiver where T : struct
    {
        /// <summary>
        /// Returns a reference to the target context.
        /// </summary>
        ref readonly T GetContext();

        /// <summary>
        /// Resets the internal context to <see langword="default"/>.
        /// </summary>
        void RemoveContext();

        /// <summary>
        /// Sets the target context.
        /// </summary>
        /// <param name="context">Context to set as a target of this <see cref="IContextualDiagnosticReceiver{T}"/>.</param>
        void SetContext(in T context);
    }
}