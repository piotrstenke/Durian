// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using Microsoft.CodeAnalysis;

namespace Durian.Analysis.Logging
{
	/// <summary>
	/// A <see cref="INodeDiagnosticReceiver"/> that reports <see cref="Diagnostic"/> to both <see cref="LoggableSourceGenerator"/> and <see cref="ContextualDiagnosticReceiver{T}"/>.
	/// </summary>
	public sealed class ContextualLoggableGeneratorDiagnosticReceiver<T> : LoggableGeneratorDiagnosticReceiver where T : struct
	{
		private readonly ContextualDiagnosticReceiver<T> _diagnosticReceiver;
		private bool _enableLogging;
		private bool _enableReport;

		/// <summary>
		/// Specifies whether this <see cref="ContextualLoggableGeneratorDiagnosticReceiver{T}"/> should log the <see cref="Diagnostic"/>s, report them or both or neither.
		/// </summary>
		/// <remarks>The default value is <see cref="ReportDiagnosticTarget.Both"/>.</remarks>
		public ReportDiagnosticTarget Target
		{
			get => GeneratorLoggingConfiguration.GetReportDiagnosticTarget(_enableLogging, _enableReport);
			set
			{
				_enableReport = value is ReportDiagnosticTarget.Report or ReportDiagnosticTarget.Both;
				_enableLogging = value is ReportDiagnosticTarget.Log or ReportDiagnosticTarget.Both;
			}
		}

		/// <inheritdoc cref="ContextualLoggableGeneratorDiagnosticReceiver{T}(LoggableSourceGenerator, ContextualDirectReportAction{T}, in T)"/>
		public ContextualLoggableGeneratorDiagnosticReceiver(LoggableSourceGenerator generator, ContextualDirectReportAction<T> action) : base(generator)
		{
			_diagnosticReceiver = new(action);
			SetTarget(generator);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ContextualLoggableGeneratorDiagnosticReceiver{T}"/> class.
		/// </summary>
		/// <param name="generator"><see cref="LoggableSourceGenerator"/> that will log the received <see cref="Diagnostic"/>s.</param>
		/// <param name="action"><see cref="ContextualDirectReportAction{T}"/> to be performed when the <see cref="ReportDiagnostic(Diagnostic)"/> method is called.</param>
		/// <param name="context">Context of this <see cref="ContextualDiagnosticReceiver{T}"/>.</param>
		/// <exception cref="ArgumentNullException"><paramref name="action"/> is <see langword="null"/>.</exception>
		public ContextualLoggableGeneratorDiagnosticReceiver(LoggableSourceGenerator generator, ContextualDirectReportAction<T> action, in T context) : base(generator)
		{
			_diagnosticReceiver = new(action, in context);
			SetTarget(generator);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ContextualLoggableGeneratorDiagnosticReceiver{T}"/> class.
		/// </summary>
		/// <param name="generator"><see cref="LoggableSourceGenerator"/> that will log the received <see cref="Diagnostic"/>s.</param>
		/// <param name="diagnosticReceiver">Target <see cref="ContextualDiagnosticReceiver{T}"/>.</param>
		public ContextualLoggableGeneratorDiagnosticReceiver(LoggableSourceGenerator generator, ContextualDiagnosticReceiver<T> diagnosticReceiver) : base(generator)
		{
			_diagnosticReceiver = diagnosticReceiver;
			SetTarget(generator);
		}

		/// <inheritdoc cref="ContextualDiagnosticReceiver{T}.GetContext"/>
		public ref readonly T GetContext()
		{
			return ref _diagnosticReceiver.GetContext();
		}

		/// <inheritdoc cref="ContextualDiagnosticReceiver{T}.RemoveContext"/>
		public void RemoveContext()
		{
			_diagnosticReceiver.RemoveContext();
		}

		/// <inheritdoc cref="ContextualDiagnosticReceiver{T}.ReportDiagnostic(DiagnosticDescriptor, Location?, object?[])"/>
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

		/// <inheritdoc cref="ContextualDiagnosticReceiver{T}.ReportDiagnostic(Diagnostic)"/>
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
		/// <param name="context">Context to set as a target of this <see cref="ContextualLoggableGeneratorDiagnosticReceiver{T}"/>.</param>
		public void SetContext(in T context)
		{
			_diagnosticReceiver.SetContext(in context);
		}

		private void SetTarget(LoggableSourceGenerator generator)
		{
			_enableLogging = generator.LoggingConfiguration.EnableLogging;
			_enableReport = generator.LoggingConfiguration.EnableDiagnostics;
		}
	}
}
