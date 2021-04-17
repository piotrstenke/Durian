using System;
using Microsoft.CodeAnalysis;

namespace Durian
{
	/// <summary>
	/// A <see cref="IDiagnosticReceiver"/> that accepts see <see cref="ReadonlyDiagnosticReceiverAction{T}"/> instead of <see cref="DiagnosticReceiverAction{T}"/>.
	/// </summary>
	public sealed class ReadonlyDiagnosticReceiver<T> : IDiagnosticReceiver where T : struct
	{
		private readonly ReadonlyDiagnosticReceiverAction<T> _action;
		private T _context;
		private bool _contextIsSet;

		/// <inheritdoc cref="ReadonlyDiagnosticReceiver(ReadonlyDiagnosticReceiverAction{T}, in T)"/>
		public ReadonlyDiagnosticReceiver(ReadonlyDiagnosticReceiverAction<T> action)
		{
			if (action is null)
			{
				throw new ArgumentNullException(nameof(action));
			}

			_action = action;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ReadonlyDiagnosticReceiver{T}"/> class.
		/// </summary>
		/// <param name="action"><see cref="ReadonlyDiagnosticReceiverAction{T}"/> to be performed when the <see cref="ReportDiagnostic(DiagnosticDescriptor, Location, object[])"/> method is called.</param>
		/// <param name="context">Context of this <see cref="DiagnosticReceiver{T}"/>.</param>
		/// <exception cref="ArgumentNullException"><paramref name="action"/> is <c>null</c>.</exception>
		public ReadonlyDiagnosticReceiver(ReadonlyDiagnosticReceiverAction<T> action, in T context)
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
			if (!_contextIsSet)
			{
				throw new InvalidOperationException("Target context not set!");
			}

			return ref _context;
		}

		/// <summary>
		/// Sets the target context.
		/// </summary>
		/// <param name="context">Context to set as a target if this <see cref="ReadonlyDiagnosticReceiver{T}"/>.</param>
		public void SetContext(in T context)
		{
			_context = context;
			_contextIsSet = true;
		}

		/// <summary>
		/// Resets the internal context to <c>default</c>.
		/// </summary>
		public void RemoveContext()
		{
			_contextIsSet = false;
			_context = default;
		}

		/// <summary>
		/// Reports a diagnostic.
		/// </summary>
		/// <param name="descriptor"><see cref="DiagnosticDescriptor"/> that is used to report the diagnostics.</param>
		/// <param name="location">Source <see cref="Location"/> of the reported diagnostic.</param>
		/// <param name="messageArgs">Arguments of the diagnostic message.</param>
		/// <exception cref="InvalidOperationException">Target context not set.</exception>
		public void ReportDiagnostic(DiagnosticDescriptor descriptor, Location? location, params object?[]? messageArgs)
		{
			if (!_contextIsSet)
			{
				throw new InvalidOperationException("Target context not set!");
			}

			_action.Invoke(in _context, descriptor, location, messageArgs);
		}
	}
}
