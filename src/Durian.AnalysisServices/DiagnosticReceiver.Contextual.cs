// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using Microsoft.CodeAnalysis;

namespace Durian.Analysis
{
	public sealed partial class DiagnosticReceiver
	{
		/// <inheritdoc cref="IContextualDiagnosticReceiver{T}"/>
		public sealed class Contextual<T> : IContextualDiagnosticReceiver<T> where T : struct
		{
			private readonly ReportAction.DirectContextual<T> _action;

			private T _context;

			private bool _contextIsSet;

			/// <inheritdoc cref="Contextual{T}(ReportAction.DirectContextual{T}, in T)"/>
			public Contextual(ReportAction.DirectContextual<T> action)
			{
				if (action is null)
				{
					throw new ArgumentNullException(nameof(action));
				}

				_action = action;
			}

			/// <summary>
			/// Initializes a new instance of the <see cref="Contextual{T}"/> class.
			/// </summary>
			/// <param name="action"><see cref="ReportAction.DirectContextual{T}"/> to be performed when the <see cref="ReportDiagnostic(Diagnostic)"/> method is called.</param>
			/// <param name="context">Context of this <see cref="ReportAction.DirectContextual{T}"/>.</param>
			/// <exception cref="ArgumentNullException"><paramref name="action"/> is <see langword="null"/>.</exception>
			public Contextual(ReportAction.DirectContextual<T> action, in T context)
			{
				if (action is null)
				{
					throw new ArgumentNullException(nameof(action));
				}

				_action = action;
				_context = context;
				_contextIsSet = true;
			}

			/// <summary>
			/// Returns a reference to the target context.
			/// </summary>
			/// <exception cref="InvalidOperationException">Target context not set.</exception>
			public ref readonly T GetContext()
			{
				CheckContext();
				return ref _context;
			}

			/// <inheritdoc/>
			public void RemoveContext()
			{
				_contextIsSet = false;
				_context = default;
			}

			/// <inheritdoc/>
			/// <exception cref="InvalidOperationException">Target context not set.</exception>
			public void ReportDiagnostic(DiagnosticDescriptor descriptor, Location? location, params object?[]? messageArgs)
			{
				if (descriptor is null)
				{
					throw new ArgumentNullException(nameof(descriptor));
				}

				CheckContext();
				_action.Invoke(_context, Diagnostic.Create(descriptor, location, messageArgs));
			}

			/// <inheritdoc/>
			/// <exception cref="InvalidOperationException">Target context not set.</exception>
			public void ReportDiagnostic(Diagnostic diagnostic)
			{
				if (diagnostic is null)
				{
					throw new ArgumentNullException(nameof(diagnostic));
				}

				CheckContext();
				_action.Invoke(_context, diagnostic);
			}

			/// <summary>
			/// Sets the target context.
			/// </summary>
			/// <param name="context">Context to set as a target of this <see cref="Contextual{T}"/>.</param>
			public void SetContext(in T context)
			{
				_contextIsSet = true;
				_context = context;
			}

			private void CheckContext()
			{
				if (!_contextIsSet)
				{
					throw new InvalidOperationException("Target context not set!");
				}
			}
		}
	}
}