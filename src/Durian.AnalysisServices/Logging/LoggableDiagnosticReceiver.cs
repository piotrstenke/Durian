// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;

namespace Durian.Analysis.Logging
{
    /// <summary>
    /// A <see cref="INodeDiagnosticReceiver"/> that uses a <see cref="LoggableGenerator"/> to log the received <see cref="Diagnostic"/>s.
    /// </summary>
    public partial class LoggableDiagnosticReceiver : INodeDiagnosticReceiver
    {
        private readonly DiagnosticBag _bag;

        /// <inheritdoc/>
        public int Count => _bag.Count;

        /// <summary>
        /// <see cref="ILoggableGenerator"/> this <see cref="LoggableDiagnosticReceiver"/> reports the diagnostics to.
        /// </summary>
        public ILoggableGenerator Generator { get; }

        /// <summary>
        /// Determines what to output when a <see cref="SyntaxNode"/> is being logged.
        /// </summary>
        public NodeOutput NodeOutput { get; set; }

        /// <summary>
        /// Name of the log file to log to.
        /// </summary>
        public string? HintName { get; private set; }

        /// <summary>
        /// Target <see cref="CSharpSyntaxNode"/>.
        /// </summary>
        public CSharpSyntaxNode? Node { get; private set; }

        /// <inheritdoc cref="LoggableDiagnosticReceiver(ILoggableGenerator, NodeOutput)"/>
        public LoggableDiagnosticReceiver(ILoggableGenerator generator)
        {
            if (generator is null)
            {
                throw new ArgumentNullException(nameof(generator));
            }

            _bag = new();
            Generator = generator;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LoggableGenerator"/> class.
        /// </summary>
        /// <param name="generator"><see cref="LoggableGenerator"/> that will log the received <see cref="Diagnostic"/>s.</param>
        /// <param name="nodeOutput">Determines what to output when a <see cref="SyntaxNode"/> is being logged.</param>
        /// <exception cref="ArgumentNullException"><paramref name="generator"/> is <see langword="null"/>.</exception>
        public LoggableDiagnosticReceiver(ILoggableGenerator generator, NodeOutput nodeOutput) : this(generator)
        {
            NodeOutput = nodeOutput;
        }

        /// <summary>
        /// Removes all <see cref="Diagnostic"/>s that weren't logged using the <see cref="Push"/> method.
        /// </summary>
        public virtual void Clear()
        {
            _bag.Clear();
        }

        /// <summary>
        /// Actually writes the diagnostics to the target file.
        /// </summary>
        public virtual void Push()
        {
            if (_bag.Count > 0)
            {
                Generator.LogDiagnostics(Node!, HintName!, _bag.GetDiagnostics(), NodeOutput);
                Clear();
            }
        }

        /// <inheritdoc/>
        public virtual void ReportDiagnostic(DiagnosticDescriptor descriptor, Location? location, params object?[]? messageArgs)
        {
            _bag.ReportDiagnostic(descriptor, location, messageArgs);
        }

        /// <inheritdoc/>
        public virtual void ReportDiagnostic(Diagnostic diagnostic)
        {
            _bag.ReportDiagnostic(diagnostic);
        }

        /// <inheritdoc/>
        public void SetTargetNode(CSharpSyntaxNode? node, string? hintName)
        {
            Node = node;
            HintName = hintName;
        }
    }
}