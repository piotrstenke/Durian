// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using Microsoft.CodeAnalysis;

namespace Durian.Analysis
{
	/// <summary>
	/// A <see cref="IDirectDiagnosticReceiver"/> that accepts see <see cref="ReadonlyContextualReportAction{T}"/> instead of <see cref="ContextualReportAction{T}"/>.
	/// </summary>
	public sealed class ReadonlyContextualDiagnosticReceiver<T> : IContextualDiagnosticReceiver<T> where T : struct
	{
		private readonly ReadonlyContextualDirectReportAction<T> _action;

		private T _context;

		private bool _contextIsSet;

		/// <inheritdoc cref="ReadonlyContextualDiagnosticReceiver(ReadonlyContextualDirectReportAction{T}, in T)"/>
		public ReadonlyContextualDiagnosticReceiver(ReadonlyContextualDirectReportAction<T> action)
		{
			if (action is null)
			{
				throw new ArgumentNullException(nameof(action));
			}

			_action = action;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ReadonlyContextualDiagnosticReceiver{T}"/> class.
		/// </summary>
		/// <param name="action"><see cref="ReadonlyContextualDirectReportAction{T}"/> to be performed when the <see cref="ReportDiagnostic(Diagnostic)"/> method is called.</param>
		/// <param name="context">Context of this <see cref="ContextualDiagnosticReceiver{T}"/>.</param>
		/// <exception cref="ArgumentNullException"><paramref name="action"/> is <see langword="null"/>.</exception>
		public ReadonlyContextualDiagnosticReceiver(ReadonlyContextualDirectReportAction<T> action, in T context)
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
			CheckContext();
			_action.Invoke(in _context, Diagnostic.Create(descriptor, location, messageArgs));
		}

		/// <inheritdoc/>
		/// <exception cref="InvalidOperationException">Target context not set.</exception>
		public void ReportDiagnostic(Diagnostic diagnostic)
		{
			CheckContext();
			_action.Invoke(in _context, diagnostic);
		}

		/// <summary>
		/// Sets the target context.
		/// </summary>
		/// <param name="context">Context to set as a target if this <see cref="ReadonlyContextualDiagnosticReceiver{T}"/>.</param>
		public void SetContext(in T context)
		{
			_context = context;
			_contextIsSet = true;
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