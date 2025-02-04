﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Durian.Analysis.Cache;
using Durian.Analysis.CodeGeneration;
using Durian.Analysis.Data;
using Durian.Analysis.Filtering;
using Durian.Analysis.Logging;
using Durian.Analysis.SymbolContainers;
using Durian.Analysis.SyntaxVisitors;
using Durian.Info;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Durian.Analysis.CopyFrom;

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
	private const string GROUP_METHODS = "Methods";
	private const string GROUP_TYPES = "Types";
	private const int NUM_STATIC_TREES = 5;

	/// <inheritdoc/>
	public override string GeneratorName => "CopyFrom";

	/// <inheritdoc/>
	public override string GeneratorVersion => "1.0.0";

	/// <inheritdoc/>
	public override int NumStaticTrees => NUM_STATIC_TREES;

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
		return new ISourceTextProvider[NUM_STATIC_TREES - 1]
		{
			new CopyFromAdditionalNodesProvider(),
			new CopyFromTypeAttributeProvider(),
			new CopyFromMethodAttributeProvider(),
			new PatternAttributeProvider(),
		};
	}

	/// <inheritdoc/>
	public override IReadOnlyFilterContainer<IGeneratorSyntaxFilter>? GetFilters(CopyFromPassContext context)
	{
		FilterContainer<IGeneratorSyntaxFilter> list = new();

		list.RegisterGroup(GROUP_METHODS, new Methods.CopyFromMethodFilter());
		list.RegisterGroup(GROUP_TYPES, new Types.CopyFromTypeFilter());

		return list;
	}

	/// <inheritdoc/>
	protected internal override ICompilationData? CreateCompilationData(CSharpCompilation compilation)
	{
		return new CopyFromCompilationData(compilation);
	}

	/// <inheritdoc/>
	protected internal override IDurianSyntaxReceiver CreateSyntaxReceiver()
	{
		return new CopyFromSyntaxReceiver();
	}

	/// <inheritdoc/>
	protected internal override IEnumerable<ISourceTextProvider>? GetInitialSources()
	{
		return GetSourceProviders();
	}

	/// <inheritdoc/>
	protected internal override DurianModule[] GetRequiredModules()
	{
		return new DurianModule[]
		{
			DurianModule.CopyFrom
		};
	}

	/// <inheritdoc/>
	protected internal override void AfterExecutionOfGroup(IReadOnlyFilterGroup<IGeneratorSyntaxFilter> filterGroup, CopyFromPassContext context)
	{
		if (context.DependencyQueue.Count > 0)
		{
			GenerateFromDependencyQueue(filterGroup, context);
		}
		else
		{
			context.SymbolRegistry.Clear();
		}

		base.AfterExecutionOfGroup(filterGroup, context);
	}

	/// <inheritdoc/>
	protected internal override bool Generate(IMemberData data, string hintName, CopyFromPassContext context)
	{
		if (data is not ICopyFromMember target)
		{
			return false;
		}

		if (context.SymbolRegistry.IsRegistered(target.Symbol))
		{
			return true;
		}

		if (!HasAllDependencies(target.Dependencies, context))
		{
			context.DependencyQueue.Enqueue(target.Declaration!, hintName);
			return false;
		}

		if (Generate(target, hintName, context))
		{
			context.SymbolRegistry.Register(target.Symbol);
			return true;
		}

		return false;
	}

	/// <inheritdoc/>
	protected override CopyFromPassContext CreateCurrentPassContext(ICompilationData currentCompilation, GeneratorExecutionContext context)
	{
		return new();
	}

	private static void AddTypeParameterReplacements(
		ImmutableArray<ITypeParameterSymbol> typeParameters,
		ImmutableArray<ITypeSymbol> typeArguments,
		List<(string, string)> list
	)
	{
		for (int i = 0; i < typeParameters.Length; i++)
		{
			ITypeParameterSymbol parameter = typeParameters[i];
			ITypeSymbol argument = typeArguments[i];

			if (parameter.Name != argument.Name && !SymbolEqualityComparer.Default.Equals(parameter, argument))
			{
				list.Add((parameter.Name, argument.GetTypeKeyword() ?? argument.GetGenericName(true)));
			}
		}
	}

	private static string ApplyPattern(ICopyFromMember member, CopyFromPassContext context, string input)
	{
		string current = input;

		for (int i = 0; i < member.Patterns!.Length; i++)
		{
			ref readonly PatternData pattern = ref member.Patterns[i];

			Regex regex = context.RegexProvider.GetRegex(pattern.Pattern);
			current = regex.Replace(current, pattern.Replacement);
		}

		return current;
	}

	private static bool CanCache(Queue<(SyntaxNode, string)> cache)
	{
		return cache.Count < 32;
	}

	private static Queue<(SyntaxReference, string)> GetDependenciesFromQueue(
		IReadOnlyFilterGroup<IGeneratorSyntaxFilter> filterGroup,
		CopyFromPassContext context,
		out Queue<(SyntaxNode, string)> cache
	)
	{
		Queue<(SyntaxReference, string)> dependencies = context.DependencyQueue.ToSystemQueue();
		cache = new(dependencies.Count);

		return filterGroup.Name switch
		{
			GROUP_METHODS => CreateQueue<MethodDeclarationSyntax>(cache),
			GROUP_TYPES => CreateQueue<TypeDeclarationSyntax>(cache),
			_ => dependencies,
		};

		Queue<(SyntaxReference, string)> CreateQueue<T>(Queue<(SyntaxNode, string)> cache) where T : SyntaxNode
		{
			Queue<(SyntaxReference, string)> queue = new(dependencies.Count);

			while (dependencies.Count > 0)
			{
				(SyntaxReference reference, string hintName) result = dependencies.Dequeue();

				if (result.reference.GetSyntax(context.CancellationToken) is T node)
				{
					if (CanCache(cache))
					{
						cache.Enqueue((node, result.hintName));
					}
					else
					{
						queue.Enqueue(result);
					}
				}
				else
				{
					context.DependencyQueue.Enqueue(result.reference, result.hintName);
				}
			}

			return queue;
		}
	}

	private static DocumentationCommentTriviaSyntax? GetDocumentationTrivia(SyntaxNode node)
	{
		return node
			.GetLeadingTrivia()
			.Where(trivia => trivia.HasStructure)
			.Select(trivia => trivia.GetStructure())
			.OfType<DocumentationCommentTriviaSyntax>()
			.FirstOrDefault();
	}

	private static string HandleAttributeText(ICopyFromMember member, SyntaxList<AttributeListSyntax> attributeLists, CopyFromPassContext context)
	{
		StringBuilder attributeText = new();

		if (member.Patterns is null)
		{
			foreach (AttributeListSyntax list in attributeLists)
			{
				for (int i = 0; i < context.CodeBuilder.CurrentIndent; i++)
				{
					attributeText.Append('\t');
				}

				string current = NodeToString(list);
				attributeText.AppendLine(current);
			}
		}
		else
		{
			foreach (AttributeListSyntax list in attributeLists)
			{
				for (int i = 0; i < context.CodeBuilder.CurrentIndent; i++)
				{
					attributeText.Append('\t');
				}

				string current = NodeToString(list);
				current = ApplyPattern(member, context, current);
				attributeText.AppendLine(current);
			}
		}

		return attributeText.ToString();
	}

	private static bool HasAllDependencies(ISymbol[]? symbols, CopyFromPassContext context)
	{
		if (symbols is null)
		{
			return true;
		}

		foreach (ISymbol symbol in symbols)
		{
			if (!context.SymbolRegistry.IsRegistered(symbol))
			{
				return false;
			}
		}

		return true;
	}

	private static void ReplaceTypeParameters(
		ref SyntaxNode currentNode,
		TypeParameterReplacer replacer,
		List<(string identifier, string replacement)> replacements
	)
	{
		foreach ((string identifier, string replacement) in replacements)
		{
			replacer.Identifier = identifier;
			replacer.Replacement = replacement;

			currentNode = replacer.Visit(currentNode);
		}
	}

	private static MemberDeclarationSyntax SkipGeneratorAttributes(MemberDeclarationSyntax declaration, SemanticModel semanticModel, CopyFromPassContext context)
	{
		SyntaxList<AttributeListSyntax> attributeLists = declaration.AttributeLists;

		if (TrySkipGeneratorAttributes(ref attributeLists, semanticModel, context))
		{
			return declaration.WithAttributeLists(attributeLists);
		}

		return declaration;
	}

	private static string TryApplyPattern(ICopyFromMember member, CopyFromPassContext context, string input)
	{
		if (member.Patterns is not null)
		{
			return ApplyPattern(member, context, input);
		}

		return input;
	}

	private static bool TrySkipGeneratorAttributes(ref SyntaxList<AttributeListSyntax> attributeLists, SemanticModel semanticModel, CopyFromPassContext context)
	{
		if (!attributeLists.Any())
		{
			return false;
		}

		List<AttributeListSyntax> newAttributeLists = new(attributeLists.Count);
		List<AttributeSyntax> newAttributes = new();

		bool isChanged = false;

		foreach (AttributeListSyntax attrList in attributeLists)
		{
			SeparatedSyntaxList<AttributeSyntax> attributes = attrList.Attributes;

			if (!attributes.Any())
			{
				continue;
			}

			foreach (AttributeSyntax attribute in attributes)
			{
				if (semanticModel.GetSymbolInfo(attribute, context.CancellationToken).Symbol?.ContainingType is not INamedTypeSymbol attrType)
				{
					continue;
				}

				if (!SymbolEqualityComparer.Default.Equals(attrType, context.TargetCompilation.DurianGeneratedAttribute) &&
					!SymbolEqualityComparer.Default.Equals(attrType, context.TargetCompilation.GeneratedCodeAttribute))
				{
					newAttributes.Add(attribute);
				}
			}

			if (newAttributes.Count > 0)
			{
				if (newAttributes.Count == attributes.Count)
				{
					newAttributeLists.Add(attrList);
				}
				else
				{
					newAttributeLists.Add(attrList.WithAttributes(SyntaxFactory.SeparatedList(newAttributes)));
					isChanged = true;
				}

				newAttributes.Clear();
			}
			else
			{
				isChanged = true;
			}
		}

		if (isChanged)
		{
			attributeLists = SyntaxFactory.List(newAttributeLists);
			return true;
		}

		return false;
	}

	private bool Generate(
		IMemberData data,
		string hintName,
		CopyFromPassContext context,
		Queue<(SyntaxReference, string)> dependencies,
		Queue<(SyntaxNode, string)> cache
	)
	{
		if (data is not ICopyFromMember target)
		{
			return false;
		}

		if (context.SymbolRegistry.IsRegistered(target.Symbol))
		{
			return true;
		}

		if (!HasAllDependencies(target.Dependencies, context))
		{
			if (CanCache(cache))
			{
				cache.Enqueue((target.Declaration!, hintName));
			}
			else
			{
				dependencies.Enqueue((target.Declaration!.GetReference(), hintName));
			}

			return false;
		}

		if (Generate(target, hintName, context))
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

		if (data is Methods.CopyFromMethodData method)
		{
			return GenerateMethod(method, hintName, context);
		}

		return false;
	}

	private bool GenerateFromDependencyQueue(IReadOnlyFilterGroup<IGeneratorSyntaxFilter> filterGroup, CopyFromPassContext context)
	{
		Queue<(SyntaxReference, string)> dependencies = GetDependenciesFromQueue(filterGroup, context, out Queue<(SyntaxNode, string)> cache);

		if (dependencies.Count == 0 && cache.Count == 0)
		{
			return true;
		}

		while (true)
		{
			int current = dependencies.Count + cache.Count;

			HandleFilterGroup(filterGroup, context, dependencies, cache, GetNodesFromQueue());

			int result = dependencies.Count + cache.Count;

			if (result >= current)
			{
				return false;
			}

			if (result == 0)
			{
				return true;
			}
		}

		IEnumerable<(SyntaxNode, string)> GetNodesFromQueue()
		{
			int cacheLength = cache.Count;
			int depLength = dependencies.Count;

			for (int i = 0; i < cacheLength; i++)
			{
				yield return cache.Dequeue();
			}

			for (int i = 0; i < depLength; i++)
			{
				(SyntaxReference reference, string hintName) = dependencies.Dequeue();

				SyntaxNode node = reference.GetSyntax();

				yield return (node, hintName);
			}
		}
	}

	private void GenerateFromFiltersWithGeneratedSymbols(
		IReadOnlyFilterGroup<IGeneratorSyntaxFilter> filterGroup,
		List<IGeneratorSyntaxFilter> filtersWithGeneratedSymbols,
		CopyFromPassContext context,
		Queue<(SyntaxReference, string)> dependencies,
		Queue<(SyntaxNode, string)> cache,
		IEnumerable<(SyntaxNode node, string hintName)> nodes
	)
	{
		BeforeFiltersWithGeneratedSymbols(context);
		BeforeFiltrationOfGeneratedSymbols(filterGroup, context);

		foreach (IGeneratorSyntaxFilter filter in filtersWithGeneratedSymbols)
		{
			IterateThroughFilter(filterGroup, filter, context, dependencies, cache, nodes);
		}
	}

	private void HandleFilterGroup(
		IReadOnlyFilterGroup<IGeneratorSyntaxFilter> filterGroup,
		CopyFromPassContext context,
		Queue<(SyntaxReference, string)> dependencies,
		Queue<(SyntaxNode, string)> cache,
		IEnumerable<(SyntaxNode node, string hintName)> nodes
	)
	{
		int numFilters = filterGroup.Count;
		List<IGeneratorSyntaxFilter> filtersWithGeneratedSymbols = new(numFilters);

		//filterGroup.Unseal();
		BeforeFilteringOfGroup(filterGroup, context);
		//filterGroup.Seal();

		// TODO: Current implementation will enumerate 'nodes' for each filter in the group.
		// That's OK for now, since all filter groups in CopyFrom contain only a single filter,
		// but if there will ever be need to make this code more generic (e.g. by moving it to one of abstract DurianGenerators),
		// the implementation must be re-written.

		foreach (IGeneratorSyntaxFilter filter in filterGroup)
		{
			if (filter.IncludeGeneratedSymbols)
			{
				filtersWithGeneratedSymbols.Add(filter);
			}
			else
			{
				IterateThroughFilter(filterGroup, filter, context, dependencies, cache, nodes);
			}
		}

		BeforeExecutionOfGroup(filterGroup, context);

		GenerateFromFiltersWithGeneratedSymbols(filterGroup, filtersWithGeneratedSymbols, context, dependencies, cache, nodes);

		//filterGroup.Unseal();
		AfterExecutionOfGroup(filterGroup, context);
	}

	private void IterateThroughFilter(
		IReadOnlyFilterGroup<IGeneratorSyntaxFilter> filterGroup,
		IGeneratorSyntaxFilter filter,
		CopyFromPassContext context,
		Queue<(SyntaxReference, string)> dependencies,
		Queue<(SyntaxNode, string)> cache,
		IEnumerable<(SyntaxNode node, string hintName)> nodes
	)
	{
		if (filter is not ISyntaxValidator single)
		{
			throw new InvalidOperationException($"Filter in group '{filterGroup.Name}' does not implement the '{nameof(ISyntaxValidator)}' interface");
		}

		foreach ((SyntaxNode node, string hintName) in nodes)
		{
			PreValidationContext validation = new(node, context.TargetCompilation, context.CancellationToken);

			if (single.ValidateAndCreate(validation, out IMemberData? member) && Generate(member, hintName, context, dependencies, cache))
			{
				context.FileNameProvider.Success();
			}
		}
	}

	private void WriteGeneratedMember(
		ICopyFromMember member,
		SyntaxNode node,
		ISymbol original,
		CopyFromPassContext context,
		GenerateDocumentation applyInheritdoc,
		DocumentationCommentTriviaSyntax? documentation,
		bool includeGenerationAttributes
	)
	{
		string current = NodeToString(node);

		if (documentation is null)
		{
			WriteGeneratedMemberWithoutDocumentation(member, original, context, applyInheritdoc, includeGenerationAttributes, current);
			return;
		}

		string doc = documentation.ToFullString();

		for (int i = 0; i < member.Patterns!.Length; i++)
		{
			ref readonly PatternData pattern = ref member.Patterns[i];

			Regex regex = context.RegexProvider.GetRegex(pattern.Pattern);
			current = regex.Replace(current, pattern.Replacement);
			doc = regex.Replace(doc, pattern.Replacement);
		}

		context.CodeBuilder.Indent();
		context.CodeBuilder.Write(doc);

		if(includeGenerationAttributes)
		{
			string generatedFrom = AutoGenerated.GetDurianGeneratedAttribute(SymbolToString(original));
			WriteGeneratedMember_Internal(current, generatedFrom, context);
		}
		else
		{
			context.CodeBuilder.Indent();
			context.CodeBuilder.WriteLine(current);
			context.CodeBuilder.NewLine();
		}
	}

	private void WriteGeneratedMemberWithoutDocumentation(
		ICopyFromMember member,
		ISymbol original,
		CopyFromPassContext context,
		GenerateDocumentation applyInheritdoc,
		bool includeGenerationAttributes,
		string current
	)
	{
		current = ApplyPattern(member, context, current);

		if (includeGenerationAttributes)
		{
			string generatedFrom = AutoGenerated.GetDurianGeneratedAttribute(SymbolToString(original));

			if (applyInheritdoc == GenerateDocumentation.Always)
			{
				WriteGeneratedMember_Internal(current, generatedFrom, AutoGenerated.GetInheritdoc(original.GetFullyQualifiedName(QualifiedName.Xml)), context);
			}
			else
			{
				WriteGeneratedMember_Internal(current, generatedFrom, context);
			}
		}
		else
		{
			if (applyInheritdoc == GenerateDocumentation.Always)
			{
				context.CodeBuilder.Indent();
				context.CodeBuilder.WriteLine(AutoGenerated.GetInheritdoc(original.GetFullyQualifiedName(QualifiedName.Xml)));
			}

			context.CodeBuilder.NewLine();
			context.CodeBuilder.Indent();
			context.CodeBuilder.WriteLine(current);
			context.CodeBuilder.NewLine();
		}
	}

	private void WriteGeneratedMember(
		ICopyFromMember member,
		SyntaxNode node,
		ISymbol original,
		CopyFromPassContext context,
		GenerateDocumentation applyInheritdoc,
		bool includeGenerationAttributes
	)
	{
		if (member.Patterns is null || member.Patterns.Length == 0)
		{
			WriteGeneratedMember(node, original, context, includeGenerationAttributes, applyInheritdoc);
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
			documentation,
			includeGenerationAttributes
		);
	}
}
