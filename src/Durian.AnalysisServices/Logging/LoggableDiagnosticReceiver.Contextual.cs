// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Microsoft.CodeAnalysis;
using System;

namespace Durian.Analysis.Logging
{
    public partial class LoggableDiagnosticReceiver
    {
        /// <summary>
        /// A <see cref="INodeDiagnosticReceiver"/> that reports <see cref="Diagnostic"/> to both <see cref="LoggableGenerator"/> and <see cref="DiagnosticReceiver.Contextual{T}"/>.
        /// </summary>
        public sealed class Contextual<T> : LoggableDiagnosticReceiver where T : struct
        {
            private readonly DiagnosticReceiver.Contextual<T> _diagnosticReceiver;

            /// <summary>
            /// Specifies whether this <see cref="Contextual{T}"/> should log the <see cref="Diagnostic"/>s, report them or both or neither.
            /// </summary>
            /// <remarks>The default value is <see cref="ReportDiagnosticTarget.Both"/>.</remarks>
            public ReportDiagnosticTarget Target
            {
                get => LoggingConfiguration.GetReportDiagnosticTarget(Generator.LoggingConfiguration.EnableLogging, Generator.LoggingConfiguration.EnableDiagnostics);
                set
                {
                    Generator.LoggingConfiguration.EnableDiagnostics = value is ReportDiagnosticTarget.Report or ReportDiagnosticTarget.Both;
                    Generator.LoggingConfiguration.EnableLogging = value is ReportDiagnosticTarget.Log or ReportDiagnosticTarget.Both;
                }
            }

            /// <inheritdoc cref="Contextual{T}(ILoggableGenerator, ReportAction.DirectContextual{T}, in T)"/>
            public Contextual(ILoggableGenerator generator, ReportAction.DirectContextual<T> action) : base(generator)
            {
                _diagnosticReceiver = new(action);
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="Contextual{T}"/> class.
            /// </summary>
            /// <param name="generator"><see cref="ILoggableGenerator"/> that will log the received <see cref="Diagnostic"/>s.</param>
            /// <param name="action"><see cref="ReportAction.DirectContextual{T}"/> to be performed when the <see cref="ReportDiagnostic(Diagnostic)"/> method is called.</param>
            /// <param name="context">Context of this <see cref="Contextual{T}"/>.</param>
            /// <exception cref="ArgumentNullException"><paramref name="action"/> is <see langword="null"/>.</exception>
            public Contextual(ILoggableGenerator generator, ReportAction.DirectContextual<T> action, in T context) : base(generator)
            {
                _diagnosticReceiver = new(action, in context);
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="Contextual{T}"/> class.
            /// </summary>
            /// <param name="generator"><see cref="LoggableGenerator"/> that will log the received <see cref="Diagnostic"/>s.</param>
            /// <param name="diagnosticReceiver">Target <see cref="DiagnosticReceiver.Contextual{T}"/>.</param>
            public Contextual(ILoggableGenerator generator, DiagnosticReceiver.Contextual<T> diagnosticReceiver) : base(generator)
            {
                _diagnosticReceiver = diagnosticReceiver;
            }

            /// <inheritdoc cref="DiagnosticReceiver.Contextual{T}.GetContext"/>
            public ref readonly T GetContext()
            {
                return ref _diagnosticReceiver.GetContext();
            }

            /// <inheritdoc cref="DiagnosticReceiver.Contextual{T}.RemoveContext"/>
            public void RemoveContext()
            {
                _diagnosticReceiver.RemoveContext();
            }

            /// <inheritdoc cref="DiagnosticReceiver.Contextual{T}.ReportDiagnostic(DiagnosticDescriptor, Location?, object?[])"/>
            public override void ReportDiagnostic(DiagnosticDescriptor descriptor, Location? location, params object?[]? messageArgs)
            {
                if (Generator.LoggingConfiguration.EnableDiagnostics)
                {
                    _diagnosticReceiver.ReportDiagnostic(descriptor, location, messageArgs);
                }

                if (Generator.LoggingConfiguration.EnableLogging)
                {
                    base.ReportDiagnostic(descriptor, location, messageArgs);
                }
            }

            /// <inheritdoc cref="DiagnosticReceiver.Contextual{T}.ReportDiagnostic(Diagnostic)"/>
            public override void ReportDiagnostic(Diagnostic diagnostic)
            {
                if (Generator.LoggingConfiguration.EnableDiagnostics)
                {
                    _diagnosticReceiver.ReportDiagnostic(diagnostic);
                }

                if (Generator.LoggingConfiguration.EnableLogging)
                {
                    base.ReportDiagnostic(diagnostic);
                }
            }

            /// <summary>
            /// Sets the target context.
            /// </summary>
            /// <param name="context">Context to set as a target of this <see cref="Contextual{T}"/>.</param>
            public void SetContext(in T context)
            {
                _diagnosticReceiver.SetContext(in context);
            }
        }
    }
}