// Copyright (c) Piotr Stenke. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Durian.Analysis.Cache;
using Durian.Analysis.Data;
using Durian.Analysis.Extensions;
using Durian.Analysis.Filters;
using Durian.Analysis.Logging;
using Durian.Info;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.Analysis.CopyFrom
{
	/// <summary>
	/// Main class of the <c>CopyFrom</c> module. Generates source code of members marked with the <c>Durian.CopyFromAttribute</c>.
	/// </summary>
	[Generator(LanguageNames.CSharp)]
	[LoggingConfiguration(
		SupportedLogs = GeneratorLogs.All,
		LogDirectory = "CopyFrom",
		SupportsDiagnostics = true,
		RelativeToGlobal = true,
		EnableExceptions = true,
		DefaultNodeOutput = NodeOutput.SyntaxTree)]
	public sealed partial class CopyFromGenerator : CachedGenerator<ICopyFromMember, CopyFromPassContext>
	{
		private const int _numStaticTrees = 4;

		/// <inheritdoc/>
		public override string GeneratorName => "CopyFrom";

		/// <inheritdoc/>
		public override string GeneratorVersion => "1.0.0";

		/// <inheritdoc/>
		public override int NumStaticTrees => _numStaticTrees;

		/// <summary>
		/// Initializes a new instance of the <see cref="CopyFromGenerator"/> class.
		/// </summary>
		public CopyFromGenerator()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="CopyFromGenerator"/> class.
		/// </summary>
		/// <param name="context">Configures how this <see cref="CopyFromGenerator"/> is initialized.</param>
		public CopyFromGenerator(in GeneratorLogCreationContext context) : base(in context)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="CopyFromGenerator"/> class.
		/// </summary>
		/// <param name="loggingConfiguration">Determines how the source generator should behave when logging information.</param>
		public CopyFromGenerator(LoggingConfiguration? loggingConfiguration) : base(loggingConfiguration)
		{
		}

		/// <summary>
		/// Returns a collection of <see cref="ISourceTextProvider"/> used by this generator to create initial sources.
		/// </summary>
		public static IEnumerable<ISourceTextProvider> GetSourceProviders()
		{
			return new ISourceTextProvider[_numStaticTrees - 1]
			{
				new CopyFromTypeAttributeProvider(),
				new CopyFromMethodAttributeProvider(),
				new PatternAttributeProvider(),
			};
		}

		/// <inheritdoc/>
		public override ICompilationData? CreateCompilationData(CSharpCompilation compilation)
		{
			return new CopyFromCompilationData(compilation);
		}

		/// <inheritdoc/>
		public override IDurianSyntaxReceiver CreateSyntaxReceiver()
		{
			return new CopyFromSyntaxReceiver();
		}

		/// <inheritdoc/>
		public override IReadOnlyFilterContainer<IGeneratorSyntaxFilter>? GetFilters(CopyFromPassContext context)
		{
			FilterContainer<IGeneratorSyntaxFilter> list = new();

			list.RegisterGroup("Methods", new Methods.CopyFromMethodFilter());
			list.RegisterGroup("Types", new Types.CopyFromTypeFilter());

			return list;
		}

		/// <inheritdoc/>
		public override IEnumerable<ISourceTextProvider>? GetInitialSources()
		{
			return GetSourceProviders();
		}

		/// <inheritdoc/>
		public override DurianModule[] GetRequiredModules()
		{
			return new DurianModule[]
			{
				DurianModule.CopyFrom
			};
		}

		/// <inheritdoc/>
		protected internal override void AfterExecution(CopyFromPassContext context)
		{
			if(context.DependencyQueue.Count > 0)
			{
				GenerateFromDependencyQueue(context);
			}

			context.SymbolRegistry.Clear();

			base.AfterExecution(context);
		}

		/// <inheritdoc/>
		protected override CopyFromPassContext CreateCurrentPassContext(ICompilationData currentCompilation, in GeneratorExecutionContext context)
		{
			return new();
		}

		/// <inheritdoc/>
		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
		}

		/// <inheritdoc/>
		protected internal override bool Generate(IMemberData data, string hintName, CopyFromPassContext context)
		{
			if (data is not ICopyFromMember target)
			{
				return false;
			}

			if (!HasAllDependencies(target.Dependencies, context))
			{
				context.DependencyQueue.Enqueue(target, hintName);
				return false;
			}

			if(Generate(target, hintName, context))
			{
				context.SymbolRegistry.Register(target.Symbol);
				return true;
			}

			return false;
		}

		private bool Generate(ICopyFromMember data, string hintName, CopyFromPassContext context)
		{
			if (data is Types.CopyFromTypeData type)
			{
				return GenerateType(type, hintName, context);
			}

			return false;
		}

		private bool GenerateFromDependencyQueue(CopyFromPassContext context)
		{
			Queue<(ICopyFromMember, string)> localQueue = new(context.DependencyQueue.Count);

			while(true)
			{
				int current = context.DependencyQueue.Count;

				while (context.DependencyQueue.Count > 0)
				{
					if(!context.DependencyQueue.Dequeue(out ICopyFromMember? member, out string? hintName))
					{
						continue;
					}

					if (HasAllDependencies(member.Dependencies, context))
					{
						Generate(member, hintName, context);
					}
					else
					{
						localQueue.Enqueue((member, hintName));
					}
				}

				if(localQueue.Count == 0)
				{
					return true;
				}

				if(localQueue.Count == current)
				{
					return false;
				}

				while(localQueue.Count > 0)
				{
					(ICopyFromMember member, string hintName) = localQueue.Dequeue();
					context.DependencyQueue.Enqueue(member, hintName);
				}
			}
		}

		private static DocumentationCommentTriviaSyntax? GetDocumentationTrivia(CSharpSyntaxNode node)
		{
			return node
				.GetLeadingTrivia()
				.Where(trivia => trivia.HasStructure)
				.Select(trivia => trivia.GetStructure())
				.OfType<DocumentationCommentTriviaSyntax>()
				.FirstOrDefault();
		}

		private static bool HasAllDependencies(ISymbol[]? symbols, CopyFromPassContext context)
		{
			if (symbols is null)
			{
				return true;
			}

			foreach (ISymbol symbol in symbols)
			{
				if(!context.SymbolRegistry.IsRegistered(symbol))
				{
					return false;
				}
			}

			return true;
		}

		private void WriteGeneratedMember(
			ICopyFromMember member,
			CSharpSyntaxNode node,
			ISymbol original,
			CopyFromPassContext context,
			GenerateDocumentation applyInheritdoc,
			DocumentationCommentTriviaSyntax? documentation
		)
		{
			string current = NodeToString(node);
			string generatedFrom = AutoGenerated.GetDurianGeneratedAttribute(SymbolToString(original));

			if(documentation is null)
			{
				for (int i = 0; i < member.Patterns!.Length; i++)
				{
					ref readonly PatternData pattern = ref member.Patterns[i];

					Regex regex = new(pattern.Pattern, RegexOptions.CultureInvariant | RegexOptions.Singleline);
					current = regex.Replace(current, pattern.Replacement);
				}

				if (applyInheritdoc == GenerateDocumentation.Always)
				{
					WriteGeneratedMember_Internal(current, generatedFrom, AutoGenerated.GetInheritdoc(original.GetXmlFullyQualifiedName()), context);
				}
				else
				{
					WriteGeneratedMember_Internal(current, generatedFrom, context);
				}

				return;
			}

			string doc = documentation.ToFullString();

			for (int i = 0; i < member.Patterns!.Length; i++)
			{
				ref readonly PatternData pattern = ref member.Patterns[i];

				Regex regex = new(pattern.Pattern, RegexOptions.CultureInvariant | RegexOptions.Singleline);
				current = regex.Replace(current, pattern.Replacement);
				doc = regex.Replace(doc, pattern.Replacement);
			}

			context.CodeBuilder.Indent();
			context.CodeBuilder.Write(doc);
			WriteGeneratedMember_Internal(current, generatedFrom, context);
		}

		private void WriteGeneratedMember(
			ICopyFromMember member,
			CSharpSyntaxNode node,
			ISymbol original,
			CopyFromPassContext context,
			GenerateDocumentation applyInheritdoc
		)
		{
			if (member.Patterns is null || member.Patterns.Length == 0)
			{
				WriteGeneratedMember(node, original, context, applyInheritdoc);
				return;
			}

			DocumentationCommentTriviaSyntax? documentation = default;

			if (applyInheritdoc != GenerateDocumentation.Never)
			{
				documentation = GetDocumentationTrivia(node);
			}

			WriteGeneratedMember(
				member,
				node,
				original,
				context,
				applyInheritdoc,
				documentation
			);
		}
	}
}
