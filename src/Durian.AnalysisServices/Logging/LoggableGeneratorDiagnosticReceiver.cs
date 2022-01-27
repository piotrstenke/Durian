﻿// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Durian.Analysis.Logging
{
	/// <summary>
	/// A <see cref="INodeDiagnosticReceiver"/> that uses a <see cref="LoggableSourceGenerator"/> to log the received <see cref="Diagnostic"/>s.
	/// </summary>
	public class LoggableGeneratorDiagnosticReceiver : INodeDiagnosticReceiver
	{
		private readonly DiagnosticBag _bag;

		/// <inheritdoc/>
		public int Count => _bag.Count;

		/// <summary>
		/// <see cref="LoggableSourceGenerator"/> this <see cref="LoggableGeneratorDiagnosticReceiver"/> reports the diagnostics to.
		/// </summary>
		public LoggableSourceGenerator Generator { get; }

		/// <summary>
		/// Name of the log file to log to.
		/// </summary>
		public string? HintName { get; private set; }

		/// <summary>
		/// Target <see cref="CSharpSyntaxNode"/>.
		/// </summary>
		public CSharpSyntaxNode? Node { get; private set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="LoggableSourceGenerator"/> class.
		/// </summary>
		/// <param name="generator"><see cref="LoggableSourceGenerator"/> that will log the received <see cref="Diagnostic"/>s.</param>
		public LoggableGeneratorDiagnosticReceiver(LoggableSourceGenerator generator)
		{
			if (generator is null)
			{
				throw new ArgumentNullException(nameof(generator));
			}

			_bag = new();
			Generator = generator;
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
				Generator.LogDiagnostics(Node!, HintName!, _bag.GetDiagnostics());
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