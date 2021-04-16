using System;
using Microsoft.CodeAnalysis;

namespace Durian
{
	/// <summary>
	/// Provides a mechanism for reporting diagnostic messages to a context.
	/// </summary>
	/// <typeparam name="T">Type of context this <see cref="DiagnosticReceiver{T}"/> is compliant with.</typeparam>
	public sealed partial class DiagnosticReceiver<T> : IDiagnosticReceiver where T : struct
	{
		private readonly DiagnosticReceiverAction<T> _action;
		private T _context;
		private bool _contextIsSet;

		/// <inheritdoc cref="DiagnosticReceiver{T}(DiagnosticReceiverAction{T}, in T)"/>
		public DiagnosticReceiver(DiagnosticReceiverAction<T> action)
		{
			if (action is null)
			{
				throw new ArgumentNullException(nameof(action));
			}

			_action = action;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DiagnosticReceiver{T}"/> class.
		/// </summary>
		/// <param name="action"><see cref="DiagnosticReceiverAction{T}"/> to be performed when the <see cref="ReportDiagnostic(DiagnosticDescriptor, Location, object[])"/> method is called.</param>
		/// <param name="context">Context of this <see cref="DiagnosticReceiver{T}"/>.</param>
		/// <exception cref="ArgumentNullException"><paramref name="action"/> was <c>null</c>.</exception>
		public DiagnosticReceiver(DiagnosticReceiverAction<T> action, in T context)
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
		/// <param name="context">Context to set as a target if this <see cref="DiagnosticReceiver{T}"/>.</param>
		public void SetContext(in T context)
		{
			_contextIsSet = true;
			_context = context;
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

			_action.Invoke(_context, descriptor, location, messageArgs);
		}
	}
}
