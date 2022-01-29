// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using Microsoft.CodeAnalysis;

namespace Durian.Analysis.Logging
{
	public partial class LoggableGenerator
	{
		/// <summary>
		/// A <see cref="INodeDiagnosticReceiver"/> that reports <see cref="Diagnostic"/> to both <see cref="LoggableGenerator"/> and <see cref="Analysis.ReadonlyContextualDiagnosticReceiver{T}"/>.
		/// </summary>
		public sealed class ReadonlyContextualDiagnosticReceiver<T> : DiagnosticReceiver where T : struct
		{
			private readonly Analysis.ReadonlyContextualDiagnosticReceiver<T> _diagnosticReceiver;

			private bool _enableLogging;

			private bool _enableReport;

			/// <summary>
			/// Specifies whether this <see cref="ReadonlyContextualDiagnosticReceiver{T}"/> should log the <see cref="Diagnostic"/>s, report them or both or neither.
			/// </summary>
			/// <remarks>The default value is <see cref="ReportDiagnosticTarget.Both"/>.</remarks>
			public ReportDiagnosticTarget Target
			{
				get => LoggingConfiguration.GetReportDiagnosticTarget(_enableLogging, _enableReport);
				set
				{
					_enableReport = value is ReportDiagnosticTarget.Report or ReportDiagnosticTarget.Both;
					_enableLogging = value is ReportDiagnosticTarget.Log or ReportDiagnosticTarget.Both;
				}
			}

			/// <inheritdoc cref="ReadonlyContextualDiagnosticReceiver{T}(LoggableGenerator, ReadonlyContextualDirectReportAction{T}, in T)"/>
			public ReadonlyContextualDiagnosticReceiver(LoggableGenerator generator, ReadonlyContextualDirectReportAction<T> action) : base(generator)
			{
				_diagnosticReceiver = new(action);
				SetTarget(generator);
			}

			/// <summary>
			/// Initializes a new instance of the <see cref="ReadonlyContextualDiagnosticReceiver{T}"/> class.
			/// </summary>
			/// <param name="generator"><see cref="LoggableGenerator"/> that will log the received <see cref="Diagnostic"/>s.</param>
			/// <param name="action"><see cref="ContextualDirectReportAction{T}"/> to be performed when the <see cref="ReportDiagnostic(Diagnostic)"/> method is called.</param>
			/// <param name="context">Context of this <see cref="Analysis.ReadonlyContextualDiagnosticReceiver{T}"/>.</param>
			/// <exception cref="ArgumentNullException"><paramref name="action"/> is <see langword="null"/>.</exception>
			public ReadonlyContextualDiagnosticReceiver(LoggableGenerator generator, ReadonlyContextualDirectReportAction<T> action, in T context) : base(generator)
			{
				_diagnosticReceiver = new(action, in context);
				SetTarget(generator);
			}

			/// <summary>
			/// Initializes a new instance of the <see cref="ReadonlyContextualDiagnosticReceiver{T}"/> class.
			/// </summary>
			/// <param name="generator"><see cref="LoggableGenerator"/> that will log the received <see cref="Diagnostic"/>s.</param>
			/// <param name="diagnosticReceiver">Target <see cref="Analysis.ReadonlyContextualDiagnosticReceiver{T}"/>.</param>
			public ReadonlyContextualDiagnosticReceiver(LoggableGenerator generator, Analysis.ReadonlyContextualDiagnosticReceiver<T> diagnosticReceiver) : base(generator)
			{
				_diagnosticReceiver = diagnosticReceiver;
				SetTarget(generator);
			}

			/// <inheritdoc cref="Analysis.ReadonlyContextualDiagnosticReceiver{T}.GetContext"/>
			public ref readonly T GetContext()
			{
				return ref _diagnosticReceiver.GetContext();
			}

			/// <inheritdoc cref="Analysis.ReadonlyContextualDiagnosticReceiver{T}.RemoveContext"/>
			public void RemoveContext()
			{
				_diagnosticReceiver.RemoveContext();
			}

			/// <inheritdoc cref="Analysis.ReadonlyContextualDiagnosticReceiver{T}.ReportDiagnostic(DiagnosticDescriptor, Location?, object?[])"/>
			public override void ReportDiagnostic(DiagnosticDescriptor descriptor, Location? location, params object?[]? messageArgs)
			{
				if (_enableReport)
				{
					_diagnosticReceiver.ReportDiagnostic(descriptor, location, messageArgs);
				}

				if (_enableLogging)
				{
					base.ReportDiagnostic(descriptor, location, messageArgs);
				}
			}

			/// <inheritdoc cref="Analysis.ReadonlyContextualDiagnosticReceiver{T}.ReportDiagnostic(Diagnostic)"/>
			public override void ReportDiagnostic(Diagnostic diagnostic)
			{
				if (_enableReport)
				{
					_diagnosticReceiver.ReportDiagnostic(diagnostic);
				}

				if (_enableLogging)
				{
					base.ReportDiagnostic(diagnostic);
				}
			}

			/// <summary>
			/// Sets the target context.
			/// </summary>
			/// <param name="context">Context to set as a target of this <see cref="ContextualDiagnosticReceiver{T}"/>.</param>
			public void SetContext(in T context)
			{
				_diagnosticReceiver.SetContext(in context);
			}

			private void SetTarget(LoggableGenerator generator)
			{
				_enableLogging = generator.LoggingConfiguration.EnableLogging;
				_enableReport = generator.LoggingConfiguration.EnableDiagnostics;
			}
		}
	}
}