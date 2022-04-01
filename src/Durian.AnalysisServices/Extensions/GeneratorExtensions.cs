// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using Durian.Analysis.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;

namespace Durian.Analysis.Extensions
{
	/// <summary>
	/// Contains various extension methods for <see cref="ISourceGenerator"/>s.
	/// </summary>
	public static class GeneratorExtensions
	{
		/// <summary>
		/// Returns the <see cref="IGeneratorPassContext.LogReceiver"/> or <see cref="DiagnosticReceiver.Factory.Empty"/>.
		/// </summary>
		/// <param name="context"><see cref="IGeneratorPassContext"/> to get the <see cref="INodeDiagnosticReceiver"/> from.</param>
		/// <param name="includeDiagnostics">
		/// Determines whether to include diagnostics other than log files.
		/// <para>If <see cref="FilterMode"/> is equal to <see cref="FilterMode.Both"/>,
		/// <paramref name="includeDiagnostics"/> is <see langword="true"/>, otherwise <see langword="false"/>.</para>
		/// </param>
		/// <exception cref="ArgumentNullException"><paramref name="context"/> is <see langword="null"/>.</exception>
		public static INodeDiagnosticReceiver GetLogReceiverOrEmpty(this IGeneratorPassContext context, bool includeDiagnostics = false)
		{
			if (context is null)
			{
				throw new ArgumentNullException(nameof(context));
			}

			if (includeDiagnostics && context.DiagnosticReceiver is not null)
			{
				if (context.LogReceiver is IContextualDiagnosticReceiver<GeneratorExecutionContext>)
				{
					return context.LogReceiver;
				}

				if (context.Generator is ILoggableGenerator loggable)
				{
					return LoggableDiagnosticReceiver.Factory.SourceGenerator(loggable, context.DiagnosticReceiver);
				}

				if (context.LogReceiver is not null)
				{
					return DiagnosticReceiver.Factory.Composite(context.LogReceiver, context.DiagnosticReceiver);
				}
			}
			else if (context.LogReceiver is not null)
			{
				return context.LogReceiver;
			}

			return DiagnosticReceiver.Factory.Empty();
		}

		/// <summary>
		/// Returns the <see cref="IGeneratorPassContext.DiagnosticReceiver"/> or <see cref="DiagnosticReceiver.Factory.Empty"/>.
		/// </summary>
		/// <param name="context"><see cref="IGeneratorPassContext"/> to get the <see cref="IDiagnosticReceiver"/> from.</param>
		/// <exception cref="ArgumentNullException"><paramref name="context"/> is <see langword="null"/>.</exception>
		public static IDiagnosticReceiver GetDiagnosticReceiverOrEmpty(this IGeneratorPassContext context)
		{
			if (context is null)
			{
				throw new ArgumentNullException(nameof(context));
			}

			if (context.DiagnosticReceiver is null)
			{
				return DiagnosticReceiver.Factory.Empty();
			}

			return context.DiagnosticReceiver;
		}

		/// <summary>
		/// Returns a <see cref="FilterMode"/> associated with the specified <paramref name="context"/>.
		/// </summary>
		/// <param name="context"><see cref="IGeneratorPassContext"/> to get the <see cref="FilterMode"/> associated with.</param>
		/// <exception cref="ArgumentNullException"><paramref name="context"/> is <see langword="null"/>.</exception>
		public static FilterMode GetFilterMode(this IGeneratorPassContext context)
		{
			if (context is null)
			{
				throw new ArgumentNullException(nameof(context));
			}

			return context.Generator.GetFilterMode();
		}

		/// <summary>
		/// Returns a <see cref="FilterMode"/> associated with the specified <paramref name="generator"/>.
		/// </summary>
		/// <param name="generator"><see cref="IDurianGenerator"/> to get the <see cref="FilterMode"/> associated with.</param>
		/// <exception cref="ArgumentNullException"><paramref name="generator"/> is <see langword="null"/>.</exception>
		public static FilterMode GetFilterMode(this IDurianGenerator generator)
		{
			if (generator is ILoggableGenerator loggable)
			{
				return GetFilterMode(loggable);
			}

			if (generator is null)
			{
				throw new ArgumentNullException(nameof(generator));
			}

			return GetFilterMode(generator.EnableDiagnostics, generator.EnableLogging);
		}

		/// <summary>
		/// Returns a <see cref="FilterMode"/> associated with the specified <paramref name="generator"/>.
		/// </summary>
		/// <param name="generator"><see cref="ILoggableGenerator"/> to get the <see cref="FilterMode"/> associated with.</param>
		/// <exception cref="ArgumentNullException"><paramref name="generator"/> is <see langword="null"/>.</exception>
		public static FilterMode GetFilterMode(this ILoggableGenerator generator)
		{
			if (generator is null)
			{
				throw new ArgumentNullException(nameof(generator));
			}

			return generator.LoggingConfiguration.CurrentFilterMode;
		}

#pragma warning disable RCS1224 // Make method an extension method.

		internal static FilterMode GetFilterMode(bool enableDiagnostics, bool enableLogging)
#pragma warning restore RCS1224 // Make method an extension method.
		{
			FilterMode mode = FilterMode.None;

			if (enableDiagnostics)
			{
				mode += (int)FilterMode.Diagnostics;
			}

			if (enableLogging)
			{
				mode += (int)FilterMode.Logs;
			}

			return mode;
		}
	}
}
