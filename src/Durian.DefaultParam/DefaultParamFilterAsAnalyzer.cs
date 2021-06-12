// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using Durian.Generator.Cache;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Durian.Generator.DefaultParam
{
	/// <summary>
	/// Base class for all DefaultParam <see cref="ISyntaxFilter"/>s that want to act as an Roslyn analyzer.
	/// </summary>
	/// <typeparam name="T">Type of <see cref="IDefaultParamTarget"/> this <see cref="DefaultParamFilterAsAnalyzer{T}"/> supports.</typeparam>
	public abstract class DefaultParamFilterAsAnalyzer<T> : ICachedAnalyzerInfo<IDefaultParamTarget> where T : IDefaultParamTarget
	{
		/// <inheritdoc/>
		public bool AllowGenerated => true;

		/// <inheritdoc/>
		public bool Concurrent => true;

		/// <summary>
		/// Initializes a new instance of the <see cref="DefaultParamFilterAsAnalyzer{T}"/> class.
		/// </summary>
		protected DefaultParamFilterAsAnalyzer()
		{
		}

		/// <inheritdoc/>
		public void Register(IDurianAnalysisContext context, CSharpCompilation compilation, ConcurrentDictionary<FileLinePositionSpan, T> cached)
		{
			DefaultParamCompilationData data = new(compilation);

			if (data.HasErrors)
			{
				return;
			}

			context.RegisterSyntaxNodeAction(context => Analyze(data, context, cached), SyntaxKind.TypeParameterList);
		}

		/// <inheritdoc/>
		public void Register(IDurianAnalysisContext context, CSharpCompilation compilation)
		{
			DefaultParamCompilationData data = new(compilation);

			if (data.HasErrors)
			{
				return;
			}

			context.RegisterSyntaxNodeAction(context => Analyze(data, context, out _), SyntaxKind.TypeParameterList);
		}

		void ICachedAnalyzerInfo<IDefaultParamTarget>.Register(IDurianAnalysisContext context, CSharpCompilation compilation, ConcurrentDictionary<FileLinePositionSpan, IDefaultParamTarget> cached)
		{
			DefaultParamCompilationData data = new(compilation);

			if (data.HasErrors)
			{
				return;
			}

			context.RegisterSyntaxNodeAction(context => Analyze(data, context, cached), SyntaxKind.TypeParameterList);
		}

		/// <summary>
		/// Performs analysis on the specified <paramref name="context"/> and returns a new instance of <see cref="IDefaultParamTarget"/> of type <typeparamref name="T"/> if the analysis was a success.
		/// </summary>
		/// <param name="compilation">Current <see cref="DefaultParamCompilationData"/>.</param>
		/// <param name="context"><see cref="SyntaxNodeAnalysisContext"/> to analyze.</param>
		/// <param name="data"><see cref="IDefaultParamTarget"/> that is returned if the analysis is a success.</param>
		protected abstract bool Analyze(DefaultParamCompilationData compilation, SyntaxNodeAnalysisContext context, [NotNullWhen(true)] out T? data);

		private void Analyze(DefaultParamCompilationData compilation, SyntaxNodeAnalysisContext context, ConcurrentDictionary<FileLinePositionSpan, T> dict)
		{
			Analyze(compilation, context, out T? data);

			if (data is not null)
			{
				dict.AddOrUpdate(context.Node.GetLocation().GetLineSpan(), data, (key, value) => data);
			}
		}

		private void Analyze(DefaultParamCompilationData compilation, SyntaxNodeAnalysisContext context, ConcurrentDictionary<FileLinePositionSpan, IDefaultParamTarget> dict)
		{
			Analyze(compilation, context, out T? data);

			if (data is not null)
			{
				dict.AddOrUpdate(context.Node.GetLocation().GetLineSpan(), data, (key, value) => data);
			}
		}
	}
}
