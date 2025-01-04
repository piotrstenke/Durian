using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;

namespace Durian.Analysis
{
	public sealed partial class DiagnosticReceiver
	{
		/// <summary>
		/// Provides a mechanism for reporting diagnostic messages to a context.
		/// </summary>
		/// <typeparam name="T">Type of context this <see cref="IContextualDiagnosticReceiver{T}"/> is compliant with.</typeparam>
		public sealed class ReadonlyContextual<T> : IContextualDiagnosticReceiver<T> where T : struct
		{
			private readonly ReportAction.ReadonlyDirectContextual<T> _action;

			private T _context;

			/// <inheritdoc/>
			[MemberNotNullWhen(true, nameof(_context))]
			public bool HasContext { get; private set; }

			/// <inheritdoc cref="ReadonlyContextual(ReportAction.ReadonlyDirectContextual{T}, in T)"/>
			public ReadonlyContextual(ReportAction.ReadonlyDirectContextual<T> action)
			{
				if (action is null)
				{
					throw new ArgumentNullException(nameof(action));
				}

				_action = action;
			}

			/// <summary>
			/// Initializes a new instance of the <see cref="ReadonlyContextual{T}"/> class.
			/// </summary>
			/// <param name="action"><see cref="ReportAction.ReadonlyDirectContextual{T}"/> to be performed when the <see cref="ReportDiagnostic(Diagnostic)"/> method is called.</param>
			/// <param name="context">Context of this <see cref="ReadonlyContextual{T}"/>.</param>
			/// <exception cref="ArgumentNullException"><paramref name="action"/> is <see langword="null"/>.</exception>
			public ReadonlyContextual(ReportAction.ReadonlyDirectContextual<T> action, in T context)
			{
				if (action is null)
				{
					throw new ArgumentNullException(nameof(action));
				}

				_action = action;
				_context = context;
				HasContext = true;
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
				HasContext = false;
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
			/// <param name="context">Context to set as a target if this <see cref="ReadonlyContextual{T}"/>.</param>
			public void SetContext(in T context)
			{
				_context = context;
				HasContext = true;
			}

			[MemberNotNull(nameof(_context))]
			private void CheckContext()
			{
				if (!HasContext)
				{
					throw new InvalidOperationException("Target context not set!");
				}
			}
		}
	}
}
